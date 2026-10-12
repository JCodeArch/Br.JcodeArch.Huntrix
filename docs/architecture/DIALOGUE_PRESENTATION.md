# Card 81: Dialogue Presentation

DialoguePresenter2D presents an actual responsive immediate-mode panel with speaker, color portrait, progressive localized text and a Continue button. Advance first reveals the current line, then changes lines, then emits completion. It supports PT/EN and explicit voice routing through75; the original text assets have no voice recordings. INarrativePresentationPolicy is an optional earlier-card interface: a bound policy must resolve every line; disabled/unclassified/rejected policies fail closed. Policy changes cancel the displayed text, suppress raw voice, and expose cancellation to sequencers. Age-specific implementation belongs88. This uses built-in GUI rather than requiring an additional UI package; cinematic graphics and controller navigation are pending production work.

No Unity execution, audio listening pass or runtime frame-rate result is claimed. User deferred tests to the final phase.
