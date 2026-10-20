"""
Monitorador de Limite de Uso e Agendador de Clique Automático

Este script:
1. Fica monitorando a tela procurando pela mensagem:
   "Você atingiu seu limite de uso... tente novamente na seguinte data ou horário: DD de MÊS de AAAA, HH:MM"
2. Ao detectar a mensagem, extrai a data e hora de liberação.
3. Agenda a execução do clique para exatamente 1 MINUTO DEPOIS do horário indicado.
4. Exibe uma contagem regressiva em tempo real.
5. Quando o horário chegar, localiza o botão de Play ('btn_play.png') e clica nele automaticamente.
"""

import argparse
import asyncio
import os
import re
import sys
import time
from datetime import datetime, timedelta

import pyautogui
from PIL import Image
import winocr

MONTHS = {
    'jan': 1, 'fev': 2, 'mar': 3, 'abr': 4, 'mai': 5, 'jun': 6,
    'jul': 7, 'ago': 8, 'set': 9, 'out': 10, 'nov': 11, 'dez': 12
}


def parse_limit_message(text):
    """
    Extrai a data e hora da mensagem de limite:
    Exemplo: '5 de out. de 2026 00:38' ou '05 de out de 2026, 00:38'
    """
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

    # Fallback se encontrar apenas o horário (ex: '00:38') próximo ao texto de limite
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


def click_play_button(template_name="btn_play.png", max_retries=15, delay=1.5):
    """Localiza o botão de Play na tela e executa o clique."""
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

    print("[AVISO] Não foi possível localizar o botão de Play na tela após várias tentativas.")
    return False


def ocr_screen():
    """Captura a tela inteira e reconhece o texto via OCR nativo do Windows."""
    try:
        screenshot = pyautogui.screenshot()
    except Exception as e:
        print(f"\n[AVISO OCR] Falha ao capturar a tela: {e}")
        return ""

    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        res = loop.run_until_complete(winocr.recognize_pil(screenshot, 'pt'))
        return res.text
    except Exception as e:
        print(f"\n[AVISO OCR] Erro no processamento de texto: {e}")
        return ""
    finally:
        loop.close()


def wait_until_target(click_time, reset_time):
    """Exibe contagem regressiva até o horário agendado e então aciona o clique."""
    print("\n" + "=" * 65)
    print(f" HORÁRIO ATUAL:                {datetime.now().strftime('%d/%m/%Y %H:%M:%S')}")
    print(f" HORÁRIO DE FIM DO LIMITE:     {reset_time.strftime('%d/%m/%Y %H:%M:%S')}")
    print(f" HORÁRIO DO CLIQUE (+1 MIN):   {click_time.strftime('%d/%m/%Y %H:%M:%S')}")
    print("=" * 65 + "\n")
    print("[AGENDADO] Aguardando o horário para executar o clique...")
    print("Pressione Ctrl+C para interromper a qualquer momento.\n")

    try:
        while True:
            now = datetime.now()
            diff = (click_time - now).total_seconds()
            if diff <= 0:
                break

            rem_h = int(diff // 3600)
            rem_m = int((diff % 3600) // 60)
            rem_s = int(diff % 60)

            msg = f"\r[CONTAGEM REGRESSIVA] {rem_h:02d}h {rem_m:02d}m {rem_s:02d}s restantes | Disparo às {click_time.strftime('%H:%M:%S')} "
            sys.stdout.write(msg)
            sys.stdout.flush()

            step = 1 if diff < 60 else 5
            time.sleep(step)

        print("\n\n" + "#" * 65)
        print(f" [HORÁRIO ATINGIDO: {click_time.strftime('%H:%M:%S')}] DISPARANDO CLIQUE AGORA!")
        print("#" * 65)

        click_play_button()

    except KeyboardInterrupt:
        print("\n\n[INTERROMPIDO] Monitoramento cancelado pelo usuário.")


def run_monitor(check_interval=10, extra_minutes=1):
    print("=" * 65)
    print("   MONITOR DE LIMITE DE USO - RETOMADA AUTOMÁTICA")
    print("=" * 65)
    print(f"Intervalo de verificação da tela: {check_interval} segundos")
    print(f"Acréscimo após liberação:         +{extra_minutes} minuto(s)")
    print("Aguardando mensagem na tela... (Ctrl+C para sair)\n")

    check_count = 0
    while True:
        check_count += 1
        now_str = datetime.now().strftime('%H:%M:%S')
        sys.stdout.write(f"\r[{now_str}] Verificação #{check_count}: Analisando tela... ")
        sys.stdout.flush()

        text = ocr_screen()
        if "limite" in text.lower() or "tente novamente" in text.lower():
            reset_time = parse_limit_message(text)
            if reset_time:
                click_time = reset_time + timedelta(minutes=extra_minutes)
                print(f"\n\n[MENSAGEM DE LIMITE DETECTADA!]")
                print(f"Trecho reconhecido: \"{text.strip()[:160]}...\"")
                wait_until_target(click_time, reset_time)
                break

        time.sleep(check_interval)


def main():
    parser = argparse.ArgumentParser(description="Monitora mensagem de limite e clica 1 minuto após liberação.")
    parser.add_argument("--interval", "-i", type=int, default=10, help="Intervalo de checagem da tela em segundos (padrão: 10)")
    parser.add_argument("--extra-minutes", "-e", type=int, default=1, help="Minutos extras após o horário do limite (padrão: 1)")
    parser.add_argument("--time", "-t", type=str, help="Definir manualmente data/hora (ex: '5 de out. de 2026, 00:38' ou '00:38')")

    args = parser.parse_args()

    if args.time:
        reset_time = parse_limit_message(args.time)
        if not reset_time:
            print(f"[ERRO] Formato de data/hora não reconhecido: {args.time}")
            sys.exit(1)
        click_time = reset_time + timedelta(minutes=args.extra_minutes)
        wait_until_target(click_time, reset_time)
    else:
        run_monitor(check_interval=args.interval, extra_minutes=args.extra_minutes)


if __name__ == "__main__":
    main()
