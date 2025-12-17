using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using DotAutoDocConfig.SourceGenerator.DocumentationGenerators;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotAutoDocConfig.SourceGenerator;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class DocumentationSourceGenerator : IIncrementalGenerator
{
    private const string DocumentationAttributeFullName = "DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute";

    // Track used root output files to avoid collisions when includeNamespaces=false
    private static readonly HashSet<string> UsedRootOutputFiles = new(StringComparer.OrdinalIgnoreCase);

    private static void LogInfo(SourceProductionContext context, string message, params object[] args)
    {
        // Emit an informational diagnostic that shows up in the build output
        DiagnosticDescriptor descriptor = new(
            id: "DDG000",
            title: "DocumentationGeneratorInfo",
            messageFormat: message,
            category: "DocumentationGenerator",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);
        Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None, args);
        context.ReportDiagnostic(diagnostic);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes annotated with the [Documentation] attribute. Only filtered Syntax Nodes can trigger code generation.
        IncrementalValuesProvider<ClassDeclarationSyntax> provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DocumentationAttributeFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode);

        // Try to read the project directory and project name from analyzer config global options (MSBuildProjectDirectory/MSBuildProjectName).
        IncrementalValueProvider<(string projectDirectory, string projectName)> buildProps = context.ProjectDirectoryAndNameProvider();

        // Compilation, gefundene Klassen und Build-Props kombinieren
        IncrementalValueProvider<((Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) Left, (string projectDirectory, string projectName) Right)> combined = context.CompilationProvider
            .Combine(provider.Collect())
            .Combine(buildProps);

        context.RegisterSourceOutput(
            combined,
            static (spc, data) =>
            {
                Compilation compilation = data.Left.Left;
                ImmutableArray<ClassDeclarationSyntax> classes = data.Left.Right;
                (string projectDirectory, string projectName) = data.Right;
                GenerateCode(spc, compilation, classes, projectDirectory, projectName);
            });
    }

    private static (INamedTypeSymbol?, List<DocumentationOptionsDataModel>) GetDocumentationDataModels(
        Compilation compilation,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        List<DocumentationOptionsDataModel> documentationDataModels = [];

        // We need to get semantic model of the class to retrieve
        SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
        {
            return (null, documentationDataModels);
        }

        // Go through all attributes of the class.
        foreach (AttributeData attributeData in classSymbol.GetAttributes())
        {
            string attributeName = attributeData.AttributeClass?.ToDisplayString() ?? string.Empty;
            // Check the full name of the [Documentation] attribute.
            if (attributeName != DocumentationAttributeFullName)
            {
                continue;
            }

            // Retrieve constructor arguments.
            if (attributeData.ConstructorArguments.Length < 2)
            {
                continue;
            }

            // Read numeric value of the enum (as byte/int) to avoid referencing the external enum type.
            byte formatValue = 0;
            object? raw = attributeData.ConstructorArguments[0].Value;
            if (raw is int intVal)
            {
                formatValue = (byte)intVal;
            }
            else if (raw is byte bVal)
            {
                formatValue = bVal;
            }

            string outputPath = attributeData.ConstructorArguments[1].Value!.ToString() ?? string.Empty;

            // Optional third argument: ComplexParameterFormat
            byte complexFormat = 0; // default InlineJsonShort
            if (attributeData.ConstructorArguments.Length >= 3)
            {
                object? rawComplex = attributeData.ConstructorArguments[2].Value;
                if (rawComplex is int ci)
                {
                    complexFormat = (byte)ci;
                }
                else if (rawComplex is byte cb)
                {
                    complexFormat = cb;
                }
            }

            // Optional fourth argument: includeNamespaces (bool)
            bool includeNamespaces = false;
            if (attributeData.ConstructorArguments.Length >= 4 && attributeData.ConstructorArguments[3].Value is bool b)
            {
                includeNamespaces = b;
            }

            documentationDataModels.Add(new DocumentationOptionsDataModel
            {
                Format = formatValue,
                OutputPath = outputPath,
                ComplexParameterFormat = complexFormat,
                IncludeNamespaces = includeNamespaces
            });
        }
        return (classSymbol, documentationDataModels);
    }

    /// <summary>
    /// Generate code action.
    /// It will be executed on specific nodes (ClassDeclarationSyntax annotated with the [Report] attribute) changed by the user.
    /// </summary>
    /// <param name="context">Source generation context used to add source files.</param>
    /// <param name="compilation">Compilation used to provide access to the Semantic Model.</param>
    /// <param name="classDeclarations">Nodes annotated with the [Report] attribute that trigger the generate action.</param>
    /// <param name="projectDirectory">Directory of the project (.csproj) used to resolve relative output paths.</param>
    /// <param name="projectName">MSBuild project name; currently used for diagnostics only.</param>
    private static void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations, string projectDirectory, string projectName)
    {
        // Go through all filtered class declarations.
        foreach (ClassDeclarationSyntax classDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            {
                continue;
            }

            // determine repository root: prefer projectDirectory from analyzer config; otherwise fallback to the directory of the source file.
            string repoRoot = projectDirectory;
            if (string.IsNullOrEmpty(repoRoot))
            {
                string filePath = classDeclarationSyntax.SyntaxTree.FilePath;
                repoRoot = Path.GetDirectoryName(filePath) ?? string.Empty;
            }

            LogInfo(context, "RepoRoot resolved: {0}; ProjectName: {1}", repoRoot, string.IsNullOrEmpty(projectName) ? "(empty)" : projectName);

            // Get per-attribute documentation options
            (INamedTypeSymbol? symbol, List<DocumentationOptionsDataModel> docs) = GetDocumentationDataModels(compilation, classDeclarationSyntax);
            if (symbol is null)
            {
                continue;
            }

            foreach (DocumentationOptionsDataModel docOptions in docs)
            {
                // Log selected complex parameter format to surface behavior in build output
                LogInfo(context, "ComplexParameterFormat: {0}", docOptions.ComplexParameterFormat);

                StringBuilder sb = new();
                LocalFormat fmt = (LocalFormat)docOptions.Format;
                ComplexParameterFormat complexFmt = (ComplexParameterFormat)docOptions.ComplexParameterFormat;
                IDocumentationGenerator documentationGenerator = DocumentationGeneratorFactory.CreateGenerator(fmt);

                if (complexFmt == ComplexParameterFormat.SeparateTables)
                {
                    DocumentationTablesModel tables = SeparateTableCollector.CollectDocumentationEntries(classSymbol);

                    // Prepare file names for each type table
                    Dictionary<INamedTypeSymbol, string> typeToFileName = new(SymbolEqualityComparer.Default);
                    HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
                    {
                        string ext = fmt == LocalFormat.AsciiDoc ? ".adoc" : ".md";
                        string baseName = docOptions.IncludeNamespaces
                            ? CreateFileBaseNameWithNamespace(kvp.Key)
                            : kvp.Key.Name;
                        string fileName = EnsureUniqueFileName(baseName, ext, usedNames);
                        typeToFileName[kvp.Key] = fileName;
                    }

                    // Render root file with links to separate type files
                    documentationGenerator.GenerateWithFileLinks(sb, classSymbol, tables, typeToFileName, docOptions.IncludeNamespaces);

                    // Resolve root output path (directory or file supported)
                    string rootFullPath = ComposeRootOutputPath(
                        docOptions.OutputPath,
                        projectDirectory,
                        repoRoot,
                        fmt,
                        classSymbol,
                        docOptions.IncludeNamespaces);

                    // Write root file
                    WriteResolvedFile(context, rootFullPath, sb.ToString());

                    // Write each type table to its own file next to the root output (same directory)
                    foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
                    {
                        string typeFileName = typeToFileName[kvp.Key];
                        string rootPath = rootFullPath; // already resolved file path
                        string? rootDir = Path.GetDirectoryName(rootPath);
                        string typePath = Path.Combine(rootDir ?? string.Empty, typeFileName);

                        StringBuilder subSb = new();
                        documentationGenerator.GenerateTypeTable(subSb, kvp.Key, kvp.Value, docOptions.IncludeNamespaces);

                        // Write sub file
                        WriteResolvedFile(context, typePath, subSb.ToString());
                    }

                    // Move on to next docOptions
                }
                else
                {
                    // InlineJsonShort: single file generation
                    List<DocumentationDataModel> entries = InlineTableCollector.CollectDocumentationEntries(classSymbol);

                    documentationGenerator.Generate(sb, classSymbol, entries, docOptions.IncludeNamespaces);

                    // Resolve root output path and write
                    string rootFullPath = ComposeRootOutputPath(
                        docOptions.OutputPath,
                        projectDirectory,
                        repoRoot,
                        fmt,
                        classSymbol,
                        docOptions.IncludeNamespaces);

                    WriteResolvedFile(context, rootFullPath, sb.ToString());
                }
            }
        }
    }

    private static string CreateFileBaseNameWithNamespace(INamedTypeSymbol symbol)
    {
        string fq = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat); // e.g., global::Namespace.Type
        if (fq.StartsWith("global::"))
        {
            fq = fq.Substring("global::".Length);
        }
        StringBuilder sbName = new();
        foreach (char c in fq)
        {
            if (char.IsLetterOrDigit(c))
            {
                sbName.Append(c);
            }
            else if (c == '.' || c == '_' || c == '+')
            {
                sbName.Append('-');
            }
        }
        return sbName.ToString();
    }

    private static string EnsureUniqueFileName(string baseName, string ext, HashSet<string> usedNames)
    {
        string candidate = baseName + ext;
        int i = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = baseName + "-" + i + ext;
            i++;
        }
        return candidate;
    }

    private static void WriteResolvedFile(SourceProductionContext context, string fullPath, string content)
    {
        try
        {
            string? dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(fullPath, content, Encoding.UTF8);
            LogInfo(context, "Writing documentation (resolved): {0}", fullPath);
        }
        catch (Exception ex)
        {
            DiagnosticDescriptor desc = new("DDG001", "DocumentationGeneratorWriteFailed", "Failed to write documentation file '{0}': {1}", "DocumentationGenerator", DiagnosticSeverity.Warning, true);
            Diagnostic diagnostic = Diagnostic.Create(desc, Location.None, fullPath, ex.Message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string ComposeRootOutputPath(
        string requestedPath,
        string projectDirectory,
        string repoRoot,
        LocalFormat fmt,
        INamedTypeSymbol classSymbol,
        bool includeNamespaces)
    {
        string ext = fmt == LocalFormat.AsciiDoc ? ".adoc" : ".md";

        // Resolve base path first (absolute or under project root)
        string baseProjectRoot = !string.IsNullOrEmpty(projectDirectory) ? projectDirectory : repoRoot;
        string resolved = Path.IsPathRooted(requestedPath)
            ? requestedPath
            : Path.GetFullPath(Path.Combine(baseProjectRoot, requestedPath));

        // Determine if the user provided a directory or a file
        bool looksLikeDirectory = resolved.EndsWith(Path.DirectorySeparatorChar.ToString())
                                  || resolved.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                                  || !Path.HasExtension(resolved)
                                  || Directory.Exists(resolved);

        if (!looksLikeDirectory)
        {
            // It's a file path, return as-is
            // Ensure directory exists will be handled by WriteResolvedFile
            UsedRootOutputFiles.Add(resolved);
            return resolved;
        }

        // Ensure directory exists (create later on write)
        string directory = resolved.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string baseName = includeNamespaces ? CreateFileBaseNameWithNamespace(classSymbol) : classSymbol.Name;

        // Ensure uniqueness to avoid collisions when includeNamespaces=false
        string candidate = Path.Combine(directory, baseName + ext);
        int i = 2;
        while (UsedRootOutputFiles.Contains(candidate))
        {
            candidate = Path.Combine(directory, baseName + "-" + i + ext);
            i++;
        }
        UsedRootOutputFiles.Add(candidate);
        return candidate;
    }
}
