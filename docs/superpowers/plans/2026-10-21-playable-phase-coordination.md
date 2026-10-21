# HUNTR/X — plano de entrega da primeira fase jogável

> Execução: Codex e Antigravity, com revisão cruzada. Continuação do plano aprovado de 19/10. Antigravity aceitou a divisão em 21/10, seção 5 de `COORDINATION_ANTIGRAVITY_CODEX.md`; Codex aceitou sua correção de dependência: Rumi completa antes da calibração definitiva de escala.

**Objetivo:** entregar uma fase curta completa, visualmente coerente, com as três Hunters, combate legível, capitão, vitória, derrota e reinício verificáveis.

**Base verificada em 21/10/2026:** clone `feature/hd-presentation-recovery`, commit `c6bfde0c01fbe8d8994180125cd80e7c094ed509`, handoff Supabase 148. A apresentação do gameplay permanece reprovada. A prova HD da Rumi contém somente Idle e não substitui o conjunto jogável.

**Referência:** plano aprovado `2026-10-19-huntrx-hd-effects.md`. Campanha, cooperação e demais personagens continuam no escopo total; esta fase é o primeiro marco de qualidade.

## Divisão proposta e evidência

Não existe evidência suficiente para declarar superioridade geral de um agente.
O Antigravity já entregou o importador e uma proposta de câmera; isso sustenta sua responsabilidade inicial por câmera e composição.
O Codex integrou e revisou as entregas, executou suites e builds e identificou regressões e falhas visuais; isso sustenta sua responsabilidade inicial por integração e aceite.
Arte exige revisão visual por resultado. Nenhum agente recebe aprovação artística automática.

| Ordem | Entrega | Executor proposto | Revisor | Dependência e aceite |
|---|---|---|---|---|
| 1A | Câmera única e limites de fase | Antigravity | Codex | Reconciliar código com clone; bordas, salto, troca, reinício, 16:9 e 20:9; centralizar também quando altura ou largura forem menores que a janela |
| 1B | Roteiro de teste completo e diagnóstico de progressão | Codex | Antigravity | Início, objetivo, combate, capitão, saída, derrota, checkpoint e reinício; registrar bloqueios reproduzíveis |
| 2A | Escala, sobreposição e leitura de ataques | Antigravity | Codex | Depois de 1A e Rumi completa em 2B; comparar corpo visível com hurtbox e alcance; heroína reconhecível entre dois inimigos; antecipação inimiga preservada |
| 2B | Animações HD do trio | Codex | Antigravity e revisão visual | Rumi primeiro; Idle, Run, Jump/Fall, Attack, Hurt, Dash e Death completos; depois Mira e Zoey; sem recortes, arma cortada ou estados incompatíveis |
| 3A | Encontros e capitão com progressão confiável | Codex | Antigravity | Depois de 1B; inimigos alcançáveis, saída com requisito explícito, dois padrões legíveis do capitão, checkpoint consistente |
| 3B | Cenário, HUD e áudio coerentes | Antigravity | Codex | Depois de 1A/2A; cenário em camadas, piso alinhado, textos legíveis, objetivo claro, áudio audicionado e origem registrada |
| 4 | Efeitos e opções de conforto | Antigravity | Codex | Depois de 2A/2B; efeitos reforçam autoria do golpe; tremor zero e modo reduzido mantêm informação de perigo |
| 5 | Build e aceite integrado | Codex | Antigravity | Depois de 2–4; testes, build do mesmo commit, partidas completas e relatório de desempenho |

## Contrato de colaboração

