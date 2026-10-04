# Parry — Design

**Data:** 2026-10-04
**Card:** Trello #26 — Implementar parry
**Status:** Escopo técnico decidido sob a delegação do proprietário do projeto.

## Objetivo
Adicionar uma janela configurável de parry à resolução de combate existente. Um ataque inimigo elegível durante a janela é cancelado antes de causar dano ou knockback, e gera um evento distinto para que sistemas futuros possam oferecer contra-ataque ou recompensa.

## Contratos e decisões
- A janela é definida por um ScriptableObject estático de parry, com duração finita e positiva. Não criar asset exemplo nem escolher valor de balanceamento neste card.
- Um componente de runtime dedicado controla elegibilidade, ativação, tempo restante e consumo da janela. Uma API pública de gameplay inicia o parry; bindings de input são escopo futuro.
- A janela avança em passos de física usando tempo escalado. Pause e hit stop congelam a simulação e a janela.
- Um ataque só pode ser aparado quando todas as validações existentes do receiver passam: configuração válida, atacante válido, vivo, facção oposta, não é auto-hit, direção válida e sem invulnerabilidade de dash.
- A decisão de defesa ocorre antes de mutar vida ou Rigidbody2D. Resultado de resolução distingue dano aceito de parry aceito e rejeição. Invulnerabilidade de dash do defensor precede parry, e TryStartParry recusa o defensor durante esse dash.
- Parry bem-sucedido consome a janela uma vez, não altera vida nem impulso, e emite CombatParryEvent com defensor, atacante, AttackDefinition e índice do passo de combo. AttackHitbox2D é o publisher interno; AttackController2D do atacante expõe ParryOccurred, com inscrição única no enable e remoção no disable, isolando exceções de cada listener.
- AttackHitbox2D registra o receptor resolvido (somente dano ou parry aceito) na ativação antes de callbacks observáveis, impedindo duplicatas por múltiplas hurtboxes ou OnTriggerStay. Rejeições não consomem dedupe; outra ativação/passos de combo podem resolver novamente.
- CombatImpactEvent e hit stop continuam exclusivos de dano aceito. Parry não os publica nem solicita hit stop.
- O evento de parry é somente um gancho para contra-ataque/recompensa futuros. Este card não causa dano automático de contra-ataque, concede moeda/pontos, salva progresso ou introduz economia.
- Sem apresentação, cena de demonstração, regra de prioridade para vários atacantes no mesmo passo, rede/coop ou balanceamento final.

## Ciclo de vida
- Configuração ausente/inválida impede abertura; validação é explícita e não cria valor padrão oculto.
- Componente inativo, desabilitado ou morto não abre a janela. A janela é limpa ao desabilitar/destruir e não persiste entre ativações.
- Janela usa o intervalo semiaberto [0, WindowDuration): início incluído, fim excluído. Duração fracionária é válida e é arredondada para cima ao próximo passo de física; qualquer duração positiva permite ao menos uma simulação física. Após esses passos, a janela fecha no FixedUpdate seguinte, antes de outra simulação.
- O primeiro contato válido consome a janela. Contatos inválidos não consomem a janela.
- Eventos toleram listeners que falham sem interromper a resolução física, seguindo o isolamento existente nos eventos de combate.

## Segurança e acessibilidade
O card adiciona uma ação de defesa de gameplay sem bindings de plataforma. Nenhum requisito de Child Safety, privacidade ou dados pessoais é introduzido. Futuro input deverá integrar remapeamento/acessibilidade segundo os cards apropriados.

## QA e performance
Testar abertura e rejeições, antes/depois dos limites (incluindo duração menor que um fixed step), congelamento por pause/hit stop, consumo único, ausência de dano/knockback/impacto em parry, comportamento de dano fora da janela, filtros de combate, dash invulnerável, vida zero, múltiplas hurtboxes, nova ativação de combo e limpeza de lifecycle. Avaliar performance estática: resolução deve usar componentes existentes/referências locais, sem buscas por frame, alocações por contato ou corrotinas por parry. Metas de dispositivo não definidas no card permanecem abertas; não declarar medições de dispositivo.

## Fora do escopo / dependências
#24 fornece defesa de dash; #25 define combo e hit stop. Este card usa esses contratos sem alterá-los. Input e apresentação ficam para os cards correspondentes. Nenhum asset numérico padrão é criado.
