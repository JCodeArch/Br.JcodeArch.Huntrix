# Zoey — card #30 prototype

`Resources/Characters/Zoey_Prototype` contains a separate Zoey character profile and precision-shot configuration. Shared Rumi movement, melee-combo and parry assets remain temporary fixtures, not final Zoey balance or animations. Art, sound, physical input mapping, character management (#31) and switching (#32) are outside this implementation.

## Contract

`ZoeyRangedAttack2D.TryFire(Vector2 aimDirection)` accepts a finite nonzero world-space direction, normalizes it and resolves an immediate hitscan. Horizontal, diagonal and vertical directions use the same path; aerial targets need no special faction or classification. The method returns true for an emitted shot (including a miss or blocked shot), false for invalid configuration/direction, disabled/dead actor, dash, melee, parry, cooldown, re-entry or saturated collision query.

`ZoeyRangedDefinition` references the existing `AttackDefinition` damage/knockback contract and defines range, cooldown and world-space muzzle offset. Prototype values: damage 12, range 12 world units, cooldown 0.3 seconds, muzzle offset (0, 0.1). These are provisional values rather than approved balance.

The 64-contact ray query includes triggers independently of global trigger-query settings. Self colliders are ignored; the closest hurtbox receives one shared `Hurtbox2D.ResolveHit` resolution and consumes the shot regardless of damage acceptance. Non-trigger geometry blocks the shot; unrelated triggers are skipped. Collision mask is explicitly serialized (all layers by default). Saturation rejects the shot conservatively instead of risking damage through an omitted blocker. Authored masks must retain solid blockers and desired hurtboxes. Origin placement must stay within the actor silhouette and clear of level geometry.

Shared resolution preserves faction rejection, dash invulnerability, parry and Mira protection. `HitConfirmed`, `HitParried` and `ShotFired(origin, end)` are presentation integration points; no new audio/visual asset or global hit-stop policy is introduced.

Cooldown uses scaled game time. Disable resets it; no projectile, coroutine or target reference survives disable. The component creates no external event subscriptions or asynchronous work; presentation subscribers own their subscription lifecycle. `TrySetDefinition` validates before mutation and rejects replacement during shot/cooldown. Immediate resolution reserves cooldown and re-entry state before damage/events.

## Verification limits

New EditMode/PlayMode tests are authored separately by QA. Execution in Unity, manual Combat Lab inspection, platform/device performance and independent review must be recorded before this card is marked complete. File presence or static checks are not Unity test evidence.
