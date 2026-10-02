# Controles multiplataforma — HUNTR/X: Honmoon

**Card de origem:** [08 — Definir controles multiplataforma](https://trello.com/c/ltBsR8f7/8-08-definir-controles-multiplataforma)  
**Estado:** proposta-base de mapeamento para o protótipo; validação de game feel e acessibilidade ainda necessária.  
**Escopo:** mapear teclado, gamepad e touch para ações lógicas comuns. Não altera regras, janelas, efeitos nem identidade do gameplay.

## Princípio de entrada

Gameplay consome **ações lógicas** (por exemplo, `Move`, `Jump` e `Attack`) e não deve depender de uma tecla, botão ou coordenada de toque específicos. Teclado, gamepad e touch emitem as mesmas ações; o sistema de gameplay decide o resultado segundo os contratos dos cards próprios. `Move` representa uma direção 2D: teclado e D-pad produzem direções digitais, enquanto stick e touch podem produzir valores contínuos. Dead zone, aceleração e resposta de movimento pertencem aos cards de gameplay. O mapa é uma proposta de protótipo e pode ser ajustado após a Combat Lab/playtest, sem mudar a semântica das ações.

## Ações e mapa inicial

| Ação lógica | Teclado — Windows | Gamepad — Windows/Android/iOS | Touch — Android/iOS |
|---|---|---|---|
| `Move` | WASD ou setas | Stick esquerdo ou D-pad | Stick virtual à esquerda |
| `Jump` | Espaço | Botão inferior (South / A no Xbox) | Botão “Pular” |
| `Dash` | Shift esquerdo | Botão direito (East / B no Xbox) | Botão “Dash” |
| `Attack` | J ou Z | Botão esquerdo (West / X no Xbox) | Botão “Atacar” |
| `Special` | K ou X | Botão superior (North / Y no Xbox) | Botão “Especial” |
| `SwitchCharacter` | Q ou Tab | Ombro esquerdo (LB / L1) | Botão “Trocar” |
| `Pause` | Esc | Menu/Start | Botão de pausa no canto superior |

Os nomes entre parênteses descrevem uma referência de layout, não exigem um fabricante de controle. O layout do gamepad deve usar rótulos apropriados ao controle detectado. Os pares secundários do teclado são atalhos iniciais para reduzir conflito com a mão esquerda/direita; ambos devem acionar a mesma ação lógica.

`SwitchCharacter` só fica habilitada nos estados/modos em que a troca for permitida pelo gameplay. `Special` e as demais ações seguem os critérios, custos e estados definidos pelos cards de gameplay; este documento não cria regras para elas. O mapa não inclui mira por mouse, combos, ataque direcional, remapeamento completo ou gestos.

## Apresentação e comportamento

- Windows mostra prompts de teclado ou gamepad conforme o último dispositivo usado; quando a troca do prompt puder distrair, manter o prompt estável até uma pausa ou menu.
- Android/iOS usam touch como padrão quando não há controle físico ativo. Conectar ou usar um gamepad permite jogar pelo controle e oculta os controles na tela; a interface não deve alternar repetidamente por ruído de entrada.
- A área de toque oferece movimento contínuo no lado esquerdo e botões de ação separados no lado direito. Deve aceitar movimento e ao menos uma ação simultânea. O alcance e a posição inicial do stick virtual não podem exigir que a pessoa alcance um canto específico da tela.
- Em retrato, a experiência jogável não é suportada pelo contrato de câmera do item 07. Em paisagem, controles e HUD respeitam recortes físicos e safe areas; o layout não reduz a zona de ação segura definida em `docs/architecture/RESOLUTION_AND_CAMERA.md`.
- Não exigir precisão de toque, gestos rápidos ou simultaneidade além do que o protótipo validar. Tamanho, espaçamento, transparência, zonas de descanso dos dedos e resposta multitoque serão verificados em aparelhos reais antes de aprovar o touch.
- Teclado e gamepad devem permitir que as ações essenciais de gameplay sejam executadas sem depender de touch. Pausa e menus devem ser operáveis pelo método ativo.

## Cooperação e plataformas

Windows mantém teclado e gamepad como métodos cobertos; Android e iOS mantêm touch e gamepad como métodos cobertos. A compatibilidade real depende de dispositivos e sistema operacional testados nos cards de plataforma.

O requisito de produto de até três personagens/jogadores e o modo solo continuam válidos, mas este card não decide coop local/online, pareamento de dispositivos por participante, vários jogadores compartilhando um teclado/tela touch, ou suporte simultâneo de múltiplos controles no mobile. Esses pontos são gates explícitos antes de declarar coop demonstrada. Um jogador em touch é a referência do protótipo móvel até que o escopo cooperativo seja decidido.

## Dependências e gates de validação

- O card 13 configura os pacotes essenciais, incluindo Input System, e decidirá a versão e a arquitetura concreta de entrada da Unity; esta proposta não adiciona dependência nem escolhe versão do pacote.
- Os cards de gameplay definirão o conjunto final de ações, seus estados e semântica. Atualizar este mapa junto desses contratos se uma ação for alterada ou removida.
- Na Combat Lab, verificar as sete ações aplicáveis em cada método, transições entre teclado/gamepad, gamepad/touch em mobile, prompts corretos e os pares de ações simultâneas exigidos pelos contratos de gameplay então definidos, incluindo no touch. Um pressionamento/toque distinto não pode disparar a mesma ação lógica duas vezes, salvo repetição ou comportamento contínuo previsto pelo contrato da ação.
- O playtest verifica legibilidade, alcance, esforço, erros de acionamento e sensação de resposta. Nenhum mapeamento é declarado validado por este documento.
- Opções de remapeamento, tamanho/posição de botões e ajustes acessíveis completos permanecem nos cards de acessibilidade. Não dispensar requisitos de acessibilidade ou Child Safety por causa do escopo deste card.

## Referências Unity

- [Unity 6 — Input](https://docs.unity3d.com/6000.0/Documentation/Manual/Input.html): visão geral dos sistemas de entrada e dispositivos.
- [Unity 6 — Input System Package](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.inputsystem.html): pacote, compatibilidade do editor e documentação versionada.
- [Input System — InputAction](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputAction.html): ações abstratas para representar conceitos lógicos em vez de controles físicos.
- [Input System — On-Screen Controls](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/OnScreen.html): controles na tela usando toque ou outro dispositivo apontador.

As versões nas referências do pacote servem para localizar a documentação; a versão que será instalada permanece para o card 13.


