# HUNTR/X Engineering Agents

This directory contains the operational contracts for the engineering agent team.

## Agents

- 00-game-orchestrator
- 01-system-architect
- 02-core-gameplay-developer
- 03-solid-auditor
- 04-performance-engineer
- 05-qa-automation-engineer
- 06-pull-request-review-analyst

Each agent defines:

- role and mission;
- input/output contract;
- responsibilities;
- constraints;
- quality gates;
- failure conditions;
- handoff rules.

## PR review and integration ownership

Agent 06 owns independent PR review and, under the owner's authorization, a separate verified merge phase to `develop`. Its [contract](06-pull-request-review-analyst.md) defines head-specific re-review, required checks, GitHub identity limits and protected-branch gates. The Orchestrator records the resulting evidence and pending validation; no unattended monitoring is implied.
