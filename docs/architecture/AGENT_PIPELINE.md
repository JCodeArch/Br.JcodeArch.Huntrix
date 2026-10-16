# HUNTR/X Agent Pipeline

## Purpose

Define how engineering agents collaborate without bypassing architecture or quality gates.

## Standard Flow

1. Orchestrator receives requirement.
2. Architect defines contracts and architecture.
3. Core Developer implements the approved contract.
4. Pull Request Review Analyst independently reviews scope, the complete diff, requirement evidence, serialized Unity references, regressions, and readiness for specialist reviews. Findings route to the responsible agent; the analyst does not replace specialist gates.
5. SOLID Auditor reviews structure and maintainability.
6. Performance Engineer reviews runtime risks.
7. QA Automation Engineer defines tests and executes applicable verification after fixes and required reviews, recording Unity/runtime execution deferred by the owner as pending.
8. Pull Request Review Analyst enters its separate authorized integration phase only after required gates pass, revalidates the current PR head/base and branch protections, and verifies the merge to `develop`.
9. Orchestrator records the verified review, approval and merge evidence in Trello/continuity. Static integration keeps the status **Implementado — validação pendente** where runtime gates remain open; merge alone does not complete a card.

## Rejection Rules

- Architectural issue -> System Architect.
- Implementation issue -> Core Developer.
- Structural/code-quality issue -> Core Developer, or Architect when the contract itself is wrong.
- Performance issue -> Core Developer; architectural performance issues return to Architect.
- Test failure -> responsible implementation owner after root-cause analysis.
- Pull Request Review Analyst findings -> the responsible specialist by finding type; the analyst rechecks the correction before integration.

## Core Principle

Agents must not silently change another agent's contract or introduce unrequested game mechanics.
