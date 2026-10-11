# Card 82: Narrative Transitions

StageNarrativeTransitions2D binds actual VerticalSliceSession BeatChanged events to seven authored sequences from airplane through show, with a short screen fade. Busy dialogue preserves the current scene and retries the new cue; cues present once and policy cancellation permits retry. SliceNarrativeComposition2D creates the explicit stage-narrative prefab, binds session and optional audio, then tears it down on stop. This prototype overlays gameplay without pausing enemies or locking player input. It does not invent a stage progress prerequisite. Original staging, UI focus and final cinematic/pause behavior remain pending production review.

No Unity execution, audio listening pass or runtime frame-rate result is claimed. User deferred tests to the final phase.
