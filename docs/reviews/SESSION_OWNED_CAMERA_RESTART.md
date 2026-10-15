# Owned camera restart lifecycle

Scope: `VerticalSliceSession2D.Stop` and its PlayMode regressions, based on develop `8e63e56`.

When a session creates its fallback camera, `Stop` scheduled the camera object for destruction but retained the `camera` field until Unity completed destruction. Calling `TryBegin` in the same frame therefore reused that retiring camera. The next frame could leave the new session without its camera.

Teardown now clears only a reference to its owned camera, disables the owned object immediately, and schedules destruction. An externally composed camera remains active and retained. No gameplay or rendering configuration changes.

Authored PlayMode scenarios load the real VerticalSlice composition and check immediate Stop/begin replacement through the deferred destruction boundary, and preservation of the scene's external camera through restart.

Validation: `git diff --check` passed; `python scripts/test-evidence-tooling.py` passed 10 tooling tests. `bash scripts/run-unity-tests.sh` exited 1 at the missing `UNITY_EDITOR` prerequisite; no Unity import, compile, RED/GREEN, PlayMode, visual or device execution occurred. Both new PlayMode scenarios remain deferred under PROJECT_RULES.md; tooling results do not validate gameplay. Independent review required before publication.
