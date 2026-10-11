# Card58 — encontros de fãs em risco

Implementado — validação Unity pendente. Definição explícita de grupo com registro limitado a64 fãs, perda inicial de alma provisória20. FanRiskEncounter_Prototype instancia FanDrainEncounter_Prototype: dreno automático telegrafado com recuperação/resgate já existentes. FanSlice_Prototype usa grupo passivo separado para a introdução jogável, sem pressão antes do beat de resgate.

Bootstrap automático exige definition e origin explícitos. Roots instanciadas são independentes; disable encerra inscrições e destrói somente grupo próprio. Não usa busca global. Callbacks de perda inicial revalidam ownership antes de continuar. Valores são protótipo, sem aprovação de balanceamento. Não executados importação, compilação ou gameplay Unity.
