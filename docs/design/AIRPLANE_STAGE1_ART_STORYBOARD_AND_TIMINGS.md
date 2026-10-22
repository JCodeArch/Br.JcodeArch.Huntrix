# Stage 1 (`airplane`) — Art Direction, Storyboard & Cutscene Timing Specification (Lote 13)

## 1. Scope & Zero-Conflict Isolation
This art-direction package provides Unity-ready `1376x768` (`16:9`, 2D cel-shaded) cutscene frames, deterministic `.meta` importers (`TextureImporterType.Sprite`, `spriteMode = Single`, `100 PPU`), a self-contained runtime catalog (`AirplaneCutsceneArtCatalog`), and EditMode import/palette verification (`AirplaneCutsceneArtAssetTests`) for the two narrative transitions of Stage 1 (`airplane`):
1. **Sequence A — Captain Dokkaebi Cockpit Entry (`Assets/Resources/Art/Cutscenes/AirplaneCockpitEntry/0..3.png`)**
2. **Sequence B — Post-Victory Dramatic Airplane Exit (`Assets/Resources/Art/Cutscenes/AirplaneVictoryExit/0..3.png`)**

To guarantee zero merge conflicts with Claude Desktop's Lote 12 worktree:
- **Untouched files:** `EnemyPrototypeVisual2D.cs`, `SliceAirplaneBeat2D.cs`, `LivingStageBackdropAnimator2D.cs`, and all combat/traversal PlayMode tests.

---

## 2. Visual Continuity & Character/Environment Contracts

- **Rumi Canonical Outfit & Weapon Signature:**
  - Long purple bubble braid (`#8F4CE8`..`#C27BFF`).
  - Mustard-yellow biker jacket (`#D89A2B`) worn open over a crisp white crop top/blouse (`#F3F5FA`).
  - Blue denim shorts (`#2B4268`) with a crimson-red belt (`#C6303E`), black platform boots with vertical red stripes.
  - Signature **cyan/blue Saingeom sword** (`#28E8FF`..`#7CF5FF`) with high-value electric blue edge highlights that read clearly against dark indigo cabin walls (`#1A1E3B`) and crimson demon flames.
- **Scale & Silhouette Legibility:**
  - In `AirplaneCockpitEntry/2.png` and `3.png`, `BOSS_Cockpit_CaptainDokkaebi` is framed at **`1.35x` Rumi's standing height** with a broad horned silhouette, white pilot shirt with gold epaulets and blue necktie, and crimson-purple aura, maintaining a clean gap between Rumi (left third) and the boss/bulkhead (right third).
- **Stage Continuity with Lote 11 Native Frames (`01..05`):**
  - Preserves the oval cabin windows with the purple-indigo night sky and cyan-lit jet wing outside, cyan/magenta vertical wall ribs, grated metallic floor, and the armored cockpit bulkhead door with yellow/black hazard stripes (`Cockpit_ArmorDoorFrame`).

---

## 3. Timing & Transition Specification

### Sequence A — Captain Dokkaebi Cockpit Entry (`Total: 3.50s`)
| Frame | Resource Path | Window (`s`) | Duration (`s`) | Transition (`s`) | Transition Kind | Camera Shake | Narrative Action |
| :---: | :--- | :---: | :---: | :---: | :--- | :---: | :--- |
| `0` | `Art/Cutscenes/AirplaneCockpitEntry/0` | `0.00 - 0.85` | `0.85s` | `0.14s` | `SmoothCrossfade` | `0.05` | Rumi approaches the locked cockpit bulkhead (`x ~ 36..40`) as crimson-violet Dokkaebi smoke seeps through the hydraulic door seam. |
| `1` | `Art/Cutscenes/AirplaneCockpitEntry/1` | `0.85 - 1.70` | `0.85s` | `0.08s` | `HardCutWithFlash` | `0.18` | The blast doors wrench open under red warning beacons; the Captain steps forward as his human disguise fractures and curved horns burst through his pilot cap. |
| `2` | `Art/Cutscenes/AirplaneCockpitEntry/2` | `1.70 - 2.65` | `0.95s` | `0.12s` | `SmoothCrossfade` | `0.25` | Full Red Captain Dokkaebi boss reveal (`1.35x` scale ratio vs. Rumi), roaring in front of the open cockpit threshold while Rumi holds her cyan Saingeom guard. |
| `3` | `Art/Cutscenes/AirplaneCockpitEntry/3` | `2.65 - 3.50` | `0.85s` | `0.16s` | `ImpactHoldToGameplay` | `0.30` | Clash telegraph: Rumi lunges right with a blazing cyan-blue Saingeom slash arc meeting Captain Dokkaebi's crimson-amber claw shockwave; hands off to active boss combat. |

### Sequence B — Post-Victory Dramatic Airplane Exit (`Total: 3.80s`)
| Frame | Resource Path | Window (`s`) | Duration (`s`) | Transition (`s`) | Transition Kind | Camera Shake | Narrative Action |
| :---: | :--- | :---: | :---: | :---: | :--- | :---: | :--- |
| `0` | `Art/Cutscenes/AirplaneVictoryExit/0` | `0.00 - 0.90` | `0.90s` | `0.15s` | `HardCutWithFlash` | `0.22` | Rumi lands a sweeping cyan-blue Saingeom finisher purifying Captain Dokkaebi into Honmoon sparks as the bulkhead beacon switches from red to emerald-cyan. |
| `1` | `Art/Cutscenes/AirplaneVictoryExit/1` | `0.90 - 1.80` | `0.90s` | `0.16s` | `SmoothCrossfade` | `0.08` | Rumi, Mira, and Zoey regroup side-by-side at the unsealed cabin exit hatch with night wind whipping their hair. |
| `2` | `Art/Cutscenes/AirplaneVictoryExit/2` | `1.80 - 2.75` | `0.95s` | `0.16s` | `SmoothCrossfade` | `0.14` | Exterior wide shot: the HUNTR/X trio leaps together out of the cabin hatch over the cyan-trimmed swept jet wing into the purple night clouds. |
| `3` | `Art/Cutscenes/AirplaneVictoryExit/3` | `2.75 - 3.80` | `1.05s` | `0.22s` | `ImpactHoldToGameplay` | `0.12` | High-altitude skydiving dive toward the glowing neon stadium dome of Seoul below, with Rumi leading in the center with her crackling cyan-blue Saingeom blade. |

---

## 4. Honest Limitations & Integration Notes
1. **Runtime Wiring Deferred to Lote 12 Merge:** Because `SliceAirplaneBeat2D.cs` and `LivingStageBackdropAnimator2D.cs` are reserved for Claude Desktop's Lote 12, this lote delivers the imported assets + `AirplaneCutsceneArtCatalog` API (`TryLoadSprites`, `TryLoadTextures`, `TryResolveFrameAtTime`) without modifying `SliceAirplaneBeat2D.cs`. Once Lote 12 merges, triggering either sequence requires only calling `AirplaneCutsceneArtCatalog.TryResolveFrameAtTime(...)` from the cockpit proximity trigger or `Stage_End` transition.
2. **4-Keyframe Storyboard Animatics (`1376x768`):** Each sequence is a 4-keyframe 2D cel-shaded storyboard animatic (matching the `1376x768` resolution of `Anime_01_AirplaneFight.png`), designed for crossfade/flash presentation in the PiP card or full-screen beat transition rather than a 24fps video stream (`VideoClip`), keeping repository size compact (`~1.2 MB` per PNG).
