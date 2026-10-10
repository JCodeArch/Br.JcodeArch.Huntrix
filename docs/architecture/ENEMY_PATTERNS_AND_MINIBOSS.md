# Cards #37–#38 — padrões e primeiro miniboss

## Contrato do protótipo

#37 compõe `EnemyAgent2D` com um único `EnemyPatternController2D`, sem controller terrestre/voador concorrente. O alvo é fornecido explicitamente por `TrySetTarget`; o prefab não procura personagens globalmente. O controller avança o agente, aproxima-se, alterna dois comportamentos, inicia telegráfico para cada ataque de um burst e aguarda recuperação entre ciclos.

`EnemyPatternDefinition` referencia dois `EnemyDefinition` válidos. A troca entre ciclos preserva saúde e alvo e muda a geometria/alcance do ataque e o tempo do telegráfico, além do número de ataques e descanso. Elite usa jab estreito/rápido e sweep largo/lento. `StateChanged(Telegraph)` do agente e `PatternStarted` permitem feedback; o visual geométrico herdado do agente fica amarelo durante o aviso; a interface visual/sonora final não está implementada. Os bursts não causam dano durante o aviso: o agente resolve dano somente em `Attacking`, reutilizando facção, parry, proteção e linha de visão.

#38 adiciona `MinibossPhaseController2D`, duas definições de padrão e um limiar provisório de 50% da saúde. Fase inicial alterna sweep e slam amplo; fase dois alterna jab em burst e slam com aviso mais curto. Transição encerra a ação anterior e recomeça o novo padrão. Fase dois é monotônica para a mesma instância e não retorna à abertura ao desativar/reativar. Nova instância reinicia o encontro. Inicialização espera o Update com agente vivo/configurado, evitando dependência da ordem de Awake entre componentes. O driver de fase executa Update antes do padrão por DefaultExecutionOrder(-50), impedindo um aviso inicial que seria cancelado pela inicialização da fase.

Callbacks são isolados por assinante; uma mudança de definição em `PatternStarted` invalida o ciclo anterior antes de atacar. Não há coroutine, busca de cena ou driver duplicado nos prefabs. Desativar encerra o agente e reseta o ciclo; morte/ausência de alvo válido impede ataque. As políticas do agente de saturação conservadora e deduplicação de receivers permanecem aplicáveis.

## Limites e valores

Todos os números e nomes de padrões são protótipos, sem balanceamento definitivo, narrativa, animação ou áudio finais. Os prefabs precisam receber alvo por composição de cena/encounter. Nenhuma integração de fase do jogo ou build é presumida. Não há alegação de comportamento cooperativo qualificado nem medição móvel.

Revisão estática e validação de GUIDs não comprovam importação, compilação ou execução. Execução Unity fica adiada conforme `docs/governance/DEFERRED_VALIDATION.md`. Validar depois: alternância e telegráfico antes de cada hit, dois shapes realmente distintos, esquiva/parry/proteção, alvo inválido/morto/desativado, interrupção de fase, limiar exato, reativação, callback reentrante, obstáculos, múltiplos colliders e saturação.
