# HUNTR/X Project Context

## Purpose and source of truth

This repository develops **HUNTR/X — Honmoon**, a Unity game. The authoritative product vision, approved decisions, open decisions, MVP scope, game loop, characters, systems, platforms, and risks are recorded in the [master GDD](GDD_MASTER.md). This file is an engineering orientation and status index; it does not override the GDD or Trello.

The Trello board [HUNTR/X — Honmoon](https://trello.com/b/O5yAS8lM/huntr-x-honmoon) is the master backlog. Preserve its card order and scope. On 10 October 2026 the owner authorized parallel implementation with separate responsibilities and deferred Unity test execution until the game is nearly ready. Prepare dependent cards against an agreed contract and integrate dependencies first. Each implementation receives an identifiable commit, Trello evidence, and integration to `develop` after static review without blockers. Runtime validation remains pending under [Deferred Validation](governance/DEFERRED_VALIDATION.md); integration is not final gameplay approval. See [project rules](../PROJECT_RULES.md).

## Engineering workflow

The standard pipeline is defined in [Agent Pipeline](architecture/AGENT_PIPELINE.md):

1. Orchestrator confirms the card, scope, dependencies, and handoffs.
2. System Architect documents the contract and design decisions when architecture is affected.
3. Core Gameplay Developer implements the approved scope.
4. Pull Request Review Analyst independently reviews the full change against the card and evidence; this does not replace specialist gates.
5. SOLID Auditor reviews responsibility, cohesion, coupling, and extension.
6. Performance Engineer reviews runtime/platform cost and records unmeasured budgets as open.
7. QA Automation Engineer validates acceptance and proportional regression coverage.
8. Integration verifies the branch, commit, tests, and Trello record before completion.

The card determines which specialist gates apply. Any omitted gate needs a recorded reason; QA, Trello traceability, and verifiable integration apply to every card. Agent contracts live under [`agents/`](agents/README.md).

## Pull Request review policy

Review every Pull Request from another AI or contributor against its exact Trello card and approved design before merging. Read the complete diff and changed-file list, verify Unity asset/meta references, run or inspect evidence for relevant tests, check regressions and Definition of Done, and report findings by severity with file/line evidence and required fixes. Treat author summaries and green checks as claims to verify, not substitutes for review. Do not merge a PR with unresolved static blockers or scope drift. The owner's dated exception permits integration while Unity tests are pending; preserve dependency order and never report deferred tests as passed.

## Technical baseline

- Unity **6.6.4f1** (Unity 6000.6.4f1) and C#.
- Unity assets and `.meta` files are versioned together; GUID references must resolve.
- Gameplay data uses typed `ScriptableObject` definitions where the approved design calls for static authored data. Runtime state belongs in runtime components.
- The project has Runtime, Editor, EditMode test, and PlayMode test assemblies.
- Full design and unresolved product decisions remain in `GDD_MASTER.md`; architecture and QA documents provide system-specific contracts and evidence.

## Current implementation status

### Completed through card #28

Cards through the Rumi prototype are integrated to `develop` at the start of the Mira feature. See the GDD and architecture index for the canonical state; do not infer later features from a prototype asset alone.

### Card #29 — Mira defensive field

The Mira implementation is integrated to `develop` at `2ee691189e7362b19e3819fbb9653eb4c9fd8ae7` from `feature/card-29-mira` (feature implementation commit `a1bc26b7cfa0ce7808f5a69577f72d201203bd94`). Trello card #29 is complete; its runtime, prefab, regression evidence, and independent reviews are recorded. It adds a temporary defensive field for explicitly opted-in damage receivers. Static definition validation, field lifecycle, multiple colliders/overlapping fields, current-pose checks, protected-hit resolution, attack-activation deduplication, Mira prefab integration, and Rumi regression are covered by the feature tests.

Recorded Unity 6.6.4f1 regression results: **65/65 EditMode and 245/245 PlayMode tests passed**, with zero failed, skipped, or inconclusive tests. Independent review outcomes: SOLID **approved**; Performance **approved**, with mobile CPU/allocation measurements still open; QA **approved** for the implemented scope. Actual fan integration is deferred to cards #42–50. The design and implementation report explain the limited scope and risks.

### Card #30 — Zoey ranged prototype

Status: **IMPLEMENTED — pending Unity validation** on `feature/card-30-zoey`; published as [PR #1](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/1), commit `f9be88fcf8deba3d96d30df414e5e68f62f5c949`. **Merged to develop** in `7230774` after independent static review and the owner's explicit authorization to defer Unity tests on 10 October 2026. The prototype adds a validated ranged definition, configurable immediate precision shot, authored character/data assets, and presentation events. Explicit world-space aim supports airborne targets; shared hurtbox resolution preserves faction, parry, dash invulnerability, and Mira protection rules.

Independent static review is **approved** for the submitted implementation. **26 new test cases are authored (11 EditMode and 15 PlayMode), but have not been run.** No Unity compilation, runtime regression, Combat Lab playtest, or target-device performance result is claimed. The previous card #29 totals above are historical evidence and do not validate this feature. See [Zoey architecture](architecture/ZOEY_CHARACTER.md) and [card #30 review](reviews/2026-10-10-card-30-zoey.md).

### Card #31 — CharacterManager

**IMPLEMENTED and merged to develop — Unity validation pending.** [PR #3](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/3), feature commit `c40886ea1dafb1f3164fba97eb1945bc63ae03dc`, merge `44338f9`. One local slot owns cached character instances, active identity/state, checkpoint binding and fresh respawn. Individual health is preserved when switching; stale input/contact state is cleared. See [manager contract](architecture/CHARACTER_MANAGER.md).

### Card #32 — Character switching

**IMPLEMENTED and merged to develop — Unity validation pending.** [PR #4](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/4), feature commit `498594ce6f2d4beecc2902bd47f7efadf3b03e4a`, merge `8ed8339`, after #31. Logical direct/circular selection uses a defensive roster and delegates transitions to the manager. `HuntrXPlayerSlot_Prototype` composes manager/controller/local flow and Rumi/Mira/Zoey references, with explicit Configure/TrySpawn initialization. No physical input, auto-spawn or Combat Lab demo is claimed. See [switching contract](architecture/CHARACTER_SWITCHING.md).

Both cards received independent static code/SOLID and asset/performance reviews, with corrected lifecycle findings documented in [code review](reviews/2026-10-10-character-management.md) and [asset/performance review](reviews/2026-10-10-character-management-assets-performance.md). Compilation, Unity asset import, physics, automated suites, scene inspection and profiling were not executed. This round prioritized implementation and recorded scenarios without adding a new management/switching suite. Runtime acceptance and qualified cooperative play remain pending.

### Cards #33–#40 — enemy and encounter prototypes

**IMPLEMENTED — Unity validation pending.** The owner authorized this round through Jinu (#40), with six specialists and separate feature branches. Cards #33–#39 have verified merges to develop; #40 packages Jinu, EnemyLab and the final integration documentation in its own feature. Its published PR/merge is recorded in Trello after integration.

| Card | Verified integration | Feature commit |
|---|---|---|
| #33 | [PR #6](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/6) | `eddf5c1` |
| #34 | [PR #7](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/7) | `9cdff28` |
| #35 | [PR #8](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/8) | `b487350` |
| #36 | [PR #9](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/9) | `0e0a925` |
| #37 | [PR #10](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/10) | `3489a27` |
| #38 | [PR #11](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/11) | `e4cbdc1` |
| #39 | [PR #12](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/12) | `56ee765` |
| #40 | `feature/card-40-jinu-arc`; published evidence in Trello | Commit of this feature |

The round adds terrestrial melee; flying pursuit, altitude, dives and bounded projectiles; healer/protector/drainer; bounded hordes; alternating elite attacks; a two-phase miniboss; four individual Saja tactics; and Jinu combat-stage evolution with authored narrative cue events. Values, geometric visuals and narrative cues are prototypes. Drainer uses health damage, with Honmoon integration future; final dialogue/cinematics and narrative fidelity remain open.

Independent [static review](reviews/2026-10-10-enemies-33-40.md) and asset audits found no remaining blocking issue in the reviewed snapshot. No Unity compilation/import, tests, physics/visual playtest or device measurement was executed. Existing #29 test results do not validate these changes. See [integration guide](enemies/INTEGRATION_GUIDE.md) and [deferred scenarios](governance/DEFERRED_VALIDATION.md).

Open `Assets/Scenes/EnemyLab.unity` during final validation to inspect the automatic terrestrial horde encounter. It explicitly wires a player slot, floor and spawn positions. The logical initial player has no SpriteRenderer or physical input mapping; appearance and execution remain unverified. See [EnemyLab composition](architecture/ENEMY_LAB.md).

### Next planned work

The requested implementation scope ends at #40. The next backlog item is #41; confirm its exact scope in Trello before new implementation. Preserve deferred Unity compilation, suites, regressions and visual validation. Physical controls, final art/audio/balance, final narrative content and qualified cooperative play remain open for their corresponding cards.


## Context and decision boundaries

- Do not turn provisional prototype values into final balance decisions.
- Do not infer fan protection, faction, rescue, reward, co-op, save, selection, or platform behavior outside the cards that define them.
- Do not claim mobile performance targets without device measurements.
- Keep open decisions open and link changes to their source card/spec.
- Preserve uncommitted work owned by another tool or contributor; inspect it separately and accept it only after the same card gates pass.

## Documentation map

| Topic | Source |
|---|---|
| Product scope and decisions | [GDD Master](GDD_MASTER.md) |
| Agent handoffs | [Agent Pipeline](architecture/AGENT_PIPELINE.md), [agent contracts](agents/README.md) |
| Completion gates | [Definition of Done](governance/DEFINITION_OF_DONE.md) |
| Data-driven architecture | [Data-Driven Definitions](architecture/DATA_DRIVEN_DEFINITIONS.md) |
| Character prototypes | [Rumi](architecture/RUMI_CHARACTER.md), [Mira](architecture/MIRA_CHARACTER.md), [Zoey](architecture/ZOEY_CHARACTER.md) |
| Card-specific plans/specs | [`superpowers/plans/`](superpowers/plans/) and [`superpowers/specs/`](superpowers/specs/) |
