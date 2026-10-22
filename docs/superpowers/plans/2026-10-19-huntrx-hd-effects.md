# HUNTR/X HD e efeitos — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrar a arte HD aprovada, todos os personagens existentes e efeitos de profundidade/combate numa experiência lateral verificável, corrigindo cada falha encontrada antes de aceitar a etapa.

**Architecture:** Separar catálogo de sprites, composição do cenário, câmera e efeitos por eventos. Reaproveitar pools/adaptadores existentes e manter a física independente da apresentação. Um único proprietário da câmera e uma única composição visual por fase evitam movimento e efeitos duplicados.

**Tech Stack:** Unity 6000.6.4f1, C#; URP 17.6.0 e Renderer2D; pacotes 2D Animation, Sprite Shape e Cinemachine já declarados; Particle System e pools locais. Sem serviços pagos ou novas dependências comerciais.

**Spec:** `outputs/auditoria-2026-10-19/DIRECAO_HUNTRX_HD.md`, prévias em `outputs/assets-hd-preview/` e mensagem do proprietário de 19/10 aprovando a linha visual e solicitando os efeitos. Este plano incorpora esses requisitos adicionais; detalhes técnicos e valores abaixo são propostas para revisão, não implementações concluídas.

## Global Constraints

- Arte: **2D desenhado em alta resolução**. Gameplay lateral ortográfico; profundidade visual não muda os eixos de movimento para isometria.
- Preservar Rumi, Mira, Zoey, campanha e requisito de cooperação; não reduzir o escopo do GDD para declarar conclusão.
- Ferramentas gratuitas; nenhum serviço pago ou compra.
- Fonte observada: `D:/Workspace/Br.JcodeArch.Huntrix`, branch `feat/art-animations-and-vertical-slice`, HEAD `c42fbd031466115209f23f71f49ddcecc978b556`. Há alterações locais de terceiros; nenhuma deve ser apagada.
- Git nesta execução retornou `Permission denied` ao acessar explicitamente o diretório, embora leituras PowerShell e `rev-parse` funcionassem. Reconciliar acesso e snapshot antes de qualquer edição; nunca interpretar a contagem vazia de um comando Git que falhou como árvore limpa.
- Editor local confirmado em `C:/Program Files/Unity/Hub/Editor/6000.6.4f1/Editor/Unity.exe`; AndroidPlayer contém SDK, NDK e OpenJDK. Existência não comprova licença, compilação ou aparelho conectado.
- Todas as assemblies devem compilar com zero erros e zero warnings; resultados antigos não validam a fonte nova.
- Nenhum efeito de câmera ou filtro pode alterar colisões, input ou tempo de ataque. Oferecer intensidade de tremor zero e efeitos reduzidos.
- Orçamentos iniciais propostos: até 128 partículas de ambiente e 256 de combate simultâneas no perfil PC; perfil reduzido com 32/64; até 3 imagens de rastro por ator; 4 luzes locais visíveis, 1 com sombra no perfil completo. Ajustar após medições, registrando a mudança.
- Meta PC: pelo menos 95% dos frames abaixo de 16,7 ms no trecho medido de 60 segundos, após aquecimento, com máquina e build identificados. Android tem aceitação por aparelho; não alegar aprovação sem aparelho medido.

## Review Focus

1. Troca de heroína, morte e reinício não deixam rastros, emissores, luzes ou assinaturas de eventos do ator anterior.
2. Tremor e look-ahead não revelam bordas vazias, não deslocam o HUD nem fazem o cenário derivar ao longo do tempo.
3. Efeitos reduzidos mantêm antecipações e informações de perigo, com tremor zero e sem flashes repetidos.
4. Ataques simultâneos respeitam limites dos pools; saturação visual nunca suprime dano, resgate ou avanço de fase.
5. Mudanças de resolução/proporção e shaders no build real preservam chão, pés, HUD e sprites sem material rosa.

## Escopo de personagens e conteúdo

Catálogo identificado em `Assets/Data/Art`: Rumi, Mira, Zoey, Jinu, Abby, Mystery, Romance, Baby e GwiMa. Inimigos referenciados pelo apresentador: CaptainDokkaebi, MagentaStewardess, BlueCyclopsCrew, SmokeImp, CabinBrute, GumihoFox, CorruptedHaetae, BulgasariBrute, TalchumPhantom, WingedJeoseung e DokkaebiShield. Cruzar também NPCs/fãs e todas as cenas antes de congelar o inventário: esta lista não declara que o elenco está completo.

