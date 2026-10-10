# Card #53 — efeitos mecânicos das peças

As peças alteram ações de gameplay sem modificar definições compartilhadas:

- **AirStepBoots:** após contato real com piso, permite um impulso vertical adicional quando no ar, uma vez antes de pousar novamente. Não executa no chão, em dash/parry/combo ativo ou com personagem morta/desativada.
- **ResonanceBracelet:** ação lógica causa explosão circular limitada, com cooldown, deduplicação por receiver, linha de visão e pipeline `Hurtbox2D.ResolveHit`. Preserva facção, parry e proteção. Saturação de buffers de overlap/raycast rejeita conservadoramente.

`EquipmentMechanics2D` aceita somente peça válida liberada na progressão explicitamente vinculada. Nenhuma compra ou equipamento não adquirido gera habilidade. Bind de outra progressão remove a peça equipada; trocar peças não zera cooldown nem restitui o passo aéreo gasto. Desativação limpa contatos e exige novo pouso, preservando cooldown pendente. Notificações isoladas são protegidas contra troca/equip reentrantes durante a habilidade.

`RumiEquipment_Prototype` compõe física e kit real da Rumi, progressão, mecânica e `EquipmentPrototypeBinding2D`. O binding demonstrativo cria um perfil efêmero opaco, sem recompensa ao iniciar. Os story points desbloqueiam as peças por colisão e a última peça adquirida fica equipada. API `TryEquip` permite seleção futura; `TryUseAbility` requer binding lógico futuro, sem controles físicos adicionados. Perfil definitivo deve ser fornecido pela composição de save/slot, sem confundir esse perfil efêmero com progresso persistido.

Arte, animação, áudio, valores, marcos narrativos e balanceamento definitivos permanecem abertos. Não há coop qualificada, persistência ou medição de plataforma. Validar no Unity final: pouso e impulso extra, morte/respawn, troca de peças/perfis, callbacks, obstáculos, múltiplos colliders, saturação, facção, parry/proteção e preservação do kit Rumi/Mira/Zoey.
