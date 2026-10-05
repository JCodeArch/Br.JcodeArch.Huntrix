# Mira Defensive Field Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to execute this plan task-by-task. Each step uses checkbox syntax for tracking.

**Goal:** Deliver a Mira prototype with a temporary area field that protects opted-in allied combat receivers from damage and knockback.

**Architecture:** A validated static ScriptableObject stores field duration and radius. A runtime field controller owns timing and overlap membership, while a receiver marker owns registration from multiple fields. DamageReceiver2D resolves the new Protected contact after existing parry resolution, and AttackHitbox2D deduplicates the protected target without publishing damage or parry events.

**Tech Stack:** Unity 6.6.4f1, C#, ScriptableObject, Rigidbody2D, CircleCollider2D, Unity Test Framework.

**Spec:** docs/superpowers/specs/2026-10-04-mira-defensive-field-design.md

## Global Constraints
- Use Unity 6.6.4f1.
- Keep authored duration/radius finite and positive; do not claim final balance.
- Only DamageProtection2D opt-in receivers can be protected.
- Mira's protected area uses a child CircleCollider2D centered at local zero on the owner's Rigidbody2D.
- The area registers existing overlaps synchronously on activation and maintains membership through trigger callbacks; do not add per-frame overlap scans.
- A Protected contact is deduplicated for the rest of the current attack activation; a later hitbox activation checks protection again.
- DamageProtection2D.IsProtected validates its already-registered source/collider pairs on demand with collider distance checks and active/enabled guards; it does not scan every frame.
- Existing validity, self/faction, dash-invulnerability, and parry rules retain their current order; parry resolves before field protection.
- Do not add fan lifecycle/rescue rules, Honmoon, costs, cooldown, physical input, character switching/respawn, co-op/network behavior, or final art.
- Reuse existing movement/combo/parry definitions as provisional basic combat fixtures; no unique attack choreography or final tuning is claimed.
- Use CombatLab as an empty host scene and instantiate authored test prefabs/targets at runtime.

## Review Focus
- Invalid or missing profile/collider/owner dependencies must fail activation without partially enabling the field. Test in Task 2.
- A receiver with multiple colliders stays protected until its final collider exits; a target without DamageProtection2D never opts in. Test in Task 2.
- Receivers already inside are protected synchronously; a Protected contact remains deduplicated if the field expires mid-activation, while a later activation follows current protection. Test in Task 2.
- A target or the field can move before the first physics simulation after activation; on-demand collider distance checks must prevent stale out-of-area protection without per-frame scans. Test in Task 2.
- Overlapping fields and disable/destroy cleanup must remove only their own registrations. Test in Task 2.
- Dash and parry precedence, event silence for Protected contacts, and unchanged damage/knockback after expiry must hold. Test in Tasks 2–3.

---

### Task 1: Static Mira protection definition

**Files:**
- Create: Assets/Scripts/Data/Definitions/MiraProtectionDefinition.cs and its Unity .meta file
- Test: Assets/Tests/EditMode/MiraProtectionDefinitionTests.cs and its Unity .meta file

**Interfaces:**
- Produces MiraProtectionDefinition.DurationSeconds, Radius, and IsValid(out string error).
- Serialized values are durationSeconds and radius; CreateAssetMenu path is HUNTR/X/Data/Mira Protection.
- Consumes GameDataDefinition conventions and the project's finite-positive validation style.

- [x] Write EditMode tests for valid values and zero, negative, NaN, positive infinity, and negative infinity for each field.
- [x] Run the focused EditMode test and confirm the invalid-value assertions fail before implementation.
- [x] Implement only static validated data; do not add runtime timer or target state to the ScriptableObject.
- [x] Run focused EditMode tests and verify the definition passes/fails at the specified boundaries.
- [x] Commit the task as a focused feat/test commit on feature/card-29-mira.

### Task 2: Runtime field membership and protected contact resolution

**Files:**
- Create: Assets/Scripts/Gameplay/Protection/DamageProtection2D.cs and its Unity .meta file
- Create: Assets/Scripts/Gameplay/Protection/MiraProtectionField2D.cs and its Unity .meta file
- Modify: Assets/Scripts/Gameplay/Combat/DamageReceiver2D.cs
- Modify: Assets/Scripts/Gameplay/Combat/AttackHitbox2D.cs
- Modify: Assets/Scripts/Gameplay/Combat/CombatContactResult.cs
- Test: Assets/Tests/PlayMode/MiraProtectionFieldTests.cs and its Unity .meta file

**Interfaces:**
- DamageProtection2D exposes bool IsProtected; internal Register(MiraProtectionField2D source, Collider2D overlap), Unregister(MiraProtectionField2D source, Collider2D overlap), and UnregisterSource(MiraProtectionField2D source) maintain bidirectional multi-collider membership. IsProtected checks the current geometry of its registered pairs on demand.
- MiraProtectionField2D exposes MiraProtectionDefinition Definition, bool IsActive, and bool TryActivate(). It uses a root DamageReceiver2D and a child trigger CircleCollider2D.
- CombatContactResult adds Protected as a distinct result.
- DamageReceiver2D checks the optional protection receiver after its current validity/faction/self/dash checks and after ParryController2D resolution.
- AttackHitbox2D treats Protected as resolved for the active hitbox activation and emits neither AcceptedHit nor ParriedHit for that outcome.

