# Windows D3D11 visual pass — 21/10/2026

Build: commit `34b5ba39a3c860961bb183006b7a75aa796debd7`, Unity 6000.6.4f1, Windows Development, manifest `outputs/b73/Windows.evidence.json` (0 build errors/warnings). Player launched with `-force-d3d11`; log confirms GTX 1660 Ti / Direct3D 11. The owner manually dismissed the Windows Firewall prompt with Cancelar before inspection.

## What was actually observed

- Opening tableau has coherent character colors and a cyan Rumi sword; this is an illustration, not evidence of runtime sprite quality.
- After the dialogue, Mira's gameplay silhouette is approximately 110–125 screen pixels high at 1282×752, while the nearby ground demon is about 190 pixels and flying demon about 200 pixels. These are visual estimates from the captured frame, not calibrated import measurements.
- Both enemies closed to the player start while the inspector was stationary. The ground demon and its magenta attack arc occluded Mira almost entirely. The active heroine did not remain distinguishable during the first combat cluster. Evidence: `outputs/b73/qa/combat-overlap.jpg`.
- The top objective remains a generic corridor instruction. The bottom strip includes developer-like labels (`SCORE`, `SUB-TANK`, key mappings, `Development Build`) and overlaps the gameplay floor. The options panel exposes a nine-stage navigation list and unfinished IMGUI appearance. Evidence: `outputs/b73/qa/options-menu.jpg`.
- The displayed health dropped from 100 to 44 without intentional movement during observation, then rose after death/recovery. The player log contains 24 Mira `Death/0` clip rejections (384×512 versus expected 512×512), consistent with repeated actor setup/respawn. This is evidence of a stationary spawn pressure loop, not proof that an actively controlled human cannot win. There were no application exceptions in this log.
- Code inspection explains the immediate pressure: the first authored ground and aerial enemies spawn within their detection ranges of the stage start. Enemies are not cleared by a respawn, and checkpoint-adjacent spawns can remain active. This is a code-level diagnosis that needs a controlled gameplay regression test before a fix is accepted.
- The player log also notes stripped URP Gaussian/Bokeh Depth of Field, Camera Motion Blur, and Panini Projection shaders. No visual assertion about those effects is made.

## Limits of this pass

The Windows UI controller sends discrete key presses rather than held movement. It could advance dialogue, open options, and trigger individual actions, but it could not perform a trustworthy 3–5 minute traversal with continuous controls. The camera was visually within the start-area scene; end bounds, moving follow behavior and real combat completion remain unobserved. The automated camera tests cover the geometric limits separately.

The options pause behavior was not measured. Source inspection shows `SliceHud2D` requests `HitStopService.SetPaused(true)` for options/dialogue, and that service sets `Time.timeScale` to zero. The observed health loss must not be used to claim that the options menu fails to pause.

## Acceptance decision and next work

Presentation **rejected** for this build: gameplay-scale characters and overlapping enemy/VFX silhouettes fail combat readability, and the options/HUD are still prototype UI. Prioritize a safe first encounter/respawn review, full HD animation states, then calibrated hero/enemy scale and sorting. Rebuild and repeat visual QA after changes; do not infer a complete playable phase from passing tests.
