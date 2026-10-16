# Changelog

Este histórico distingue implementação de validação executada. Um merge de desenvolvimento não é uma release qualificada.

## Unreleased — 0.1.0-dev.1

### Gameplay e encontros

- Personagens Rumi, Mira e Zoey, troca local, movimento, ataques, defesa e alcance.
- Inimigos terrestres e voadores, suporte, hordas, elites, miniboss, rivais Saja, Jinu e fases de Gwi-Ma.
- Fãs com estados, drenagem, proteção e resgate; comida com escolha self/fan e consumo reservado.
- Honmoon, crise e união; Special conjunto em etapas com exclusão dos fãs.
- Equipamentos por pontos narrativos e mecânicas próprias; performance e sumários de ranking local por perfil opaco.
- Gramática de plataformas, rota de nove áreas e blockouts com controles e checkpoints.
- Abertura VerticalSlice em sete etapas, do avião ao show, com comida, resgate, horda/Special, miniboss e paraquedas.

### Apresentação e políticas

- Identidades visuais originais de protótipo, model sheets SVG, rigs, animações, efeitos e ambientes.
- Gerenciador de áudio com treze WAVs originais, música dinâmica e feedback de combate e torcida.
- Roteiros originais PT/EN, diálogos e transições, marcos de Rumi/Jinu e resultado final de Gwi-Ma.
- Cinco perfis de idade, apresentação e assistência por instância, política narrativa e preferências de acessibilidade.
- Controles touch e HUD com safe area, idioma, opções, bloqueio de entrada por modais e formulário de feedback local.

### Engenharia e distribuição

- Versão semântica em VERSION.txt, branch por feature/card e PR para develop.
- Documentação de arquitetura, criação de conteúdo e preparação de publicação.
- Ferramentas de testes, builds, proveniência e gates de RC preparadas para ambiente licenciado. Resultados efetivos devem vir dos relatórios associados ao commit e ao build avaliado.

### Validação e limites

As implementações desta rodada receberam revisão estática e verificações de referências autoradas. A aceitação da fonte atual por compilação, importação e suites Unity permanece pendente; playtest humano, escuta final, medições em dispositivos e builds distribuíveis exigem evidências próprias. Arte e áudio são protótipos originais; a apresentação final e os direitos necessários à publicação exigem avaliação própria.

Há evidência histórica de APK Android de desenvolvimento e 81 EditMode + 293 PlayMode (374 aprovados) na fonte `2fc41a1230e00e453dca04024a72746a1f8adfdf` do PR #100; o contexto também registra suites em `0c4a8e4053140b76ded470cbd05f899f89a26bce`. São resultados dessas fontes, com ressalvas de checkout histórico descritas no contexto, e não validação dos merges posteriores. Os resultados documentados para o card #29 também pertencem àquela versão. Consulte `docs/PROJECT_CONTEXT.md`, `docs/reviews/` e `docs/governance/DEFERRED_VALIDATION.md` para evidências e pendências específicas.

Não há tag ou release pública criada por este changelog. O backlog de evolução pós-lançamento está em `docs/POST_LAUNCH_BACKLOG.md` e não indica que já houve lançamento.
