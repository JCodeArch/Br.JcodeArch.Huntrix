# Ground Dash Implementation Plan — Trello Card 21

**Goal:** Add ground-start dash, an invulnerability state, and feedback hooks without adding unapproved tuning/assets or aerial behavior.

**Design:** `DashController2D` receives horizontal direction and grounded state externally. It owns finite positive speed/duration, uses a source-owned horizontal-velocity override on `HorizontalMovement2D`, preserves vertical velocity, exposes `IsDashing`/`IsInvulnerable`, and emits start/end state events. On completion or disable it clears its own override so ordinary movement resumes. The movement motor runs earlier in fixed-step order so duration is deterministic at physics-step resolution. A sub-step duration receives one movement tick. No cooldown, visual/audio asset, damage integration, or aerial activation is added.

**TDD:** The first targeted run failed to compile because the APIs were absent. A short-duration regression then failed because the dash ended before its first physics tick. Direction diagnostics, configured-step expiry, and disabled-component tests were also run red before their fixes. Final targeted dash PlayMode: 6/6 passed.

**Implementation:** Extend the existing horizontal motor with one owner-scoped external velocity override and execution order -100; add the dash controller, tests, and `docs/architecture/GROUND_DASH.md`. Tuning in tests is synthetic; final game timing/feedback stays open.

**Review:** [x] System Architect — confirmed timer order and exact tick behavior; [x] SOLID — confirmed disabled components cannot acquire ownership; [x] Performance — cached references, no hot-path allocations, deterministic timer/motor order; [x] QA/documentation — Unity regression suite and scope docs verified. Antigravity was checked but is not available as an app/window in this session; no review from it is claimed.

**QA:** Unity 6000.6.4f1: EditMode 10/10 and PlayMode 25/25, zero failures/skips and zero C# compiler warnings/errors. `git diff --check` passed; all 61 Unity asset meta GUIDs are unique.

**Integration:** [ ] Commit on `feature/card-21-dash`, publish, fast-forward to `develop`, verify `origin/develop`, update Trello, and confirm the next item in original list order.