# Mira Character Prototype

## Scope and sources

[Card #29 — Implementar Mira](https://trello.com/c/cnOzvJRn/29-29-implementar-mira) implements the temporary defensive-area direction approved on 2026-10-04. The [design](../superpowers/specs/2026-10-04-mira-defensive-field-design.md) and [implementation plan](../superpowers/plans/2026-10-04-mira-defensive-field.md) define its acceptance boundaries. The [GDD](../GDD_MASTER.md) retains open product decisions; the [Definition of Done](../governance/DEFINITION_OF_DONE.md) governs final reviews and integration.

## Authored composition and data

`Assets/Resources/Characters/Mira_Prototype.prefab` has its own `CharacterDefinitionApplier2D` profile, the shared movement/jump/dash/attack/combo/parry components, a dynamic root `Rigidbody2D`, `DamageReceiver2D`, hurtbox, root `DamageProtection2D`, and `MiraProtectionField2D`. A separate disabled child `CircleCollider2D` is a trigger centered at local zero, with zero collider offset and the owner's Rigidbody. `CombatLab` remains an empty host; tests instantiate the actual Resources prefab and generic targets there.

`Assets/Data/Characters/Mira/Mira.asset` owns a distinct stable character identity and display name. Movement and basic attack/combo/parry references reuse Rumi's existing assets as **provisional shared integration fixtures**. This does not establish unique Mira basic attack choreography or final tuning. Mira's distinct action is the defensive field.

`Mira_Protection_Prototype.asset` is assigned separately to the field controller. `MiraProtectionDefinition` holds a stable authoring ID, finite positive `DurationSeconds` and `Radius`; it contains no active timer, targets, resource, or cooldown state. The authored duration of 2 seconds and radius of 2 Unity units are provisional integration values. Invalid/missing data rejects activation.

## Runtime contract and lifecycle

`MiraProtectionField2D.TryActivate()` validates the enabled, live, configured root owner, valid definition, and centered child trigger composition before enabling the circle. Activation returns false while already active and does not refresh its lifetime. An inactive/missing/invalid dependency rejects activation without partially enabling the field.

Activation sets the authored radius, performs one explicit `Physics2D.SyncTransforms()` and overlap-circle query, and synchronously registers eligible receivers already inside, including Mira when opted in. Subsequent trigger enter/stay/exit callbacks maintain membership. `FixedUpdate` advances the timer in scaled fixed time and ends the field on the first fixed tick reaching its duration. There is no per-frame area scan, physical input binding, cooldown, or resource cost.

`DamageProtection2D` is the explicit opt-in contract. A target must compose it with a valid live `DamageReceiver2D`; the field does not infer eligibility from the two-value combat faction enum. Existing faction rules still decide whether an incoming attack is accepted. Receivers without the marker remain damageable. Future integration must attach the marker only to intended eligible targets.

Membership records each source and overlapping collider. One collider exiting cannot cancel another collider for the same receiver; one field ending cannot cancel another active field. `IsProtected` validates registered pairs on demand using `Collider2D.Distance` with current attached-body Transform positions/rotations, retaining authored collider offsets. Movement before the first physics simulation therefore immediately stops protection outside the area. This checks existing pairs rather than discovering new overlaps; new entry is handled by physics callbacks. Disabled/inactive colliders and sources cannot grant protection.

Field disable/destruction disables its trigger and removes its registrations. Receiver marker disable/destruction clears both sides of its registrations. These lifecycle changes cannot leave stale active protection.

## Damage and event behavior

`DamageReceiver2D` retains validity, alive, self/faction, and dash-invulnerability rejection before existing parry resolution. Parry retains precedence and its event behavior. A subsequent protected contact returns `CombatContactResult.Protected` before health or knockback changes, causing no health loss, death event, accepted-hit impact, or parry event.

`AttackHitbox2D` reserves a Protected receiver for the rest of that hitbox activation without publishing `AcceptedHit` or `ParriedHit`. Exit/expiry during the same activation cannot begin damage mid-swing. A later activation or combo step evaluates current protection again and follows the existing damage path if protection ended. This reservation is attack activation state, separate from field membership.

## Fan integration and remaining scope

Generic allied combat receivers demonstrate the contract; no fan entity exists in this card. Cards #42–50 must explicitly compose/adapt this opt-in protection contract and verify that an actual protected fan avoids health loss before claiming fan protection is integrated. Fan faction, health, states, rescue, rewards, risk/reward, and Honmoon behavior remain open.

Other deferred work includes Special/costs, cooldowns, enemy control/movement effects, physical input, selection/switching/respawn (#31–32), cooperation/network rules, final balance, and character art/animation/audio/UI. No new personal data, account, network, dialogue, interface, monetization, effects, or physical controls are added. Art/voice/captions/flash review has no new content to assess here; this is a limited scope check, with age/accessibility/Child Safety and options still requiring their own cards #85–91 and QA #102–103. No global compliance or audience approval is claimed.

## Verification and performance limits

EditMode tests cover finite-positive definition boundaries, IDs, shared references, and the actual prefab composition. PlayMode tests cover invalid activation, synchronous seed protection, immediate pose/rotation/offset checks before physics advances, entry/exit, multiple colliders/sources, scaled expiry, activation rejection, damage/knockback/event silence, parry/dash precedence, attack reservation across exit/expiry, lifecycle teardown, actual Mira integration, and unchanged Rumi behavior. Tests call logical APIs; they do not establish game feel or physical control accessibility.

Full Unity 6.6.4f1 regressions passed: **EditMode 65/65; PlayMode 245/245**, zero failed/skipped/inconclusive and zero C# compiler errors/warnings. Completed XML/log paths are recorded in the Task 4 report under `.superpowers/sdd/2026-10-04-mira-defensive-field/`. The independent SOLID, Performance and QA verdicts are recorded in [the final review record](../reviews/2026-10-05-card-29-mira.md); final branch and Trello integration remain gates until verified.

Antigravity review was not run because external transfer of project source/assets/results was rejected by the automatic approval review. This card's final branch/Trello integration remains an explicit gate until verified.

The implementation has one activation overlap query and trigger-driven collection updates. On-demand protection checks scale with registered source/collider pairs; a new source allocates a collider set and collections can grow. No mobile CPU/allocation budget or minimum-device result has been measured. Device measurements remain a gate for later platform work; current automated correctness results do not establish mobile performance targets.
