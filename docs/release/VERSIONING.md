# Versionamento e integração — card #106

`VERSION` é a fonte da versão do produto. `0.1.0-dev.1` identifica desenvolvimento e não uma versão publicada. A versão do Editor permanece em `ProjectSettings/ProjectVersion.txt`; são números independentes.

Use `python scripts/version.py check` para validar. `major`, `minor` e `patch` alteram somente VERSION; `set 0.2.0-rc.1` permite uma versão exata. O script não cria tag, release ou publicação. Bumps e notas entram em PR próprio. Códigos Android/iOS devem ser inteiros crescentes no pipeline, sem derivar unicidade apenas do SemVer.

Cada card usa `feature/card-<n>-<slug>`, criada a partir da `develop` atualizada, um commit descritivo e PR para `develop`. Integre dependências antes dos consumidores. Não reescreva histórico publicado nem descarte mudanças de outro agente. Um merge com QA adiada não autoriza release.

`develop` agrega integração. Uma branch `release/<versão>` congela conteúdo quando há evidências de compilação, gameplay, idade, acessibilidade, desempenho e direitos. Correções de release voltam à develop; tags `v<versão>` referenciam o commit efetivamente aprovado. Criação de branch de release e tag não elimina nenhum bloqueio do checklist de RC.

Mensagens explicam objetivo, comportamento, decisões e validação executada. PRs registram card, escopo e limites. O changelog deve distinguir alterações implementadas, testes executados e pendências. Uma mudança incompatível em contratos persistidos exige migração versionada, nunca apagar o progresso para acomodar o novo esquema.
