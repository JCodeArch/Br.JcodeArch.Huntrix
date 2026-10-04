# Jump Controller 2D

## Contract

`HuntrX.Gameplay.Jump.JumpController2D` receives logical jump events through `PressJump()` and `ReleaseJump()`. It reads no physical device or input binding. A future character or ground-sensing component supplies contact through `SetGrounded(bool)`; this card intentionally does not choose floor layers, collider footprints, tags, or slope thresholds.

A buffered request starts when grounded or while its coyote timer remains active. During a valid airborne wall contact, the buffered request starts a wall jump before coyote time is considered. Grounded jumping keeps priority. Ground jumps and coyote jumps set only `Rigidbody2D.linearVelocityY`, preserving horizontal motion. A wall jump replaces both velocity components: horizontal velocity points away from the wall and vertical velocity uses the same configured `jumpVelocity`. It requires a separate positive finite `wallJumpHorizontalSpeed` setting. Unity describes `gravityScale` as the per-body proportion of global gravity ([Unity API reference](https://docs.unity3d.com/6000.0/ScriptReference/Rigidbody2D-gravityScale.html)); `linearVelocityY` modifies the vertical component without changing the horizontal component ([Unity API reference](https://docs.unity3d.com/6000.0/ScriptReference/Rigidbody2D-linearVelocityY.html)).

The contact provider reports `SetWallContact(bool touching, Vector2 outwardNormal)`. The vector points from the wall toward the character. Only finite, nonzero normals with `abs(x) > abs(y)` are accepted and normalized. One wall jump is available per continuous contact; only an explicit `touching == false` report rearms it. Changing the reported side while still touching does not. Air-dash jump suppression continues to discard input and prevent a deferred wall jump. See [WALL_JUMP.md](WALL_JUMP.md) for the full contact contract and card scope.

If Jump is released while the current jump is rising, the component applies the configured cut multiplier once. Releasing before a buffered jump starts cuts that jump at launch; holding the input leaves its initial upward speed uncut. Coyote/buffer timers and jump processing run in `FixedUpdate`.

## Configuration and deferred integration

`jumpVelocity`, per-body `gravityScale`, `coyoteTime`, and `jumpBufferTime` must be finite and positive. `jumpCutMultiplier` must be finite and between 0 and 1. Invalid settings produce one diagnostic and the component does not initiate a jump. No product defaults have been approved; PlayMode test values are synthetic fixtures only.

This component is not wired to a player or sensor yet. The future character/input layer must translate physical controls to `PressJump`/`ReleaseJump` and report ground contact with `SetGrounded` and wall contact with `SetWallContact`. Final jump tuning, ground-sensing geometry, animation, double-jump rules, and air control stay with their respective backlog cards. Wall-jump behavior exists in the controller, but no scene sensor or authored character tuning has been integrated. This card does not modify the horizontal movement controller or global physics settings.

## QA

`JumpController2DPlayModeTests` covers ground jump, horizontal velocity preservation, coyote acceptance/expiry, buffered press on landing/expiry/contact, early-release cut, held jump, release before buffered launch, wall-contact validation and consumption, launch direction/priority, per-body gravity, and invalid configuration diagnostics. Passing these contract tests does not claim final game-feel values, scene sensor integration, or target-device performance.
