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

The project is in iterative gameplay development. Trello is the master backlog. The current implementation round reaches card #40 (Jinu), with a dedicated feature branch and descriptive commits for each card, independent static review and sequential integration to `develop`. Unity compilation, import, EditMode/PlayMode, visual playtests and device profiling remain pending under the owner's 10 October 2026 instruction to defer tests until the final phase. Implemented prototypes include characters/selection and enemies, support, hordes, elites, miniboss, Saja rivals and Jinu combat-stage/narrative cue hooks. Final art, physical controls, story/cinematics and balance remain separate work. Start with [project context](docs/PROJECT_CONTEXT.md), [master GDD](docs/GDD_MASTER.md), [integration guide](docs/enemies/INTEGRATION_GUIDE.md), [deferred validation](docs/governance/DEFERRED_VALIDATION.md) and [project rules](PROJECT_RULES.md). The authored `Assets/Scenes/EnemyLab.unity` composes an automatic ground-horde prototype; its execution has not been verified in Unity.

Start with [the full project context](docs/PROJECT_CONTEXT.md), [master GDD](docs/GDD_MASTER.md), [Definition of Done](docs/governance/DEFINITION_OF_DONE.md), and [agent pipeline](docs/architecture/AGENT_PIPELINE.md). Character-specific contracts are in `docs/architecture/` and card plans/specs are in `docs/superpowers/`.

All incoming Pull Requests receive a strict, independent review against their Trello card, full diff, Unity asset references, test evidence, and Definition of Done before merge.

## Git LFS

Git LFS tracks source art, model, audio, and video formats listed in the root .gitattributes. After cloning this repository, run git lfs install --local once to configure repository-local filters and hooks. Unity scenes, prefabs, assets, metadata, and small raster images remain in regular Git. Existing history is not migrated automatically.
