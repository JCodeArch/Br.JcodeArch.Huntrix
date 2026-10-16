# Checkpoint NÃO SINCRONIZADO — Supabase (huntrx-honmoon)

Este arquivo registra o estado observado no repositório porque não foi possível persistir ao vivo no Supabase (`bkfjhgwxhcwhmvsitckp`, schema `ai_context`, chave `huntrx-honmoon`).

Motivo: não há ferramenta de MCP/Composio disponível neste ambiente e não há credencial `x-consumer-api-key`. O `.mcp.json` contém apenas o servidor Supabase direto, sem Composio, contrariando a regra `supabase_via_composio`. Não alegar persistência no Supabase.

---

## Estado do repositório (observado localmente)

- **Branch:** `develop`
- **HEAD:** `b197690` (Merge pull request #112)
- **Remote:** `origin/develop` = `b197690` (sincronizado via `git pull origin develop`, 208 commits trazidos)
- **Working tree:** limpo
- **Branch feature/card-30-zoey:** já está integrada a `develop` (presente nos commits remotos trazidos)

## Última ação executada

- `git pull origin develop` — trouxe todos os 208 commits pendentes do remoto, incluindo PRs #109–#118 (correções de UI/sessão: safe-area, input focus, camera restart, HUD read-only, sessão HUD/lifecycle, recorder, damage contact, opções, build artifact, evidence tooling).
- O card #30 (Zoey, ataque à distância) já estava integrado a `develop` antes deste pull (revisão estática aprovada em 2026-10-10, 26 testes autorados não executados, validação de runtime pendente).

## Próximas ações registradas (do handoff do Supabase / PROJECT_HANDOFF.json)

1. Login manual Unity Cloud (pendente, Google 502) + build2 no `develop` corrigido.
2. Importar/compilar Unity e executar EditMode/PlayMode.
3. Gerar APK e preservar artifact/digest/buildGUID.
4. Playtest humano de 60 segundos nas sete etapas da VerticalSlice.
5. Medir CPU/GPU/memória/FPS mobile; validar atlas/pooling.
6. Resolver gates de produto (cooperação, save/perfis, tuning, arte/áudio/narrativa, direitos).
7. Qualificar RC e resolver direitos/adaptação antes de distribuição.

## Fonte de contexto Supabase (lida dos docs commiteados, NÃO ao vivo)

- `docs/integrations/SUPABASE_AI_CONTINUITY.md` — protocolo obrigatório (get_handoff, commit_checkpoint, claim_task/update_task com CAS/leases).
- `docs/integrations/PROJECT_HANDOFF.json` — snapshot do estado (revisão `7d96dfa`, 154 documentos, 125 tarefas, 115 cards oficiais).
- `docs/integrations/SUPABASE_CONTEXT_SCHEMA.sql` — assinaturas das funções SQL.

## Regras aplicáveis

- Toda IA deve ler `ai_context.get_handoff('huntrx-honmoon')` antes de trabalhar e atualizar checkpoint nas transições e ao encerrar.
- Se o Supabase não puder ser alcançado: registrar checkpoint não sincronizado no repositório e reportar continuidade desatualizada, nunca alegar sincronização.
- Git é autoridade de código; Trello é backlog de produto; Supabase é espelho de continuidade operacional; Figma é design.

---

**Persistência no Supabase:** não confirmada. Próxima sessão deve tentar `get_handoff('huntrx-honmoon')` via Composio/Supabase autorizado e, se acessível, publicar este checkpoint com CAS na revisão lida.
