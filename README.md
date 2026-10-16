# HUNTR/X — Honmoon

Projeto Unity 2D de ação e plataformas: alternância entre Rumi, Mira e Zoey, resgate de fãs, Honmoon, Special conjunto e equipamentos por progressão narrativa. O backlog oficial é o [Trello do projeto](https://trello.com/b/O5yAS8lM/huntr-x-honmoon).

O estado atual é **desenvolvimento com protótipos integrados**. Integração na `develop` não comprova importação, compilação, gameplay, performance ou prontidão para publicação. Veja os limites e as evidências em [contexto](docs/PROJECT_CONTEXT.md), [validação adiada](docs/governance/DEFERRED_VALIDATION.md) e [revisões](docs/reviews/).

## Abrir e executar

1. Clone o repositório e selecione `develop`.
2. No Unity Hub, abra a raiz que contém `Assets`, `Packages` e `ProjectSettings`. Use **6000.6.4f1**, conforme `ProjectSettings/ProjectVersion.txt`. O Codespace permite editar C#, mas seu VS Code não substitui o Editor Unity.
3. Aguarde a importação e resolva erros de compilação antes de Play. Use `CombatLab` para os controles lógicos de combate e `EnemyLab` para os encontros de inimigos. Esses labs anteriores não possuem entrada física completa.
4. Abra `Assets/Scenes/VerticalSlice.unity` e entre em Play após a importação passar. A cena compõe sete etapas: avião, ramen, primeiro resgate, horda/Special, miniboss, paraquedas e show. Não exige configuração manual do Inspector. A execução dessa composição ainda precisa ser verificada no Unity.
5. As nove cenas `*_Blockout.unity` permitem inspecionar as áreas; `ArtBlockout` e `ArtVerticalSlice` apresentam os assets originais de protótipo. Arte, animação e áudio finais ainda precisam de avaliação em execução.

Controles da slice: A/D e W/S para movimento/direção, Espaço para salto, Shift para dash, J para ataque, K para parry, Q para troca, E para interação/resgate, R para comida própria, F para comida no fã e L para Special. Durante resgate, mantenha E ou ative a opção de alternância. No gamepad, use stick/D-pad, South para salto, RB para dash, West para ataque, East para parry, LB para troca, North para interação, Select para comida própria, clique do stick direito para comida no fã, RT para Special e Start para pausa/opções (Esc no teclado; remapeável nas opções). O painel touch envia as mesmas ações.

Em **Opções e acessibilidade**, escolha idioma da narrativa PT/EN e faixa etária, ajuste flashes, legendas, mira assistida, salto/resgate por alternância e remapeamento de teclas. Ao terminar a abertura, o formulário aceita uma avaliação humana e pode salvar um resumo JSON local. O recorder distingue uma janela completa de 60 segundos de uma sessão mais curta; ainda não há playtest humano registrado.

## Testes, build e contribuição

Preserve os testes existentes. Em um ambiente com Editor licenciado, execute `bash scripts/run-unity-tests.sh` com `UNITY_EDITOR` apontando para o executável 6000.6.4f1. O runner preserva resultados por execução e registra proveniência do commit. Sem Editor configurado ele falha explicitamente; nenhum XML ou build é inventado.

`bash scripts/build-unity.sh` prepara targets Windows, Android e exportação iOS por cenas explícitas; veja `docs/build/` para argumentos e requisitos. A CI executa verificações de ferramentas em PRs e pushes, enquanto suites e builds Unity exigem runner licenciado configurado. `scripts/check-release-candidate.py` exige evidências vinculadas ao commit e ao build. A validação e os builds do HEAD atual, medições em dispositivos e aprovação de RC continuam pendentes.

Há evidência histórica de APK Android de desenvolvimento e **81 EditMode + 293 PlayMode = 374 testes aprovados** na fonte `2fc41a1230e00e453dca04024a72746a1f8adfdf` do PR #100. O contexto também registra suites em `0c4a8e4053140b76ded470cbd05f899f89a26bce`. Esses resultados pertencem aos SHAs indicados; não validam alterações posteriores. Consulte [proveniência e ressalvas](docs/PROJECT_CONTEXT.md), incluindo modificações geradas no checkout histórico. Instalação em aparelho, playtest humano, Windows/iOS e RC exigem evidências próprias.

O Codespace continua útil para C#, Git e ferramentas; edição online pode usar uma VM ou runner com Unity para a execução final, sem exigir instalar o Editor na máquina pessoal.

Crie uma branch por feature/card a partir da `develop` atualizada, um commit que explique objetivo, alterações e validação, e um PR para `develop`. Corrija findings antes do merge e registre evidências no Trello. As [regras do projeto](PROJECT_RULES.md) e o [guia de versionamento](docs/release/VERSIONING.md) definem o fluxo.

Leia o [GDD](docs/GDD_MASTER.md), o [guia da arquitetura](docs/architecture/SYSTEM_MAP.md) e o [guia de conteúdo](docs/content/CONTENT_AUTHORING.md). Assets e `.meta` devem entrar juntos. Não inclua credenciais, certificados, dados pessoais, binários gerados ou música sem direitos documentados. Consulte [preparação de publicação](docs/release/PUBLICATION_PACKAGE.md) antes de distribuição.


## Assets e Git LFS

O Git LFS acompanha os formatos de arte, modelos, áudio e vídeo em `.gitattributes`. Após clonar, execute `git lfs install --local` e `git lfs pull` para obter o áudio original. Scenes, prefabs, assets, metadados e imagens pequenas permanecem no Git normal; não migre histórico sem uma tarefa específica. Veja o [pipeline dos agentes](docs/architecture/AGENT_PIPELINE.md) para responsabilidades e revisões.
