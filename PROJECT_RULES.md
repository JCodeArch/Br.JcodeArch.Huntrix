# Regras do projeto HUNTR/X — Honmoon

Repositório: `JCodeArch/Br.JcodeArch.Huntrix`.
Regras definidas pelo proprietário do projeto, Jonathas (Joe).
Atualizado em 10/10/2026, horário de São Paulo.

## 1. Contexto e ordem de trabalho

- Ler `docs/PROJECT_CONTEXT.md`, o GDD e o cartão correspondente no Trello antes de implementar.
- Usar o Trello como backlog principal e preservar sua ordem.
- Preservar a ordem de integração das dependências; preparar e implementar tarefas em paralelo quando seus contratos e arquivos estiverem separados.
- Decisão do proprietário em 10/10/2026: avançar a implementação sem aguardar a execução dos testes Unity, que ficará para quando o jogo estiver quase pronto. Registrar cada entrega como implementada, com validação de runtime pendente.
- Preservar trabalho existente de outros agentes e ferramentas.
- Registrar decisões, limitações e evidências para permitir continuidade em outra sessão.

## 2. Agentes e responsabilidades

- Selecionar especialistas conforme o assunto: arquitetura, gameplay, QA, revisão de código, SOLID, desempenho, UX, arte, áudio, plataforma e integração.
- Cada agente deve ter responsabilidade, arquivos sob sua responsabilidade, entrega e critérios de aceite claros.
- Quando surgir um problema que exija outra especialidade, criar ou acionar um agente especialista para solucioná-lo.
- Agentes com dependências devem comunicar diretamente o bloqueio ao agente responsável, solicitar a entrega necessária e informar o coordenador.
- Evitar edição simultânea dos mesmos arquivos; coordenar mudanças compartilhadas.
- Preferência do proprietário: mais de 10 agentes em paralelo. Respeitar o limite real da sessão; quando houver menos vagas, distribuir as responsabilidades em rodízio e informar o limite, sem afirmar que agentes inexistentes estão executando.

## 3. Branches, commits e integração

- Criar uma branch por feature, partindo de `develop` atualizada.
- Padrão: `feature/card-<numero>-<nome>`, por exemplo `feature/card-30-zoey`.
- Usar branches próprias para correções e documentação quando constituírem tarefas independentes.
- Todos os commits devem ter uma descrição clara: problema ou objetivo, alterações realizadas, motivo das decisões e verificações executadas.
- Registrar testes não executados e limitações relevantes na mensagem ou evidência associada; nunca apresentar teste escrito como teste aprovado.
- Fazer commit e push da branch e abrir PR com destino a `develop`.
- O PR deve explicar comportamento, escopo, validação, limitações e cartão do Trello.
- Fazer merge para `develop` após revisão e atendimento dos critérios de integração aplicáveis, para os testes integrados.
- A autorização para commit, push e criação de PR já foi dada pelo proprietário. Não pedir novamente para essas ações dentro do escopo aprovado.
- Não confundir commit local, branch publicada, PR criado e merge realizado: verificar e comunicar cada estado separadamente.

## 4. Qualidade e testes

- Aplicar `docs/governance/DEFINITION_OF_DONE.md`.
- Exigir revisão independente proporcional à mudança e corrigir findings bloqueadores.
- Adiar a execução de EditMode, PlayMode, regressão e playtests Unity para a fase final, conforme orientação expressa do proprietário em 10/10/2026. Preservar os testes existentes e registrar os cenários ainda necessários em `docs/governance/DEFERRED_VALIDATION.md`.
- Integrar em `develop` após revisão independente sem bloqueadores e verificações estáticas aplicáveis, mantendo explícita a validação de runtime pendente. Essa integração não comprova que o jogo compila ou está pronto para distribuição.
- Verificar arquivos `.meta`, GUIDs, referências e integração dos prefabs.
- Revisão estática não substitui compilação, EditMode, PlayMode ou inspeção visual.
- Não declarar desempenho mobile sem medição no dispositivo correspondente.
- Se faltar Editor, licença, acesso ou outra dependência, registrar a limitação concreta e preservar a entrega revisável. O adiamento dos testes não impede outras implementações autorizadas; não marcar a validação final como concluída indevidamente.

## 5. Atualizações ao proprietário

- Enviar atualizações durante o trabalho, incluindo implementação, testes, revisão, publicação e bloqueios.
- Explicar o que foi concluído, o que está em execução e o que falta.
- Durante execução ativa, não deixar mais de 60 segundos sem uma atualização significativa quando o ambiente permitir comunicação.
- Reportar resultados verificados e distinguir intenção de ação executada.
- Não prometer execução em segundo plano ou notificações futuras sem uma tarefa ou automação efetivamente configurada.

## 6. Autorização para confiar na pasta

O proprietário autorizou expressamente, em 10/10/2026, clicar em
**“Confiar nesta pasta e continuar”**, **“Confiar na pasta e continuar”**
ou controle equivalente de confiança do workspace para a pasta deste projeto
no Codespace, sempre que necessário, sem repetir a pergunta.

- Escopo: `/workspaces/Br.JcodeArch.Huntrix` no Codespace do HUNTR/X.
- Codespace informado: `https://cautious-potato-vg7xvv6xj9rcxrp7.github.dev/`.
- Confirmar que o prompt se refere à pasta deste projeto antes de clicar.
- Esta autorização trata da confiança do workspace; não concede autorização genérica para ampliar permissões, publicar dados privados, aceitar contratos, alterar credenciais ou remover proteções de outros sistemas.
- Cumprir as exigências de confirmação da plataforma quando forem aplicáveis a uma ação diferente da confiança desta pasta.

## 7. Continuidade e rastreabilidade

- Atualizar o contexto e o Trello com entregas, evidências, commit/PR e pendências.
- Preservar código em Git e fornecer patch quando a publicação estiver bloqueada.
- Ao retomar, verificar o estado atual da branch e do ambiente antes de repetir ações.
- Ler este arquivo no início de novas sessões de trabalho do projeto. Este documento registra as preferências do proprietário; não equivale a memória permanente da aplicação.
