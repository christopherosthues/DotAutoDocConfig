using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class TypeSymbolExtensions
{
    extension(ITypeSymbol? typeSymbol)
    {
        public string GetExampleValue()
        {
            try
            {
                if (typeSymbol is null)
                {
                    return string.Empty;
                }

                if (typeSymbol.TypeKind == TypeKind.Enum && typeSymbol is INamedTypeSymbol enumType)
                {
                    IFieldSymbol? first = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.ConstantValue != null);
                    return first?.Name ?? string.Empty;
                }

                switch (typeSymbol.SpecialType)
                {
                    case SpecialType.System_String:
                        return "example";
                    case SpecialType.System_Boolean:
                        return "true";
                    case SpecialType.System_Char:
                        return "c";
                    case SpecialType.System_Double:
                    case SpecialType.System_Single:
                    case SpecialType.System_Decimal:
                    case SpecialType.System_Int32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_Int16:
                    case SpecialType.System_Byte:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_UInt64:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_SByte:
                        return "123";
                }

                if (typeSymbol is IArrayTypeSymbol)
                {
                    return "[ ]";
                }

                if (typeSymbol is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length == 1)
                {
                    ITypeSymbol firstArg = named.TypeArguments[0];
                    if (firstArg.SpecialType == SpecialType.System_String)
                    {
                        return "[ \"en\" ]";
                    }

                    return "[ ]";
                }

                return "{ }";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
