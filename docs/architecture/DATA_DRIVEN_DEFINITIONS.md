# Data-Driven Definitions

## Purpose

Card #17 establishes typed Unity assets for static, authored data. Later gameplay cards can add approved fields to these definitions and reference them without mixing data with runtime behavior.

## Runtime contracts

The runtime assembly exposes six ScriptableObject types in the HuntrX.Data namespace:

| Definition | Asset menu |
|---|---|
| CharacterDefinition | HUNTR/X/Data/Character |
| EnemyDefinition | HUNTR/X/Data/Enemy |
| StageDefinition | HUNTR/X/Data/Stage |
| AttackDefinition | HUNTR/X/Data/Attack |
| FoodDefinition | HUNTR/X/Data/Food |
| ProfileDefinition | HUNTR/X/Data/Profile |

All derive from GameDataDefinition. Each new object receives a serialized GUID string once; saving and reloading preserves it. Duplicating an asset also duplicates its ID, so the project validator reports the collision. IDs are authoring identity, not gameplay values. `AttackDefinition` now also stores static base attack damage, startup/active/recovery durations, hitbox size/offset, and horizontal/upward knockback impulse as approved by card #24. It validates these fields without selecting product tuning values. Other definitions keep the fields approved by their respective cards.

The menu command HUNTR/X/Data/Validate Definitions scans ScriptableObject assets beneath Assets/Data. The shared validator reports null references, blank IDs, and repeated IDs; the Editor output includes the asset path. Validation reports problems without editing the assets. The scan is an explicit authoring/QA action, not runtime work.

## Profile and save boundary

ProfileDefinition represents static template metadata only and currently contains only the inherited ID. It does not hold campaign progress, player preferences, PII, or mutable save state. The future runtime save model remains separate and follows the logical contract in SAVE_AND_PROFILES.md. AgeProfileData is a different concept owned by card #85.

## Fields deliberately deferred

- Character attributes, skills, abilities, and switch rules: gameplay cards #28–32.
- Enemy stats, AI, attack patterns, and encounter behavior: cards #33–41.
- Attack costs, range progression, combos, parry, and balance/tuning decisions: cards #19-27 and their dependent combat cards. Card #24 defines only the base attack data contract; authored assets and final values remain deferred.
- Food acquisition, use, and effects: cards #42–50.
- Stage scene references, sequence, and authored level content: level-design cards; card #18 only sets up Combat Lab.
- Save-slot count and management, preferences, serialization, migration, cloud sync, and co-op save rules: cards #9 and subsequent save/profile decisions.
- Age-specific configuration and accessibility: card #85 and the related QA cards.

No sample assets are created by this card. The architecture waits for the relevant backlog decisions before these fields or authored content are added.
