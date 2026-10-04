# Rumi Character Kit Implementation Plan

> **Execution record:** Steps use checkbox syntax; completed gates are checked with Unity results and review evidence.

**Goal:** Ship a Rumi prototype asset and prefab that apply validated movement, sword-combo, and parry definitions through existing controllers.
**Architecture:** Static data stays in ScriptableObjects. CharacterDefinitionApplier2D preflights the complete definition and actor composition, then configures existing components through explicit APIs.
**Tech Stack:** Unity 6.6.4f1, C#, ScriptableObject, Rigidbody2D, Unity Test Framework.
**Spec:** docs/superpowers/specs/2026-10-04-rumi-character-design.md

## Global Constraints
- Reuse existing movement, combat, damage, combo, and parry controllers.
- Keep definitions static and runtime state on components.
- Do not implement physical input, CharacterManager, respawn consumption, or character switching.
- Treat all Rumi numbers as prototype-only; claim no final tuning or playtest approval.
- Do not add packages or modify CombatLab.

## Review Focus
- Missing/invalid references must reject setup without partial application.
- Disabling the actor during attack, parry, or dash must retain existing teardown behavior.
- Rumi ground/aerial selection must route accepted attacks through existing direct damage.
- Reconfiguration during active attack/parry must be rejected.
- Authored asset GUIDs and references must be valid and unique.

---

### Task 1: Static character and movement definitions
**Files:** modify `Assets/Scripts/Data/Definitions/CharacterDefinition.cs`; create `Assets/Scripts/Data/Definitions/CharacterMovementDefinition.cs`; test `Assets/Tests/EditMode/CharacterDefinitionTests.cs`.

**Interfaces:** Produce CharacterMovementDefinition.IsValid(out string error), CharacterDefinition.DisplayName, Movement, CombatCombo, Parry, and CharacterDefinition.IsValid(out string error). Consume GameDataDefinition, CombatComboDefinition, and ParryDefinition.

- [x] Write failing EditMode tests for required references and movement ranges.
- [x] Run targeted EditMode tests and confirm expected failures.
- [x] Implement validated static definitions without runtime state.
- [x] Run focused EditMode (7/7) and full EditMode (52/52) tests.

### Task 2: Controller configuration and actor applier
**Files:** modify existing movement, jump, dash, attack, and parry controllers; create `Assets/Scripts/Gameplay/CharacterDefinitionApplier2D.cs`; test `Assets/Tests/PlayMode/RumiCharacterPlayModeTests.cs`.

**Interfaces:** Existing controllers expose internal validated setters. `CharacterDefinitionApplier2D.TryApply(CharacterDefinition, out string error)` validates the complete profile, actor composition, and busy state before applying. Consume Task 1's definition contract.

- [x] Write PlayMode tests for applying the profile through the prefab and exercising the shared controllers.
- [x] Implement direct configuration APIs and atomic preflight without reflection.
- [x] Run full PlayMode regression (206/206); coverage includes Combat Lab runtime validation, movement, jump, dash, combo progression, direct damage, parry, and action teardown.

### Task 3: Rumi prototype content
**Files:** create `Assets/Data/Characters/Rumi` static assets, `Assets/Resources/Characters/Rumi_Prototype.prefab`, runtime test target, and `Assets/Tests/PlayMode/RumiCharacterPlayModeTests.cs`.

**Interfaces:** Consume Task 1–2 APIs and existing attack/combo/parry definitions. Produce a prefab with Rigidbody2D, damage receiver, hitbox, movement, jump, dash, attack, parry, and applier components.

- [x] Add tests for runtime prefab, both combo contexts, direct damage, parry, and teardown.
- [x] Author ScriptableObject assets and prefab; mark all tuning prototype-only.
- [x] Run full EditMode (52/52) and PlayMode (206/206) suites.

### Task 4: Docs, regression, and agent review
**Files:** create `docs/architecture/RUMI_CHARACTER.md`; update `docs/architecture/DATA_DRIVEN_DEFINITIONS.md` and `docs/GDD_MASTER.md`.

- [x] Document the Rumi kit, boundaries, provisional values, and open input/art/playtest gates.
- [x] Run all EditMode/PlayMode suites; record exact results.
- [x] Remove incidental ProjectSettings edits; verify all 108 asset GUIDs are unique; inspect the final diff.
- [x] Obtain SOLID, Performance, and QA reviews; resolve findings and document age/accessibility N/A limits.
- [ ] Verify feature commit and push, fast-forward/push develop, and update Trello DoD before completing the card.
