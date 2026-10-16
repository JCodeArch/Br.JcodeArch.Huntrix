# Andamento da sessão — HUNTR/X Honmoon (huntrx-honmoon)

**Data:** 2026-10-16  
**Chave do projeto:** `huntrx-honmoon`  
**Projeto Supabase:** `bkfjhgwxhcwhmvsitckp`  
**Schema:** `ai_context` (não acessível ao vivo neste ambiente)

---

## Estado do repositório (Git, authority de código)

- **Branch atual:** `develop`
- **HEAD:** `b197690` (Merge pull request #112)
- **Remote:** `origin/develop` = `b197690` (sincronizado via `git pull origin develop`)
- **Commits trazidos:** 208 commits pendentes do remoto (PRs #109–#118 incluídos)
- **Working tree:** limpo
- **Branches locais relevantes:** `feature/card-30-zoey` (já integrada a develop), `feature/unity-ui-layout`, `feature/card-30-zoey-melee-2026-10-05` (archive)
- **Commit de checkpoint não sincronizado:** `ead0a8f` na branch `docs/supabase-unsynced-checkpoint` (push para GitHub feito)

## Contexto Supabase (lido dos docs commiteados, NÃO ao vivo)

Não foi possível acessar o Supabase/Composio ao vivo: não há cliente MCP disponível neste ambiente e o `.mcp.json` contém apenas o servidor Supabase direto (sem Composio), contrariando a regra `supabase_via_composio`. Não há credencial `x-consumer-api-key`.

**Fonte lida:** `docs/integrations/PROJECT_HANDOFF.json` e `docs/integrations/SUPABASE_AI_CONTINUITY.md` (commit `3b28546`, ainda não integrado a `origin/develop`).

- **Revisão Supabase (snapshot):** `7d96dfa00def38227f7b6834ec3ccd5f515cd189` (histórico, não o HEAD real `b197690`)
- **Documentos carregados:** 154
- **Tarefas carregadas:** 125
- **Cards oficiais carregados:** 115 (card #105 não existe)
- **Estado:** `source_integrated: true`, `unity_validation: pending`, `apk: not_generated`
- **Bloqueador:** login Unity Cloud pendente (Google 502); build2 não submetido
- **Foco ativo:** Supabase publication and resume Unity/game UI
- **Figma:** file `AoNgky0oNVLU31UvML5xns`, 5 telas mobile, design completo, integração Unity pendente

## Última ação executada (esta sessão)

1. `git pull origin develop` — sincronizou `develop` com `origin/develop` (208 commits).
2. Inspeção do card #30 (Zoey, ataque à distância): implementação completa em `Assets/Scripts/Gameplay/Combat/ZoeyRangedAttack2D.cs`, `Assets/Scripts/Data/Definitions/ZoeyRangedDefinition.cs`, testes EditMode/PlayMode autorados (26 casos, não executados sem Unity Editor), revisão estática aprovada em 2026-10-10. Já integrado a `develop`.
3. Registro de checkpoint não sincronizado: `docs/governance/UNSYNCED_SUPABASE_CHECKPOINT.md` (commit `ead0a8f`, branch `docs/supabase-unsynced-checkpoint`, push para GitHub).

## Próximas ações (do handoff do Supabase / PROJECT_HANDOFF.json)

| Prioridade | Ação | Critério de conclusão |
|------------|------|----------------------|
| 1 | Login manual Unity Cloud (pendente, Google 502) + build2 manual no `develop` corrigido | Registrar número/URL da build e SHA resolvido; confirmar correspondência com head solicitado |
| 2 | Importar/compilar Unity e executar EditMode/PlayMode; corrigir bloqueios adicionais em branch própria sob revisão | Logs e XML reais ligados ao mesmo commit, sem promover configuração a resultado |
| 3 | Gerar APK e preservar artifact/digest/buildGUID/proveniência; instalar/testar em dispositivo | Artifact verificável ligado ao commit e relatório de instalação/interrupções |
| 4 | Percorrer sete etapas da VerticalSlice e executar playtest humano de 60 segundos (#68, #100–104) | Resultados humanos reais e perfis/configuração/build documentados |
| 5 | Medir CPU/GPU/memória/FPS mobile; validar atlas/pooling; gerar Windows e export/iOS | Medições e binários reais por plataforma/dispositivo |
| 6 | Resolver gates de produto: cooperação/modalidade/prova, save/perfis/storage/migração/sync, tuning final, arte/áudio/narrativa/tradução | Decisões explícitas aprovadas e evidência de integração |
| 7 | Qualificar RC e resolver direitos da adaptação/personagens/marca/música e gates loja/SDK/privacidade infantil | Gate RC preenchido com evidências no mesmo SHA e aprovação humana/direitos/documentados |

## Card #30 — Zoey (já integrado a develop)

- **Branch:** `feature/card-30-zoey` (já mesclada)
- **Implementação:** `ZoeyRangedAttack2D.TryFire(Vector2 aimDirection)` — tiro preciso imediato (hitscan) via raycast de 64 contatos, mira horizontal/diagonal/aérea
- **Definição:** `ZoeyRangedDefinition` — range 12, cooldown 0.3s, muzzle offset (0, 0.1) (valores provisórios)
- **Testes:** 11 EditMode + 15 PlayMode autorados, não executados (Unity Editor indisponível)
- **Revisão estática:** APROVADA (2026-10-10)
- **Validação de runtime:** pendente (sem Unity Editor no ambiente)
- **Deferido:** mapeamento de input físico, CharacterManager (#31), troca (#32), cooperação, animação/áudio final, tuning final

## Regras e restrições aplicáveis

- Toda IA deve ler `ai_context.get_handoff('huntrx-honmoon')` antes de trabalhar (SUPABASE_AI_CONTINUITY.md).
- Se o Supabase não puder ser alcançado: registrar checkpoint não sincronizado no repositório e reportar continuidade desatualizada, nunca alegar sincronização.
- Git é authority de código; Trello é backlog de produto; Supabase é espelho de continuidade operacional; Figma é design.
- Não declarar persistência no Supabase sem confirmação.
- Não alegar testes Unity como executados sem Editor disponível.
- Não promover merge, tooling sintético ou planos QA como build/diversão/performance/release aceitos.

## Persistência no Supabase — NÃO CONFIRMADA

Este checkpoint está gravado apenas no repositório (`docs/governance/UNSYNCED_SUPABASE_CHECKPOINT.md`). A próxima sessão deve tentar acessar o Supabase via Composio (chave `x-consumer-api-key`) e publicar este estado com `commit_checkpoint` usando CAS na revisão lida.

SQL para publicar (quando houver acesso):
```sql
select ai_context.get_handoff('huntrx-honmoon');
-- depois commit_checkpoint com p_expected_revision = revisão lida
```

## Endereços úteis

- Repositório: https://github.com/JCodeArch/Br.JcodeArch.Huntrix.git
- Trello: https://trello.com/b/O5yAS8lM/huntr-x-honmoon
- Figma: https://www.figma.com/design/AoNgky0oNVLU31UvML5xns
- Supabase MCP (direto, não Composio): https://mcp.supabase.com/mcp?project_ref=bkfjhgwxhcwhmvsitckp&features=database,docs
- Unity Cloud org: 4674262819723, projeto: acc5ceb5-588f-4ef7-9b7b-3e50d3139a49, target: Huntrix-Android-develop, editor: 6000.6.4f1
