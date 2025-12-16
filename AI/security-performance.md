# Security and Performance Notes

- No network calls or access to external resources from the generator.
- Keep IO minimal and restricted to document writing.
- Avoid expensive re-analysis: use incremental providers and combine data sources efficiently.
