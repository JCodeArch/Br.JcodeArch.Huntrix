# Character management — Unity assets and performance review

Date: 2026-10-10. Scope: cards #31 and #32; `CharacterManager2D`, `CharacterManagerState2D`, `CharacterSwitchController2D`, checkpoint ownership additions and `HuntrXPlayerSlot_Prototype.prefab`.

Status: **static review performed; Unity execution deferred by the owner**. No Editor was installed, no license was activated, and no test suite or mobile build was executed. This report does not establish Unity compilation, correct physics, frame budgets or completed card Definition of Done.

## Asset integrity

- New character scripts and their folder have `.meta` files. The inventory of `Assets` found no duplicate GUID, invalid GUID or orphan `.meta` file. Existing `.gitkeep` placeholders are excluded from import expectations.
- Rumi, Mira and Zoey prefab/data references resolve against committed project `.meta` GUIDs. Built-in Unity IDs are not treated as missing project assets.
- Player-slot prefab document IDs are unique; local component and transform references resolve. Its manager, switch-controller and checkpoint-flow script GUIDs resolve to the correct scripts. Its three roster GUIDs resolve to the character prefabs, and each referenced root GameObject fileID exists.
- The new folder falls within `HuntrX.Runtime.asmdef`; no separate assembly or package dependency was introduced. Existing EditMode and PlayMode assemblies reference the runtime assembly. JSON syntax of these assembly definitions was inspected, not compiled.
- Existing URP/template package references cannot be fully resolved by an `Assets`-only inventory; this is not evidence of missing scripts. Package import remains an Editor check.

The player-slot prefab is a composition aid: it includes manager/controller/flow and a StageStart child, but does not automatically initialize the manager. Scene integration must call `Configure` and `TrySpawn` with the initial prefab, flow and stage-start Transform. It is not connected to CombatLab and is not an automatically playable scene change.

## Lifecycle and ownership

Actors remain independent scene roots, preserving the existing `transform.root` ownership checks in damage, Zoey shots and Mira protection. They are not children of the player-slot prefab. The manager controls only instances it creates.

Switches explicitly clear horizontal input, buffered jump/coyote state and old grounded/wall contacts. This matters because the existing movement/jump controllers retain state across disable. The source and target keep individual health; world pose and velocities transfer on a permitted switch. New input and contact sensors must populate the destination again; this prototype does not supply them.

Inactive cached actors are deactivated. Existing attack, hitbox, parry, dash and protection teardown routines perform cleanup. Switch admission rejects active dash, melee attack, parry window, Mira field and Zoey recovery; switching cannot intentionally erase their active recovery. These are code observations requiring runtime confirmation.

Checkpoint additions expose attempt ownership and cancellation. The reviewed manager checks `IsAttemptInProgress` and `BoundActor` before switching, preventing rebinding into an externally cancelled attempt. Manager disable cancels the owned attempt, removes its subscription and deactivates/destroys the cache. If the flow is independently disabled or cancelled, coordinated lifecycle reset is required; a manager should not be assumed to resume that attempt automatically.

Accepted death deactivates owned actors before replacement. Death during a transition callback is captured and recovered through one deferred LateUpdate attempt rather than recursively respawning on the same call stack. Runtime order, third-party callback effects, failed candidate rollback and teardown during callbacks remain deferred validation cases.

## Performance observations and limits

There is no repeated Resources.Load or actor instantiation in the ordinary per-frame path. LateUpdate performs a small condition check and attempts recovery only for deferred death. Component lookups and prefab/profile validation occur during configuration or discrete spawn/switch requests rather than every physics frame.

The cache avoids re-instantiating a previously selected live character and preserves its health. The first selection of each character still incurs Instantiate/Awake/profile setup; respawn destroys the previous attempt's cache and creates a fresh actor. These transitions may cause CPU/GC/frame spikes. No profiling evidence is available, so no frame-time claim is made.

`GetInvocationList` allocates a subscriber snapshot when publishing ownership changes; defensive roster cloning allocates on Configure, and cache growth allocates as new prefab identities are selected. These are transition-bound allocations, not zero-allocation operation. Dictionary/roster iteration is acceptable for the supplied three-character prototype by code inspection; there is no hard three-entry limit or prewarming policy in the general API. Integrators must keep the allowed roster explicit and bounded. The prototype does not establish a mobile memory budget.

## Deferred validation

Run the real Unity 6000.6.4f1 import/compilation and EditMode/PlayMode checks later, including root ownership, health continuity, ground/air transitions, fresh contact/input routing, action/recovery rejection, checkpoint continuity, dead-target rejection, exactly-once respawn, cancelled-flow handling, exception/reentrant subscribers, independent slots and disable/destroy cleanup. Profile first switch and respawn on target hardware. Camera, physical controls, presentation, co-op and mobile performance are outside this static review and remain unapproved.
