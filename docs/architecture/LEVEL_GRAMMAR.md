# Level grammar prototype — card #55

`StageBlockoutDefinition` separates authored geometry and placement from runtime construction. A stage has stable identity, theme, finite platform rectangles, start/end/arena positions, checkpoint destinations and bounded enemy/aerial/fan spawn points. Limits are 128 platforms, eight checkpoints, 32 ground enemy/fan points and 16 aerial points. These are authoring safety limits, not measured performance budgets.

The grammar uses a continuous safe floor, reachable lower steps, optional upper paths, checkpoints before encounter regions, an open arena and an exit marker. Platform colliders use authored world dimensions; only their visual children are scaled. Ground enemies, aerial enemies, fans and bosses are independently composed by encounter adapters, so a shared transform root cannot accidentally suppress combat.

`StageBlockoutBuilder2D.TryBuild(definition)` creates owned collision geometry, visible colored platforms, `StartPoint`, `EndPoint` and stable checkpoint anchors. `TryClear()` synchronously deactivates and disposes only its owned geometry/visual resources. It creates no player, input controller or implicit scene lookup. Existing anchors have a narrow one-time `TryConfigure(id, destination)` API for new runtime-generated anchors; preauthored nonempty identities cannot be rewritten through it.

The standalone grammar fixture is `Data/Stages/Grammar_Prototype.asset` and `Resources/Levels/Grammar_Blockout.prefab`. Its host starts construction when instantiated. It is a geometry fixture; the playable player-slot compositions are card #60.

For the current Rumi fixture (jump velocity 8 and gravity scale 2, assuming gravity 9.81), ideal maximum rise is approximately 1.63 units. The lower platform rise is 1.4–1.45 units; taller routes require intermediate steps. This is a theoretical design check, not a measured traversal result. Runtime collision, early-release jump cuts, dash distances, timing, camera and accessibility require deferred Unity inspection.
