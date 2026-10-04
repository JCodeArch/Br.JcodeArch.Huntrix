# Wall Jump Design — Card 23

## Goal
Let the character gain height by jumping away from a wall, reusing the existing logical Jump action and jump behavior.

## Approved behavior
- Extend JumpController2D; do not add a second jump input or another controller.
- The grounding/contact layer supplies SetWallContact(bool touching, Vector2 outwardNormal). It reports contact start and loss. The normal points from the wall toward the character: left wall is +X, right wall is -X.
- Only finite, non-zero, predominantly horizontal normals are valid (abs(x) > abs(y)). Normalize accepted normals. Invalid contact clears current contact so stale data cannot launch the character.
- A buffered Jump while grounded keeps the current ground-jump path. While airborne, an available wall contact takes priority over coyote time and starts a wall jump. Without an available wall contact, existing coyote and buffer behavior remains unchanged.
- Wall jump replaces Rigidbody2D X and Y velocity at launch. X is away from the wall and uses a new positive finite Inspector setting wallJumpHorizontalSpeed; Y uses existing jumpVelocity. No numeric default is invented.
- Wall jumps use the existing active-jump and release-cut behavior. HorizontalMovement2D can apply the next input-controlled X velocity on the following FixedUpdate; no steering lock or timed velocity override is introduced.
- One wall jump is available per continuous wall-contact episode. Contact loss clears the consumed latch; a later valid contact re-arms it. Merely changing the reported normal while still touching does not re-arm it.
- Air dash remains higher priority: its current jump suppression blocks wall jumps and clears pending input. A press during suppression is ignored and cannot fire after dash completion. Ground dash behavior is unchanged.
- No wall sensor, scene integration, physical binding, wall cling, extra charge, cooldown, VFX, SFX, animation, event, or device tuning is part of this card.

## Architecture
JumpController2D remains the sole owner of jump eligibility, buffer, coyote state, active jump and jump cut. Wall contact is an input contract from the future/owning contact layer, just like the existing grounded signal. The wall jump directly sets launch velocity; it does not acquire HorizontalMovement2D's shared dash override. This preserves air-dash ownership and immediate player steering.

## Failure and lifecycle behavior
- Reject invalid wall normals and non-positive/non-finite wall horizontal speed without consuming the contact latch or changing velocity.
- Emit one diagnostic per invalid wall-tuning category, matching existing controller diagnostics.
- Disable has no additional owned override or suppression to clean up.
- The character collider, sensor, authored wall settings, and feel/tuning are not represented by the current Combat Lab; those remain unverified until a character scene exists.

## Verification
Extend JumpController2D PlayMode coverage for left/right launch, ground-jump preservation, wall priority over coyote, ordinary coyote/buffer preservation without a wall, one use per contact and re-arm after separation, invalid/missing/vertical normals, invalid wall speed, jump cut, and no deferred wall jump after air-dash suppression. Run all available EditMode and PlayMode suites. Record that no device performance profile or scene-level sensor integration was measured.
