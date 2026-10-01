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

Quando faltar detalhe, este documento registra a lacuna em vez de preenchê-la com uma mecânica presumida. As cartas 02–05 do Trello continuam responsáveis por formalizar pilares, escopo do MVP, Definition of Done e métricas de diversão.

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

As prioridades do MVP e as métricas de diversão serão fechadas, respectivamente, nos itens 03 e 05 do backlog. A lista acima consolida intenções já registradas, mas não substitui esses itens.

## 2.1 Pilares de design

**FECHADO — princípios orientadores**

1. **Ação 2D:** priorizar resposta imediata, game feel, legibilidade e simplicidade de controle. Movimento, combate, troca de personagem, dash, salto, ataque e Special são áreas abrangidas; comandos, regras e valores continuam em aberto nos itens de gameplay.
2. **Fandom:** personagens, identidade visual, música e relações são parte central da experiência. O conteúdo deve permanecer separável dos sistemas técnicos para permitir evolução independente.
3. **Proteção:** proteger personagens, aliados e objetivos é um eixo mecânico e narrativo. Risco/recompensa e cooperação orientam o design, sem fixar regras ou recompensas neste item. A adaptação etária deve ajustar intensidade, violência, linguagem e dificuldade sem mudar a identidade do jogo.
4. **Música dinâmica:** a música pode reagir a exploração, combate, tensão, Special e espetáculo. Regras de gameplay devem permanecer desacopladas do sistema de áudio; gatilhos e transições específicas ficam em aberto.
5. **Espetáculo:** combate, Special, transformações e eventos devem ter impacto visual claro, preservando responsividade, acessibilidade e orçamento mobile.

**FECHADO — cooperação como requisito de design:** prever até três jogadores/personagens simultâneos na mesma sessão, com cada jogador controlando um personagem da equipe; manter a experiência solo quando aplicável. Combate, câmera, inimigos, proteção, Special, música, progressão, dificuldade e entradas por teclado, gamepad e touch devem considerar os modos de 1, 2 e 3 jogadores, conforme as plataformas.

**EM ABERTO:** decidir antes da implementação de rede se a cooperação será local, online ou ambas. Câmera, sincronização, entrada, balanceamento, comportamento de inimigos, progressão e regras para a experiência solo permanecem decisões de cards próprios. Este requisito não define arquitetura de rede.

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

O projeto prevê progressão por perfil e equipamentos/peças de evolução. A progressão de roupas ou equipamentos começa depois da sequência inicial.

**EM ABERTO**

Estrutura e quantidade de perfis, salvamento automático, capítulos, replay, obtenção e efeitos de equipamentos, rankings e persistência entre dispositivos. A sequência visual “pijama → equipamento → peças de show → forma máxima” foi discutida como possibilidade, mas permanece **RECOMENDAÇÃO**, não regra fechada. Ver itens 52–54 e 09 do backlog.

## 10. Público, idade, acessibilidade e segurança

**FECHADO**

- A intensidade e o conteúdo devem ser adaptáveis por idade.
- Está previsto um modo infantil para 4–8 anos.
- Haverá dificuldade dinâmica, acessibilidade e requisitos de Child Safety.

**EM ABERTO**

Faixas etárias além de 4–8 anos, classificação indicativa, diferenças de violência visual, diálogos, cutscenes e dificuldade por perfil, opções de acessibilidade, dados coletados, modalidade de cooperação (local/online) e controles parentais. Recursos online e requisitos de segurança associados não estão definidos. Os itens 85–91 detalham esses requisitos.

## 11. Direção visual e áudio

**RECOMENDAÇÃO:** “2D HD estilizado” foi sugerido como direção de arte.

**EM ABERTO:** o estilo visual final e as regras de consistência devem ser fechados no item 06.

**EM ABERTO:** resolução lógica, escala, proporção de tela, câmera, dimensões de sprites, técnica final de animação/rig e regras de consistência. A decisão formal está no item 06; os itens 07 e 69–74 detalham câmera, personagens, animação, VFX e ambientes.

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

Versões mínimas de sistema operacional e dispositivos, resolução/câmera, controles por plataforma, pacotes Unity, estrutura concreta de pastas, formato de dados, save, integração contínua, metas de FPS/memória e distribuição. Esses detalhes pertencem aos itens de pré-produção e Fundação Unity; não são requisitos técnicos fixados aqui.

## 13. Vertical Slice

**FECHADO:** o Vertical Slice deve preceder a produção completa e demonstrar o DNA do jogo com conteúdo limitado.

**RECOMENDAÇÃO:** a sequência discutida para o slice é avião → combate → ramen → horda → resgate de fãs → Special → mini-boss → paraquedas → show/estádio. Também foi proposto que o slice demonstre movimento, combate terrestre e aéreo, troca de personagens, fãs, Honmoon, música dinâmica e espetáculo.

**EM ABERTO:** sequência final, duração, conteúdos obrigatórios, critérios de aceitação e como provar que o combate é divertido. O backlog 61–68 contém os cartões do Vertical Slice e do playtest de 60 segundos. A métrica e o critério de avanço ficam para esses itens e para o item 05.

## 14. Registro de decisões e itens relacionados

| Tema | Estado neste GDD | Próximo item do Trello |
|---|---|---|
| Pilares de design | Cinco princípios e cooperação para até 3 jogadores registrados; modalidade local/online em aberto | 02 |
| Escopo do MVP | Não fechado neste item | 03 |
| Definition of Done | Não redefinida neste item | 04 |
| Métricas de diversão | A validar | 05 |
| Estilo visual | Direção conceitual; detalhes abertos | 06 |
| Resolução e câmera | Em aberto | 07 |
| Controles multiplataforma | Em aberto | 08 |
| Salvamento e perfis | Conceito aprovado; regras em aberto | 09 |
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
- Repositório: `docs/architecture/AGENT_PIPELINE.md` e `docs/governance/DEFINITION_OF_DONE.md`.


