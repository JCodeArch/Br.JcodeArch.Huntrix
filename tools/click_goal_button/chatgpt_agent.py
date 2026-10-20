"""
================================================================================
  AGENTE AUTÔNOMO: ChatGPT Limit Watchdog & Auto-Resumer
================================================================================
Responsabilidade:
  1. Monitorar continuamente a janela do ChatGPT.
  2. Detectar automaticamente a notificação de limite de uso e extrair a data/hora:
     "Você atingiu seu limite de uso... tente novamente na seguinte data ou horário: DD de MÊS de AAAA, HH:MM"
  3. Agendar o disparo do clique para exatamente 1 MINUTO APÓS o horário indicado.
  4. Fazer contagem regressiva em tempo real.
  5. Ao atingir o horário, focar a janela do ChatGPT, localizar o botão de Play e disparar o clique.
  6. Validar a retomada da meta e retornar ao modo de monitoramento para o próximo ciclo.
"""

import asyncio
import ctypes
import os
import re
import sys
import time
import winsound
from datetime import datetime, timedelta

import pyautogui
from PIL import Image
import winocr

MONTHS = {
    'jan': 1, 'fev': 2, 'mar': 3, 'abr': 4, 'mai': 5, 'jun': 6,
    'jul': 7, 'ago': 8, 'set': 9, 'out': 10, 'nov': 11, 'dez': 12
}

user32 = ctypes.windll.user32


def focus_chatgpt_window():
    """Localiza a janela do ChatGPT e traz para o primeiro plano."""
    try:
        import pygetwindow as gw
        windows = gw.getWindowsWithTitle("ChatGPT")
        if windows:
            chat_win = windows[0]
            if chat_win.isMinimized:
                chat_win.restore()
            chat_win.activate()
            time.sleep(0.5)
            return True
    except Exception:
        pass

    # Fallback via EnumWindows nativo
    def enum_cb(hwnd, lparam):
        length = user32.GetWindowTextLengthW(hwnd)
        if length > 0:
            buff = ctypes.create_unicode_buffer(length + 1)
            user32.GetWindowTextW(hwnd, buff, length + 1)
            if "chatgpt" in buff.value.lower():
                user32.ShowWindow(hwnd, 9)  # SW_RESTORE
                user32.SetForegroundWindow(hwnd)
                return False
        return True

    WNDENUMPROC = ctypes.WINFUNCTYPE(ctypes.c_bool, ctypes.c_void_p, ctypes.c_void_p)
    user32.EnumWindows(WNDENUMPROC(enum_cb), 0)
    time.sleep(0.5)
    return True


def ocr_screen():
    """Captura a tela e faz OCR com proteção contra falhas temporárias."""
    try:
        screenshot = pyautogui.screenshot()
    except Exception as e:
        return ""

    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        res = loop.run_until_complete(winocr.recognize_pil(screenshot, 'pt'))
        return res.text
    except Exception:
        return ""
    finally:
        loop.close()


def parse_limit_message(text):
    """Extrai a data e hora do aviso de limite."""
    # Formato completo: '5 de out. de 2026, 00:38'
    pattern = r'(\d{1,2})\s+de\s+([a-zA-Zçáéíóú]+)\.?\s+de\s+(\d{4})[, ]+\s*(\d{1,2}):(\d{2})'
    match = re.search(pattern, text, re.IGNORECASE)
    if match:
        day = int(match.group(1))
        month_str = match.group(2)[:3].lower()
        month = MONTHS.get(month_str)
        year = int(match.group(3))
        hour = int(match.group(4))
        minute = int(match.group(5))
        if month:
            return datetime(year, month, day, hour, minute, 0)

    # Fallback: horário simples junto à palavra limite
    if "limite" in text.lower():
        time_match = re.search(r'(\d{1,2}):(\d{2})', text)
        if time_match:
            hour = int(time_match.group(1))
            minute = int(time_match.group(2))
            now = datetime.now()
            target = now.replace(hour=hour, minute=minute, second=0, microsecond=0)
            if target <= now:
                target += timedelta(days=1)
            return target

    return None


