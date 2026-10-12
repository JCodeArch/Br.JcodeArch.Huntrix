# Card61 — abertura jogável

Implementado — validação Unity pendente. Abrir Assets/Scenes/VerticalSlice.unity e Play. A composição atual contém somente o primeiro beat do avião; cards62–68 expandirão a sequência em mudanças próprias. A sessão cria slot, câmera ortográfica, atores com roots independentes, geometria de Avião, dois demônios terrestres e três voadores. Complete derrotando inimigos e chegando ao fim. Checkpoints e respawn usam o fluxo existente; troca preserva saúde individual.

Teclado: A/D/W/S movimento e direção; Espaço pulo; Shift dash; J ataque; K parry; Q troca; E interação segurada; R comida própria; F comida para fã; L Special. Gamepad: stick/dpad; south pulo; RB dash; west ataque; east parry; LB troca; north interação; select/start comida; RT Special. Ações físicas passam pelo mesmo frame lógico usado pela futura interface touch. Preferências são referência explícita; remapeamento, toggle de interação com release e assistência de mira têm consumidores reais.

Arte geométrica original é provisória, sem afirmar arte final. Física/sensores e controles precisam playtest real; zero testes Unity executados nesta rodada. TryEnterStage valida todos os pré-requisitos antes de encerrar tentativa antiga, preserva HP/cache e inicia checkpoint da nova fase sem reviver.
