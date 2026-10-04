# Data-Driven Definitions Implementation Plan

> **For agentic workers:** Execute tasks in order. Keep this Trello card as one feature branch and one final card commit.

**Goal:** Add six typed ScriptableObject definition contracts with stable IDs and an editor validation path, without adding gameplay content or rules.

**Architecture:** A runtime GameDataDefinition base owns a stable ID and ID validation. Six concrete ScriptableObject types give future gameplay cards distinct references and asset creation menus. An editor-only validator scans Assets/Data; save/profile runtime data remains separate from static profile templates.

**Tech Stack:** Unity 6.6.4f1, C#, Unity Test Framework 1.8.0, ScriptableObject, AssetDatabase.

**Spec:** docs/superpowers/specs/2026-10-04-data-driven-definitions-design.md

## Global Constraints

- Preserve the approved GDD; do not invent gameplay values, effects, names, scene ordering, profile limits, or age rules.
- Do not store mutable user save data or PII in ScriptableObjects.
- Do not add package dependencies, Addressables, runtime managers, or sample assets.
- Keep definitions in HuntrX.Runtime; keep AssetDatabase use inside an Editor assembly.
- Keep data authoring separate from behavior and runtime state.

## Review Focus

- Blank IDs are rejected and duplicates across definition types are reported with asset paths.
- Definitions remain separate reference types and can be created from Unity’s asset menu.
- Profile templates contain no save progress, preferences, age settings, or PII.
- Empty definition assets are valid scaffolding until later cards approve fields.
- The explicit editor scan does not add work to runtime startup or per-frame paths.

---

### Task 1: Pin definition contracts with EditMode tests

**Files:** Assets/Tests/EditMode/HuntrX.EditModeTests.asmdef; Assets/Tests/EditMode/DataDefinitionTests.cs

- [x] Tests cover six ScriptableObject types, unique asset menus, initial IDs, ID persistence across asset save/reload, and no serialized user-state fields on ProfileDefinition.
- [x] Unity compile failed first because the public runtime types did not exist.

### Task 2: Implement static definition contracts

**Files:** Assets/Scripts/Data/Definitions/GameDataDefinition.cs and six concrete type files.

- [x] Add a serialized GUID identity to the common base and six sealed, fieldless types with unique CreateAssetMenu entries.
- [x] EditMode tests passed.

### Task 3: Add ID validation and editor scan

**Files:** Assets/Scripts/Data/Definitions/DataDefinitionValidation.cs; Assets/Editor/HuntrX.Editor.asmdef; Assets/Editor/DataDefinitionAssetValidator.cs; Assets/Tests/EditMode/DataDefinitionValidationTests.cs.

- [x] Tests cover null entries, blank IDs, cross-type duplicate IDs, distinct valid IDs, and duplicate real assets with their paths.
- [x] Unity compile failed first because the validation API was missing; the test import issue was corrected before the successful run.
- [x] Implement shared validation in runtime code; keep AssetDatabase use in Editor code.
- [x] EditMode suite: 9/9 passed.

### Task 4: Document boundaries and open decisions

**Files:** docs/architecture/DATA_DRIVEN_DEFINITIONS.md; this spec and plan.

- [x] Document six types, ID persistence, explicit Editor validation, profile/save separation, and intentionally deferred fields.
- [x] Review against cards #9, #17, #18, #19–54, and #85. Existing GDD decisions remain intact.

### Task 5: Final verification and integration

- [x] Unity 6000.6.4f1: EditMode 9/9 and PlayMode 5/5 passed; no failures, skips, or C# warnings.
- [x] Self-review: responsibilities stay split between runtime definitions and explicit Editor tooling; no per-frame/startup work, and validation is linear in the definitions scanned. Source diff checked; no duplicate asset GUIDs or unintended generated files. Independent reviewer agents hit their usage limit; Antigravity has no accessible UI/CLI in this session, so no external approval is claimed.
- [ ] Commit the card once on feature/card-17-data-driven-architecture, publish, fast-forward integrate to develop, and verify origin/develop.
- [ ] Record the result and the first next card in Trello.
