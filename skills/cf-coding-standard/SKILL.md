---
name: cf-coding-standard
description: "Coding placement and reuse standard for any project built on the cf*
  modules (cfEngine, cfGameEngine, cfGodotEngine). Use when creating a new class,
  file, or feature; refactoring or moving code; deciding which module/repo code
  belongs in; or reviewing a diff for module-boundary violations. Covers the module
  litmus test, generic-first authoring, onion decomposition, logic/data split, and
  composition-over-inheritance across module boundaries. Do NOT use for in-game
  layer rules (Info/Service/Session/View/Controller) — see .agent/architecture.md."
---

# cf-coding-standard

Where code goes across the cf* module stack, how to author reusable code, and how to
review a diff against the standard.

## Module litmus test (first match from top wins)

1. Does it know MY game's rules?          → game repo
2. Does it touch the Godot API?           → cfGodotEngine
3. Is it a GAME system any game reuses?   → cfGameEngine
   (Mission, Achievement, Energy, IAP, Inventory, gameitem)
4. Is it useful to ANY app, game or not?  → cfEngine
   (Rx, Res/Optional, validation, IService, UserDataManager, pooling)

The game repo is the LAST resort, never the default. If nothing matches, stop and
ask — do not improvise a location.

## Module boundaries

| Module | Holds | Must NOT hold |
|---|---|---|
| cfEngine | App-generic pure C# | Game concepts; Godot; heavy deps |
| cfGameEngine | Game-generic pure C# systems | Godot; one game's rules/data; app utilities |
| cfGodotEngine | Godot adapters (UI, scene, binding, tools, debug) | Game rules; logic testable without Godot |
| Game repo | Game domains, Info data, composition root | Anything matching tests 2–4 |

Dependencies point DOWN only (game → cfGodotEngine → cfGameEngine → cfEngine).
cfEngine and cfGameEngine are Godot-free — enforced by the cross-module
dependency-direction test.

## Authoring rules

1. **Generic-first.** Decompose every feature onion-style: generic core sinks to the
   lowest valid layer, game-specific shell stays. Canonical example: ArenaController →
   state machine (cfEngine), pure match logic (cfGameEngine), node-lifecycle shell
   (cfGodotEngine), this game's rules (game repo).
2. **Generalize the shape, implement the need.** Correct layer + game-free interface,
   but no configuration knobs for imagined future games.
3. **Logic/data split.** System logic lives in the module; system data (which
   achievements, what costs) lives in the game's Info tables. Hardcoded game rules in
   a module = that part belongs in the game repo.
4. **Modules export tools, not ancestors.** Compose module classes as members
   ([Export], constructor, Domain.Current.Get<>()). Inheritance is allowed only for
   the one Godot-forced shell level (Actor2D, UIPanel, StateMachineNode). Never build
   a hierarchy chain across module boundaries — a virtual change in a module silently
   breaks subclasses the module repo cannot see. Module classes: sealed by default,
   minimal virtual hooks.
5. **Debug code** lives in cfGodotEngine/debug and must be strippable for release.

## Review checklist (diffs)

1. Every new/changed file passes the litmus test — correct module/repo?
2. Any new file in a module that references a game type, or a lower module
   referencing a higher one? (Must be none; dependency-direction test covers this.)
3. New Godot API usage outside cfGodotEngine / the game repo's Godot-side layers?
4. New inheritance from a module class beyond the Godot-forced shell level?
   New `virtual`/`abstract` members added to module classes? (Flag for justification.)
5. Module change with hardcoded game rules/data? (Belongs in the game's Info tables.)
6. Module API broken (signature change, rename)? → All consuming games must still
   compile; shared trunk means no silent breakage.
7. Module-touching commit pushed parent AND subtrees (Tools/subtree.ps1 push)?

## Gates

- `dotnet build` + `dotnet test` green; LayeringRuleTests; cross-module
  dependency-direction test.
- Module-touching commits: push parent repo AND every touched subtree
  (`Tools/subtree.ps1 push <name>`), per systems/subtrees.md.

## References

- In-game layers (R1–R9, Info/Service/Session/View/Controller): .agent/architecture.md
- Subtree workflow: .agent/systems/subtrees.md
- Per-module scope: each module's README
