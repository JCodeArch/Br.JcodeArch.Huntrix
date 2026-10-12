# Estado da implementação — cards 41–116

Consolidação em 12 de outubro de 2026 (UTC), após o lote anterior a este documento. **75 cards reais; #105 não existe no backlog oficial. 74 têm PR/merge de fonte em `develop` comprovado pelos receipts; #116 é a documentação de backlog e deste ledger, preparada neste PR.** Não se afirma merge do próprio #116 antes de sua publicação. Estes totais descrevem fonte, assets, planos e ferramentas; não 75 funcionalidades aceitas em gameplay.

A implementação é um conjunto de protótipos de fonte e apresentação original. **Compilação/importação Unity, builds executáveis, QA humano/device, balanceamento, qualificação RC e direitos/publicação continuam pendentes.** Integrar um PR após revisão estática sob a exceção autorizada não aprova runtime nem release. Histórico executado do card #29 não valida cards posteriores.

A cena principal é `Assets/Scenes/VerticalSlice.unity`, com sete etapas serializadas: avião, ramen, fãs, horda/Special/Honmoon, mini-boss, paraquedas e show. Existem teclado/gamepad/touch, HUD, opções de faixa etária/acessibilidade/idioma, arte/áudio/narrativa de protótipo e registro de 60 segundos. Não foi feito playtest humano nem comprovado funcionamento no Editor/dispositivo.

Fonte consolidada anterior ao #116: [`ae68ba4a26`](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/commit/ae68ba4a26649afc3c87f23e0f5fe4b347569345), merge do card 115/PR #87. O orquestrador confirmou `develop` limpo no Codespace nesse commit, repetiu os 7 testes Python, versão `0.1.0-dev.1` e sintaxe Bash com sucesso. Este commit não inclui o próprio documento #116 nem evidencia execução Unity.

## Evidência executada nesta consolidação

