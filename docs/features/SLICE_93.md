# Card93 — implementação com validação Unity pendente

HUD de vida, Honmoon, Special, comida e objetivo, dimensionado pela safe area. Opções visíveis ajustam faixa etária, flashes, legendas, mira, toggle e teclas. Roster age-aware real e scope binding explícito para manager/horde dinâmicos. Audio original75 e narrativa82 com política88 obrigatória integrados. Avaliação humana só salva por ação explícita; registro60s identifica janela incompleta. Arte final e runtime/mobile UX exigem revisão e execução Unity.

Snapshot de cena em manifests/snapshots/card-93-VerticalSlice.unity. Não executados testes Unity nem playtest humano.

Opções são modais: bloqueiam input/touch e GUI narrativa, pausam com snapshot de estado anterior e restauram no fechamento/disable. Feedback após conclusão bloqueia somente gameplay; o registro60s pode completar. Narrativa não é cancelada pelo menu. Legendas false ocultam só linha com voz reproduzida; texto primário semvoz ou adaptado pela política etária permanece legível. PT/EN são escolhidos por controle visível.
