# 🤖 Agente Watchdog do ChatGPT (Auto-Resumer)

Agente autônomo projetado para rodar em segundo plano, monitorar a janela do ChatGPT, detectar bloqueios por limite de uso e retomar a execução automaticamente.

---

### 🔄 Ciclo de Vida do Agente (Máquina de Estados)

1. **`MONITORING` (Monitoramento Ativo):**
   * Faz varreduras a cada 8 segundos utilizando o motor OCR nativo do Windows (`winocr`).
   * Procura pela notificação:
     > *"Você atingiu seu limite de uso. Faça upgrade do plano ou adicione créditos para continuar, ou tente novamente na seguinte data ou horário: DD de MÊS de AAAA, HH:MM."*
   
2. **`WAITING` (Agendamento & Contagem Regressiva):**
   * Ao detectar o aviso, extrai a data e hora exatas de liberação.
   * Calcula o disparo para: **Horário da Liberação + 1 Minuto**.
   * Emite alerta sonoro no Windows e exibe uma contagem regressiva em tempo real.

3. **`EXECUTING` (Retomada Automática):**
   * No segundo exato programado, foca e restaura a janela do **ChatGPT** na tela.
   * Localiza a imagem do botão Play ([`btn_play.png`](./btn_play.png)) via visão computacional.
   * Move o cursor do mouse e dispara o clique.
   * Emite dois bips sonoros de confirmação.

4. **`RESET & RE-MONITOR` (Ciclo Contínuo):**
   * Após 30 segundos, o agente limpa o estado e volta a monitorar automaticamente, pronto para o próximo limite.

---

### 🚀 Como Iniciar

Dê dois cliques no arquivo:
👉 **[`start_agent.bat`](./start_agent.bat)**

Ou pelo PowerShell / Prompt:
```powershell
cd d:\Workspace\Br.JcodeArch.Huntrix\tools\click_goal_button
.\start_agent.bat
```

Você pode deixar a janela do agente minimizada enquanto trabalha. Ele continuará operando e cuidará de retomar suas tarefas no ChatGPT sem necessidade de intervenção manual.
