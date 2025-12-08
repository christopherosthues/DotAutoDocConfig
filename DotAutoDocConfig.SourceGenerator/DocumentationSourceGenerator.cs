using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DotAutoDocConfig.Core.ComponentModel;
using DotAutoDocConfig.Core.ComponentModel.Attributes;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace DotAutoDocConfig.SourceGenerator;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class DocumentationSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes annotated with the [Report] attribute. Only filtered Syntax Nodes can trigger code generation.
        IncrementalValuesProvider<ClassDeclarationSyntax> provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{typeof(DocumentationAttribute).Namespace}.{nameof(DocumentationAttribute)}",
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
    }

    private static (INamedTypeSymbol?, List<DocumentationOptionsDataModel>) GetDocumentationDataModels(
        Compilation compilation,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        List<DocumentationOptionsDataModel> documentationDataModels = [];

        // We need to get semantic model of the class to retrieve
        SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            return (null, documentationDataModels);
        // Go through all attributes of the class.
        foreach (AttributeData attributeData in classSymbol.GetAttributes())
        {
            string attributeName = attributeData.AttributeClass?.ToDisplayString() ?? string.Empty;
            // Check the full name of the [Documentation] attribute.
            if (attributeName != $"{typeof(DocumentationAttribute).Namespace}.{nameof(DocumentationAttribute)}")
                continue;
            // Retrieve constructor arguments.
            if (attributeData.ConstructorArguments.Length != 2)
                continue;
            DocumentationFormat format = (DocumentationFormat)attributeData.ConstructorArguments[0].Value!;
            string outputPath = attributeData.ConstructorArguments[1].Value!.ToString() ?? string.Empty;
            documentationDataModels.Add(new()
            {
                Format = format,
                OutputPath = outputPath
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
    private void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations)
    {
        // Go through all filtered class declarations.
        foreach (ClassDeclarationSyntax? classDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            // Get per-attribute documentation options
            (INamedTypeSymbol? symbol, List<DocumentationOptionsDataModel> docs) = GetDocumentationDataModels(compilation, classDeclarationSyntax);
            if (symbol is null)
                continue;

            foreach (DocumentationOptionsDataModel? docOptions in docs)
            {
                // Collect flattened entries recursively
                List<DocumentationDataModel> entries = GeneratorHelpers.CollectDocumentationEntries(classSymbol, compilation);

                // Build content
                StringBuilder sb = new();
                switch (docOptions.Format)
                {
                    case DocumentationFormat.AsciiDoc:
                        DocumentationGenerators.AsciiDocGenerator.GenerateAsciiDoc(sb, classSymbol, entries);
                        break;
                    case DocumentationFormat.Markdown:
                        DocumentationGenerators.MarkdownGenerator.Generate(sb, classSymbol, entries);
                        break;
                    default:
                        DocumentationGenerators.MarkdownGenerator.Generate(sb, classSymbol, entries);
                        break;
                }

                // Sanitize filename and add as generated source (virtual file)
                string fileName = GeneratorHelpers.SanitizeFileName(docOptions.OutputPath, docOptions.Format);
                // Ensure unique name per class to avoid collisions
                string uniqueName = classSymbol.Name + "." + fileName;
                context.AddSource(uniqueName, SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        }
    }
}
