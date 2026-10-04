# Data-Driven Definitions — Design

**Status:** Approved for card #17 implementation on 2026-10-04.

## Intent

The #17 backlog card requests ScriptableObjects for characters, enemies, stages, attacks, food, and profiles. Provide distinct static authoring types now so later gameplay cards can reference typed assets, while leaving gameplay fields to the cards that decide them.

## Contracts

- GameDataDefinition : ScriptableObject is the common static-definition identity contract. Each definition has a stable, non-empty string ID.
- Concrete types in namespace HuntrX.Data: CharacterDefinition, EnemyDefinition, StageDefinition, AttackDefinition, FoodDefinition, and ProfileDefinition.
- Each concrete type is creatable through a unique Unity asset menu entry.
- Definitions remain data-only; they do not initialize systems or implement gameplay behavior.
- A shared validation function reports null entries, blank IDs, and duplicate IDs. An Editor menu scans project data assets and reports findings with paths without changing files.

## Profile boundary

ProfileDefinition is a static authored template identity only. The mutable per-user save remains a separate serializable runtime model following docs/architecture/SAVE_AND_PROFILES.md; neither save data nor PII belongs in a ScriptableObject. This definition is not AgeProfileData, which is explicitly deferred to card #85.

## Explicitly deferred

Character attributes and skills; enemy stats and AI; stage scene references/order; attack damage, timing, costs, ranges, or hitboxes; food acquisition/effects; profile count, management, preferences, age rules, save serialization/migration, cloud sync, and co-op rules. The definitions are scaffolding, not sample content. No Addressables, runtime data registry, persistence system, or new package is required.

## Acceptance evidence

EditMode tests prove type distinction, asset menu availability, stable ID presence, invalid/duplicate ID detection, and static/profile save separation. The Unity Editor validator completes over Assets/Data. Documentation records the separation and deferred decisions.
