using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class NamedTypeSymbolExtensions
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

        public bool IsCustomClass()
        {
            // Do not recurse into system types or enums or delegates
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                return false;
            }

            if (typeSymbol.SpecialType != SpecialType.None)
            {
                return false;
            }

            // Common framework types we treat as leaves
            string ns = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            if (ns.StartsWith("System", StringComparison.Ordinal))
            {
                return false;
            }

            // Treat records/classes defined in user's code as custom classes
            return true;
        }
    }
}
