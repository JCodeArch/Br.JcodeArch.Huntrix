# Combat Lab Scene Implementation Plan — Trello Card 18

**Goal:** Add a separate Unity scene for later gameplay-system tests without changing the startup scene or inventing level content.

**Design:** `Assets/Scenes/CombatLab.unity` is a dedicated host scene in build settings after `SampleScene`. It uses the existing 2D camera contract (`orthographicSize = 5.4`, x/y at zero, no rotation, camera z at -10), template Global Light 2D, and one active `GameBootstrap` with an empty composition. No arena geometry, test actors, gameplay, art, or follow camera are added.

**TDD:** Added EditMode and PlayMode acceptance tests first. The first EditMode run failed as expected because `CombatLab` was absent from build settings. After implementation, the complete EditMode suite passed 10/10 and the PlayMode suite passed 6/6 on Unity 6000.6.4f1, with no failures, skips, or C# warnings.

**Implementation:** Duplicated the approved scene composition into a distinct Unity scene with a unique asset GUID; kept `SampleScene` unchanged and first in EditorBuildSettings; placed CombatLab second. Added `docs/architecture/COMBAT_LAB.md` with use and deferred scope.

**Self-review:** Architecture preserves scene-owned composition and startup behavior. SOLID and runtime performance changes are N/A: this card adds scene data and test-only assertions, no production code or per-frame work. Independent reviewer agents were unavailable after hitting their usage limit; Antigravity UI/CLI was not accessible in this session, so no external approval is claimed.

**Integration:** [ ] Commit on `feature/card-18-combat-lab`, publish, fast-forward to `develop`, verify `origin/develop`, and record result/next item in Trello.