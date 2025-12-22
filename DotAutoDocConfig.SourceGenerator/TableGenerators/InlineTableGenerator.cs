using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationParser;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using DotAutoDocConfig.SourceGenerator.Renderers;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.TableGenerators;

internal class InlineTableGenerator : TableGeneratorBase
{
    public override void GenerateTable(DocumentationOptionsDataModel docOptions, SourceProductionContext context,
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot)
    {
        LocalFormat fmt = (LocalFormat)docOptions.Format;
        IDocumentationRenderer documentationRenderer = DocumentationRendererFactory.CreateRenderer(fmt);
        IDocumentationParser documentationParser = new InlineTableParser();
        IList<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> trees = documentationParser.Parse(classSymbol, docOptions.IncludeNamespaces);

        (INamedTypeSymbol symbol, IDocumentationNode tree) = trees.First();
        tree.Accept(documentationRenderer);

        // Resolve root output path and write
        string directory = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot);
        string ext = fmt.ToFileExtension();

        string baseName = docOptions.IncludeNamespaces ? CreateFileBaseNameWithNamespace(symbol) : symbol.Name;
        string candidate = Path.Combine(directory, baseName + ext);

        WriteResolvedFile(context, candidate, documentationRenderer.GetResult());
    }
}
