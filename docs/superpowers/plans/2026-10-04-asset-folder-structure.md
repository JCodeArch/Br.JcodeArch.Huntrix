# Asset Folder Structure Plan — Card 14

**Goal:** Create the missing top-level Unity asset folders requested by Trello card 14.

**Existing state:** `Assets/Scenes` already contains `SampleScene.unity`; `Assets/Settings` contains URP, input, and template resources; `Assets/Welcome` contains the official template welcome assets. Preserve all of them and their `.meta` files.

**Scope:** Add only `Assets/Scripts`, `Assets/Prefabs`, `Assets/Art`, `Assets/Audio`, `Assets/Data`, and `Assets/Tests`. Keep `Assets/Scenes` as-is. Do not add nested taxonomies, scripts, prefabs, art/audio/data content, assembly definitions, test fixtures, or move template files. Let Unity generate durable folder `.meta` files; add a small `.gitkeep` marker so Git preserves each empty directory in clones.

**Pipeline:** Orchestrator confirms #13 DoD and #14 order; System Architect approves minimal layout; Core Gameplay Developer, SOLID Auditor, Performance Engineer, and Design/Audio review are N/A for empty folders; QA Automation Engineer verifies exact paths, Unity-generated metadata/GUID uniqueness, preserved existing scene/settings/template assets, and Unity 6000.6.4f1 import; Integration checks focused diff, commits this card on its feature branch, publishes and fast-forward integrates to develop, then confirms DoD before the next card.

**Acceptance:** all requested categories exist under `Assets`, including the pre-existing `Scenes`; each new folder has its generated `.meta` and a `.gitkeep` marker; template files and project settings remain unchanged; Unity import succeeds without folder/meta/GUID or compile errors; only this card's folder metadata and plan are committed.
