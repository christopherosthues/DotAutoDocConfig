using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Collectors;
using DotAutoDocConfig.SourceGenerator.DocumentationGenerators;
using DotAutoDocConfig.SourceGenerator.DocumentationParser;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using DotAutoDocConfig.SourceGenerator.Renderers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotAutoDocConfig.SourceGenerator.TableGenerators;

internal class InlineTableGenerator : TableGeneratorBase
{
    public override void GenerateTable(DocumentationOptionsDataModel docOptions, SourceProductionContext context,
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot)
    {
        // Convert RootRows into DocumentationDataModel list for existing generator method
        LocalFormat fmt = (LocalFormat)docOptions.Format;
        // IDocumentationGenerator documentationGenerator = DocumentationGeneratorFactory.CreateGenerator(fmt);
        IDocumentationRenderer documentationRenderer = DocumentationRendererFactory.CreateRenderer(fmt);
        // IConfigurationCollector configurationCollector = new InlineTableCollector();
        // DocumentationTablesModel tables = configurationCollector.Collect(classSymbol);
        // List<DocumentationDataModel> entries = [..tables.RootRows.Select(row => row.Data)];
        IDocumentationParser documentationParser = new InlineTableParser();
        IList<IDocumentationNode> trees = documentationParser.Parse(classSymbol, docOptions.IncludeNamespaces);

        IDocumentationNode node = trees.First();
        node.Accept(documentationRenderer);
        // documentationGenerator.Generate(sb, classSymbol, entries, docOptions.IncludeNamespaces);

        // Resolve root output path and write
        string directory = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot);
        string ext = fmt.ToFileExtension();

        string baseName = docOptions.IncludeNamespaces ? CreateFileBaseNameWithNamespace(classSymbol) : classSymbol.Name;
        string candidate = Path.Combine(directory, baseName, ext);

        WriteResolvedFile(context, candidate, documentationRenderer.GetResult());
    }
}
