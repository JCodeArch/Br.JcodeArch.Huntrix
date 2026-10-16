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

## PR #101 pause routing review — 2026-10-15

HUD menu input uses its own enabled InputAction while options disable gameplay input.
The keyboard binding follows AccessibilityPreferences2D Pause remap/default restore;
Gamepad Start pauses and FoodFan remains moved to rightStickPress. Legacy remap
selectors include Pause. Validate remapped key/default restore, repeated close at
zero timeScale, prior disabled input, gamepad route exclusivity and HUD teardown.
`HudPauseRoutingTests` adds three regression scenarios; they were NOT RUN here:
`scripts/run-unity-tests.sh` exits 1 because UNITY_EDITOR is unset. No Unity
import/compile/runtime pass is claimed for this correction.


## PR #106 — alvos touch da avaliação do UiLayoutLab — 2026-10-15

Os dois sliders de avaliação foram ampliados de 43,2 para 64,8 unidades nominais
na referência 1280×720; os handles ficam contidos nos controles. Esse tamanho de
autoria não comprova 64 pixels físicos, dp ou dimensão mínima após redução da safe area.
A revisão estática independente foi aprovada. Executar o novo cenário
`SurveySlidersMeetReferenceTouchMinimumAndAcceptEdgeTaps`, incluindo dimensões,
vizinhança sem sobreposição e mudança de avaliação por toque nas bordas.
Validar visualmente em 16:9 e telas largas, com insets assimétricos à esquerda e à
direita e redução vertical da safe area; conferir legibilidade, ausência de sobreposição,
arraste e acessibilidade física nos dispositivos alvo. Importação, compilação,
PlayMode e validação visual/dispositivo permanecem pendentes. O runner local encerrou
com `UNITY_EDITOR` ausente; nenhum teste Unity ou build foi executado nesta correção.


## Etapa 3 — HUD UGUI somente leitura na VerticalSlice — 2026-10-15

`SliceReadOnlyHud2D` espelha a sessão real em um Canvas de telemetria sem
EventSystem, Selectables, GraphicRaycaster ou comandos. `SliceHud2D` permanece
ativo e proprietário de pausa, opções, input, políticas etárias e feedback; a
posse exclusiva da apresentação oculta apenas suas linhas de telemetria. Ao
perder fonte, owner, exclusividade ou habilitação, o Canvas some e a posse é
liberada. Opções e conclusão mantêm suas telas IMGUI existentes.

Vida/personagem acompanham o ator atual; Honmoon e Special exibem valores reais,
sem converter carga bruta em fração. Resgates usam o snapshot de performance da
execução ativa, sem inferir denominador ou usar Registry.Count. Fontes ausentes
ou desabilitadas aparecem indisponíveis. Menu, perfil, novos comandos touch,
replay e persistência de preferências não foram integrados nesta etapa.

Executar `SliceReadOnlyHudTests`: cena real, dados renderizados, mudança de vida,
troca de personagem/recursos, fontes ausentes/desabilitadas, pausa/opções,
exclusividade e fallback ao desativar o presenter. Complementar com respawn,
transições de beat, conclusão, foco, teardown/reload e regressão dos testes de
pausa existentes. Inspecionar legibilidade/oclusão com mundo e botão de opções
em 16:9, telas largas e safe areas assimétricas/verticais; medir custo real e
validar touch/teclado/gamepad em dispositivos. Compilação/importação e todos os
cenários Unity permanecem pendentes: `UNITY_EDITOR` ausente no runner local.

## VerticalSlice options touch cancellation — 2026-10-15

The legacy touch producer now remembers contacts seen while gameplay input is
blocked, or when the producer loses focus, is disabled or rebound. Those contacts
must end before they can send gameplay again. Newly pressed contacts remain
independent. Live IMGUI options already use separate Jump and Interact preferences;
this correction does not combine them or integrate the isolated OptionsUiView.

