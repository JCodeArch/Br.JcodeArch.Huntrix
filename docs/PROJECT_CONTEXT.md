# HUNTR/X Project Context

## Purpose and source of truth

This repository develops **HUNTR/X — Honmoon**, a Unity game. The authoritative product vision, approved decisions, open decisions, MVP scope, game loop, characters, systems, platforms, and risks are recorded in the [master GDD](GDD_MASTER.md). This file is an engineering orientation and status index; it does not override the GDD or Trello.

The Trello board [HUNTR/X — Honmoon](https://trello.com) is the master backlog. Preserve its card order and scope. Work on one card at a time; before starting the next card, verify the previous card against [Definition of Done](governance/DEFINITION_OF_DONE.md). Each completed card receives an identifiable commit, Trello evidence, and integration to `develop`.

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

Review every Pull Request from another AI or contributor against its exact Trello card and approved design before merging. Read the complete diff and changed-file list, verify Unity asset/meta references, run or inspect evidence for relevant tests, check regressions and Definition of Done, and report findings by severity with file/line evidence and required fixes. Treat author summaries and green checks as claims to verify, not substitutes for review. Do not merge a PR with unresolved blockers, missing acceptance evidence, or scope drift. Keep the backlog order and one-card-at-a-time rule intact.

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

The Mira implementation is integrated to `develop` at `2ee691189e7362b19e3819fbb9653eb4c9fd8ae7` from `feature/card-29-mira` (feature implementation commit `a1bc23b7cfa0ce7808f5a69577f72d201203bd94`). Trello card #29 is complete; its runtime, prefab, regression evidence, and independent reviews are recorded. It adds a temporary defensive field for explicitly opted-in damage receivers. Static definition validation, field lifecycle, multiple colliders/overlapping fields, current-pose checks, protected-hit resolution, attack-activation deduplication, Mira prefab integration, and Rumi regression are covered by the feature tests.

Recorded Unity 6.6.4f1 regression results: **65/65 EditMode and 245/245 PlayMode tests passed**, with zero failed, skipped, or inconclusive tests. Independent review outcomes: SOLID **approved**; Performance **approved**, with mobile CPU/allocation measurements still open; QA **approved** for the implemented scope. Actual fan integration is deferred to cards #42–50. The design and implementation report explain the limited scope and risks.

### Next planned work

Card #30, **Implementar Zoey**, is next in the recorded board order. Its requirements and acceptance must be taken from that Trello card before implementation. Prototype files present only as uncommitted external work are not accepted deliverables or evidence.

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
| Character prototypes | [Rumi](architecture/RUMI_CHARACTER.md), [Mira](architecture/MIRA_CHARACTER.md) |
| Card-specific plans/specs | [`superpowers/plans/`](superpowers/plans/) and [`superpowers/specs/`](superpowers/specs/) |
