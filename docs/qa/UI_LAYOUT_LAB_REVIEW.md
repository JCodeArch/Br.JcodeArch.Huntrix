# UI Layout Lab — PR #102 follow-up

The isolated `Assets/Scenes/UiLayoutLab.unity` scene now contains a bootstrap that composes the five screens in Play mode. Open this scene and press Play to inspect it. The editor menu `HUNTR/X/Build UI Layout Lab Scene` recreates the same bootstrap; it does not bake a visual preview into the scene in edit mode.

The shared `SliceUiLabLayout2D` composition creates real uGUI buttons, text, toggles, sliders and an optional comments input. Views bind controls by stable child names before navigation hides inactive screens. Start, profile confirmation/back, pause/options return, demonstrative results, replay and menu all navigate without loading gameplay. Continue and remapping remain explicitly unavailable. Values and the survey are a session-only demonstration, not gameplay telemetry or persisted feedback.

This work does not integrate the lab into VerticalSlice and does not replace the existing gameplay HUD. Design refinement by Agy and device/local validation by Kylo remain separate follow-up work.

## Verification

- Static view-control identity coverage and unique bootstrap GUID checked.
- `git diff --check` passed.
- Independent QA static review found no demonstrable blocker for the isolated lab.
- Six regression scenarios load the committed lab scene and exercise real controls, including raycast/pointer navigation, telemetry, preferences/survey and safe-area containment.
- Unity runner stopped at `UNITY_EDITOR` prerequisite in this environment. No new compilation, EditMode/PlayMode pass, visual screenshot or device performance result is claimed.
- Required local gate: import/compile on Unity 6000.6.4f1, execute six scene tests plus regression suites, then inspect landscape 16:9, ultrawide and notched-device screenshots and touch targets. Interactive slider hit areas require particular device review.
