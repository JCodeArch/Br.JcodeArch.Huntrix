# EnemyLab — protótipo de encontro

Abra `Assets/Scenes/EnemyLab.unity` no Unity e execute Play na fase de validação autorizada. A cena mantém a câmera, luz global e host de bootstrap do Combat Lab, e compõe `GroundHordeEncounter_Prototype`: personagem inicial, fluxo de tentativa, piso, pontos de spawn e horda com alvo explícito. O bootstrap inicia o personagem e a horda automaticamente; não é necessário escrever código para fornecer o alvo.

O encontro padrão demonstra a horda terrestre. Para inspecionar variantes, selecione o objeto de horda dentro da instância do encounter e atribua outro `HordeDefinition` no Inspector. Prefabs de elite, miniboss, Saja e Jinu podem ser usados como integrantes de uma definição de horda válida. A horda registra o alvo e acompanha morte/troca do personagem pelo manager. Essas alterações de Inspector são preparação de conteúdo, não alegação de cenas adicionais já integradas.

Os inimigos usam silhuetas geométricas originais; amarelo indica telegráfico, branco indica ataque e cinza indica morte. O personagem inicial é um protótipo lógico sem SpriteRenderer: o alvo não tem silhueta visível nesta entrega; seu estado pode ser inspecionado no Inspector. A cena não adiciona controles físicos, arte final, HUD, câmera dinâmica, cooperação ou rede. Sem mapeamento físico, o personagem inicial serve de alvo estático para observar aproximação, ataques e respawn automático.

A cena é uma composição authored revisável. Compilação, importação de prefab/scene, execução do encontro e aparência ainda precisam de validação Unity final; não foram executadas nesta rodada. Abrir a cena diretamente no Editor não exige alterar a lista de cenas de build, que permanece fora desta entrega.
