# HUNTR/X Agent Pipeline

## Purpose

Define how engineering agents collaborate without bypassing architecture or quality gates.

## Standard Flow

1. Orchestrator receives requirement.
2. Architect defines contracts and architecture.
3. Core Developer implements the approved contract.
4. SOLID Auditor reviews structure and maintainability.
5. Performance Engineer reviews runtime risks.
6. QA Automation Engineer defines and executes tests.
7. Orchestrator integrates only when required gates pass.

## Rejection Rules

- Architectural issue -> System Architect.
- Implementation issue -> Core Developer.
- Structural/code-quality issue -> Core Developer, or Architect when the contract itself is wrong.
- Performance issue -> Core Developer; architectural performance issues return to Architect.
- Test failure -> responsible implementation owner after root-cause analysis.

## Core Principle

Agents must not silently change another agent's contract or introduce unrequested game mechanics.