- [x] Write PlayMode tests for invalid activation dependencies, synchronous registration for receivers already inside, and no opt-in protection; run them and confirm the expected failures.
- [x] Implement activation validation and synchronous overlap seeding; run the focused tests and verify they pass.
- [x] Write failing tests for multiple colliders, overlapping sources, and bidirectional disable/destroy cleanup; implement per-source collider membership and teardown; run these tests and verify they pass.
- [x] Write failing tests for trigger entry/exit and protection after movement; implement trigger membership updates without per-frame scans; run these tests and verify they pass.
- [x] Write failing tests for scaled duration expiry and repeated activation; implement FixedUpdate timing where repeated activation does not refresh duration; run these tests and verify they pass.
- [x] Write failing tests for movement of the target and field before the first physics simulation; implement on-demand geometry checks over registered pairs; run these tests and verify stale membership never protects an out-of-area target.
- [x] Write failing tests for Protected resolution, health/knockback/event silence, and parry/dash precedence; implement the result in DamageReceiver2D and AttackHitbox2D after parry; run these tests and verify they pass.
- [x] Write failing tests for same-activation dedupe across expiry/exit and a later activation rechecking protection; implement activation-scoped reservation; run these tests and verify they pass.
- [x] Run the focused PlayMode suite; verify every behavior and event assertion.
- [x] Commit the runtime contract and tests as a focused commit.

### Task 3: Mira authored profile and prefab

**Files:**
- Create: Assets/Data/Characters/Mira assets and their .meta files
- Create: Assets/Resources/Characters/Mira_Prototype.prefab and its .meta file
- Test: Assets/Tests/EditMode/MiraCharacterTests.cs and its .meta file
- Test: Assets/Tests/PlayMode/MiraCharacterPlayModeTests.cs and its .meta file

**Interfaces:**
- The Mira CharacterDefinition applies existing movement, combo, and parry controllers using provisional shared basic-combat fixtures.
- MiraProtectionField2D on the actual prefab references MiraProtectionDefinition and uses its own child CircleCollider2D.
- Tests load CombatLab as the empty host and instantiate the actual Mira prefab plus generic opted-in/unopted targets.

- [x] Write EditMode and PlayMode tests for Mira definition IDs/references, valid field profile, prefab component/collider composition, definition application, protection of an in-range opted-in ally, and unchanged Rumi behavior; run them and confirm expected failures.
- [x] Author Mira's ScriptableObject profile/field data and prefab with valid stable GUIDs; preserve CombatLab unchanged.
- [x] Run the focused EditMode and PlayMode tests; verify the actual asset/prefab references resolve and all authored Mira behaviors pass in CombatLab.
- [x] Commit Mira assets and integration tests as a focused content commit.

### Task 4: Documentation, reviews, and card integration evidence

**Files:**
- Create: docs/architecture/MIRA_CHARACTER.md
- Modify: docs/architecture/DATA_DRIVEN_DEFINITIONS.md
- Modify: docs/GDD_MASTER.md
- Modify: Assets/Tests/PlayMode/MiraProtectionFieldTests.cs to remove the deprecated autoSyncTransforms setting toggle; the explicit-pose collider distance regression does not depend on global auto-sync, and the project uses m_AutoSyncTransforms=0.
- Modify: Assets/Tests/PlayMode/AttackController2DPlayModeTests.cs to replace the deprecated FindObjectsSortMode overload with the active-only FindObjectsInactive.Exclude overload, preserving existing behavior.
- Modify: this implementation plan with completed task and verification checkboxes

- [x] Document the Mira field, provisional shared basic-combat profile, target opt-in contract, actual fan integration dependency, and non-goals.
- [x] Remove the two observed CS0618 test-only API warnings with behavior-preserving overload/fixture cleanup; verify the regressions remain meaningful.
- [x] Run the complete Unity 6.6.4f1 EditMode and PlayMode suites; record exact totals and logs, including zero compiler errors/warnings.
- [x] Verify all new Unity .meta GUIDs are present and unique; inspect the complete branch diff and working tree.
- [x] Obtain SOLID, Performance, and QA reviews; fix blockers and record limited N/A/risks. SOLID and QA approved; Performance approved with a Minor device-profiling follow-up. Antigravity was not used: automatic approval review rejected exporting source/assets/test results to the external service; independent local reviews and test evidence are recorded in `docs/reviews/2026-10-05-card-29-mira.md`.
- [x] Perform final branch review; fix the extra blank lines at the design spec EOF and verify the complete diff and Unity metadata references.
- [x] Commit the architecture/GDD/review record as the card's own final docs commit.
- [ ] Push feature/card-29-mira, fast-forward and push develop, verify both remote refs and a clean working tree.
- [ ] Update Trello card #29 description/checklist with criteria, test evidence, agent reviews, limits, files, and commit; mark complete and move it only after the Definition of Done is verified.
- [ ] Re-read Trello open cards and confirm #30 is the next item in the planned order before beginning another card.



Task 4 implementation evidence: full Unity 6.6.4f1 EditMode 65/65 and PlayMode 245/245 passed, no failed/skipped/inconclusive tests and no C# compiler warnings/errors. Twelve branch-added Unity metadata GUIDs are present and unique. Completed XML/logs and the local task report are under `.superpowers/sdd/2026-10-04-mira-defensive-field/`. SOLID/Performance/QA reviews and the final branch review are recorded; mobile profiling, actual fan integration, remote integration, and Trello closure remain open as applicable. The Antigravity external review was not run because automatic approval review rejected transferring project source/assets/test results to that service. See `docs/reviews/2026-10-05-card-29-mira.md` and `docs/architecture/MIRA_CHARACTER.md`.
