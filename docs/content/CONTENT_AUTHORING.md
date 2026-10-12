# Guia de criação de conteúdo — card #114

## Definições e identidade

Crie dados pelo menu `Create → HUNTR/X → Data` correspondente à definição tipada. IDs devem ser estáveis e exclusivos no catálogo; nomes de exibição podem mudar. Salve o asset e sua `.meta` no mesmo commit. Não copie um GUID de outro asset e não altere estado de runtime no ScriptableObject compartilhado. `IsValid(out reason)` rejeita referências ausentes e números fora do domínio; valide antes de ligar o conteúdo a uma cena.

## Personagem

Use Rumi, Mira ou Zoey como referência de composição. Um ator físico precisa de Rigidbody2D, colliders/hurtboxes, DamageReceiver2D com facção HuntrX, definição e os controladores correspondentes. O manager controla a instância ativa; entradas devem ir somente para ela. Um novo personagem exige inclusão explícita no roster e revisão de troca, morte, checkpoint, ataque, apresentação e controles. Não duplique a instância ativa sob uma raiz que reúna adversários, pois a resolução de dano usa raízes e facções.

## Inimigo e boss

Crie AttackDefinition e EnemyDefinition com dano, alcance, telegraph e recuperação legíveis. O prefab precisa de DamageReceiver Demon, EnemyAgent2D, hurtbox e exatamente um controlador principal de comportamento. Flying, rival Saja e pattern controller são modelos diferentes; não combine drivers que tickem o mesmo agente. Bosses observam vida e alteram padrões em fronteiras seguras. Assets de Gwi-Ma demonstram fases monotônicas e o evento tipado de encerramento; novos scripts narrativos consomem o evento, sem colocar diálogos no agente de combate.

## Fã, item e equipamento

Fãs usam FanActor2D e registro explícito, fora da facção Demon e do pipeline de ataques. Ajuste os limiares em FanDefinition, configure a interação em FanRescueDefinition e a drenagem em FanDrainDefinition. Ligue recompensas aceitas ao Honmoon e à performance, evitando duplicação de callbacks. Comida usa FoodDefinition, pickup e inventário limitado; escolha self/fan deve consumir só quando há benefício aceito. Peças usam EquipmentPieceDefinition e ponto narrativo estável; repetição do encontro não pode conceder o mesmo desbloqueio novamente.

## Fase e apresentação

Defina plataformas, rotas verticais, pontos de entrada, checkpoints e regiões de encontro. Use geometria de blockout com colisão antes de arte final. Posicione fãs de modo que risco e interação sejam alcançáveis, e telegraphs não fiquem escondidos pela câmera. Boss arena liga boss, fãs, Honmoon e performance por referências explícitas; nada deve depender de uma busca global em todas as cenas.

Assets de arte e áudio precisam de origem e permissão documentadas. Placeholders são identificados como provisórios. O conteúdo final deve ser revisado em todos os perfis de idade e nas opções de redução de flashes e legendas.

## Entrega

Crie branch por card a partir da develop, commit descritivo e PR com escopo, dependências, assets e evidências. Execute importação, compilação e cenário no Editor quando disponível; escreva os resultados reais. Inspecione colisões, troca/morte, cancelamentos, retry, transição e salvamento. Verifique mobile e acessibilidade quando aplicável. Uma definição criada ou um prefab que não foi aberto no Unity não conta como conteúdo validado em gameplay.
