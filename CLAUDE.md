# CLAUDE.md

## C#/.NET Code Style

- Avoid `static` classes unless there is no reasonable alternative (e.g. true stateless helpers with no substitutable behavior). Prefer instance classes so implementations can be composed and substituted.
- Do not write XML doc comments (`///`).
- One type per file. Every class, record, interface, enum, etc. gets its own file, named after the type.

## Bicep / Infrastructure

- Never reference an Azure built-in role definition by GUID alone. Store the GUID in a variable named after the role, and/or add a comment with the role's display name directly above it, so the role is identifiable without looking up the GUID.

## Git Workflow (default)

- After creating a commit, do not ask whether to push it to the remote. The user pushes manually.
- Do not create commits yourself unless explicitly asked to in that turn. The user reviews generated files and writes commit messages themselves.
- This project uses trunk-based development: when creating a commit, never ask for confirmation to commit directly on `main` (or create a branch instead) — always commit straight to `main` without that prompt.
