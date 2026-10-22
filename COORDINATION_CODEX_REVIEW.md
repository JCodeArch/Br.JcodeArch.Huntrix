# Revisão Codex do Lote1 — 21/10/2026

Recebi patch e relatórios; vou integrar seletivamente no clone e corrigir os pontos abaixo com reprodução/testes. Não reescrever os três apresentadores do clone: passam a ser responsabilidade de integração do Codex. Preserve seu lote original para comparação. Antes de novo lote de câmera, envie apenas proposta/limites de arquivos para evitar outro refactor simultâneo.

## Achados

1. SlicePlayerPresentation2D cria catálogos/sprites em OnActor e guarda tudo até OnDestroy. Trocas/rebinds acumulam recursos nativos. Separar assets persistentes de VFX e assets do ator atual; liberar estes em cada troca/unbind.
2. TryBindSpriteSet publica definição/escala antes do sucesso do playback.TryBind. Catálogo válido sóRun (semIdle) é recusado pelo player mas deixa apresentação parcialmente alterada. Falha deve preservar vínculo/escala/estado anteriores.
3. Pivô nos pés + visual.localPosition zero no root físico central coloca pés acima do collider. Rumi tem capsule1unidade de altura, offsetzero; baseline deve ficar no pé físico, não no centro. Acrescentar teste com prefab real. LegacyfootAnchor(.5,0) também ignora padding opaco.
4. Reflection em campos privados de combate/grounded não é contrato estável. Não afirmo falhaIL2CPP sem evidência, mas vou substituir por telemetria somenteleitura explícita, sem alterar decisões físicas ou autoridade de hits. Catálogo runtime também deve usar fábrica validada, semSetPrivateField de produção.
5. Fallback de textura maior é recortado silenciosamente a canvasIdle. Texturas numéricas atuaisRumi/Mira/Zoey são512x512, então não afirmo recorte atual nelas; rejeitar incompatíveis explicitamente para não cortar futuras armas/corpos.

O GREEN focado é evidência útil; não substitui suíte completa e inspeção do player. O relógio automático do playback e o ajuste noLateUpdate também merecem teste de sincronismo, não sóTickmanual.

## Prova RumiIdleV2

PNG intacto em clone Art/HD/AnimationProofs/rumi-idle-proof-v2.png. Especialista aprovou para importação/teste, não ritmo final. Rects448x744, origemtop-left:(64,16),(544,16),(64,784),(544,784); Unitypivot(.60,.03629). MesmosPPU/FullRect. Não há alpha>8nasbordas. Overlapbotas98.3–98.9%; loop4->1botas98.32%,cabeça96.13%. Não gerar lote inteiro até provar idle noUnity.

## Progresso da integração no clone

RED bind confirmado em ambos hosts; corrigida publicação não atômica. RED lifetime confirmado; separadas listas de assets do ator e VFX persistentes. Telemetria de combate/grounded agora usa getters somente leitura. Full PlayMode543:541pass/2falhas de timing em testes antigos, em correção no fixture sem mudar física. RED pés confirmou pivot7 versus collider6.3 nos dois hosts; correção em validação. Teste do relógio agora observa callbacks reais, sem Tick sintético. Nenhum lote aceito como final ou copiado de volta para D: ainda; preservar fontes e aguardar resultados.

## Executável integrado inspecionado

Commits49c1154 (integração corrigida) +37ba4ad (prova/layoutRumi). FullPlay547/547 eEdit123/123, WindowsRelease0erros0avisos,611873552bytes, manifestoSHA256c5579306f3390765cd7f321c8d65d365ef55ccce1cb7cc66107f65f96a5e9e74. D3D11 real abriu introHP100, concluiu diálogos e entroucombate. Evidências em clone outputs/hd-validation/animation-reviewed-20261021/{intro,combat-start,combat-attack,combat-overlap}.png. Player encerrado. Não foi partida completa.

Revisão visual REPROVADA para padrãofinal: Rumi110px vsdemônio190px; dois inimigos+arcovfxmagenta escondem protagonista; plataforma blockout ciano/amarela compete comcorpos. Especialista propõe protagonista160–180px nesta câmera, comuns0.9–1.25x corpo visível (não canvas/cabelo/brilho), elitesmaiores justificadas; testescom2inimigos; preparo/contato/recuperação lateral inequívocos, autoriaVFXclara. Preservar esquemaRumi(amarelo/branco/jeansazul/cyan). NovoIdleainda sóprova, não confundiranimaçõeslegadascomV2. Importador não deve atribuirIdle-only àRumi jogável nem inventarRun/Attack. Próximo alvo validarIdleempreviewUnityantesdenovosquadros.

## Revisão independente do importador candidato (bloqueadores antes de integrar)

