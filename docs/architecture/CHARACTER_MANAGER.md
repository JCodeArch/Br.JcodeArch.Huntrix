# Character ownership, respawn and switching prototype

## Scope and status

[Card #31](https://trello.com/c/1cF6xp0x) requires management of the active character, states and respawn. [Card #32](https://trello.com/c/4qIS7hJ9) requires quick, safe switching during gameplay. These cards do not define physical bindings, switching costs, final health rules or a network model. The GDD remains authoritative for those open decisions.

The following is the minimal engineering contract for this prototype. Implemented code, static review and deferred Unity validation must be reported separately. The owner authorized parallel development and deferred Unity execution on 2026-10-10; this does not constitute a passed test, approved gameplay feel or completed Definition of Done.

## Ownership and API

`CharacterManager2D` is a scene component instantiated per player slot. It owns only runtime characters that it creates from supplied prefabs; it never destroys or deactivates prefab assets or arbitrary external actors. It is not a singleton and has no network, account, save, camera or device-input responsibility.

The minimal API agreed with gameplay is:

| Member | Contract |
| --- | --- |
| `Configure(prefab, flow, stageStart, startId)` | Validate dependencies and configure the stage start before spawning. Reject changes during a live attempt. |
| `TrySpawn()` | Create the initial character, bind its damage receiver to the configured checkpoint flow and start the attempt. Reject duplicate spawning or invalid configuration. |
| `ActiveCharacter` | The live runtime `DamageReceiver2D` controlled by this slot, or null when unavailable. |
| `State` | `Uninitialized`, `Alive`, `Respawning` or `Disabled`; ownership state rather than a duplicate of movement/combat state machines. |
| `ActiveCharacterChanged(previous, current)` | Notify integrations after ownership has been committed. Listeners cannot cause a partially completed transition. |
| `TrySwitch(prefab)` | Manager-owned lifecycle boundary consumed by the card #32 switching adapter. Validate and commit a replacement atomically; return false without changing ownership on rejection. |

The card #32 adapter owns the permitted roster and request routing. It receives logical switch requests rather than binding keyboard, gamepad or touch controls. A roster is bounded to the supplied HUNTR/X prototypes and contains no null or duplicate entries; unavailable identities are rejected. No switch cooldown, resource cost, unlock system or automatic AI companion is introduced.

## Individual character state and safe switching

The prototype retains each managed character's own runtime instance while it is inactive, indexed by its prefab identity. Health belongs to that instance and survives switching; switching does not heal, copy percentages, pool team health or revive a dead character. This is a technical continuity rule for the prototype, not final health balancing. Only the manager's active instance participates in gameplay; inactive instances are deactivated, and their existing component teardown removes hitboxes, parry and protection registrations.

A switch validates the enabled manager, live current actor, permitted target prefab and complete target composition before deactivating the current instance. The request is rejected during an ownership transition, death/respawn, dash, active melee attack, parry window, Mira protection field or Zoey attack recovery. These guards prevent switching from silently cancelling an active combat effect or bypassing its recovery. No new gameplay timer is added. A dead cached target is unavailable until the next attempt reset.

The destination uses the source world position and rotation. Its Rigidbody pose and velocity are transferred consistently so an airborne switch does not teleport to an earlier cached position or reset falling motion. Clear historical horizontal input on both source and destination through `SetMovementInput(Vector2.zero)` because the current movement component retains input across disable. Wall/ground sensing and device ownership are not copied implicitly; the input integration must route subsequent logical input to the new active instance. Facing/sensor state requires explicit integration rather than assuming cached state is current.

Binding the target to the checkpoint flow occurs in the same attempt. Switching does not call `ConfigureStart` or `StartAttempt`, reset the selected checkpoint or increment the attempt counter. Failed validation leaves the old character active and bound. If creating a candidate fails, clean up that candidate without losing the source. Nested listener callbacks cannot initiate another transition before the first commits.

## Checkpoint and respawn lifecycle

The existing `CheckpointAttemptFlow2D` owns checkpoint selection, attempt numbers and accepted death. Its `RespawnRequested` contains the ended attempt number and resolved checkpoint pose. The manager subscribes exactly once while enabled and consumes only a request for its current bound actor and current ended attempt. Stale or duplicate requests and reentrant transitions are rejected.

On accepted death, the manager enters `Respawning`, stops gameplay participation of owned instances and creates a fresh instance of the active prefab at the request pose. Fresh instantiation initializes health and controller state without adding an unrestricted health-reset API to `DamageReceiver2D`. The next attempt starts only after receiver validation and successful `BindActor`; checkpoint selection remains intact. Failure is observable and must not be presented as a successful respawn.

An attempt reset disposes this slot's inactive cache as well as its old active actor. This prevents earlier dead or damaged cached instances from leaking into a new attempt. Other slots' instances are never affected. Destruction is deferred by Unity, so an old instance is deactivated synchronously before `Destroy` to prevent a frame of duplicate gameplay.

On manager disable/destruction, unsubscribe from the flow, clear active ownership and deactivate/destroy every owned runtime instance. Existing combat components perform their own cleanup. Re-enable requires a valid lifecycle initialization; do not silently reuse stale receiver subscriptions. The flow must not retain a dead/destroyed actor after manager teardown; its existing lifecycle or a narrow explicit unbind contract must be used and verified.

## Solo and cooperative boundary

Solo uses one manager. The architecture permits up to three independent managers with separate input ownership and checkpoint flows. This preserves the GDD requirement without claiming an implemented cooperative mode. Local/online/both, shared camera, team death, checkpoint coordination, duplicate character selection across slots and network authority remain unresolved. No global active character or shared singleton may replace per-slot ownership.

## Dependencies and verification still required

Dependencies are the actual Rumi/Mira/Zoey prefabs, `CharacterDefinitionApplier2D`, dynamic Rigidbody, valid live `DamageReceiver2D`, and the card #27 checkpoint flow. Selection uses the card #31 lifecycle API; it does not mutate definitions or recreate checkpoint logic.

Deferred Unity checks must cover invalid configuration without partial ownership, initial spawn, exactly-once death/respawn, checkpoint preservation, fallback destination, duplicate/stale requests, failed candidate rollback, idle and airborne switches, busy-state rejection, per-character health continuity, dead target rejection, cache reset, listener reentrancy, disable/destroy cleanup, and two independent slots. Tests for physical input, camera and real network play belong to their integrations. Static review cannot establish physics correctness, frame timing, mobile performance or game feel.