`TouchOptionsCancellationTests` authors four hold/toggle combinations through the
real HUD Escape InputAction route, simultaneous jump/rescue/pad contacts, an attack
finger first pressed in options, partial release and fresh-contact recovery. Three
mouse scenarios cover producer disable/enable, focus loss and rebind. The fixture
invokes production Update methods synchronously and inspects the submitted frame;
it isolates session composition and does not prove full-scene consumer behavior.
Run these regressions and the full EditMode/PlayMode suites in licensed Unity
6000.6.4f1, then verify options in the real VerticalSlice with touch hardware.

The test was authored before the implementation. The local runner exited 1 at
`UNITY_EDITOR` being unset, before starting Unity: RED/GREEN behavior, import,
compilation, runtime, device and visual validation remain unexecuted.

Focus cancellation follow-up: the touch producer now keeps an explicit focus gate.
Every unfocused Update suppresses newly observed contacts and clears held/toggle
state, preventing contacts begun without focus from replaying on regain even
when a downstream consumer drops submitted frames. Four additional touch cases
cover hold/toggle and release-before-regain versus held-through-regain, followed
by fresh-contact recovery. Unity execution remains unavailable (`UNITY_EDITOR`
unset), so these cases are authored and statically reviewed only.

## Card #30 — Zoey presentation subscriber isolation (2026-10-16)

