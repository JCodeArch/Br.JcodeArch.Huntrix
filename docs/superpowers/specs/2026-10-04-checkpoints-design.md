# Checkpoints e tentativas — Card 27

## Contexto e objetivo

O card #27 implementa checkpoint de fase e coordenação do ciclo de tentativa. Ele fornece um pedido de respawn explícito para que o `CharacterManager` do card #31 possa aplicar a recuperação ao personagem ativo. O projeto já define progressão por capítulo e checkpoints como limites de persistência futura, mas ainda não possui storage runtime; esta entrega mantém o estado da fase em memória.

## Decisões

- Um checkpoint é um ponto nomeado e identificável na cena, representado por um `CheckpointAnchor2D` e um `Transform` de destino. O uso é explicitamente ativado pela camada de gameplay; adicionar trigger/collider e regras de level design fica para o card de integração da fase.
- O fluxo recebe um destino inicial da fase. Esse destino é usado até um checkpoint válido ser ativado.
- O checkpoint mais recentemente ativado vence. A chamada de ativação valida a referência, o ID e o destino antes de alterar o estado ativo. IDs são comparados ordinalmente e não se presume ordenação de progresso.
- A tentativa é iniciada explicitamente com o ator controlado. O fluxo se inscreve no evento de morte de um `DamageReceiver2D` configurado e vivo e o desinscreve ao trocar/desativar. A troca por outro ator vivo durante uma tentativa mantém o mesmo número de tentativa; o ator anterior deixa de ser observado.
- `DamageReceiver2D` publica `Died` uma única vez quando a vida cruza de positiva para zero, após resolver o knockback já aceito. Hits posteriores continuam rejeitados pela regra existente.
- A morte encerra uma tentativa uma única vez e publica `RespawnRequested` com número da tentativa, ID de checkpoint, posição e rotação copiados do destino ativo. O consumidor (futuro `CharacterManager`) decide quando aplicar o pedido e iniciar a próxima tentativa.
- O fluxo não move nem cura o personagem, não limpa controladores de movimento/combate e não reseta inimigos. Essas ações pertencem ao consumidor do pedido no card #31 e aos sistemas próprios.
- Sem autosave real, arquivo, cloud, snapshot do combate/cena, vidas, penalidades ou regra nova de cura. O destino inicial cobre a retomada runtime quando nenhum checkpoint foi ativado. Persistência da referência do checkpoint depende do futuro sistema de save.
- Não adicionar o fluxo ao `GameBootstrap`: ele pode ser composto pela cena/CharacterManager quando a integração existir. Se a implementação revelar que `GameBootstrap` é necessário, pausar o desenho e incluir contrato e teste da falha de inicialização parcial antes de conectar sistema stateful.

## Componentes e contratos

- `CheckpointAnchor2D`: componente leve de cena com ID e destino opcional (usa seu próprio `Transform` quando nenhum destino separado for atribuído). Oferece uma leitura validada de seus dados; não se ativa sozinho.
- `RespawnRequest`: valor imutável com `AttemptNumber`, `CheckpointId`, `Position` e `Rotation`.
- `CheckpointAttemptFlow2D`: componente de cena que recebe destino inicial, vincula ator ativo, inicia/encerra tentativa, ativa checkpoint e publica `RespawnRequested`. Todas as mutações são chamadas explícitas; não há busca por objetos por frame nem serviço global.
- `DamageReceiver2D.Died`: evento de domínio disparado somente na transição viva→morta após o knockback aceito. Cada subscriber é isolado; exceção é registrada e não impede os seguintes. `CheckpointActivated` fornece ID aos futuros consumidores de save.

## Falhas e lifecycle

- Configuração nula/inválida (destino inicial ausente, ID vazio, âncora/destino destruídos ou inativos, transformação inválida, ator inválido ou morto) é rejeitada com retorno falso e estado atual preservado; não lança exceção em callbacks de gameplay.
- Trocar o ator remove o handler antigo antes de conectar o novo. Ator morto ou inválido é rejeitado sem afetar o vínculo atual. `OnDisable` desinscreve e cancela a tentativa atual; reativação exige novo vínculo e nova chamada de início, com contador monotônico.
- Uma tentativa já encerrada ignora mortes duplicadas. Nova tentativa recebe número monotônico e pode ser iniciada apenas com ator configurado e vivo.
- Se o destino do checkpoint ativo for destruído, o pedido usa o destino inicial ainda válido; se nenhum destino for válido, não publica respawn e registra diagnóstico único. Não escolhe checkpoint por posição, ordem serializada ou proximidade.

## Fora de escopo

Integração de input/trigger, personagem final, CharacterManager, mudança/troca de personagem, restauração de estado do ator, vida extra, cura, feedback visual/sonoro, transição de cena, persistência concreta, migração/sincronização, reset/respawn de inimigos, co-op e balanceamento.

## Verificação

- EditMode: âncora válida/inválida, destino próprio/alternativo, ativação do último checkpoint, fallback inicial, payload do pedido, incremento da tentativa, morte duplicada, troca de ator e lifecycle/desativação.
- PlayMode: dano que mata emite um único evento de morte após knockback; fluxo vinculado produz um único pedido com checkpoint certo; hits posteriores não repetem o evento; reativação/rebinding não duplica inscrições; ator destruído pode ser religado; ativação desativada/inativa é recusada; segunda tentativa preserva checkpoint; destino destruído usa fallback ou rejeita com diagnóstico; callback observa knockback já aplicado; assinantes com exceção não bloqueiam os seguintes.
- Executar suítes EditMode e PlayMode completas e regressões de saúde, movimento, dash, pulo, combate e parry. Unity Editor disponível: 6000.6.4f1.
- SOLID e Performance revisam o contrato real após implementação. Sem alvo de dispositivo aprovado, registrar avaliação estática, sem alegar benchmark.
- Child Safety/privacidade: N/A para fluxo local sem conteúdo ou coleta; acessibilidade: N/A para API lógica sem input/feedback. Registrar justificativas no Trello.

## Agentes e integração

Orchestrator, System Architect e QA revisaram a definição antes do código. O Antigravity foi consultado via `agy --mode plan --sandbox`; não foi possível obter parecer porque a CLI não está autenticada nesta sessão. SOLID, Performance e QA final revisarão a mudança executável. Integration verificará commit próprio na feature, fast-forward de `develop`, refs remotas e árvore limpa.
