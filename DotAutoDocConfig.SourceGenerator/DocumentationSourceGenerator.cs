using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.IO;
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
    private enum LocalFormat : byte
    {
        AsciiDoc = 0,
        Markdown = 1,
        Html = 2
    }

    protected enum ComplexParameterFormat : byte
    {
        InlineJsonShort = 0,
        SeparateTables = 1
    }

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
                "DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute",
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode);

        // Try to read the project directory and project name from analyzer config global options (MSBuildProjectDirectory/MSBuildProjectName).
        IncrementalValueProvider<(string projectDirectory, string projectName)> buildProps = context.AnalyzerConfigOptionsProvider
            .Select(static (optsProvider, _) =>
            {
                AnalyzerConfigOptions opts = optsProvider.GlobalOptions;
                if (!opts.TryGetValue("build_property.MSBuildProjectDirectory", out string? projectDirectory))
                {
                    opts.TryGetValue("build_property.ProjectDir", out projectDirectory);
                }

                opts.TryGetValue("build_property.MSBuildProjectName", out string? projectName);
                return (projectDirectory: projectDirectory ?? string.Empty, projectName: projectName ?? string.Empty);
            });

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
                new DocumentationSourceGenerator().GenerateCode(spc, compilation, classes, projectDirectory, projectName);
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
            if (attributeName != "DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute")
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

            documentationDataModels.Add(new DocumentationOptionsDataModel
            {
                Format = formatValue,
                OutputPath = outputPath,
                ComplexParameterFormat = complexFormat
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
    private void GenerateCode(SourceProductionContext context, Compilation compilation,
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

                if (complexFmt == ComplexParameterFormat.SeparateTables) // SeparateTables (0=InlineJsonShort, 1=SeparateTables)
                {
                    DocumentationTablesModel tables = SeparateTableCollector.CollectDocumentationEntries(classSymbol);

                    // Prepare file names for each type table
                    Dictionary<INamedTypeSymbol, string> typeToFileName = new(SymbolEqualityComparer.Default);
                    foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
                    {
                        string ext = fmt == LocalFormat.AsciiDoc ? ".adoc" : ".md";
                        string fileName = CreateSafeFileName(kvp.Key, ext);
                        typeToFileName[kvp.Key] = fileName;
                    }

                    // Render root file with links to separate type files
                    switch (fmt)
                    {
                        case LocalFormat.AsciiDoc:
                            DocumentationGenerators.AsciiDocGenerator.GenerateAsciiDocRootWithFileLinks(sb, classSymbol, tables, typeToFileName);
                            break;
                        case LocalFormat.Markdown:
                            DocumentationGenerators.MarkdownGenerator.GenerateMarkdownRootWithFileLinks(sb, classSymbol, tables, typeToFileName);
                            break;
                        default:
                            DocumentationGenerators.MarkdownGenerator.GenerateMarkdownRootWithFileLinks(sb, classSymbol, tables, typeToFileName);
                            break;
                    }

                    // Write root file
                    WriteFile(context, sb.ToString(), docOptions.OutputPath, projectDirectory, repoRoot);

                    // Write each type table to its own file next to the root output (same directory)
                    foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
                    {
                        string typeFileName = typeToFileName[kvp.Key];
                        string rootPath = ResolveOutputPath(docOptions.OutputPath, projectDirectory, repoRoot);
                        string? rootDir = Path.GetDirectoryName(rootPath);
                        string typePath = Path.Combine(rootDir ?? string.Empty, typeFileName);

                        StringBuilder subSb = new();
                        switch (fmt)
                        {
                            case LocalFormat.AsciiDoc:
                                DocumentationGenerators.AsciiDocGenerator.GenerateAsciiDocTypeTable(subSb, kvp.Key, kvp.Value);
                                break;
                            case LocalFormat.Markdown:
                                DocumentationGenerators.MarkdownGenerator.GenerateMarkdownTypeTable(subSb, kvp.Key, kvp.Value);
                                break;
                            default:
                                DocumentationGenerators.MarkdownGenerator.GenerateMarkdownTypeTable(subSb, kvp.Key, kvp.Value);
                                break;
                        }

                        // Write sub file
                        WriteResolvedFile(context, typePath, subSb.ToString());
                    }

                    // Move on to next docOptions
                    continue;
                }
                else
                {
                    // InlineJsonShort: single file generation
                    List<DocumentationDataModel> entries = InlineTableCollector.CollectDocumentationEntries(classSymbol);

                    switch (fmt)
                    {
                        case LocalFormat.AsciiDoc:
                            DocumentationGenerators.AsciiDocGenerator.GenerateAsciiDoc(sb, classSymbol, entries);
                            break;
                        case LocalFormat.Markdown:
                            DocumentationGenerators.MarkdownGenerator.GenerateMarkdown(sb, classSymbol, entries);
                            break;
                        default:
                            DocumentationGenerators.MarkdownGenerator.GenerateMarkdown(sb, classSymbol, entries);
                            break;
                    }

                    // Write root file
                    WriteFile(context, sb.ToString(), docOptions.OutputPath, projectDirectory, repoRoot);
                }
            }
        }
    }

    private static string ResolveOutputPath(string requestedPath, string projectDirectory, string repoRoot)
    {
        if (Path.IsPathRooted(requestedPath))
        {
            return requestedPath;
        }
        string baseProjectRoot = !string.IsNullOrEmpty(projectDirectory) ? projectDirectory : repoRoot;
        return Path.GetFullPath(Path.Combine(baseProjectRoot, requestedPath));
    }

    private static string CreateSafeFileName(INamedTypeSymbol symbol, string ext)
    {
        // Use fully qualified name to distinguish same-named types from different namespaces
        string fq = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat); // e.g., global::Namespace.Type
        // Remove leading 'global::'
        if (fq.StartsWith("global::"))
        {
            fq = fq.Substring("global::".Length);
        }
        // Replace dots and plus (nested types) with dashes, keep alnum and dashes/underscores
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
        return sbName.ToString() + ext;
    }

    private static void WriteResolvedFile(SourceProductionContext context, string fullPath, string content)
    {
#pragma warning disable RS1035
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
        catch (System.Exception ex)
        {
            DiagnosticDescriptor desc = new("DDG001", "DocumentationGeneratorWriteFailed", "Failed to write documentation file '{0}': {1}", "DocumentationGenerator", DiagnosticSeverity.Warning, true);
            Diagnostic diagnostic = Diagnostic.Create(desc, Location.None, fullPath, ex.Message);
            context.ReportDiagnostic(diagnostic);
        }
#pragma warning restore RS1035
    }

    private static void WriteFile(SourceProductionContext context, string content, string requestedPath, string projectDirectory, string repoRoot)
    {
        LogInfo(context, "RequestedPath: {0}", requestedPath);
#pragma warning disable RS1035
        try
        {
            if (Path.IsPathRooted(requestedPath))
            {
                string fullPath = requestedPath;
                string? dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                LogInfo(context, "Writing documentation (absolute): {0}", fullPath);
                File.WriteAllText(fullPath, content, Encoding.UTF8);
            }
            else
            {
                string baseProjectRoot = !string.IsNullOrEmpty(projectDirectory) ? projectDirectory : repoRoot;
                string fullProjectPath = Path.GetFullPath(Path.Combine(baseProjectRoot, requestedPath));

                string? dir = Path.GetDirectoryName(fullProjectPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                LogInfo(context, "Writing documentation (projectroot): Base={0}; FullPath={1}", baseProjectRoot, fullProjectPath);
                File.WriteAllText(fullProjectPath, content, Encoding.UTF8);
            }
        }
        catch (System.Exception ex)
        {
            DiagnosticDescriptor desc = new("DDG001", "DocumentationGeneratorWriteFailed", "Failed to write documentation file '{0}': {1}", "DocumentationGenerator", DiagnosticSeverity.Warning, true);
            Diagnostic diagnostic = Diagnostic.Create(desc, Location.None, requestedPath, ex.Message);
            context.ReportDiagnostic(diagnostic);
        }
#pragma warning restore RS1035
    }
}
