# Pipeline de assets — HUNTR/X: Honmoon

**Card de origem:** [10 — Definir pipeline de assets](https://trello.com/c/QCjoqgpG/10-10-definir-pipeline-de-assets)  
**Estado:** fluxo de produção, integração e validação definido em nível de processo.  
**Escopo:** handoffs e critérios de aprovação; não produz assets nem escolhe técnica de animação, especificações finais de arte ou configurações de importação.

## Objetivo e princípios

Todo asset precisa ter finalidade de jogo identificável, origem rastreável e aprovação visual antes de ser integrado. A validação adequada ao uso é necessária antes de ser considerado **validado**. O fluxo protege a legibilidade definida em [`VISUAL_STYLE_GUIDE.md`](VISUAL_STYLE_GUIDE.md), explicita direitos e dependências e evita que arquivos de origem sejam confundidos com conteúdo aprovado para uso.

O card 10 fecha **a sequência de trabalho**, não decisões específicas de personagem ou de runtime. As decisões de identidade e model sheet continuam nos itens 69–70; a escolha frame-by-frame, rig ou híbrida por personagem cabe ao item 71; composição dos ambientes cabe ao item 74. Pastas, pacotes, presets e regras automatizadas de importação dependem da fundação Unity e de cards técnicos próprios.

## Estados do asset

Cada entrega avança por estados explícitos no registro do asset. Reprovação ou alteração material devolve a entrega ao estado anterior relevante, preservando histórico e motivo.

| Estado | Significado | Critério de saída |
|---|---|---|
| **Brief** | Função, contexto de uso e dependências estão descritos. | Brief rastreável e riscos de origem/licença identificados. |
| **Conceito** | Silhueta, composição ou abordagem visual está sendo explorada. | Revisão de direção visual aprova uma proposta para acabamento. |
| **Arte final** | A versão visual foi finalizada para o uso descrito. | Revisão visual e proveniência aprovadas; export/handoff identificado. |
| **Handoff** | Fontes, exports e metadados necessários foram entregues. | Responsável de integração confirma que recebeu versões e dependências correspondentes. |
| **Integrado** | O asset foi importado e referenciado no projeto Unity. | Importação revisada no Editor, referências e metadados preservados. |
| **Validado** | O uso previsto passou pela revisão de qualidade do card consumidor. | Evidências, limitações e aprovação registradas no card relacionado. |

## Fluxo e gates

### 1. Brief e conceito

Antes de produzir, registrar no manifesto: identificador/nome provisório; função jogável ou narrativa; beat/cena e card consumidor; responsável; referências e proveniência; status de permissão/licença; dependências; escala de visualização pretendida e critérios de aprovação. Distinguir referência de inspiração, material de origem e conteúdo autorizado para incorporação.

Design/arte confere o brief contra o GDD, o estilo visual e as decisões já aprovadas. Ambiguidades sobre identidade, uniforme, composição ou animação são encaminhadas aos cards 69–74, sem resolvê-las por inferência neste pipeline. Nenhum material reconhecível de terceiro avança como conteúdo de distribuição sem a permissão necessária registrada.

### 2. Arte final e revisão visual

A proposta aprovada recebe arte final coerente com a finalidade, a pose/estado e a escala de uso. O handoff inclui os arquivos de trabalho necessários para futuras revisões, os exports para integração e a versão correspondente do manifesto. O formato e a organização dos arquivos são definidos junto do uso e da fundação Unity; este card não impõe formato universal.

A revisão compara assets relacionados lado a lado e na escala real de gameplay definida para o card consumidor. Aplica os testes do guia visual: leitura de silhueta, contraste e distinção em tons de cinza, hierarquia de detalhe, separação contra o cenário, consistência de luz/traço e efeitos que não encobrem personagens ou sinais de perigo. O parecer identifica aprovado, mudanças necessárias ou bloqueio por direitos/proveniência.

### 3. Handoff de sprite/rig/animação

Arte final estática pode avançar após sua revisão visual. O pacote de arte informa estados, poses, pivô/âncora quando aplicável, ordem/identificação de partes e dependências necessárias à integração. O método de animação **não** é decidido aqui: o card 71 escolherá frame-by-frame, rig ou híbrido por personagem. Exports e requisitos específicos de animação (por exemplo, spritesheet, rig, clips ou pivôs de animação) aguardam essa decisão; até lá, a entrega registra o que contém e o que ainda depende do card 71, sem presumir uma estrutura ou runtime.

### 4. Integração e importação no Unity

O integrador importa a versão aprovada no projeto Unity e confere o resultado no Editor. O local definitivo no projeto, convenções de pasta, formatos, import settings, presets e automações serão estabelecidos pelos cards de arquitetura/configuração que deles dependam; não ficam travados pelo card 10.

O projeto deve versionar o arquivo de asset junto de seus metadados Unity associados. A documentação oficial descreve que o Unity usa arquivos `.meta` para IDs e configurações de importação; mover ou renomear assets fora do Editor sem preservar/atualizar o `.meta` pode quebrar referências. Assim, movimentações são feitas pelo Editor ou mantêm o pareamento asset/metadado, e ambos são revisados no diff antes do commit. A pasta gerada `Library` não integra a entrega versionada.

### 5. Validação e retorno

No contexto de uso aprovado, revisar: aparência importada; enquadramento e escala conforme o card 07; referências e dependências resolvidas; leitura em movimento/estado quando houver animação disponível; contraste e hierarquia sem obstruir feedback de gameplay; integridade de metadados; origem e permissões; e requisitos aplicáveis de idade, privacidade e acessibilidade do GDD §10 e dos cards 85–91. Para referências com pessoas ou material de terceiros, conferir também proveniência e permissão antes de incorporá-las. QA escolhe a evidência proporcional ao asset e ao card consumidor: inspeção documental/editorial para conteúdo estático e verificação executável no alvo definido quando o comportamento depender de runtime.

O card consumidor registra dispositivo/build/contexto efetivamente verificado, achados, limitações e aprovação. Uma inspeção no Editor não é relatada como teste em dispositivo ou como validação de runtime. Falha de leitura retorna para revisão de arte; falha de import/referência volta à integração; dúvida de direitos bloqueia distribuição e é encaminhada ao plano de riscos do item 11.

## Registro mínimo por entrega

Manter uma entrada por asset ou conjunto coeso, ligada ao card que o produz e ao card que o consome:

- identificador e versão do asset;
- finalidade e cena/beat de uso;
- responsável por brief/arte e por integração;
- links/arquivos de origem, exports e dependências;
- referência/proveniência e condição de permissão/licença;
- decisões upstream de que depende (por exemplo, 69–71 ou 74);
- estado atual e pareceres/revisões com data;
- evidência de importação/validação e limitações;
- destino do conteúdo integrado e commit que inclui arquivos e metadados.

## Gates que continuam abertos

- Conteúdo autorizado, identidade visual específica e model sheets: itens 69–70 e validação de direitos.
- Técnica de animação e requisitos por personagem: item 71.
- Composição de cada ambiente: item 74.
- Estrutura Unity, diretórios, pacotes e política concreta de importação: itens técnicos de fundação/configuração; não fixados aqui.
- Alvos, dispositivos, orçamento de memória/desempenho e critérios de build: cards de plataforma/QA correspondentes; nenhuma medição é afirmada por este documento.
- Licenciamento e risco de publicação: item 11 e gate de release.

## Referência técnica

- Unity 6 Manual (6000.0), [Asset metadata](https://docs.unity3d.com/6000.0/Documentation/Manual/AssetMetadata.html): IDs, configurações de importação, arquivos `.meta` e pareamento ao mover/renomear assets.
- Unity 6 Manual (6000.0), [Asset workflow](https://docs.unity3d.com/6000.0/Documentation/Manual/AssetWorkflow.html): importação e relação entre arquivos de origem e versões importadas.


