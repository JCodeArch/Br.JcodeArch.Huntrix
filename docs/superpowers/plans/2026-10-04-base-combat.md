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
- Persistent overlap must be processed during the active window (`OnTriggerStay2D` or equivalent), because a rejected `OnTriggerEnter2D` will not fire again when invulnerability ends.
- Zero startup/recovery must skip directly through the phase without an artificial fixed tick; positive phase boundaries are quantized to fixed steps.
- Every configured damage receiver requires a dynamic `Rigidbody2D`, so each accepted hit can receive the configured impulse.
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

- [x] Write tests for valid definition property round-trip; invalid damage, startup, active, recovery, both size components, both offset components, and both knockback components (NaN/infinity/range); maximum-health initialization; invalid maximum health; and invalid serialized faction.
- [x] Run tests and confirm they fail for missing API/behavior.
- [x] Add serialized fields/properties: damage, startup, active, recovery, hitbox size/offset, horizontal/upward knockback impulse; validate with the ranges in the spec.
- [x] Implement `AttackDefinition.IsValid(out string error)` and receiver health/faction state with no product defaults.
- [x] Run focused PlayMode tests; confirm each invalid value is rejected and health begins at maximum only with valid configuration (63/63 pass, including invalid serialized faction; Unity 6000.6.4f1).

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

- [x] Write tests for attack start from idle in ground/air; invalid facing; requests discarded in each startup/active/recovery phase; no queued press; fresh press after idle; zero startup/recovery boundaries; and hitbox off before/after the active phase.
- [x] Write tests for hitbox size and facing-mirrored X offset, missing/non-trigger collider rejection and one diagnostic, root-hitbox isolation, one hit per receiver under persistent/multiple-hurtbox overlap, several opposing targets, self/same-faction rejection, health clamp/zero rejection, a target without DashController2D, dash rejection at contact and acceptance after dash ends during persistent overlap.
- [x] Run tests and confirm failure for missing hit/damage behavior.
- [x] Store faction/health on DamageReceiver2D on attacker and target roots. Controller/hitbox use the attacker root receiver; receiver compares root identity/faction and owns alive/dash-invulnerability acceptance. Hurtbox2D only relays. AttackHitbox2D owns accepted-target dedupe and only adds after acceptance.
- [x] Implement controller startup/active/recovery/idle phases and discard requests outside idle; input caller invokes the API on a logical press edge. Require AttackHitbox2D and DamageReceiver2D on the attacker root; hitbox owns its referenced child BoxCollider2D.
- [x] Implement `AttackHitbox2D.BeginActivation(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection)` / `EndActivation()`, hurtbox relay, and `DamageReceiver2D.TryReceiveHit(AttackDefinition attack, DamageReceiver2D attacker, float facingDirection)`. Receiver applies validated health damage/impulse and returns acceptance; only accepted receivers enter the per-activation set. Dynamic Rigidbody2D required.
- [x] Run focused PlayMode tests; verify cleanup when controller/hitbox is disabled, despawn cleanup, and root-hitbox isolation (25/25 pass after final changes).

### Task 3: Apply knockback and complete regression coverage

**Files:**
- Modify: `Assets/Scripts/Gameplay/Combat/DamageReceiver2D.cs`
- Test: `Assets/Tests/PlayMode/AttackController2DPlayModeTests.cs`
- Test: `Assets/Tests/PlayMode/DashController2DPlayModeTests.cs`
- Test: `Assets/Tests/PlayMode/HorizontalMovement2DPlayModeTests.cs`

**Interfaces:**
- Consumes: accepted hit, source position/facing, configured horizontal/upward impulse, target Rigidbody2D.
- Produces: one `AddForce(..., ForceMode2D.Impulse)` per accepted receiver hit; no direct velocity write.

- [x] Write tests for knockback away from left/right sources, upward component, exact-overlap facing fallback, and movement deceleration after an accepted impulse. In the dash test assert that invulnerability rejects both health change and force while the existing dash override remains unchanged.
- [x] Run tests and confirm missing/incorrect impulse behavior (exact-overlap fallback exposed and fixed a direction bug).
- [x] Implement impulse after hit acceptance; reject all damage/impulse while dash is invulnerable.
- [x] Run focused combat/dash/movement tests and all EditMode/PlayMode suites (25/25 AttackController focused; 63/63 data/health focused; 11/11 EditMode; 141/141 full PlayMode, 0 failures/skips, Unity 6000.6.4f1).

### Task 4: Document, review, and integrate

**Files:**
- Create: `docs/architecture/BASE_COMBAT.md`
- Update: `docs/architecture/DATA_DRIVEN_DEFINITIONS.md`
- Update: `docs/architecture/HORIZONTAL_MOVEMENT.md`
- Update: `docs/superpowers/plans/2026-10-04-base-combat.md`

- [x] Document APIs, data validation, attack phases, target/faction rules, dash immunity, knockback ownership, #25/#26 boundaries, and unverified integrations.
- [x] Obtain System Architect, SOLID, Performance, and QA reviews; resolve findings and record justified N/A gates. System Architect: APPROVED (invalid serialized faction recommendation implemented and retested); SOLID: APPROVED; Performance: APPROVED (reused HashSet and cached stable references; trigger parent lookup remains contact-driven); QA: APPROVED; documentation audit: APPROVED; final independent integration review: APPROVED after removing generated ProjectSettings changes.
- [x] N/A gates recorded: device benchmark has no approved device/metric target in this card/backlog; evidence is full Unity EditMode/PlayMode regression plus static hot-path review. UI/accessibility input testing is N/A because this card adds no UI, device binding, or accessibility option. Age/privacy/content-rights review finds no authored art/audio/text content, personal data, or licensed material changed; existing Child Safety and rights requirements remain applicable to future content cards.
- [x] Review full diff, Unity metadata, branch ancestry, Definition of Done, and test evidence; integration reviewer confirmed feature branch merge-base at origin/develop 536e47c and reports 25/25 controller, 63/63 data/health, 11/11 EditMode, 141/141 PlayMode.
- [ ] Commit card #24 on `feature/card-24-base-combat`, fast-forward/push `develop`, and verify remote refs and clean tree.
- [ ] Update Trello evidence/checklist and mark complete only after integration verification.

## Self-review

- Every spec section maps to Tasks 1–4; exact method/state/data names are consistent across tasks.
- TDD tests cover API, phases, configuration, target filtering, damage, invulnerability, impulse direction/ownership, and cleanup.
- No scene, asset, visual, audio, animation, AI, combo, parry, death flow, or device tuning is implied by the implementation tasks.
