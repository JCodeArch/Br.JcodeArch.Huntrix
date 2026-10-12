# Card 75: Audio Manager

GameAudioManager2D owns twelve preallocated 2D SFX voices, six synchronized looping music sources and one exclusive dialogue voice. Saturation drops SFX; no voice stealing or per-frame creation. Disable stops every source. Master volume and per-layer target volumes validate finite values. The original prototype prefab assigns thirteen repository-authored synthesized PCM WAVs; no external recording or soundtrack is embedded. Spatial position is accepted as a future contract; this prototype uses non-spatial playback. Voice supports explicit clips but no spoken recording is authored. AudioSource counts and audible mixes require final Unity validation.

No Unity execution, audio listening pass or runtime frame-rate result is claimed. User deferred tests to the final phase.
