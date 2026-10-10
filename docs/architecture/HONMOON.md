# Honmoon shared scene resource — card #47

`HonmoonController2D` is explicitly assigned as the shared resource of one scene/session. It has no static singleton, global actor lookup, persistence, network authority or implicit links to performance. Instantiate one resource owner and assign it to all consumers in that session.

`HonmoonDefinition` authors an initial value in 0..100 and crisis entry/exit thresholds. The prototype starts at 100, enters crisis at 20 or below and exits at 40 or above. Hysteresis avoids toggling repeatedly around one threshold. These values are provisional fixtures, not approved difficulty balancing.

The float `Value` is clamped to 0..100. `Band` uses floor thresholds: 100=Full, 80..99=Strong, 60..79=Guarded, 40..59=Low, 20..39=Critical and 0..19=Empty. A band is a presentation tier; `Empty` does not imply the resource value is exactly zero. `IsInCrisis` uses the separate authored hysteresis contract.

`TryChange(delta)` and `TrySetValue(value)` reject disabled/uninitialized owners, invalid definitions and nonfinite data. Clamped no-change returns false. Configuration initializes once; disable/re-enable preserves this runtime resource. State is committed before `ValueChanged(before, after)`, `BandChanged(band)` and `CrisisChanged(active)` notifications. Exceptions in one listener do not suppress remaining listeners, and recursive resource writes during publication are rejected.

`HonmoonFanAdapter2D` applies authored effects after accepted fan transitions: rescue completion grants 10 prototype resource units, accepted soul loss costs 0.25 units per actual soul lost. It subscribes to explicitly assigned rescue controllers and the encounter's dynamically created drainer; it does not infer effects from proximity or overwrite the fan's own state. Bounds and resource changes are validated, and subscriptions are removed on disable. This adapter supplies the resource side of cards #43/#44 without making those earlier controllers depend on card #47.

Authored resources are `Assets/Data/Honmoon/Honmoon_Prototype.asset`, `Resources/Honmoon/Honmoon_Prototype.prefab` and `Resources/Honmoon/HonmoonFanEncounter_Prototype.prefab`. The fan encounter is a copy composed with its actual registry, three rescue controllers and drainer bootstrap, plus the resource adapter; the card #44 prefab is preserved. Crisis effects and union are implemented separately in card #48.

Unity execution is deferred by owner instruction. Validation still needs threshold boundaries, nonfinite/overflow inputs, reentrant callbacks, enable/disable, actual drain/rescue notifications, duplicate-source deduplication and scene composition. Structural inspection is not runtime or DoD approval.
