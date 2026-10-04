# Checkpoints e tentativas — plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Adicionar seleção de checkpoint e coordenação determinística de morte/tentativa com pedido de respawn consumível pelo CharacterManager futuro.

**Architecture:** `CheckpointAnchor2D` define um destino de cena. `CheckpointAttemptFlow2D` controla destino ativo, ator vinculado e número da tentativa; em morte ele publica um `RespawnRequest` imutável sem mutar o ator. `DamageReceiver2D` emite `Died` apenas na transição viva→morta. Não integrar com save ou GameBootstrap neste card.

**Tech Stack:** Unity 6.6.4f1, C#, UnityEngine, NUnit EditMode/PlayMode.

**Spec:** `docs/superpowers/specs/2026-10-04-checkpoints-design.md`

## Global Constraints

- Seguir `docs/governance/DEFINITION_OF_DONE.md` e os limites de `docs/architecture/SAVE_AND_PROFILES.md`.
- Não implementar `CharacterManager` (#31), storage, cura/penalidade, input/trigger, ou reset de inimigos.
- Manter o fluxo scene-local, explícito, sem singleton, service locator ou busca a cada frame.
- Evitar alteração de comportamento de combate fora do evento de morte documentado.

## Review Focus

1. Ator trocado/destruído durante tentativa não deixa handler inscrito nem publica pedido obsoleto.
2. Reentrada/rebinding/OnDisable não duplica eventos nem perde estado válido.
3. Checkpoint ativado que some antes da morte usa fallback inicial válido.
4. Dano letal publica evento após o impacto aceito e somente uma vez.
5. Transform inválido, ID vazio, input não finito ou destino destruído não sobrescrevem checkpoint válido nem causam exception.

---

### Task 1: Contrato e evento de morte

**Files:**
- Modify: `Assets/Scripts/Gameplay/Combat/DamageReceiver2D.cs`
- Modify: `Assets/Tests/PlayMode/AttackController2DPlayModeTests.cs`
- Create: `Assets/Tests/EditMode/CheckpointAttemptFlowEditModeTests.cs` (testes de valores/âncora podem começar depois da Task 2)

**Interfaces:**
- Produz `public event Action<DamageReceiver2D> Died`.
- Evento dispara depois do dano e knockback que cruzam de vida positiva para zero; hits subsequentes não o repetem.

- [x] Testes PlayMode cobrem hit não letal, evento na morte após knockback e ausência de evento em hits após morte.
- [x] Testes adicionados em revisão do estado existente; casos de findings novos foram executados red/green antes das correções.
- [x] Implementar detecção `previousHealth > 0 && CurrentHealth <= 0` e invocar o evento depois do knockback aceito.
- [x] Executar as suítes completas EditMode e PlayMode no Unity 6000.6.4f1; 44/44 e 203/203 aprovados.
- [x] Revisar diff e registrar a evidência no Trello.

### Task 2: Âncora e payload de respawn

**Files:**
- Create: `Assets/Scripts/Gameplay/Checkpoints/CheckpointAnchor2D.cs` e `.meta`
- Create: `Assets/Scripts/Gameplay/Checkpoints/RespawnRequest.cs` e `.meta`
- Create/Modify: `Assets/Tests/EditMode/CheckpointAttemptFlowEditModeTests.cs` e `.meta`

**Interfaces:**
- `CheckpointAnchor2D.TryGetCheckpoint(out string checkpointId, out Vector3 position, out Quaternion rotation)` valida ID e destino ativo.
- `RespawnRequest` expõe propriedades somente leitura `AttemptNumber`, `CheckpointId`, `Position`, `Rotation`.

- [x] Escrever testes de ID vazio, destino próprio/alternativo/inativo, fallback e payload imutável.
- [x] Executar testes EditMode; contratos e validações passaram.
- [x] Implementar âncora e payload mínimos, sem trigger/autoativação.
- [x] Revisar GUID/meta: 87 arquivos meta, zero GUIDs duplicados; EditMode completo aprovado.

### Task 3: Fluxo de checkpoint/tentativa

**Files:**
- Create: `Assets/Scripts/Gameplay/Checkpoints/CheckpointAttemptFlow2D.cs` e `.meta`
- Modify: `Assets/Tests/EditMode/CheckpointAttemptFlowEditModeTests.cs`
- Create: `Assets/Tests/PlayMode/CheckpointAttemptFlowPlayModeTests.cs` e `.meta`

**Interfaces:**
- `ConfigureStart(string checkpointId, Transform startPoint)` estabelece fallback inicial.
- `BindActor(DamageReceiver2D actor)`, `StartAttempt()`, `ActivateCheckpoint(CheckpointAnchor2D checkpoint)` e `RespawnRequested` são operações explícitas.
- `StartAttempt` inicia somente com destino inicial válido e ator configurado e vivo; número da tentativa cresce monotonicamente.
- morte encerra a tentativa uma vez e publica request com destino validado; se destino ativo inválido, usar fallback inicial, senão rejeitar com diagnóstico único.
- `OnDisable` remove handler de morte; `BindActor` troca handler com segurança.

- [x] Escrever testes EditMode/PlayMode para fallback, último checkpoint ativo, rejeições que preservam estado, tentativa seguinte, rebind e exceções.
- [x] Cobrir morte→pedido único, desativação/reativação, destino destruído/inválido, ator destruído/morto, ordem do knockback e segunda tentativa.
- [x] Executar red/green nos findings de assinante e ator morto; suítes completas aprovadas.
- [x] Implementar o fluxo mínimo, sem mutação direta do ator e sem dependência de GameBootstrap.
- [x] Executar as suítes completas, incluindo regressões relacionadas.

### Task 4: Revisões, documentação e integração

**Files:**
- Create: `docs/architecture/CHECKPOINTS_AND_ATTEMPTS.md`
- Modify: `docs/architecture/SAVE_AND_PROFILES.md`
- Modify: `docs/GDD_MASTER.md` somente onde a referência a #27 precise refletir escopo aprovado
- Modify: descrição/checklist Trello #27 após evidência

- [x] EditMode 44/44 e PlayMode 203/203 no Unity 6000.6.4f1; zero falhas/skips.
- [x] Regressões em movimento, pulo, dash, combate, combos/hit stop e parry cobertas nas suítes completas.
- [x] Revisões independentes SOLID, Performance e QA aprovadas; finding de ator morto corrigido. Performance estática, sem benchmark; risco registrado.
- [x] N/A justificado: API runtime local sem UI, coleta, dados pessoais ou conteúdo novo; docs atualizados.
- [x] `git diff --check` limpo; 87 metas verificadas, zero GUID duplicado; artefatos Unity removidos.
- [x] Commit do card: `30391faa46bcfa8c570a595bfca98d90db6fe6dd`; feature e `develop` publicados/verificados nesse SHA; árvore limpa.
- [x] Checklist DoD Trello 9/9 concluída; card movido para `🏆 18 — Concluído` após verificar a integração.
