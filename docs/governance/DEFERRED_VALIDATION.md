# Validação Unity adiada

## Decisão do proprietário — 10/10/2026

O proprietário do projeto, Jonathas Silva de Souza (Joe), instruiu explicitamente o time a adiar a execução dos testes Unity até o jogo estar quase pronto e continuar o desenvolvimento em paralelo. Este registro modifica o momento da validação, sem declarar aprovação de compilação, testes ou gameplay.

A implementação pode ser integrada à `develop` após revisão estática sem findings bloqueadores, verificação dos arquivos/referências disponíveis, documentação e rastreabilidade de cada card. O estado utilizado é **Implementado — validação pendente**. O trabalho dependente pode avançar com a pendência registrada. O card permanece sem conclusão integral da Definition of Done; qualificação de release e distribuição dependem da validação runtime final.

A decisão não autoriza dispensar requisitos de segurança infantil, privacidade ou direitos/licenciamento. Os testes existentes ficam preservados. Ausência de execução é registrada como **Não executado — adiado pelo proprietário**, nunca como teste aprovado ou gate N/A.

## Evidência e riscos

Revisão estática pode verificar responsabilidades, contratos, caminhos de erro, referências textuais, GUIDs e consistência documental. Ela não comprova compilação no Unity, importação dos assets, comportamento da física, ordem de Awake/OnEnable/Start, entrada real, carregamento de cenas ou compatibilidade de builds.

O adiamento acumula riscos de erros de compilação/API, referências serializadas inválidas, interações entre componentes e regressões de personagens. Mudanças dependentes podem precisar de retrabalho quando executadas. Performance móvel, renderização e acessibilidade em dispositivos continuam sem comprovação quando não medidas. O acúmulo deve ser visível na matriz abaixo e no Trello, com o commit correspondente de cada implementação.

Resultados históricos de outros commits não aprovam o código atual. A validação final deve identificar o SHA testado e repetir os testes afetados após qualquer correção relevante.

## Matriz inicial de validação pendente

Os cards #30–#32 foram implementados e integrados em develop com revisão estática, sem execução Unity: PR #1 (Zoey), PR #3 (CharacterManager) e PR #4 (troca). Commits de implementação: f9be88f, c40886e e 498594c, respectivamente. Cenários desta matriz continuam pendentes; integração não equivale a aprovação de runtime.

| Card | Escopo a verificar | Evidência disponível/esperada antes da execução | Validação Unity pendente |
|---|---|---|---|
| #30 — Zoey | Disparo de precisão, alcance, mira aérea, cooldown, primeiro contato, bloqueadores, facção, parry e proteção | Implementação e testes escritos; revisão estática; perfil/prefab e GUIDs conferidos; commit registrado no card | Compilação/importação; executar os 26 casos novos e regressão relevante; conferir prefab real e Combat Lab; validar input físico somente quando integrado |
| #31 — CharacterManager | Gestão das três personagens, identidade, instâncias e ciclo de vida segundo contrato final do card | Contrato e composição documentados; código e referências revisados; commit próprio e limitações registrados | Instanciar personagens reais; verificar ativação/desativação, preservação de instâncias/estado e limpeza; cenas e regressão de combate, proteção e checkpoints conforme composição implementada |
| #32 — Troca de personagens | Solicitação de troca, seleção válida e transições segundo contrato final do card | Contrato de input/troca e estados documentado; revisão dos caminhos inválidos e dependências; commit próprio | Trocas repetidas e limites; ausência de input duplicado; encerramento das ações conforme contrato; integração com câmera, morte/respawn e checkpoints quando abrangidos; execução solo com as três personagens |

### Inimigos e encontros — #33–#40

Implementações e assets receberam revisão estática independente nesta rodada. Os cards continuam com validação runtime pendente; o Trello registra o PR/commit publicado de cada feature. Não foram escritas nem executadas novas suítes de inimigos nesta rodada, conforme a priorização do proprietário.

