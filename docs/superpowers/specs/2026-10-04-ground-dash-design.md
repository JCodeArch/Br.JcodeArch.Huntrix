# Ground Dash Design

## Purpose

Card #21 implements the ground-start dash without inventing the open gameplay tuning or presentation assets.

## Contract

- `DashController2D` receives a horizontal direction from the logical character/action layer. It has no physical input bindings.
- Starting requires an enabled component, an externally supplied grounded state, finite non-zero horizontal direction, and valid positive speed/duration. Invalid direction and tuning are each diagnosed once. The configured direction sign selects left or right.
- During the configured active duration, the dash owns a horizontal-velocity override on the same `HorizontalMovement2D` component. The override is owner-scoped so another component cannot clear it accidentally. On completion/disable it releases the override, and regular movement resumes.
- `HorizontalMovement2D` runs before the dash controller in the fixed-step order, so the requested number of physics ticks receive the dash velocity before expiry releases ownership. Durations shorter than one fixed step still receive one movement tick. The dash preserves vertical velocity and exposes `IsDashing` / `IsInvulnerable` as the same active interval. Future damage/hurtbox code can consume this property.
- Start/end events provide hooks for later animation, visual, and audio feedback; no visual or audio content is created here.
- Airborne activation, cooldown, damage integration, and final timing/tuning are not introduced. A ground-start dash continues for its active duration if ground contact is lost after activation; air dash activation belongs to card #22.

## Tuning and validation

Dash speed and duration must be finite and positive. There are no product defaults because the GDD does not approve dash values. Test values are synthetic. No cooldown is added by this card.

## Tests

PlayMode coverage verifies grounded activation, direction, active lock, duration, invulnerability, feedback transitions, vertical velocity preservation, invalid tuning/direction, exact fixed-step expiry, sub-step duration, disabled-component rejection, override ownership, and return to regular movement. These tests validate controller contracts, not final game feel or damage resolution.