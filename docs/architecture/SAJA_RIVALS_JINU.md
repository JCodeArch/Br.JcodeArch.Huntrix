# Saja rivals and Jinu encounter — prototype cards #39 / #40

Status: implemented prototype, Unity validation deferred by the owner. No test suite, Editor compilation, scene playtest or performance approval is claimed. Combat parameters and identity-to-mechanic assignments are provisional game design, not canonical powers from the film.

## #39 — individual rivals

`SajaRivalDefinition` binds a named identity to an enemy/attack profile and an actual tactic. `SajaRivalController2D` is the only driver of its `EnemyAgent2D`; do not add GroundEnemyController or EnemyPatternController to these prefabs.

| Rival | Actual prototype behavior | Attack distinction |
| --- | --- | --- |
| Abby | Pursues target, stops to telegraph a heavy strike, then waits between attacks | Large/tall nearby impact, long readable windup |
| Baby | Pursues and performs a three-attack burst before an extra pause | Small quick close-range impacts with individual recoveries |
| Mystery | Remains stationary when target is outside its attack range; waits for target to approach | Delayed long/narrow precision band; not a tracking projectile |
| Romance | Retreats horizontally when target is closer than preferred distance, otherwise pursues into range | Distant broad horizontal band, independently telegraphed |

Shared contact queries apply the existing receiver's parry/protection/invulnerability/faction rules and line-of-sight checks. Facing is captured by the agent at telegraph start. No direct health subtraction, invented dialogue or copyrighted audio is included. Long-range attacks here are instantaneous authored hitbox bands, not bullets; final visuals, targeting telegraphs and animation are pending.

Files for #39: `SajaRivalDefinition.cs`, `Gameplay/Rivals/SajaRivalController2D.cs`, `Data/Rivals/{Abby,Baby,Mystery,Romance}_{Attack,Enemy,Rival}_Prototype.asset`, and `Resources/Rivals/{Abby,Baby,Mystery,Romance}_Prototype.prefab`, with their `.meta` files and shared Rivals folder metadata.

## #40 — Jinu combat evolution and narrative hooks

The Jinu prefab uses the same tactic controller, plus `JinuEncounterController2D` observing its live damage receiver. `JinuEncounterDefinition` supplies distinct opening/pressure profiles, a provisional 50% health transition and authored narrative cue identifiers.

Opening pursues into a nearby broad strike. At the health threshold, the encounter waits for a safe Idle/Chasing boundary instead of cancelling telegraph or recovery, then changes to a faster three-hit burst using a longer, narrower attack band. Health and target are preserved by the shared agent's profile configuration. Stage progression is monotonic within that instance: Uninitialized → Encounter → Pressure → Defeated; death can also skip Pressure. Fresh respawn instances start their own encounter.

`NarrativeCueRequested(stage, cueId)` requests presentation integration using `jinu.encounter`, `jinu.pressure`, and `jinu.defeated`. These are technical hook IDs, not invented film scenes, dialogue or claims about Jinu's ultimate narrative outcome. A story/cutscene system must connect reviewed film-aligned content later. Events snapshot the stage and isolate subscriber exceptions; lifecycle re-enable reconciles a death that occurred while the observer was disabled without replaying earlier cues.

Files for #40: `JinuEncounterDefinition.cs`, `Gameplay/Rivals/JinuEncounterController2D.cs`, `Data/Rivals/Jinu{Opening,Pressure}_{Attack,Enemy,Rival}_Prototype.asset`, `Data/Rivals/Jinu_Encounter_Prototype.asset`, and `Resources/Rivals/Jinu_Prototype.prefab`, with metadata. Depends on #39's generic rival controller and #33's shared agent.

## Integration

Load/instantiate a prefab from `Resources/Rivals/<Name>_Prototype` as a scene root with a dynamic Rigidbody. Ground placement, explicit target binding and encounter ownership belong to the scene/encounter integration. After Awake, call `GetComponent<EnemyAgent2D>().TrySetTarget(playerDamageReceiver)`. When the player's manager changes the active character, rebind the enemy target explicitly; no scene searching is performed. Subscribe to Jinu narrative events immediately after Instantiate, before Start, to receive the initial cue.

Do not assume these prefabs are wired into CombatLab, a complete boss encounter, fan-drain sequence or co-op system. Shared geometric silhouettes are original placeholders. The manager/source target must remain alive/active and faction-opposed; no target means no attack.

Execution order: shared agent 100, rival controller 200, Jinu observer 300, authored with DefaultExecutionOrder attributes. Their .meta executionOrder values remain 0 (no custom override), so the attributes supply the default ordering. Only the rival controller advances agent Tick; the Jinu observer changes profiles and emits cues but does not drive movement/attacks. Its phase transition waits for a safe state.

## Deferred validation and open design

Validate actual Unity import, composition/Awake order, identity assets, target rebinding, faction/root rejection, ray/overlap saturation, wall occlusion, ground contact, attack positioning, parry/protection, burst pause timings, retreat vs world bounds, stationary ambush readability, death/disable during event callbacks, safe one-time phase changes, missed-death reconciliation and initial narrative subscription timing. Profile target hardware later. Romance retreat is a prototype motor without edge detection/pathfinding; long bands require authored visual telegraphs. Final tuning, canon scene mapping, cutscene assets, sounds, animation and boss encounter orchestration remain open.

Identity names checked against Netflix Tudum's cast overview: https://www.netflix.com/tudum/articles/kpop-demon-hunters-cast. The repository GDD_MASTER.md sections 151–161 provide the project scope: distinct Saja rivals and Jinu narrative arc, with specific mechanics and story beats still open.
