# Tasks for Copilot

- Small improvements:
  - Tackle nullable warnings surgically without altering functionality.
  - Consolidate logging IDs and messages; avoid excessive output.
  - Make path handling more robust (e.g., trim/normalize `outputPath`).
- Feature extensions (add tests first):
  - Support more formats (e.g., HTML) if generators exist.
  - Optional configuration via MSBuild properties (e.g., default output directory, switch to disable writing).
- Documentation:
  - Enhance README (short section “How to use the attribute” with examples).
