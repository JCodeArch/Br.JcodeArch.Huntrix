# Base Combat Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add one configurable basic attack with 2D hitbox/hurtbox, health damage, and knockback for card #24.

**Architecture:** AttackController2D owns one attack activation and its fixed-step phases. AttackHitbox2D detects hurtboxes only while active; DamageReceiver2D owns faction, health, invulnerability checks, and accepted-hit impulse. AttackDefinition stores authored static values; movement continues owning velocity and dash override.

**Tech Stack:** Unity 6 / C#, Unity 2D Physics, Unity Test Framework PlayMode, existing `AttackDefinition`, `HorizontalMovement2D`, and `DashController2D`.

**Spec:** `docs/superpowers/specs/2026-10-04-base-combat-design.md`

## Global Constraints

- Use the logical Attack action; add no device binding.
- Add one attack and one hitbox; no combo, hold-repeat, buffer, parry, hit stop, or feedback.
- Every gameplay value comes from explicit valid configuration; product balance defaults are not approved.
- Faction, damage, hit timing, and knockback follow the spec's exact behavior.
- Combat code never assigns Rigidbody2D velocity; dash invulnerability rejects the complete hit.
- Do not add test actors or sample combat assets to CombatLab.

## Review Focus

- Startup/active/recovery boundaries and fixed-step timing; pin with phase and hitbox state tests.
- Persistent overlap, duplicate hurtboxes, multiple targets, self, and same-faction filtering; pin one-hit-per-receiver tests.
- Dash starts or remains active at the time of hit; verify damage and knockback are both rejected, then verify persistent overlap can be accepted if the active window remains after dash end.
- Knockback during normal movement; verify accepted hits use AddForce and the existing movement motor decelerates the result. During dash, verify damage/impulse are rejected and the dash velocity override remains intact.
- Invalid configuration, zero health, disable during activation, and exact horizontal overlap; verify no delayed hits or invalid state changes.

---

### Task 1: Author attack data, factions, and receiver health

**Files:**
- Modify: `Assets/Scripts/Data/Definitions/AttackDefinition.cs`
- Create: `Assets/Scripts/Gameplay/Combat/CombatFaction2D.cs`
- Create: `Assets/Scripts/Gameplay/Combat/DamageReceiver2D.cs`
- Create: `Assets/Tests/PlayMode/AttackDataAndHealthPlayModeTests.cs`

**Interfaces:**
- Consumes: explicit authored values and configured `maximumHealth`.
- Produces: read-only `AttackDefinition` properties and `IsValid(out string error)`; `CombatFaction2D` enum; `DamageReceiver2D.Faction`, `MaximumHealth`, `CurrentHealth`, `IsAlive`, and `IsConfigurationValid`.

- [ ] Write tests for valid definition property round-trip; invalid damage, startup, active, recovery, both size components, both offset components, and both knockback components (NaN/infinity/range); maximum-health initialization; and invalid maximum health.
- [ ] Run tests and confirm they fail for missing API/behavior.
- [ ] Add serialized fields/properties: damage, startup, active, recovery, hitbox size/offset, horizontal/upward knockback impulse; validate with the ranges in the spec.
- [ ] Implement `AttackDefinition.IsValid(out string error)` and receiver health/faction state with no product defaults.
- [ ] Run focused PlayMode tests; confirm each invalid value is rejected and health begins at maximum only with valid configuration.

### Task 2: Implement attack phases, hitboxes, hurtboxes, damage, and dash immunity

**Files:**
- Create: `Assets/Scripts/Gameplay/Combat/AttackController2D.cs`
- Create: `Assets/Scripts/Gameplay/Combat/AttackHitbox2D.cs`
- Create: `Assets/Scripts/Gameplay/Combat/Hurtbox2D.cs`
- Modify: `Assets/Scripts/Gameplay/Combat/DamageReceiver2D.cs`
- Create: `Assets/Tests/PlayMode/AttackController2DPlayModeTests.cs`

**Interfaces:**
- Consumes: controller phase and AttackDefinition; target Hurtbox2D; target DashController2D.IsInvulnerable when present.
- Produces: `AttackController2D.TryStartAttack(float facingDirection)` and `AttackState2D`; referenced child AttackHitbox2D with BoxCollider2D; Hurtbox2D relay; DamageReceiver2D hit acceptance/damage; AttackHitbox2D successful-target dedupe.

