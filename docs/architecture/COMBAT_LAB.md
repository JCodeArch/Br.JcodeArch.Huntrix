# Combat Lab

`Assets/Scenes/CombatLab.unity` is a dedicated host scene for exercising gameplay systems as their Trello cards are implemented. It is separate from the application startup scene, `Assets/Scenes/SampleScene.unity`.

## Scene contract

- The build scene list keeps `SampleScene` first and includes enabled `CombatLab` second.
- One active `Main Camera` uses orthographic 2D projection, `orthographicSize = 5.4`, zero x/y offset and zero rotation. Its z position remains at -10 so it can render the 2D scene.
- The template `Global Light 2D` remains active.
- Exactly one active `GameBootstrap` is present. Its systems list is empty until later approved systems are added.
- No stage geometry, characters, enemies, HUD, art, follow camera, or gameplay rules are authored in this card.

## Use

Open `Assets/Scenes/CombatLab.unity` in the Unity Editor to inspect the test host. PlayMode tests load the enabled `CombatLab` build scene directly by its scene name. The project's first/startup scene remains `SampleScene`.

## Validation and deferred work

`CombatLabBuildSettingsTests` verifies enabled scene order. `CombatLabScenePlayModeTests` verifies loadability, camera, light, and empty bootstrap composition. The scene is infrastructure only; level layout, test actors/targets, checkpoints, camera tracking, combat content, and Vertical Slice sequence remain with their future Trello cards. Passing these scene tests does not claim those gameplay systems are implemented or validated.