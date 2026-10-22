# Validação cooperativa — primeira fase

## Correções verificadas

O tableau de heroínas sentadas foi removido da apresentação quando o controle jogável começa (ee8fffc). Novo WindowsRelease desse commit compilou com0erros/0avisos;611869024bytes e hash do manifesto39d31237c961d42f0a83b4a3cca7a3ea9be8ba55dd55eb39284c5fa4f2bee757. Inspeção real em D3D11 confirmou introHP100/100 e ausência da mesa após conclusão explícita do diálogo. Evidência local: outputs/hd-validation/tableau-fixed-20261020/combat-tableau-removed.png. Isso não aprova proporções nem toda a partida.

Os spawns terrestres x8/x22 foram elevados para1.55/1.7; o aéreo x23 foi trazido de y5 para3.5, abaixo do teto; checkpointx33 passou a1.5; guarda extra x36 passou a1.5. Não foram alterados colliders ou regras de combate.

Reprodução antes da correção:

- airplane-spawn-red.xml:3/3 falhas por sobreposição de terrestre, aéreo e checkpoint com sólidos.
- airplane-extra-red.xml:1/1 falha identifica FlightCrew_MagentaStewardess_CockpitGuard dentroPlatform_3 em(36,.8).

Verificação após correção:

- airplane-spawn-green-EditMode.xml:123/123 passaram.
- airplane-extra-green-full.xml:536/536 PlayMode passaram.
- Nenhum warning/error C# encontrado nos dois logs finais.
- Revisão independente de gameplay sem bloqueador neste escopo. Envelopes conservadores usam colliders reais; teste runtime inclui extras antes de um passo físico resolver interpenetrações.

## Trabalho cooperativo

## Capitão com comportamento de miniboss

O campo explícito captainEnemy nas duas cenas de slice aponta para Miniboss_Prototype. Apenas o último guarda extra emx42.5 usa esse prefab; os demais continuam terrestres comuns. O Capitão agora tem350HP e controlador de padrões com mudança de fase a50% de vida; sua identidade visual já reconhece o nomeCaptainDokkaebi. Continua registrado na sessão para troca de alvo, limpeza e contagem de inimigos necessários à saída.

RED1/1 em airplane-captain-red.xml confirmou ausência do controlador no guarda antigo. GREEN completo537/537 em airplane-captain-green-full.xml, exit0. Revisão independente sem bloqueador introduzido. Isto não comprova arena, balanceamento, antecipação visual ou vitória em partida real. O loop dos extras ainda ignora uma eventual falha de spawn; tratar explicitamente na consolidação do encontro.

Antigravity propôs aproveitar CharacterSpritePlayback2D e retirar seleção/normalização concorrentes dos apresentadores. Execução solicitada em sua cópia D:, com patch seletivo e testes antes de integrar no clone validado. Revisão preventiva enviada: ápice aéreo não pode virarIdle por velocity.zero, animação deve acompanhar fases reais do combate e respeitar pausa, takeover/unbind deve restaurar o presenter anterior. Integração ainda não aceita neste registro.

## Pendências que esta validação não encerra

- fanSpawns e composição do resgate; câmera/limites/encontros; chefe real; reinício previsível e partida completa.
- Escala corporal e integração dos masters/quadros de animação.
- Novo executável contendo os ajustes de spawn ainda precisa de inspeção; o executável acima contém somente a correção do tableau.
- Android em dispositivo real e inicialização Windows padrão sem argumento de D3D11.
- Reconciliação da base original modificada com os commits seletivos do clone. Nenhuma cópia integral ou publicação ampla foi feita.
