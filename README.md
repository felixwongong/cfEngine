# cfEngine

## Module standard

**Layer 1** of the cf* module standard — the app-generic base.

- **Holds:** app-generic pure C# useful to ANY app, game or not (Rx, Res/Optional/validation, IService/Domain/ServiceLocator, UserDataManager, pooling, GridMap, serialize).
- **Must NOT hold:** game concepts (items, missions, HP); Godot; heavy deps (≈ Newtonsoft only).

Dependencies point **DOWN only**: `game → cfGodotEngine → cfGameEngine → cfEngine`. cfEngine references nothing above it and is strictly Godot-free (enforced by the cross-module dependency-direction test). See [`.agent/architecture.md`](.agent/architecture.md) and [`skills/cf-coding-standard/SKILL.md`](skills/cf-coding-standard/SKILL.md).