- [ ] Write tests for attack start from idle in ground/air; invalid facing; requests discarded in each startup/active/recovery phase; no queued press; and fresh press accepted only after idle.
- [ ] Write tests for no hit before/after active phase, hitbox size and facing-mirrored X offset for both facings, one hit per receiver under persistent/multiple-hurtbox overlap, several opposing targets, self/same-faction rejection, health clamp/zero rejection, a target without DashController2D, dash rejection at contact, and acceptance after dash ends during persistent overlap.
- [ ] Run tests and confirm failure for missing hit/damage behavior.
- [ ] Store faction/health on DamageReceiver2D on attacker and target roots. Controller and hitbox require the attacker root receiver; receiver compares root identity/faction and is the sole authority for alive/dash-invulnerability checks. Hurtbox2D only relays. Keep successful-target dedupe in AttackHitbox2D and add only after acceptance.
- [ ] Implement controller startup/active/recovery/idle phases and discard all requests outside idle; input adapter calls once per logical press edge. Require AttackHitbox2D and DamageReceiver2D on the same attacker root; hitbox owns its referenced child BoxCollider2D.
- [ ] Implement `AttackHitbox2D.BeginActivation(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection)` / `EndActivation()`, hurtbox relay, and `DamageReceiver2D.TryReceiveHit(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection)`. Receiver applies validated health damage and returns acceptance; only an accepted result enters the hitbox's per-activation dedupe set.
- [ ] Run focused PlayMode tests; verify cleanup when either attack controller or hitbox is disabled.

### Task 3: Apply knockback and complete regression coverage

**Files:**
- Modify: `Assets/Scripts/Gameplay/Combat/DamageReceiver2D.cs`
- Test: `Assets/Tests/PlayMode/AttackController2DPlayModeTests.cs`
- Test: `Assets/Tests/PlayMode/DashController2DPlayModeTests.cs`
- Test: `Assets/Tests/PlayMode/HorizontalMovement2DPlayModeTests.cs`

**Interfaces:**
- Consumes: accepted hit, source position/facing, configured horizontal/upward impulse, target Rigidbody2D.
- Produces: one `AddForce(..., ForceMode2D.Impulse)` per accepted receiver hit; no direct velocity write.

- [ ] Write tests for knockback away from left/right sources, upward component, exact-overlap facing fallback, and movement deceleration after an accepted impulse. In the dash test assert that invulnerability rejects both health change and force while the existing dash override remains unchanged.
- [ ] Run tests and confirm missing/incorrect impulse behavior.
- [ ] Implement impulse after hit acceptance; reject all damage/impulse while dash is invulnerable.
- [ ] Run focused combat/dash/movement tests and all EditMode/PlayMode suites.

### Task 4: Document, review, and integrate

**Files:**
- Create: `docs/architecture/BASE_COMBAT.md`
- Update: `docs/architecture/DATA_DRIVEN_DEFINITIONS.md`
- Update: `docs/architecture/HORIZONTAL_MOVEMENT.md`
- Update: `docs/superpowers/plans/2026-10-04-base-combat.md`

- [ ] Document APIs, data validation, attack phases, target/faction rules, dash immunity, knockback ownership, #25/#26 boundaries, and unverified integrations.
- [ ] Obtain System Architect, SOLID, Performance, and QA reviews; resolve findings and record justified N/A gates.
- [ ] Review full diff, Unity metadata, branch ancestry, Definition of Done, and test evidence.
- [ ] Commit card #24 on `feature/card-24-base-combat`, fast-forward/push `develop`, and verify remote refs and clean tree.
- [ ] Update Trello evidence/checklist and mark complete only after integration verification.

## Self-review

- Every spec section maps to Tasks 1–4; exact method/state/data names are consistent across tasks.
- TDD tests cover API, phases, configuration, target filtering, damage, invulnerability, impulse direction/ownership, and cleanup.
- No scene, asset, visual, audio, animation, AI, combo, parry, death flow, or device tuning is implied by the implementation tasks.
