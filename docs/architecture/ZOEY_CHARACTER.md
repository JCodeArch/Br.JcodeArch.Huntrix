# Zoey — card #30 prototype

`Resources/Characters/Zoey_Prototype` contains a separate Zoey character profile and precision-shot configuration. Shared Rumi movement, melee-combo and parry assets remain temporary fixtures, not final Zoey balance or animations. Art, sound, physical input mapping, character management (#31) and switching (#32) are outside this implementation.

## Contract

`ZoeyRangedAttack2D.TryFire(Vector2 aimDirection)` accepts a finite nonzero world-space direction, normalizes it and resolves an immediate hitscan. Horizontal, diagonal and vertical directions use the same path; aerial targets need no special faction or classification. The method returns true for an emitted shot (including a miss or blocked shot), false for invalid configuration/direction, disabled/dead actor (including a disabled `DamageReceiver2D`), dash, melee, parry, cooldown, re-entry or saturated collision query.

`ZoeyRangedDefinition` references the existing `AttackDefinition` damage/knockback contract and defines range, cooldown and world-space muzzle offset. Prototype values: damage 12, range 12 world units, cooldown 0.3 seconds, muzzle offset (0, 0.1). These are provisional values rather than approved balance.

The 64-contact ray query includes triggers independently of global trigger-query settings. Self colliders are ignored; the closest hurtbox receives one shared `Hurtbox2D.ResolveHit` resolution and consumes the shot regardless of damage acceptance. Non-trigger geometry blocks the shot; unrelated triggers are skipped. Collision mask is explicitly serialized (all layers by default). Saturation rejects the shot conservatively instead of risking damage through an omitted blocker. Authored masks must retain solid blockers and desired hurtboxes. Origin placement must stay within the actor silhouette and clear of level geometry.

Shared resolution preserves faction rejection, dash invulnerability, parry and Mira protection. `HitConfirmed`, `HitParried` and `ShotFired(origin, end)` are presentation integration points. Each subscriber is isolated: exceptions are logged and later subscribers still receive the committed event; they do not change the accepted shot result, damage/parry resolution or reserved cooldown. Listener snapshots refresh on subscription changes, preserving multicast snapshot behavior without a per-shot invocation-list allocation; no new audio/visual asset or global hit-stop policy is introduced.

Cooldown uses scaled game time. Disable resets it; no projectile, coroutine or target reference survives disable. The component creates no external event subscriptions or asynchronous work; presentation subscribers own their subscription lifecycle. `TrySetDefinition` validates before mutation and rejects replacement during shot/cooldown. Immediate resolution reserves cooldown and re-entry state before damage/events.

## Verification limits

New EditMode/PlayMode tests are authored separately by QA. Execution in Unity, manual inspection in a populated gameplay scene, platform/device performance and independent review must be recorded before this card is marked complete and full runtime acceptance is claimed. The current `CombatLab` is an empty bootstrap host, so use `VerticalSlice` with Zoey selected for the integrated character walkthrough; an empty lab is not character evidence. File presence or static checks are not Unity test evidence.


## Current implementation verification

The focused completion review starts from develop `d92d73b`. Ranged definition, character profile and prefab references resolve; VerticalSlice's third roster entry is the Zoey prefab and its input adapter routes Attack to `TryFire`, using movement direction or the current facing fallback. Character management prevents switching while the ranged cooldown is active. Shared movement/melee/parry remain prototype fixtures, not a new Zoey moveset.

The follow-up guards the disabled combat receiver before emitting a shot or reserving cooldown. Five additional PlayMode regressions cover that guard, finite extreme vertical aim, atomic definition replacement, event subscription snapshots, and dash/melee/parry rejection. These tests are authored, not executed in this environment. A licensed Unity 6000.6.4f1 Editor is required for import/compile and execution.

For final verification, run `scripts/run-unity-tests.sh` on the exact clean integrated commit and retain both suites' XML/logs/provenance. Then open VerticalSlice, select Zoey via character switching, and exercise horizontal/upward/diagonal attacks against grounded and airborne targets, solid blockers, cooldown and character switching. Record the commit, scene, controls/profile used, observed damage and any failures. Art/audio polish and target-device performance remain separate acceptance gates; authored scenarios do not establish those results.