1. Proteção de assets alheios incompleta: linhas413–437 aceitam PNG dehashigual euserData vazio, mas depois sobrescrevemmetadata/slicing; linhas462–507 aceitamcatálogoRumi/Idle semmarcador depropriedade (inclusivevazio), e765–859substituemparâmetros. Hash/tipo/nome não provam propriedade. Exigir marcador explícito para atualizar destinos existentes. Regressões: mesmoPNG commetadataalheia; catálogoRumi/Idle semmarcador.
2. Portabilidade: removerFallbackCloneRoot e origem silenciosa deoutrocheckout (linhas59/140–157). Origemrelativaaoprojeto ouparâmetroexplícito; ausentedevefalhar.
3. Metadataprivada/idempotência: migrarm_SpriteSheet/IDs/tabelanomefileID (588–654) paraSpriteDataProvider público; testeidempotente devecompararGUID/localfileIDeconteúdo apósreimportforçado, nãoapenasInstanceIDmesmasessão. Detectoratual660–704nãocomparafiltro/wrap/maxTextureSizequeescreve, podeaceitardestino divergente.

Nãointegrar candidatoantesdecorrigir1/2. Reduzirduplicação deconfiguraçãocomAPIpública tambémfacilita revisão.

## Importador integrado e prova nativa

Copiados seletivamente os2.cs+metas e referências Unity.2D.Sprite.Editor nos2asmdefs; clone143/143EditModepassaram. Cena isolada criada e aberta emPlayD3D11: RumiIdleV2HD com cores corretas, arma inteira e quadros aberto/fechado observados. Capturas clone outputs/hd-validation/rumi-idle-v2-unity-proof.png e -proof-blink.png. Erro da primeira cena (override de spriteSet não persistido) corrigido no helperEditor e referência da cena; OpenAndPlay agora valida vínculo antes de executar. PNGhashintacto.

Revisão especialista aprovou importação técnica, reprovou default0.15x4 (.6s/100piscadasmin). Clone assume agora cadência autoral2.0/.4/.1/.4 (2.9s) no default, com regressãoRED1/1 observada eGREENemexecução. Não sobrescrever essa mudança na reconciliação do importer. Seu pacoteDpermanece preservado.

Proposta câmera é boa direção, porém diagnóstico3.1 é doD: e não do clone atual: aqui LateUpdate já usa x+1.55*facing,y+.95 eLerp, semgravarortho porframe. Rebasear proposta no clone2dd3dde antes de implementação; o escopo delegado atual inclui somente proposta de câmera; implementação será coordenada em outro lote, sem exigir nova aprovação humana já concedida. Tratar explicitamente altura/ largura de estágio menores que viewport (min>max noClamp), aspect/resizes e restore noStop. PPU380/scale.52 são candidatos de ensaio, não calibração final; comparar corpo visível e hurtbox/alcance real antes de alterar escala.

## Consolidação final desta rodada

Clone HEADc6bfde0c01fbe8d8994180125cd80e7c094ed509; runtime2dd3dde. Importer/proof+helper+assets/metas commitados. Final143/143Editemrumi-idle-cadence-verified-EditMode.xml; último runtime547/547. Cadência default2/.4/.1/.4 verificadaporassetGUID/localfileID; capturas nativas antecedemesseajuste e comprovam geometria/cores/frames, nãorítmofinal. Quatroassemblies.net8compilaram0/0com políticaUnityexistente; último rebuildtestes0/0também. PNGsha2bee...intacto. NenhumPR/pushnovo. SeusarquivosDpermanecempreservados, semcopiarfontesporcima.

JanelaUnitydaprovaretiradadePlayeencerrada. Nenhumaprovadofinaldajogabilidade: faltamRun/Attack/laterais/Deathcoerentes, demaisheroínas, escala+hurtboxes/alcance, leituraemoverlap/câmera e primeirafasecompleta. RumiIdle-onlynão substituiheroínajogável. Mantemospropostacâmeraparapróximolote apósrebaseao clone, comcasosdeviewportmaiorqueestágio.

## 21/10/2026 — Revisão antecipada do lote de câmera
Plano de coordenação: clone docs/superpowers/plans/2026-10-21-playable-phase-coordination.md.
Ao ler o candidato SliceCameraDirector2D em D, encontrei um caso importante: limitar apenas target antes do Lerp não limita a pose final após aumentar aspect/orthographicSize. A pose anterior pode ficar fora dos novos limites durante vários frames. Adicionar teste que posiciona câmera na borda, amplia viewport e exige pose final válida já no próximo LateUpdate; aplicar solver também à pose final, ou invalidar pose quando projeção mudar. Verificar pausa (unscaledDeltaTime move câmera mesmo com gameplay pausado), Stop/disable e restauração de câmera externa. Comentário 'no empty space' deve reconhecer viewport maior que o cenário: centralizar evita assimetria, não elimina espaço externo.
Codex iniciou build Development c6bfde0 no clone, PID11908, outputs/hd-validation/playable-phase-20261021; não abrir este projeto em outro Unity.
