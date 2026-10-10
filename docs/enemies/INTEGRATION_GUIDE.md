# Enemy prototype integration — cards #33–40

## Scope and evidence boundary

This guide coordinates the shared enemy foundation and its specialized prototype drivers. The Trello cards describe categories, not final numeric balance or complete production kits:

| Card | Recorded requirement | Implementation boundary |
| --- | --- | --- |
| [33](https://trello.com/c/5l9eZvVd) | Basic ground melee enemy for combat validation | Shared agent and ground driver |
| [34](https://trello.com/c/tYNazBxA) | Flying enemies with dive, pursuit, projectiles and altitude variation | Separate flight data/driver and bounded projectile |
| [35](https://trello.com/c/lq4Getd6) | Healer, protector and soul drainer | Support actions using explicit receivers/sources |
| [36](https://trello.com/c/R8iF7ol3) | Group system with performance limits | Bounded spawn ownership and cadence |
| [37](https://trello.com/c/47dVtgEw) | Advanced patterns and telegraphs | Specialist driver over shared agent |
| [38](https://trello.com/c/0wiyVwvz) | First enemy with simple phases | Explicit phase driver over shared agent |
| [39](https://trello.com/c/pciEsGBR) | Rivals with individual identities and mechanics | Distinct authored prototype identities and patterns |
| [40](https://trello.com/c/44I8S338) | Combat arc and narrative evolution | Observable prototype transitions; final story remains open |

Authored values and patterns are integration fixtures. Character names or state changes do not establish approved dialogue, film adaptation, art or narrative sequencing. Soul drain integration with fans and Honmoon belongs to the later fan/resource cards; numeric enemy combat is not proof that those systems exist.

The owner authorized parallel implementation and deferred Unity execution. Record these cards as implemented with validation pending until execution and integration evidence exists. Review and structural inspection do not establish runtime correctness, completed DoD, a measured platform budget or finished gameplay.

## Shared foundation contract

`HuntrX.Data.EnemyDefinition` supplies an `AttackDefinition Attack`, `MoveSpeed`, `DetectionRange`, `AttackRange`, `TelegraphDuration` and `RecoveryDuration`, with `IsValid(out error)`. Static authored definitions contain no target, health, timers or phase progress. Validate finite ranges, valid nested attack data and stable IDs; authoring fixtures must satisfy the same validator as existing definitions.

`HuntrX.Gameplay.Enemies.EnemyAgent2D` owns the current explicit target and action state. The locked API is:

| Member | Intended integration |
| --- | --- |
| `Definition`, `Self`, `Target`, `State` | Read current shared state; do not bypass the receiver health model |
| `TryConfigure(definition, out error)` | Validate before committing authored configuration |
| `TrySetTarget(DamageReceiver2D target)` | Supply a live, opposing, active target through the encounter integration |
| `Stop()` | Stop motion/actions; never revive a dead owner |
| `TryBeginTelegraph()` | Enter the visible preparation boundary for a valid attack |
| `TryExecuteMelee()` | Resolve a prepared melee action through the existing combat pipeline |
| `Tick(float dt)` | Advance finite, nonnegative scaled action time once per driver tick |
| `MoveTowardsTarget(bool flying = false)` | Move under the selected movement mode without duplicating target lookup |

States are `Idle`, `Chasing`, `Telegraph`, `Attacking`, `Recovering` and `Dead`. `GroundEnemyController2D` drives the basic ground actor. Support, elite, mini-boss and rival drivers compose an agent **without** the ground driver, so only one controller advances time and movement for each actor. Flying behavior uses its specialized definition/controller and the foundation's explicit target model.

Missing/inactive/dead/self/same-faction targets cannot justify movement or damage. No global scene `Find` or singleton active character is required. The encounter/input integration sets a target for each enemy; target selection, threat weighting and cooperative difficulty remain product decisions. When a player switches or respawns, that integration must supply the new runtime receiver rather than retaining the deactivated old instance.

## Combat and support seams

All hostile hits resolve through `Hurtbox2D.ResolveHit`/`DamageReceiver2D`, preserving current self/faction rejection, dash invulnerability, parry precedence and explicit protection. Multiple hurtbox colliders cannot multiply one accepted attack. A bounded query must treat saturation conservatively rather than silently claiming it found all contacts. Solid obstacles and target hits require an explicit policy for each ranged action; projectiles must sweep their travelled segment and have finite lifetime/range.

Accepted damage, parry, protected and rejected contacts are distinct outcomes. VFX, drain events, health restoration or phase rewards must not treat overlap alone as damage. If a drain prototype publishes an accepted-contact event, document that it does not alter fans, Honmoon or an invented resource economy. Do not add self-healing from lost target health unless that mechanic is explicitly approved.

Healing is bounded by maximum health and applies only to valid living receivers with explicit eligible source/faction rules. It must not revive a dead actor or operate after the source is disabled/dead/out of range. Protection is registered per live source through the existing opt-in contract. Ending one source cannot remove another source's protection, and a stale health/shield snapshot must not overwrite state changed by another actor.

## Lifecycle, movement and spawn ownership

Every driver rejects commands when disabled, unconfigured or dead. Death/disable clears pending telegraphs, queued actions and movement so delayed execution cannot produce a post-death hit. Target changes during preparation are either cancelled or explicitly revalidated; they cannot damage a stale deactivated receiver. Only one driver owns each Rigidbody's movement. Configure changes during an active action require a defined rejection/cancellation boundary.

Projectiles own their bounded runtime lifetime and collision state. Their launch owner remains the original receiver for the existing damage/faction path; target position may guide aim at launch, but no invisible homing rule is implied. Source teardown policy must be explicit and inspected, including already launched projectiles.

Horde controllers own only actors they instantiate. Spawn runtime enemy roots independently from the horde controller so `transform.root` self/faction logic does not classify an entire spawn group as the same combat actor. Never parent player and enemy actors under a shared runtime hierarchy without reviewing those existing root comparisons.

Concurrency, total-spawn count, cadence and query capacity are finite authored limits. Track live owned instances, release dead/disabled/destroyed instances from the concurrency budget consistently, and do not spin indefinitely on an invalid prefab or failed spawn. Disable/destruction cancels future spawning and synchronously stops owned actors before deferred Unity destruction. Other encounter controllers' actors and authored prefab assets must remain untouched.

Bounded counts are safety limits, not measured performance results. Avoid per-tick global discovery, unbounded growing collections and duplicate driver updates. Pooling, mobile allocation/CPU budgets and minimum-device frame targets require later measurements; do not claim them from static inspection.

## Asset integration

Each new script, folder, ScriptableObject and prefab under `Assets` needs its corresponding `.meta` with a unique stable GUID. Definitions need unique authoring IDs, valid nested references and clear provisional naming. Prefabs require a configured dynamic damage receiver, correct faction, Rigidbody/hurtbox composition, one behavior driver, and explicit dependencies. A referenced prefab file is not evidence that its behavior was exercised.

Profiles for individual rivals must differ in documented prototype behavior, rather than only their display names. Complete art, audio, transitions and canonical narrative remain separate production work. Generic Combat Lab targets can demonstrate combat integration once executed; they cannot stand in for real fans or a finished encounter.

## Deferred Unity validation matrix

| Area | Required execution evidence |
| --- | --- |
| Foundation | Invalid definitions/dt, owner/target liveness, faction/self rejection, disabled commands, exactly one driver, preparation/recovery, target changes |
| Damage | Range/obstruction, multiple colliders, query saturation, damage once, parry/dash/protection, death/event ordering |
| Flying/projectiles | Pursuit/altitude/dive phases, swept collision, zero/nonfinite aim, expiry, owner/target teardown and repeated contacts |
| Support | Capped healing, no revival, invalid source, range exit, overlapping protection sources, protected drain contact and no invented economy |
| Horde | Concurrent/total/cadence limits, failed spawn, zero/invalid budgets, actor root separation, death release and disable cleanup |
| Elites/bosses | Telegraph visibility signal, phase thresholds/boundaries, no post-death attack, reset/disable and shared-agent ownership |
| Saja/Jinu | Distinct authored identity/pattern, narrative-state event boundaries and no assumed final story |
| Player integration | Switching/respawn retarget, inactive cached character exclusion and two independent player slots |
| Regression | Existing Rumi/Mira/Zoey, manager/switching, checkpoints, shared hurtboxes, GUID/import checks |

For each result record the exact commit, Unity version, suite, XML/log artifact and failures/skips. Keep unexecuted rows pending. Manual Combat Lab inspection and device profiling remain separate from automated correctness results.

## Independent authoring inspection

The architecture agent independently inspected authored files for cards #33–40 on 2026-10-10. Structural inspection covered 14 enemy/projectile/encounter/rival prefabs and their definitions, plus the EnemyLab encounter reference. The final inspected state had no duplicate GUIDs or authored IDs, missing asset GUID references, missing local or external YAML file IDs, invalid YAML mappings, duplicate script components, or unrecognized serialized definition fields. Damageable enemy roots had dynamic Rigidbody2D bodies, Demon faction and positive health; the flying root had zero gravity. Each enemy had one behavior driver; phase/narrative observers did not duplicate that driver. The flight fixture's attack range permits its provisional hover height.

Authors corrected findings before this structural verdict: malformed attack-script references in elite/mini-boss assets, a dangling copied child Transform in the ground prefab, and duplicate protection components in support prefab copies. The EnemyLab PrefabInstance resolves its authored encounter GUID and root Transform; Unity's imported source-prefab handle is distinct from a raw YAML object anchor.

This is an authoring verdict only. No Unity import, compiler, scene execution, physics, test suite or device measurement was run by this inspection. Reinspect changed assets if authoring changes after the recorded review, and attach commit-specific execution evidence before claiming runtime acceptance.
