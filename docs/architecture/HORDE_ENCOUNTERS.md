# Swarm/horda — cartão #36

Fonte: [Trello #36](https://trello.com/c/R8iF7ol3), “Sistema de grupos com limites de performance”. `HordeDefinition` referencia um prefab inimigo #33, concorrência 1–64, total 1–256 e intervalo finito positivo. Os limites são barreiras técnicas do protótipo, não orçamento mobile medido nem balanceamento final.

`HordeController2D` recebe pontos de spawn e um alvo explícito ou CharacterManager por slot. `TryStart` exige alvo vivo e configuração válida. O controller instancia no máximo um inimigo por frame, sem catch-up de pausas, em raízes independentes e pontos circulares; define o alvo no EnemyAgent e mantém uma lista limitada de instâncias próprias. Morte/desativação remove e destrói somente instâncias da horda. Alvo do manager é atualizado para acompanhar troca e respawn; enquanto indisponível, pausa novos spawns e para agentes. Não há descoberta global de jogadores.

Apenas o término natural (total emitido e nenhum inimigo vivo) marca `IsComplete`. Cancelamento/desativação não é vitória. Falha de ponto/prefab/spawn encerra emissão, sem retry infinito; já emitidos continuam rastreados até limpeza. `Stop` desativa/destrói todos os próprios. Sem pooling nesta etapa: alocações de instantiate/destroy precisam de perfil antes de estabelecer orçamento real. Não cria offspring automaticamente nem representa uma cena completa.

Composição: configurar prefab/definition e pontos, fornecer manager ou receiver explicitamente e chamar `TryStart` pelo integrador da fase. Slots/inimigos nunca devem ser parentados sob o mesmo root de combate. `GroundHordeEncounter_Prototype` oferece manager/flow/controller de troca, pontos, piso de colisão provisório e `EnemyEncounterBootstrap2D` com referências explícitas. `autoStart` inicia uma única vez em Start; `TryBegin` permite tentativa explícita, sem retry por frame. O bootstrap configura o slot e inicia a onda, sem controles físicos ou descoberta global. A composição deve ser arrastada para uma cena com câmera.

Status: **Implementado — validação pendente**. Testes pendentes: caps concurrent/total/cadence, não burst, death/disable externo, target swap/respawn, stop vs complete, spawn inválido, cleanup, roots/facções e alocações no dispositivo. Nenhum teste escrito/executado nesta rodada.


O início do encontro não promete uma transação entre subsistemas: se o jogador for criado e a horda recusar configuração, o jogador permanece válido; após corrigir referências, `TryBegin` pode ser chamado novamente. O bootstrap revalida seu estado após callbacks de spawn para não iniciar uma onda depois de ser desativado. Cancelamento de horda protege reentrada e remove ownership antes de publicar callbacks de teardown.
