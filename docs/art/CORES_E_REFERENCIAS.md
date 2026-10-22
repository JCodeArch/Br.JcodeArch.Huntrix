# Cores e referências — decisões do proprietário em 20/10/2026

## Fonte principal

`referencia-cores-proprietario.png` fornecida pelo usuário. A leitura de cores/figurino dessa imagem prevalece sobre as primeiras pranchas geradas. A adaptação continua desenhada em 2D HD; a referência não muda o projeto para personagens 3D.

| Identidade | Blocos de cor e figurino |
|---|---|
| Rumi | Trança violeta; jaqueta amarela viva; top branco; short jeans azul escuro; botas pretas com vermelho; lâmina azul/ciano brilhante com núcleo claro |
| Mira | Cabelo rosa avermelhado; camiseta cropped preta; saia laranja/amarela com painel lateral e correntes; botas altas pretas com detalhes vermelhos; arma longa com luz azul clara |
| Zoey | Cabelo preto em dois coques; top verde-petróleo; calça escura com desenhos/painéis amarelo-dourados; tênis amarelos; lâminas luminosas azuladas |

Os blocos precisam continuar reconhecíveis sob iluminação do cenário. Evitar bloom e gradação tão fortes que transformem amarelo em branco ou roupa inteira em violeta. Conferir nas três heroínas em movimento e em fundo claro/escuro.

## Segunda referência

`referencia-demonios-cenario-proprietario.png`: demônios de portes e silhuetas variados, figuras de chapéu coreano e robes, brutos magenta/violeta, criaturas menores azul/verde, cidade e montanhas coreanas em camadas, Honmoon azul/ciano e atmosfera de palco. Figurinos escuros de palco da imagem ficam registrados como variantes; não substituir a roupa principal de cada estado de animação aleatoriamente.

## Arte gerada e estado

- `hunters-capitao-hd-v3-paleta-referencia.png`: nova referência de cores do elenco, gerada com image_gen integrado usando a primeira imagem do proprietário. Ainda possui efeitos incorporados; não é atlas recortado nem animação pronta.
- `../assets-hd-production/candidates/demonios-hd-v1.png`: quatro propostas de identidade para WingedJeoseung, CabinBrute, SmokeImp e TalchumPhantom. Todos foram desenhados olhando à direita apesar do pedido de orientação esquerda; registrar/ajustar a convenção no catálogo antes de integrar. Não sobrescrever sprites existentes diretamente.
- `../assets-hd-production/candidates/rumi-run-v1.png` e `rumi-run-v2.png`: não aprovados. Repetição de passada, bordas e roupa anterior incompatível com a referência corrigida. Não integrar.

Prompts usados: corrigir somente figurinos e blocos de cor da prancha 2D para a referência fornecida, preservando identidades e poses; gerar quatro demônios separados em estilo HD consistente, com silhuetas de chapéu/robe, bruto, imp e máscara, transparência e sem cenário. Originais gerados preservados; nenhuma compra nem fallback de API paga utilizado.

## Aplicação da referência de demônios e jogo

- Separar as famílias pela silhueta antes dos detalhes: figura alta de chapéu e robe, bruto curvado com braços largos, criatura pequena e ágil, criatura aérea. Cada família deve permitir reconhecer sua função durante o combate.
- Preservar pele magenta/violeta nos brutos e azul/verde nas criaturas menores. Chapéus e robes escuros precisam de separação de valor contra os fundos noturnos.
- Cenários urbanos em camadas: montanhas e céu ao fundo, edifícios na distância, telhados/plataformas no plano jogável e detalhes discretos em primeiro plano. A referência é composição de ambiente; não usar o pôster inteiro como fundo repetido.
- Reservar ciano intenso para armas e energia Honmoon; violeta e magenta compõem a ameaça e a atmosfera. Sinais de ataque devem continuar legíveis sem depender apenas da cor.
- Validar os sprites no tamanho real de jogo, contra fundos claros e escuros, com efeitos ligados e desligados. Ilustração bonita não comprova animação, colisão ou leitura em combate.

## Matrizes cel 2D revisadas

As bases estáticas de Rumi, Mira e Zoey estão em `Art/HD/Masters`, com README, QA e manifest. Preservam as cores definidas aqui; Mira usa lâmina curva azul/ciano após correção dirigida. As matrizes são fontes de produção, não animações nem sprites já integrados. Prompts e procedência estão em `Art/HD/PROMPTS_*_CEL_20261020.md`.
