# Horizontal Movement 2D

## Contract

`HuntrX.Gameplay.Movement.HorizontalMovement2D` consumes the horizontal axis of the logical `Move` action through `SetMovementInput(Vector2)`. It has no key, gamepad, or touch bindings; an input adapter can forward the shared action value when a character/input card supplies that layer.

The component requires a `Rigidbody2D` and updates its horizontal velocity in `FixedUpdate` using Unity 6's `Rigidbody2D.linearVelocityX` ([Unity API reference](https://docs.unity3d.com/6000.0/ScriptReference/Rigidbody2D-linearVelocityX.html)). The vertical velocity is left to gravity and other systems. Input is clamped to [-1, 1]. Acceleration moves velocity toward the target while increasing speed in the same direction or starting from rest. Releasing or reversing direction uses deceleration until the target is reached.

`State` is `Idle` when horizontal speed is approximately zero and `Moving` otherwise. It does not represent grounded/airborne state, animation state, or a gameplay action. The component caches its `Rigidbody2D` once in `Awake`; invalid tuning is diagnosed once and does not apply horizontal input.

## Tuning and deferred decisions

`maxHorizontalSpeed`, `acceleration`, and `deceleration` must all be configured as finite positive values. No product defaults are selected because neither the GDD nor card #19 approves movement numbers. Test fixtures use synthetic values only to check algorithm behavior. Tune actual characters in the Combat Lab when character contracts and playtest evidence exist.

The component is not wired into a player object yet; character implementation is later in the backlog. Jumping, grounded/airborne state, animation, facing, slope/ground detection, dash, and wall jump remain in their own cards. Knockback will be introduced by combat cards; those cards must define how external horizontal impulses coexist with this movement controller before wiring the systems together.

## QA

`HorizontalMovement2DPlayModeTests` covers gradual acceleration and speed limiting, deceleration to idle after release, direction reversal, input clamping, vertical-velocity preservation, state transitions, and invalid tuning diagnostics. These tests verify the controller contract, not final game feel or approved player values.