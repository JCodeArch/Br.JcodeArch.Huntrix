# Rumi Idle V2 — prova técnica no Unity

A entrega do Antigravity foi revisada antes de entrar no clone. Foram copiados somente o importador, seus testes e metas; adicionada a referência `Unity.2D.Sprite.Editor` nos dois assemblies pertinentes. Nenhum csproj gerado foi copiado.

A revisão removeu o fallback para um checkout pessoal e o acesso a metadata privada do Unity. O importador agora usa Sprite Data Provider público e exige marcadores explícitos para atualizar destinos existentes, mesmo quando o PNG tem hash igual. Os testes verificam também GUID/local fileID e reimportação idempotente.

## Arte e geometria

- PNG intacto: SHA256 `2bee6f1261a993e6d17a1eacd9e1df3c6b703c479d5c4cec89137c3ef73cf8eb`.
- Textura 1024×1536; quatro recortes iguais 448×744, origens calibradas no layout JSON.
- Pivô normalizado `(0.60, 0.03629)`, FullRect, PPU200 e worldScale1 na prova.
- Catálogo apenas Idle, sem estados inventados para corrida, ataque ou derrota.
- Destino: `Assets/Art/HD/Rumi/IdleProofV2`.

## Evidência

O importador passou seis testes no D: e no clone. A suíte EditMode final do clone passou **143/143**, exit0, em `outputs/hd-validation/rumi-idle-cadence-verified-EditMode.xml`. A última suíte completa runtime passou **547/547**, em `sprite-fallback-final-PlayMode.xml`, antes das mudanças exclusivamente de Editor e assets da prova.

As quatro assemblies compilaram com o SDK8 embarcado no Unity e a política de diagnósticos já usada pelo motor. O teste final de cadência também teve compilação independente prévia com zero erros/avisos. A distinção entre build bruto e política Unity está documentada no relatório da integração; não declarar todos os logs brutos sem avisos.

A cena isolada `RumiIdleProofPreview.unity` foi aberta em Play Mode real, D3D11. Foram observados figurino e paleta corretos, transparência, espada inteira e quadros com olhos abertos/fechados. A primeira versão da cena perdeu o override do catálogo e exibiu arte legada; a referência persistida foi corrigida. O helper passou a registrar o override e validar o vínculo antes de entrar em Play Mode.

Capturas nativas, sem edição de imagem:

- `outputs/hd-validation/rumi-idle-v2-unity-proof.png`
- `outputs/hd-validation/rumi-idle-v2-unity-proof-blink.png`

O Editor foi retirado de Play Mode e encerrado; o PID da sessão deixou de existir. A janela exibiu um aviso de Account API do Editor, sem impedir a prova. Não foi uma partida completa do jogo.

## Cadência e limite de aprovação

O especialista aprovou a importação técnica e reprovou o default de0,15s em cada quadro: loop0,6s, aproximadamente100 piscadas/minuto. A regressão RED confirmou duração0,6s quando se esperava uma prova mais tranquila.

A cadência provisória agora é **2,0 / 0,4 / 0,10 / 0,4 segundos**, ciclo2,9s. Os testes verificam hold aberto, janela breve de piscada, reabertura e duração total por identificadores dos sprites persistidos. Comparar referência CLR entre wrappers recarregados era incorreto; a checagem usa GUID/local fileID. Uma tentativa intermediária também chamou um método interno indisponível ao assembly de testes; esse erro foi corrigido, sem ampliar API runtime.

As capturas antecedem o ajuste final de cadência. O novo tempo foi verificado por amostragem do catálogo no EditMode; a avaliação perceptiva desse ritmo ainda está pendente. Quatro quadros não provam fluidez profissional, e a pose frontal não substitui poses laterais de combate.

Próximos critérios: continuidade do loop em movimento, animações laterais de corrida/ataque/pulo/queda/derrota sob o mesmo contrato, conjunto equivalente para as demais personagens, calibração com hurtboxes e câmera, legibilidade durante sobreposição e primeira fase completável. Esta prova não substituiu a heroína da cena jogável.
