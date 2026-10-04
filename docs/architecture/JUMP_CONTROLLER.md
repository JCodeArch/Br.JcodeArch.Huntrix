# Jump Controller 2D

## Contract

`HuntrX.Gameplay.Jump.JumpController2D` receives logical jump events through `PressJump()` and `ReleaseJump()`. It reads no physical device or input binding. A future character or ground-sensing component supplies contact through `SetGrounded(bool)`; this card intentionally does not choose floor layers, collider footprints, tags, or slope thresholds.

A buffered request starts when grounded or while its coyote timer remains active. The controller sets only `Rigidbody2D.linearVelocityY`, preserving horizontal motion. It assigns a configured positive `gravityScale` to the body's own Rigidbody2D and leaves global `Physics2D.gravity` unchanged. Unity describes `gravityScale` as the per-body proportion of global gravity ([Unity API reference](https://docs.unity3d.com/6000.0/ScriptReference/Rigidbody2D-gravityScale.html)); `linearVelocityY` modifies the vertical component without changing the horizontal component ([Unity API reference](https://docs.unity3d.com/6000.0/ScriptReference/Rigidbody2D-linearVelocityY.html)).

If Jump is released while the current jump is rising, the component applies the configured cut multiplier once. Releasing before a buffered jump starts cuts that jump at launch; holding the input leaves its initial upward speed uncut. Coyote/buffer timers and jump processing run in `FixedUpdate`.

## Configuration and deferred integration

`jumpVelocity`, per-body `gravityScale`, `coyoteTime`, and `jumpBufferTime` must be finite and positive. `jumpCutMultiplier` must be finite and between 0 and 1. Invalid settings produce one diagnostic and the component does not initiate a jump. No product defaults have been approved; PlayMode test values are synthetic fixtures only.

This component is not wired to a player or sensor yet. The future character/input layer must translate physical controls to `PressJump`/`ReleaseJump` and report ground contact with `SetGrounded`. Final jump tuning, ground-sensing geometry, animation, double-jump rules, wall jump, and air control stay with their respective backlog cards. This card does not modify the horizontal movement controller or global physics settings.

## QA

`JumpController2DPlayModeTests` covers ground jump, horizontal velocity preservation, coyote acceptance/expiry, buffered press on landing/expiry, early-release cut, held jump, release before buffered launch, per-body gravity, and invalid configuration diagnostics. Passing these contract tests does not claim final game-feel values or character integration.