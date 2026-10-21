# Integração cooperativa de animações — 21/10/2026

Base: f066798, branch feature/hd-presentation-recovery, clone isolado. Lote Antigravity importado seletivamente: CharacterArtRenderer2D, CharacterGameplayAnimator2D, SlicePlayerPresentation2D e CharacterAnimationIntegrationPlayModeTests. Os três preimages do patch eram idênticos às fontes locais; a cópia não substituiu mudanças locais nesses arquivos. O lote original permanece em D: para comparação.

## Evidência e correções

- integration-bind-red.xml: 5 testes, 3 passaram, 2 falharam. Catálogo válido somente com Run, sem Idle, era rejeitado pelo playback mas alterava catálogo/escala publicados nos hosts.
- integration-bind-green-lifetime-red.xml: 6 testes, 5 passaram, 1 falhou. Os testes de bind passaram após confirmar o playback antes de publicar o novo estado. O teste de lifetime demonstrou catálogo antigo sobrevivendo a rebind.
- A lista de recursos do ator agora é separada dos VFX persistentes; rebind/unbind liberam catálogos/sprites anteriores e snapshots de ghosts, preservando texturas Resources e VFX reutilizáveis.
- Reflection da telemetria de grounded/ataque substituída por getters somente leitura. Não houve mudança na autoridade física ou de hits.
- animation-integration-full-green.xml (nome da tentativa, não resultado): 543 testes, 541 passaram e 2 falharam; nenhum warning/error C# registrado. Falhas antigas de sincronização de teste: transferência comparada com posição anterior ao passo físico; HUD consultado durante recomeço da introdução. Correções limitadas ao fixture: capturar estado imediatamente antes da troca e completar o diálogo da nova composição como no setup inicial.
- foot-alignment-red.xml: 4 testes, todos falharam: dois confirmaram foot pivot worldY7 quando o collider terminava em6.3; dois repetiram as falhas antigas acima. Correção de pés usa bounds do collider e conversão para o parent do sprite, sem alterar física/escala.

A ausência de warnings no log captain GREEN anterior não provava recompilação limpa dos testes: o Csc desse assembly não foi executado ali. A recompilação desta integração revelou quatro CS0618 preexistentes em duas chamadas FindObjectsByType com enum obsoleto. Atualizadas para o overload sem ordenação, preservando semântica de None.

## Limites de aceitação

Ainda não aprovado como integração final. Aguardar regressões finais, EditMode, executável e inspeção real. Não há evidência de velocidade renderizada duplicada: o problema do relógio sob revisão é avanço transitório no Update seguido de reamostragem no LateUpdate. Fábrica runtime de catálogo, rejeição de canvas incompatível, padding dos sprites legados e inspeção da nova Rumi ainda pendentes.

Prova Rumi Idle V2: PNG intacto, geometria aprovada pelo especialista, layout uniforme 448×744 com origens calibradas registrado no JSON da prova; ritmo e leitura em movimento não aprovados ainda.

## Verificação final da base integrada

- clock-ownership-red.xml: 3 testes, 2 passaram; probe de callbacks reais detectou 6 avanços transitórios do relógio do ataque (maior desvio 0,0301172s). Ataque agora usa SampleStateAt absoluto, suspendendo Tick/Update; SetState retoma automático nas demais poses. Teste adicional cobre entradas inválidas, retomada no mesmo estado e rebind.
- animation-reviewed-final-PlayMode.xml: 547 testes, 545 passaram; dois testes novos de pés emitiam erro de configuração porque criavam DamageReceiver ativo antes de atribuir saúde. Corrigida fixture inativa antes de Awake, sem ocultar logs inesperados.
- animation-reviewed-final2-PlayMode.xml: **547/547 passaram**, exit0.
- animation-reviewed-final-EditMode.xml: **123/123 passaram**, exit0.
- Ambos logs finais Unity sem warning/error C#. Revisão independente de bind/lifetime/telemetry/foot não identificou bloqueador novo.

Compilação independente com SDK8.0.318 já embarcado no Unity: todas as quatro assemblies compilam. O build bruto registra206 CS0649 no Runtime (também repetidos ao recompilar referências): campos serializados preenchidos pelo Unity. As201 localizações únicas examinadas têm SerializeField. Não declarar build bruto sem avisos. O comando repetido com a mesma política de diagnósticos do compilador Unity (RSP existente: nowarn0169/0649/0282/1701/1702, maisUSG0001 já no csproj) teve **0erros/0avisos nas quatro assemblies**. Logs brutos e equivalentes preservados; nenhuma supressão adicionada ao código ou ao projeto. A primeira tentativa --no-restore falhou por project.assets.json ausente em Temp; restore normal resolveu.

A integração de código passou nas regressões; aprovação visual e de jogabilidade continua pendente. Importador da Rumi delegado por mensagem nativa ao Antigravity em21/10, limitado a arquivos novos Editor/testes/pasta da prova, PNG intacto e catálogo apenas Idle. Câmera solicitada como proposta antes de implementação.
