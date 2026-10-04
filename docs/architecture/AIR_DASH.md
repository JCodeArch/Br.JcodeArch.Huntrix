# Air Dash — contrato de gameplay

**Card Trello:** [22 — Implementar dash aéreo](https://trello.com/c/AKhLc7pA/22-22-implementar-dash-a%C3%A9reo)

## Entrada e disponibilidade

Gameplay chama DashController2D.TryStartAirDash com a direção lógica Move (Vector2); bindings de teclado, controle e touch não pertencem ao controller.

A carga começa disponível. O dash só inicia se SetGrounded(false) for o último sinal de chão recebido, o controller estiver ativo, não houver dash já em execução, a direção for finita/não zero, velocidade e duração estiverem configuradas com valores positivos finitos e o HorizontalMovement2D aceitar a posse temporária de X. Rejeição não consome carga nem emite eventos.

Uma direção diagonal é normalizada antes de multiplicar pela velocidade configurada. O controller escala componentes antes de normalizar para também aceitar vetores finitos de magnitude muito grande.

## Velocidade, física e duração

No início, o controller define Rigidbody2D.linearVelocity para a direção normalizada multiplicada por airDashSpeed. Durante airDashDuration, HorizontalMovement2D mantém somente a componente X do dash via o override temporário existente. A componente Y não é reescrita: Rigidbody2D continua integrando gravidade. Ao encerrar ou desativar, o controller limpa apenas o override de sua própria posse e não restaura velocidade Y antiga.

Os campos airDashSpeed e airDashDuration são configurados no Inspector. O card não introduz cooldown, bindings físicos, quantidade adicional de cargas ou recarga ao tocar paredes.

## Relação com pulo

O dash aéreo tem prioridade sobre Jump durante sua execução. JumpController2D.SetJumpSuppressed(true) descarta buffer e janela de coyote pendentes, cancela o estado de jump cut e ignora novos PressJump enquanto suprimido. No término/desativação, o dash restaura Jump. Dash terrestre não ativa essa supressão.

Uma aterrissagem enquanto o dash está ativo restaura a carga, mas o dash atual continua até sua duração expirar. O estado grounded ainda impede iniciar um air dash. Uma nova carga fica disponível após o próximo SetGrounded(false).

## Estado compartilhado e responsabilidade

DashController2D continua sendo a autoridade de IsDashing e IsInvulnerable. DashStateChanged emite true uma vez no início válido e false uma vez no término. A camada de grounding deve alimentar SetGrounded no personagem; esta implementação não cria sensor de chão nem integra input de dispositivo.

## Validação e limites

A suíte PlayMode valida direção, gravidade, posse e conflito de X, duração, carga/recarregamento, aterrissagem, pulo suprimido, desativação, eventos e regressão de dash terrestre. A suíte Unity do projeto continua sendo necessária antes da integração. Não foi feita medição de performance em dispositivo móvel; o caminho de física não cria alocações por frame e usa apenas componentes já presentes.

## Pipeline and verification — 2026-10-04

- Orchestrator: selected one air charge per airborne interval and no invented cooldown or tuning values.
- System Architect: approved initial normalized 2D velocity plus the existing owner-scoped horizontal override; gravity integrates Y. Recommended temporary Jump suppression to make dash priority independent of FixedUpdate ordering.
- Core Gameplay Developer: implemented the approved contract and PlayMode coverage.
- SOLID Auditor: APPROVED, no findings. N/A: multi-owner jump suppression is outside scope; this card has one caller.
- Performance Engineer: APPROVED, no findings. FixedUpdate adds no allocations or Physics2D queries; component references are cached, and normalization occurs only at dash start. Device profiling is N/A; no target-device performance measurement was performed.
- QA Automation Engineer: PASS. The reviewer’s coverage suggestions were resolved with natural-expiry event/state assertions, a grounded gate after expiry, and Infinity direction/tuning cases.
- Unity 6000.6.4f1 final full runs: EditMode 11/11 passed, 0 failed, 0 skipped; PlayMode 36/36 passed, 0 failed, 0 skipped.
- Git whitespace check: clean. Unity compiler errors/warnings: none in captured logs.
- Antigravity: agy CLI is installed and accessible. The requested code-review prompt was rejected by automatic approval review because it would disclose private repository files to an external model. No Antigravity code review is claimed; a written user approval for that specific disclosure is needed before trying again.
- Integration: feature commit `0618753bb004c2c55cca251bec0fc0b2476476b6` was pushed to `origin/feature/card-22-air-dash`; `develop` was fast-forwarded and pushed; `HEAD`, `origin/develop`, and `origin/feature/card-22-air-dash` all resolve to the same SHA. Working tree is clean after integration.