# CLAUDE.md

## C#/.NET Code Style

- Avoid `static` classes unless there is no reasonable alternative (e.g. true stateless helpers with no substitutable behavior). Prefer instance classes so implementations can be composed and substituted.
- Do not write XML doc comments (`///`).

## Git Workflow

- After creating a commit, do not ask whether to push it to the remote. The user pushes manually.
- Do not create commits yourself unless explicitly asked to in that turn. The user reviews generated files and writes commit messages themselves.
