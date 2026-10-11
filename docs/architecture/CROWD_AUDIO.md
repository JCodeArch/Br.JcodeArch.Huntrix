# Card 78: Crowd Audio

CrowdAudioBridge2D maps normalized performance and the show-arrival floor to three crowd layers: base crowd plus pitched cheering and roaring sources allocated once. The authored original synthetic clap/noise chorus is a temporary crowd representation, with no human voice or copyrighted recording. Scene ShowArrival may call IShowArrivalAudioSink2D directly. SliceAudioComposition2D instantiates one explicitly supplied audio prefab and binds the live session, its current actor, fans, Honmoon, performance and Special. Beat6 supplies the show-arrival floor. It tears down the owned audio root when the session stops. Scene composition belongs card93 to avoid a forward dependency in the original slice61.

No Unity execution, audio listening pass or runtime frame-rate result is claimed. User deferred tests to the final phase.
