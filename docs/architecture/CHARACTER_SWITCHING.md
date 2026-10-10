# Troca de HUNTR/X — cartão #32

Fonte: [Trello — Implementar troca de HUNTR/X](https://trello.com/c/4qIS7hJ9), cujo requisito é “Troca rápida e segura durante o gameplay”. O GDD mantém condições e valores finais em aberto. Este contrato delimita o protótipo solo por slot, sem implementar rede ou controles físicos.

## Seleção lógica

`CharacterSwitchController2D` configura um `CharacterManager2D` e um roster ordenado de pelo menos dois prefabs distintos. `Configure` valida antes de alterar referências e copia o array para impedir mutações externas. `TrySwitchNext(out error)` solicita o próximo índice circular; `TrySwitchTo(index, out error)` solicita um alvo explícito. O índice atual é calculado por `manager.ActivePrefab`, evitando desatualização após respawn ou seleção externa.

Pedidos exigem controller e manager habilitados, manager no estado `Alive`, personagem ativo vivo e presente no roster. Índices inválidos, alvo já ativo, entries nulas/duplicadas e reentrada são recusados. O controller não tenta automaticamente outro alvo quando o próximo é recusado. Não existe cooldown adicional presumido.

## Propriedade e efeitos

O manager #31 é o único dono das instâncias, cache, vida individual, pose, inputs transitórios, vínculo de checkpoint/tentativa e admissão por estado de ação. `TrySwitch` aplica suas regras de segurança e mantém o ator original se recusar o pedido. O controller nunca instancia, destrói, aplica dano/cura, altera tentativas ou manipula componentes do ator. Eventos de alteração pertencem a `CharacterManager2D.ActiveCharacterChanged`; não há subscription duplicada ou evento de apresentação adicional neste controller.

A política do protótipo preserva o estado individual de cada personagem no cache do manager. A troca não é um respawn nem uma cura. Dash, ataque, parry, campo da Mira e recarga da Zoey são avaliados pelo manager para impedir evasão de estados transitórios; o contrato detalhado permanece em `CHARACTER_MANAGER.md` quando disponível. Final balanceamento, disponibilidade/desbloqueio, regras cooperativas e câmera continuam fora deste cartão.

## Composição e pendências

O componente é um adaptador lógico para futura entrada `SwitchCharacter`, sem mapping de teclado/gamepad/touch. `Resources/Characters/HuntrXPlayerSlot_Prototype` fornece manager, controller e fluxo de checkpoint no mesmo slot, com roster Rumi/Mira/Zoey serializado e um filho `StageStart`. Não altera a cena CombatLab nem cria atores automaticamente. Isso preserva o dono único e evita criar managers em cada personagem.

Status: **Implementado — validação pendente**. O usuário adiou os testes para a fase final; nesta rodada o time priorizou a implementação e registrou os cenários pendentes, sem adicionar uma nova suíte ou executar testes. Não há evidência de compilação Unity, teste de cena ou game feel.

Cenários de validação pendentes: ciclo Rumi → Mira → Zoey → Rumi; seleção direta e índices inválidos; configuração nula/duplicada e array externo mutado; fonte fora do roster; manager/controller desabilitados; morte/respawn; ações e recarga que bloqueiam troca; alvo inválido/morto; reentrada durante evento do manager; preservação de checkpoint/tentativa/vida individual/pose; limpeza de input; ausência de duplicação de ownership; integração física de input e câmera.


## Uso da composição mínima

Arraste `HuntrXPlayerSlot_Prototype` para a cena e posicione `StageStart` no ponto inicial válido. O bootstrap/integrador da fase deve obter o manager e o `CheckpointAttemptFlow2D` do slot, chamar `Configure(rumiPrefab, slotFlow, stageStart, "stage-start")` e depois `TrySpawn()`, verificando os dois retornos. A configuração do manager é explicitamente runtime, não um campo serializado deste prefab. Cada slot deve usar seu próprio fluxo; não compartilhar um flow entre managers. A entrada lógica pode então chamar `TrySwitchNext(out error)` ou `TrySwitchTo(index, out error)` no controller.

O prefab facilita a composição, mas não é uma demo pronta ao apertar Play: inicialização de fase, controles físicos, sensores e câmera continuam integradores pendentes. Os atores criados pelo manager permanecem raízes independentes da cena, sem parenting sob o slot, preservando ownership `transform.root` do combate e campo da Mira. Importação e execução deste prefab ainda exigem validação Unity.
