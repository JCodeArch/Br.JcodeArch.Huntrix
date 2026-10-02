# Resolução e câmera — HUNTR/X: Honmoon

**Card de origem:** [07 — Definir resolução e câmera](https://trello.com/c/Xw7OlxdN/7-07-definir-resolu%C3%A7%C3%A3o-e-c%C3%A2mera)  
**Estado:** contrato-base de enquadramento e comportamento definido; validação visual em build continua necessária.  
**Escopo:** jogo 2D de ação/plataforma para Windows, Android e iOS. Define apresentação e escala de câmera, não metas de desempenho, HUD, controles ou comportamento de rede.

## Decisões

| Tema | Decisão |
|---|---|
| Orientação inicial | Paisagem nas plataformas mobile; no Windows, janela e tela cheia em paisagem. Retrato fica fora do envelope inicial de suporte. |
| Quadro de composição | Referência 16:9, 1920 × 1080. É referência para composição e escala, não uma resolução de renderização forçada. |
| Projeção | Câmera 2D ortográfica, sem perspectiva e sem rotação. |
| Escala inicial de autoria | 100 pixels por unidade de mundo (PPU). Em 16:9, altura visível de 10,8 unidades, equivalente a `orthographicSize = 5,4`; a largura decorre da proporção de tela. |
| Renderização | Usar a resolução de saída suportada pelo dispositivo/janela e escalar a apresentação sem esticar a imagem. Qualidade e render scale ficam para os cards de plataforma/performance. |
| Pixel art | O projeto segue o estilo HD 2D estilizado/cel painting do item 06. Pixel Perfect Camera não é requisito da direção visual; snapping/pixel-perfect não será ativado por padrão. |
| Faixa inicial de proporções | Paisagem entre 4:3 e 21:9; 16:9 é a referência. Fora da faixa, preservar proporção sem esticar e tratar como apresentação fora do suporte inicial. |

`orthographicSize` define metade da altura visível; a largura acompanha o aspect ratio. Com a escala inicial, a altura de mundo permanece 10,8 unidades e a largura muda com a tela: 14,4 unidades em 4:3, 19,2 em 16:9 e 25,2 em 21:9. A orientação paisagem e o enquadramento lateral sustentam a leitura do jogo de ação/plataforma 2D.

## Composição e zonas seguras

- O retângulo central de 4:3 na altura de referência é a **zona de ação segura**. Personagens ativas, ameaças, telegráficos, objetivos e evidências necessárias para concluir uma tarefa devem permanecer compreensíveis dentro dela.
- Proporções mais largas revelam espaço lateral adicional. Esse espaço pode ampliar ambiente e antecipação visual; não deve revelar um objetivo obrigatório ou uma ameaça essencial antes do ponto de entrada previsto no encontro.
- A autoria de fase preserva margem para animações, saltos, dash e combate terrestre/aéreo. A câmera não corta personagem, objetivo ou telegráfico ativo para manter composição decorativa.
- A proporção mais estreita não reduz a altura nem estica a imagem. Se um encontro exigir mais largura que a zona segura, ajustar sua composição ou revisar explicitamente o contrato antes da produção; não esconder mecânicas nas bordas.
- Interface e recortes físicos de tela usam as safe areas do dispositivo; o layout do HUD permanece nos cards de UI/acessibilidade.

## Comportamento da câmera

1. **Câmera compartilhada:** manter uma única vista para os personagens ativos na sessão. Isso acomoda solo e a exigência de até três personagens simultâneos sem decidir se a cooperação será local ou online. Split-screen não é assumido por este card.
2. **Alvo:** em solo, acompanhar a personagem controlada. Com mais de uma personagem ativa, acompanhar o centro do retângulo que contém o grupo; não favorecer uma personagem a ponto de ocultar outra participante.
3. **Acompanhamento:** usar zona morta pequena, amortecimento suave e antecipação horizontal moderada na direção do movimento. O enquadramento vertical responde ao deslocamento do grupo em saltos e combate aéreo sem movimentos bruscos.
4. **Limites de fase:** restringir a câmera aos limites de cada trecho jogável. Em transição narrativa ou set-piece, permitir mudança de composição com blend; devolver o acompanhamento normal ao concluir o evento.
5. **Leitura de combate:** manter personagens ativas e ataques telegrafados no quadro seguro. O zoom-base não pulsa a cada ação; qualquer ajuste de zoom para manter o grupo enquadrado preserva a leitura da personagem e dos sinais de perigo.
6. **Sem teleporte visual:** reposicionamentos, respawn e trocas de foco usam transição deliberada ou reposicionamento oculto por uma transição prevista pela fase. Não deslocar bruscamente a câmera durante controle normal.

Os valores de zona morta, amortecimento, antecipação, limites de zoom e transições serão afinados no protótipo/Combat Lab. A viabilidade de manter até três participantes no quadro seguro, inclusive em trechos verticais, é gate de validação antes de declarar esse modo demonstrado.

## Matriz futura de validação

Verificar em 4:3, 16:9 e 21:9, com resoluções de saída baixas e altas disponíveis nos dispositivos de teste. Em cada combinação, revisar:

- personagem e ponto de ação visíveis ao iniciar, correr, saltar, usar dash e combater no ar;
- limites de fase e transições sem revelar área indevida ou cortar gameplay;
- leitura de objetivo, ameaça e telegráfico dentro da zona segura;
- acompanhamento solo e do grupo com até três personagens quando o modo existir;
- ausência de deformação, barras inesperadas dentro da faixa suportada, clipping e oscilações visíveis;
- contraste e silhueta conforme [`docs/art/VISUAL_STYLE_GUIDE.md`](../art/VISUAL_STYLE_GUIDE.md).

Estes são critérios futuros de QA, não resultados já medidos. Itens 12–18 configuram projeto e Combat Lab; metas de resolução/qualidade por dispositivo permanecem nos cards de mobile/performance e release.

## Base técnica e referências

Unity documenta `orthographicSize` como metade da altura do volume visível da câmera ortográfica; a largura deriva desse valor e do aspect ratio. A documentação de Pixel Perfect Camera descreve a preservação do visual de pixel art em resoluções diferentes. Como a direção aprovada para HUNTR/X é HD estilizada, o componente não é obrigatório.

- [Unity 6 — Camera.orthographicSize](https://docs.unity3d.com/6000.0/ScriptReference/Camera-orthographicSize.html)
- [Unity 6 — Pixel Perfect Camera (URP)](https://docs.unity3d.com/6000.0/Manual/urp/2d-pixelperfect-ref.html)
- [Card 06 — direção visual 2D](https://trello.com/c/NQl7W8MA/6-06-definir-estilo-visual-2d)

