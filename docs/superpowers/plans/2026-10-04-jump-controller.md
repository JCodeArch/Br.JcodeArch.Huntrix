# Jump Implementation Plan — Trello Card 20

**Goal:** Implement a reusable 2D jump controller with configurable coyote time, jump input buffering, per-body gravity, and variable height by early release.

**Design:** `JumpController2D` requires/caches `Rigidbody2D`; receives `PressJump`, `ReleaseJump`, and `SetGrounded(bool)` from future input/ground-sensing layers. It applies a configured vertical velocity, preserves horizontal velocity, sets only that body's gravityScale, buffers input through landing until expiry, and applies a configured vertical cut once on early release. No physical bindings, ground layers, player assets, or gameplay values are selected.

**TDD and QA:** PlayMode tests were added first. Unity 6000.6.4f1 produced expected RED compile errors while the runtime component was absent. Targeted GREEN passed 8/8 jump tests. Final complete suites passed EditMode 10/10 and PlayMode 18/18, with no failures, skips, or C# warnings/errors.

**Implementation:** Added the runtime component, jump contract tests, and `docs/architecture/JUMP_CONTROLLER.md`. Values in test fixtures are synthetic; project tuning remains open for character/Combat Lab playtesting.

**Review:** System Architect self-review approved: grounded signal, timers, velocity axes, and per-body gravity ownership are explicit. SOLID self-review found one cohesive jump controller with no new dependency. Performance self-review: cached Rigidbody2D, fixed-step scalar operations, no per-step allocations or scene searches. Independent specialist agents hit their usage limit; Antigravity UI/CLI was unavailable in this session, so no external review is claimed.

**Integration:** [ ] Commit on `feature/card-20-jump`, publish, fast-forward to `develop`, verify `origin/develop`, update Trello, and confirm next item.