# Base Movement Implementation Plan — Trello Card 19

**Goal:** Implement a reusable horizontal movement controller that consumes logical `Move` input and supports acceleration, deceleration, and observable base states without inventing game-feel tuning.

**Design:** `HorizontalMovement2D` requires and caches a `Rigidbody2D`, horizontal input value, positive serialized max-speed/acceleration/deceleration parameters, and `Idle`/`Moving` state. It updates only horizontal velocity in `FixedUpdate`; it preserves vertical velocity, clamps input to [-1, 1], accelerates toward a target when starting/speeding up in the same direction, and decelerates on release or reversal. It has no input-device bindings and no character/scene wiring.

**TDD:** PlayMode tests were added first. Unity 6000.6.4f1 produced the expected RED compile errors for missing `HuntrX.Gameplay.Movement` types. Targeted GREEN passed 4/4 movement tests. Final complete suites passed EditMode 10/10 and PlayMode 10/10, with no failures, skips, or C# warnings/errors.

**Implementation:** Added the runtime controller, four PlayMode contract tests, and `docs/architecture/HORIZONTAL_MOVEMENT.md`. Test values are synthetic fixtures only; product tuning remains open until the approved character/Combat Lab playtest. Knockback composition is an explicit follow-up for combat cards.

**Review:** System Architect self-review approved: logical input boundary and horizontal-only ownership are explicit. SOLID self-review found one cohesive controller with no new production abstraction or dependency. Performance self-review: one `GetComponent` in `Awake`, no per-physics-step allocations or searches. QA directly ran both Unity suites. Independent specialist agents hit their usage limit; Antigravity UI/CLI was unavailable in this session, so no external review is claimed.

**Integration:** [ ] Commit on `feature/card-19-base-movement`, publish, fast-forward to `develop`, verify `origin/develop`, update Trello, and confirm the first next card.