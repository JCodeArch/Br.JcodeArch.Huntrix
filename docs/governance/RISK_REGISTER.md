# Plano e registro inicial de riscos — HUNTR/X: Honmoon

**Card de origem:** [11 — Criar plano de riscos](https://trello.com/c/vlFLYHBd/11-11-criar-plano-de-riscos)  
**Baseline:** 2026-10-01 (America/Sao_Paulo); pré-produção, antes da fundação Unity e do Vertical Slice.  
**Estado:** riscos identificados; respostas e gates registrados; nenhuma licença, métrica de desempenho, classificação ou conformidade de publicação é declarada como obtida.

## Objetivo e método

Este registro torna explícitos os riscos de direitos autorais/marcas, escopo, diversão, performance, arte, áudio, segurança/idade e publicação. Ele orienta a ordem do backlog e os gates para avançar; não é parecer jurídico, certificação de segurança, aprovação de loja ou prova de que uma mitigação já foi executada.

As probabilidades **não foram estimadas**: ainda não há assets finais licenciados, build, medições, playtest ou decisões completas de publicação. Por isso, este baseline não calcula um score numérico nem apresenta hipótese como frequência. A prioridade indica a urgência do tratamento pelo impacto potencial e pelo gate necessário:

- **P0 — bloqueio de distribuição:** não publicar, comercializar, promover com o conteúdo ou habilitar o recurso afetado enquanto a evidência de liberação não existir.
- **P1 — gate antes de escalar/validar:** fechar a decisão ou coletar evidência no card indicado antes de ampliar conteúdo ou declarar o Vertical Slice validado.
- **P2 — gate de release/monitoramento:** preparar evidência específica por plataforma e reavaliar antes de cada release.

**Estado inicial:** todos os riscos abaixo estão **ABERTOS**. A função indicada é uma sugestão de responsabilidade a atribuir; nenhuma pessoa foi designada neste card. “Mitigação planejada” não significa que o risco está mitigado.

## Registro de riscos

### R-01 — Direitos autorais e adaptação de obra

- **Prioridade / estado:** P0 — ABERTO; bloqueia distribuição do conteúdo não liberado.
- **Risco:** personagens, história, cenários, figurinos, símbolos, diálogos ou elementos reconhecíveis associados ao filme podem não estar cobertos pelos direitos disponíveis ao projeto. Criar arte própria ou dar crédito, por si só, não demonstra autorização.
- **Sinal de disparo:** qualquer proposta, arquivo, build, trailer ou material público usa ou deriva de elemento identificável sem documentação de titularidade/licença aplicável.
- **Prevenção e resposta:** antes de incorporar conteúdo em uma entrega distribuível, registrar titular/contato, cadeia de autorização, elementos cobertos, usos (jogo, trailer, divulgação), plataformas/territórios, prazo, limites, créditos, aprovações e condições. Manter um registro de proveniência por asset conforme `docs/art/ASSET_PIPELINE.md`. Sem evidência suficiente, substituir por material original não confundível ou manter o conteúdo fora da build pública; encaminhar a decisão a responsável qualificado e aos titulares.
- **Gate relacionado:** cards 69–70 e 74 para assets específicos; gate de release em 92–111. Nenhum desses cards, sozinho, concede direitos.
- **Responsável sugerido:** responsável de produto/licenciamento, com assessoria jurídica qualificada.

### R-02 — Marcas, identidade e promoção

- **Prioridade / estado:** P0 — ABERTO para qualquer uso público/comercial de marcas ou identidade reconhecível.
- **Risco:** nome do jogo, nomes/logotipos de grupo ou personagens, símbolos, apresentação visual e materiais promocionais podem sugerir autorização ou associação inexistente, ou exceder o escopo permitido por uma licença de conteúdo.
- **Sinal de disparo:** uso de nomes, logos, vestuário/símbolos distintivos ou alegação de vínculo em página de loja, redes, trailer, press kit, merch ou build pública.
- **Prevenção e resposta:** revisar separadamente direitos sobre obra/personagens e marcas; registrar permissão de uso comercial e canais aprovados. Não usar aprovação de arte ou autorização para gameplay como aprovação automática de marca, marketing ou merchandising. Suspender a peça pública afetada enquanto escopo e aprovação não estiverem documentados.
- **Gate relacionado:** cards 69–70, 74 e 107–111; validação de release permanece aberta.
- **Responsável sugerido:** produto/marketing e licenciamento, com assessoria jurídica.

### R-03 — Escopo, dependências e cooperação

- **Prioridade / estado:** P1 — ABERTO; gate antes de expandir o Vertical Slice.
- **Risco:** a visão inclui até três jogadores/personagens simultâneos, múltiplas plataformas, conteúdo narrativo amplo, perfis etários e vários sistemas de combate. A modalidade de cooperação e os critérios de aprovação do slice continuam em aberto; implementar tudo como requisito simultâneo pode tornar o MVP de validação inexequível ou ocultar dependências.
- **Sinal de disparo:** implementação começa antes de fechar dependências; itens são adicionados ao slice sem critério de aceite; coop/rede ou conteúdo fora do recorte entra sem revisão de escopo.
- **Prevenção e resposta:** respeitar os gates 02–11; manter o MVP como Vertical Slice de validação definido no item 03; exigir que cada card consumidor cite dependências e aceite observável; registrar mudança de escopo e impacto antes de aprová-la. Não remover o requisito de cooperação do produto por conveniência: decidir modalidade e prova necessária nos cards correspondentes antes de declarar o MVP validado.
- **Gate relacionado:** cards 02–05, 12–18, 61–68 e decisão de cooperação anterior às dependências de rede.
- **Responsável sugerido:** produto/design e System Architect.

### R-04 — Diversão e game feel ainda não comprovados

- **Prioridade / estado:** P1 — ABERTO; gate antes de escalar a produção.
- **Risco:** pilares e protocolo/alvos provisórios estão documentados, mas ainda não existe build nem resultado de playtest. Confundir intenção, checklist ou alvo provisório com evidência de diversão pode levar a investir em conteúdo antes de validar o combate e o slice.
- **Sinal de disparo:** expansão de fases, animações ou bosses é aprovada sem executar o experimento definido e registrar os resultados, ou um alvo provisório passa a ser tratado como aprovado sem decisão.
- **Prevenção e resposta:** implementar primeiro o Combat Lab e o recorte definido pelo backlog; coletar observações conforme os itens 05 e 68; registrar participantes/contexto, dados, limitações e decisão de iterar/avançar. Uma amostra pequena orienta iteração e não representa validação de toda a audiência.
- **Gate relacionado:** cards 05, 12–18 e 61–68.
- **Responsável sugerido:** Design e QA/playtest.

### R-05 — Performance em mobile e metas ausentes

- **Prioridade / estado:** P1 — ABERTO; critérios e dispositivos precisam ser definidos antes de alegar aprovação técnica.
- **Risco:** Android e iOS são plataformas-alvo, mas dispositivos mínimos, metas de FPS/memória, tiers de qualidade e orçamento do slice ainda não foram aprovados. Efeitos, inimigos, arte HD e cooperação podem ultrapassar o orçamento do hardware de referência.
- **Sinal de disparo:** conteúdo ou efeito é aprovado sem orçamento; profiling acontece somente no final; alegação de desempenho não identifica dispositivo, build/configuração e cenário medidos.
- **Prevenção e resposta:** selecionar dispositivos e metas nos cards técnicos/de mobile próprios; manter o escopo do conteúdo proporcional ao experimento; medir cenas representativas e piores casos no alvo acordado; documentar build, configuração, condições e resultados; reduzir custo ou escopo caso o orçamento aprovado falhe. Até metas e evidência existirem, registrar “não medido”.
- **Gate relacionado:** cards 12–18, 68 e 92–97; metas gerais de performance permanecem abertas.
- **Responsável sugerido:** Performance Engineer e System Architect.

### R-06 — Capacidade, legibilidade e integração da arte

- **Prioridade / estado:** P1 — ABERTO; risco deve ser revisitado com os primeiros assets integrados.
- **Risco:** arte final pode competir com leitura de ação em telas menores, divergir da direção visual, depender de técnica de animação ainda não escolhida ou perder referências Unity quando arquivos/metadados forem movidos. O volume de assets também pode crescer antes que tempo/capacidade de produção sejam conhecidos.
- **Sinal de disparo:** falha em silhueta/contraste/tons de cinza na escala do jogo; referência visual sem proveniência/permissão; export de animação feito antes do card 71; `.meta` perdido; retrabalho recorrente ou backlog de assets crescendo sem estimativa.
- **Prevenção e resposta:** usar os gates e manifesto de `docs/art/ASSET_PIPELINE.md`; validar contra `docs/art/VISUAL_STYLE_GUIDE.md` e resolução/câmera do item 07; aguardar os cards 69–74 para decisões de personagem, animação e ambiente; integrar asset e `.meta` juntos no Unity; produzir o próximo lote somente após revisão do slice e da capacidade real.
- **Gate relacionado:** cards 06–07, 10, 69–74 e fundação Unity 12–18.
- **Responsável sugerido:** Design/Art e Unity Integrator.

### R-07 — Direitos e integração de áudio

- **Prioridade / estado:** P0 para áudio não liberado em distribuição; P1 para integração do sistema.
- **Risco:** composição, gravação/master, interpretação, voz, efeitos e música dinâmica podem ter titulares, permissões, custos ou escopos distintos. Uma licença limitada pode não cobrir jogo, trailer, plataformas, territórios ou exploração comercial planejados. Gatilhos dinâmicos também podem não se integrar ao combate sem retrabalho.
- **Sinal de disparo:** faixa ou voz é usada sem proveniência e escopo documental; direitos de composição/master/performance são presumidos como um único direito; integração depende de gatilhos que ainda não foram definidos/testados.
- **Prevenção e resposta:** manter ficha de origem/licença por peça e uso, com titular, autorização, canais, plataformas/territórios, prazo, créditos e limites; priorizar material original/licenciado com escopo compatível; bloquear distribuição do item não liberado. Definir transições e gatilhos nos cards 75–79 e verificar resposta ao jogo sem transformar a experiência em rhythm game.
- **Gate relacionado:** cards 51 e 75–79; gate de publicação 92–111.
- **Responsável sugerido:** Audio Lead/Design e licenciamento, com assessoria jurídica quando necessário.

### R-08 — Idade, privacidade, acessibilidade e Child Safety

- **Prioridade / estado:** P0 para distribuição de recursos que dependam de requisito não validado; P1 para decisões de arquitetura afetadas.
- **Risco:** modo infantil de 4–8 anos é previsto, mas faixas, conteúdo, classificação, dados, controles parentais, recursos de acessibilidade e modalidade de cooperação permanecem em aberto. Projetar perfis, analytics ou recursos online sem esses limites pode exigir retrabalho ou expor crianças/dados.
- **Sinal de disparo:** coleta ou transmissão de dados sem finalidade/retensão aprovadas; chat/serviço online introduzido antes de decidir coop e salvaguardas; conteúdo ou interface não revisados para idade/acessibilidade; publicação planejada antes da validação dos perfis.
- **Prevenção e resposta:** aplicar minimização de dados e evitar dados pessoais desnecessários; não introduzir chat, analytics ou coop online por suposição; mapear requisitos por perfil e plataforma; validar conteúdo, controles parentais, acessibilidade e Child Safety conforme os cards próprios antes de habilitar/releasear os recursos afetados.
- **Gate relacionado:** cards 08–09, 85–103; detalhes continuam abertos no GDD §10.
- **Responsável sugerido:** produto, QA/acessibilidade e responsável de privacidade/segurança infantil.

### R-09 — Publicação, plataformas e operação de release

- **Prioridade / estado:** P2 — ABERTO; gate obrigatório antes de qualquer lançamento público.
- **Risco:** requisitos de lojas/plataformas, classificação etária, permissões, assinaturas/builds, requisitos de privacidade e evidências de QA podem diferir entre Windows, Android e iOS e mudar ao longo do projeto. Uma build tecnicamente executável pode não estar pronta ou elegível para publicação.
- **Sinal de disparo:** submissão planejada sem checklist atual da plataforma, classificação/permissões, licença, build reproduzível e QA; publicação tratada como consequência automática de compilar.
- **Prevenção e resposta:** identificar requisitos oficiais atuais por plataforma nos cards de release; manter matriz de builds/assinaturas e evidências; testar instalação, atualização, remoção e configurações do produto nos alvos suportados; concluir revisão etária, privacidade, acessibilidade, segurança e direitos antes da submissão. Se uma regra de plataforma mudar, reabrir a avaliação afetada antes de enviar nova versão.
- **Gate relacionado:** cards 85–111; lançamento e elegibilidade ainda não avaliados.
- **Responsável sugerido:** Release/QA e responsáveis de produto, privacidade e licenciamento.

## Regras de escalonamento e aceite

1. Um risco permanece **ABERTO** até existir evidência do controle e revisão do responsável. Planejar uma ação não fecha um risco.
2. R-01, R-02, R-07 e os aspectos aplicáveis de R-08 são bloqueios de distribuição para conteúdo/recurso afetado. Créditos, atribuição, uso privado do repositório ou aprovação artística não substituem autorização/documentação necessária.
3. Escopo, fun, arte e performance permanecem gates de produção/validação; ausência de medição ou playtest é registrada como **não avaliado**, nunca como aprovado.
4. Aceitação de risco residual exige registro do impacto, build/conteúdo afetado, evidência disponível, alternativa, prazo de revisão e aprovação explícita do responsável de produto. Não se aceita como residual uma questão de direitos, privacidade ou segurança infantil sem revisão qualificada.
5. Revisar este registro quando um card fechar uma decisão, houver mudança de escopo/fornecedor/plataforma, surgir evidência de build/playtest/perfil, ou um requisito oficial de release mudar. Atualizar status e links no próprio card que produz a evidência.

## Referências e limites

- `docs/GDD_MASTER.md`, seções 2.2, 10–15: MVP, gates, idade, plataformas e riscos conhecidos.
- `docs/governance/DEFINITION_OF_DONE.md`: evidência e gate de release por card.
- `docs/art/ASSET_PIPELINE.md` e `docs/art/VISUAL_STYLE_GUIDE.md`: proveniência, fluxo e legibilidade de assets.
- Brasil, [Lei nº 9.610/1998 — Direitos Autorais](https://www.planalto.gov.br/ccivil_03/leis/l9610.htm) e [Lei nº 9.279/1996 — Propriedade Industrial](https://www.planalto.gov.br/ccivil_03/leis/l9279.htm): referências iniciais para encaminhamento; não determinam titularidade, licença aplicável, exceção, território internacional ou conclusão jurídica do projeto.
- As leis e páginas de plataforma devem ser revalidadas no momento da decisão/publicação. Este registro não substitui aconselhamento jurídico nem verificação dos requisitos oficiais de cada loja/plataforma.

