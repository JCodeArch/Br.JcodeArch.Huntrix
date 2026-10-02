# Direção visual — HUNTR/X: Honmoon

**Card de origem:** [06 — Definir estilo visual 2D](https://trello.com/c/NQl7W8MA/6-06-definir-estilo-visual-2d)  
**Estado:** direção 2D HD estilizada e regras-base de consistência definidas.  
**Escopo:** linguagem visual e critérios de revisão; sem criar assets ou fixar resolução, câmera, proporções de personagens ou pipeline de animação.

## Direção aprovada

HUNTR/X — Honmoon usa **ilustração 2D HD estilizada, com acabamento gráfico de cel painting e composição cinematográfica**. Personagens e elementos jogáveis combinam silhuetas expressivas, formas simplificadas, contornos controlados, planos claros de luz/sombra e acentos de cor. Cenários podem usar pintura e atmosfera em camadas, mantendo a ação e os objetivos imediatamente legíveis.

A prioridade visual segue esta ordem: leitura da ação e do perigo; identidade da personagem ou elemento; impacto emocional/espetáculo; detalhe decorativo. A imagem deve sustentar ação rápida e ser entendida em telas mobile sem depender de detalhes minúsculos.

## Regras de consistência

1. **Silhueta primeiro:** cada personagem, ameaça e objetivo importante deve ser distinguível pela forma externa e pose antes de depender de cor ou detalhe interno.
2. **Hierarquia de traço:** contornos externos e linhas internas seguem uma hierarquia estável por cena e escala; detalhes internos ficam subordinados à forma principal. A espessura final será validada após a resolução e a câmera do item 07.
3. **Luz e volume:** agrupar sombra e luz em planos legíveis; reservar gradações, textura e brilho para volumes, materiais ou momentos de espetáculo que ganhem clareza com eles. Manter uma fonte de luz coerente dentro de cada plano/cena.
4. **Cor funcional:** cada cena define papéis de cor para personagens jogáveis, aliados/objetivos, ameaças, perigos e feedback. Contraste de valor e forma acompanha a diferença cromática; informação essencial nunca depende apenas de distinguir matizes.
5. **Separação de planos:** personagens e perigos que afetam o gameplay mantêm contraste suficiente contra o fundo. Cenários usam profundidade e atmosfera sem competir com silhuetas, sinais de ataque ou objetivos. Usar enquadramento e zona segura definidos em `docs/architecture/RESOLUTION_AND_CAMERA.md`.
6. **Efeitos com leitura temporal:** movimento, preparação, impacto e resultado usam formas visuais coerentes. Efeitos apoiam a ação sem cobrir personagens, ameaças ou sinais antecipados de ataque por tempo maior que o necessário à leitura.
7. **Detalhe compatível com escala:** texturas e ornamentos preservam a hierarquia de leitura quando a arte é vista no enquadramento real do jogo. O item 07 decide escala, resolução lógica e câmera antes do acabamento final dos assets.
8. **Variação com unidade:** personagens e ambientes podem ter paletas, materiais e motivos próprios, mantendo o mesmo tratamento de contorno, luz/sombra, nível de detalhe e contraste estabelecido para a cena.

## Revisão de consistência por asset

Antes de aprovar um asset para integração, verificar:

- a silhueta comunica função e ação em pose estática e na menor escala de gameplay prevista;
- personagem, aliado, ameaça, objetivo e perigo mantêm leitura por forma e contraste, inclusive em tons de cinza;
- o tratamento de contorno, luz/sombra e textura corresponde à direção da cena;
- o contraste do fundo deixa livres os elementos jogáveis e os sinais de perigo;
- movimento e efeitos preservam o ponto de ação e a resposta que o jogador precisa perceber;
- referências visuais têm origem registrada e uso permitido para a finalidade pretendida.

Se um teste falhar, ajustar primeiro silhueta, pose, contraste e hierarquia; adicionar detalhe não substitui correção de leitura. Revisões entre assets relacionados devem ocorrer lado a lado e na escala de gameplay.

## Referências, direitos e proveniência

Usar referências para estudar princípios gerais de composição, forma, cor ou acabamento. Registrar origem e licença/condição de uso de cada referência incorporada ao trabalho. Personagens, figurinos, marcas, cenários, símbolos e outros elementos reconhecíveis de obras de terceiros só podem ser usados após confirmação das permissões necessárias; a direção deste documento, por si só, não concede direitos. O risco de licenciamento permanece aberto no GDD.

## Limites e próximos cards

- A **direção geral HD 2D estilizada** e as regras acima estão fechadas para orientar pré-produção.
- Resolução de referência, escala de autoria, proporções de tela e comportamento-base da câmera foram definidos no item 07 e em `docs/architecture/RESOLUTION_AND_CAMERA.md`; o tuning visual aguarda o protótipo.
- Identidade, cores específicas, figurinos, proporções, expressões e referências de Rumi, Mira, Zoey e antagonistas pertencem ao Character Bible e model sheets (itens 69–70).
- A escolha frame-by-frame, rig ou híbrida pertence ao item 71. Ambientes e composição de cada cenário pertencem ao item 74.
- Paletas finais por personagem/cena e parâmetros de produção podem ser refinados nos cards de arte correspondentes, sem contrariar as regras de leitura e consistência deste guia.

## Registro de revisão

Mudanças futuras devem apontar a cena/assets afetados, referência visual usada, motivo da mudança e confirmação de que os critérios de legibilidade e direitos continuam atendidos.

