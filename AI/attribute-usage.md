# Example: Using the Documentation Attribute

```csharp
using DotAutoDocConfig.Core.ComponentModel.Attributes;

[Documentation((int)DocumentationFormat.Markdown, "Docs/Config.md")]
public class AppConfiguration
{
    // ... properties documented by the generator ...
}
```

Note: `DocumentationFormat` is located in `DotAutoDocConfig.Core.ComponentModel`. If you prefer not to import the enum, pass the numeric value.
