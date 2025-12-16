# DotAutoDocConfig – Documentation Source Generator

This project provides an incremental Roslyn source generator that produces documentation files (Markdown or AsciiDoc) for classes annotated with a Documentation attribute. It is designed for configuration-type classes where public properties and their XML doc comments form the basis of the generated docs.

## What it does
- Scans classes annotated with `DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute`.
- Walks all public properties; recurses into user-defined complex types to flatten nested properties into dot-separated keys (e.g., `Database.CommandTimeout`).
- Uses XML doc comments:
  - `<summary>` becomes the description
  - `<example>` becomes the example value (falls back to sensible defaults)
- Supports `ExcludeFromDocumentationAttribute` to skip properties (or classes).
- Writes documentation to the `outputPath` from the attribute:
  - Relative paths are resolved against the project root (the `.csproj` directory)
  - Absolute paths are written as-is
- Supported formats via `DocumentationFormat` enum:
  - `AsciiDoc` (1), `Markdown` (2); `Html` is reserved; unknown defaults to Markdown
- Emits diagnostics:
  - `DDG000` Info logs (repo/project root, requested path, final path)
  - `DDG001` Warning when writing a file fails
  - `DDG002` Warning when the output path is empty

## Key files
- Generator core: [DocumentationSourceGenerator.cs](./DocumentationSourceGenerator.cs)
- Property traversal and XML extraction: [GeneratorHelpers.cs](./GeneratorHelpers.cs)
- Content writers: [DocumentationGenerators/MarkdownGenerator.cs](./DocumentationGenerators/MarkdownGenerator.cs), [DocumentationGenerators/AsciiDocGenerator.cs](./DocumentationGenerators/AsciiDocGenerator.cs)
- Models: [Models/DocumentationDataModel.cs](./Models/DocumentationDataModel.cs), [Models/DocumentationOptionsDataModel.cs](./Models/DocumentationOptionsDataModel.cs)
- Debug profile: [Properties/launchSettings.json](./Properties/launchSettings.json)

Related projects:
- Sample app using the generator: [DotAutoDocConfig.SourceGenerator.Sample](../DotAutoDocConfig.SourceGenerator.Sample/DotAutoDocConfig.SourceGenerator.Sample.csproj)
- Tests: [DotAutoDocConfig.SourceGenerator.Tests](../DotAutoDocConfig.SourceGenerator.Tests)
- Core attributes and enums: `DotAutoDocConfig.Core`

## Usage in a consumer project
1) Reference the generator as an Analyzer in your `.csproj` (example uses the sample project):

```xml
<ItemGroup>
  <ProjectReference Include="..\DotAutoDocConfig.SourceGenerator\DotAutoDocConfig.SourceGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

2) Annotate a class you want to document:

```csharp
using DotAutoDocConfig.Core.ComponentModel;
using DotAutoDocConfig.Core.ComponentModel.Attributes;

[Documentation(DocumentationFormat.Markdown, "docs/AppConfiguration.md")]
[Documentation(DocumentationFormat.AsciiDoc, "docs/AppConfiguration.adoc")]
public class AppConfiguration
{
    /// <summary>Maximum number of items.</summary>
    /// <example>50</example>
    public int MaxItems { get; set; } = 100;

    public DatabaseConfiguration Database { get; set; } = new();
}

public class DatabaseConfiguration
{
    /// <summary>Timeout in seconds applied to database commands.</summary>
    public int CommandTimeout { get; set; } = 60;
}
```

3) Build the project. The generator writes documentation files to the specified `outputPath`:
- If the path is relative (e.g., `docs/AppConfiguration.md`), the file is created under the project root
- If the path is absolute, it is written directly to that location

See a complete example in the sample app: [AppConfiguration.cs](../DotAutoDocConfig.Sample.Console/AppConfiguration.cs)

## Debugging the generator
- Use the dedicated profile in [launchSettings.json](./Properties/launchSettings.json): it debugs the generator while loading the sample project target.
- You can also debug tests under `DotAutoDocConfig.SourceGenerator.Tests` for a tight TDD loop.

## Notes and limitations
- The generator performs file I/O to write documentation. On failure, it reports a warning (`DDG001`) and continues without crashing the build.
- Only public properties are documented. Use `ExcludeFromDocumentationAttribute` to opt out specific members.
- For collections, an item type is inferred (e.g., `IEnumerable<T>` → `T`) to decide whether to recurse or emit a leaf.

## Learn more
- Generator behavior and path resolution: [AI/generator.md](../AI/generator.md)
- Attribute usage example: [AI/attribute-usage.md](../AI/attribute-usage.md)
- Build & run (commands): [AI/build-run.md](../AI/build-run.md)
- Troubleshooting: [AI/troubleshooting.md](../AI/troubleshooting.md)
