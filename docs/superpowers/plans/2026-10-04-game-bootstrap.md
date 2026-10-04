# GameBootstrap Plan — Trello Card 16

**Goal:** Add an explicit scene-owned composition root that initializes referenced runtime systems in a predictable order, without creating gameplay systems or global service state.

**Approved scope:** `GameBootstrap` is attached to the existing startup scene. It holds an ordered serialized list of `MonoBehaviour` components implementing `IGameInitializable`. It validates the entire list before invoking any initializer, reports invalid/duplicate references clearly, and stops at the first initialization exception. Initialization happens once in `Start`, after active scene objects have completed `Awake`. No singleton, `DontDestroyOnLoad`, async loading, DI framework, or fabricated game services. The list remains empty until later approved cards provide systems.

**Implementation:** Add a small runtime assembly under `Assets/Scripts`, contract and bootstrap scripts with Unity metadata, a PlayMode test assembly to verify order/exactly-once/invalid-list behavior, and a serialized bootstrap object in the existing `SampleScene`. Preserve current camera/light and build settings. Document lifecycle, ordering, errors, empty composition today, and multi-scene limitations.

**Pipeline:** Orchestrator confirms #15 DoD and #16 order; System Architect reviews contract; Core Gameplay Developer implements; SOLID reviews composition boundary; Performance reviews one-time startup cost; QA runs focused Unity tests plus project import/compile and scene smoke where editor capability permits; Integration commits/pushes feature and fast-forward integrates into develop. Antigravity review will be attempted only if an authenticated usable session is available; otherwise record the blocker. No art/design review applies.

**Verification:** TDD first for initialization order, single initialization, and invalid component configuration. Run focused PlayMode tests using the installed Unity Test Framework, then Unity 6000.6.4f1 batch import/compile and inspect scene serialization/build settings. Record any unavailable manual Editor smoke separately; do not claim it ran.

**Acceptance:** Startup scene contains one valid enabled bootstrap; systems are explicitly ordered and validated before any are initialized; invalid configuration prevents partial start; no global lifetime or gameplay assumptions are introduced; tests/import/compile pass; documentation and Trello DoD include findings/evidence; one card-specific commit is published on the feature branch and `develop`.

**TDD red observed:** Before adding the runtime contract/class, Unity 6000.6.4f1 failed test-assembly compilation with unresolved HuntrX.Bootstrap.GameBootstrap and IGameInitializable symbols (expected).

**TDD green and QA evidence:** Unity 6000.6.4f1 ran 5 PlayMode tests: 5 passed, 0 failed, 0 skipped. The suite opened SampleScene and confirmed exactly one active bootstrap. Unity batch import compiled both runtime and test assemblies; log had no C# compile/import errors. SampleScene remains enabled in EditorBuildSettings; 31 asset .meta GUIDs were checked with no duplicates. SOLID review APPROVED with one P2 follow-up documented in docs/architecture/GAME_BOOTSTRAP.md; Performance review APPROVED (one-time startup only).
