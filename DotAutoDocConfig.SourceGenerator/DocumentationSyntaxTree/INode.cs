using DotAutoDocConfig.SourceGenerator.Renderers;

namespace DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;

internal interface INode
{
    void Accept(IDocumentationRenderer renderer);
}
