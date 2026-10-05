# Br.JcodeArch.Huntrix

## HUNTR/X — Honmoon

AI-assisted game development foundation for HUNTR/X, focused on modular Unity 6 + C# engineering, agent orchestration, gameplay systems, architecture, performance and automated QA.

## Engineering Agent Pipeline

\`\`\`
Orchestrator
    -> System Architect
    -> Core Gameplay Developer
    -> Pull Request Review Analyst
    -> SOLID Auditor
    -> Performance Engineer
    -> QA Automation Engineer
    -> Integration
\`\`\`

## Repository Goals

- Modular and maintainable Unity architecture.
- Data-driven systems using ScriptableObjects where appropriate.
- Explicit contracts between agents and software modules.
- Automated quality gates.
- Performance-conscious runtime code.
- Testable gameplay logic.
- No hidden scope expansion.

## Current status

The project is in iterative gameplay development. Trello is the master backlog; cards are completed in planned order, one at a time, with a dedicated commit, applicable agent reviews, QA evidence, and verified integration to `develop`. The current completed gameplay milestone is Rumi (card #28); Mira (card #29) has an implementation branch with its runtime, prefab, regression evidence, and independent reviews, pending final integration and Trello closure.

Start with [the full project context](docs/PROJECT_CONTEXT.md), [master GDD](docs/GDD_MASTER.md), [Definition of Done](docs/governance/DEFINITION_OF_DONE.md), and [agent pipeline](docs/architecture/AGENT_PIPELINE.md). Character-specific contracts are in `docs/architecture/` and card plans/specs are in `docs/superpowers/`.

All incoming Pull Requests receive a strict, independent review against their Trello card, full diff, Unity asset references, test evidence, and Definition of Done before merge.

## Git LFS

Git LFS tracks source art, model, audio, and video formats listed in the root .gitattributes. After cloning this repository, run git lfs install --local once to configure repository-local filters and hooks. Unity scenes, prefabs, assets, metadata, and small raster images remain in regular Git. Existing history is not migrated automatically.
