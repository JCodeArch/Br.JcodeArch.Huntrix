# Time paralelo — 10/10/2026

## Coordenação e capacidade

O proprietário solicitou especialistas por assunto e progresso paralelo. A sessão dispõe de **sete vagas simultâneas**, incluindo o coordenador: seis especialistas podem trabalhar ao mesmo tempo. A preferência por mais de dez agentes é atendida por rodízio de especialidades quando necessário, sem afirmar que mais agentes estão ativos do que o ambiente permite.

A execução ocorre durante a sessão ou tarefa efetivamente ativa. Este registro não cria execução persistente, automação ou acompanhamento após o encerramento. Os nomes abaixo representam responsabilidades da rodada; estados atuais devem ser confirmados pelo coordenador antes de comunicar atividade ao proprietário.

## Responsabilidades da rodada #31/#32

| Responsável | Entrega e arquivos sob sua responsabilidade | Dependências e cobrança direta |
|---|---|---|
| Coordenador | Escopo/Trello, sequência de integração, `PROJECT_RULES.md`, `docs/PROJECT_CONTEXT.md`, atualização ao proprietário | Recebe bloqueios e decisões; verifica entrega e estado real dos agentes; não duplica edição dos especialistas |
| Arquiteto | Contratos de CharacterManager e troca; documentos de arquitetura/design correspondentes | Confirma requisitos dos cards; entrega API e invariantes diretamente a gameplay e troca; revisa mudanças de contrato antes de adoção |
| Especialista gameplay/CharacterManager | Runtime de gestão, cache, personagens ativos e ciclo de vida; arquivos específicos do manager | Cobra contrato do arquiteto; informa API disponível ao responsável pela troca; corrige findings de runtime recebidos do revisor |
| Especialista troca/integração | Runtime e composição da troca; documentação correspondente; publicação Git/Trello somente sob coordenação | Cobra API do manager diretamente ao gameplay; prepara contra contrato e integra após dependência; não altera arquivos do manager sem coordenação |
| Especialista QA/documentação | `docs/governance/DEFERRED_VALIDATION.md`, `DEFINITION_OF_DONE.md`, este registro e critérios de validação pendente | Recebe contratos e findings de arquitetura/revisão; mantém matriz honesta; execução Unity permanece adiada conforme decisão explícita |
| Revisor independente de código/SOLID | Findings, rechecagem de correções e relatório de revisão; não modifica runtime do autor | Examina diff completo e contratos; cobra correções diretamente ao autor e informa o coordenador sobre bloqueadores |
| Especialista Unity/assets/desempenho | Revisão de prefabs, `.meta`, GUIDs, composição e riscos de plataforma; arquivos de assets atribuídos | Cobra referências/API dos autores; coordena qualquer alteração compartilhada; não declara importação ou desempenho aprovado sem execução/medição |

O coordenador atribui caminhos concretos antes de cada edição. Quando um arquivo precisar de dois especialistas, um deles fica responsável pela edição e o outro envia a alteração proposta. Revisor e QA fornecem findings sem sobrescrever implementação alheia.

## Comunicação de dependências

O especialista bloqueado envia ao responsável: contrato/arquivo necessário, entrega esperada e impacto na própria tarefa. Informa o coordenador em seguida. O responsável confirma a disponibilidade ou identifica a decisão pendente. Findings são enviados diretamente ao autor; questões de contrato voltam ao arquiteto. O coordenador acompanha as cobranças e reorganiza vagas se surgir outro assunto que exija especialista.

Preparação paralela não autoriza integração fora da ordem. #32 pode ser preparado contra o contrato de #31, mas a dependência deve estar revisada e integrada antes da integração de #32.

## Critério de integração

Aplicar a exceção de [validação adiada](DEFERRED_VALIDATION.md) e a [Definition of Done](DEFINITION_OF_DONE.md): revisão independente sem bloqueadores, escopo/contrato preservados, verificações estáticas aplicáveis, documentação e commit identificável por card, confirmação da branch/PR e registro no Trello.

O estado após integração é **Implementado — validação pendente**. Testes escritos ou revisão estática não comprovam compilação, importação, física ou gameplay. Conclusão integral da DoD e qualificação de release aguardam execução runtime final. Preservar os testes existentes e registrar cenários necessários, sem executar Unity nesta rodada.

## Estado registrado e próxima transição

Rodada integrada em 10/10/2026: #31 (CharacterManager) pelo PR #3 e #32 (troca) pelo PR #4, em branches próprias e na ordem das dependências. Revisão estática aprovada; validação runtime pendente. As seis entregas especializadas desta rodada foram concluídas; isso não afirma execução contínua após a sessão. A política de QA documental foi revisada e aprovada, com testes Unity adiados. O estado de #30, commits, PRs e integrações deve ser consultado no contexto e Trello atuais.

## Rodada #33–#40

Em 10/10/2026, o proprietário autorizou avançar até Jinu (#40). Foram atribuídas seis frentes simultâneas, com dependências cobradas diretamente entre os responsáveis:

| Responsável | Escopo desta rodada |
|---|---|
| architecture | Contratos comuns, guia de integração, revisão de assets e limites de desempenho |
| gameplay | #33 terrestre melee, foundation compartilhada e #34 voadores/projéteis |
| integration | #35 healer/protector/soul drainer e #36 hordas com orçamento limitado |
| qa_environment | #37 elites/padrões telegráficos e #38 mini-boss com fases |
| unity_validation | #39 rivais Saja Boys e #40 Jinu com evolução de combate e cues narrativos |
| review | Revisão independente incremental, cobrança de findings e conferência das correções |
| Coordenador | Branch/commit/PR por card, integração sequencial em develop, Trello e atualizações |

Os nomes dos agentes são identificadores reutilizados; a coluna de escopo descreve o trabalho efetivo desta rodada. O responsável por QA também implementou padrões e mini-boss; a revisão independente foi atribuída a outro agente. Nenhuma execução Unity foi encomendada: cenários finais permanecem na matriz de validação diferida.

A ordem de integração é #33 → #34 → #35 → #36 → #37 → #38 → #39 → #40. Os contratos compartilhados pertencem à foundation; adaptações específicas ficam em arquivos próprios. O guia `docs/enemies/INTEGRATION_GUIDE.md` registra composição e limitações. O estado publicado e os PRs devem ser consultados no contexto atualizado e nos cards, sem inferir execução persistente após a sessão.
