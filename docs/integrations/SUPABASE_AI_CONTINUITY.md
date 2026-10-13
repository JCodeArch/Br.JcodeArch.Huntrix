# Continuidade obrigatória das IAs — múltiplos projetos

Toda IA deve ler o contexto no Supabase **antes de iniciar qualquer trabalho**, reconciliá-lo com as fontes reais e atualizá-lo nas transições relevantes e ao encerrar a sessão. Esta regra também vale para trabalho de design, documentação e validação. Uma IA sem acesso não deve alegar que leu ou atualizou o contexto: deve comunicar o impedimento.

Projeto Supabase compartilhado: `bkfjhgwxhcwhmvsitckp`. Schema privado genérico: `ai_context`. Cada projeto possui sua própria `project_key`; neste repositório, ela é `huntrx-honmoon`. Acesso exclusivamente pela conexão SQL autorizada do MCP, como operador PostgreSQL. Não colocar credenciais no repositório, criar cliente público ou expor o schema pela Data API. As funções são `SECURITY INVOKER`; acesso de `public`, `anon` e `authenticated` está revogado.

## Isolamento e cadastro de projetos

Toda leitura e escrita deve usar a chave do projeto em trabalho. Objetivos, regras, branch, fontes, tarefas, documentos e artefatos são isolados por projeto. Não importar premissas de HUNTR/X para outro projeto. A separação lógica por chave não representa uma barreira de autorização entre operadores MCP privilegiados.

Para cadastrar um projeto novo, escolha uma chave estável e execute o insert abaixo com nome e URL reais; `repository_url` aceita `null` para projetos sem repositório. Exemplo neutro, somente para cadastro de um projeto chamado Exemplo:

```sql
insert into ai_context.projects (project_key, name, repository_url)
values ('exemplo-projeto', 'Exemplo', null);

select ai_context.get_handoff('exemplo-projeto');
```

O cadastro inicia revisão zero, sem checkpoint. Publique o primeiro checkpoint com `p_expected_revision = 0`, usando objetivo, regras e estado próprios desse projeto. Para um projeto existente, leia a revisão atual; não recrie, não redefina a revisão e não sobrescreva o cadastro. Exemplos HUNTR/X abaixo devem ter a chave substituída ao trabalhar em outro projeto.

## Leitura inicial

Execute:

```sql
select ai_context.get_handoff('huntrx-honmoon') as handoff;

select path, content, sha256, source_commit, captured_at
from ai_context.documents
where project_key = 'huntrx-honmoon'
order by path;
```

`get_handoff` reúne projeto, checkpoint atual, tarefas, metadados dos documentos, artefatos e os 30 eventos mais recentes. O conteúdo integral dos documentos exige a segunda consulta. Guarde `project.revision`, versões das tarefas e referências das fontes. Confirme identidade do repositório, branch, HEAD e alterações locais. `develop` e `7d96dfa` são referência inicial histórica; consulte o HEAD real. Releia cartões Trello pertinentes antes de agir sobre eles.

Git comprova commits e arquivos; Trello comprova o estado observado dos cartões; resultados de validação comprovam critérios de aceite. Cartão concluído e afirmação anterior de uma IA não substituem evidência de implementação. Registre divergências e fontes indisponíveis explicitamente.

## Checkpoint completo com CAS

Assinatura instalada:

```sql
ai_context.commit_checkpoint(
  p_project_key text,
  p_expected_revision bigint,
  p_payload jsonb,
  p_writer text,
  p_source_metadata jsonb default '{}'::jsonb
) returns bigint
```

O payload é um objeto JSON completo e deve conter `project`, `current_state`, `resume` e `operating_rules`. Inclua objetivo, restrições, branch/HEAD verificados, decisões, estado das tarefas, bloqueios, próxima ação exata, evidências e tempos de observação. Preserve campos existentes relevantes: não substitua um checkpoint completo por um resumo parcial.

Chame a função com a revisão realmente lida e um identificador único da sessão. A função bloqueia a linha do projeto, verifica a revisão esperada, insere o snapshot, incrementa a revisão e registra evento atomicamente. O checkpoint atual é aquele cuja revisão coincide com `projects.revision`; não há coluna de ponteiro separada.

Em erro `40001`, releia o handoff, reconcilie alterações concorrentes e refaça o payload antes de tentar novamente. Não repita cegamente um snapshot antigo. Se a resposta se perder, consulte checkpoints/eventos e verifique se a operação já foi aplicada antes de repetir. O schema atual não oferece chave de idempotência dedicada; use um identificador de operação no payload/metadados para essa investigação.

## Tarefas e leases

Assinaturas instaladas:

