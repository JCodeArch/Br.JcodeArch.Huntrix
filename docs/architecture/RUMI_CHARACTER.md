# Rumi Character Prototype

## Card scope

Card #28 composes the existing movement, jump, dash, combat, damage, combo, and parry systems into a data-driven Rumi prototype. The prototype is mobile and uses direct, close-range sword attacks. It does not implement character selection, active-character ownership, respawn consumption, input bindings, or final art; those remain with cards #31–32 and the later input/art work.

## Static data and runtime ownership

`CharacterDefinition` contains Rumi's display name and references to a `CharacterMovementDefinition`, `CombatComboDefinition`, and `ParryDefinition`. The profile and attack assets hold authored values only. Runtime health, combo progress, dash state, parry windows, hitbox activation, and physics remain on the existing components.

`CharacterDefinitionApplier2D` runs after the other character components' `Awake` methods. It validates the complete definition, all required components, and whether an action is in progress before writing configuration. It then uses internal, non-rejecting setters for the already validated values. Invalid data, missing components, an active dash, attack, or parry window prevents application before any controller changes.

The `Rumi_Prototype` prefab composes `Rigidbody2D`, colliders, `DamageReceiver2D`, `Hurtbox2D`, `HorizontalMovement2D`, `JumpController2D`, `DashController2D`, `AttackHitbox2D`, `AttackController2D`, `ParryController2D`, and the applier. The child trigger hitbox is disabled outside an attack activation. Input and ground/wall sensing are supplied by future integration layers; automated tests load the authored Combat Lab host and instantiate Rumi/target fixtures there, then call the existing logical controller APIs directly. The Combat Lab scene asset remains an empty host.

## Prototype kit

The data supplies three ground sword steps (quick slash, cross slash, rising cut) and two aerial steps (dive slash, downward cut). Both chains use the shared `AttackController2D` and existing damage receiver path. Parry uses the shared `ParryController2D`.

Movement, damage, timing, hitbox, and knockback numbers in `Assets/Data/Characters/Rumi` are **provisional integration fixtures**, not approved character balance or a result of playtesting. No unique skill, attribute, resource, or final combo identity is claimed. The qualitative distinction follows the current card boundary: mobile, direct, close-range sword combat, in contrast to Mira's area/defense direction and Zoey's ranged-precision direction. Those neighboring cards may refine this relationship before their implementation.

## Verification

The EditMode suite validates required profile data, movement ranges, all authored definition IDs, and prefab composition. The PlayMode suite loads Combat Lab, instantiates Rumi and a target, verifies movement/jump/dash configuration, advances ground and aerial chains, observes direct damage from both, rejects an incoming attack during a parry window, and checks teardown while the hitbox and parry are active. Full current regression results for card #28: **EditMode 52/52 passed; PlayMode 206/206 passed** on Unity 6.6.4f1.

These automated tests do not establish game feel, final tuning, controller bindings, animation, VFX, or visual readability. The prefab is a gameplay composition prototype without character art.

## Age and accessibility review scope

This card adds no age profile, personal data, account, network, chat, dialogue, interface, monetization, voice, animation, or visual effects. The current combat prototype applies numeric damage and has a timed parry window; tests call logical APIs and do not assess physical controls, feedback, timing accessibility, assist options, or remapping. The art/readability review is N/A until character identity, model-sheet, and animation-pipeline work in cards #69–71; captions/dialogue and flash review are N/A because this prefab contains no voice/text or effects. This is a limited negative scope check, not a global age, classification, Child Safety, or accessibility approval. Age adaptations and player options remain open under cards #85–91 and QA #102–103.
