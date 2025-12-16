# Source Generator – Behavior and Paths

- The generator looks for classes with `DotAutoDocConfig.Core.ComponentModel.Attributes.DocumentationAttribute`.
- The attribute has two arguments: `(format, outputPath)`
  - `format`: numeric value (enum-compatible) – supported: `AsciiDoc`, `Markdown` (fallback), `Html` (reserved).
  - `outputPath`: target file path (relative or absolute).
- Path resolution:
  - If `outputPath` is absolute: write directly to that path.
  - If relative: resolve against the project root (directory of the `.csproj`) and write there.
  - Project context (`ProjectName`/`ProjectDirectory`) is read from AnalyzerConfig global options (`build_property.MSBuildProjectName`, `build_property.MSBuildProjectDirectory`/`ProjectDir`).
- Logging (Diagnostics):
  - Info messages (ID `DDG000`) for repo/project root, requestedPath, and final write path.
  - Warning (ID `DDG001`) on write failures.

## Structure and key files
- Generator: `DotAutoDocConfig.SourceGenerator/DocumentationSourceGenerator.cs`
  - `Initialize` combines compilation, discovered classes, and build properties.
  - `GenerateCode` creates content via `DocumentationGenerators.*` and writes files.
  - Helper functions: `GetDocumentationDataModels`.
- Content generators:
  - `DotAutoDocConfig.SourceGenerator/DocumentationGenerators/MarkdownGenerator.cs`
  - `DotAutoDocConfig.SourceGenerator/DocumentationGenerators/AsciiDocGenerator.cs`
