# Cards #31 and #32 — independent static review

**Date:** 2026-10-10. **Scope:** working-tree implementation of `CharacterManager2D`, `CharacterManagerState2D`, `CharacterSwitchController2D`, the checkpoint-flow lifecycle additions, and `HuntrXPlayerSlot_Prototype`.

**Sources:** [card #31](https://trello.com/c/1cF6xp0x) (active character, states and respawn), [card #32](https://trello.com/c/4qIS7hJ9) (quick, safe switching), `GDD_MASTER.md`, `PROJECT_RULES.md`, `CHARACTER_MANAGER.md`, `CHARACTER_SWITCHING.md` and the deferred-validation decision. This review does not establish runtime acceptance or final gameplay rules.

## Verdict

**Independent static review / SOLID: APPROVED for the documented prototype.** No unresolved blocking finding was identified in the reviewed source and asset snapshot after the corrections below. Publication/integration may use **Implemented — validation pending** under the owner's deferred-validation decision. This is not completed runtime QA, full Definition of Done, qualified cooperative play or release approval.

## Responsibilities and ownership

- The manager owns runtime actor creation, its per-slot cache, the current actor, checkpoint binding and respawn lifecycle. It is not a singleton and does not own physical input, camera, networking, save data or UI.
- The switching controller validates/copies an ordered roster and delegates lifecycle changes to the manager. `ActiveIndex` derives from `ActivePrefab`, avoiding a second mutable identity state after respawn or external selection.
- Individual health remains on persistent cached receivers. Switching does not recreate a healthy actor or heal the cache. Accepted death creates a fresh attempt and disposes the previous attempt's cache.
- Separate actor roots preserve the existing combat/protection `transform.root` ownership assumptions. Inactive cache entries are synchronously deactivated; destruction remains deferred by Unity.
- Changes to checkpoint flow expose current attempt/bound actor and provide cancellation without resetting selected checkpoint or attempt number. Each manager must use a separate flow.

## Findings corrected during review

| Finding | Correction verified in source |
| --- | --- |
| A disabled/re-enabled or cancelled checkpoint flow could leave a live manager accepting switches outside an attempt. | `TrySwitch` requires an active attempt and `flow.BoundActor == ActiveCharacter`. External flow cancellation requires lifecycle reset rather than silently continuing gameplay ownership. |
| Respawn failure left a dead actor/cache active. | `HandleRespawn` clears transient input and deactivates owned cache before attempting recovery; failure preserves an explicit retryable `Respawning` state. |
| A death caused by an `ActiveCharacterChanged` listener was discarded during the transition guard. | A valid death request is captured while transitioning. `LateUpdate` performs a deferred recovery, avoiding recursive event-driven respawn chains. |
| Candidate validation checked receiver health but could accept invalid combat composition. | `ValidRuntimeActor` validates dynamic Rigidbody, root ownership, required enabled components, hitbox configuration, hurtbox receiver and character profile; authored ranged/protection definitions are also checked. Cached targets are revalidated after activation. |
| A cached missing Rigidbody could be dereferenced before rejection. | Source validation precedes Rigidbody access; candidate validation occurs immediately after activation, before velocity assignment or checkpoint binding. |
| A destroyed stage-start transform prevented recovery at a valid selected checkpoint. | Pending respawn uses its validated request pose; stage-start availability is required only for an initial spawn. A pending request from another attempt is rejected. |

These corrections were inspected statically; none has been exercised in Unity in this round.

## State and integration checks

Admission rejects disabled/dead sources, nested transitions, inactive or wrong checkpoint ownership, invalid targets and active dash/melee/parry/Mira field/Zoey recovery. A failed target validation does not deactivate the source or rebind its attempt. Successful switching transfers position, rotation and Rigidbody velocities, clears stale logical movement/jump/contact state, then commits identity before publishing the change event. Switching does not increment attempts or reset checkpoint selection.

Disable unsubscribes from the flow, cancels the owned attempt, clears owned actors and publishes loss of ownership. Listener exceptions are isolated. The switching controller releases its reentry guard with `finally` and does not mutate actor internals or checkpoint counters.

Static asset checks found all new scripts/prefabs paired with `.meta` files, no duplicate GUIDs in `Assets`, no unresolved GUIDs in the slot prefab and no unresolved local prefab fileID references. Its manager/controller/flow references and three-character roster are consistent. The `StageStart` child is a composition aid; no auto-spawn, real input binding or Combat Lab integration is claimed.

## Remaining validation and limits

**Unity compilation, asset import, EditMode/PlayMode, scene inspection and device measurements: NOT EXECUTED — deferred by the owner.** No new automated management/switching suite was added in this round; scenarios are recorded in `DEFERRED_VALIDATION.md`. Historical tests do not validate these changes.

Runtime verification must cover initial spawn, invalid candidates and rollback, cached per-character health, repeated/aerial switches, action/recovery rejection, checkpoint preservation, death/respawn, listener-induced death, failed recovery retry, stale requests, disable/re-enable cleanup and independent slots. Physics pose synchronization, Awake/OnEnable order, ground/wall sensor refresh and input/camera routing need actual integration checks.

Normal accepted death recovery occurs inside the checkpoint/death notification chain. `DamageReceiver2D` snapshots its death listeners and isolates exceptions; the manager deactivates the old instance and schedules deferred destruction. Subsequent death/impact observers therefore refer to the old damaged receiver, which can already be inactive, while `ActiveCharacter` can be the replacement. Actual callback/physics timing and presentation consumers must be checked in Unity; this review does not claim that behavior has been exercised.

Static component checks are not comprehensive validation of arbitrary third-party prefabs or dynamically mutated controller internals. The supplied authored Rumi/Mira/Zoey compositions remain the prototype integration fixtures; full protection-collider behavior, physics and gameplay feel require Unity execution. External systems must not share/rebind the slot's flow or deactivate it independently without resetting the manager lifecycle.

The cache has no per-frame enumeration outside pending-death handling and performs no per-frame polling of controllers. This observation is not a mobile performance measurement. Up to three independent slot compositions are structurally possible, but cooperative input, camera, team rules and networking remain separate gates.
