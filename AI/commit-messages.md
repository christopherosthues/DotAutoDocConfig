# GitHub Copilot – Commit Message Instructions

Goal: Consistent, meaningful commit messages following Conventional Commits.

## Format

- Subject: `type(scope)!: short imperative summary`
  - Imperative mood, max 50 characters, no trailing period
  - `!` indicates a breaking change
- Body: Explain why and what changed, and the impact
  - Wrap lines at ~72 characters
  - Use bullet points for multiple changes
- Footer: Issue references and co-authors
  - `Closes: #123`, `Refs: #456`
  - `BREAKING CHANGE: description`
  - `Co-authored-by: Name <mail@example.com>`

## Types

- `feat`: new feature
- `fix`: bug fix
- `docs`: documentation only
- `style`: formatting, no code changes (whitespace, commas, etc.)
- `refactor`: code change that neither fixes a bug nor adds a feature
- `perf`: performance improvements
- `test`: adding or updating tests
- `build`: build system, NuGet, packaging
- `ci`: pipelines, workflows
- `chore`: maintenance tasks
- `revert`: revert a previous commit

## Scopes (examples)

- `core`, `generator`, `analyzer`, `runtime`, `cli`
- `docs`, `tests`, `samples`
- `build`, `ci`, `infrastructure`, `security`

Pick the narrowest scope that fits. Prefer repo terms: `DotAutoDocConfig.Core` → `core`, `DotAutoDocConfig.SourceGenerator` → `generator`, etc.

## Rules for Copilot

- Always write the subject in imperative mood, max 50 characters
- Add a body when context matters (why, impact, trade-offs)
- Mark breaking changes with `!` in the subject and `BREAKING CHANGE:` in the footer
- Call out relevant performance, security, or API aspects
- For multiple changes: use a bullet list in the body
- Reference issues in the footer (`Closes: #...` or `Refs: #...`)
- For pure refactors: state that behavior does not change

## Templates

One-liner (small change):
- `fix(generator): handle null attributes in symbol walker`

With body:
- Subject: `feat(analyzer)!: add rule to forbid sync IO in generators`
- Body:
  - Why: prevent build deadlocks and timeouts
  - What: new rule `GEN001`, default severity `warning`
  - Migration: opt-out via `#pragma warning disable GEN001`
- Footer:
  - `BREAKING CHANGE: builds may fail if severity is elevated to error`
  - `Closes: #321`

Footer examples:
- `Refs: #123`
- `Co-authored-by: Jane Doe <jane@example.com>`

## Good examples

- `perf(generator): avoid allocations in syntax visitor`
- `refactor(core): extract service registration extension`
- `docs(samples): add attribute usage examples for generator`

## Bad examples

- `update stuff`
- `fix`
- `changes`

## Checklist

- Subject is short, clear, imperative
- Correct `type` and `scope`
- Body explains why and impact (if not obvious)
- Tests/docs updated (mention notable changes in body)
- Issues and breaking changes in footer

