# Rumi Character Kit — Design Specification

**Card:** #28 — Implementar Rumi
**Board:** HUNTR/X — Honmoon

## Goal
Deliver Rumi as a data-configured prototype: mobile close-range sword combat, direct damage, ground/aerial combos, and parry. Compose existing systems so later character cards can provide their own static loadouts.

## Evidence and intent
The card names sword, mobility, parry, direct damage, and aerial combat. The GDD requires distinct styles; neighboring cards characterize Mira around area control/defense and Zoey around ranged precision. Rumi's qualitative boundary is mobile, direct, close-range sword combat.

Constraints: shared movement/combo/damage/parry behavior remains owned by existing controllers; data assets contain static templates only; CharacterManager, respawn consumption, and switching belong to #31–32. Numeric tuning, physical input bindings, final art/animation, and device game feel remain unapproved/open in the GDD.

## Design
1. Add static CharacterMovementDefinition for the parameters currently serialized on HorizontalMovement2D, JumpController2D, and DashController2D. Validate finite values and the ranges currently enforced by those controllers.
2. Extend CharacterDefinition with display name and references to movement, CombatComboDefinition, and ParryDefinition. IsValid rejects a blank name, missing references, or invalid referenced definitions.
3. Add explicit validated configuration APIs to the existing controllers. CharacterDefinitionApplier2D validates all data and actor dependencies before applying any part of a definition; do not use reflection.
4. Create Rumi static assets and a prefab composition. Provide three ground and two aerial sword steps; this gives short chains within the existing minimum of two steps. Prototype numbers are integration fixtures only, clearly named provisional, and not final balance or playtest results.
5. Keep the prefab input-agnostic because the approved control map has no Parry action. Preserve CombatLab's empty authored host; automated PlayMode tests load that scene and add Rumi and target fixtures at runtime to validate the kit there.

## Error and lifecycle behavior
- Invalid definition or missing controller: one actionable error, no partial application.
- Reconfiguration while an attack chain or parry window is active is rejected.
- Existing component teardown remains responsible for hitbox, parry, and movement-override cleanup.
- Definition assets never store mutable runtime state.

## Verification
EditMode covers movement ranges and required character references. PlayMode covers atomic configuration, ground/aerial selection, direct damage, parry, movement/dash configuration, and teardown. Existing combat, movement, and checkpoint suites run for regression. SOLID/performance reviews are static; target-device profiling is future work. No playtest, platform input, co-op mode, final balance, or visual presentation is claimed.

## Ruling
Express Rumi's distinction qualitatively from this card and the neighboring character cards, treating prototype numbers as provisional. This uses approved roles without inventing final abilities or balance. Cost if wrong: a separate design iteration may be needed before #29–30 rely on the shared profile fields.
