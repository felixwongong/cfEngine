# Architecture (cf* module standard)

The reusable, game-agnostic standard for any project built on the cf* modules. This is
the **base** copy that ships with `cfEngine`; a game repo keeps its own `.agent/architecture.md`
as the game-specific instance (canonical reference domain, real paths, this game's rules).

Authoring at the module/repo level is governed by the `cf-coding-standard` skill
(`Modules/cfEngine/skills/cf-coding-standard/SKILL.md`). This file covers the **in-game**
layer rules.

## The module standard

Four layers. Dependencies point DOWN only; nothing points up.

| Layer | Module | Should have | Must NOT have |
|---|---|---|---|
| 1 | **cfEngine** | App-generic pure C# (useful to ANY app, game or not): Rx, Res/Optional/validation, IService/Domain/ServiceLocator, UserDataManager, pooling, GridMap, serialize | Game concepts (items, missions, HP); Godot; heavy deps (≈ Newtonsoft only) |
| 2 | **cfGameEngine** | Game-generic systems in pure C# (any game reuses): Mission, Achievement, Energy, IAP, Inventory, gameitem | Godot types; one game's rules/data; app-generic utilities (those sink to cfEngine) |
| 3 | **cfGodotEngine** | Godot adapters of the layers below: UIPanel/UIRoot, Actor2D, StateMachineNode, binding, scene/asset managers, GodotLogger, editor + debug tooling | Game rules; logic testable without Godot (push it down) |
| 4 | **Game repo** | Game-specific domains, Info data, composition root, game states | Anything matching tests 2–4 of the litmus below |

**Authoring litmus test** (first match from top wins):

1. Does it know MY game's rules? → game repo
2. Does it touch Godot API? → cfGodotEngine
3. Is it a game system ANY game reuses? → cfGameEngine
4. Is it useful to ANY app? → cfEngine

The game repo is the LAST resort, never the default.

## Where does my code go? (decision tree)

Answer these in order. First match wins. If nothing matches, **stop and ask** — do not improvise a location.

```text
1. Reads config/design tables?                    → Scripts/Info/<X>Info/<X>InfoManager : ConfigInfoManager
2. Needs Godot API (scene tree, nodes, Control)?
   ├─ Display only (panel, HUD, tile visuals)?    → View layer (see §Layers)
   └─ Input / gameplay glue / scene root?         → Scripts/<Domain>/Controller/ (Actor2D)
3. Per-instance runtime state with a create/dispose
   lifecycle (a stage run, a battle, a match)?    → Scripts/<Domain>/Session/
4. App-scoped facade: resolves Info, reads/writes
   save data, validation, owns Session lifetime?  → Scripts/<Domain>/Service/ : IService
5. Pure domain logic used by a Session/Service?   → Scripts/<Domain>/<Subsystem>/ (e.g. Board/, Player/)
6. None of the above                              → STOP. Ask before creating any new file or folder.
```

Copy the shape of your canonical reference domain for every new domain.

## Layers

| Layer | Path pattern | Base type | Pure C#? | May reference |
|---|---|---|---|---|
| Info | `Scripts/Info/<X>Info/` | `ConfigInfoManager<string, X>` | yes | cfEngine only |
| Subsystem | `Scripts/<Domain>/<Subsystem>/` | plain class | yes | cfEngine, Info |
| Session | `Scripts/<Domain>/Session/` | `IDisposable` | yes* | cfEngine, Info, Subsystems |
| Service | `Scripts/<Domain>/Service/` | `IService` | yes* | cfEngine, Info, Session, save data |
| View state | beside the view | plain class + `Bindable<T>` fields | yes | cfEngine.Rx |
| View shell | `Scripts/<Domain>/View/`, `Scripts/UI/<Domain>UI/` | `UIPanel` / Godot node | no | Service (read + subscribe), Bindables |
| Controller | `Scripts/<Domain>/Controller/` | `Actor2D` | no | Service, Session, Views, UIRoot |

\* "Pure C#" here means: **no Godot scene/tree/node APIs**. Godot *math* types (`Vector2I`, `Vector2`) are allowed.

## Two purity levels (do not conflate)

- **cfEngine and cfGameEngine are strictly Godot-free** — no `using Godot`, no Godot package,
  enforced by the cross-module dependency-direction test.
- **Godot math types are allowed only in the game repo's pure layers** (Info / Service /
  Session / Subsystems). That tolerance does NOT extend to the modules.

