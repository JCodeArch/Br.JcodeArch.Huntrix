# Inimigos de suporte — cartão #35

Fonte: [Trello #35](https://trello.com/c/lq4Getd6), “Healer, protector e soul drainer”. O protótipo é composto pelo EnemyAgent #33 e um controller de pulsos estacionários. Não adiciona perseguição automática, rede, input, arte, recursos de fãs ou Honmoon.

Healer restaura até o máximo de vida de um demônio aliado vivo e habilitado dentro do raio; não cura a si mesmo nem revive. `DamageReceiver2D.TryRestoreHealth` valida origem viva, facção igual, valor finito positivo e vida atual antes de alterar estado. Protector registra a própria fonte apenas em aliados que optam via `DamageProtection2D`; o contrato externo revalida origem, vida, facção, distância e duração em cada pedido de dano. Fontes de Mira continuam independentes. Desativação/morte/expiração removem os registros próprios.

Soul drainer é um **protótipo de dano de combate**, dirigido ao alvo explícito do EnemyAgent, com range e pulsos configuráveis, respeitando parry/proteção/dash pelo pipeline existente. `DrainAccepted(target, actualHealthLost)` só sinaliza perda aceita. Não cura o atacante, drena Honmoon, almas persistidas, fãs, moeda ou recompensas; integração de economia requer os cartões próprios. O raio é uma zona de efeito, sem contrato de linha de visão; não se declara projétil/beam final.

A consulta de aliados é limitada a 32 colliders, inclui triggers, deduplica receptores e rejeita um buffer cheio. Não usa buscas globais nem consulta de física por frame; cooldown e duração usam tempo escalado. Valores de assets são provisórios. Death callbacks não introduzem novos sistemas de recompensa.

Status: **Implementado — validação pendente**. Nenhum teste escrito/executado nesta rodada; QA final deve validar facções, cap/no-revive, fontes sobrepostas Mira+suporte, saída do raio, desativação, triggers múltiplos, saturação, drain protegido/aparado e ciclo de vida dos assets/prefabs.


## Assets e composição

`HealerDemon_Prototype`, `ProtectorDemon_Prototype` e `SoulDrainerDemon_Prototype` incluem EnemyAgent, receiver/Hurtbox, corpo dinâmico, visual provisório, opt-in único de proteção e o controller de suporte como único driver (sem GroundEnemyController). Os assets `Data/Enemies/Support` configuram os três papéis e ataque de dreno provisório. O drainer requer alvo explícito via `EnemyAgent.TrySetTarget`; healer/protector consultam apenas seu raio local. `EnemyAgent.Tick` é conduzido pelo suporte; nenhuma segunda IA é composta.
