# Rumi idle — correção dirigida v2

Ferramenta: image_gen integrada; transparent_background=true; nenhuma edição por código.
Referência/alvo: rumi-idle-proof-v1.png.
Saída original: exec-fea66fc7-54b6-4827-904e-c146e797f8c7.png.
SHA256: 2bee6f1261a993e6d17a1eacd9e1df3c6b703c479d5c4cec89137c3ef73cf8eb.
Status: importação técnica aprovada e observada em Play Mode no Unity; qualidade final de movimento e poses laterais pendentes.

## Motivo

V1 reprovada pelo especialista: a espada cruza x512 (13pixels alpha>128 emx511 e12emx512), corta quadros1/3 e contamina2/4 no slicing512x768. Pés inferiores deslocados6px e registro horizontal variável. Alpha real foi examinado; não foi inferência a partir de hazeRGB. Cores e variação de respiração/trança/piscada aproveitáveis.

## Prompt enviado

Precise-object-edit of the attached four-frame Rumi idle sprite sheet. Preserve the four existing drawings, character identity, cel style, exact colors, costume, proportions, full straight cyan sword length, subtle breathing, braid movement and blink in frame3. Correct ONLY sprite registration and transparent padding. Keep a strict 2x2 equal-cell grid on a portrait transparent sheet; NO background, text, grid lines or borders. All four sprites must have EXACTLY the same fixed boot positions relative to their cell. Uniformly reduce the entire character AND sword together in ALL four cells to approximately85% of their current size, with the same scaling for every frame, so ALL visible pixels including blade and braid fit comfortably INSIDE EACH cell. No opaque pixels may touch any cell boundary or cross into a neighbor. Align the midpoint between the boots to the horizontal CENTER of every cell; align the lowest boot sole to the SAME horizontal floor line in each cell at approximately91% of cell height. The top of the head should be approximately8% of cell height, leaving at least5% transparent padding all around the full silhouette including sword tip. Using a 1024x1536 output: eachcell512x768, fixed boot-midpoint x256 and solebaseline y704 LOCAL to eachcell; headtop about64 LOCAL. Global top-row baseline704, bottom-row1472. Keep legs/boots/pelvis completely registered; breathing moves only chest/shoulders and braid subtly, not whole-body translation. Keep all frames the same size and orientation. Fix the current diagonal sword tip that crosses the central column: move/scale the whole sprite, NEVER shorten or cut the sword. Frame4 must return smoothly to frame1. Genuine transparent alpha, no haze or ground shadow. This is a production slicing correction, not a redesign.

## Verificação pedida ao especialista

Medir alpha, pés e arma. Se a grade fixa continuar inadequada, avaliar retângulos de slicing no Unity com tamanho e pivô comuns, mas origens calibradas por quadro, sem cortar pixels ou capturar o vizinho. Isso é metadado do motor; PNG permanece intacto. Não aprovar por aparência estática nem fingir loop validado.

## Resultado da revisão independente

RGBA 1024×1536. Quatro retângulos uniformes 448×744 com origens calibradas, registrados em `rumi-idle-proof-v2.layout.json`; nenhum pixel alpha>8 nas bordas. Espada preservada sem captura significativa da figura vizinha. Sobreposição das botas entre quadros consecutivos: 98,3–98,9%; no fechamento 4→1: botas 98,32%, cabeça 96,13%. Margens mínimas: 9px à direita da espada, 24px acima da cabeça e 26px abaixo dos pés.

Aprovação restrita ao recorte e registro geométrico. Ritmo, leitura em movimento e integração com o chão ainda precisam ser vistos no Unity. O PNG permanece intacto.

## Prova no Unity — 21/10/2026

PNG copiado intacto para Assets/Art/HD/Rumi/IdleProofV2 e catálogo Idle-only importado via Sprite Data Provider público. Seis testes do importador passaram no D: e no clone; a suíte EditMode completa do clone passou143/143 antes do ajuste de cadência.

Preview real em Unity D3D11 confirmou o figurino, a espada completa, transparência e quadros com olhos abertos/fechados. A primeira cena de prova carregou o legado por uma referência de prefab não persistida; o vínculo foi corrigido e OpenAndPlay agora o valida antes de executar. Capturas nativas em outputs/hd-validation/rumi-idle-v2-unity-proof.png e -proof-blink.png.

Especialista aprovou a importação técnica, mas reprovou o loop inicial0,6s: a piscada se repetia100vezes/minuto. Cadência provisória adotada:2,0/0,4/0,10/0,4s (ciclo2,9s). Continua sendo prova de quatro quadros, sem aprovação de fluidez final ou combate lateral. Não atribuir este catálogo apenas Idle à heroína jogável como se Run/Attack/Death estivessem prontos.
