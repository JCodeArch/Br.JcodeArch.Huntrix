# Additional card 99 critical gameplay test source

Status: 13 additional scenarios authored; Unity execution NOT RUN. These files extend the existing validation suite without modifying runtime code or shared fixtures. Independent authored-source static review: APPROVED. This verdict provides no Editor compilation or runtime evidence.

| Source file | Scenarios | Risk covered |
| --- | ---: | --- |
| SpecialSequenceIntegrityTests | 5 | Eligible nearest selection, fraction excludes allies/fans, four actual attack stages, committed versus uncommitted owner invalidation, reserved charge, reentrancy and disable cancellation |
| FanHonmoonTransactionTests | 6 | Crisis hysteresis and generations, commit/event order, finite bounds, fan soul mutation reentrancy, completed rescue with duplicate resource enrollment and nested restart, food exception rollback |
| AcceptedCombatNotificationTests | 2 | Health committed before accepted notifications/death, captured receiver subscription despite callback removal, no duplicate death, no accepted notifications for rejected contacts |

The fixtures instantiate existing authored prototype prefabs. Mutable Special/rescue definitions are cloned; original assets are untouched. Dynamic bodies retain their required type while constraints prevent physics drift. Timed sequences use UnityTest frame progression with five-second real-time deadlines and restore the previous time scale in teardown. Assertions run outside runtime event callbacks, whose exception isolation would otherwise hide assertion failures.

Before runtime execution, verify prefab references and public API compilation with the pinned Unity editor. Run these alongside the existing combat, Mira, Zoey, checkpoints, pooling, replay and performance fixtures. No successful compilation, physics result, executable build or passing runtime assertion is inferred from source inspection.
