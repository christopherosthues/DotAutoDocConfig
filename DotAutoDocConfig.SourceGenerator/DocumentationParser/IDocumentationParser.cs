using System.Collections.Generic;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal interface IDocumentationParser
{
    IList<IDocumentationNode> Parse(INamedTypeSymbol namedTypeSymbol, DocumentationOptionsDataModel options);
}
