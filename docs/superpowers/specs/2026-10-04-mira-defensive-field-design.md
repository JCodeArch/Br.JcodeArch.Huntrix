# Card 29 — Mira Defensive Field Design

## Status and source of truth

This design applies only to Trello card #29, “Implementar Mira” (https://trello.com/c/cnOzvJRn/29-29-implementar-mira). Its approved direction is a temporary defensive area that protects allies in the field. The Trello card currently says “Controle de área, defesa e proteção de fãs.” The user approved the defensive-area direction on 2026-10-04.

The GDD closes the principle of protecting characters, allies, and objectives, but leaves protection rules open. It also leaves fan states, rescue interaction, and risk/reward open for cards #42–50. This card establishes a reusable combat-protection contract and tests it with generic allied receivers; it does not implement fan lifecycle or rescue rules. The later fan cards must explicitly compose or adapt this contract and verify that a protected fan does not lose health before claiming fan protection is integrated.

## Goal

Compose a Mira gameplay prototype whose distinctive action creates a temporary circular safe area. Combat receivers with the protection component inside the area do not take damage or knockback from accepted enemy attacks while the field is active. Leaving the area or ending the field immediately ends this protection.

## Architecture

- Reuse the existing movement, jump, dash, attack, combo, parry, and CharacterDefinition contracts from card #28. Mira's current movement and basic combat references are provisional shared fixtures; this card does not claim unique basic attack choreography or final tuning.
- Add a static MiraProtectionDefinition ScriptableObject containing a finite positive duration and radius. It stores authored configuration only.
- Add a runtime MiraProtectionField2D controller that exposes TryActivate(), owns the active timer, and enables/disables a child circular trigger using the definition's radius. It reads no device input and has no cooldown, resource cost, or fan/progression dependency.
- Add a DamageProtection2D opt-in receiver marker that tracks active field sources and exposes whether its owner is currently protected. Only receivers carrying this component are eligible; this explicit composition marks protected allies without inferring allegiance from the current two-value combat faction enum. Multiple overlapping fields are supported; one field ending cannot clear another field's protection.
- The Mira prototype is an authored Resources prefab with its own CharacterDefinitionApplier2D profile and a separately assigned MiraProtectionDefinition on MiraProtectionField2D; field setup is verified on the actual prefab, not only a synthetic component fixture. On TryActivate, MiraProtectionField2D immediately queries overlapping receivers so protection is active for targets already in range before the next physics callback. While active, trigger enter/stay/exit maintains membership without frame-by-frame scans. A field tracks overlapping collider instances per DamageProtection2D receiver, so one receiver with multiple colliders remains protected until its last collider leaves. Mira herself is eligible when her receiver has the opt-in component. The child trigger belongs to the field owner's Rigidbody2D, consistent with the project's 2D combat composition.
- DamageReceiver2D checks protection after existing validity, self/faction, and dash-invulnerability checks, and after the existing parry resolution. A protected hit returns a distinct Protected result before health or knockback changes. AttackHitbox2D reserves a Protected target once for the rest of the current hitbox activation without emitting an impact or parry event. If the field expires or the target leaves its radius during that same attack activation, damage does not begin mid-swing; a later combo step/new activation evaluates the target again. If the existing parry resolves first, its current parry event behavior remains unchanged.
- The field does no per-frame overlap scan. Collection updates occur on trigger transitions and lifecycle changes; contact deduplication follows the existing hitbox collection behavior.

## Observable behavior and error handling

1. A valid, enabled Mira field activates once; a second activation while active is rejected without resetting its duration. Existing overlaps are registered synchronously during activation. The active timer advances in scaled fixed time and ends on the first fixed tick that reaches/exceeds the authored duration.
2. Invalid/missing definition, trigger composition, or owner receiver rejects activation without partially enabling the field.
3. A contact resolved as Protected causes no health loss, death event, knockback, or impact/parry event. Existing parry has higher precedence and retains its current result/event behavior.
4. Same-faction, self, invalid, dead, and dash-invulnerable contact behavior remains unchanged. Existing parry retains precedence over the field.
5. Receivers outside the circle and receivers entering after field expiry follow the existing damage path. After a receiver exits or the field expires, a new hitbox activation follows the existing damage path; an activation that already resolved that receiver as Protected remains deduplicated until the activation ends. Receivers without the opt-in component are never protected.
6. Disabling/destroying Mira or its field removes every registration and disables the trigger. Disabling/destroying a protected receiver clears its registrations from both the field and the receiver so no stale protection remains. One of several colliders exiting does not clear protection while another collider for the receiver remains inside.
7. A generic allied receiver with DamageProtection2D can be protected; a receiver without it cannot. No fan entity exists yet. Future fan implementation must compose/adapt the opt-in contract and test that a protected fan avoids damage before claiming integration; fan-specific faction, health, rescue, state, reward, or Honmoon behavior is deferred to #42–50.

## Test and review evidence

- EditMode: definition rejects missing, zero, negative, NaN, and infinite radius/duration; valid values persist; prefab composition and definition references are valid.
- PlayMode: activation and scaled fixed-time duration; immediate protection for targets already inside before physics advances; inside versus outside eligible targets; targets without the opt-in component; one receiver with multiple colliders remains protected until all exit; zero health/knockback/impact events for contacts resolved as Protected; new attacks after exit/expiry damage normally while the same attack activation remains deduplicated; multiple overlapping fields; existing parry precedence and event behavior; disable/destruction cleanup; actual Mira prefab composition; invalid setup; no change to Rumi's current profile or hit flow.
- Run the full Unity EditMode and PlayMode regression suites on Unity 6.6.4f1.
- SOLID review checks single ownership of field timing, target registration, and damage acceptance, with no fan-specific types in the combat layer.
- Performance review checks trigger-based membership without per-frame area scans and avoids new allocations on repeated contact handling. Mobile device budgets remain unmeasured and open.
- QA review covers only the implemented gameplay surface. No new art, animation, audio, dialogue, UI, personal data, account, network, or physical input is introduced. This limited scope does not claim global age, accessibility, Child Safety, classification, or playtest approval.

## Boundaries

Out of scope: fan records/states/rescue, Honmoon, Special/resource costs, enemy movement/control effects, cooldowns, physical bindings, character selection/switching/respawn (#31–32), co-op/network rules, final movement/attack/field tuning, and final art/animation. Future fan work must explicitly compose or adapt the protection contract before claiming that actual fans are protected.
