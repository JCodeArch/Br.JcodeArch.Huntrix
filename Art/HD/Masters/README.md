# Matrizes estáticas do trio — cel 2D HD

As três imagens foram revisadas por especialista de sprites em20/10/2026 e aprovadas como bases estáticas de produção. Os PNGs originais foram preservados sem edição posterior. O manifest registra seus hashes.

| Personagem | Arquivo | Identidade visual preservada |
|---|---|---|
| Rumi | rumi-master-cel-v1.png | Trança violeta, jaqueta amarela, top branco, short jeans azul, botas pretas/vermelhas, espada azul/ciano |
| Mira | mira-master-cel-v1.png | Cabelo rosa avermelhado, cropped preto, saia laranja/amarela, botas pretas/vermelhas, arma longa com lâmina curva azul/ciano |
| Zoey | zoey-master-cel-v1.png | Dois coques pretos, top verde-petróleo, calça escura com desenhos amarelos, tênis amarelos, lâminas pequenas azul/ciano |

**Ainda não são animações nem sprites integrados ao jogo.** Não duplicar estas poses para representar corrida. Derivar vistas funcionais, apoios, quadros articulados e trajetórias de arma; conferir a leitura no tamanho real antes de produzir os ciclos completos.

Rumi e Mira têm pouco espaço superior/lateral para movimentos de cabelo e arma. O canvas comum de animação precisa acomodar esses movimentos. A pose em três quartos é uma matriz de identidade, não uma vista lateral final de combate.

O RGB colorido em pixels de alpha0 não representa fundo renderizado. A revisão examinou o canal alpha real; não classificou a prévia RGB preta/colorida como halo opaco. Interiores predominam em alpha253–254, próximos de99% de opacidade.

Nenhuma imagem foi colocada em Assets/Resources. A integração depende dos testes de recorte, escala, pivô, filtragem e animação. Estes arquivos em Art são fontes de produção.
