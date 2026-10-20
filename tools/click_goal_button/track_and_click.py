"""
Script para rastrear o aviso de limite da Meta ("Meta com uso limitado ... Xh Ym Zs"),
calcular o tempo de expiração e disparar o clique no botão de Play 1 minuto após a liberação.

Modos de uso:
    1. Automático via OCR (requer winocr ou leitura de tela):
       python track_and_click.py

    2. Informando o tempo diretamente via linha de comando:
       python track_and_click.py --time "13h 17m 3s"
       python track_and_click.py -H 13 -M 17 -S 3

Dependências recomendadas:
    pip install pyautogui pillow opencv-python winocr
"""

import argparse
import asyncio
import os
import re
import sys
import time
from datetime import datetime, timedelta

try:
    import pyautogui
except ImportError:
    print("\n[ERRO] O módulo 'pyautogui' não está instalado.")
    print("Execute: pip install pyautogui pillow opencv-python\n")
    sys.exit(1)


def parse_time_string(time_str):
    """Extrai horas, minutos e segundos de strings como '13h 17m 3s', '13h17m', '45m 10s', etc."""
    pattern = r'(?:(\d+)\s*h)?\s*(?:(\d+)\s*m)?\s*(?:(\d+)\s*s)?'
    match = re.search(pattern, time_str, re.IGNORECASE)
    if not match or not any(match.groups()):
        return None

    h = int(match.group(1)) if match.group(1) else 0
    m = int(match.group(2)) if match.group(2) else 0
    s = int(match.group(3)) if match.group(3) else 0
    return h, m, s


def scan_screen_for_countdown():
    """Tenta ler o texto da tela usando winocr (OCR nativo do Windows 10/11)."""
    try:
        import winocr
    except ImportError:
        print("[INFO] Biblioteca 'winocr' não encontrada. Para detecção automática via OCR:")
        print("       pip install winocr")
        return None

    print("\n[OCR] Capturando tela e analisando textos visíveis...")
    screenshot = pyautogui.screenshot()

    # Executa OCR nativo do Windows (assíncrono no winocr)
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        result = loop.run_until_complete(winocr.recognize_pil(screenshot, "pt"))
    finally:
        loop.close()

    text = result.text
    print(f"[OCR] Texto detectado na tela:\n---\n{text}\n---")

    # Procura padrões de contagem como: '13h 17m 3s' ou '13h 17m'
    match = re.search(r'(\d+)\s*h\s*(\d+)\s*m(?:\s*(\d+)\s*s)?', text, re.IGNORECASE)
    if match:
        h = int(match.group(1))
        m = int(match.group(2))
        s = int(match.group(3)) if match.group(3) else 0
        print(f"[OCR] Contagem regressiva identificada: {h}h {m}m {s}s")
        return h, m, s

    return None


def click_play_button(template_name="btn_play.png", max_retries=10, delay=1.5):
    """Procura o botão na tela e clica."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    template_path = os.path.join(script_dir, template_name)

    if not os.path.exists(template_path):
        print(f"[ERRO] Imagem do botão não encontrada: {template_path}")
        return False

    print(f"\n[AÇÃO] Procurando o botão de Play ('{template_name}') na tela...")
    confidences = [0.85, 0.80, 0.75, 0.70]

    for attempt in range(1, max_retries + 1):
        print(f"Tentativa de clique {attempt} de {max_retries}...")
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
            target_x, target_y = location
            print(f"[SUCESSO] Botão encontrado em X={target_x}, Y={target_y}")
            pyautogui.moveTo(target_x, target_y, duration=0.4)
            time.sleep(0.1)
            pyautogui.click()
            print("[CONCLUÍDO] Botão de Play clicado com sucesso!")
            return True

        time.sleep(delay)

    print("[AVISO] Não foi possível localizar o botão de Play na tela.")
    return False


def wait_and_click(hours, minutes, seconds, extra_delay_minutes=1):
    total_seconds = (hours * 3600) + (minutes * 60) + seconds
    extra_seconds = extra_delay_minutes * 60
    final_wait_seconds = total_seconds + extra_seconds

    now = datetime.now()
    target_time = now + timedelta(seconds=final_wait_seconds)
    reset_time = now + timedelta(seconds=total_seconds)

    print("\n" + "="*60)
    print(f" HORÁRIO ATUAL:             {now.strftime('%d/%m/%Y %H:%M:%S')}")
    print(f" EXPIRAÇÃO DO LIMITE:      {reset_time.strftime('%d/%m/%Y %H:%M:%S')} ({hours}h {minutes}m {seconds}s)")
    print(f" DISPARO DO CLIQUE (+{extra_delay_minutes}m): {target_time.strftime('%d/%m/%Y %H:%M:%S')}")
    print("="*60 + "\n")

    print("[AGUARDANDO] Iniciando contagem regressiva...")
    print("Pressione Ctrl+C a qualquer momento para cancelar.\n")

    try:
        while True:
            remaining = int((target_time - datetime.now()).total_seconds())
            if remaining <= 0:
                break

            rem_h = remaining // 3600
            rem_m = (remaining % 3600) // 60
            rem_s = remaining % 60

            status = f"\r[TEMPO RESTANTE PARA O CLIQUE] {rem_h:02d}h {rem_m:02d}m {rem_s:02d}s (Alvo: {target_time.strftime('%H:%M:%S')}) "
            sys.stdout.write(status)
            sys.stdout.flush()

            # Atualiza a cada 1 segundo se faltar menos de 60s, ou a cada 5s se faltar mais
            sleep_step = 1 if remaining < 60 else 5
            time.sleep(sleep_step)

        print("\n\n[ALVO ALCANÇADO] 1 minuto após o fim do limite atingido!")
        click_play_button()

    except KeyboardInterrupt:
        print("\n\n[CANCELADO] Execução interrompida pelo usuário.")


def main():
    parser = argparse.ArgumentParser(description="Monitora tempo de limite da Meta e clica no Play após liberação.")
    parser.add_argument("--time", "-t", type=str, help="Tempo da mensagem (ex: '13h 17m 3s')")
    parser.add_argument("--hours", "-H", type=int, default=0, help="Horas restantes")
    parser.add_argument("--minutes", "-M", type=int, default=0, help="Minutos restantes")
    parser.add_argument("--seconds", "-S", type=int, default=0, help="Segundos restantes")
    parser.add_argument("--extra-delay", "-e", type=int, default=1, help="Minutos extras após o término (padrão: 1)")

    args = parser.parse_args()

    h, m, s = args.hours, args.minutes, args.seconds

    # Se informado via string --time
    if args.time:
        parsed = parse_time_string(args.time)
        if parsed:
            h, m, s = parsed
        else:
            print(f"[ERRO] Formato de tempo inválido: '{args.time}'. Use ex: '13h 17m 3s'")
            sys.exit(1)

    # Se não passou parâmetros de tempo, tenta escanear via OCR
    if h == 0 and m == 0 and s == 0:
        ocr_result = scan_screen_for_countdown()
        if ocr_result:
            h, m, s = ocr_result
        else:
            print("\n[CONFIGURAÇÃO MANUAL]")
            user_input = input("Digite o tempo que aparece na mensagem (ex: 13h 17m 3s): ").strip()
            parsed = parse_time_string(user_input)
            if parsed:
                h, m, s = parsed
            else:
                print("[ERRO] Não foi possível interpretar o tempo informado.")
                sys.exit(1)

    wait_and_click(h, m, s, extra_delay_minutes=args.extra_delay)


if __name__ == "__main__":
    main()
