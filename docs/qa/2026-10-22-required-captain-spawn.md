# Required airplane captain spawn — 22/10/2026

`SliceAirplaneBeat2D.TryBegin` previously ignored failed creation of its three extra corridor enemies, including the mandatory cockpit captain. A missing captain could leave a running phase whose objective tells the player to defeat a boss that was never created.

The new PlayMode test temporarily invalidates the captain prefab's enemy definition in memory, loads the real `VerticalSlice` scene, then restores the prefab and unloads the scene. RED: the session kept running without the captain (`outputs/b80/CaptainRed.xml`, 0/1). The beat now returns failure if any required extra spawn fails; the session stops rather than publishing an incomplete encounter. GREEN: `outputs/b80/CaptainGreen2.xml`, 1/1. The complete clone suites passed 558/558 PlayMode and 156/156 EditMode (`outputs/b80/PlayMode.xml`, `outputs/b80/EditMode.xml`) with no C# compiler errors or warnings. The prefab asset is unchanged in Git.

This checks failure handling at stage startup. It does not establish that a player can defeat the captain or reach the exit; a controlled full playthrough is still needed.

The Windows Development build from commit `dff8521d139429f677658e66bcf8350f823f0ed7` succeeded with zero errors and warnings (`outputs/b81/Windows.evidence.json`). Artifact: `outputs/b81/Windows/Huntrix.exe`, SHA-256 `fabb3747c9c7aeca8b5153ad5793e7a84e09b057148087295c78eb36f5378f32`. This validates packaging; it is not a substitute for a complete player-run encounter.

The Android Development build from commit `df7c8635c7019cc54f8bc6cf5b4113e0fb492e8a` succeeded (`outputs/b82/Android/Android.evidence.json`): `outputs/b82/Android/Huntrix.apk`, 187,353,350 bytes. The report records zero errors and one warning: Unity Cloud had no token to upload native symbols; local symbols were generated. `apksigner verify --print-certs` passed with the Android Debug certificate. `adb devices` found no attached device, so installation, performance and a complete touch-control playthrough remain unverified.
