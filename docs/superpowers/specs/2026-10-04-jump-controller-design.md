# Jump Controller 2D Design

## Purpose

Card #20 adds a reusable jump component for the later character system. Existing GDD/controls define the logical Jump action but leave jump values and game-feel tuning open.

## Contract

- `JumpController2D` requires and caches a `Rigidbody2D`.
- Input comes from logical `PressJump()` and `ReleaseJump()` calls, with no keyboard/gamepad/touch bindings.
- A later character/ground-sensing layer reports contact through `SetGrounded(bool)`. This card does not invent layers, collider footprints, slope limits, or floor tags.
- A grounded or coyote-valid press applies configured upward velocity while preserving horizontal velocity.
- A press during airtime remains buffered until grounded or the configured timer expires.
- The component applies a configured positive `gravityScale` to the body's own Rigidbody2D. It never changes global `Physics2D.gravity`.
- Releasing jump while the current jump is rising applies a configured cut multiplier once. Holding Jump does not apply the cut early.
- State/timers update in `FixedUpdate` using fixed physics time.

## Configuration and limits

Jump velocity, per-body gravity scale, coyote duration, buffer duration, and the early-release multiplier have no approved project defaults; the component requires explicit valid values and rejects invalid configurations once with a diagnostic. Test fixture values are synthetic only. No player, collider, ground sensor, input binding, animation, wall jump, or final tuning is added.

## Validation

PlayMode tests cover ground jump, coyote acceptance/expiry, buffered press on landing/expiry, early-release jump cut/hold, gravity-scale scope, horizontal-velocity preservation, and invalid configuration. Future character work must provide the grounded signal and physical input adapter.