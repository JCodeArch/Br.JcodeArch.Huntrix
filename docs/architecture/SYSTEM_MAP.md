# Guia da arquitetura — card #113

`HuntrX.Runtime` contém contratos, dados e runtime; ferramentas de Editor e testes usam assemblies próprios. Definições tipadas são dados autorados, enquanto componentes possuem estado por instância. Catálogos e prefabs usam GUIDs versionados com `.meta`.

| Responsabilidade | Componentes e fronteira |
| --- | --- |
| Controle local | CharacterManager2D mantém atores e vida por personagem, troca o ativo e coordena respawn/checkpoint; entrada física chama controles lógicos |
| Combate | DamageReceiver2D aceita dano e recuperação; hurtboxes encaminham hits; parry, dash e proteção são respeitados pela resolução comum |
| Inimigos | EnemyAgent2D possui estado e alvo explícito; um driver principal decide comportamento; patterns e observadores de fase especializam bosses |
| Encontros | HordeController2D possui instâncias, limites e ciclo de cancelamento; bootstrap configura alvo, roster e spawnpoints |
| Fãs | FanActor2D guarda integridade e estado; FanRegistry2D limita alvos; rescue/drain publicam resultados aceitos |
| Recurso global | HonmoonController2D publica faixas e crise; adapters convertem resgate/drenagem; crise modifica a horda por fonte |
| Special | Controller reserva carga, escolhe Demon e executa etapas ordenadas; apresentação consome eventos, sem atingir fãs |
| Recursos e peças | FoodInventory2D reserva uso e confirma benefício; EquipmentProgress2D desbloqueia por narrativa e EquipmentMechanics2D muda mecânicas |
| Performance e replay | Impactos aceitos e resgates alimentam uma execução; sumários por perfil opaco são limitados, validados e locais |
| Níveis e fluxo | Geometria e pontos autorados alimentam builders; mudança de fase mantém HP e ciclo de execução explícito |
| Apresentação e políticas | Áudio, diálogos, feedback visual, idade e acessibilidade consomem estado/eventos; assets não armazenam progresso do jogador |

## Regras de dependência

Prefira referências explícitas e coleções limitadas; não faça `FindObjectsOfType` em loops. Cada evento deve declarar quando o estado já está comprometido. Callbacks podem desabilitar o componente, trocar o alvo, iniciar outro ciclo ou lançar exceção; use reserva e época de execução para evitar efeitos antigos num ciclo novo. Remova assinatura em disable e reconcilie mortes perdidas ao voltar. Não repita dano, recompensa ou encerramento depois de um callback de transição.

Raízes e facções importam: inimigos são raízes próprias, fãs não são DamageReceiver Demon, e personagens alternáveis pertencem ao roster explícito. Dados inválidos ou buffers saturados falham sem aplicar efeitos parciais. Limites de horda, registros e rankings evitam crescimento ilimitado; desempenho precisa de medição real, não da simples presença de limites.

## Persistência e migração

Separe perfil, campanha, checkpoints, peças e resultados de replay. IDs opacos não precisam de dados pessoais. Resumos de ranking local não devem apagar progresso da campanha. Validar esquema e integridade antes de substituir dados, preservar backup e registrar corrupção sem conceder progresso indevido. Novas versões exigem estratégia de migração; o contrato anterior está em `SAVE_AND_PROFILES.md`.

## Validação e release

Revisão estática verifica contratos, ciclos de vida e referências; Editor comprova importação e compilação; PlayMode e playtest verificam comportamento; dispositivos medem performance e interrupções. Consulte `../governance/DEFERRED_VALIDATION.md`: merges autorizados com execução adiada não qualificam builds ou release. Consulte os documentos específicos de cada sistema nesta pasta e os relatórios em `../reviews/` para limites conhecidos.
