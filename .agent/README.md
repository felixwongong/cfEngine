# .agent — Agent context

Read this file first each session. These docs are the source of truth for cfEngine context.

## Must read (every session)

1. [`project.md`](project.md) — what this library is, stack, domain

Root pointer: [`AGENTS.md`](../AGENTS.md) in the library root.

## Task routing

| If you are... | Read |
|---|---|
| Working on a subsystem | Source code and the relevant CatSweeper `systems/` doc |
| Changing how CatSweeper uses this library | CatSweeper `.agent/systems/subtrees.md` and this `project.md` |
| Open questions or blockers | [`pending.md`](pending.md) |

## Rules

- Keep docs in sync with code changes.
- Append one line to [`CHANGELOG.md`](CHANGELOG.md) per significant change if the file exists.
