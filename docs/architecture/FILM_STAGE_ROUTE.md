# Authored stage route prototype — card #56

The card names nine areas. `StageRouteDefinition` references their independent `StageBlockoutDefinition` assets in the prototype order below. It rejects invalid/null stages, duplicate stage identities and routes outside 1..32 entries. Runtime/save state does not belong to these authored definitions.

| Area | Stable stage ID | Blockout distinction |
| --- | --- | --- |
| Avião | airplane | Interior corridor, low catwalks and overhead cabin boundary |
| Show | show | Broad performance platform and side tiers |
| Myeongdong | myeongdong | Street kiosks with low optional stepping routes |
| Hanok | hanok | Alternating courtyard/roof stepping levels |
| Cheongdam | cheongdam | Rooftop terrace ascent and return to the lower path |
| Awards | awards | Wide presentation stage with separate wings |
| Naksan | naksan | Seven-step climb to a raised arena/checkpoint/exit |
| COEX | coex | Interior mezzanine and broad connecting platforms |
| Namsan | namsan | Raised wide final encounter platform and approach steps |

Every area supplies explicit ground/aerial/fan placements. The route is an authored experiment, not an assertion that final film chronology, dialogue, boss placement, licensing or cinematic adaptation has been validated. The parachute/fall transition is a vertical-slice beat, not a tenth area silently added to this list.

Assets live in `Assets/Data/Stages`; the route asset is `FilmRoute_Prototype.asset`. Independent playable scene/prefab composition belongs to card #60. Traversal and the complete route remain unexecuted in Unity under the owner's deferred-validation instruction.