Produzir fichas e estados compatíveis para cada identidade realmente usada; aparições narrativas recebem poses/expressões necessárias, combatentes recebem ciclos e antecipações. Não substituir todo inimigo pelo capitão nem selecionar identidade por hash. A prancha aprovada é referência de acabamento: ainda contém efeitos incorporados, enquadramento apertado e não é um atlas de runtime.

## Task 1: Baseline, inventário e build canônico

**Files:** preservar fonte inteira; criar `docs/art/HD_ASSET_INVENTORY.md` e evidências em uma pasta nova `outputs/hd-validation/<build-id>/`; usar `Assets/Editor/Build/PrototypeBuildPipeline.cs` e scripts de evidência existentes.

**Interfaces:** produz inventário `identity/state/source/runtime/status`, snapshot Git e build com hash/commit/dirty patch identificados; sem alteração de API de gameplay.

- [ ] Reconciliar branch, HEAD, arquivos modificados e não rastreados; arquivar snapshot sem sobrescrever o trabalho compartilhado. Preparar isolamento a partir de develop e portar apenas dependências identificadas da arte local.
- [ ] Cruzar `Assets/Data/Art`, Resources, prefabs, cenas, GDD e backlog. Registrar cada personagem e o caminho realmente carregado; manter ausências como pendências.
- [ ] Executar import/compile e suites atuais numa cópia autorizada. Guardar baseline, XML e logs; classificar falhas existentes sem atribuí-las às mudanças futuras.
- [ ] Gerar WindowsDevelopment com `HuntrX.Editor.Build.PrototypeBuildPipeline.WindowsDevelopment`, `-huntrxOutput` novo e `-force-d3d11` no primeiro ensaio local por causa do histórico de DX12. Conferir API efetiva no log; não prometer correção do driver.
- [ ] Abrir build, registrar início, troca de personagem, morte/reinício e trecho atual. Commitar apenas inventário/configurações pertinentes após revisão.

## Task 2: Catálogo HD, importação e animação de todo o elenco

**Files:** criar `Assets/Scripts/Data/Definitions/CharacterSpriteSet2D.cs`, `Assets/Scripts/Presentation/Art/CharacterSpritePlayback2D.cs`, `Assets/Editor/Art/HdSpriteImportValidator.cs`; modificar `SlicePlayerPresentation2D.cs`, `CharacterArtRenderer2D.cs`, `EnemyPrototypeVisual2D.cs` e assets/prefabs explicitamente inventariados. Testes novos: `Assets/Tests/EditMode/CharacterSpriteSetTests.cs`, `Assets/Tests/PlayMode/CharacterSpritePlaybackTests.cs`.

**Interfaces:** `CharacterSpriteSet2D.TryGetClip(string stateId, out SpriteAnimationClip2D clip)`; clip contém frames, durações por frame, loop/hold e âncora comum. `CharacterSpritePlayback2D.SetState(string stateId, bool restart)` e `Tick(float deltaTime)` selecionam frame; não aplicam dano.

- [ ] Escrever testes que rejeitam frame nulo, duração não positiva e estado duplicado; validar reset ao trocar estado, hold do último frame de derrota e escala constante entre frames. Executar e observar falha antes da implementação.
- [ ] Criar os contratos, importador e player. Uma escala/âncora por personagem; restringir deformação ao filho visual. Expor progresso do ataque no controlador somente se a integração não puder usar os estados já existentes, com teste de preservação dos tempos de contato.
- [ ] Gerar/exportar por identidade, usando ficha aprovada como referência, corpos sem magia incorporada e ciclos reais. Inspecionar cada frame claro/escuro, mãos, arma, pés e continuidade. Não tratar sheet gerado como aprovado automaticamente.
- [ ] Integrar em lotes: trio; tripulação e capitão; rivais/chefe/NPCs. Identidade explícita por asset do encontro substitui seleção por hash. Consolidar os caminhos de apresentação para impedir dois renderers de personagem simultâneos.
- [ ] Testar troca durante dash/ataque, pausa, morte e reinício: nenhum ghost ou assinatura antiga sobrevive. Rodar regressões de ataque/personagens; captura em movimento e revisão de sprites antes de cada commit de lote.

## Task 3: Cenários em camadas e câmera com limites

**Files:** criar `Assets/Scripts/Presentation/Art/StageLayerDefinition2D.cs`, `StageLayerComposer2D.cs` e `Assets/Scripts/Gameplay/VerticalSlice/SliceCameraDirector2D.cs`; modificar `StageBlockoutBuilder2D.cs`, `StageEnvironmentRenderer2D.cs`, `LivingStageBackdropAnimator2D.cs`, `SlicePlayerPresentation2D.cs`, `VerticalSliceSession2D.cs`; testes `StageLayerComposerTests.cs` e `SliceCameraDirectorTests.cs`.

