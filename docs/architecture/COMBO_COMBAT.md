# Combo Combat

## Scope and ownership

Card #25 extends the base attack contract from [BASE_COMBAT.md](BASE_COMBAT.md) with authored ground and air chains, global hit stop, and a typed event for accepted impacts. The card adds runtime contracts and tests only; it does not create sample attack/combo assets, tune final combat values, wire presentation, or add parry/reward behavior. Card #26 adds parry as a separate resolution result and does not change the accepted-impact contract.

| Responsibility | Owner |
|---|---|
| Static chain and step configuration | `CombatComboDefinition`, `ComboStep` |
| Chain selection, phases, timing, and transitions | `AttackController2D` |
| Candidate contacts and per-step target deduplication | `AttackHitbox2D` |
| Health acceptance, damage, invulnerability, knockback, and pre-damage parry resolution | `DamageReceiver2D` + optional `ParryController2D` |
| Global pause/hit-stop time-scale ownership | `HitStopService` |
| Public accepted-impact notification | `AttackController2D.ImpactOccurred` |
| Public successful-parry notification | `AttackController2D.ParryOccurred` |

## Definition contract

`CombatComboDefinition` contains explicit grounded and aerial sequences. Each must contain at least two steps. Each step references a valid `AttackDefinition`, supplies a finite nonnegative hit-stop duration, and, except for the final step, a finite non-empty chain window within that attack's total duration. The final step has no successor; its window fields are ignored.

Definitions are static template data. Runtime health, save state, and mutable progress do not belong in these assets. The existing single-argument `TryStartAttack(float facingDirection)` remains a grounded compatibility entry point; the overload with grounded state samples it only when idle.

## Chain behavior

A chain captures its grounded/aerial sequence at the start. That context remains fixed through landing or takeoff until the chain ends. A fresh idle chain selects again. Facing is sampled again on each accepted transition.

The logical Attack press edge advances one step only when the most recently completed fixed tick reports elapsed step time in the semi-open interval `[start, end)`. Fixed-step sampling quantizes when boundaries become observable. A press outside the window is discarded; it is never buffered, and holding does not repeat. A whiff may still chain. During hit stop, scaled elapsed time stops; an in-window press can advance state while physics is frozen. A final step has no outgoing transition.

Every step starts a fresh hitbox activation and clears that activation's receiver set. Each receiver can accept once per step, and can be hit again by a later step. Existing self/faction, health, dash-invulnerability, persistent-overlap, and knockback rules remain in force.

## Hit stop ownership

`HitStopService` is the designated owner of global `Time.timeScale` during pause and hit stop. It bootstraps before gameplay scenes and has an editor/test fallback, uses a single unscaled maximum deadline, and never writes `Time.fixedDeltaTime` or creates one coroutine per impact. Pause overlays a pending deadline without resuming simulation; resuming before expiry reapplies the remaining freeze, and resuming after expiry restores the captured base scale. New hit-stop requests while paused are ignored.

If a direct external write changes the scale to nonzero while the service expects zero, including during pause, the service relinquishes ownership, preserves the external scale, and clears pause/deadline state. Disabling or destroying a runner while it still owns a pause/freeze restores the captured base scale and clears ownership.

Global freeze is a provisional single-player prototype decision. Revisit it before local co-op because it freezes all local game-time simulation. Future pause, slow motion, and network simulation must coordinate with the same owner.

## Accepted impact event

The hitbox reports an internal accepted-hit notification only after the receiver accepts damage and the receiver is added to the per-step dedupe set. The enabled controller subscribes once and removes the subscription on disable. Disabling the controller first cancels the attack and disables the hitbox, preventing processing without a controller.

On accepted hit, the controller requests hit stop before dispatching its public `CombatImpactEvent`. The value-type payload contains attacker, receiver, attack definition, and zero-based combo step index. It omits contact position because a trigger overlap has no unique physical contact point. Rejected, duplicate, self, same-faction, dead, and dash-invulnerable contacts publish nothing.

Presentation code subscribes to `ImpactOccurred` and should unsubscribe in its own `OnDisable`/enable lifecycle. Listener exceptions are isolated so one callback cannot prevent later callbacks, damage handling, dedupe, or hit stop. Dispatch has no explicit per-impact allocation while the listener snapshot and collections remain stable; the receiver HashSet can grow as new receivers are encountered, and the listener snapshot is rebuilt when subscriptions change.

## Verification and integration notes

EditMode tests cover definition validity and malformed data. PlayMode tests cover sequence selection, captured context, fixed-time window boundaries, one transition per press, hitbox reset/teardown, accepted-hit feedback order, rejected contacts, listener lifecycle/re-entry/exception isolation, and hit-stop timing, ownership, pause, and cleanup. The final card record lists exact full-suite totals and Unity XML/log evidence.

Changing the controller's serialized definition from a single `AttackDefinition` to `CombatComboDefinition` changes the field contract. No versioned scene or prefab in this repository contains an old serialized reference; any external or future-authored scene/prefab must migrate its controller reference before use. Unity may regenerate local ProjectSettings files during batch testing; generated incidental changes are removed before commits.

## Deferred

- Authored combo assets, final attack timings/damage, and balance.
- VFX, SFX, animation, contact-point effects, and scene/prefab presentation wiring.
- Physical input bindings, buffering, hold-repeat, parry/counterattack/rewards.
- Network/co-op policy and device performance targets.

## Parry resolution (card #26)

`DamageReceiver2D` first applies the existing validation gates, including defender dash invulnerability, then asks an optional `ParryController2D` to consume an active window before changing health or applying knockback. The result is explicitly rejected, damaged, or parried. The parry window uses scaled fixed time. Its requested duration is rounded up to whole physics steps, with any positive duration providing at least one simulation step; the window closes on the next `FixedUpdate` before another simulation. It cannot start while its owner is dead or dash-invulnerable. Missing/invalid definition prevents activation. `AttackHitbox2D` deduplicates accepted damage and parry per activation before callbacks; rejected contacts remain eligible for reevaluation. `AttackController2D.ParryOccurred` forwards the distinct payload (defender, attacker, attack, zero-based combo step), without hit stop or `ImpactOccurred`. Listener exceptions are isolated. The event is a future counter/reward hook; this card does not apply counter damage or economy rewards.
