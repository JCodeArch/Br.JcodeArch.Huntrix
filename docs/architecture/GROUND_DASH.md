# Ground Dash

## Contract

`HuntrX.Gameplay.Dash.DashController2D` starts a dash only when an external character/grounding layer has supplied grounded state and a finite, non-zero horizontal direction. The component rejects starts while disabled and diagnoses invalid directions once. It has no keyboard, gamepad, or touch bindings. `TryStartDash(float)` accepts the logical direction; `SetGrounded(bool)` accepts the external grounded signal.

Dash speed and duration are serialized tuning values and must be finite and positive. The GDD and card do not approve product values, so no gameplay defaults or cooldown are selected. While active, the dash owns horizontal velocity through `HorizontalMovement2D.TrySetHorizontalVelocityOverride`; the shared motor applies that value in its existing `FixedUpdate` before the dash timer advances (the motor has an explicit earlier execution order), preserving vertical velocity and avoiding competing Rigidbody writers. Durations resolve at fixed-step precision; even a sub-step duration is applied for one physics movement tick before the state ends. On expiry or disable, the dash clears only its own override and regular movement resumes.

`IsDashing` and `IsInvulnerable` are true for the same active interval. `DashStateChanged` emits `true` at start and `false` at end as hooks for a future animation/VFX/SFX layer. Damage/hurtbox integration, audiovisual assets/timing, character grounding, and air dash remain separate backlog work.

## QA

`DashController2DPlayModeTests` covers ownership and return to regular movement, horizontal direction, vertical velocity preservation, grounded/direction gating, active invulnerability, transition events, duration, and invalid tuning and direction diagnostics, exact fixed-step expiry, sub-step movement, and disabled-component rejection. Test values are synthetic and do not set final game feel.