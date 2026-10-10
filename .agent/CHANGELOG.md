# CHANGELOG

2026-10-10 — Holds the canonical `cf-coding-standard` skill (run `framework-extraction` VS6, RELEASE): `skills/cf-coding-standard/SKILL.md` is now the single source of truth for module/repo placement and authoring rules across the cf* stack (each game repo holds a thin stub whose body points here). The same slice adds the generalized in-game architecture standard (`Modules/cfEngine/.agent/architecture.md` — module standard + litmus + layers + R1–R9 + decision tree + gates) and re-exports the reusable layering-test fixture as `skills/cf-coding-standard/LayeringRuleTests.cs.template` (shipped with a `.cs.template` suffix so cfEngine's `**/*.cs` compile glob does not pick it up). The module README states cfEngine's Layer-1 row + dependency direction.

2026-07-28 — Add `service/validation/`: intent-gate standard for server-style validation of gameplay/user actions (`IIntent`, `ValidationError`, `ValidationMode`, `IntentRule`, `IIntentGate`, `IntentGate`, `NullIntentGate`). Async-shaped from day one for a future network transport. Reference usage: CatSweeper `StageCatchGate`.

2026-07-17 — Add `.agent/` docs for cfEngine: README, project.md, AGENTS.md.
