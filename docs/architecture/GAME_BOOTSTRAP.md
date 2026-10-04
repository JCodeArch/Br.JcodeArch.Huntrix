# Game Bootstrap

`GameBootstrap` is the scene-owned composition root for application runtime systems. The startup scene is `Assets/Scenes/SampleScene.unity`; it contains exactly one active `GameBootstrap` object.

The serialized `systems` array is the explicit composition list. Every entry must be a unique, active and enabled `MonoBehaviour` implementing `IGameInitializable`. Its array order is the initialization order. `GameBootstrap.Start` validates the full array before invoking any entry, so missing, inactive, duplicate or incompatible references produce a diagnostic and prevent all initialization. An exception from an initializer stops the remaining entries and is logged with the failing component; there is no rollback for earlier initializers.

`Start` runs after active scene objects have completed `Awake`. Systems that depend on this composition should do that dependent work in `Initialize`, not in `Awake` or another `Start` whose relative order is unspecified. The contract is synchronous and called once for each bootstrap component's scene lifetime. Loading a new scene with a new bootstrap runs that scene's composition again.

The current list is empty because the project has no approved runtime systems yet. Later cards can add concrete systems and implement `IGameInitializable`; this card does not create service locators, singletons, persistent global objects, async startup, or gameplay behavior. Keep one bootstrap in each scene that needs composition. Multi-scene ownership, cross-scene persistence and rollback remain undecided until a card requires them.

## Accepted follow-up risk

The SOLID review approved this foundation with one non-blocking P2 finding: if a future `Initialize` call throws, earlier systems may already have side effects, later systems will not run, and this bootstrap logs the failure without rolling back or preventing the scene from continuing. This is accepted for this card because the startup composition is empty. Before connecting the first stateful system, the System Architect and Core Gameplay Developer must define a failed-start state or compensation/rollback contract and add a PlayMode failure-path test; consumers must not use partial composition as ready state. This follow-up remains open until that system is added.
