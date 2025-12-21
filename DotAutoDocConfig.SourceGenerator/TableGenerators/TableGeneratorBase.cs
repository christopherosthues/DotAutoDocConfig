using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.TableGenerators;

internal abstract class TableGeneratorBase : ITableGenerator
{
    public abstract void GenerateTable(DocumentationOptionsDataModel docOptions, SourceProductionContext context,
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot);

    protected static string CreateFileBaseNameWithNamespace(INamedTypeSymbol symbol)
    {
        string fq = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat); // e.g., global::Namespace.Type
        if (fq.StartsWith("global::"))
        {
            fq = fq.Substring("global::".Length);
        }

        StringBuilder sbName = new();
        foreach (char c in fq)
        {
            if (char.IsLetterOrDigit(c))
            {
                sbName.Append(c);
            }
            else if (c == '.' || c == '_' || c == '+')
            {
                sbName.Append('-');
            }
        }

        return sbName.ToString();
    }

    protected static string EnsureUniqueFileName(string baseName, string ext, HashSet<string> usedNames)
    {
        string candidate = baseName + ext;
        int i = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = baseName + "-" + i + ext;
            i++;
        }

        return candidate;
    }

    protected static void WriteResolvedFile(SourceProductionContext context, string fullPath, string content)
    {
        try
        {
            string? dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(fullPath, content, Encoding.UTF8);
#pragma warning disable CS8620
            context.LogInfo("Writing documentation (resolved): {0}", fullPath);
#pragma warning restore CS8620
        }
        catch (Exception ex)
        {
            DiagnosticDescriptor desc = new(DiagnosticIds.FileOutputFailed, "DocumentationGeneratorWriteFailed",
                "Failed to write documentation file '{0}': {1}", "DocumentationGenerator", DiagnosticSeverity.Warning,
                true);
            Diagnostic diagnostic = Diagnostic.Create(desc, Location.None, fullPath, ex.Message);
            context.ReportDiagnostic(diagnostic);
        }
    }

    protected static string ComposeRootOutputPath(
        SourceProductionContext context,
        string requestedPath,
        string projectDirectory,
        string repoRoot)
    {
        // Resolve base path first (absolute or under project root)
        string baseProjectRoot = !string.IsNullOrEmpty(projectDirectory) ? projectDirectory : repoRoot;
        string resolved = Path.IsPathRooted(requestedPath)
            ? requestedPath
            : Path.GetFullPath(Path.Combine(baseProjectRoot, requestedPath));

        // Determine if the user provided a directory or a file
        bool looksLikeDirectory = resolved.EndsWith(Path.DirectorySeparatorChar.ToString())
                                  || resolved.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                                  || !Path.HasExtension(resolved)
                                  || Directory.Exists(resolved);

        if (!looksLikeDirectory)
        {
            // The configured output path looks like a file. Only directories are allowed for the outputDirectory option.
            // Emit a warning and fall back to the containing directory of the provided path (or project root if none).
            string parentDir = Path.GetDirectoryName(resolved) ?? baseProjectRoot;
            DiagnosticDescriptor desc = new(DiagnosticIds.OutputDirectoryNotADirectory,
                "OutputDirectoryMustBeDirectory",
                "Output path '{0}' looks like a file; only directories are supported. Using directory '{1}' instead.",
                "DocumentationGenerator", DiagnosticSeverity.Warning, true);
            Diagnostic diagnostic = Diagnostic.Create(desc, Location.None, requestedPath, parentDir);
            context.ReportDiagnostic(diagnostic);

            // Treat the parent directory as the resolved directory
            resolved = parentDir;
        }

        // Ensure directory exists (create later on write)
        string directory = resolved.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return directory;
    }
}
