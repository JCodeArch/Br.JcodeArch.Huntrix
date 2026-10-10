# Enemy foundation — card #33

Source: [33 — Criar inimigo terrestre básico](https://trello.com/c/5l9eZvVd), “Inimigo melee para validar combate.” Implementation is a provisional, original geometric demon prototype with an automatic explicit-target chase/melee loop, not final art or balance.

## Authoring and runtime

Instantiate `Resources/Enemies/GroundDemon_Prototype` as an independent scene root and call its `EnemyAgent2D.TrySetTarget(livingOpposingDamageReceiver)`. The authored prefab contains a dynamic rigidbody, capsule, Demon-faction receiver (100 health), hurtbox, protection opt-in, agent, **one** GroundEnemyController and a geometric visual. Idle has no target; it does not search the scene or assume a singleton player. Each local player slot can be supplied explicitly; automatic co-op target selection is future integration.

`EnemyDefinition` owns `Attack`, `MoveSpeed`, `DetectionRange`, `AttackRange`, `TelegraphDuration`, `RecoveryDuration`; all positive finite values, attack range <= detection range. Ground values: speed 2, detection 12, attack range 1.5 world units, telegraph 0.5s, recovery 0.7s, damage 8. These are authored prototype values, not final balance.

The automatic GroundEnemyController calls `Tick(Time.fixedDeltaTime)` once, moves toward the target in Idle/Chasing, begins a telegraph when target is in range and visible, and resolves melee in Attacking. State transitions are Idle → Chasing → Telegraph → Attacking → Recovering → Idle; invalid target cancels the action, accepted death enters Dead. Ground movement supplies horizontal velocity, preserves vertical gravity, and relies on physics for wall collision. No pathfinding, cliff avoidance, terrain jump or navigation graph is implemented.

A melee attack is one instant box contact at the end of the readable telegraph. `AttackDefinition.ActiveDuration` remains valid shared attack metadata but does not create a persistent damage window in this enemy controller. The facing direction is captured at telegraph start so moving away can dodge the authored hitbox. Each receiver resolves once per attack, through `Hurtbox2D.ResolveHit` for faction, parry, Mira/other protection and dash invulnerability. Damage/knockback come from the shared AttackDefinition.

## Composition API

Specialized controllers compose `EnemyAgent2D` **instead of** GroundEnemyController. Exactly one controller owns Tick/movement/attack phases; do not attach both drivers. Public properties: `Definition`, `Self`, `Target`, `State`, `HasValidTarget`, `IsConfigurationValid`. Operations: `TryConfigure(definition,out error)`, `TrySetTarget(receiver)` (null explicitly unbinds), `Stop()`, `Tick(dt)`, `MoveTowardsTarget(bool flying)`, `MoveTowardsPoint(point,bool flying)`, `TryBeginTelegraph()`, `TryExecuteMelee()`, `BeginRecovery()`, `HasLineOfSight(point)`. Configure accepts Idle/Chasing, preserves current target and health; rejects active/dead action. Stop cancels timer/motion but retains the target and never revives a dead actor. Tick rejects invalid/negative time.

Agent initializes after the receiver via execution order 100. Drivers that configure the agent in Awake must run later (e.g. 200). Disable unsubscribes death and cancels motion/timers; owned projectile controllers additionally cancel their projectiles. No health reset/pooling is introduced here.

Contact and LOS queries use 32-entry buffers and explicit trigger-inclusive filters, regardless of global trigger settings. Saturation is conservative (no damage/LOS). Melee ignores source-root colliders, deduplicates receivers and rejects solid occlusion. LOS ignores character hurtboxes and unrelated triggers; ordinary non-trigger environment geometry blocks. The authored mask includes all layers, including solid blockers.

`StateChanged`, `ImpactOccurred` and `ParryOccurred` isolate subscriber exceptions. Recovery/resolution is reserved before callbacks; callbacks that Stop/disable/kill the actor cancel subsequent contacts. Visual telegraph is yellow, attacking white, dead gray; flashes/audio/animation/accessibility are not validated by this prototype.

## Deferred verification

Unity import/compilation, executable tests, combat/playtest and device performance are deferred to the final validation phase requested by the user. Static review and prefab/GUID checks are not a replacement for them. Pending scenarios: chase/range/occlusion; finite configuration; target death/disable/rebind; readable telegraph and dodge; hit deduplication; shield/parry/dash; death/disable during callback; saturation; scene unload; 1–3 explicit targets and measured mobile CPU/GC. There is no executed-test or final DoD claim.
