using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class PropertySymbolExtensions
{
    extension(IPropertySymbol propertySymbol)
    {
        public string GetDefaultValue()
        {
            try
            {
                SyntaxReference? declRef = propertySymbol.DeclaringSyntaxReferences.FirstOrDefault();
                if (declRef != null)
                {
                    SyntaxNode node = declRef.GetSyntax();
                    if (node is PropertyDeclarationSyntax propSyntax && propSyntax.Initializer != null)
                    {
                        return propSyntax.Initializer.Value.ToString();
                    }
                }

                if (propertySymbol.Type.TypeKind == TypeKind.Enum)
                {
                    if (propertySymbol.Type is INamedTypeSymbol enumType)
                    {
                        IFieldSymbol? first = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.ConstantValue != null);
                        return first?.Name ?? string.Empty;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
