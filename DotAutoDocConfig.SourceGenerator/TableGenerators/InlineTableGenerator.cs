using System.Collections.Generic;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.DocumentationParser;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Models;
using DotAutoDocConfig.SourceGenerator.Renderers;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.TableGenerators;

internal class InlineTableGenerator : TableGeneratorBase
{
    public override void GenerateTable(DocumentationOptionsDataModel docOptions, SourceProductionContext context,
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot, ISet<string> filePaths)
    {
        LocalFormat fmt = docOptions.Format;
        string directory = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot);
        IDocumentationRenderer documentationRenderer = DocumentationRendererFactory.CreateRenderer(fmt);
        IDocumentationParser documentationParser = new InlineTableParser();
        IList<IDocumentationNode> trees = documentationParser.Parse(classSymbol, docOptions, directory, filePaths);

        IDocumentationNode tree = trees.First();
        tree.Accept(documentationRenderer);

        // TODO: Handle name conflicts

        WriteResolvedFile(context, tree.OutputFilePath, documentationRenderer.GetResult());
    }
}
