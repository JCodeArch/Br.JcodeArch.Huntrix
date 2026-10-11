# Card 77: Combat Audio

CombatAudioBridge2D binds the current CharacterManager actor, committed DamageDealt/DamageReceived events, dash starts and Zoey shots; it also accepts enrolled rescue sources and the aggregate FanEncounter rescue event. Melee startup transitions and opening parry windows produce action cues; the parry cue denotes a window opening, not proof of a successful block. Enemy-hit feedback is the player received-hit event. Honmoon band changes and successful rescues receive distinct cues. Disable and actor changes detach listeners. Actor state sampling runs once per frame; final high-frequency/low-framerate missed-start audibility checks are deferred.

No Unity execution, audio listening pass or runtime frame-rate result is claimed. User deferred tests to the final phase.