- Um executor por arquivo em cada lote; nenhum espelhamento de diretório completo.
- Codex integra no clone; Antigravity trabalha no checkout D e entrega patch seletivo com base, arquivos, evidência e limitações.
- Antes de aplicar, comparar a base: a proposta anterior de câmera descreve valores diferentes dos atuais no clone.
- Tarefas ficam em revisão até evidência visual e funcional. Teste automatizado aprovado não comprova diversão ou qualidade de animação.
- Limite de trabalho simultâneo: uma entrega por agente. Terminar e revisar antes de abrir outra frente.
- Dúvida ou alternativa melhor deve vir com impacto, arquivos e demonstração; o outro agente revisa antes da integração.
- Se o parceiro não responder, registrar isso e assumir somente tarefa sem conflito de propriedade confirmado.
- Trello registra tarefa, executor lógico, revisor, dependências, critérios, commit e evidências. Não inventar contas de agentes como membros do Trello.

## Primeiro lote técnico

### 1A — câmera

Arquivos candidatos: `StageBlockoutDefinition.cs`, novo `SliceCameraDirector2D.cs`, ligação em `VerticalSliceSession2D.cs` e remoção da escrita da câmera em `SlicePlayerPresentation2D.cs`.

- [ ] Antigravity confirma a base atual e propõe patch sobre ela.
- [ ] Testes de limites incluem estágio menor que viewport em ambos os eixos.
- [ ] Um único componente escreve a pose final; parallax apenas lê.
- [ ] Troca de fase/heroína, pausa, reinício e mudança de proporção não mantêm alvo antigo.
- [ ] Codex revisa patch, executa regressões e observa o build.

### 1B — percurso completo

Arquivos a inspecionar: `VerticalSliceSession2D.cs`, `SliceAirplaneBeat2D.cs`, `SessionHudLifecycleTests.cs`, controladores de checkpoint e HUD.

- [ ] Construir o commit identificado e registrar todos os passos do percurso.
- [ ] Reproduzir morte, retomada, vitória e reinício; investigar cada falha antes de alterar código.
- [ ] Criar regressão para cada defeito funcional confirmado e corrigir em lote isolado.
- [ ] Registrar duração e trechos confusos; não confundir comandos enviados com ações observadas.

## Critério do primeiro marco

- Fase com alvo de 3–5 minutos, introdução breve, aprendizado, escalada e conclusão.
- Cinco conclusões consecutivas sem bloqueio de progressão, com versão e roteiro registrados.
- Morte e reinício testados deliberadamente, sem duplicação de atores, VFX ou HUD.
- As três Hunters identificáveis, com roupas e armas fiéis às referências.
- Ataques mostram antecipação, contato e recuperação; fundo e VFX não escondem perigos.
- PC: medir 60 segundos após aquecimento; alvo existente de 95% dos frames abaixo de 16,7 ms, com hardware registrado.
- Android: build e teste em aparelho identificado; aprovação permanece pendente sem medição real.
- Aprovação final da experiência inclui a avaliação do proprietário. “Perfeito” não é substituído por contagem de testes.

## Estado de publicação

Plano principal publicado: https://trello.com/c/P3KJ2Qd8.
Câmera: https://trello.com/c/O2V57jsW.
Subtarefas publicadas: [percurso](https://trello.com/c/2CvtFzVm), [animações HD](https://trello.com/c/rTFx0FW9), [escala](https://trello.com/c/4xWDx54h), [encontros/cenário/HUD/áudio](https://trello.com/c/9aaBeY30), [efeitos](https://trello.com/c/PiF2O2UK) e [aceite](https://trello.com/c/seXAmsJt).
Antigravity entregou 1A em D com correção de clamp final. Codex aplicou seletivamente o solver, diretor e testes no clone, adaptando a ligação de câmera que divergia no patch. Integração validada no Unity 6000.6.4f1: EditMode 152/152 e PlayMode 550/550, incluindo 9+3 testes novos (`outputs/hd-validation/playable-phase-20261021/*-edit.xml` e `*-play.xml`). Build Development anterior, de c6bfde0, concluído com zero erros/avisos, manifesto em `outputs/hd-validation/playable-phase-20261021/Windows.evidence.json`; não inclui a câmera. É necessário novo build e playthrough real para aceitar a apresentação e o gameplay.