```sql
ai_context.claim_task(
  p_project_key text, p_task_key text, p_expected_version bigint,
  p_actor text, p_seconds integer default 900
) returns jsonb

ai_context.update_task(
  p_project_key text, p_task_key text, p_expected_version bigint,
  p_lease_token uuid, p_status text, p_details jsonb,
  p_evidence jsonb, p_actor text, p_release boolean default true
) returns bigint
```

Antes de reivindicar, verifique dependências e critérios de aceite. `claim_task` não verifica dependências automaticamente. Use a versão lida e um lease de 900 segundos; a função retorna a nova versão, token e prazo. Status permitidos: `pending`, `in_progress`, `blocked`, `implemented_pending_validation`, `validated`, `done`, `cancelled`.

Cada atualização exige versão atual, token válido, mesmo ator e prazo não expirado. `p_release=true` libera a tarefa; `false` conserva o lease, mas **não renova seu prazo**. Não existe função de renovação instalada. Antes do vencimento, conclua e libere um trecho coerente de trabalho; releia e reivindique novamente se necessário. Não escreva com token expirado. Para retomar lease vencido, reconcilie primeiro commits, arquivos, artefatos e eventos do proprietário anterior.

Ao atualizar tarefa e checkpoint relacionados, execute as duas chamadas na mesma transação SQL, com parâmetros reais derivados da leitura anterior:

```sql
begin;
-- select ai_context.update_task(...);
-- select ai_context.commit_checkpoint(...);
commit;
```

Os comentários representam chamadas a preencher; não constituem atualização executável. Falha em qualquer chamada exige rollback. Leases de banco não impedem escrita externa por um agente antigo: revalide propriedade antes de alterar Git ou Trello e evite edição simultânea dos mesmos arquivos.

## Quando atualizar e como encerrar

Publique checkpoint ao iniciar após reconciliação, assumir tarefa, mudar escopo/decisão, implementar etapa material, validar, encontrar/resolver bloqueio, produzir artefato ou alterar fonte externa. Grave evidência concreta: commit, arquivo/versão, resultado de teste, ID e estado observado de cartão. Marque `done` somente com critérios de aceite cumpridos.

Ao encerrar, publique estado verificado, pendências, próxima ação, bloqueios, referências e frescor das fontes; libere leases da sessão e releia o handoff para confirmar a revisão persistida. Se a persistência falhar, informe que o handoff não foi salvo. Uma próxima IA deve reconstruir interrupções a partir de evidências, sem inventar progresso.

Este protocolo é obrigatório e dirigido por eventos de sessões ativas. Não existe processo automático agendado ou monitoramento contínuo implícito. Toda observação deve carregar data/hora; o contexto pode ficar desatualizado entre sessões.

## Fontes SQL verificáveis

Inspecione definições reais antes de adaptar clientes ou consultas:

```sql
select p.proname, pg_get_function_identity_arguments(p.oid) as arguments,
       pg_get_functiondef(p.oid) as definition
from pg_proc p
join pg_namespace n on n.oid = p.pronamespace
where n.nspname = 'ai_context'
order by p.proname;

select revision, writer, source_metadata, created_at
from ai_context.checkpoints
where project_key = 'huntrx-honmoon'
order by revision desc limit 10;
```

`documents` guarda corpo, SHA-256 e commit de origem; `events` registra transições; `artifacts` registra URI, commit e estado de validação. URI isolada não comprova disponibilidade nem conclusão. O SQL efetivamente instalado é a fonte para assinaturas e garantias, e a regra operacional acima é a fonte para obrigações das IAs.

## Conectar outras IAs

A configuração `.mcp.json` usa o servidor oficial Supabase, limitado ao projeto compartilhado `bkfjhgwxhcwhmvsitckp` e às ferramentas database/docs. Não contém token. Cada cliente/IA precisa autenticar sua própria conexão OAuth autorizada; esse arquivo não autentica nem concede acesso sozinho. O banco contém múltiplos projetos lógicos, separados por `project_key`.

Clientes que aceitam `.mcp.json` podem reutilizá-lo. No Codex, a configuração equivalente é:

```bash
codex mcp add supabase --url 'https://mcp.supabase.com/mcp?project_ref=bkfjhgwxhcwhmvsitckp&features=database,docs'
```

Depois de autenticar, a primeira chamada deve ler `ai_context.get_handoff` com a chave correta. Para leitores exclusivos, use `read_only=true`; agentes responsáveis por atualizar o estado precisam da conexão autorizada de escrita. Nunca compartilhe chaves service_role ou tokens em chats, Git ou Figma.

Referência: https://supabase.com/docs/guides/getting-started/mcp
