# Regras para toda IA

Antes de iniciar qualquer trabalho, leia o handoff pela conexão SQL autorizada do MCP Supabase (`bkfjhgwxhcwhmvsitckp`):

```sql
select ai_context.get_handoff('huntrx-honmoon');
select path, content, sha256, source_commit, captured_at
from ai_context.documents
where project_key = 'huntrx-honmoon' order by path;
```

Siga `docs/integrations/SUPABASE_AI_CONTINUITY.md`. O schema `ai_context` é multiprojeto: use `huntrx-honmoon` neste repositório e a chave própria em outro projeto; nunca importe regras HUNTR/X para ele. Reconcilie fontes atuais, atualize checkpoint/tarefa nas transições materiais e ao encerrar e use CAS, versão e lease. Não declare persistência sem confirmação. Sem acesso, comunique o impedimento.

Verifique branch, HEAD e working tree antes de editar; `develop` e `7d96dfa` são referência histórica, não HEAD garantido. Preserve alterações de outros agentes e respeite o escopo de branch autorizado. Não exponha schema, credenciais ou chaves em clientes públicos. Não alegue monitoramento automático sem processo instalado e verificado.

**Premissa de Build e Qualidade de Código:** O projeto está compilando com sucesso e sem erros ou warnings. Todas as assemblies (`HuntrX.Runtime`, `HuntrX.Editor`, `HuntrX.EditModeTests`, `HuntrX.PlayModeTests`) devem compilar com **0 erros e 0 warnings** (.NET 8 / C# 9.0). Nenhuma alteração pode introduzir novos erros de compilação ou warnings no código.
