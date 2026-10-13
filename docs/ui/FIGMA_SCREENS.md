# Telas Figma — HUNTR/X

Arquivo de design: [HUNTR/X no Figma](https://www.figma.com/design/AoNgky0oNVLU31UvML5xns).

Cinco frames estáticos landscape de **1280×720**, na página `01 · Mobile landscape` (`0:1`). A resolução é referência de design, não novo contrato de resolução Unity.

| Tela | Nó | Link |
| --- | --- | --- |
| Menu principal | `2:2` | [Abrir](https://www.figma.com/design/AoNgky0oNVLU31UvML5xns?node-id=2-2) |
| Equipe e perfil | `2:29` | [Abrir](https://www.figma.com/design/AoNgky0oNVLU31UvML5xns?node-id=2-29) |
| HUD mobile — protótipo | `2:71` | [Abrir](https://www.figma.com/design/AoNgky0oNVLU31UvML5xns?node-id=2-71) |
| Opções e acessibilidade | `2:110` | [Abrir](https://www.figma.com/design/AoNgky0oNVLU31UvML5xns?node-id=2-110) |
| Conclusão e replay | `2:150` | [Abrir](https://www.figma.com/design/AoNgky0oNVLU31UvML5xns?node-id=2-150) |

Revisão visual realizada: screenshots das cinco telas inspecionados; recortes de idioma e texto lateral em Opções corrigidos e screenshot posterior conferido. Aprovação humana e legibilidade em dispositivo alvo permanecem pendentes. Não há wiring de navegação nem telas implementadas ou validadas no Unity nesta frente. Resultados e valores ilustrativos não são evidência de playtest.

Antes de trabalhar nas telas, toda IA deve executar a leitura de contexto descrita em [SUPABASE_AI_CONTINUITY.md](../integrations/SUPABASE_AI_CONTINUITY.md). O agente Figma deve receber um `context_packet` que comece pela referência ao arquivo e pelo inventário de telas acima: URL, IDs/nomes, orientação, dimensões e estado de validação. Em seguida, deve incluir revisão do checkpoint, objetivo de cada tela, restrições visuais, decisões, fontes e próximo passo.

Após cada transição material, registrar no Supabase a URL, IDs dos nós efetivamente criados, nomes confirmados, estado e evidências de revisão. Usar artefatos para referências de design e tarefas para implementação/validação. Mudanças feitas no Figma devem ser verificadas por leitura ou captura antes de serem declaradas concluídas.

Próximos passos: revisão humana/dispositivo; definir navegação, estados vazio/disabled/pausa, remapeamento e safe areas; aprovar direção final e assets; integrar Unity e testar touch/acessibilidade. Fontes: `docs/GDD_MASTER.md`, `docs/architecture/AGE_PROFILES.md`, `docs/PROJECT_CONTEXT.md` e `docs/IMPLEMENTATION_STATUS.md`. Cooperação até três continua requisito, sem modalidade ou prova definida pelas telas.
