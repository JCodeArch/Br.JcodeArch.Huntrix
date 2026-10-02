
# GDD Mestre — HUNTR/X: Honmoon

**Versão:** 0.1 — consolidação da visão aprovada  
**Data:** 2026-10-01  
**Estado:** documento-base para orientar as próximas decisões do Trello  
**Item de origem:** [01 — Consolidar GDD Mestre](https://trello.com/c/TOpMQvI6/1-01-consolidar-gdd-mestre)

## Como ler este documento

Este GDD registra a visão aprovada no projeto e separa o nível de certeza de cada conteúdo:

- **FECHADO** — decisão conceitual aprovada. Isso não significa que valores, regras de implementação ou conteúdo final já estejam definidos.
- **EM ABERTO** — decisão ainda não tomada; será resolvida no item correspondente do backlog.
- **RECOMENDAÇÃO** — proposta para orientar a discussão. Não deve ser tratada como decisão sem aprovação.

Quando faltar detalhe, este documento registra a lacuna em vez de preenchê-la com uma mecânica presumida. Os pilares e o escopo do MVP estão formalizados nas cartas 02 e 03; a Definition of Done e as métricas de diversão serão tratadas, respectivamente, nas cartas 04 e 05.

## 1. Visão do jogo

**FECHADO**

HUNTR/X — Honmoon é um jogo de ação e plataforma 2D para Android, iOS e Windows, desenvolvido com Unity 6 e C#. A experiência combina combate e plataforma com a fantasia de acompanhar Rumi, Mira e Zoey como integrantes da HUNTR/X e caçadoras de demônios.

A referência de gameplay aprovada é Mega Man X / Zero. Ela comunica a direção de ação e plataforma; não determina cópia de sistemas, movimentos, armas, fases ou outros elementos específicos dessas obras.

A concepção também prevê história próxima aos acontecimentos do filme. O escopo exato dessa adaptação e as permissões de uso de personagens, história, música e identidade visual continuam sujeitos a validação.

## 2. Experiência pretendida

**FECHADO**

- Jogar como Rumi, Mira e Zoey, cujos estilos de gameplay devem ser distintos.
- Jogar solo quando aplicável ou em cooperação com até três jogadores/personagens simultâneos na mesma sessão; cada jogador controla um integrante da equipe.
- Combater demônios em encontros terrestres e aéreos.
- Resgatar fãs e contar com um sistema global Honmoon; a relação mecânica entre ambos permanece em aberto.
- Usar o Special da HUNTR/X e um recurso simples de comida.
- Associar desempenho e espetáculo ao combate, sem transformar o jogo em um rhythm game.
- Usar música dinâmica como parte da experiência.
- Introduzir a produção completa somente depois de validar um Vertical Slice.

**EM ABERTO**

O escopo do MVP está detalhado na seção 2.2. Os critérios de conclusão estão em `docs/governance/DEFINITION_OF_DONE.md`, definidos no item 04; o protocolo e os alvos provisórios de diversão estão em `docs/qa/PLAYTEST_60S.md`, definidos no item 05. Os resultados do playtest ainda dependem de uma build e de participantes (item 68).

## 2.1 Pilares de design

**FECHADO — princípios orientadores**

1. **Ação 2D:** priorizar resposta imediata, game feel, legibilidade e simplicidade de controle. Movimento, combate, troca de personagem, dash, salto, ataque e Special são áreas abrangidas; comandos, regras e valores continuam em aberto nos itens de gameplay.
2. **Fandom:** personagens, identidade visual, música e relações são parte central da experiência. O conteúdo deve permanecer separável dos sistemas técnicos para permitir evolução independente.
3. **Proteção:** proteger personagens, aliados e objetivos é um eixo mecânico e narrativo. Risco/recompensa e cooperação orientam o design, sem fixar regras ou recompensas neste item. A adaptação etária deve ajustar intensidade, violência, linguagem e dificuldade sem mudar a identidade do jogo.
4. **Música dinâmica:** a música pode reagir a exploração, combate, tensão, Special e espetáculo. Regras de gameplay devem permanecer desacopladas do sistema de áudio; gatilhos e transições específicas ficam em aberto.
5. **Espetáculo:** combate, Special, transformações e eventos devem ter impacto visual claro, preservando responsividade, acessibilidade e orçamento mobile.

**FECHADO — cooperação como requisito de design:** prever até três jogadores/personagens simultâneos na mesma sessão, com cada jogador controlando um personagem da equipe; manter a experiência solo quando aplicável. Combate, câmera, inimigos, proteção, Special, música, progressão, dificuldade e entradas por teclado, gamepad e touch devem considerar os modos de 1, 2 e 3 jogadores, conforme as plataformas.

**EM ABERTO:** decidir antes da implementação de rede se a cooperação será local, online ou ambas. Câmera, sincronização, entrada, balanceamento, comportamento de inimigos, progressão e regras para a experiência solo permanecem decisões de cards próprios. Este requisito não define arquitetura de rede.

## 2.2 Escopo do MVP

**FECHADO — finalidade:** neste projeto, “MVP” significa a primeira versão integrada para validar a diversão e os pilares do jogo por meio do Vertical Slice. Não significa lançamento comercial mínimo nem substitui o escopo completo planejado para o produto. O teste deve ocorrer antes de escalar a produção.

### Obrigatório para a validação do MVP

- Uma build jogável de validação, com a fundação Unity e a Combat Lab dos itens 12–18 limitadas ao que o slice exige. A ordem de configuração inicial de Windows/Android permanece conforme o backlog; incluir em 13 somente pacotes justificados pelo experimento.
- Respeitar a sequência de pré-produção do backlog, sem pular gates: 04 Definition of Done → 05 métricas e limiar do teste → 06 direção visual → 07 resolução/câmera → 08 controles do protótipo → 09 decidir se o recorte exige save/perfil e qual parte demonstrar → 10 pipeline de assets → 11 riscos. Fechar cada decisão antes do ponto de implementação ou validação que dela depende.
- O recorte jogável descrito nos itens 61–68: avião, ramen, primeiro resgate de fãs, horda com Honmoon/Special, mini-boss, transição de paraquedas, show e playtest de 60 segundos. Cada beat incluído deve servir a um momento de gameplay e permitir observar os pilares; sequência e conteúdo detalhado continuam sujeitos às cartas próprias.
- O núcleo de ação necessário para jogar esse recorte (movimento, salto, dash e combate base), incluindo a identidade distinta das três integrantes e a troca quando necessária ao teste. Inimigos terrestres/aéreos, horda e mini-boss ficam limitados às variantes necessárias para os encontros do slice.
- Demonstração integrada de proteção/resgate, fãs, Honmoon, Special, Performance e música dinâmica, sem exigir ainda todo o catálogo, balanceamento final ou conteúdo completo do jogo.
- Restrições transversais de acessibilidade, adaptação etária e Child Safety devem orientar o protótipo desde o início; o slice não introduz chat, loot boxes, stamina, anúncios intrusivos ou coleta desnecessária. A validação completa de perfis e opções pertence aos itens 85–103 antes de qualquer lançamento público.
- O requisito de cooperação para até três jogadores permanece obrigatório para o produto. O modo local/online/ambos, quantos jogadores o experimento do slice precisa suportar e como esse requisito será demonstrado continuam **EM ABERTO**; registrar e resolver esses gates antes de implementar dependências de rede ou declarar o MVP validado quanto à cooperação. Não tratar cooperação como pós-lançamento por omissão.
- O item 04 define a Definition of Done e o item 05 define o protocolo e alvos provisórios de Game Feel; o item 68 coleta resultados antes de expandir produção. Este escopo não antecipa números de balanceamento, metas de FPS, dispositivos mínimos, câmera ou arquitetura.

### Desejável após a validação do slice e antes da versão completa do produto

- Expandir a campanha às áreas e momentos narrativos planejados (itens 55–60 e 80–84), o elenco completo de inimigos/bosses (33–41), kits e conteúdo além do necessário para distinguir as três personagens (28–32), estados e variações adicionais de fãs/Honmoon/recursos (42–50), progressão, replay e rankings (51–54).
- Expandir arte, animação, áudio e narrativa à produção completa (69–84); fechar e validar todos os perfis etários e recursos de acessibilidade, controles parentais e QA de cada perfil (85–103); estabelecer tiers/metas de performance e pipelines completos de mobile, QA e builds de release (92–111).
- Esses itens são expansão pós-validação ou preparação da versão completa, não remoção silenciosa de requisitos já registrados para o produto. Windows, Android e iOS permanecem plataformas-alvo; o pipeline iOS pode ocorrer após validar o slice e antes de lançamento, conforme item 98.

### Pós-lançamento

- Permanecem nesta categoria os itens explicitamente registrados no card 116: Boss Rush, Time Attack, novos modos e melhorias futuras. Não se acrescentam recursos ao pós-lançamento por inferência neste card.

**EM ABERTO — gates de produto:** a modalidade e prova de cooperação, os beats mínimos e os critérios de aprovação do slice/build, a necessidade de save/perfil no experimento, plataformas/dispositivos e metas técnicas, e a cobertura completa de idade/acessibilidade são refinados nos itens correspondentes (04–11, 68, 85–104). Os alvos provisórios do protocolo de 60 segundos são hipóteses para investigar e iterar; não aprovam o slice/build nem substituem os critérios que serão executados no item 68. Direitos/licenciamento continuam condição para distribuição, não hipótese resolvida pelo MVP.

## 3. Loop de jogo

**RECOMENDAÇÃO — hipótese para validação**

A conversa de concepção propôs o seguinte loop como modelo inicial:

1. Explorar uma fase.
2. Encontrar fãs e ameaças demoníacas.
3. Combater e criar oportunidades de resgate.
4. Resgatar fãs e avançar a experiência de HUNTR/X.
5. Usar o Special em um momento apropriado.
6. Recuperar ou proteger o Honmoon e avançar para o próximo encontro narrativo.

A ordem, a causalidade entre resgates, energia, Performance e Honmoon, e as condições de falha ou sucesso precisam ser definidas e testadas. Este loop não fixa regras, recursos ou números.

## 4. Personagens e combate

### HUNTR/X

**FECHADO:** Rumi, Mira e Zoey são as protagonistas jogáveis e devem ter estilos distintos.

**EM ABERTO:** habilidades, atributos, ataques, funções complementares, disponibilidade e condições para trocar de personagem. O backlog separa a implementação individual e a troca nos itens 28–32; movimento e combate base estão nos itens 19–27.

### Combate

**FECHADO:** o jogo inclui combate terrestre e aéreo, com inimigos que podem voar.

**EM ABERTO:** combos, parry e checkpoints aparecem como tópicos nos cartões de gameplay do backlog; sua aprovação, comportamento e critérios ainda precisam ser definidos nos respectivos itens.

**EM ABERTO:** comandos, janelas, alcance, dano, hitboxes, hurtboxes, knockback, tempos, custos, limites de inimigos simultâneos e parâmetros de balanceamento. Esses valores devem vir de protótipos e testes, não de suposições neste GDD.

## 5. Fãs, Honmoon e recursos

- **FECHADO:** haverá um sistema de fãs/resgate e o Honmoon será um sistema global do jogo.
- **FECHADO:** comida será um recurso simples.
- **FECHADO:** haverá um Special da HUNTR/X.
- **EM ABERTO:** estados dos fãs, interação de resgate, risco ou benefício, efeito de “soul drain”, efeitos e recuperação do Honmoon, condições de Honmoon Crisis e relação entre esses sistemas.
- **EM ABERTO:** função exata da comida, aquisição e consumo do Special, duração, limites, efeitos e interface.

O backlog detalha esses temas nos itens 42–50. Este documento não presume que resgates curem, concedam pontos ou ativem automaticamente o Special.

## 6. Performance e música

**FECHADO**

O Performance System deve refletir a execução do combate e contribuir para o espetáculo de HUNTR/X. A música será dinâmica. O projeto não será um jogo de ritmo.

**EM ABERTO**

Ações avaliadas, faixas ou estados de performance, benefícios, penalidades, feedback visual/sonoro e regras de transição musical. A relação entre música e performance deve apoiar o combate sem exigir sincronização rítmica do jogador. O backlog correspondente é 51 e 75–79.

**FECHADO — protocolo formativo:** o teste de 60 segundos e os indicadores/limiares provisórios de Game Feel estão em [`docs/qa/PLAYTEST_60S.md`](qa/PLAYTEST_60S.md). Eles medem apenas a build/trecho/configuração testados. Nenhum resultado foi coletado; o protocolo não representa aprovação de diversão ou validação da audiência.

## 7. Inimigos e confrontos

**FECHADO**

O conceito prevê demônios terrestres, voadores, de suporte, elites e hordas. Saja Boys serão rivais e/ou bosses; Jinu terá um arco narrativo; Gwi-Ma será o confronto final planejado.

**EM ABERTO**

Comportamentos, padrões de ataque, progressão de dificuldade, composição de encontros, frequência de bosses, relação entre os Saja Boys e Jinu, e forma do confronto final. O backlog 33–41 trata dessas entidades e categorias.

## 8. História e estrutura de fases

**FECHADO**

A história deve acompanhar de perto os acontecimentos do filme e conectar as fases a momentos narrativos, incluindo o arco de Jinu e o confronto com Gwi-Ma.

**EM ABERTO**

Quais acontecimentos são obrigatórios, adaptáveis ou excluídos; sequência, quantidade e duração das fases; diálogos e transições; e como preservar o ritmo jogável entre cenas. Não se acrescentam eventos ou personagens além dos já citados na concepção. Os itens 55–60 e 80–84 do backlog detalham level design, cutscenes e arcos narrativos.

## 9. Progressão, perfis e salvamento

**FECHADO**

O projeto prevê progressão por perfil e equipamentos/peças de evolução. A progressão de roupas ou equipamentos começa depois da sequência inicial. O contrato lógico de perfis, autosave, progresso por capítulo e replay foi definido no item 09 em [`docs/architecture/SAVE_AND_PROFILES.md`](architecture/SAVE_AND_PROFILES.md): progresso independente por perfil, salvamento em transições duráveis e replay que preserva a campanha.

**EM ABERTO**

Continuam em aberto o limite e a gestão de perfis, preferências por perfil/dispositivo, vínculo com perfil etário, checkpoints concretos, formato/storage/migração/backup, sincronização entre dispositivos, regras de coop e detalhes de replay. Obtenção e efeitos de equipamentos e rankings pertencem aos itens 52–54. A sequência visual “pijama → equipamento → peças de show → forma máxima” foi discutida como possibilidade, mas permanece **RECOMENDAÇÃO**, não regra fechada.

## 10. Público, idade, acessibilidade e segurança

**FECHADO**

- A intensidade e o conteúdo devem ser adaptáveis por idade.
- Está previsto um modo infantil para 4–8 anos.
- Haverá dificuldade dinâmica, acessibilidade e requisitos de Child Safety.

**EM ABERTO**

Faixas etárias além de 4–8 anos, classificação indicativa, diferenças de violência visual, diálogos, cutscenes e dificuldade por perfil, opções de acessibilidade, dados coletados, modalidade de cooperação (local/online) e controles parentais. Recursos online e requisitos de segurança associados não estão definidos. Os itens 85–91 detalham esses requisitos.

## 11. Direção visual e áudio

**FECHADO — direção visual:** ilustração 2D HD estilizada, com acabamento gráfico de cel painting e composição cinematográfica. Silhuetas, contraste, hierarquia de traço, planos de luz/sombra e leitura de ação seguem as regras de [`docs/art/VISUAL_STYLE_GUIDE.md`](art/VISUAL_STYLE_GUIDE.md), definidas no item 06.

**EM ABERTO:** identidade e paletas específicas das personagens (69–70); técnica de animação (71); composição de ambientes (74). Resolução, escala de autoria, proporções suportadas e comportamento-base da câmera foram definidos no item 07 e em [`docs/architecture/RESOLUTION_AND_CAMERA.md`](architecture/RESOLUTION_AND_CAMERA.md). Direitos/licenciamento dos elementos reconhecíveis de terceiros continuam sem validação, conforme o risco registrado neste GDD.

**FECHADO:** haverá música dinâmica, áudio de combate e resposta de torcida como áreas de trabalho.

**EM ABERTO:** repertório, composição, mixagem, implementação e licenciamento musical. Direitos e permissões para uso comercial são um risco que precisa ser resolvido antes de qualquer publicação; este documento não faz uma conclusão jurídica. Itens 75–79 cobrem o pipeline de áudio e licenças.

## 12. Plataformas e direção técnica

**FECHADO**

- Plataformas principais: Android, iOS e Windows.
- Tecnologia-base: Unity 6 e C#.
- A arquitetura pretendida é modular e data-driven.
- O desenvolvimento deve validar o núcleo em um Vertical Slice antes de escalar a produção.
- O pipeline inicial deve priorizar ferramentas sem custos recorrentes.

**EM ABERTO**

Versões mínimas de sistema operacional e dispositivos, pacotes Unity, estrutura concreta de pastas, formato de dados, save, integração contínua, metas de FPS/memória e distribuição. Resolução lógica, escala e câmera 2D seguem o contrato do item 07 em `docs/architecture/RESOLUTION_AND_CAMERA.md`. O mapa inicial de teclado, gamepad e touch, sem alterar a semântica do gameplay, está no item 08 em [`docs/architecture/INPUT_CONTROLS.md`](architecture/INPUT_CONTROLS.md); dispositivo mínimo, package/configuração Unity, cooperação local/online e validação em aparelhos continuam em aberto para os cards próprios.

## 13. Vertical Slice

**FECHADO:** o Vertical Slice deve preceder a produção completa e demonstrar o DNA do jogo com conteúdo limitado.

**RECOMENDAÇÃO:** a sequência discutida para o slice é avião → combate → ramen → horda → resgate de fãs → Special → mini-boss → paraquedas → show/estádio. Também foi proposto que o slice demonstre movimento, combate terrestre e aéreo, troca de personagens, fãs, Honmoon, música dinâmica e espetáculo.

**EM ABERTO:** sequência e duração totais do slice, conteúdo exato de cada build e resultado observado de diversão. Os itens 61–67 especificam a construção dos beats; o item 05 registra protocolo/alvos provisórios e o item 68 executará o playtest e decidirá a próxima iteração com base em dados reais.

## 14. Registro de decisões e itens relacionados

| Tema | Estado neste GDD | Próximo item do Trello |
|---|---|---|
| Pilares de design | Cinco princípios e cooperação para até 3 jogadores registrados; modalidade local/online em aberto | 02 |
| Escopo do MVP | MVP de validação definido como Vertical Slice; cooperação permanece gate explícito | 03 |
| Definition of Done | Gates comuns e por tipo registrados em `docs/governance/DEFINITION_OF_DONE.md` | 04 |
| Métricas de diversão | Protocolo/alvos provisórios definidos; resultados aguardam build e playtest | 05 e 68 |
| Estilo visual | Direção geral 2D HD estilizada e regras de consistência definidas; detalhes por asset nos cards de arte | 06; 69–74 |
| Resolução e câmera | Referência 16:9/1920×1080, 100 PPU, câmera ortográfica e regra de adaptação definidos; parâmetros de tuning aguardam build | 07 |
| Controles multiplataforma | Ações lógicas e mapa inicial de teclado/gamepad/touch registrados; validação, remapeamento e arquitetura de pacote em aberto | 08; 13; 85–103 |
| Salvamento e perfis | Contrato lógico de perfis independentes, autosave, progresso por capítulo e replay definido; formato, gestão, sync e regras de coop abertos | 09; 27; 52–54; 85–103 |
| Pipeline de assets e riscos | Em aberto | 10–11 |
| Fundação Unity, gameplay e personagens | Em aberto | 12–54 |
| Level design e Vertical Slice | Em aberto | 55–68 |
| Arte, áudio, história, idade e acessibilidade | Em aberto | 69–91 |
| Mobile, QA, release e documentação | Em aberto | 92–116 |

## 15. Riscos conhecidos

- **Direitos e licenciamento:** a proximidade com o filme e o uso de personagens, música e identidade visual exigem confirmação de permissões para distribuição.
- **Escopo:** definir limites do MVP antes de produzir conteúdo em escala.
- **Diversão:** testar o Combat Lab e o slice antes de produzir dezenas de fases, sprites e cutscenes.
- **Performance:** considerar dispositivos móveis durante arquitetura e produção; metas mensuráveis ainda estão em aberto.
- **Arte e áudio:** conteúdo final depende de pipeline, consistência e licenciamento.
- **Publicação e segurança infantil:** requisitos devem ser especificados e validados antes do lançamento.

## 16. Critérios de manutenção do GDD

Uma futura alteração deve preservar o rótulo de estado, registrar a decisão que mudou e evitar mudanças silenciosas em contratos de gameplay. Recomendações só passam a FECHADO quando aprovadas. Parâmetros numéricos devem ser derivados de protótipos, balanceamento ou requisitos técnicos específicos.

## Fontes de escopo

- Conversa do projeto **“Aprimorar ideia de jogo”** — concepção aprovada, sistemas previstos e hipóteses do Vertical Slice.
- Conversa do projeto **“Criar agentes especializados”** — princípios de arquitetura e pipeline de agentes.
- Trello: [HUNTR/X — Honmoon](https://trello.com/b/O5yAS8lM/huntr-x-honmoon) e [card 01 — Consolidar GDD Mestre](https://trello.com/c/TOpMQvI6/1-01-consolidar-gdd-mestre).
- Trello: [card 05 — Definir métricas de diversão](https://trello.com/c/DEkBR4PB/5-05-definir-m%C3%A9tricas-de-divers%C3%A3o) e [card 68 — Playtest de 60 segundos](https://trello.com/c/XxeFEI8g/58-68-playtest-de-60-segundos).
- Trello: [card 06 — Definir estilo visual 2D](https://trello.com/c/NQl7W8MA/6-06-definir-estilo-visual-2d) e guia de estilo em `docs/art/VISUAL_STYLE_GUIDE.md`.
- Trello: [card 07 — Definir resolução e câmera](https://trello.com/c/Xw7OlxdN/7-07-definir-resolu%C3%A7%C3%A3o-e-c%C3%A2mera) e contrato em `docs/architecture/RESOLUTION_AND_CAMERA.md`.
- Trello: [card 08 — Definir controles multiplataforma](https://trello.com/c/ltBsR8f7/8-08-definir-controles-multiplataforma) e proposta de mapeamento em `docs/architecture/INPUT_CONTROLS.md`.
- Trello: [card 09 — Definir estrutura de save](https://trello.com/c/54w6st2I/9-09-definir-estrutura-de-save) e contrato em `docs/architecture/SAVE_AND_PROFILES.md`.
- Repositório: `docs/architecture/AGENT_PIPELINE.md` e `docs/governance/DEFINITION_OF_DONE.md`.


