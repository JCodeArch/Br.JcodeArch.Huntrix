# Estrutura de save e perfis — HUNTR/X: Honmoon

**Card de origem:** [09 — Definir estrutura de save](https://trello.com/c/54w6st2I/9-09-definir-estrutura-de-save)  
**Estado:** contrato lógico de perfis, progresso por capítulo, autosave e replay definido; formato e persistência concreta ficam para implementação.  
**Escopo:** manter progresso independente por perfil e permitir retomar/rejogar capítulos. Não define serviço cloud, formato serializado, arquitetura de rede ou regras de recompensa.

## Decisões do item 09

1. **Múltiplos perfis independentes:** cada perfil mantém sua própria campanha e referências de progressão. O limite de perfis, nome/avatar, ações de criar/copiar/renomear/apagar e forma de seleção continuam em aberto para a UI e os cards de acessibilidade.
2. **RECOMENDAÇÃO para o protótipo — save local sem conta:** preferir salvar e carregar progresso nesta instalação sem exigir cadastro, conexão de rede ou sincronização entre dispositivos. É uma direção provisória, não um requisito de produto aprovado; backend/persistência local e eventual cloud sync dependem das decisões de plataforma e release.
3. **Progresso por capítulo:** guardar identificador do capítulo atual/de retomada, capítulos liberados e capítulos concluídos. A ordem narrativa e as condições que liberam cada capítulo pertencem ao conteúdo/level design; o save persiste o resultado desses sistemas e não os decide.
4. **Autosave em transições duráveis:** salvar ao criar um perfil, ao concluir um capítulo, quando uma mudança aprovada de progressão libera conteúdo e nos checkpoints que os cards de gameplay definirem. Não salvar estado transitório de combate a cada frame/ação. Até que checkpoints sejam definidos, retomar capítulo em andamento pelo início dele, preservando os capítulos concluídos.
5. **Replay separado da campanha:** permitir iniciar novamente capítulos já liberados/concluídos sem apagar o progresso da campanha. Métricas, recordes, critérios de ranking e efeitos de replay são do item 54; obtenção de equipamentos pertence ao item 52 e seus efeitos ao item 53. Este item não define prêmios repetíveis.
6. **Sem dados pessoais no save:** usar um identificador opaco local para distinguir perfis; não guardar nome civil, data de nascimento, contato, conta ou identificadores de rede. Vínculo com faixa etária e suas opções só pode ser definido pelos cards 85–103, respeitando privacidade e Child Safety.

O produto continua prevendo progressão por perfil. A quantidade de perfis, preferências por perfil ou por dispositivo, relação com `AgeProfileData`, direitos de perfil em coop, resolução de conflitos e política de exclusão continuam **EM ABERTO**. Modalidade local/online/ambas da cooperação também não é decidida por este card.

## Contrato lógico de dados

O formato serializado é futuro, mas a implementação deve preservar estes limites conceituais:

| Registro lógico | Conteúdo esperado | Limites |
|---|---|---|
| `ProfileSave` | Identificador local opaco, versão do esquema, progresso de campanha, preferências apenas se os cards decidirem que pertencem ao perfil | Não conter PII, credenciais, estado de sessão/rede ou dados transitórios de combate |
| `CampaignProgress` | Capítulo de retomada, IDs de capítulos liberados/concluídos e referência ao último checkpoint persistível quando esse sistema estiver definido | IDs devem apontar para conteúdo; não duplicar regras de desbloqueio nem estado de inimigo/personagem em execução |
| `ProgressionReferences` | IDs dos itens/equipamentos realmente liberados pelo sistema de progressão | Card 52 define a obtenção; card 53 define efeitos; o save apenas persiste os IDs e estados aprovados |
| `ReplayRecords` | Resumos/recordes associados a capítulo e perfil somente quando o item 54 definir os campos e critérios | Dados de replay não sobrescrevem `CampaignProgress` nem definem rankings por este contrato |

Configurações de controle, acessibilidade, áudio e idioma precisam de uma decisão explícita sobre escopo (perfil ou dispositivo); não inferir que preferências de um perfil substituem as de outro. O perfil infantil não deve expor nem armazenar dados pessoais para resolver essa decisão.

## Autosave e recuperação

- Cada escrita deve preservar o último save válido se a gravação for interrompida ou falhar; a implementação futura deve sinalizar o problema sem apresentar progresso não gravado como persistido.
- Gravar depois de transições de progresso estáveis e consistentes, não durante atualização de frame nem no meio de uma ação de combate.
- Ao carregar, validar a versão/esquema e a integridade estrutural antes de aplicar progresso. Migração, recuperação de arquivo inválido e política de backup precisam ser definidas e verificadas quando a serialização/storage forem implementadas.
- Perfil sem progresso começa no primeiro conteúdo jogável. Perfil existente carrega apenas os dados daquele identificador. Troca de perfil não pode direcionar escrita ao perfil anterior depois da mudança.
- Se não houver checkpoint aprovado para o capítulo atual, retomar pelo início desse capítulo; não prometer retomada do quadro exato ou de inimigos em combate.

## Gates para implementação e QA

- Os itens de save/profile e progressão posteriores devem decidir limites de perfis, operações de UI, preferências, política de falha/migração, sincronização e regras de recompensa sem violar a separação por perfil.
- QA deverá verificar criação/seleção/troca sem vazamento de progresso entre perfis; conclusão e liberação de capítulo; autosave nos limites acordados; retomada depois de fechar/reabrir; replay sem apagar campanha; e falha de escrita preservando o último save válido.
- Testar em dispositivos e plataformas só depois que os tiers/dispositivos e a implementação de persistência estiverem definidos. Sincronização cloud, se um card futuro a aprovar, exige testes próprios. Nenhuma compatibilidade, persistência entre dispositivos ou recuperação foi medida por este documento.
- A integração cooperativa de saves aguarda a decisão local/online e não pode pressupor que vários jogadores compartilhem um perfil.

## Referências do backlog

- [Card 04 — Definition of Done](https://trello.com/c/oDEBZawI/4-04-definir-definition-of-done): gates de QA, integração e evidência.
- [Card 09 — estrutura de save](https://trello.com/c/54w6st2I/9-09-definir-estrutura-de-save): origem deste contrato.
- [Card 27 — Implementar checkpoints](https://trello.com/c/k43sA6MK/27-27-implementar-checkpoints): define respawn, recuperação de estado e fluxo de tentativa; este card persiste somente o checkpoint aprovado por ele.
- [Card 52 — Implementar progressão de equipamentos](https://trello.com/c/um3Ke7Ob/52-52-implementar-progress%C3%A3o-de-equipamentos): aquisição de peças em pontos narrativos.
- [Card 53 — Definir efeitos das peças](https://trello.com/c/FJ58KiwC/53-53-definir-efeitos-das-pe%C3%A7as): efeitos mecânicos de cada peça.
- [Card 54 — replay e rankings](https://trello.com/c/N7UiMtCh/54-54-implementar-replay-e-rankings): critérios e dados de recordes.
- [Card 85 — AgeProfileData](https://trello.com/c/mMQB7Zgi/75-85-ageprofiledata): estrutura dos perfis etários.
- [Card 90 — Accessibility](https://trello.com/c/e477OffI/80-90-accessibility): opções de acessibilidade e remapeamento.
- [Card 91 — Child Safety](https://trello.com/c/POdhnFFJ/81-91-child-safety): coleta de dados e proteções infantis.
- [Card 102 — Age QA](https://trello.com/c/yzBKwWfb/92-102-age-qa) e [card 103 — Accessibility QA](https://trello.com/c/hi25R5Rd/93-103-accessibility-qa): validação por perfil e recurso.


