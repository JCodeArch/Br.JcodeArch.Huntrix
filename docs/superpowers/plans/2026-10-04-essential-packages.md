# Essential Unity Packages Plan — Card 13

**Goal:** Configure the seven packages named by Trello card 13 for the pinned Unity Editor 6000.6.4f1, without implementing gameplay behavior or moving later backlog items.

**Scope:** Keep template-inherited Input System 1.20.0, 2D Animation 16.0.1, Timeline 6.6.0, and Test Framework 1.8.0. Enable Editor-bound Cinemachine 6.6.0. Add released Localization 1.5.13 and let UPM resolve its transitive Addressables graph. TMP is already shipped through uGUI 2.6.0 in this project; do not add deprecated standalone `com.unity.textmeshpro`.

**Sequence:**
1. Orchestrator confirms card 12 DoD and #13 is first pending; preserve Trello order.
2. System Architect validates package choices, version compatibility, and dependency graph.
3. Core Gameplay Developer updates dependency declarations only; gameplay implementation is N/A.
4. SOLID Auditor records N/A because no project gameplay architecture/code is changed.
5. Performance Engineer records runtime benchmarking N/A because no workload or budget exists; note dependency footprint unmeasured.
6. QA Automation Engineer resolves all listed packages and performs a clean-cache Editor import/compile check at 6000.6.4f1.
7. Integration checks branch, minimal diff, lock consistency, updates the card, commits, pushes, integrates to develop, and rechecks DoD before choosing the next Trello card.

**Completion evidence:** all seven package needs are direct dependencies or explicitly justified as uGUI/Core packages; exact versions are locked with complete transitive dependencies; clean-cache package resolution and import succeed with no package, C#, assembly, shader, or import errors; reviewers' pass/N/A decisions and commit are recorded in Trello.
