# Wall Jump 2D

## Owner and inputs

`HuntrX.Gameplay.Jump.JumpController2D` owns jump eligibility, input buffering, coyote time, active-jump state, and jump-release cutting. The existing logical Jump action is reused through `PressJump()` and `ReleaseJump()`.

The contact provider reports wall state through `SetWallContact(bool touching, Vector2 outwardNormal)`. The normal points away from the wall and toward the character: a wall to the character's left reports a positive X normal; a wall to the right reports a negative X normal. A valid normal must have finite components, be nonzero, and be predominantly horizontal (`abs(x) > abs(y)`). The controller normalizes an accepted normal before using its X sign. Invalid input cannot initiate a wall jump.

## Eligibility and launch

When a buffered Jump is consumed, grounded jump has priority. While airborne, an unused valid wall contact has priority over coyote time. Without an available wall jump, the existing coyote and buffer rules apply.

A wall jump assigns both components of `Rigidbody2D.linearVelocity`: horizontal velocity points away from the wall at the configured `wallJumpHorizontalSpeed`; vertical velocity uses the existing `jumpVelocity`. Wall jumps enter the existing active-jump state and use the same release-cut multiplier. Ground and coyote jumps continue to preserve horizontal velocity.

`wallJumpHorizontalSpeed` must be finite and positive. No product default is approved. Invalid tuning prevents the launch and is diagnosed once per controller. Test fixture values are synthetic and do not represent final game tuning.

## Contact lifetime and movement ownership

Only one wall jump is allowed during a continuous contact episode. An explicit `SetWallContact(false, ...)` clears the consumed state; a later valid contact can then be used. Changing sides or sending another normal while `touching` remains true does not rearm the wall jump.

Air dash suppression remains authoritative: jump input during suppression is discarded and does not launch after the dash ends. The wall jump does not claim `HorizontalMovement2D`'s dash-override slot. Horizontal input may update X velocity on the following `FixedUpdate`.

## Scope and verification limits

This controller contract does not provide a wall sensor, collider geometry, scene wiring, player binding, wall cling, cooldown, extra charges, animation, VFX, SFX, or events. Scene-level contact reporting, authored movement tuning, game feel, and device performance have not been verified. Integrate and validate those when the player scene and target device profiles are defined.
