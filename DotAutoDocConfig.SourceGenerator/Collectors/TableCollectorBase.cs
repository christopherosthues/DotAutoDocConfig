using System;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Collectors;

internal abstract class TableCollectorBase
{
    protected static bool ShouldRecurseInto(INamedTypeSymbol type)
    {
        // Do not recurse into system types or enums or delegates
        if (type.TypeKind == TypeKind.Enum)
        {
            return false;
        }

        if (type.SpecialType != SpecialType.None)
        {
            return false;
        }

        // Common framework types we treat as leaves
        string ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (ns.StartsWith("System", StringComparison.Ordinal))
        {
            return false;
        }

        // Treat records/classes defined in user's code as recurse candidates
        return true;
    }
}