Three additional PlayMode scenarios are authored in `ZoeyRangedAttack2DPlayModeTests`: a throwing first hit subscriber must preserve one committed damage result and deliver later impact/shot callbacks; a throwing first shot subscriber must preserve later callbacks, re-entry rejection and firing after cooldown; a throwing first parry subscriber must preserve consumed parry/no damage and later parry/shot callbacks. Source reviewed against the immediate precision-shot contract in [card #30](https://trello.com/c/DgAV9fe4/30-30-implementar-zoey). These scenarios have NOT been compiled or executed in Unity. Run them with existing Zoey, combat, Rumi and Mira regression on the exact integrated SHA before final card acceptance. No new balance, aim, asset or platform behavior is introduced.


## Card #44 — disabled soul-drain agent (2026-10-16)

`FanSoulDrainController2D` now requires its `EnemyAgent2D` to be active and enabled,
matching the agent's own operational gate. Disabling only the agent previously
left the sibling drain controller able to acquire fans and resolve a pending pulse.
`FanDrainAgentLifecycleTests` authors acquisition rejection, pending telegraph
cancellation and an enabled-agent positive control through the real fan/agent
prefabs and registry. Tests were authored before the guard; the runner exited 1
because `UNITY_EDITOR` is unset. RED/GREEN behavior, Unity import/compilation and
all PlayMode execution remain unverified. Run these scenarios and fan/Honmoon
regressions on the exact integrated SHA before acceptance of card #44.

## Card #93 — UI flow views presentation contracts (2026-10-16)

PlayMode contract tests authored in `MainMenuUiViewContractTests`, `TeamProfileUiViewContractTests`,
and `ResultsReplayUiViewContractTests` cover presentation requirements for Figma frames 2:2,
2:29, and 2:150:
- Main menu start and options emission, plus strict disabled state of Continue without save/resume.
- Team profile selection of characters (Rumi, Mira, Zoey), age profiles (4-5, 6-8, 9-12, 13-15, 16+), confirm and back events.
- Results and replay score presentation, 1-5 survey slider clamping, feedback status coloring and navigation events.

Unity execution remains unavailable in this environment (`UNITY_EDITOR` unset); tests are authored and statically reviewed only.

## Card #93 — integração do subpainel de remapeamento no UiLayoutLab (2026-10-16)

O botão **Remapear teclas** deixou de estar indisponível no laboratório e passa a
abrir `RemapPanelUiView` como sobreposição de Opções. O controlador preserva a tela
origem, transfere o foco ao subpainel e o devolve ao botão de remapeamento ao voltar.
O painel recebe `AccessibilityPreferences2D` próprio da sessão de demonstração;
não persiste preferências nem altera a cena de gameplay. A cena também configura o
controle de seleção de ação, campo de tecla, aplicar, restaurar e feedback.

`RemapPanelOpensFromOptionsAndBackRestoresOptionsFocus` foi escrito antes desta
integração e a verificação estática confirmou referências e rotas de eventos. Unity
não está disponível neste ambiente (`UNITY_EDITOR` unset), portanto importação,
compilação e execução PlayMode continuam **não executadas**. Executar esse cenário,
`RemapPanelContractTests` e a regressão de navegação do UiLayoutLab no SHA integrado;
validar visualmente dropdown, entrada de texto, aplicar/restaurar e foco em teclado,
gamepad, touch e telas com safe area antes de concluir o card.

## Cards #55 & #56 — Level design grammar and stage route contracts (2026-10-16)

EditMode e PlayMode tests are authored in `StageBlockoutDefinitionTests` and
`StageBlockoutBuilderPlayModeTests`, covering structural validation for authored
stage definitions/routes and runtime blockout assembly (platform colliders,
visuals, start/end points, checkpoint anchors, reentrancy and cleanup).

Unity execution remains unavailable in this environment (`UNITY_EDITOR` unset);
importação, compilação e execução PlayMode continuam pendentes. Executar os
cenários no Unity 6000.6.4f1 e validar a rota no VerticalSlice antes de concluir
os cards.

## Cards #57 & #59 — Aerial encounters and boss arenas contracts (2026-10-16)

PlayMode tests are authored in `AerialStageEncounterPlayModeTests` and `BossArenaControllerPlayModeTests`:
- `AerialStageEncounter2D` configuration and lifecycle (rejection of null parameters, scene instance prefabs, spawn guards without stage runtime root, and clean flyer disposal).
- `BossArenaController2D` configuration and arena lifecycle (rejection of null dependencies, scene instance prefabs, spawn guards without stage runtime root, completion reset, and clean disposal).

Unity execution remains unavailable in this environment (`UNITY_EDITOR` unset); tests are authored and statically reviewed only.

## Card #58 — Fan encounters lifecycle and configuration (2026-10-16)

PlayMode tests are authored in `FanEncounterPlayModeTests`:
- `FanEncounterDefinition` structural validation (enforcing explicit registry group of 1..64 fans, rejection of scene instances and negative soul loss).
- `FanEncounterController2D` lifecycle (rejection of null/invalid profiles, non-finite positions, verified instantiation of registry group with authored initial soul loss, reentrancy guards, and cleanup on `Stop()` and `OnDisable()`).

Unity execution remains unavailable in this environment (`UNITY_EDITOR` unset); tests are authored and statically reviewed only.

## Cards #52 & #53 — Progressão de equipamentos e efeitos mecânicos das peças (2026-10-16)

A suíte PlayMode `EquipmentProgressionPlayModeTests` foi autorada cobrindo a definição, progressão e mecânicas das peças de equipamento:
1. `EquipmentPieceDefinition`: validação de `narrativePointId`, `cooldown`, parâmetros exclusivos de `AirStep` (`airStepVelocity > 0`) e de `ResonanceBurst` (`burstRadius > 0` e `burstAttack` válido).
2. `EquipmentProgress2D`: vinculação de perfil com formato estrito de GUID hexadecimal de 32 dígitos ("N"), idempotência de binding e rejeição de retentativas conflitantes; conclusão de pontos narrativos com guards de componente ativo, perfil vinculado, validação de catálogo (máx 32 peças) e prevenção de duplicidade; isolamento de exceções em subscribers de `PieceUnlocked`.
3. `EquipmentStoryPoint2D`: delegação da conclusão ao `EquipmentProgress2D` alvo com salvaguardas de componente ativo.
4. `EquipmentMechanics2D`: binding de progressão, salvaguarda de equipagem com checagem de desbloqueio em `progression.IsUnlocked`, guards de prontidão (`Ready`), dinâmica de `AirStep` (necessidade de ter tocado o solo anteriormente, voo no ar, bloqueio de uso duplo no mesmo salto, aplicação de velocidade vertical e cooldown), e dinâmica de `ResonanceBurst` (detecção de inimigos no raio, dano e knockback via `ResolveHit`, respeito a linha de visão e emissão de eventos).
5. `EquipmentPrototypeBinding2D`: vinculação automática de perfil e progressão e auto-equipamento ao desbloquear peças.

Conforme a diretiva de adiamento, esses cenários foram autorados e inspecionados estaticamente, sem execução no Unity (`UNITY_EDITOR` ausente no ambiente). Executar os 20 casos no commit integrado durante a validação final da Definition of Done.

## Card #60 — Blockouts jogáveis e controlador de level blockout (2026-10-16)

A suíte PlayMode `BlockoutLevelControllerPlayModeTests` foi autorada cobrindo o ciclo de vida do carregador de fases em blockout e regiões de checkpoint:
1. `BlockoutLevelController2D`: validação de dependências e rota (`builder`, `route`, `player`, `flow`, `playerPrefab` sem cena), validação de limites de estágio (`CurrentStageIndex`), construção da geometria via builder, instanciação e configuração do jogador no ponto de início, disparo seguro de `StageLoaded`, transição limpa entre estágios com descarte da fase anterior, e redefinição de estado ao desabilitar (`OnDisable`).
2. `BlockoutCheckpointRegion2D`: configuração na montagem do estágio com `BoxCollider2D` tipo gatilho (tamanho 1.5x2.0), salvaguarda contra receptores externos/inimigos e ativação estrita de checkpoint apenas quando o ator do jogador correspondente entra na área.

Execução Unity indisponível neste ambiente (`UNITY_EDITOR` ausente); testes autorados e inspecionados estaticamente. Executar no Unity 6000.6.4f1 antes de considerar os blockouts concluídos.

## Card #54 — Replay local e rankings por capítulo (2026-10-16)

A suíte PlayMode `LocalReplayPlayModeTests` foi autorada cobrindo registros locais, persistência tolerante a falhas, critérios de ranking e ciclo do controlador:
1. `LocalReplayRecord`: validação estrutural do formato do perfil (`32` dígitos hexadecimais minúsculos), validação de limites estritos (`rescuedFans` 0..1024, `performancePoints` 0..1000000, `performanceNormalized` 0..1, `elapsedSeconds` 0..86400, `maximumCombo` 0..10000) e integridade temporal.
2. `LocalReplayRecordStore`: serialização em envelope versionado (máx. 64 registros, máx. 256KB), escrita atômica com arquivo `.pending` e retenção de `.backup`, recuperação resiliente via backup se o arquivo principal for corrompido (`LastLoadRecovered`), idempotência/desduplicação por `recordId`, limpeza segura com `TryReset` sem afetar arquivos de campanha/equipamento, e ordenação composta de rankings:
   - Fãs resgatados (descendente)
   - Performance normalizada (descendente)
   - Combo máximo (descendente)
   - No-damage (descendente)
   - Tempo decorrido (ascendente)
   - Conclusão UTC ticks (descendente)
3. `LocalReplayController2D`: ciclo de vida completo (`Configure`, `TryBeginRun`, `TryRequestReplay`, `TryCompleteRun`, `TryReadRankings`), portão de autorização de replay via `IReplayChapterAccess`, binding de execução em `CombatPerformanceController2D` e emissão dos eventos `ReplayRequested` e `RunSaved`.

Execução Unity indisponível neste ambiente (`UNITY_EDITOR` ausente); testes autorados e inspecionados estaticamente. Executar no Unity 6000.6.4f1 antes de considerar o sistema de replay e rankings concluído.

## Card #48 — Honmoon Crisis e União de Equipe (2026-10-16)

A suíte PlayMode `HonmoonCrisisPlayModeTests` foi autorada cobrindo a intensificação da horda durante a crise e a mecânica de recuperação/proteção por união de equipe:
1. `HonmoonCrisisDefinition`: limites bounded de multiplicador de intervalo de spawn (0..1], inimigos concorrentes extras [0..64], valor de recuperação de Honmoon (0..100] e duração da proteção de união (0..30].
2. `HonmoonCrisisController2D`: escuta de histerese em `HonmoonController2D.CrisisChanged`, acionamento e cancelamento de intensidade em `HordeController2D` (`IsIntensifying` e evento `IntensityChanged`), salvaguarda de invocação única de união por ciclo de crise (`usedThisCrisis`), disparo de `TryExecuteTeamUnion()` e recuperação de recurso em `definition.UnionRecoveryValue`, além de emissão do evento `TeamUnited`.
3. `CrisisTeamUnion2D`: validação de participantes (1..3 character managers vivos da facção HUNTR/X), concessão temporária de imunidade com registro externo de `IDamageProtectionSource2D` no ator e verificação estrita de cobertura (`Covers`) sem afetar entidades não autorizadas.

Execução Unity indisponível neste ambiente (`UNITY_EDITOR` ausente); testes autorados e inspecionados estaticamente. Executar no Unity 6000.6.4f1 antes de considerar o sistema de crise de Honmoon concluído.

## Cards #45 & #46 — Recursos de Comida e Escolha Self/Fan (2026-10-16)

A suíte PlayMode `FoodResourceAndChoicePlayModeTests` foi autorada cobrindo a definição, inventário, coleta e mecânica de escolha self/fã:
1. `FoodDefinition`: validação dos quatro tipos (`Ramen`, `Onigiri`, `Gyoza`, `Dango`) com propriedades positivas finitas de recuperação de si (`SelfRecovery`) e de fã (`FanRecovery`).
2. `FoodPickup2D` e `FoodInventory2D`: carregamento dos prefabs de pickup (`Ramen_Pickup`, `Onigiri_Pickup`, `Gyoza_Pickup`, `Dango_Pickup`), validação de capacidade (`capacity=32`), coleta atômica (`TryCollect`) desativando o pickup no mundo e prevenindo re-coleta duplicada, incremento de `TotalCount` e contagem segregada por item, emissão segura de `QuantityChanged`, e rejeição de acréscimo além da capacidade máxima.
3. `FoodChoiceController2D`: binding de auto-receptor (`TryBindSelf`), rejeição estrita de facções não autorizadas (rejeita facção `Demon`, aceita `HuntrX`), e mecânica bifurcada de consumo:
   - Uso em si (`TryUseOnSelf`): rejeitado quando o jogador está com vida máxima ou morto/desabilitado (com rollback imediato no inventário de comida sem perda do item), bem-sucedido quando o jogador está ferido, restaurando vida em `SelfRecovery` e decrementando o inventário.
   - Uso no fã (`TryUseOnFan`): checagem de distância bounded por `fanUseRange` (3.0m), rejeição de fãs além do alcance ou com alma cheia (rollback imediato do item), e sucesso ao alimentar fãs drenados/danificados, recuperando a alma em `FanRecovery` e consumindo o item.

Execução Unity indisponível neste ambiente (`UNITY_EDITOR` ausente); testes autorados e inspecionados estaticamente. Executar no Unity 6000.6.4f1 antes de considerar os cards #45 e #46 concluídos.

## Card #41 — Boss Final Gwi-Ma e Encerramento Narrativo (2026-10-16)

A suíte PlayMode `GwiMaFinalBossPlayModeTests` foi autorada cobrindo a definição, progressão de fases e transição para o encerramento narrativo do boss final:
1. `GwiMaBossDefinition`: validação dos três padrões distintos (`opening`, `pressure`, `finale`), limiares de vida decrescentes (`0 < finaleHealthFraction < pressureHealthFraction < 1f`) e cue de encerramento (`endingCue`).
2. `GwiMaBossController2D`:
   - Inicialização em fase `Opening`.
   - Transição monotônica de fases por limiares de vida (`Opening` -> `Pressure` -> `Finale`), rejeitando regressão em caso de cura.
   - Gerenciamento de ciclo de vida e suspensão de padrões no `OnDisable`, restaurando no `OnEnable` se o boss estiver vivo.
   - Transição de derrota (`Defeated`) acionada na morte (`self.Died`), emitindo `EndingRequested` com payload imutável `FinalBossOutcome2D` contendo identificador, cue e posição, e garantindo idempotência e não-duplicação de eventos.
3. `FinalBossEndingController2D`: binding e recepção de `FinalBossOutcome2D` na derrota do boss, validação de conformidade da cue e instância da apresentação de encerramento (`EndingPresentation2D`).

Execução Unity indisponível neste ambiente (`UNITY_EDITOR` ausente); testes autorados e inspecionados estaticamente. Executar no Unity 6000.6.4f1 antes de considerar o card #41 concluído.

## Cards #39 & #40 — Saja Boys e Encontro com Jinu (2026-10-16)

A suíte PlayMode `SajaBoysAndJinuRivalsPlayModeTests` foi autorada cobrindo a definição, táticas e evolução narrativa dos rivais Saja Boys e Jinu:
1. `SajaRivalDefinition`: validação de identidade (`SajaIdentity`: Abby, Baby, Mystery, Romance, Jinu), táticas (`SajaTactic`: Pressure, Burst, Ambush, KeepDistance), contagem de burst (`burstCount >= 1`), pausas e distâncias preferidas bounded.
2. `SajaRivalController2D`: carregamento dos quatro protótipos de rivais (`Abby_Prototype`, `Baby_Prototype`, `Mystery_Prototype`, `Romance_Prototype`), execução de tática KeepDistance aplicando velocidade contrária à posição do alvo dentro da distância preferida, e espera estática na tática Ambush fora do alcance de ataque.
3. `JinuEncounterDefinition` e `JinuEncounterController2D`:
   - Configuração de perfil duplo (`Opening` e `Pressure`) de Jinu e cues narrativos (`EncounterCue`, `PressureCue`, `DefeatedCue`).
   - Disparo do estágio inicial `Encounter` e emissão da cue de encontro no `Start()`.
   - Transição segura em fronteira (`Idle`/`Chasing`) para o estágio `Pressure` ao cruzar o limiar `PressureHealthFraction`, emitindo a cue de pressão e reconfigurando a tática do rival.
   - Transição terminal para `Defeated` ao ocorrer a morte (`self.Died`), emitindo a cue de derrota com comportamento idempotente.

Execução Unity indisponível neste ambiente (`UNITY_EDITOR` ausente); testes autorados e inspecionados estaticamente. Executar no Unity 6000.6.4f1 antes de considerar os cards #39 e #40 concluídos.

## Card #35 — Inimigos de Suporte: Healer, Protector e Soul Drainer (2026-10-16)

A suíte PlayMode `SupportEnemyPlayModeTests` foi autorada cobrindo os três papéis de inimigos de suporte (`HealerDemon_Prototype`, `ProtectorDemon_Prototype`, `SoulDrainerDemon_Prototype`):
1. `SupportEnemyDefinition`: validação estrutural dos papéis (`Healer`, `Protector`, `SoulDrainer`), raio de ação, intervalo de pulso, cura e duração de proteção bounded, além de ataque de drenagem válido para o papel `SoulDrainer`.
2. `SupportEnemyController2D`:
   - Papel `Healer`: pulso de restauração de vida em aliados feridos da mesma facção dentro do raio (`TryRestoreHealth`).
   - Papel `Protector`: concessão externa de proteção (`IDamageProtectionSource2D`) a aliados com `DamageProtection2D` dentro do raio, sustentada por `protectionDuration` e verificada via `Covers()`.
   - Papel `SoulDrainer`: dreno de almas de alvos adversários dentro do raio via `TryReceiveHit` com `DrainAttack`, emitindo o evento `DrainAccepted(target, lost)`.
   - Limpeza e ciclo de vida: descarte e desregistro de proteção externa no `OnDisable` e `OnDestroy`.

Execução Unity indisponível neste ambiente (`UNITY_EDITOR` ausente); testes autorados e inspecionados estaticamente. Executar no Unity 6000.6.4f1 antes de considerar o card #35 concluído.
