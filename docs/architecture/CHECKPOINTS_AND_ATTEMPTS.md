# Checkpoints and attempt flow

**Related cards:** [#27 — Implement checkpoints](https://trello.com/c/k43sA6MK/27-27-implementar-checkpoints), [#31 — Implement CharacterManager](https://trello.com/c/1cF6xp0x/31-31-implementar-charactermanager).

## Runtime contract

- `CheckpointAnchor2D` gives a scene checkpoint a stable ID and a validated respawn pose. It uses its own transform unless a separate respawn point is assigned.
- `CheckpointAttemptFlow2D` is scene-local. The scene configures a stage-start ID/pose, binds the living actor, and explicitly starts an attempt. Checkpoints activate through an explicit call; the latest valid activation becomes the selected destination.
- A failed activation or actor bind leaves the current selection/binding unchanged. Disabled anchors, inactive destinations, empty IDs, destroyed references, invalid transforms, unconfigured starts, and dead/unconfigured actors are rejected.
- Rebinding a living actor during an active attempt transfers the death subscription and keeps the current attempt number. The old actor is unsubscribed. `OnDisable` removes the subscription and cancels the active attempt; reactivation requires a new binding and explicit `StartAttempt`, which increments the monotonic attempt number.
- `DamageReceiver2D.Died` fires once when accepted damage changes health from positive to zero, after accepted knockback is applied. One subscriber throwing is logged and does not prevent delivery to later subscribers.
- A death ends the active attempt once. `RespawnRequested` carries the attempt number, selected checkpoint ID, and a copied position/rotation. A missing or destroyed selected checkpoint falls back to the valid stage start. With no valid destination, the flow logs one diagnostic and sends no request.
- Event subscriber exceptions are isolated for `Died`, `CheckpointActivated`, and `RespawnRequested`. Event dispatch allocates a delegate snapshot only when the event is published; there is no per-frame polling or lookup.

## Ownership and persistence

This card owns checkpoint selection and the attempt request. It deliberately does not move or heal the actor, restore movement/combat state, reset enemies, or change scenes. Card #31 owns the CharacterManager integration that consumes the request and applies the character recovery contract.

`CheckpointActivated` publishes the checkpoint ID as a seam for future persistence. The selection currently exists only in memory. There is no file/cloud save, serialization, load/resume, or durable autosave in this runtime. Until a save consumer is implemented, restarting the application does not restore this checkpoint. See [`SAVE_AND_PROFILES.md`](SAVE_AND_PROFILES.md).

## Validation boundary

The EditMode and PlayMode suites cover identity/pose validation, explicit selection, fallback, request payload, single death/request, rebind and disable lifecycle, invalid destinations, destroyed references, repeated attempts, knockback-before-death ordering, and exception isolation. Unity 6000.6.4f1 full-suite evidence is recorded on the Trello card. Performance review is static: event snapshot allocations are rare and no target device or frame/GC benchmark has been approved or measured. Privacy, Child Safety, and accessibility do not apply to this local runtime API; screen feedback, input, and child-facing UI are outside this card.