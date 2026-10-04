# Combat Lab Scene Design

## Purpose

Card #18 creates a dedicated Unity scene that can host gameplay-system testing as later cards add those systems. It is scene infrastructure, not a level-design or gameplay-content card.

## Approved boundaries

- Keep `SampleScene.unity` as the startup scene and preserve its existing camera, lighting, and bootstrap.
- Add `CombatLab.unity` as a separate scene and list it after `SampleScene` in `EditorBuildSettings`.
- Use the established card #7 camera contract: orthographic 2D, zero x/y offset, zero rotation, and `orthographicSize = 5.4`; keep the standard camera z depth needed to render the 2D scene.
- Keep one active `GameBootstrap` in the Combat Lab with an empty systems list. Later cards add approved systems.
- Preserve the template's 2D global light. Add no gameplay objects, geometry, art, audio, HUD, or camera follow behavior.

## Validation

- PlayMode loads `CombatLab` by its build-settings scene name and checks the active scene, camera settings, 2D light, one active bootstrap, and empty composition.
- EditMode checks that both scenes are enabled and `SampleScene` remains first.
- Run Unity EditMode and PlayMode suites; record editor/import diagnostics and existing project warnings separately.

## Deferred

Stage geometry/layout, test actors, targets/enemies, camera tracking, spawn/checkpoint behavior, combat rules, and Vertical Slice sequence remain with their Trello cards. The lab is a host scene that later cards can extend when those contracts are approved.