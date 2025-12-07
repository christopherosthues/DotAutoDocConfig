using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.DocumentationGenerators;

public static class MarkdownGenerator
{
    public static void Generate(StringBuilder sb, INamedTypeSymbol classSymbol)
    {
        sb.AppendLine("= Configuration Documentation");
        sb.AppendLine();
        sb.AppendLine($"== {classSymbol.Name}");
        sb.AppendLine();

        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            // Check for ExcludeFromDocumentation attribute
            var excludeAttribute = member.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() ==
                                        "DotAutoDocConfig.Core.ComponentModel.Attributes.ExcludeFromDocumentationAttribute");

            if (excludeAttribute != null)
            {
                continue; // Skip this property
            }

            sb.AppendLine($"=== {member.Name}");
            sb.AppendLine();
            sb.AppendLine($"Type: `{member.Type.ToDisplayString()}`");
            sb.AppendLine();
            sb.AppendLine($"Default Value: `{GetDefaultValue(member)}`");
            sb.AppendLine();
        }
    }
}
