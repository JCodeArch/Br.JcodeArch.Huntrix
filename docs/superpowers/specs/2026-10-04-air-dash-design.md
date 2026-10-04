# Air Dash Design — Card 22

## Goal
Add one directional air dash to the existing 2D dash controller while preserving ground dash behavior.

## Approved behavior
- Gameplay passes its logical Move vector to DashController2D.TryStartAirDash(Vector2 direction); hardware bindings remain outside gameplay.
- The air dash is available only while the last grounding signal says airborne. It starts only with finite, non-zero direction, valid air speed/duration, enabled controller, no active dash, available charge, and an available movement override.
- Normalize direction before applying configured air dash speed, so diagonal input has the same magnitude as cardinal input.
- Set Rigidbody2D velocity to normalized direction multiplied by air dash speed once at dash start. Keep the existing owner-scoped horizontal override active for the dash duration, locking X. Do not rewrite Y each FixedUpdate: Rigidbody2D gravity continues integrating vertical velocity. At end, clear only the dash controller's override and do not restore a stale Y value.
- One air dash is allowed per airborne interval. SetGrounded(true) restores the charge idempotently. Landing during a dash replenishes the charge but does not stop the active dash; grounded state still prevents starting an air dash.
- Ground and air dash share IsDashing, IsInvulnerable, and DashStateChanged. Every valid start emits true once and its termination emits false once.
- Air dash takes priority over JumpController2D until dash termination. Successful air-dash start suppresses jump start, jump buffer/coyote processing, and jump cut, discarding pending/active jump state. Ending or disabling the air dash releases suppression. Ground dash does not suppress Jump.

## Configuration
Add serialized airDashSpeed and airDashDuration fields authored through the Inspector. Do not invent numeric defaults, cooldown, hardware bindings, extra charges, or wall reset behavior.

## Architecture boundaries
DashController2D owns dash eligibility, duration, charge, and public state. HorizontalMovement2D remains the Rigidbody2D horizontal velocity writer and already owns a temporary override slot identified by a MonoBehaviour owner. Reuse that horizontal-only slot for air dash. Do not add a general vector override to the motor. Ground dash continues writing only X and preserving Y. DashController2D caches the optional JumpController2D on the same character object and toggles only its narrow suppression API.

## Failure and lifecycle behavior
- Reject zero, NaN, or infinite air directions and non-positive/non-finite air tuning; log each invalid category at most once, matching ground-dash diagnostic style.
- A rejected start does not consume the air charge or emit state events.
- Disable ends an active dash, clears movement ownership, releases jump suppression, and emits one end transition.
- Ground dash remains grounded-only, horizontal-only, with its existing validation/events.
- SetGrounded(true) during an air dash restores the charge but does not cancel the current dash.

## Verification
PlayMode tests cover normalized cardinal/diagonal direction, vertical launch with gravity changing Y while X stays owned, duration expiry, ownership conflict and cleanup, one-use/recharge state, invalid input/configuration, landing mid-dash, disable lifecycle, paired events, jump buffer/coyote/cut suppression and release, and ground-dash regression. Run the complete EditMode and PlayMode suites available in the project. Record exact results and limitations; do not claim device performance measurements without profiling a target build.