def execute_click(template_name="btn_play.png", max_retries=15):
    """Localiza o botão Play na tela e executa o clique com verificação."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    template_path = os.path.join(script_dir, template_name)

    if not os.path.exists(template_path):
        print(f"[AGENTE - ERRO] Imagem de referência '{template_name}' não encontrada.")
        return False

    confidences = [0.88, 0.82, 0.76, 0.70]

    for attempt in range(1, max_retries + 1):
        location = None
        for conf in confidences:
            try:
                location = pyautogui.locateCenterOnScreen(template_path, confidence=conf)
            except Exception:
                try:
                    location = pyautogui.locateCenterOnScreen(template_path)
                except Exception:
                    pass
            if location:
                break

        if location:
            tx, ty = location
            print(f"\n[AGENTE - AÇÃO] Botão Play localizado em X={tx}, Y={ty}. Executando clique...")
            pyautogui.moveTo(tx, ty, duration=0.3)
            time.sleep(0.1)
            pyautogui.click()
            time.sleep(0.5)
            # Toca aviso sonoro de conclusão
            try:
                winsound.Beep(1200, 300)
                winsound.Beep(1600, 400)
            except Exception:
                pass
            print("[AGENTE - SUCESSO] Clique disparado com sucesso no botão Play!\n")
            return True

        time.sleep(1.0)

    print("\n[AGENTE - ALERTA] Não foi possível localizar o botão Play na tela após as tentativas.")
    return False


class ChatGPTWatchdogAgent:
    def __init__(self, check_interval=8, extra_minutes=1):
        self.check_interval = check_interval
        self.extra_minutes = extra_minutes
        self.state = "MONITORING"
        self.target_time = None
        self.reset_time = None
        self.cycle_count = 0

    def log(self, tag, message):
        now = datetime.now().strftime("%H:%M:%S")
        print(f"[{now}] [{tag}] {message}")

    def notify(self, title, message):
        """Dispara bip e log."""
        try:
            winsound.Beep(880, 200)
        except Exception:
            pass

    def run(self):
        os.system("cls" if os.name == "nt" else "clear")
        print("=" * 70)
        print("      🤖 AGENTE AUTÔNOMO: WATCHDOG CHATGPT (AUTO-RESUMER)")
        print("=" * 70)
        print(f" • Intervalo de varredura: {self.check_interval} segundos")
        print(f" • Margem pós-liberação:   +{self.extra_minutes} minuto(s)")
        print(" • Pressione Ctrl+C para encerrar o agente a qualquer momento.\n")

        self.log("AGENTE", "Inicializado com sucesso. Monitorando a tela em busca de avisos de limite...")

        last_scan_time = 0

        while True:
            try:
                # ESTADO: MONITORANDO
                if self.state == "MONITORING":
                    text = ocr_screen()
                    if ("limite de uso" in text.lower() or "tente novamente na seguinte data" in text.lower()):
                        reset_dt = parse_limit_message(text)
                        if reset_dt:
                            self.reset_time = reset_dt
                            self.target_time = reset_dt + timedelta(minutes=self.extra_minutes)
                            self.state = "WAITING"

                            self.notify("Limite Detectado", f"Liberação às {self.reset_time.strftime('%H:%M:%S')}")

                            print("\n" + "#" * 70)
                            self.log("EVENTO", "🚨 MENSAGEM DE LIMITE DE USO DETECTADA!")
                            self.log("HORÁRIO LIMITE", f"{self.reset_time.strftime('%d/%m/%Y %H:%M:%S')}")
                            self.log("AGENDAMENTO", f"Clique agendado para: {self.target_time.strftime('%d/%m/%Y %H:%M:%S')} (+{self.extra_minutes}m)")
                            print("#" * 70 + "\n")
                            continue

                    now_str = datetime.now().strftime("%H:%M:%S")
                    sys.stdout.write(f"\r[{now_str}] [MONITORANDO] Tela analisada. Nenhum bloqueio ativo... ")
                    sys.stdout.flush()
                    time.sleep(self.check_interval)

                # ESTADO: AGUARDANDO LIBERAÇÃO (+ 1 MINUTO)
                elif self.state == "WAITING":
                    now = datetime.now()
                    diff = (self.target_time - now).total_seconds()

                    if diff <= 0:
                        self.state = "EXECUTING"
                        continue

                    rem_h = int(diff // 3600)
                    rem_m = int((diff % 3600) // 60)
                    rem_s = int(diff % 60)

                    msg = f"\r[AGENDADO - AGUARDANDO] Faltam {rem_h:02d}h {rem_m:02d}m {rem_s:02d}s | Disparo às: {self.target_time.strftime('%H:%M:%S')} "
                    sys.stdout.write(msg)
                    sys.stdout.flush()

                    step = 1 if diff < 60 else 5
                    time.sleep(step)

                # ESTADO: EXECUTANDO O CLIQUE
                elif self.state == "EXECUTING":
                    print("\n\n" + "=" * 70)
                    self.log("AÇÃO", f"⏰ HORÁRIO ATINGIDO ({self.target_time.strftime('%H:%M:%S')})! Retomando meta...")
                    print("=" * 70)

                    self.log("AÇÃO", "Focando na janela do ChatGPT...")
                    focus_chatgpt_window()
                    time.sleep(1.0)

                    success = execute_click("btn_play.png")
                    self.cycle_count += 1

                    if success:
                        self.log("STATUS", f"Ciclo #{self.cycle_count} concluído! Meta retomada.")
                    else:
                        self.log("AVISO", "Tentativa de clique finalizada. Verifique se o botão já foi acionado.")

                    self.log("AGENTE", "Aguardando 30 segundos antes de reiniciar o monitoramento...")
                    time.sleep(30)

                    self.state = "MONITORING"
                    self.target_time = None
                    self.reset_time = None
                    self.log("AGENTE", "Retornando ao estado de MONITORAMENTO ativo para futuros limites.\n")

            except KeyboardInterrupt:
                print("\n\n[AGENTE] Encerrado pelo usuário.")
                break
            except Exception as e:
                self.log("ERRO", f"Ocorreu uma exceção não fatal: {e}")
                time.sleep(5)


if __name__ == "__main__":
    agent = ChatGPTWatchdogAgent(check_interval=8, extra_minutes=1)
    agent.run()
