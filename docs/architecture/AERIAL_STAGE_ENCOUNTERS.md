# Card 57: Aerial Stage Encounters

Explicit composition calls Configure(builder, manager, flyingPrefab), then TrySpawn after the stage geometry exists. Up to sixteen authored aerial points instantiate independent roots with the existing flying behavior. Character changes retarget the owned cohort. Clear removes ownership before controller callbacks and tears down only these flyers. Bootstrap owns calling Clear before stage geometry rebuild. No global acquisition or input bindings. Heights and positions come from the nine stage definitions; flight/physics validation is deferred to the final Unity test phase.

Static review is separate from Unity execution; no runtime pass is claimed.
