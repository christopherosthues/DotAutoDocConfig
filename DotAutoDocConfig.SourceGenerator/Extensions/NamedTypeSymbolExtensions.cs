using System.Text;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

public static class NamedTypeSymbolExtensions
{
    extension(INamedTypeSymbol typeSymbol)
    {
        public string FriendlyQualifiedName(bool includeNamespaces)
        {
            if (!includeNamespaces)
            {
                return typeSymbol.Name;
            }
            string ns = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            if (string.IsNullOrEmpty(ns))
            {
                return typeSymbol.Name;
            }

            return ns + "." + typeSymbol.Name;
        }

        public void AddSummary(StringBuilder sb)
        {
            string summary = typeSymbol.GetSummary();
            if (!string.IsNullOrEmpty(summary))
            {
                sb.AppendLine();
                sb.AppendLine(summary);
            }
            sb.AppendLine();
        }
    }
}
