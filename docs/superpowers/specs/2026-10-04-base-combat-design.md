# Base Combat — Card 24

## Goal and scope

Implement one data-driven 2D basic attack and the minimum target contract needed to apply damage and knockback. A caller supplies the existing logical `Attack` action; gameplay code does not read keyboard, gamepad, or touch bindings. The same basic attack is available on the ground and in the air.

This card does not add combo chains, held-button repetition, input buffering, hit stop, impact feedback, parry, counterattack, reward, enemy AI, authored combatants, scene wiring, death/respawn flow, costs, cooldowns, animation, VFX, SFX, networking, or final tuning. Combos/hit stop/feedback belong to #25; parry/counterattack/reward belong to #26.

## Existing constraints

- `AttackDefinition` already exists as a static ScriptableObject with no gameplay fields. It is the authored source for this attack's explicit configuration.
- `Attack` is an existing logical action. This card exposes a caller-facing attack API but adds no physical binding.
- `DashController2D.IsInvulnerable` is true during a dash. `DamageReceiver2D` is the single authority that rejects both damage and knockback while the target is dashing; `Hurtbox2D` only relays the contact. A target without `DashController2D` is vulnerable by default.
- `HorizontalMovement2D` is the sole routine writer of horizontal velocity during normal movement and owns dash overrides. Knockback uses `Rigidbody2D.AddForce(..., ForceMode2D.Impulse)` after an accepted hit; the movement motor then handles the resulting velocity through its existing deceleration, while an active dash override remains higher priority. Combat code does not assign velocity directly.
- The GDD leaves combat values, attack timings, ranges, and balance open. Serialized fields have no approved product defaults; tests use synthetic values only.

## Runtime contract

### Attack activation

`AttackController2D`, its referenced `AttackHitbox2D`, and the attacker's `DamageReceiver2D` live on the attacker root. `AttackHitbox2D` references a child `BoxCollider2D` set to trigger. The controller exposes `TryStartAttack(float facingDirection)` and read-only `AttackState2D State` (`Idle`, `Startup`, `Active`, `Recovery`). It obtains the attacker's faction from the root receiver. The input adapter calls the method only on a logical Attack press edge. The method starts one activation when idle and when the direction is finite and nonzero; direction is normalized to its sign. It consumes an assigned valid `AttackDefinition`. A request during startup, active, or recovery is discarded, not queued. After recovery returns the controller to idle, a new press edge can start an activation. A held action does not emit another press edge and does not repeat. There is one attack definition and one active hitbox; no chain or ground/air variant is created. The controller begins/ends the hitbox through `AttackHitbox2D.BeginActivation(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection)` and `EndActivation()`.

The controller progresses through startup, active, recovery, and idle using `FixedUpdate`. The attack hitbox is disabled except during the active phase. Disabling the controller disables the hitbox and clears activation state so no hit occurs after re-enable.

### Authored data

`AttackDefinition` exposes read-only properties `Damage`, `StartupDuration`, `ActiveDuration`, `RecoveryDuration`, `HitboxSize`, `HitboxOffset`, `HorizontalKnockbackImpulse`, and `UpwardKnockbackImpulse`. Their serialized fields are `damage`, `startupDuration`, `activeDuration`, `recoveryDuration`, `hitboxSize`, `hitboxOffset`, `horizontalKnockbackImpulse`, and `upwardKnockbackImpulse`. Requirements are positive finite damage; finite nonnegative startup/recovery; finite positive active duration; finite positive X/Y hitbox size; finite X/Y offset; positive finite horizontal impulse; and finite nonnegative upward impulse. `IsValid(out string error)` checks each scalar/vector component for NaN and infinity as well as its range and returns the first validation error without logging. No sample AttackDefinition asset or product values are added. An invalid definition rejects activation with one diagnostic per AttackController2D and does not enable the hitbox. `DamageReceiver2D` exposes read-only `Faction`, `MaximumHealth`, `CurrentHealth`, `IsAlive`, and `IsConfigurationValid`; it requires finite positive serialized `maximumHealth`, initializes `CurrentHealth` to that maximum, sets `IsConfigurationValid` false and current health to zero otherwise, and rejects damage when invalid or already at zero. Invalid maximum health yields one receiver diagnostic.

### Targets and damage

`CombatFaction2D` is a two-value enum (`HuntrX`, `Demon`) stored on each `DamageReceiver2D`. All HUNTR/X characters share one faction; demon targets use the other. `Hurtbox2D` finds the `DamageReceiver2D` on its own root and only relays the attacker's root receiver, attack definition, and facing. `DamageReceiver2D.TryReceiveHit(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection)` rejects its own root, same-faction sources, dead targets, invalid health configuration, or dash invulnerability; otherwise it applies health damage and returns true. It has no dependency on a generic team or status framework. `AttackHitbox2D` owns a per-activation set of receivers already successfully hit, clears it at activation start/end/disable, and adds a receiver only after `TryReceiveHit` returns true. Thus a rejected dash hit does not consume the target's hit for that attack; persistent overlap may be accepted once if the target becomes vulnerable before the active window ends. A single activation can hit several opposing targets, but each receiver accepts at most one hit.

An enabled `Hurtbox2D` forwards its contact to the target's `DamageReceiver2D`. The receiver owns configured maximum/current health, starts at maximum health, clamps received damage at zero, and rejects hits when already at zero. It has no death event, animation, respawn, or save behavior. A hit during `DashController2D.IsInvulnerable` is rejected without changing health or applying knockback. An invulnerability rejection does not consume that receiver's accepted-hit slot: if overlap persists and the same attack remains active after the dash ends, the hit may then be accepted once.

### Hit and knockback

The configured rectangular hitbox is offset horizontally by attack facing and is active only during the configured active interval. Its `BoxCollider2D.size` uses the configured size and its local X offset is multiplied by facing; Y offset is unchanged. Overlap with a valid opposing hurtbox sends damage and knockback once for that receiver in the activation. Horizontal knockback points away from the attacker; exact horizontal overlap falls back to the attack facing. Vertical knockback points upward. The receiver applies the configured impulse through `Rigidbody2D.AddForce` in impulse mode, never by assigning `linearVelocity`. Normal movement decelerates the resulting horizontal velocity using its existing motor; an active dash override remains the motor's movement priority, and dash invulnerability prevents accepted hits during that state.

## Verification contract

PlayMode tests exercise attack phase timing, hitbox off/on/off boundaries, repeated input, ground/air availability, multi-target hits, one hit per receiver, self/faction filters, damage/health clamping, zero-health rejection, dash invulnerability and continued overlap after dash, knockback direction and impulse composition with movement. Invalid definitions and controller disable must fail safely. Tests use synthetic physics values and do not establish final game feel or device performance.

No test actor or combat asset is added to `CombatLab`; tests construct their own physics fixtures. Device performance, actual character/enemy integration, player-facing input mapping, combat art/audio/animation, and co-op/network rules remain unverified and outside this card.