| Card | Validação final necessária |
|---|---|
| #33 — Terrestre | Importar prefab/dados; perseguição, detecção, telegráfico e contato melee; paredes, facções, múltiplos colliders, parry/dash/proteção; morte, disable, callbacks e saturação de buffer |
| #34 — Voadores | Altitude variável e ascensão após dive; mergulhos sucessivos, obstáculos e retarget durante ação; projétil swept, primeiro contato, consumo único, lifetime/range e limite por fonte |
| #35 — Suporte | Cura sem revive/overflow, elegibilidade e dedup; proteção de fontes sobrepostas e limpeza por morte/disable/distância; dreno aceito/rejeitado por combate; integração Honmoon continua futura |
| #36 — Horda | Limites vivos/total/cadência, uma instanciação por frame, cancelamento versus conclusão; falha parcial e cleanup reentrante; mudança/respawn do alvo; medir custo e alocações com carga real |
| #37 — Elites | Alternância real jab/sweep, hitboxes e tempos; ciclos, descanso, listener reentrante e um único driver; legibilidade visual dos telegráficos |
| #38 — Mini-boss | Ordem de inicialização, limiar de fase e fronteira segura; preservação de fase no disable/enable, morte e callbacks; padrão/cadência da segunda fase |
| #39 — Saja Boys | Quatro identidades e táticas distinguíveis; faixas de alcance e recuo, emboscada e bursts; configuração coerente sob callbacks; validar balanceamento e arte autorada posterior |
| #40 — Jinu | Evolução de combate na fronteira segura; cues narrativos exatamente uma vez por estágio; morte com observer desativado; story/cinematics autorados e fidelidade final ainda pendentes |
| EnemyLab | Importar cena/encontro e verificar bootstrap automático, piso, pontos, alvo lógico, horda e respawn. Aparência não inspecionada; player inicial sem SpriteRenderer/controles físicos |

Até três jogadores/alvos permanecem requisito de design. A seleção explícita atual evita singleton, mas não qualifica cooperação, rede ou balanceamento para três jogadores. Valores de protótipo e limites de horda/projéteis não comprovam desempenho de dispositivo. A regressão final inclui proteção Mira e combate/player anteriores após os novos seams de suporte.

### Regressões específicas de #31 identificadas na revisão

Estes cenários precisam de execução final após as correções correspondentes; o registro não afirma que a correção já foi concluída ou testada:

- Listener de `ActiveCharacterChanged` provoca dano fatal durante uma transição: a solicitação de respawn não pode ser perdida nem deixar o manager em estado inconsistente.
- Desativar e reativar o fluxo de tentativas/checkpoints: verificar a propriedade da tentativa e a coordenação com o manager.
- Falha na preparação do candidato durante respawn: nenhum personagem morto do cache pode permanecer ativo.
- Candidato com hitbox de combate inválida ou controllers obrigatórios desabilitados: rejeitar ativação sem comprometer o personagem ativo válido.

Gestão e troca solo não qualificam cooperativo ou rede. Até três personagens/jogadores simultâneos permanecem requisito do produto, com validação própria quando implementados. Contratos definitivos de #31/#32 prevalecem sobre esta lista inicial; atualizar a matriz se o escopo aprovado for mais específico.

## Etapas de validação final

1. Preparar o ambiente com a versão registrada em `ProjectSettings/ProjectVersion.txt` (atualmente Unity **6000.6.4f1**) e licença válida no próprio ambiente. Selecionar e registrar o commit a validar.
2. Importar o projeto, resolver erros de compilação e referências ausentes e registrar logs. Conferir profiles, prefabs e cenas alterados.
3. Executar EditMode e PlayMode, preservar logs/XML com SHA, versão do Editor, data, totais, falhas e casos ignorados. Exigir execução real, contagem positiva e zero falhas nos testes requeridos; um comando sem XML não comprova aprovação.
4. Executar regressão proporcional às dependências acumuladas, incluindo Rumi, Mira, Zoey, combate, proteção, gestão/troca e checkpoints. Corrigir falhas e repetir os testes afetados sobre o novo SHA.
5. Inspecionar o Combat Lab no Editor gráfico: movimento, ataques, mira aérea, feedback, troca e câmera. Execução batchmode sem gráficos não substitui inspeção visual.
6. Gerar e verificar builds das plataformas cobertas pela entrega. Medir desempenho e validar entradas/acessibilidade nos dispositivos definidos; alvos ainda abertos permanecem pendentes, sem alegação de suporte qualificado.
7. Registrar evidências no Trello e revisões finais. Completar a DoD somente quando os gates aplicáveis estiverem satisfeitos, e então qualificar a release.

## Atualização do registro

O agente de integração registra por card: arquivos e commit, veredito estático, pendências, dependências e próximo gate. QA mantém esta matriz sincronizada com os contratos implementados. Quando os testes forem executados, substituir a pendência pela evidência identificada, preservando o histórico da decisão e evitando atribuir o resultado a commits posteriores não verificados.
