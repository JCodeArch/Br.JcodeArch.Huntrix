# Primeira fase — auditoria de progressão

Base: c6bfde0, 21/10/2026. Inspeção de fonte; não equivale a playthrough.

## Achados verificáveis

1. `SliceAirplaneBeat2D.TryBegin` valida a criação dos inimigos do asset, mas ignora a falha de criação dos quatro inimigos extras, incluindo o capitão. Uma falha pode permitir iniciar uma fase incompleta. Precisa de teste de falha de composição e interrupção consistente.
2. A saída do avião exige zero inimigos terrestres vivos, zero aéreos vivos e distância de até 2 unidades do fim. O texto mostrado depois de x32 pede derrotar o capitão, mesmo se ele já morreu e restam inimigos atrás. Deve mostrar o requisito ainda pendente.
3. O teste `CompletingAirplaneBeatReplacesTitleWithoutRecreatingHud` chama `ClearEnemies` e transporta o corpo até a saída. Prova a ligação de transição/HUD, mas não prova que os encontros são vencíveis com controles reais.
4. `CharacterManager2D` tenta reaparecer automaticamente ao receber `RespawnRequest`; isso explica uma possível recuperação de vida entre capturas, mas não comprova a causa daquela observação. A experiência de derrota/retomada precisa ser observada e comunicada ao jogador.
5. `SliceHud2D.RestartCompletedSession` oferece reinício somente após `IsComplete`, carrega a cena pelo buildIndex e condiciona o reinício ao salvamento de avaliação quando há recorder. Falha de gravação pode impedir repetir a partida. Verificar o comportamento desejado e um caminho de recuperação antes do aceite.

## Próxima validação

- Build Development do mesmo commit em `outputs/hd-validation/playable-phase-20261021`.
- Percorrer intro, corredor, capitão e saída com input real, sem `ClearEnemies` ou teleporte.
- Registrar se o jogador entende inimigos restantes, morte e retomada.
- Testar cinco conclusões e reinício; reportar falhas como falhas, sem extrapolar de testes de componentes.

## Revisão enviada ao Antigravity

O candidato inicial de câmera limita o destino antes de interpolar. Após ampliar a janela de câmera, a pose anterior pode permanecer fora dos novos limites durante a interpolação. Solicitação de teste e correção registrada em `D:/Workspace/Br.JcodeArch.Huntrix/COORDINATION_CODEX_REVIEW.md`.
