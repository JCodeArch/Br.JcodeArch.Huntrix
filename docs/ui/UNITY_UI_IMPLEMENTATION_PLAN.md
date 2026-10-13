# Contrato de integração das telas Figma no Unity

Estado em 13/10/2026: **planejamento revisável, nenhuma alteração de runtime**. Base observada `develop` limpo, `8ebd177c48f945d04b9f816bac1c841d4e03c068`. Handoff Supabase revisão 3 e documentos lidos; tarefa `figma-screens` reivindicada da versão 2 para 3 pelo ator `context_packet_ui_plan`. Reconciliar o HEAD antes de implementar porque outra frente corrige o recorder em worktree separado.

## Referência visual

Arquivo: https://www.figma.com/design/AoNgky0oNVLU31UvML5xns . Página `0:1`, cinco frames estáticos landscape 1280×720; leitura de nomes/textos/dimensões repetida nesta sessão. Screenshots foram revisados na criação e opções corrigidas. São referência de composição, não resolução nova do mundo Unity nem evidência de runtime.

| Tela | Nó | Desenho atual e contrato de integração |
| --- | --- | --- |
| Menu principal | 2:2 | Novo fluxo de entrada. Começar pode abrir seleção; Continuar está desenhado indisponível e deve permanecer assim até existir contrato real de retomada. Não inferir save de campanha do checkpoint em memória. |
| Equipe/perfil | 2:29 | Seleção local entre os assets existentes 4–5, 6–8, 9–12, 13–15, 16+. Personagem inicial exige extensão explícita do bootstrap; a sessão atual não recebe seleção inicial por API. Não iniciar e trocar em seguida silenciosamente. |
| HUD mobile | 2:71 | Espelhar fontes reais de vida/Honmoon/Special/comida/beat. O desenho revisado inclui nove ações e movimento 2D: parry, resgate e comida própria/no fã estão presentes. Preservar os contratos reais antes de substituir o touch atual. |
| Opções | 2:110 | Reapresentar opções existentes: flashes, legendas, mira, alternar pulo/resgate, remapeamento/restauração, PT/EN da narrativa e faixa etária. Texto maior, câmera suave e volume 70% foram removidos após a revisão; remapeamento ainda exige subpainel e binding. |
| Conclusão/replay | 2:150 | Mostrar resultado real, preservar formulário 1–5 diversão/dificuldade, comentários, status do registro e salvar/restart seguro. Não hardcode 5/5 ou “Honmoon recuperado”. Troca/menu são fluxos novos a contratar. |

## Fontes e separação de responsabilidades

Fontes lidas: `SliceHud2D`, `SliceTouchControls2D`, `SlicePlayerInput2D`, `SliceInputFrame`, `VerticalSliceSession2D`, `CharacterSwitchController2D`, `AgeProfileSelection2D`, `AccessibilityPreferences2D`, `FanRegistry2D`, `FanActor2D`; documentos AGENTS/continuidade/Figma/GDD/idade. UI atual usa OnGUI/GUILayout e safeArea; touch é IMGUI com contatos independentes, não UGUI. Seleção global e efeitos por idade estão compostos pelo HUD. Não desativar `SliceHud2D` para apenas esconder o desenho: seu OnDisable restaura pausa/input/narrativa e limpa a política visual; desativá-lo perde bindings e responsabilidades existentes.

Proposta de stack: UGUI/Canvas com layouts nativos, após confirmar pacote disponível e asset/meta/GUIDs; não adicionar dependências por inferência. Primeira etapa visual isolada não substitui a composição principal. Uma etapa seguinte separa rendering de `SliceHud2D` e mantém coordenação de idade, binding, pausa e feedback em um controlador. Nomes de contratos seguintes são propostos, não APIs existentes: `SliceUiState2D` (snapshot de leitura), `SliceUiFlow2D` (tela/transições), view HUD, view opções e adaptador touch. Usar referências explícitas, sem buscas globais por frame, sem estado gameplay duplicado em UI.

## Layout landscape e safe areas

Manter contrato de mundo/câmera 1920×1080/16:9 existente. Figma 1280×720 é unidade visual com proporção idêntica. Proposta: Canvas em Screen Space Overlay, CanvasScaler Scale With Screen Size referência 1280×720; ajustar largura/altura com matriz de dispositivos em vez de fixar tuning sem medição. Um root seguro converte `Screen.safeArea` em anchors normalizados. Reconstruir somente quando resolução/safeArea/orientação mudar. Mundo/render ocupa viewport normal; HUD e controles usam área segura.

