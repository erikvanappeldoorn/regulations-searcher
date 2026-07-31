# CLAUDE.md

## C#/.NET Code Style

- Avoid `static` classes unless there is no reasonable alternative (e.g. true stateless helpers with no substitutable behavior). Prefer instance classes so implementations can be composed and substituted.
- Do not write XML doc comments (`///`).
