# Replay without feedback storage — 22/10/2026

After completion, the HUD previously offered only “Salvar avaliação e jogar novamente” when the playtest recorder existed. If submitting or writing the report failed, `RestartCompletedSession` returned without loading the stage. A local storage failure could therefore block replay.

The feedback panel now keeps the save-and-replay action and also offers “Jogar novamente sem salvar”. The latter reloads the completed stage without calling the recorder, so the player can explicitly skip feedback storage. A PlayMode regression test first failed because no such route existed (`outputs/b83/RestartRed.xml`, 0/1), then passed after the change (`outputs/b83/RestartGreen3.xml`, 1/1). It loads the stage through Build Settings because the editor scene-loading helper assigns `buildIndex = -1`, while the player replay path requires a non-negative build index.

Complete clone suites passed: `outputs/b83/PlayMode.xml` 559/559 and `outputs/b83/EditMode.xml` 156/156, with no C# compiler errors or warnings. This verifies the reload path and suite isolation; a human still needs to complete the stage and inspect both buttons in the player.

Windows Development packaging of commit `135ca7cafb7ecbfc62a6447d69b6de2c9e522d03` also succeeded with zero errors and warnings (`outputs/b84/Windows.evidence.json`). The artifact folder SHA-256 manifest is `7922475d536462b8c4fbc4f84d69efe932e8a98e9924194ab1edebba8a4c6f3e`.
