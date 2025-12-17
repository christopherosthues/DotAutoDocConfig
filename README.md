# DotAutoDocConfig

Generate configuration documentation from C# classes via a Roslyn source generator.

- Output formats: Markdown (.md) and AsciiDoc (.adoc)
- Complex property rendering:
  - InlineJsonShort: flatten nested properties using JsonShort notation (Property:Child:GrandChild)
  - SeparateTables: generate separate tables (and optionally separate files) for nested types
- Reuse-aware: nested type tables are generated once and referenced via links
- Namespaces: headings and file names include namespaces only when `includeNamespaces` is true

## Quick start

1. Add the package (project reference here) `DotAutoDocConfig.SourceGenerator` to your project.
2. Annotate your configuration class with `DocumentationAttribute`:

```csharp
[Documentation(DocumentationFormat.Markdown, "docs/AppConfiguration.md")]
public class AppConfiguration { /* ... */ }
```

Separate tables and file-per-type:

```csharp
[Documentation(
    DocumentationFormat.Markdown,
    "docs/AppConfiguration.md",
    ComplexParameterFormat.SeparateTables,
    includeNamespaces: true)]
public class AppConfiguration { /* ... */ }
```

## How it works

- The generator walks public properties, respects `ExcludeFromDocumentationAttribute`, parses XML docs for `summary` and `example`.
- In SeparateTables mode, complex properties get a link to their table. Tables for the same type (same symbol) are deduplicated.
- Output files for nested types:
  - If `includeNamespaces = true`: file names are based on the fully qualified type name (namespace + type) to avoid conflicts.
  - If `includeNamespaces = false`: file names are based on the simple type name; duplicates are disambiguated by `-2`, `-3`, ... suffixes.

## Samples

See `DotAutoDocConfig.Sample.Console` and the generated files under `DotAutoDocConfig.Sample.Console/docs/` after build.

## Docs

- AI working instructions: `AI/`
- Generator behavior: `AI/generator.md`
- Coding guidelines: `AI/coding-guidelines.md`
- Build & run: `AI/build-run.md`
- Tests: `AI/tests.md`

## License

See [LICENSE](LICENSE).
