# Project Overview

- Project language is English. Keep code comments, documentation, and AI instructions in English.
- The main AI instruction file (this file for Copilot; use the equivalent location for other AIs) must reference specific chapters via clickable Markdown file links (format: `[Title](relative/path.md)`).

- Solution: `DotAutoDocConfig.slnx`
- Main components:
  - `DotAutoDocConfig.Core`: base types and attributes (e.g., `DocumentationAttribute`).
  - `DotAutoDocConfig.SourceGenerator`: incremental Roslyn source generator producing docs from annotated classes.
  - `DotAutoDocConfig.Sample.Console`: sample app demonstrating usage; mirrors docs under `bin/Debug/DocsGenerated/` via MSBuild targets.
  - `DotAutoDocConfig.SourceGenerator.Tests`: tests for the generator.

- Additional guides:
  - [Commit message instructions](./commit-messages.md)