| Verificação | Resultado observado | Limite |
|---|---|---|
| `python scripts/test-evidence-tooling.py` | 7 testes Python passaram; saída `OK`, código de saída 0 | Testa ferramentas/fixtures sintéticas; não Unity/gameplay/build real |
| `python scripts/version.py check` | `0.1.0-dev.1`, código de saída 0 | Valida a versão de fonte; não gera binário |
| `bash -n scripts/build-unity.sh scripts/run-unity-tests.sh` | código de saída 0 | Sintaxe Bash; não execução Editor/build |
| Revisão independente de fonte/assets | Protótipos aceitos estaticamente; referências verificadas | Não comprova importação, render, física ou desempenho |
| `bash scripts/run-unity-tests.sh` (tentativa do orquestrador) | Bloqueada na linha 3: `UNITY_EDITOR` não configurado | Nenhum teste Unity foi iniciado; Editor não disponível nos ambientes local/Codespace examinados |
| Consulta aos runners GitHub | HTTP 403, `Resource not accessible by integration`; inconclusiva | Falha de acesso não demonstra ausência de runners; disponibilidade/licença/módulos não confirmados |
| CI final de ferramentas ([run 38289892178](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/actions/runs/38289892178), head do PR #87/card 115) | Sucesso confirmado pela orquestração/diagnóstico | Somente ferramentas; jobs Unity/build/iOS ignorados (`skipped`), não executados |
| CI intermediário do card 107 | Falhou porque `check-release-candidate.py` do #110 ainda não estava publicado | Resolvido pela integração do lote; CI final de ferramentas passou sem patch adicional |

Os receipts GitHub foram lidos de `receipts-41-54.json` e `cardN-remote.json` pelo orquestrador e confrontados com `merged:true`, base `develop`, URL/commit. Os 74 merges anteriores foram confirmados nos receipts ao finalizar o lote; não há receipt do próprio #116 nesta fotografia. O backlog de 75 foi lido de `backlog-41-116.json`. Evidência detalhada: [revisão integrada](reviews/2026-10-12-integration-55-116.md), [validação adiada](governance/DEFERRED_VALIDATION.md), [estabilização](qa/STABILIZATION_LEDGER.md), [gate RC](qa/RELEASE_CANDIDATE_GATE.md).

## Ledger de escopo e integração

“Integrado” significa merge de fonte verificado. “Plano QA”, “preparação de build” e “documentação” preservam o tipo real da entrega; execução não é inferida do status Git.

<!-- RECEIPT_LEDGER_BEGIN -->
| Card | Escopo do Trello | Entrega | Integração de fonte | Evidência GitHub |
|---|---|---|---|---|
| [41](https://trello.com/c/lsykpVLG/41-41-criar-gwi-ma) | Criar Gwi-Ma | Protótipo de implementação | Integrado develop | [PR #14](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/14) · `1fd46cca2a` |
| [42](https://trello.com/c/YDxtK7wQ/42-42-implementar-estados-dos-f%C3%A3s) | Implementar estados dos fãs | Protótipo de implementação | Integrado develop | [PR #15](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/15) · `6677f7e0ed` |
| [43](https://trello.com/c/Vx2NROMc/43-43-implementar-resgate-de-f%C3%A3s) | Implementar resgate de fãs | Protótipo de implementação | Integrado develop | [PR #16](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/16) · `b1dde88eea` |
| [44](https://trello.com/c/drW9VawR/44-44-implementar-soul-drain) | Implementar soul drain | Protótipo de implementação | Integrado develop | [PR #17](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/17) · `bf37b2fd56` |
| [45](https://trello.com/c/22Drvqk0/45-45-implementar-comida) | Implementar comida | Protótipo de implementação | Integrado develop | [PR #18](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/18) · `3ad7754144` |
| [46](https://trello.com/c/AvT9CFqD/46-46-implementar-escolha-self-fan) | Implementar escolha self/fan | Protótipo de implementação | Integrado develop | [PR #19](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/19) · `5668318c8c` |
| [47](https://trello.com/c/mzLJYgUm/47-47-implementar-honmoon) | Implementar Honmoon | Protótipo de implementação | Integrado develop | [PR #20](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/20) · `4c25028f6b` |
| [48](https://trello.com/c/GqHu0qeY/48-48-implementar-honmoon-crisis) | Implementar Honmoon Crisis | Protótipo de implementação | Integrado develop | [PR #21](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/21) · `337f193fbb` |
| [49](https://trello.com/c/PVbkIfDF/49-49-implementar-special) | Implementar Special | Protótipo de implementação | Integrado develop | [PR #22](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/22) · `fae232dcb5` |
| [50](https://trello.com/c/8RbUCMCK/50-50-implementar-efeitos-do-special) | Implementar efeitos do Special | Protótipo de implementação | Integrado develop | [PR #23](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/23) · `82c95a5315` |
| [51](https://trello.com/c/N1apjT90/51-51-implementar-combat-performance) | Implementar Combat Performance | Protótipo de implementação | Integrado develop | [PR #24](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/24) · `1a809c95bd` |
| [52](https://trello.com/c/um3Ke7Ob/52-52-implementar-progress%C3%A3o-de-equipamentos) | Implementar progressão de equipamentos | Protótipo de implementação | Integrado develop | [PR #25](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/25) · `9b1dcb567a` |
| [53](https://trello.com/c/FJ58KiwC/53-53-definir-efeitos-das-pe%C3%A7as) | Definir efeitos das peças | Protótipo de implementação | Integrado develop | [PR #26](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/26) · `c99e062cff` |
| [54](https://trello.com/c/N7UiMtCh/54-54-implementar-replay-e-rankings) | Implementar replay e rankings | Protótipo de implementação | Integrado develop | [PR #27](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/27) · `75e070f2b3` |
| [55](https://trello.com/c/QTxlWZ6r/55-55-definir-gram%C3%A1tica-de-level-design) | Definir gramática de level design | Protótipo de implementação | Integrado develop | [PR #28](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/28) · `f8c2a27bee` |
| [56](https://trello.com/c/UW2pfzmH/95-56-planejar-fases-do-filme) | Planejar fases do filme | Protótipo de implementação | Integrado develop | [PR #29](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/29) · `e4dea8e6eb` |
| [57](https://trello.com/c/IRPeBsbr/96-57-projetar-encontros-a%C3%A9reos) | Projetar encontros aéreos | Protótipo de implementação | Integrado develop | [PR #31](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/31) · `c409405e6b` |
| [58](https://trello.com/c/gSYfmQ72/97-58-projetar-encontros-com-f%C3%A3s) | Projetar encontros com fãs | Protótipo de implementação | Integrado develop | [PR #37](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/37) · `a267072eb1` |
| [59](https://trello.com/c/31S2pykD/98-59-projetar-boss-arenas) | Projetar boss arenas | Protótipo de implementação | Integrado develop | [PR #32](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/32) · `d9c422090a` |
| [60](https://trello.com/c/i7XRnHKD/99-60-criar-blockouts) | Criar blockouts | Protótipo de implementação | Integrado develop | [PR #30](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/30) · `f0ff617570` |
| [61](https://trello.com/c/QmViaOIh/100-61-vertical-slice-avi%C3%A3o) | Vertical Slice: Avião | Protótipo de implementação | Integrado develop | [PR #38](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/38) · `635e395815` |
| [62](https://trello.com/c/n1qZp8D9/101-62-vertical-slice-ramen) | Vertical Slice: Ramen | Protótipo de implementação | Integrado develop | [PR #52](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/52) · `7c254dc5fe` |
| [63](https://trello.com/c/LgPMs55V/102-63-vertical-slice-f%C3%A3s) | Vertical Slice: Fãs | Protótipo de implementação | Integrado develop | [PR #53](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/53) · `e77ab56b9a` |
| [64](https://trello.com/c/ucDTyNLW/103-64-vertical-slice-horda-special) | Vertical Slice: Horda + Special | Protótipo de implementação | Integrado develop | [PR #54](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/54) · `147318a5a6` |
| [65](https://trello.com/c/rvDCzPwS/104-65-vertical-slice-mini-boss) | Vertical Slice: Mini-boss | Protótipo de implementação | Integrado develop | [PR #55](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/55) · `44c9bfbe42` |
| [66](https://trello.com/c/dwgtKEFu/56-66-vertical-slice-paraquedas) | Vertical Slice: Paraquedas | Protótipo de implementação | Integrado develop | [PR #56](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/56) · `a0cfd903ef` |
| [67](https://trello.com/c/pVifyaKg/57-67-vertical-slice-show) | Vertical Slice: Show | Protótipo de implementação | Integrado develop | [PR #57](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/57) · `5ff39e09f9` |
| [68](https://trello.com/c/XxeFEI8g/58-68-playtest-de-60-segundos) | Playtest de 60 segundos | Instrumentação de playtest | Integrado develop | [PR #58](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/58) · `edb96b478d` |
| [69](https://trello.com/c/MOmwyC38/59-69-character-bible) | Character Bible | Documentação/preparação | Integrado develop | [PR #39](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/39) · `00705a6b99` |
| [70](https://trello.com/c/YkHimQ8E/60-70-model-sheets) | Model sheets | Documentação/preparação | Integrado develop | [PR #59](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/59) · `799d1c8804` |
| [71](https://trello.com/c/sTyyNQda/61-71-sprite-rig-pipeline) | Sprite/rig pipeline | Protótipo de implementação | Integrado develop | [PR #60](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/60) · `bef6363e8c` |
| [72](https://trello.com/c/wGjYFXXR/62-72-anima%C3%A7%C3%B5es-de-gameplay) | Animações de gameplay | Protótipo de implementação | Integrado develop | [PR #61](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/61) · `e3a4e7e424` |
| [73](https://trello.com/c/Lh8I9THv/63-73-vfx-gameplay) | VFX gameplay | Protótipo de implementação | Integrado develop | [PR #62](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/62) · `ccc0177f21` |
| [74](https://trello.com/c/ZWOGJXnf/64-74-environments) | Environments | Protótipo de implementação | Integrado develop | [PR #63](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/63) · `a17337afa6` |
| [75](https://trello.com/c/UjSKTB8D/65-75-audiomanager) | AudioManager | Protótipo de implementação | Integrado develop | [PR #40](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/40) · `1a71c78bd6` |
| [76](https://trello.com/c/OrwJvjBK/66-76-m%C3%BAsica-din%C3%A2mica) | Música dinâmica | Protótipo de implementação | Integrado develop | [PR #41](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/41) · `8d8558298c` |
| [77](https://trello.com/c/6ryGpALS/67-77-sfx-de-combate) | SFX de combate | Protótipo de implementação | Integrado develop | [PR #42](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/42) · `46d9ba6916` |
| [78](https://trello.com/c/Ps1gZ55e/68-78-sistema-de-torcida) | Sistema de torcida | Protótipo de implementação | Integrado develop | [PR #64](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/64) · `90ac3a740b` |
| [79](https://trello.com/c/T926rWLs/69-79-pol%C3%ADtica-de-m%C3%BAsica-licenciamento) | Política de música/licenciamento | Documentação/preparação | Integrado develop | [PR #43](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/43) · `ff0adbe759` |
| [80](https://trello.com/c/w2iDm42o/70-80-roteiro-das-cutscenes) | Roteiro das cutscenes | Documentação/preparação | Integrado develop | [PR #44](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/44) · `f9e86b03c7` |
| [81](https://trello.com/c/rSq9WhUM/71-81-sistema-de-di%C3%A1logos) | Sistema de diálogos | Protótipo de implementação | Integrado develop | [PR #45](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/45) · `1211d8a5b5` |
| [82](https://trello.com/c/zZMmgFPU/72-82-roteiro-das-transi%C3%A7%C3%B5es) | Roteiro das transições | Protótipo de implementação | Integrado develop | [PR #65](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/65) · `5b61e7c04f` |
| [83](https://trello.com/c/dYumnSUl/73-83-arco-rumi-jinu) | Arco Rumi/Jinu | Protótipo de implementação | Integrado develop | [PR #46](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/46) · `ba679e8456` |
| [84](https://trello.com/c/Ir0Sl82N/74-84-final-e-gwi-ma) | Final e Gwi-Ma | Protótipo de implementação | Integrado develop | [PR #47](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/47) · `0f6c4e4704` |
| [85](https://trello.com/c/mMQB7Zgi/75-85-ageprofiledata) | AgeProfileData | Protótipo de implementação | Integrado develop | [PR #48](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/48) · `03005cd391` |
| [86](https://trello.com/c/YtuCNqbR/76-86-adaptar-viol%C3%AAncia-visual) | Adaptar violência visual | Protótipo de implementação | Integrado develop | [PR #49](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/49) · `e608e4fba7` |
| [87](https://trello.com/c/BUghwmkX/77-87-adaptar-dificuldade) | Adaptar dificuldade | Protótipo de implementação | Integrado develop | [PR #66](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/66) · `5cd20263b0` |
| [88](https://trello.com/c/YldhB00v/78-88-adaptar-di%C3%A1logos-cutscenes) | Adaptar diálogos/cutscenes | Protótipo de implementação | Integrado develop | [PR #50](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/50) · `1a60672b53` |
| [89](https://trello.com/c/PY3YaT08/79-89-dynamic-difficulty) | Dynamic Difficulty | Protótipo de implementação | Integrado develop | [PR #67](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/67) · `401286d862` |
| [90](https://trello.com/c/e477OffI/80-90-accessibility) | Accessibility | Protótipo de implementação | Integrado develop | [PR #68](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/68) · `b735ed4fd2` |
| [91](https://trello.com/c/POdhnFFJ/81-91-child-safety) | Child Safety | Protótipo de implementação | Integrado develop | [PR #51](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/51) · `90910449e3` |
| [92](https://trello.com/c/s3I59eSy/82-92-touch-controls) | Touch controls | Protótipo de implementação | Integrado develop | [PR #69](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/69) · `67807e9e1e` |
| [93](https://trello.com/c/6hEa1A28/83-93-ui-responsiva) | UI responsiva | Protótipo de implementação | Integrado develop | [PR #72](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/72) · `c2e955d106` |
| [94](https://trello.com/c/fe6480O3/84-94-otimizar-sprites-e-atlases) | Otimizar sprites e atlases | Artefatos/editor de atlas | Integrado develop | [PR #70](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/70) · `8f3abf9813` |
| [95](https://trello.com/c/qluU8CIr/85-95-object-pooling) | Object pooling | Pooling de protótipo | Integrado develop | [PR #71](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/71) · `498ec6fe1c` |
| [96](https://trello.com/c/u3wPuw1Z/86-96-performance-60-fps) | Performance 60 FPS | Instrumentação de performance | Integrado develop | [PR #73](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/73) · `816c1df191` |
| [97](https://trello.com/c/yOqmXshE/87-97-android-build-pipeline) | Android build pipeline | Preparação de build/qualificação | Integrado develop | [PR #74](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/74) · `2256586c2c` |
| [98](https://trello.com/c/GJL69MzY/88-98-ios-pipeline) | iOS pipeline | Preparação de build/qualificação | Integrado develop | [PR #75](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/75) · `d0fc0f4bc6` |
| [99](https://trello.com/c/IvLRhXDb/89-99-testes-automatizados) | Testes automatizados | Testes/ferramentas autorados | Integrado develop | [PR #76](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/76) · `a3cb8b6107` |
| [100](https://trello.com/c/mG79WYcS/90-100-gameplay-qa) | Gameplay QA | Plano de execução QA | Integrado develop | [PR #77](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/77) · `1806dc593b` |
| [101](https://trello.com/c/K1044ctU/91-101-balanceamento) | Balanceamento | Plano de execução QA | Integrado develop | [PR #78](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/78) · `deb5373482` |
| [102](https://trello.com/c/yzBKwWfb/92-102-age-qa) | Age QA | Plano de execução QA | Integrado develop | [PR #79](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/79) · `8d6a15f6f9` |
| [103](https://trello.com/c/hi25R5Rd/93-103-accessibility-qa) | Accessibility QA | Plano de execução QA | Integrado develop | [PR #80](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/80) · `141ba423e7` |
| [104](https://trello.com/c/wc5pkihb/94-104-mobile-qa) | Mobile QA | Plano de execução QA | Integrado develop | [PR #81](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/81) · `fb8a1942bf` |
| [106](https://trello.com/c/M9f55xp7/105-106-versionamento) | Versionamento | Documentação/preparação | Integrado develop | [PR #33](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/33) · `e5b3f6afb5` |
| [107](https://trello.com/c/2GIRu4mj/106-107-ci-cd) | CI/CD | Preparação de build/qualificação | Integrado develop | [PR #82](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/82) · `373a616378` |
| [108](https://trello.com/c/BFIQ8tE9/107-108-build-windows) | Build Windows | Preparação de build/qualificação | Integrado develop | [PR #83](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/83) · `c29d78b125` |
| [109](https://trello.com/c/Lr8Keakh/108-109-build-android) | Build Android | Preparação de build/qualificação | Integrado develop | [PR #84](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/84) · `449bbde58d` |
| [110](https://trello.com/c/N7tdXo5w/109-110-release-candidate) | Release Candidate | Preparação de build/qualificação | Integrado develop | [PR #85](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/85) · `2e15caf4d0` |
| [111](https://trello.com/c/FercJZOl/110-111-plano-de-publica%C3%A7%C3%A3o) | Plano de publicação | Documentação/preparação | Integrado develop | [PR #34](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/34) · `b954e4036e` |
| [112](https://trello.com/c/CW3A7U9p/111-112-readme-do-projeto) | README do projeto | Documentação/preparação | Integrado develop | [PR #86](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/86) · `09863e3fe4` |
| [113](https://trello.com/c/CPiuwlPV/112-113-guia-de-arquitetura) | Guia de arquitetura | Documentação/preparação | Integrado develop | [PR #35](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/35) · `af62156082` |
| [114](https://trello.com/c/unuaaQLc/113-114-guia-de-cria%C3%A7%C3%A3o-de-conte%C3%BAdo) | Guia de criação de conteúdo | Documentação/preparação | Integrado develop | [PR #36](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/36) · `c949fd684a` |
| [115](https://trello.com/c/4RGtc2AQ/114-115-changelog) | Changelog | Documentação/preparação | Integrado develop | [PR #87](https://github.com/JCodeArch/Br.JcodeArch.Huntrix/pull/87) · `ae68ba4a26` |
| [116](https://trello.com/c/RC9iPaDi/115-116-backlog-p%C3%B3s-lan%C3%A7amento) | Backlog pós-lançamento | Documentação/preparação | Documento deste PR | Publicação própria ainda não comprovada |

<!-- RECEIPT_LEDGER_END -->

## Aceitação ainda necessária

- Editor 6000.6.4f1 licenciado, importação e compilação das assemblies; suites EditMode/PlayMode sobre o commit integrado exato. Os novos testes Unity são autorados, não executados.
- Percurso completo da slice e avaliação humana de 60 segundos; gameplay, balanceamento, cinco faixas etárias, acessibilidade e mobile (#100–104). O formulário/registro não constitui avaliação realizada.
- Android, Windows e export/iOS reais, módulos/runners, assinaturas/provisionamento, instalação e interrupções em dispositivos. Atlas/pooling e alvo de 60 FPS precisam medição de CPU/GPU/memória, sem benefício quantitativo alegado.
- Relatórios/proveniência de testes, artifacts/digests/buildGUID, perfis de dispositivo e aceite humano associados ao mesmo commit; somente então qualificar RC.
- Arte, áudio, história, tradução, desenho/cooperação e tuning finais; direitos da adaptação/personagens/marca/música, requisitos de lojas/SDKs/privacidade infantil e decisão de publicação. Material original de protótipo não autoriza a adaptação nem certifica conformidade.

#116 documenta ideias pós-lançamento, sem afirmar que lançamento ocorreu ou que Boss Rush/Time Attack/expansões foram implementados. Nunca promover um plano, receita de merge ou fixture sintética a prova de execução.
