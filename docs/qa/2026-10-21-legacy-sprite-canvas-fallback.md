# Legacy sprite canvas fallback

The full PlayMode run recorded in `outputs/hd-validation/sprite-factory-final-PlayMode.xml` failed 2 of 547 tests after strict canvas validation rejected entire runtime catalogs. The log identifies legacy `Rumi/Death/0` and `Mira/Death/0` textures sized 384x512 against a 512x512 Idle canvas. This made the catalog unavailable to otherwise valid presentation and lifetime tests.

The recovery policy now rejects only the incompatible clip. It emits a warning naming the resource path, actual size and required size, and uses the full compatible Idle fallback for that state. All textures in a clip are checked before any sprites for that clip are allocated. Other valid clips keep their artwork; frames are never cropped or stretched to hide a mismatch.

This fallback preserves presentation availability, not a finished death animation. Rumi and Mira death artwork still requires compatible padding and visual review. Do not report the static Idle fallback as approved death poses, a final death frame, or completed animation content. No PNG artwork was changed for this recovery.

Regression coverage includes an oversized first attack frame, a later oversized attack frame after a compatible prefix, and both first/later undersized death frames. Each case requires an explicit warning, a valid catalog, a full-size single-frame fallback, preserved Run artwork, and no native sprites allocated for the rejected clip. Full EditMode and PlayMode reruns remain required for the changed recovery behavior.

Final verification: sprite-fallback-final-EditMode.xml passed137/137; sprite-fallback-final-PlayMode.xml passed547/547, both exit0. The explicit legacy frame warnings remain expected diagnostics; these results do not approve the replacement Idle image as authored death animation.
