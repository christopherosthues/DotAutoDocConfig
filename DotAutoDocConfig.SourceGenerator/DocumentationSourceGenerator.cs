using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using DotAutoDocConfig.SourceGenerator.TableGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotAutoDocConfig.SourceGenerator;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class DocumentationSourceGenerator : IIncrementalGenerator
{
    private const string DocumentationAttributeFullName = "DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes annotated with the [Documentation] attribute. Only filtered Syntax Nodes can trigger code generation.
        IncrementalValuesProvider<ClassDeclarationSyntax> provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DocumentationAttributeFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode);

        // Try to read the project directory and project name from analyzer config global options (MSBuildProjectDirectory/MSBuildProjectName).
        IncrementalValueProvider<BuildProperties> buildProps = context.BuildPropsProvider();

        // Compilation, gefundene Klassen und Build-Props kombinieren
        IncrementalValueProvider<((Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) Left, BuildProperties Right)> combined = context.CompilationProvider
            .Combine(provider.Collect())
            .Combine(buildProps);

        context.RegisterSourceOutput(
            combined,
            static (spc, data) =>
            {
                Compilation compilation = data.Left.Left;
                ImmutableArray<ClassDeclarationSyntax> classes = data.Left.Right;
                BuildProperties buildProperties = data.Right;
                GenerateCode(spc, compilation, classes, buildProperties.ProjectDirectory);
            });
    }

    /// <summary>
    /// Generate code action.
    /// It will be executed on specific nodes (ClassDeclarationSyntax annotated with the [Report] attribute) changed by the user.
    /// </summary>
    /// <param name="context">Source generation context used to add source files.</param>
    /// <param name="compilation">Compilation used to provide access to the Semantic Model.</param>
    /// <param name="classDeclarations">Nodes annotated with the [Report] attribute that trigger the generate action.</param>
    /// <param name="projectDirectory">Directory of the project (.csproj) used to resolve relative output paths.</param>
    private static void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations, string projectDirectory)
    {
        IList<string> filePaths = [];

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

            // Get per-attribute documentation options
            (INamedTypeSymbol? symbol, List<DocumentationOptionsDataModel> docs) = GetDocumentationDataModels(compilation, classDeclarationSyntax);
            if (symbol is null)
            {
                continue;
            }

            foreach (DocumentationOptionsDataModel docOptions in docs)
            {
                // Log selected complex parameter format to surface behavior in build output
#pragma warning disable CS8620
                context.LogInfo("ComplexParameterFormat: {0}", docOptions.ComplexParameterFormat);
#pragma warning restore CS8620

                ComplexParameterFormat complexFmt = docOptions.ComplexParameterFormat;
                ITableGenerator tableGenerator = TableGeneratorFactory.CreateGenerator(complexFmt);

                tableGenerator.GenerateTable(docOptions, context, classSymbol, projectDirectory, repoRoot, filePaths);
            }
        }
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
            LocalFormat formatValue = LocalFormat.AsciiDoc;
            object? raw = attributeData.ConstructorArguments[0].Value;
            if (raw is int intVal)
            {
                formatValue = (LocalFormat)intVal;
            }
            else if (raw is byte bVal)
            {
                formatValue = (LocalFormat)bVal;
            }

            string outputPath = attributeData.ConstructorArguments[1].Value!.ToString() ?? string.Empty;

            // Optional third argument: ComplexParameterFormat
            ComplexParameterFormat complexFormat = ComplexParameterFormat.InlineJsonShort;
            if (attributeData.ConstructorArguments.Length >= 3)
            {
                object? rawComplex = attributeData.ConstructorArguments[2].Value;
                if (rawComplex is int ci)
                {
                    complexFormat = (ComplexParameterFormat)ci;
                }
                else if (rawComplex is byte cb)
                {
                    complexFormat = (ComplexParameterFormat)cb;
                }
            }

            // Optional fourth argument: includeNamespaces (bool)
            bool includeNamespaces = false;
            if (attributeData.ConstructorArguments is [_, _, _, { Value: bool b } _, ..])
            {
                includeNamespaces = b;
            }

            documentationDataModels.Add(new DocumentationOptionsDataModel
            {
                Format = formatValue,
                OutputDirectory = outputPath,
                ComplexParameterFormat = complexFormat,
                IncludeNamespaces = includeNamespaces
            });
        }
        return (classSymbol, documentationDataModels);
    }
}
