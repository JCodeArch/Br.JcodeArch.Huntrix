# Base Combat 2D

## Scope

Card #24 adds a single configurable attack activation for the same character on the ground and in the air. The caller forwards the logical Attack press edge to `AttackController2D.TryStartAttack(float facingDirection)`. This controller does not read device input. Card #25 adds combo chains, hit stop, and accepted-impact feedback as described in COMBO_COMBAT.md; buffering, repeated attacks, parry, counterattacks, death, and respawn belong to later cards.

## Data and health

`AttackDefinition` is a static ScriptableObject with explicit serialized values for damage, startup/active/recovery durations, hitbox rectangle and offset, and horizontal/upward knockback impulses. Its read-only properties expose these values; `IsValid(out string error)` checks finite values and their approved positive/nonnegative ranges. No authored combat asset or final tuning value is included.

`DamageReceiver2D` lives on a character root with a dynamic `Rigidbody2D`. It stores a valid `CombatFaction2D` (`HuntrX` or `Demon`) and finite positive `maximumHealth`; a valid receiver begins at maximum health. Invalid faction or other invalid configuration sets health to zero and reports one diagnostic. The receiver owns hit acceptance, current health, dash invulnerability checks, and accepted knockback. Damage clamps to zero; zero health rejects further hits but does not start a death/respawn flow.

## Attack phases and colliders

`AttackController2D` exposes read-only `AttackState2D State` (`Idle`, `Startup`, `Active`, `Recovery`) and accepts an attack only while idle with a finite, nonzero facing direction. Facing is normalized to its sign. Requests in other phases are discarded. Fixed timestep boundaries are quantized; zero startup/recovery skip directly without an extra tick. Disabling the controller ends the activation and disables its hitbox.

`AttackHitbox2D` owns a child trigger `BoxCollider2D` and a reusable set of receivers accepted in its current activation. A missing, non-child, or non-trigger collider invalidates the hitbox; `AttackController2D` rejects activation and reports one diagnostic. The controller always obtains the hitbox from the attacker's own root. The hitbox enables its collider only for the active phase, mirrors the configured local X offset by facing, and clears accepted targets when an activation starts, ends, or is disabled. `OnTriggerEnter2D` and `OnTriggerStay2D` allow a target rejected during dash invulnerability to be accepted once if the overlap remains after the dash ends.

`Hurtbox2D` caches the `DamageReceiver2D` in its parent hierarchy and only relays a candidate hit. The receiver rejects self, same-faction, invalid, dead, and dash-invulnerable targets. The hitbox adds a target to its per-activation set only after acceptance, so rejected dash contact does not consume that activation's hit.

## Knockback and movement ownership

An accepted hit applies `Rigidbody2D.AddForce` with `ForceMode2D.Impulse`. Horizontal direction points away from the attacker; exact horizontal overlap uses attack facing. Vertical impulse points upward. Combat code never writes Rigidbody velocity. During normal movement, `HorizontalMovement2D` decelerates the resulting horizontal velocity. While a dash override is active, the movement motor keeps that override; dash invulnerability rejects damage and impulse during the dash.

## Verification and limits

PlayMode tests cover data ranges, health initialization, attack state boundaries, inactive/active hitbox behavior, mirrored offsets, repeated input, target deduplication, multiple targets, faction/self filtering, damage clamp, dash rejection and persistent overlap, knockback direction, and movement interaction. Tests use synthetic physics values. This card does not verify device performance, final game feel, character/enemy integration, authored values, platform input, art/audio/animation, or network/co-op policy.

## Card #25 extension

Card #25 adds configurable ground/aerial sequences, per-step hitbox activation, accepted-impact notification, and hit stop. These contracts and their ownership boundaries are documented in [COMBO_COMBAT.md](COMBO_COMBAT.md). Card #24 remains the owner of base attack acceptance, damage, deduplication semantics, dash invulnerability, and knockback.
