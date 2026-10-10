# Definition of Done

Esta definição aplica-se a cada card de implementação, conteúdo, documentação, QA ou release do HUNTR/X. O escopo do card e suas dependências continuam sendo a autoridade; este documento define o que precisa estar comprovado antes de marcá-lo como concluído. Os gates abaixo são avaliados conforme o tipo e o escopo do card; um gate não aplicável exige justificativa registrada. QA, rastreabilidade Trello e integração verificável são obrigatórios em todos os cards.

## Exceção explícita do proprietário — 10/10/2026

O proprietário autorizou adiar a execução dos testes Unity até o jogo estar quase pronto e continuar o desenvolvimento em paralelo. Durante esse período, mudanças podem integrar a `develop` com revisão estática sem findings bloqueadores, documentação e rastreabilidade verificadas, usando o estado **Implementado — validação pendente**. Esse estado permite avançar dependências, mas não equivale à conclusão integral do card nem à qualificação de release.

Os gates de compilação, importação, testes e verificação runtime permanecem pendentes até execução real. Não registrar testes como aprovados nem usar **N/A** para justificar o adiamento. Conclusão e qualificação de release exigem satisfazer os gates aplicáveis abaixo. Segurança infantil, privacidade e direitos/licenciamento não são dispensados. O registro de decisão, riscos e validação final está em [DEFERRED_VALIDATION.md](DEFERRED_VALIDATION.md).

## Gates comuns

Um card só está concluído quando:

1. **Escopo e aceite:** o resultado está ligado ao card e aos requisitos aprovados; critérios observáveis de aceite foram satisfeitos; decisões ainda abertas continuam identificadas como abertas. Trabalho fora do escopo não foi incluído silenciosamente.
2. **Arquitetura e dependências (quando aplicável):** os limites, contratos, estados, ciclo de vida e dependências relevantes foram revisados antes da implementação. A solução segue a arquitetura aprovada e não cria abstrações genéricas sem necessidade demonstrada.
3. **Qualidade estrutural (quando aplicável):** a revisão SOLID verifica responsabilidade, coesão, acoplamento e extensão no contexto real. O veredito do auditor e os findings/ações são registrados conforme o contrato do agente.
4. **Performance e plataforma (quando aplicável):** o impacto é avaliado nas plataformas/dispositivos definidos pelo escopo aprovado do card; alvos ainda abertos permanecem abertos. Quando houver meta aprovada, a evidência de medição atende ao orçamento. Não se declara uma meta como atendida sem medição. Se a meta ou o dispositivo ainda estiver aberto no backlog, o risco, a evidência disponível e o gate para medir ficam registrados.
5. **QA e regressão:** os critérios de aceite são verificados com a forma de QA adequada ao tipo de card. Mudanças executáveis têm testes automatizados apropriados e os testes relevantes passam; a cobertura de regressão é proporcional ao risco. Em card exclusivamente documental, QA valida consistência, rastreabilidade, links e lacunas, sem inventar critérios de runtime.
6. **Segurança, idade e acessibilidade:** requisitos aplicáveis de Child Safety, privacidade, adaptação etária e acessibilidade foram revisados. Uma exceção não pode dispensar requisitos de segurança infantil, privacidade ou direitos/licenciamento.
7. **Documentação:** instruções, contratos de dados, decisões, riscos e limitações relevantes foram atualizados junto da mudança.
8. **Integração:** a mudança real está commitada no repositório. Cada card possui seu próprio commit identificável; arquivos e branch de destino foram verificados. O card Trello contém resultado, evidência do commit e revisão dos agentes aplicáveis.
9. **Estado final:** a checklist DoD do card está completa, nenhum finding marcado como bloqueador permanece sem resolução, e a integração foi verificada antes de mover o card para “Concluído”.

## Gates por tipo de trabalho

### Gameplay e sistemas de runtime

- O comportamento corresponde aos critérios do card em condições normais, limites e transições relevantes.
- Inputs, estados, eventos, feedback e efeitos colaterais têm contratos explícitos; dependências entre gameplay, áudio, UI, save e cenas seguem limites aprovados.
- Sistemas que envolvam personagens, fãs, Honmoon, Special ou cooperação respeitam os requisitos já aprovados, incluindo até três jogadores/personagens simultâneos e experiência solo quando aplicável. Modalidade de rede e regras/valores ainda abertos não são presumidos.
- Testes unitários, de integração, EditMode ou PlayMode são selecionados conforme o comportamento alterado; testes de regressão relacionados passam.

### Conteúdo, arte e áudio

- Assets seguem a direção visual/sonora, formatos, naming, consistência e licenças aprovados.
- Referências e dependências do conteúdo estão válidas; importação e uso no alvo do card foram verificados.
- Conteúdo sujeito a idade, Child Safety, música ou propriedade intelectual passa pelas revisões aplicáveis antes de distribuição.

### UI, plataforma e acessibilidade

- O comportamento foi verificado nos dispositivos, proporções de tela e métodos de entrada cobertos pelo card.
- Opções e requisitos de acessibilidade afetados foram testados; restrições mobile relevantes foram avaliadas com evidência.
- Plataformas ou dispositivos ainda não suportados permanecem explicitamente fora do aceite daquele card, sem serem removidos do escopo geral do produto.

### Documentação e decisões de design

- Fontes e cards relacionados estão identificados; fatos aprovados, recomendações e decisões em aberto estão claramente separados.
- A documentação não antecipa números, regras, plataformas, direitos ou arquitetura que o backlog ainda não decidiu.
- Qualquer gate técnico ou de domínio fora do escopo do card pode ser marcado **N/A** com justificativa explícita, risco e evidência alternativa, além da identificação do gate/agente responsável. Revisão documental de QA e integração continuam obrigatórias.

## Tratamento de findings e exceções

- Um veredito **REJECTED** de auditor bloqueia a conclusão até correção e nova revisão. Findings não bloqueadores podem permanecer somente quando registrados no card com prioridade indicada pelo agente, impacto, próximo passo/responsável e aceitação. Este documento não cria uma escala de severidade entre agentes.
- Toda exceção a teste, medição ou etapa de agente registra motivo, risco e evidência alternativa; ausência de execução não deve ser descrita como aprovação de teste.
- Se um gate depender de decisão futura de outro card, registrar o vínculo e não afirmar que o sistema dependente está totalmente validado.

## Registro do ciclo

Para cada card, o Trello registra: estado do trabalho, critérios DoD/checklist, revisões e decisões dos agentes aplicáveis, gates **N/A** justificados, arquivos alterados e link do commit próprio. A ordem do backlog permanece inalterada.

