# Tests

- Unit tests for the generator:

```powershell
dotnet test .\DotAutoDocConfig.SourceGenerator.Tests\DotAutoDocConfig.SourceGenerator.Tests.csproj
```

- When changing code, add tests: happy path + at least one edge case (no attribute, relative path, absolute path, missing write permissions).