Ancorar vida/status superior esquerdo, troca superior centro, pausa superior direito; movimento inferior esquerdo, ações inferior direito. O centro fica livre para ação e telegráficos. Não escalar hitbox por tamanho de texto. Grupos de layout e texto com wrap/flex evitam recortes, opções rolam em altura curta. Não assumir portrait jogável: fallback de orientação comunica uso landscape ou respeita decisão do produto, sem alterar PlayerSettings nesta etapa.

Validar 16:9, telas mais largas e área segura assimétrica nos dois lados. Valores físicos mínimos de toque exigem aparelhos/densidade; os 64px de referência são proposta visual, não prova de alvo acessível. Texto maior deve redistribuir layout, não cobrir ações. Estados selecionado/desabilitado têm texto/forma/foco além de cor.

## Estado HUD e binding

| Informação | Fonte runtime existente | Regra |
| --- | --- | --- |
| Beat/conclusão | session.BeatTitle, IsRunning, IsComplete | “Preparando” quando ainda sem sessão; conclusão somente real. |
| Vida | Manager.ActiveCharacter.CurrentHealth/MaximumHealth | Rebind em ActiveCharacterChanged; não manter ator antigo. |
| Honmoon | session.Honmoon.Value | Valor real, sem percentual se escala não confirmada. |
| Special | session.Special.IsReady/Charge | Pronto/carregando; UI não cria carga nem autoriza ativação. |
| Comida | session.Inventory.TotalCount | Ações FoodSelf e FoodFan continuam distintas. |
| Fãs | session.Fans.Registry, entidades/eventos de resgate | Registry.Count não significa fãs resgatados. Definir contador por beat ou agregar evento real com reset antes de apresentar fração. |
| Perfil | AgeProfileSelection2D.Current/ProfileChanged | Falha TrySelect não muda seleção visual; não modificar assets compartilhados. |

Carregar estado em Configure/bind explícito; rebind em troca de manager/ator/beat; remover subscriptions no disable/teardown. Fontes nulas/inativas não recebem comandos: mostrar indisponível. HUD lê estado, não decide dano, resgate, comida, Honmoon ou Special. Inatividade/morte/troca limpa estados de pointer e ações held.

## Input e pausa: preservação obrigatória

`SliceInputFrame` contém Move 2D, JumpPressed/Released, Dash, Attack, Parry, Switch, Interact/Held/Released, FoodSelf, FoodFan, Special. `SlicePlayerInput2D.SubmitInput` agrega edges com OR e escolhe movimento por magnitude; não disparar comandos todo frame apenas porque botão visual está selecionado. Toggle jump/interact mantém o contrato atual de press/release e toggle-off; contatos simultâneos independentes e cancelamento precisam equivalência.

Primeiro trocar apenas visual do touch ou introduzir adaptador com testes comparativos. Nunca manter dois produtores touch ativos para os mesmos pointers. Pausa/opções e conclusão bloqueiam input; abrir menu enquanto segurando salto/resgate deve emitir releases/limpar captura, fechamento exige contato novo. Cancelamento, pointer fora, focus loss/disable e troca de tela não deixam ações presas. Gamepad/teclado permanecem funcionalmente iguais. Os três botões nominados de personagem no HUD são nova seleção direta: o input existente só fornece Switch circular. `TrySwitchTo` existe mas bypassaria restrições de input se chamado cru; definir rota que preserve switchingAllowed e guards, ou manter um botão de troca cíclica com retratos informativos.

Preservar pausa por `HitStopService`: guardar estado anterior, adquirir ao abrir opções, restaurar somente o que esta UI possuía, não atribuir `Time.timeScale=1` indiscriminadamente. Preservar bloqueio anterior de input/narrativa, troca de presenter/manager e cleanup. UI deve continuar operável com timeScale zero. Opções atuais aplicam ao vivo; botão “Aplicar ajustes” requer decisão explícita de commit/cancel para não alterar essa semântica por acidente. Nesta migração inicial, usar retorno e atualização ao vivo, sem prometer transação de ajustes.

## Tokens visuais de referência

