# Required airplane captain spawn — 22/10/2026

`SliceAirplaneBeat2D.TryBegin` previously ignored failed creation of its three extra corridor enemies, including the mandatory cockpit captain. A missing captain could leave a running phase whose objective tells the player to defeat a boss that was never created.

The new PlayMode test temporarily invalidates the captain prefab's enemy definition in memory, loads the real `VerticalSlice` scene, then restores the prefab and unloads the scene. RED: the session kept running without the captain (`outputs/b80/CaptainRed.xml`, 0/1). The beat now returns failure if any required extra spawn fails; the session stops rather than publishing an incomplete encounter. GREEN: `outputs/b80/CaptainGreen2.xml`, 1/1. The complete clone suites passed 558/558 PlayMode and 156/156 EditMode (`outputs/b80/PlayMode.xml`, `outputs/b80/EditMode.xml`) with no C# compiler errors or warnings. The prefab asset is unchanged in Git.

This checks failure handling at stage startup. It does not establish that a player can defeat the captain or reach the exit; a controlled full playthrough is still needed.
