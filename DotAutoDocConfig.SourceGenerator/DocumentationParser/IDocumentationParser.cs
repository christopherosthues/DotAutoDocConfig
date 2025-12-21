using System.Collections.Generic;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationParser;

internal interface IDocumentationParser
{
    IList<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> Parse(INamedTypeSymbol namedTypeSymbol, bool includeNamespaces);
}
