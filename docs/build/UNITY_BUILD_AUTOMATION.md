# Unity Build Automation — continuidade

Registro de configuração observado em **12 de outubro de 2026, horário de São Paulo**. A infraestrutura online foi configurada para executar Unity, testes e um APK de desenvolvimento sem instalar o Editor na máquina do usuário. Este documento registra a configuração e a execução solicitada; não certifica um resultado.

## Execução registrada

**Build #1: Sent to Builder, ainda sem iniciar, observado às 21:11 de 12 de outubro de 2026 (São Paulo).** Fonte solicitada/fixada no pedido: `1a907af9d68fb586f0740821337077f993917d16`, branch `develop`, alvo `Huntrix-Android-develop`. O campo de commit resolvido ainda não estava preenchido; o checkout não foi confirmado. A interface mostrava espera de 00:01:18 e checkout de 0, indicadores de fila, não evidência de execução ou desempenho.

[Histórico/log do build #1](https://cloud.unity.com/organizations/4674262819723/projects/acc5ceb5-588f-4ef7-9b7b-3e50d3139a49/cloud-build/buildtargets/huntrix-android-develop/builds/1/log).

Compilação, importação, testes EditMode/PlayMode e geração do APK ainda **não têm resultado confirmado** nesta fotografia. O log dizia: “Build has been sent to a builder but has not yet started”. Esse estado não significa que o Editor iniciou. Não há aprovação de build, QA humano, dispositivo ou desempenho.

## Configuração confirmada

| Campo | Valor observado |
|---|---|
| Organização Unity | `4674262819723` |
| Projeto Unity | `acc5ceb5-588f-4ef7-9b7b-3e50d3139a49` |
| Build target | `Huntrix-Android-develop` |
| Repositório | `JCodeArch/Br.JcodeArch.Huntrix`, conexão SSH confirmada |
| Branch | `develop` |
| Editor exato | `6000.6.4f1` |
| Sistema da máquina de build | Windows 11 24H2 |
| Máquina | Windows Micro, elegível ao gratuito; 8 vCPU, 16 GB RAM, 512 GB disco exibidos |
| Plataforma/SDK | Android / SDK 35 |
| Application ID | `com.jcodearch.huntrx` |
| Assinatura | Keystore de debug gerado automaticamente |
| Saída | APK de desenvolvimento; AAB desativado |
| Cena | `Assets/Scenes/VerticalSlice.unity` |
| Testes | EditMode e PlayMode habilitados; build configurado para falhar se algum teste falhar |
| Agendamento | Desativado |
| Autobuild | Desativado |

Os recursos de máquina acima são configuração exibida, sem medição ou promessa de throughput/FPS. A execução é manual; esta configuração não autoriza disparos recorrentes nem publicação em loja. APK de desenvolvimento com assinatura de debug não é release candidate.

## Acesso ao código e resolução da conexão

A primeira tentativa SSH apresentou falha genérica. A conexão passou após a configuração de uma **deploy key somente leitura**, confirmada no GitHub como ID `166248292`. A chave permite obter o código para o build; seu conteúdo público/privado não é reproduzido neste documento.

O repositório foi observado como **Public** nesta sessão. Essa visibilidade é um estado externo observado; o time não alterou a visibilidade do repositório como parte desta configuração.

## Franquias e cobrança observadas

A interface confirmou as franquias gratuitas exibidas: **200 minutos Windows Micro, 100 minutos Mac, 100 minutos Linux, 25 GB de armazenamento e 100 GB de egress**. São valores observados nesta sessão, não garantia permanente de disponibilidade ou de termos futuros. Nenhum plano pago foi contratado e nenhum cartão de crédito foi informado pelo time.

## Continuidade e evidência necessária

Ao retomar, abrir o log acima e registrar o estado real do build #1 antes de solicitar outra execução. A orquestração acompanha a fila e atualizará este documento com o resultado observado antes de sua integração.

Para afirmar sucesso, preservar o resultado do Editor/importação/compilação, resultados reais EditMode e PlayMode, identificação do APK e sua associação ao commit efetivamente resolvido, verificando se corresponde ao hash solicitado. Se houver falha, registrar a etapa e o trecho pertinente do log, corrigir em branch própria e documentar a nova fonte/execução. A configuração de testes não comprova que testes foram iniciados, descobertos ou aprovados.

Instalação e teste em dispositivo, percurso da slice, avaliação humana, métricas de performance, qualificação RC, direitos e publicação continuam pendentes mesmo se o build terminar com sucesso. Ver também [pipelines do repositório](UNITY_PIPELINES.md), [estado da implementação](../IMPLEMENTATION_STATUS.md) e [gate RC](../qa/RELEASE_CANDIDATE_GATE.md).
