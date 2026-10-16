# HUNTR/X — Honmoon: Manual e Contexto Geral do Projeto

> **Documento Oficial de Continuidade, Arquitetura e Engenharia**  
> **Chave do Projeto (AI Continuity / Supabase):** `huntrx-honmoon`  
> **Repositório Principal:** `D:\Workspace\Br.JcodeArch.Huntrix`  
> **Versão Atual:** `0.1.0-dev.1` | **Unity:** `6000.6.4f1 (Unity 6 LTS)`  
> **Última Atualização:** Outubro de 2026  

---

## 1. O que é o Projeto HUNTR/X — Honmoon?

**HUNTR/X — Honmoon** é um jogo eletrônico de ação e combate estilizado em 2D (*Beat 'em Up* / *Action Platformer*), desenvolvido na **Unity 6 (URP 2D)**.

O jogo combina a energia vibrante da música K-Pop com combate dinâmico contra demônios e forças sobrenaturais. As protagonistas formam o trio idol **HUNTR/X** — **Rumi**, **Mira** e **Zoey** — que nos palcos brilham como estrelas mundiais, mas nas sombras atuam como caçadoras de demônios para proteger a energia vital da cidade e de seus fãs contra a fenda sombria da **Honmoon**, liderada pelo demônio supremo **Gwi-Ma** e pelo rival de palco e espadas **Saja Jinu**.

### 1.1. As Três Heroínas e seus Estilos de Combate
- **Rumi (Espadachim Ágil):** Especialista em combate corpo a corpo rápido com lâmina espiritual (*Spirit Katana*), combos em alta velocidade, esquiva ágil e cancelamento de animações.
- **Mira (Tanque Protetora):** Especialista em defesa com leque místico e barreiras defensivas (*Defensive Field*), absorção e redirecionamento de impacto, proteção de fãs vulneráveis e controle de grupo.
- **Zoey (Atiradora de Precisão):** Especialista em combate à distância de precisão (*Ranged Hitscan*), mira omnidirecional em 360° no chão e no ar, disparos de estilhaços e cobertura tática.

---

## 2. Para que Serve e Objetivos do Projeto

1. **Vertical Slice Jogável:** Validar a experiência completa do ciclo de jogabilidade (*Core Game Loop*) em uma fatia vertical polida de 7 etapas consecutivas na cena `Assets/Scenes/VerticalSlice.unity`.
2. **Combate Responsivo & Mobile-First:** Proporcionar uma jogabilidade de ação fluida a 60 FPS estáveis em telas sensíveis ao toque (dispositivos móveis Android e iOS) sem abrir mão de suporte refinado para PC (teclado e controle gamepad).
3. **Mecânica Central de Resgate de Fãs & Honmoon:** O combate vai além de derrotar monstros: o jogador deve interceptar o dreno de almas (*Soul Drain*), resgatar fãs em perigo antes que a fenda Honmoon consuma o palco, e restaurar recursos vitais recolhendo alimentos tradicionais de rua (*Food Resources* como Hotteok, Tteokbokki e Kimbap).
4. **Acessibilidade Universal & Segurança Infantil:** Suporte nativo a 5 perfis etários distintos (Early Child, Child, Tween, Teen, Adult) em conformidade rigorosa com normas internacionais (COPPA, GDPR-K, sem qualquer coleta de PII), assistência de mira configurável, redução de flashes visuais e suporte a legendas de alto contraste.
5. **Engenharia de Alta Confiabilidade:** Arquitetura desacoplada e estritamente orientada aos princípios SOLID, parametrização via `ScriptableObject`, separação total entre Runtime e Testes (mais de 380 testes unitários e de integração), e esteira de continuidade e governança autônoma multi-IA com persistência e rastreabilidade.

---

## 3. Onde e Como Usar

### 3.1. Requisitos do Sistema e Ambiente
- **Engine:** Unity 6000.6.4f1 (Unity 6 LTS) com Universal Render Pipeline (URP).
- **Módulos Unity:**
  - Android Build Support (OpenJDK, Android SDK 34/36, NDK r27b ou superior).
  - Windows Build Support (IL2CPP ou Mono x64).
  - iOS Build Support (para geração de projeto Xcode em macOS).
- **Controle de Versão:** Git com suporte a Git LFS (*Large File Storage*) habilitado para binários e texturas pesadas.
- **Resolução Nativa:** 1920x1080 (16:9 Landscape) com adaptação dinâmica para resoluções ultra-wide e notch de celulares móveis via `SliceSafeAreaLayout2D`.

### 3.2. Estrutura de Pastas do Repositório
```
D:\Workspace\Br.JcodeArch.Huntrix\
├── Assets\
│   ├── Scripts\
│   │   ├── Gameplay\          # Movimentação, Combate, Personagens, Inimigos, Fãs, Touch
│   │   ├── Data\Definitions\  # ScriptableObjects com parâmetros de equilíbrio e personagens
│   │   ├── Infrastructure\    # Câmera, Áudio, Save, Profiles, Pooling
│   │   └── Presentation\Hud\  # Interface visual, HUD, Pause, Safe Area, Layout Figma
│   ├── Scenes\                # VerticalSlice.unity, CombatLab.unity, GameBootstrap.unity
│   └── Tests\
│       ├── EditMode\          # Testes rápidos de lógica pura e validação de contratos
│       └── PlayMode\          # Testes simulados de física, combate, colisão e persistência
├── docs\                      # Documentação técnica, arquitetura, GDD e relatórios de QA
├── outputs\                   # Builds compiladas (APK Android, executável Windows)
├── scripts\                   # Scripts de build, automação e execução de testes em CI
├── Logs\                      # Relatórios e logs históricos de importação e validação
├── AGENTS.md                  # Regras operacionais mandatórias para desenvolvimento por IA
└── PROJECT_RULES.md           # Políticas de governança, Git, testes e Definition of Done
```

### 3.3. Como Abrir e Executar o Projeto no Unity
1. Abra o **Unity Hub** e selecione **Add project from disk**.
2. Aponte para o diretório `D:\Workspace\Br.JcodeArch.Huntrix`.
3. Garanta que a versão do Editor selecionada seja **6000.6.4f1**.
4. No Unity Editor, navegue até `Assets/Scenes/` e abra:
   - **`VerticalSlice.unity`**: para rodar o jogo completo com progressão dos 7 estágios, HUD, ondas de inimigos e chefes.
   - **`CombatLab.unity`**: para ambiente de laboratório isolado de testes de combate, combos e física.
5. Pressione o botão **Play** no topo do Editor.

### 3.4. Como Rodar os Testes Automatizados
- **Pelo Editor:** Vá em `Window` > `General` > `Test Runner`. Selecione as abas `EditMode` ou `PlayMode` e clique em **Run All**.
- **Por Linha de Comando (Headless / Batchmode):**
  ```powershell
  # Execução dos testes via script de automação
  bash scripts/run-unity-tests.sh
  ```
  Ou diretamente via executável do Unity:
  ```powershell
  & "C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Workspace\Br.JcodeArch.Huntrix" -runTests -testPlatform EditMode -testResults "TestResults/EditModeResults.xml" -quit
  ```

### 3.5. Esquema de Controles

| Ação | Teclado (PC) | Gamepad (Xbox/PlayStation) | Mobile (Touch Screen) |
|---|---|---|---|
| **Mover Esquerda / Direita** | `A` / `D` ou Setas | Analógico Esquerdo / D-Pad | Joystick Virtual Esquerdo |
| **Pular (Variável / Wall Kick)** | `Espaço` | Botão Sul (`A` / `X`) | Botão Touch `Jump` |
| **Dash Terrestre / Air Dash** | `Shift Esquerdo` ou `J` | Botão Leste (`B` / `Círculo`) | Botão Touch `Dash` |
| **Ataque Leve / Combo** | `K` | Botão Oeste (`X` / `Quadrado`) | Botão Touch `Attack` |
| **Ataque Especial / Fogo** | `L` | Botão Norte (`Y` / `Triângulo`) | Botão Touch `Special` |
| **Parry / Escudo Defensivo** | `I` | Gatilho Esquerdo (`LT` / `L2`) | Botão Touch `Parry / Shield` |
| **Trocar de Personagem** | `Q` (anterior) / `E` (próximo) | Botões Superiores (`LB` / `RB`) | Seletor Touch no HUD |
| **Pausar / Menu de Opções** | `Escape` | Botão `Start / Menu` | Botão Touch de Pausa (HUD topo) |

---

## 4. O que Já Tem Feito e o que Foi Desenvolvido

O desenvolvimento de HUNTR/X cobriu rigorosamente os 116 cards definidos no backlog mestre do projeto. Abaixo está a síntese de tudo o que foi implementado e integrado ao código-fonte:

### 4.1. Fundações, Arquitetura e Engenharia Base (#01 – #18)
- **GDD Mestre Consolidado (#01):** Documentação canônica completa com regras, mecânicas e balanceamento.
- **Setup de Pacotes Essenciais & LFS (#13, #15):** Integração do novo Unity Input System, Universal RP, 2D Animation, Git LFS configurado para metadados e texturas.
- **Hierarquia Limpa de Pastas & Bootstrapping (#14, #16):** Separação estrita de domínios em `Assets/Scripts/` e cena de inicialização limpa `GameBootstrap.unity`.
- **Arquitetura Orientada a Dados (#17):** Separação desacoplada de dados estáticos (`CharacterDefinition`, `ComboDefinition`, `DashDefinition`) em `ScriptableObjects` e estado de execução em componentes MonoBehavior isolados.
- **Laboratório de Combate (#18):** Cena `CombatLab.unity` para iteração instantânea de física de golpes e colisões.

### 4.2. Sistema de Movimentação e Física de Plataforma 2D (#19 – #23)
- **Movimento Horizontal (#19):** Aceleração progressiva, frenagem responsiva sem deslize indesejado e suporte a inclinações de terreno.
- **Pulo Avançado com Física Calibrada (#20):** Pulo de altura proporcional ao tempo em que o botão permanece pressionado, sistema de tolerância com *Coyote Time* (tempo de tolerância ao cair de beiradas) e *Jump Buffering* (registro antecipado de clique antes de tocar o chão).
- **Ground Dash (#21):** Arrancada horizontal terrestre com curva de aceleração de alta intensidade e janela temporária de invulnerabilidade (*I-Frames*).
- **Air Dash (#22):** Arrancada aérea de uso único por ciclo de salto, resetada automaticamente ao colidir com o solo ou com paredes.
- **Wall Jump & Deslize em Paredes (#23):** Detecção de contato em parede com atrito suavizado e impulso angular (*Wall Kick-off*) em vetor oposto calibrado.

### 4.3. Combate, Combos, Parry e Checkpoints (#24 – #27)
- **Combate Base & Hitboxes 2D (#24):** Arquitetura desacoplada de receptores de dano (`DamageReceiver2D`), contato físico com camadas de facção (Player vs. Inimigos) e aplicação de knockback.
- **Sistema de Combos (#25):** Encadeamento de 3 golpes sequenciais com janelas de buffer de entrada de comandos, tempos de recuperação e cancelamento.
- **Parry & Defesa Reativa (#26):** Janela de precisão de 0.15s para defletir ataques, atordoar o agressor (*Stun*) e anular todo o dano recebido.
- **Checkpoints e Respawn (#27):** Salvamento de marcos de progresso na fase, restauração de vida das idols e reativação fluida no ponto de controle.

### 4.4. Heroínas do Trio HUNTR/X & Gestão de Slots (#28 – #32)
- **Rumi (#28):** Protótipo completo da espadachim com corte preciso, velocidade de avanço e ataques ágeis.
- **Mira (#29):** Mecânica do campo defensivo (`MiraDefensiveField2D`), proteção seletiva de aliados e redirecionamento de projéteis.
- **Zoey (#30):** Protótipo da atiradora de precisão com disparo hitscan, detecção imediata de 64 pontos de impacto no raio de visão e suporte a disparo com mira aérea e diagonal.
- **CharacterManager & Slot de Jogador (#31):** Controlador centralizado que mantém instâncias em cache das 3 personagens, vida individual separada e ciclo de ativação/desativação limpo.
- **Troca Dinâmica de Personagens (#32):** Alternância instantânea no mesmo slot com transição suave de física e preservação dos estados individuais de recarga e vida.

### 4.5. Ecossistema de Inimigos, Miniboss e Rival Saja Jinu (#33 – #40, #55 – #61)
- **Inimigos Básicos Terrestres (#33, #34):** Demônios batedores (*Grunt*) e rastejadores com patrulha de bordas e ataque corpo a corpo.
- **Inimigos Voadores & Distância (#35, #36):** Demônios alados que mantêm distância do solo e atiradores com projéteis teleguiados.
- **Inimigos Tanque & Suporte (#37, #38):** Demônios de alta resistência com armadura pesada e totens que curam ou fortalecem inimigos adjacentes.
- **Miniboss & Encontros de Horda (#39, #56):** Arenas de bloqueio de tela com ondas progressivas de demônios.
- **Saja Jinu (#40):** O grande rival das idols, dotado de IA com estados de aproximação, ataque sequencial, contra-ataque e fuga tática.

### 4.6. Resgate de Fãs, Recursos e Crise da Honmoon (#41 – #54)
- **Estados de Fãs (#41, #42):** Civis nos shows que podem ser apavorados ou alvos de feitiço demoníaco.
- **Mecanismo de Dreno de Almas (#43):** Canalização contínua de energia demoníaca que o jogador deve interromper rapidamente atacando os demônios drenadores.
- **Resgate & Bônus de Palco (#44 – #46):** Interação rápida para libertar os fãs, gerando orbes de energia e restauração de moral.
- **Recursos Alimentares Coreanos (#51 – #54):** Pickups interativos espalhados pelo mapa (Hotteok, Tteokbokki, Kimbap) que fornecem regeneração de vida e aumentam temporariamente a velocidade das heroínas.

### 4.7. Fases, Arenas e Vertical Slice Integrada (#62 – #68, #74)
- **Design de Fases por Gramática Modular (#62):** Blocos de geometria com regras de transição horizontal e vertical.
- **7 Estágios da Vertical Slice Integrados (#68):**
  1. *Abertura nos Bastidores:* Apresentação do trio e tutorial de movimentação.
  2. *Corredor do Estúdio:* Primeiros confrontos com demônios básicos e quebra de barreiras.
  3. *Praça de Alimentação do Palco:* Resgate de fãs civis e coleta de alimentos tradicionais.
  4. *Combate Aéreo Vertical:* Plataformas móveis e demônios voadores com troca para Zoey.
  5. *Arena de Horda:* Bloqueio de portas e combate intenso contra grupos mistos.
  6. *Confronto contra Saja Jinu:* Duelo tático de espadas e esquiva rítmica.
  7. *Invasão da Honmoon e Fuga do Palco:* Desfecho épico da fatia vertical.

### 4.8. Arte, Áudio Dinâmico e Efeitos Visuais (#69 – #90)
- **Bíblia Visual e Paleta de Cores (#69, #70):** Design neon-urbano cyberpunk com estética coreana contemporânea.
- **Model Sheets e Rigs 2D (#71, #72):** Estruturas esqueléticas para animação 2D com interpolação de ossos.
- **VFX de Combate (#75 – #78):** Efeitos de corte de lâmina (*slash lines*), faíscas de impacto metálico, poeira de pulo e auras do campo defensivo de Mira.
- **Áudio Dinâmico & Sistema de Música (#79 – #86):** Gerenciador de áudio com camadas dinâmicas de sintetizadores e batidas de K-Pop sincronizadas com a intensidade das lutas e aplausos da multidão de fãs.

### 4.9. Acessibilidade, Inclusão e Conformidade (#91 – #96, #102, #103)
- **Cinco Perfis Etários Nativos (#91, #102):** Adaptação de ritmo, dicas contextuais e intensidade visual conforme a idade do jogador, sem violação de privacidade (zero telemetria não autorizada).
- **Opções de Acessibilidade (#93, #103):** Modo para daltônicos, redução drástica de clarões luminosos (*Flash Reduction*), assistência de mira para armas à distância e remapeamento integral de teclas.

### 4.10. Layout Figma, Interface Mobile e Safe Area
- **Telas Desenhadas no Figma:** Telas de Início, Seleção de Idade, HUD de Gameplay com 9 ações virtuais, Menu de Opções e Tela de Pausa.
- **Layout Responsivo com Safe Area:** Componente `SliceSafeAreaLayout2D` para ancoragem automática de botões fora da área de recorte da câmera e cantos arredondados de celulares modernos.

### 4.11. Automação de Compilação e Binários Gerados
- **Android APK:** Geração validada de APK assinado com v2 signature (`outputs/Huntrix-Android-VerticalSlice.apk`).
- **Windows Standalone:** Executável x64 compilado localmente em modo headless (`outputs/Windows/Huntrix.exe`).
- **Resolução de Conflitos C++:** Correção de colisão de cabeçalhos no Windows IL2CPP renomeando o arquivo de versão para `VERSION.txt`.
- **Continuidade Operacional Autônoma:** Esquema `ai_context` integrado no Supabase (`bkfjhgwxhcwhmvsitckp`) com controle de versão, leases e histórico de tarefas.

---

## 5. Próximos Passos e Como Prosseguir

1. **Validação de Testes em Runtime no Unity Editor:**
   - Abrir o projeto no Unity 6000.6.4f1 e executar a suíte completa de 381 testes (86 EditMode + 295 PlayMode) para atestar a aprovação dos testes que estavam aguardando o ambiente gráfico.
2. **Playtest Humano Guiado de 60 Segundos (#68, #100–104):**
   - Rodar a cena `VerticalSlice.unity`, percorrer as 7 etapas e registrar as sensações de fluidez, resposta ao toque, ritmo dos combos e troca de heroínas.
3. **Instalação e Validação em Aparelhos Físicos (Android / iOS):**
   - Realizar o deploy do APK via `adb install outputs/Huntrix-Android-VerticalSlice.apk` em aparelhos móveis reais de teste para medição empírica de uso de CPU, GPU, aquecimento e taxa de quadros (60 FPS estáveis).
4. **Fechamento de Gates de Publicação e Direitos:**
   - Formalizar o pacote de publicação e o registro de direitos autorais de trilha e personagens para submissão nas lojas Google Play Store e Apple App Store.

---
*Este documento é o guia definitivo de engenharia, arquitetura e uso do projeto HUNTR/X — Honmoon. Mantenha-o sempre atualizado a cada nova entrega ou marco concluído.*
