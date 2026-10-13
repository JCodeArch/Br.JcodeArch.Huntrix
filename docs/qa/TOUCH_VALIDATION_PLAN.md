# Plano de validação touch / safe area

Fonte auditada: `huntrix` HEAD `8ebd177c48f945d04b9f816bac1c841d4e03c068`, working tree limpa na leitura. Auditoria estática; nenhum teste Unity, build ou aparelho executado. Continuidade lida via ai_context.get_handoff, revisão 3. Classe real: SliceTouchControls2D, não SliceTouchInput2D.

## Evidência e divergências

- Assets/Scripts/Gameplay/Touch/SliceTouchControls2D.cs:33–38 lê Touchscreen.current.touches/press em Update. Documentação oficial Unity InputSystem touch-polling alerta que leitura direta neste ciclo perde mudanças de estado; caso de toque curto requer reprodução. Fonte: https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/touch-polling.md . Manifest fixa InputSystem 1.20.0; documentação 1.20 direta indisponível na consulta. Não confundir risco documental comprovado com bug reproduzido em aparelho.
- INPUT_CONTROLS.md exige ocultar controles touch ao usar gamepad. SliceTouchControls2D.OnGUI:69–72 não consulta gamepad nem modalidade ativa; portanto essa transição não foi implementada nessa classe.
- INPUT_CONTROLS.md exige pausa/menu operável pelo método ativo. SlicePlayerInput2D cria 13 ações sem Pause; gamepad Start está mapeado a FoodFan. SliceHud2D.OnGUI abre opções apenas via GUILayout.Button; não há rota de pausa por gamepad/teclado nessa composição. Divergência de contrato, sem alegação de execução.
- Conversão Screen.safeArea para IMGUI em HUD (origem Screen.height-safe.yMax) e touch (bottom Screen.height-safe.yMin) é matematicamente coerente para insets. Não identifiquei inversão de Y.
- O mínimo de unit=45 torna o layout incapaz de garantir contenção em toda safe area: botões utilitários iniciam 3.6*unit acima da base; safe height <162 sai pelo topo. Safe area 640x320 produz unit=45.714; utilitários y≈155.43..196.57 e HUD y=8..188, sobreposição geométrica. Isso é contraexemplo de cálculo; qualificação em resoluções alvo segue pendente.
- Assets/Tests não referencia SliceTouchControls2D, SlicePlayerInput2D, Touchscreen ou safeArea. Referências a SliceHud2D em HitStopServicePlayModeTests cobrem posse/restauração de pausa, não hit testing, input real ou layout.

## Cinco testes críticos sem cobertura direta

| Teste | Estímulo e resultado observável | Dependência |
|---|---|---|
| Eventos curtos e multitoque | Via InputSystem real, enfileirar down/up entre dois Updates e mover+atacar com dois dedos. Cada toque distinto deve produzir exatamente uma ação; soltar um dedo não interrompe o outro. Observar ações/gameplay, não apenas arrays privados. | Editor/package 1.20.0 importado; fixture session viva; InputTestFixture ou eventos oficiais |
| Segurar/alternar e cancelamento | Pulo/interação em segurar e alternar; segundo toque desliga; abrir opções, completar sessão, desabilitar/religar touch e trocar perfil durante contato. Resgate e jump release devem encerrar sem movimento/ação residual. | Sessão, jump/rescue reais, preferências, HUD; cinco perfis etários |
| Safe area, layout e alvos | Landscape 16:9 e telas mais largas, insets assimétricos, safe area 640x320 e menor; verificar rects dentro da safe area, ausência de sobreposição HUD/controles e touch nos limites sem atingir ação vizinha. Rotacionar landscape esquerda/direita preserva hit testing. | Matriz de dispositivos/aprovação de mínimo suportado; inspeção visual e toque real |
| Gamepad/touch e pausa | Conectar/usar gamepad, voltar a touch sem oscilação por ruído, navegar e abrir/fechar pausa pelo método ativo. Verificar controles visíveis e ações sem uso obrigatório de mouse. | Decisão/modalidade ativa, wiring de Pause e menus; implementação de UI |
| Troca de personagem e gating | Touch de troca em estado permitido/proibido; SP indisponível e disponível; simultâneo mover+trocar/atacar; morte/respawn com dedo ainda pressionado. Resultado deve pertencer ao ator vivo atual, sem ação duplicada ou comando atrasado ao reativar. | Manager/session/switch/special reais; estados disabled visuais definidos |

Não implementar testes que apenas reproduzam a fórmula Layout ou inspecionem privados como aceite: preferir eventos reais e efeitos públicos; inspeção de rects ajuda o diagnóstico, mas teste de alcance/legibilidade precisa de aparelho/pessoa.

## Dependências para UI / context_packet

Começar pelo arquivo Figma https://www.figma.com/design/AoNgky0oNVLU31UvML5xns e inventário: Menu 2:2, Equipe/perfil 2:29, HUD 2:71, Opções 2:110, Conclusão/replay 2:150; landscape 1280x720 é referência de design. Fonte local docs/ui/FIGMA_SCREENS.md declara cinco frames revisados visualmente, sem implementação Unity ou aprovação humana/dispositivo. Não fiz leitura live Figma nesta auditoria.

Antes de integração: definir Pause, modalidade ativa/histerese, estados disabled e cooldown, safe area mínima e layout responsivo, tamanho/posição/alvos de touch, alternativa gamepad para menus, semântica alternar em perfis e navegação. HUD atual é IMGUI; não prometer fidelidade ao Figma. Coop até três permanece requisito sem implementação/prova nessa composição. Execução dos cinco testes depende de importação/compilação Unity e build/dispositivos; manter runtime pendente.
