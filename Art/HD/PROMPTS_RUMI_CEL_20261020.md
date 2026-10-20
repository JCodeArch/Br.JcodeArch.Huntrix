# Rumi — matriz cel 2D, 20/10/2026

Ferramenta: image_gen integrada, transparent_background=true. Sem CLI/API alternativa. PNGs originais preservados; nenhuma edição por Python ou outra ferramenta foi feita. Arquivos em candidates, fora dos assets runtime.

## Geração

Referência: outputs/assets-hd-preview/referencia-cores-proprietario.png, somente identidade, roupa e cores da personagem central.

Saída: candidates/rumi-master-v2-cel-alpha-pendente.png. Original exec-b809e661-93df-428b-8409-c30e5bcf0dce.png.

Prompt:

Create ONE production character master for a high-resolution hand-drawn 2D side-scrolling action game. Reference image is ONLY the identity/costume/color reference: isolate Rumi, the purple-braided woman in the CENTER. Draw her as crisp flat 2D cel animation, clean confident ink contours, solid opaque color interiors, only two shadow tones, simplified details legible at 200px character height. Full adult-proportioned figure in a relaxed combat-ready idle stance, three-quarter SIDE VIEW facing RIGHT, two planted feet, sword held down-forward in right hand, confident focused face. Keep her thick long purple braid, cropped vivid YELLOW jacket, WHITE fitted top, dark BLUE DENIM SHORTS with readable blue denim, black knee-high boots with RED vertical accents. Her straight sword blade is BRIGHT BLUE/CYAN with pale icy-blue core; blade is solid drawn geometry, no glowing atmosphere. Preserve these exact costume color blocks. Single coherent drawing, no extra figure or alternate pose. Entire braid, crown, boots, fingers, sword tip and tassel fully visible with generous clear padding: character and sword together occupy at most 78 percent of the canvas, baseline near 87 percent height. Asset output is a truly transparent cutout: all empty pixels fully transparent, interior character opaque, crisp tightly antialiased silhouette. NO shadow under feet, NO colored haze, NO particle specks, NO bloom fringe, NO painted checkerboard, NO background, NO labels, NO frame grid, NO 3D render, NO soft airbrush. This is a clean animation source master; lighting and sword glow will be separate Unity effects.

## Iteração de extração

Alvo: saída anterior, única mudança pedida: fundo/transparência.

Saída: candidates/rumi-master-v3-extracao-alpha-pendente.png. Original exec-b81301b5-4cc7-4f2f-a7f8-585ee3c19b21.png.

Prompt:

Background extraction only. Edit target is the attached Rumi drawing. Preserve the character exactly: same face, hair, line art, skin, yellow jacket, white top, dark blue denim shorts, black and red boots, sword, pose and proportions. REMOVE ALL BLACK BACKGROUND AND ALL COLORED HAZE outside the actual character and solid sword silhouette. Also remove background between legs, arms, braid segments and sword. Output a genuinely transparent PNG cutout: exterior alpha must be zero, opaque character interiors, only a tight antialias edge. NO purple aura, NO gold aura, NO white sword halo, NO cast shadow, NO new background, NO replacement illustration, NO style change. The blue sword blade itself stays fully visible and solid. This is a technical clean cutout for compositing in Unity, not an illustration with decorative glow.

## Verificação inicial

Ambos RGBA1024x1536. V3 possui1.140.025 pixels com alpha0 entre1.572.864; modos de interior253/254. Sondas externas(0,0),(1023,0),(0,1535),(1023,1535),(900,200),(50,1000),(800,600) têm alpha0. A visualização RGB preta/colorida não prova, por si só, halo opaco. Revisão especialista solicitada para analisar alpha e margens; ainda sem ciclo idle/run e sem aprovação final para integração.

Prévia comparativa HTML original preservada em outputs/rumi-alpha-preview/index.html. Servidor de prévia bloqueado pela revisão automática (retorno apenas blocked by policy). Browser Use também bloqueou file:// por política de protocolos. Não houve tentativa de contornar essas políticas; nenhum servidor foi iniciado e nenhum PNG foi transmitido a host externo.
