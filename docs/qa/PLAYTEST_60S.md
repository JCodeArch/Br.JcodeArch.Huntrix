# Playtest de 60 segundos — protocolo e métricas

**Card de origem:** [05 — Definir métricas de diversão](https://trello.com/c/DEkBR4PB/5-05-definir-m%C3%A9tricas-de-divers%C3%A3o)  
**Estado:** protocolo definido; resultados ainda não coletados.  
**Uso:** triagem formativa do primeiro minuto jogável antes de expandir a produção; não é prova estatística de diversão nem valida o jogo completo.

## Objetivo

Encontrar atritos evidentes de controle e leitura, verificar se ações principais produzem resposta compreensível e avaliar se o minuto inicial desperta vontade de continuar. O playtest observa somente sistemas e eventos presentes na build testada. Não define mecânicas, balanceamento, arquitetura de rede ou metas técnicas ainda abertas.

## Preparação da sessão

Antes de cada rodada, registrar:

- código anônimo da sessão, versão da build, plataforma/dispositivo, método de entrada, modo (solo/coop), número de jogadores e trecho testado;
- o mesmo estado/checkpoint inicial, uma tarefa ou uma lista ordenada de tarefas, prompt literal específico de cada tarefa, critério observável de conclusão e regra de início/fim;
- eventos que o observador poderá registrar e qualquer instrumentação disponível. Latência só pode ser registrada em milissegundos se a build a medir; observação visual é anotação qualitativa, não uma medida de latência.

Antes do cronômetro, leia literalmente: “Você vai jogar por 60 segundos. Use as instruções e controles que aparecerem no jogo. Jogue da forma que achar melhor; eu não darei dicas durante o minuto, e você pode parar a qualquer momento.” Entregue então o controle e posicione a build no estado inicial comum. Quaisquer prompts específicos de tarefa devem estar definidos literalmente antes e ser apresentados da mesma forma a todos na configuração; não fornecer dicas durante a rodada. O cronômetro começa no primeiro frame de gameplay após instruções e pré-posicionamento e termina em 60 segundos; onboarding/tutorial sob avaliação exige segmento separado, definido antes da sessão. Registre conclusão e evidência até o corte; eventos posteriores ficam separados e não contam para a taxa de 60 segundos. Se houver interrupção, falha técnica ou instrução não padronizada, registrar o desvio e classificar a sessão como inválida para a análise principal, preservando-a no relatório com motivo. Não repetir a rodada sem registrar a tentativa anterior.

Usar apenas tarefas realizáveis no trecho (por exemplo, mover-se, reagir a uma ameaça ou interagir com um resgate se esses elementos existirem). Antes de cada rodada, registrar versão da build, estado inicial, se há uma ou várias tarefas, ordem fixa, prompt exato lido para cada tarefa e evidência verificável de conclusão; não pontuar sistemas ausentes. Não trocar tarefas ou ordem entre participantes da mesma configuração sem abrir uma configuração/rodada identificada separadamente.

Testar cada combinação relevante de plataforma/entrada separadamente. Resultados de teclado, gamepad e touch não são agregados sem identificação. Resultados solo e coop também são separados; se a cooperação ainda não estiver implementada ou se local/online não estiver decidido, registrar essa limitação sem concluir que o requisito foi validado.

## Participantes e privacidade

O piloto inicial usa voluntários adultos (18+) sem experiência prévia com a build. Crianças estão excluídas desta versão do protocolo e nenhum dado infantil será coletado. Registrar consentimento informado sem armazenar identidade desnecessária. Estudos posteriores com menores exigem autorização do responsável, assentimento apropriado à idade, supervisão e revisão das salvaguardas aplicáveis; se isso não estiver pronto, não executar nem declarar validação infantil.

Usar códigos em vez de nomes. Coletar apenas medidas necessárias ao teste; não coletar contato, conta, localização precisa ou outros identificadores. Gravação de tela, voz ou imagem é opcional, precisa de aviso e consentimento separado e não é requisito do protocolo. Definir acesso restrito e prazo de retenção/descarte antes de qualquer gravação.

## Ficha de observação

Separar evidências em três classes e registrar uma linha por sessão:

- **Telemetria objetiva instrumentada:** valores produzidos por instrumentação da build, com definição, unidade e timestamps. Latência numérica só com timestamps de entrada/resposta instrumentados.
- **Observação comportamental codificada:** eventos interpretados por observador, com timestamp, definição operacional e contexto. Quando viável, dois observadores codificam uma amostra e alinham divergências; se houver um único observador, declarar essa limitação. Não apresentar essas observações como telemetria objetiva.
- **Autorrelato:** respostas e notas do participante, subjetivas, identificadas por pergunta e número de respondentes.

Não calcular uma pontuação composta de “Game Feel”. A ficha comportamental deve explicitar:

| Medida | Registro |
|---|---|
| Primeira ação intencional (observação codificada) | Timestamp até a primeira ação deliberada que o protocolo define como pertinente à tarefa; descrever a ação e contexto. |
| Execução das tarefas (observação codificada) | Concluída/não concluída por tarefa, sem ajuda verbal; registrar tentativa, timestamp e evidência definida antes da sessão. |
| Atrito de controle | Comando não intencional, repetido ou corretivo observado; timestamp e contexto, sem inferir a causa. |
| Leitura | Alvo, ameaça, feedback ou resultado interpretado incorretamente, somente quando presente; anotar o que ocorreu. |
| Fluxo | Pausas, hesitação, falha, reinício, abandono ou interrupção técnica, com timestamp e contexto. |
| Resposta técnica | Registrar evento/latência apenas se houver instrumentação; caso contrário, escrever observação qualitativa, sem número inventado. |

Após o minuto, perguntar sempre na mesma ordem:

1. “O que você achou que precisava fazer?”
2. “Quão claros e previsíveis foram os controles?” (nota de 1 a 5; 1 = nada claros, 3 = mistos, 5 = muito claros.)
3. “Quão responsivas pareceram as ações?” (nota de 1 a 5; 1 = nada responsivas, 3 = razoáveis, 5 = muito responsivas.)
4. “Quão satisfatório foi o impacto/feedback das ações de combate?” (nota de 1 a 5; 1 = nada satisfatório, 3 = razoável, 5 = muito satisfatório; marcar N/A se não houve combate no trecho. Esta é a única pergunta que admite N/A.)
5. “Quanto você gostou desse minuto?” (nota de 1 a 5; 1 = nada, 3 = razoável, 5 = muito.)
6. “Você gostaria de continuar jogando agora?” (sim / talvez / não.)
7. “O que foi mais satisfatório? O que causou mais dificuldade?”

Se fãs/proteção, eventos de espetáculo ou mudança musical aparecerem no trecho, registrar espontaneamente o que o participante percebeu. Não sugerir uma resposta nem considerar a ausência de comentário uma falha automática. Música é avaliada como reforço de clima e feedback; não se exige timing musical, sincronização ou habilidade rítmica.

## Alvos provisórios do piloto

Realizar primeiro um piloto com **pelo menos 5 participantes adultos por configuração testada**. Com esse tamanho, os limiares abaixo são sinais práticos para decidir o próximo ajuste, não inferência estatística sobre a audiência.

- **Compreensão/execução:** alvo de pelo menos 80% das sessões válidas completando cada tarefa principal pré-listada em até 60 segundos, sem dica do facilitador (para n=5, isso exige ao menos 4 sucessos). Aplicar a regra de contagem sem arredondar descrita abaixo.
- **Game Feel percebido:** mediana de pelo menos 4/5 nas notas de clareza de controle, resposta, impacto e diversão, reportadas separadamente; não criar uma média composta.
- **Intenção de continuar:** alvo de pelo menos 80% dos respondentes válidos marcando “sim” (para n=5, exige 4); “talvez” e “não” permanecem discriminados no relatório.
- **Atrito recorrente:** o mesmo problema observável em 2 ou mais sessões exige revisão antes de expandir produção, ainda que as notas agregadas alcancem os alvos.
- Qualquer avaliação 1–2/5 deve ser lida junto da dificuldade narrada e observada. Falhas técnicas, desistências e sessões inválidas são reportadas com seus denominadores e motivos, nunca removidas silenciosamente.

Estes são alvos provisórios propostos para o primeiro piloto, sem resultados coletados. São gatilhos para investigar e iterar; não são critérios de aprovação do jogo nem alegações validadas de diversão. Se build, tarefas ou experiência dos participantes tornarem um alvo inadequado, registrar a razão e revisar a hipótese antes de coletar dados; não alterar o alvo após ver os resultados para declarar aprovação.

Para cada configuração, apresentar `N_attempted`, `N_valid` e `N_invalid`, com `N_attempted = N_valid + N_invalid`; se houver sessão pendente/sem desfecho, listá-la separadamente e reconciliar a contagem. Preservar todas as tentativas inválidas e seus motivos. A taxa de tarefa é `instâncias de tarefa concluídas / instâncias de tarefa-participante válidas`, por tarefa, junto à contagem de participantes que concluíram. Para alvos percentuais, não arredondar contagens: 80% exige `ceil(0,8 × denominador válido daquele alvo)` sucessos. Para tarefas, o denominador é o número de instâncias válidas tarefa-participante; para intenção de continuar, são os respondentes válidos. Uma configuração com menos de cinco sessões válidas não atinge os alvos deste piloto. Para cada pergunta, mostrar n válido, respostas ausentes e distribuição/mediana; mostrar sim/talvez/não sobre respondentes, com denominador explícito. Não combinar configurações.

## Decisão após o teste

Antes de expandir produção, comparar resultados por configuração e responder: (a) o que o participante entendeu; (b) qual atrito se repetiu; (c) quais notas e comportamentos sustentam a conclusão; (d) que mudança será testada na próxima iteração. Alvo não atingido significa investigar e iterar, não descartar um requisito de produto.

Um resultado solo não valida cooperação. Depois de definida a modalidade e existir build cooperativa, repetir sessões separadas nos modos relevantes até três jogadores; não misturar esses resultados com solo nem afirmar que coop local/online foi testada se não foi. O protocolo não valida por si só balanceamento, retenção, dificuldade dinâmica, perfis etários, acessibilidade, rede ou público amplo; esses itens seguem seus cards de QA.

## Modelo de relatório

Para cada rodada de playtest, registrar em seção/arquivo de resultados separado: data, build, configuração, estado inicial e prompts/tarefas pré-registrados, n planejado, `N_attempted`/`N_valid`/`N_invalid` e motivos, observações comportamentais sem identificadores, telemetria objetiva (se houver), distribuição e ausências dos autorrelatos, contagem/denominador dos alvos, desvios, resultado **atingido / não atingido / inconclusivo** por alvo e próxima decisão. Até haver rodada real, os resultados permanecem **não coletados**; a aprovação deste protocolo não significa que o jogo passou no teste.