**Interfaces:** `StageLayerComposer2D.TryBuild(StageLayerDefinition2D definition, Camera camera)` e `Clear()`; `SliceCameraDirector2D.Bind(Transform target, Bounds worldBounds)` e `RequestShake(float amplitude, float duration)`. CameraDirector é o único escritor da pose final; Cinemachine é adaptador somente se a API instalada for compatível, sem dois proprietários concorrentes.

- [ ] Testar parallax sem deriva: mesma posição de câmera produz mesma posição de camada; deslocamento de +10 e retorno a zero retorna à origem. Fatores iniciais de compensação: distante 0,85, meio 0,45, plano jogável 0, primeiro plano -0,08; definir fórmula e convenção no contrato.
- [ ] Testar clamp com proporções 16:9 e 20:9, reinício sem target antigo e shake que retorna exatamente ao enquadramento base; HUD permanece independente. Implementar após observar os testes falhando.
- [ ] Produzir céu/cidade/nuvens, cabine com janelas transparentes, lounge, portas, piso e primeiro plano separados. Repetir apenas módulos de corredor; lounge e porta de chefe aparecem uma vez. Remover composição visual duplicada somente após comparar as cenas.
- [ ] Separar deslocamento da câmera, vento externo e turbulência decorativa. Cabine/piso visual não podem sair do alinhamento dos colliders. Clipping/máscaras limitam chuva e nuvens ao exterior.
- [ ] Integrar e verificar extremos da fase, saltos, troca de heroína e resolução. Repetir para cada cenário inventariado, com presets próprios; commitar por conjunto revisável.

## Task 4: Luz 2D, materiais e pós-processamento

**Files:** criar materiais HD e `Assets/Scripts/Presentation/Effects/StageLightingRig2D.cs`, `VisualQualityProfile2D.cs`; modificar `Assets/Settings/Renderer2D.asset`, `UniversalRP.asset`, perfil de Volume dedicado à apresentação e configuração da câmera em `VerticalSliceSession2D.cs`; testes `VisualQualityProfileTests.cs` e `PresentationPipelineTests.cs`.

**Interfaces:** `StageLightingRig2D.Apply(VisualQualityProfile2D profile)` e `Clear()`; perfil contém orçamento de luz/partículas, bloom, sombras e distorção. Separar perfil PC e reduzido sem alterar dano ou encontros.

- [ ] Testar que o perfil reduzido desativa distorção/sombras caras e não remove luz ambiente nem antecipações; desmontagem libera apenas recursos próprios.
- [ ] Usar materiais Sprite-Lit compatíveis com URP e luz global de base; locais em janelas, porta e magia. Shadow Caster 2D somente em formas revisadas. Sombras de contato mantêm os pés no piso.
- [ ] Preparar mapas de normais alinhados à textura secundária e ao recorte de cada sprite que os usa. Comparar luz móvel da esquerda/direita para verificar orientação e volume; não rotular mapa automático de cores como normal correto. Priorizar superfícies rígidas antes de replicar nas animações.
- [ ] Configurar Volume/câmera e Bloom em material emissivo HDR: intensidade inicial 0,25, threshold 1,1; vignette 0,12. Aberração cromática permanece zero no gameplay contínuo, pulso opcional de até 0,08/0,12 s em evento especial, desligado com efeitos reduzidos. Valores são pontos de partida de revisão visual.
- [ ] Construir e abrir player: sem shader ausente/material rosa, personagens claros e efeitos visíveis. Reproduzir e corrigir qualquer warning de shader antes de declarar luz/bloom entregues. Perfil default atual possui Bloom ativo com intensidade zero; não confundir configuração existente com efeito funcionando.

## Task 5: Impacto, partículas, contorno e distorção

**Files:** modificar `Assets/Scripts/Pooling/PooledImpactSprite2D.cs`, `Assets/Scripts/Pooling/CombatImpactVfxPool2D.cs`, `Assets/Scripts/Presentation/Art/GameplayVfxPool2D.cs`, `Assets/Scripts/Presentation/Art/GameplayVfxAdapter2D.cs`; criar materiais/shaders de outline e ripple, `Assets/Scripts/Presentation/Effects/CombatPresentationEffects2D.cs`; modificar `Assets/Scripts/Gameplay/AgeAccessibility/AccessibilityPreferences2D.cs` e `AccessibilityPanel2D.cs`. Testes `CombatPresentationLifecycleTests.cs`, `VisualEffectsBudgetTests.cs`.

**Interfaces:** bridge recebe eventos existentes de impacto/parry/especial e chama o diretor de câmera/pools; `SetReducedEffects(bool reduced)` e `SetShakeIntensity(float intensity)` limitam apresentação. Não cria contatos nem dano.

