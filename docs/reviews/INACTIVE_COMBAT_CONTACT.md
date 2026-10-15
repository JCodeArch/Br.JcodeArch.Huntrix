# Inactive combat contact acceptance

Scope: `DamageReceiver2D.ResolveHit` and PlayMode health regressions, based on develop `8e63e56`.

The damage commit boundary checked target health and both configurations but did not check whether either receiver component/object remained enabled, or whether the attacker remained alive. Direct compatibility calls and pending melee contacts could therefore still change health, apply knockback and publish accepted combat/death events after a participant retired.

Resolution now rejects disabled participants and dead attackers before parry/protection/damage. Existing committed notification snapshots remain unchanged. This matches the existing projectile contract: `EnemyProjectile2D.FixedUpdate` cancels when its owner is disabled or dead; Zoey's firing entry point also requires a living attacker. This does not introduce a new policy where projectiles persist after their owner dies.

Authored tests cover disabled receiver component, disabled attacker component, inactive receiver object and inactive attacker object; all reject with unchanged target health/velocity and zero dealt/received/death events, then accept after re-enable. Another test kills the attacker through the real damage API and rejects its subsequent stale contact without effects.

Validation: `git diff --check` passed; `python scripts/test-evidence-tooling.py` passed 10 tooling tests. `bash scripts/run-unity-tests.sh` exited 1 at the missing `UNITY_EDITOR` prerequisite; no Unity import, compile, RED/GREEN or PlayMode execution occurred. Five generated PlayMode cases remain deferred under PROJECT_RULES.md. Tooling results do not validate combat. Independent review required before publication.
