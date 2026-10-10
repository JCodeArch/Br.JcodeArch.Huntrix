# Card #52 — aquisição narrativa de peças

`EquipmentProgress2D` vincula uma instância a um ID local opaco GUID N, sem dados pessoais. Não permite rebinding para outro perfil, evitando perda/vazamento de progresso. Outro perfil usa outra instância; `EquipmentMechanics2D.TryBindProgression` pode selecionar a instância correspondente e remove a peça anteriormente equipada.

Um catálogo limitado tecnicamente a 32 definições associa cada peça a um ponto narrativo authored. `TryCompleteNarrativePoint` libera somente peças válidas daquele ponto, registra ID antes de publicar e rejeita liberação repetida. `EquipmentStoryPoint2D` fornece trigger/API de conclusão; não concede recompensas por kills, moedas, grind ou repetição de replay. Os pontos `prototype-air-step-story` e `prototype-resonance-story` são marcadores demonstrativos, não marcos narrativos finais aprovados.

`SnapshotUnlockedIds` fornece referências para o consumidor futuro de save, conforme `SAVE_AND_PROFILES.md`. Nenhum storage, importação de save, progressão entre instalações ou política cloud é implementado. IDs liberados ficam em memória por instância de perfil. Desativação preserva os IDs; destruir a instância não os persiste.

Eventos isolados e trava de aquisição impedem desbloqueios ou troca de perfil reentrantes. Execução Unity está adiada; verificar depois triggers, catálogo inválido, pontos repetidos, perfis independentes, callback e retomada quando storage existir.
