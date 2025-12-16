# Troubleshooting

- No file generated:
  - Ensure the class uses `DocumentationAttribute` and both constructor arguments are properly set.
  - Check info diagnostics (ID `DDG000`) in build output — especially `RepoRoot` and `RequestedPath`.
- Path issues:
  - For relative paths verify that `obj/DocsGenerated/<ProjectName>/...` exists.
  - For absolute paths check write permissions and folder existence.
- Test failures:
  - Add representative test data and isolate generator logic.
