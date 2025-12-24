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
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot, IList<string> filePaths)
    {
        LocalFormat fmt = (LocalFormat)docOptions.Format;
        IDocumentationRenderer documentationRenderer = DocumentationRendererFactory.CreateRenderer(fmt);
        IDocumentationParser documentationParser = new InlineTableParser();
        IList<IDocumentationNode> trees = documentationParser.Parse(classSymbol, docOptions);

        IDocumentationNode tree = trees.First();
        tree.Accept(documentationRenderer);

        // Resolve root output path and write
        string directory = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot);
        string ext = fmt.ToFileExtension();

        string baseName = docOptions.IncludeNamespaces ? CreateFileBaseNameWithNamespace(tree.NamedTypeSymbol) : tree.NamedTypeSymbol.Name;
        string candidate = Path.Combine(directory, baseName + ext);

        // TODO: Handle name conflicts

        WriteResolvedFile(context, candidate, documentationRenderer.GetResult());

        filePaths.Add(candidate);
    }
}
