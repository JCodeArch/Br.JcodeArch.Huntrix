# Dreno real de fãs — cartão #44

Fonte: [Trello #44](https://trello.com/c/drW9VawR).

O `FanSoulDrainController2D` usa EnemyAgent #33 para identidade/vida/configuração do demônio e um FanRegistry explícito, sem converter fãs em alvos de combate. Seleciona o fã elegível mais próximo de até 64 referências, reserva telegraph, revalida alcance, proteção e linha de visão e então aplica `TryDrain`. Cooldown e telegraph usam tempo escalado, sem pulses durante pausa. Raycast de linha de visão é limitado a 32 contatos e rejeita saturação. Dreno não atravessa geometria sólida.

`FanDrainAccepted(fan, actualSoulLost)` só publica após uma alteração aceita, sem ganho implícito, moeda ou alteração duplicada de Honmoon. O cartão #47 compõe o adapter de Honmoon a esse evento; #44 não referencia o controller futuro para manter ordem de dependências. O protótipo de dano de combate “soul drainer” do cartão #35 continua separado: este é o comportamento real sobre fãs.

Assets: `FanDrain_Prototype.asset`, `FanSoulDrainer_Prototype.prefab` e `FanDrainEncounter_Prototype.prefab`. O encounter fornece registry, três fãs resgatáveis, piso provisório e um ponto; `FanDrainEncounterBootstrap2D` instancia um drainer em raiz independente e vincula o registry explicitamente. `autoStart` chama uma tentativa em Start; falha não gera retry automático por frame. `ActiveDrainer` e `DrainerCreated` permitem integração tipada de eventos sem Find. Desativação limpa apenas o demônio pertencente ao bootstrap. O inimigo mostra um marcador amarelo durante telegraph; a apresentação é geométrica provisória.

Para experimentar o fluxo completo: arrastar encounter para uma cena com câmera, inicializar um player slot separado, mover a integrante para perto do fã e usar a ação lógica #43 para resgatá-lo. O drainer age automaticamente; input de resgate ainda deve ser roteado pelo integrador. O cartão #47 liga variação de Honmoon, e #51 interpreta as recompensas de resgate.

Status: **Implementado — validação pendente**. Valores raio 6, dreno 10, telegraph 0.6s e cooldown 1s são provisórios. QA final deve validar LOS/saturação, troca de alvo, proteção, estados até Corrupted, morte/desativação do drainer, callbacks/reentrada, fontes/cancelamento e integração Honmoon sem duplicação.