## Rules (enforced — see §Gates)

1. **R1 — Domain layers are Godot-free.** Files under `Info/`, `Service/`, `Session/`, and domain `Subsystem/` folders MUST NOT reference `Node`, `Control`, `SceneTree`, `UIRoot`, `UIPanel`, `GetNode`, `AddChild`, `Domain.Current.Get<UI…>`. (Automated: the layering tests.)
2. **R2 — Service never touches views.** A `Service` publishes state only via `IRelay`/`ReactiveProperty`. It MUST NOT look up, bind, or show views.
3. **R3 — Binding owner = the domain's Godot-side entry point.** Controller binds views if one exists; otherwise the View self-binds in `_Ready()`. Never both.
4. **R4 — Session is conditional.** Add a `Session` only when the domain has disposable per-instance state with live subscriptions (a run, a battle). CRUD-style domains (Achievement, Shop, settings) are Info + Service + View only. When in doubt: no Session.
5. **R5 — No loose scripts.** A gameplay-domain folder contains only layer/subsystem subfolders; no `.cs` files directly at `Scripts/<Domain>/` (framework roots `Core/`, `UI/`, `Util/`, `Debug/` are exempt). No new top-level `Scripts/` folder without an entry in the architecture doc. (Automated: the layering tests.)
6. **R6 — Errors flow as `Res<T>`**, not exceptions, across Service boundaries. Cross-layer events use `IRelay`, never direct upward calls.
7. **R7 — UI navigation triggers stay on the Godot side:** GameState shows phase-root UI, Views show peer panels (buttons), Controllers show event-driven panels. Services never call `UIRoot.Show`.
8. **R8 — Fields `private` by default.** Public members need a real caller.
9. **R9 — Validated mutations chain through the fluent validation extensions.** When a Service validates gameplay/user actions, callers chain `.Validate(...)` (or `.ValidateAsync(...)`) on the input to gate execution, or on a result to verify it, branching with `.IfTrue(...)` / `.IfFalse(...)`; rules are lambdas or `ValidationRule<T>` statics, `IValidator<T>` via `ValidateWith`. `AsyncValidationRule<T>` + `ResolveAsync` are the server-growth path (remote rule / server-computed outcome).

## Anti-patterns (do not reintroduce)

| Anti-pattern | Why it's wrong | Correct shape |
|---|---|---|
| A `Service` resolving `Domain.Current.Get<UIRoot>()` to bind views | R1/R2 violation; Service untestable | Controller binds views (R3) |
| View classes loose at `Scripts/<Domain>/` root | R5; hides the layer | `View/<X>View.cs`, `Controller/<X>Controller.cs` |
| View mutating domain state directly | Views display only | Command goes through Service/Session |
| A `Session` for a stateless domain | R4; layer cargo-cult | Service + save data |

## Adding a new domain (checklist)

1. `Scripts/Info/<X>Info/` — `<X>Info` record + `<X>InfoManager : ConfigInfoManager<string, X>`; register in the info-load game state; add a pipeline source if data-driven.
2. `Scripts/<Domain>/Service/<Domain>Service.cs : IService`; register in the Domain.
3. `Session/` only if R4 says yes.
4. View: `UIPanel` + pure-C# `Bindable` state; register in the UI-load game state / `UIRoot`.
5. `Controller/` only if Godot-side interaction beyond display exists.
6. Update the game's architecture doc if the domain adds a new top-level folder.

## Gates

| Gate | Who | What |
|---|---|---|
| Author | writing agent | decision tree + rules above |
| Reviewer | review agent / human | the game repo's architecture-review checklist |
| Mechanical | `dotnet build` + `dotnet test` | the layering tests (R1, R5) + the cross-module dependency-direction test |

## Module boundaries and integration points

- **Module standard / authoring:** [`skills/cf-coding-standard/SKILL.md`](../skills/cf-coding-standard/SKILL.md).
- **Layering test fixture:** the reusable `LayeringRuleTestsBase` + its copy-in template
  (`skills/cf-coding-standard/LayeringRuleTests.cs.template`) — a game subclasses it and supplies its config.
- **Cross-module dependency-direction test:** asserts cfEngine/cfGameEngine never reference upward or Godot, and cfGodotEngine never references the game.
- **Subtrees:** cf* modules are Git subtrees; module-touching commits push parent AND every touched subtree.
