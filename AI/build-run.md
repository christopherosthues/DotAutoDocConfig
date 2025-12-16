# Build & Run

- The project builds with .NET SDK in the default Debug configuration.

## Commands (Windows PowerShell)

```powershell
# Restore solution and build
dotnet restore ; dotnet build

# Run sample
dotnet run --project .\DotAutoDocConfig.Sample.Console\DotAutoDocConfig.Sample.Console.csproj
```

- Generated docs:
  - For relative `outputPath`: files are written under the project root (the `.csproj` directory) at that relative path.
  - For absolute `outputPath`: files are written directly to the specified absolute path.
