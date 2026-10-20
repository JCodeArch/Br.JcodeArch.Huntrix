# Pipeline de apresentação HD — Bloom

O `StageLightingRig2D` configura apenas pós-processamento nesta etapa; luzes locais, normais e sombras ainda não estão implementadas nele.

A sessão cria um rig no slot e o liga à câmera resolvida, cobrindo câmera externa e fallback. O perfil compartilhado `Assets/Resources/Presentation/HuntrxBloom.asset` é carregado como asset, sem clonagem/mutação. Um Volume global filho do rig, peso1/prioridade10, usa a camada30 reservada para volumes de apresentação. A câmera preserva suas camadas anteriores e inclui essa camada. Não colocar outra câmera nessa máscara sem definir a composição desejada.

O Stop da sessão desativa o slot antes de destruir objetos. O rig desativa seu Volume imediatamente e restaura HDR, pós-processamento e máscara quando esses valores ainda correspondem aos aplicados por ele. Dados adicionais criados na câmera externa permanecem no componente, com propriedades restauradas; nenhum Volume antigo deve ficar ativo após reinício.

O stripping da URP permanece habilitado. A URP instalada examina os VolumeProfiles em Assets e preserva Bloom quando sua intensidade é positiva. O perfil padrão da Unity permanece inalterado. Avisos de DepthOfField/MotionBlur não usados devem ser avaliados separadamente de Bloom.

Primeiro ajuste para inspeção: intensidade0,35/threshold0,9, filtro de baixo custo, sem dirt. Ruling: threshold inicialmente abaixo do ponto1,1 sugerido no plano permite inspecionar contribuição nos sprites legados sem emissão HDR. Isso não é aprovação estética: pode iluminar partes claras demais. Após materiais emissivos, rever threshold1,1/intensidade0,25 e comparar armas/personagens/fundo para preservar as cores do proprietário.

Contrato atual: um rig por câmera de gameplay. Não há coordenação de múltiplos rigs simultâneos na mesma câmera. Não declarar iluminação/sombras completas nem desempenho mobile com base neste pipeline.