| Papel | Cor hex |
| --- | --- |
| Texto principal / secundário | #203047 / #53657C |
| Fundo / superfícies | #F5F8FC / #FFFFFF |
| Ação principal | #2463D4 com texto branco |
| Proteção / Special | #C5EEDF / #7653B7 |
| Identidades Rumi / Mira / Zoey | #F6CADA / #F8D987 / #D5C6F2 |

Inter, Regular/Bold; títulos 36, ações 24, corpo 20–22 na unidade de referência. Conferir licença/disponibilidade antes do font asset Unity; não depender de fonte do SO. Medir contraste nos estados finais, sobretudo disabled, sem alegar certificação. Geometria original e iniciais são placeholders. Futuro código deve obter contexto Figma de alta fidelidade com screenshot pelo skill figma-design-to-code antes de implementação; não usar screenshot como asset.

## Etapas isoladas e critérios de aceite

1. **Contrato e inventário**: aprovar mapeamento de ações/estados e resolver lacunas do desenho; reler cards 90–93/68 e dependências. Não aproveitar autorização de planejamento para alterar gameplay.
2. **Prefab/layout visual isolado**: tokens, root seguro, cinco views e estados placeholder em cena de UI própria, sem sessão/touch real. Aceite: assets/meta/GUIDs e layout sem cortes nas proporções definidas, sem cena principal alterada.
3. **HUD somente leitura**: bind real e atualização de vida/recursos/beat; manter controlador antigo e evitar desenho duplicado por separação explícita. Aceite: ator trocado/respawn/beat não deixam valores antigos; null guards e subscriptions limpos.
4. **Opções e touch equivalentes**: preservar preferências/pausa e todos os comandos; retirar produtor antigo somente após integração verificada. Aceite: multitouch, hold/toggle/release, pause/focus loss, tecla inválida/restauração e guards de troca sem regressão.
5. **Entrada/perfil/conclusão**: fluxo novo separado do re-skin; definir partida inicial/Continuar/menu, manter recorder/formulário/restart e mensagem de falha de save. Integrar correção de idempotência da outra frente antes de adaptar feedback. Aceite: falha de save mantém avaliação e sessão, repetição conserva idempotência, novo recorder por sessão.
6. **Validação**: import/compile 6000.6.4f1, EditMode/PlayMode pertinentes, walkthrough slice, captura Unity comparada ao Figma por tela e device touch/contraste/texto/PT/EN/5 faixas; registrar commit/build e evidência. Enquanto Editor/testes ausentes, status implemented_pending_validation, nunca runtime aprovado.

Próximo passo concreto: revisar este contrato e findings da auditoria de idade/touch; preparar apenas layout isolado em branch própria depois do escopo autorizado. A publicação deste contrato é documental; nenhuma implementação Unity faz parte desta entrega.

## Revisão Figma após auditorias — 13/10/2026

A revisão autorizada corrigiu as lacunas visuais anteriores: Continuar agora tem estado indisponível; perfil 6–8 tem check e texto selecionado; HUD inclui nove ações (pulo/ataque/dash/parry/troca/Special/resgate/comida própria/comida no fã) e representação de movimento 2D; retratos são informativos com botão Trocar circular; opções usam ajuda na mira, flashes, pulo/resgate segurar, legendas, narrativa PT/EN e acesso a remapear. Ajustes imediatos valem para a sessão. Texto maior/câmera/volume foram removidos, assim como a promessa de Aplicar. Notas técnicas e estados Special carregando/pronto/indisponível ficam fora dos frames produto no nó `7:28`. Resultados são marcados ilustrativos nesse handoff externo; produto não deve exibir valores fictícios.

Screenshots das cinco telas revisadas foram inspecionados. Remapeamento detalhado (campo tecla, seleção ação, aplicar/restaurar e erros) ainda precisa subpainel; estados de toggle e disabled possuem contrato acima, não runtime implementado. Touch/movimento 2D são representações estáticas e precisam de pad/contatos reais na integração.

Auditoria de touch aponta rotas explícitas de pausa/menu em teclado/gamepad (Start hoje é FoodFan), modo de entrada/histerese, risco de perda de estados em polling bruto Touchscreen.Update, safeareas assimétricas, releases de toggle e cinco faixas. Incorporar os cinco cenários de `docs/qa/TOUCH_VALIDATION_PLAN.md`; não mudar input polling nessa frente visual sem contrato e teste separado. Evidência atual permanece exclusivamente leitura estática de fonte/Figma, sem Unity.
