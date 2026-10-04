# Data-Driven Definitions

## Purpose

Card #17 establishes typed Unity assets for static, authored data. Later gameplay cards can add approved fields to these definitions and reference them without mixing data with runtime behavior.

## Runtime contracts

The runtime assembly exposes ten ScriptableObject types in the HuntrX.Data namespace:

| Definition | Asset menu |
|---|---|
| CharacterDefinition | HUNTR/X/Data/Character |
| CharacterMovementDefinition | HUNTR/X/Data/Character Movement |
| EnemyDefinition | HUNTR/X/Data/Enemy |
| StageDefinition | HUNTR/X/Data/Stage |
| AttackDefinition | HUNTR/X/Data/Attack |
| CombatComboDefinition | HUNTR/X/Data/Combat Combo |
| ParryDefinition | HUNTR/X/Data/Parry |
| MiraProtectionDefinition | HUNTR/X/Data/Mira Protection |
| FoodDefinition | HUNTR/X/Data/Food |
| ProfileDefinition | HUNTR/X/Data/Profile |

All derive from GameDataDefinition. Each new object receives a serialized GUID string once; saving and reloading preserves it. Duplicating an asset also duplicates its ID, so the project validator reports the collision. IDs are authoring identity, not gameplay values. `AttackDefinition` now also stores static base attack damage, startup/active/recovery durations, hitbox size/offset, and horizontal/upward knockback impulse as approved by card #24. It validates these fields without selecting product tuning values. Other definitions keep the fields approved by their respective cards.

The menu command HUNTR/X/Data/Validate Definitions scans ScriptableObject assets beneath Assets/Data. The shared validator reports null references, blank IDs, and repeated IDs; the Editor output includes the asset path. Validation reports problems without editing the assets. The scan is an explicit authoring/QA action, not runtime work.

## Profile and save boundary

ProfileDefinition represents static template metadata only and currently contains only the inherited ID. It does not hold campaign progress, player preferences, PII, or mutable save state. The future runtime save model remains separate and follows the logical contract in SAVE_AND_PROFILES.md. AgeProfileData is a different concept owned by card #85.

## Fields deliberately deferred

- Character attributes, skills, abilities, and switch rules: gameplay cards #28–32.
- Enemy stats, AI, attack patterns, and encounter behavior: cards #33–41.
- Attack costs, range progression, parry, and balance/tuning decisions: cards #19-27 and their dependent combat cards. Combo templates are defined by card #25 and described in COMBO_COMBAT.md. Card #24 defines only the base attack data contract; authored assets and final values remain deferred.
- Food acquisition, use, and effects: cards #42–50.
- Stage scene references, sequence, and authored level content: level-design cards; card #18 only sets up Combat Lab.
- Save-slot count and management, preferences, serialization, migration, cloud sync, and co-op save rules: cards #9 and subsequent save/profile decisions.
- Age-specific configuration and accessibility: card #85 and the related QA cards.

No sample assets are created by this card. The architecture waits for the relevant backlog decisions before these fields or authored content are added.

## Combo definitions

Card #25 adds `CombatComboDefinition` and serializable `ComboStep` as static combat templates. They reference existing `AttackDefinition` assets for grounded/aerial sequences; runtime chain state, health, input state, and save progress remain outside ScriptableObjects. Validation and runtime behavior are documented in [COMBO_COMBAT.md](COMBO_COMBAT.md).

Card #26 adds `ParryDefinition` as static template timing for a parry window. It contains no runtime cooldown/window state, no authored sample asset, and no default balance value; invalid or absent definitions prevent activation.

## Character prototype — card #28

Card #28 extends `CharacterDefinition` with a display name and static references to movement, combo, and parry definitions. `CharacterMovementDefinition` stores the validated movement, jump, and dash values currently consumed by shared controllers. The Rumi sample assets and `Rumi_Prototype` prefab demonstrate these contracts; all numeric values are provisional integration fixtures. The character applier configures existing controllers and owns no input, save, health, respawn, or character-selection state. See [RUMI_CHARACTER.md](RUMI_CHARACTER.md) for composition and verification.

## Mira protection definition — card #29

`MiraProtectionDefinition` adds finite positive duration and radius as authored data only. Its active timer, overlap membership, and protection state belong to runtime components. The Mira field asset is referenced separately by `MiraProtectionField2D`, rather than extending every `CharacterDefinition` with a Mira-specific reference. The actual Mira profile and Resources prefab reuse the existing Rumi movement/basic combat definitions as provisional shared fixtures and compose a distinct defensive field. All values remain provisional; no final tuning or unique basic attack choreography is claimed.

`DamageProtection2D` is an explicit target opt-in contract, with multiple fields and per-collider membership; the existing combat faction enum does not determine field eligibility. Actual fan integration, health and rescue rules remain dependent on cards #42–50. See [MIRA_CHARACTER.md](MIRA_CHARACTER.md) for lifecycle, damage precedence, same-activation reservation, prefab composition, validation, and performance limits.
