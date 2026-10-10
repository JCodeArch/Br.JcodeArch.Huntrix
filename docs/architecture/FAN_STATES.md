# Fãs e estados — cartão #42

Fonte: [Trello #42](https://trello.com/c/YDxtK7wQ).

Fonte: backlog #42, estados Normal, Drained, Critical e Corrupted. `FanActor2D` possui alma limitada pela `FanDefinition`, independente de facções e de `DamageReceiver2D`: Special e ataques genéricos de combate não causam dano a fãs. Dreno de alma deve chamar sua API explícita, não adicionar um receiver de combate ao prefab.

Alma no máximo corresponde a Normal; abaixo do máximo a Drained; até a fração crítica a Critical; zero a Corrupted. `TryDrain` respeita fontes explícitas de proteção e `TryRecover` permite recuperação de Corrupted sem conceito de morte/revive. Ambos recusam valores inválidos e reentrada, limitam ao intervalo e notificam mudanças após commit de estado. `FanRegistry2D` contém no máximo 64 referências explícitas, registradas via referência serializada no ator; não descobre fãs globalmente. Desabilitar somente o registry preserva inscrições para retorno, enquanto consumidores devem verificar seu estado habilitado.

`FanContentIntensity` expõe Gentle/Standard/Intense e notificação de apresentação. O visual geométrico muda a paleta, usando Gentle por padrão; isso é apenas um seam de intensidade, sem afirmar que os perfis/regras completos dos cartões #85–91 já existem. A semântica de gameplay é a mesma. Fontes de proteção são opt-in e precisam responder `Covers` sem modificar registros durante a consulta.

Assets: `Fan_Prototype.asset`, `Fan_Prototype.prefab` e `FanGroup_Prototype.prefab`. O grupo fornece registry e três fãs com inscrições explícitas. Fãs são estáticos e sem colliders/physics, favorecendo separação do combate. O integrador deve posicionar o grupo e fornecer referências a seus consumidores.

Status: **Implementado — validação pendente**. Valores 100 de alma e 30% crítico são provisórios. Pendentes Unity: limites/transições, proteção/fontes, reentrada, recuperação exata no máximo, lifecycle do registry e leitura visual/acessibilidade.
