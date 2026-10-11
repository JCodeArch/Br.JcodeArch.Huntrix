# Character Bible — original prototype v1

Card 69. Authored geometry and palettes are original project sketches; no movie frames, traced costumes, photographs or third-party textures are included. Names and gameplay roles follow the GDD. These sketches establish functional identity, not approved likenesses or final production art.

| Profile | Gameplay silhouette | Distinguishing palette/shape |
|---|---|---|
| Rumi | medium sword wielder | violet torso, gold long blade, long back hair |
| Mira | tall polearm wielder | rose torso, pale pink extra-long polearm |
| Zoey | short twin-star wielder | cyan torso, mint paired diamond stars |
| Abby | broad rival | wide dark-violet body, short microphone |
| Baby | compact rival | short blue body, pale microphone |
| Mystery | narrow rival | dark indigo, long back hair, staff |
| Romance | medium rival | burgundy, rose wide fan |
| Jinu | tall rival | dark teal, mint sword |
| Gwi-Ma | large boss | red-black body, orange paired horns |

CharacterArtDefinition owns validated immutable dimensions/colors. CharacterArtRenderer2D builds an eight-part local rig plus head/hair/weapon decorations, with no colliders or gameplay changes. Resources/Art contains nine usable display prefabs. CharacterArtBinder2D binds explicit prefab→profile mappings to one CharacterManager slot; no scene search/name matching. It hides the prior blockout marker only after successful rig creation.

Asset register: all nine are concept/prototype handoffs v1, authored by code in this repository. Original sources: CharacterArtDefinition assets and CharacterArtRenderer2D. Intended scale: approximately one world unit per playable body, camera orthographic size5. Imported/rendered readability and gameplay validation await final Unity checks; these assets are not final art or a distribution rights clearance for the underlying characters.
