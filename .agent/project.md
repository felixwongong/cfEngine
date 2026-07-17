# Project

## What we are building

`cfEngine` is a shared C# core library used by `cfGodotEngine`, `cfGameEngine`, and CatSweeper. It contains engine-agnostic utilities; Godot-specific code belongs in `cfGodotEngine`.

## Stack

| Layer | Choice | Notes |
|---|---|---|
| Language | C# | .NET 8 |
| Consumers | cfGodotEngine, cfGameEngine, CatSweeper | Imported as Git subtrees |
| Solution | `cfEngine.sln` | Build with `dotnet build` |

## Module map

Verified top-level folders:

| Folder | Contents |
|---|---|
| `monad/` | `Res<T>`, `Optional`, `Validation` result types |
| `rx/` | Reactive collections (`RtDictionary`, `RtGroup`, `RtList`) |
| `util/` | `Relay`, `SanityCheck`, command helpers |
| `log/` | Logging (`Log`, `ILogger`) |
| `datastructure/` | `GridMap` and related containers |
| `info/` | Info value loaders |
| `serialize/` | JSON serialization |
| `service/` | Auth/statistic service base |
| `asset/`, `core/`, `extension/`, `game/`, `input/`, `io/`, `pooling/` | Supporting utilities |

## Domain in one paragraph

The library provides engine-agnostic primitives that higher-level frameworks build on. `Res<T>` is the error-return envelope used across CatSweeper gameplay APIs; `Relay` underpins `cfGodotEngine` binding and `ReactiveProperty<T>`; `SanityCheck` collapses required-parameter guards to one line. Changes here ripple into `cfGodotEngine` and CatSweeper behavior.

## Non-obvious constraints

- Checked out inside CatSweeper as a Git subtree at `Modules/cfEngine/`. Only the programmer/owner pushes changes back to `https://github.com/felixwongong/cfEngine.git` via `Tools/subtree.ps1`.
- Any breaking change must be coordinated with `dotnet build CatSweeper.sln`.
- Godot-specific code does not belong here; it belongs in `cfGodotEngine`.

## CatSweeper usage

CatSweeper uses this library both directly (`Res<T>`, `Relay`, `Log`, `SanityCheck`) and through `cfGodotEngine`. Edits here require a subtree push.

See CatSweeper's `.agent/systems/subtrees.md` for the subtree workflow.