- [ ] Testar saturação do pool e retorno/reuso; efeitos não disponíveis não impedem resolução de combate. Testar troca de ator, disable/enable, morte, reinício e cancelamento sem assinatura duplicada.
- [ ] Integrar poeira de corrida/pouso, faíscas, fragmentos e dissipação demoníaca; sem sangue por padrão. Trails capturam frame e transformação reais; arcos separados seguem arma e fase de contato.
- [ ] Aplicar tremor por evento, inicial leve 0,035 unidade/0,08 s, forte 0,10/0,14 s e chefe 0,16/0,20 s, com limite acumulado 0,18. Reavaliar em enquadramento real; zero deve eliminar todo tremor.
- [ ] Outline fino destaca ator/seleção sem halo duplicado. Ripple localizado só em especial/portal, até 0,25 s, com orçamento e fallback sem distorção. Nenhum filtro de tela cobre o HUD.
- [ ] Testar preferências durante efeito ativo: tremor para, flash se reduz, telegraph permanece. Validar tintas e partículas distintas para as três Hunters e inimigos, sem mascarar ataques.
- [ ] Medir pior encontro com todos os efeitos pertinentes. Ajustar orçamento por evidência, sem instalar VFX Graph só por volume de partículas: usar pools/Shuriken enquanto atendem à apresentação medida. Revisão independente e commit.

## Task 6: Fase completa, builds e ciclo de qualidade

**Files:** `SliceAirplaneBeat2D.cs`, definição Aviao, encontros/chefes e progressão nos caminhos inventariados; suites existentes e cenários novos de regressão; evidências em pasta por build.

**Interfaces:** preservar contratos de sessão, combate, resgate e transição. Chefe do avião precisa ser configurado como encontro próprio, não apenas renomeado de inimigo comum.

- [ ] Testar spawns livres de sólidos, waves e condição de saída diagnosticável, chefe com dois padrões e reinício consistente; corrigir origem da falha antes de adicionar decoração que a esconda.
- [ ] Rodar suites EditMode/PlayMode completas e ferramentas de evidência; compilar as quatro assemblies sem erros/warnings. Gerar Windows e AndroidDevelopment novos, nunca sobrescrever artefato antigo para alegar build fresco.
- [ ] Jogar a fase completa no Windows: introdução, movimento, três personagens, ataques/dash/parry/especial/resgate aplicáveis, chefe, vitória, derrota e replay. Registrar passos, capturas e logs do build identificado. Auto-teste da máquina não substitui opinião humana de diversão.
- [ ] Medir 60 s do encontro mais pesado após aquecimento; registrar máquina, resolução, perfil, frames CPU/GPU, memória e pausas. Em Android, instalar e repetir se houver aparelho autorizado conectado; ausência de aparelho é bloqueio concreto para aceitação mobile, não para o restante.
- [ ] Repetir a mesma rota nas demais fases e para todo personagem inventariado. Verificar fluxo da campanha, áudio e transições, sem aceitar apenas o primeiro corredor.
- [ ] Executar loop: observar → registrar falha reproduzível → corrigir → teste dirigido → regressão afetada → novo build se necessário → repetir a rota. Encerrar etapa somente com critérios atendidos; se houver bloqueio externo registrar o último estado e continuar tarefas independentes.
- [ ] Revisão independente final, commit/push/PR para develop e atualização Supabase/Trello com evidências reais. Não marcar o jogo inteiro perfeito ou concluído por passar compilação ou testes automáticos.

## Critério de encerramento verificável

Elenco inventariado com assets coerentes integrados, fases verificadas sem bloqueios de progressão, efeitos legíveis e preferências funcionais, suites e builds identificados, métricas de desempenho e revisão sem bloqueadores. A avaliação humana decide se a emoção/qualidade corresponde à intenção. Não existe promessa de perfeição absoluta nem processo permanente executando depois do encerramento da sessão.

## Referências técnicas consultadas

- Unity, perspectiva lateral/isométrica: https://docs.unity3d.com/6000.0/Documentation/Manual/2d-game-perspective-reference.html
- Unity URP, materiais compatíveis e luzes 2D: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/2d-light-properties-explained.html
- Cinemachine Impulse, conceito: https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineImpulse.html — confirmar API no pacote efetivamente instalado; manifest declara 6.6.0, portanto esta página 3.1 não é prova de compatibilidade binária.

Revisão de planejamento por especialista confirmou parallax, trails e pools existentes, ausência de integração de Light2D encontrada nos scripts e Bloom com intensidade zero. Nenhuma mudança de runtime, teste Unity ou build foi executado nesta preparação.
