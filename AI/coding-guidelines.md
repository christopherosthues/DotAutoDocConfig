# Coding Guidelines

- Language: C# 10+; keep nullable enabled. For generator IO, use `#pragma warning disable RS1035` sparingly and only around the write operations.
- Prefer explicit types over `var` in all cases (built-in types, apparent types, elsewhere).
- Keep public APIs stable; changes to `DocumentationAttribute` or generator signatures must be accompanied by tests.
- Prefer incremental patterns (`IIncrementalGenerator`, `SyntaxProvider.ForAttributeWithMetadataName`, `AnalyzerConfigOptionsProvider.Select`).
- Logging strictly via `context.ReportDiagnostic`; no Console/Debug output, no external loggers.
- Paths:
  - Use `Path.Combine` and `Path.GetFullPath`.
  - Create directories before writing (`Directory.CreateDirectory`).
  - No network/IO outside the document-writing step performed by the generator.
