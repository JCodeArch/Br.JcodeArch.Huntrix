# Resgate de fãs — cartão #43

Fonte: [Trello #43](https://trello.com/c/Vx2NROMc).

`FanRescueController2D` compõe um FanActor. `TryBeginRescue(rescuer)` é a ação lógica de interação: exige uma integrante viva/habilitada da facção HUNTR/X, dentro do alcance, e um fã enfraquecido. Não impõe binding físico. O integrador de interação deve chamar a ação para o fã selecionado; criar proximidade, UI ou input físico não é responsabilidade desse controller.

Durante recuperação temporizada, o controller registra proteção explícita que só vale enquanto fonte/rescuer/fã estiverem habilitados, vivos/configurados e próximos. A recuperação conserva progresso se interrompida. Ao retornar a Normal, encerra proteção e reserva conclusão antes de publicar `RescueCompleted(FanActor2D,int rewardAmount)`; nenhuma recompensa é emitida no cancelamento ou em pedido sobre um fã já Normal. O valor é sinal para economia/Performance/Honmoon posteriores, sem saldo ou persistência neste cartão. `CompletedRescueCount` registra conclusões locais deste controller.

Esta proteção é do resgate. Não equivale a integração dos campos de Mira com fãs, que possuem identidade independente do pipeline de combate. Cooperação, escolha de múltiplos resgatadores, prevenção de exploração/balanceamento e input final permanecem nos cartões próprios.

Assets: `FanRescue_Prototype.asset`, `RescuableFan_Prototype.prefab` e `RescuableFanGroup_Prototype.prefab`. Composição real: instanciar grupo, obter o FanRescueController do fã escolhido e chamar `TryBeginRescue(manager.ActiveCharacter)` quando a ação lógica de interação ocorrer. Atualizações do controller conduzem recuperação e publicação automaticamente.

Status: **Implementado — validação pendente**. Duração 2s, alcance 2 e recompensa 5 são fixtures provisórias. QA final: range, cancelamento por troca/morte/disable, fontes sobrepostas, recuperação com alimento, callbacks reentrantes, ganho único por conclusão e nenhuma recompensa no cancelamento.
