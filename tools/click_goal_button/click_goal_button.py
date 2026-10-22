"""
Script para localizar o botão de Play/Retomar da Meta na tela e efetuar o clique.

Requisitos:
    pip install pyautogui pillow opencv-python

Uso:
    python click_goal_button.py
"""

import os
import sys
import time

try:
    import pyautogui
except ImportError:
    print("\n[ERRO] O módulo 'pyautogui' não está instalado.")
    print("Para instalar as dependências necessárias, execute:")
    print("    pip install pyautogui pillow opencv-python\n")
    sys.exit(1)


def find_and_click(template_name="btn_play.png", max_retries=5, delay=1.0):
    script_dir = os.path.dirname(os.path.abspath(__file__))
    template_path = os.path.join(script_dir, template_name)

    if not os.path.exists(template_path):
        print(f"[ERRO] Imagem de referência não encontrada: {template_path}")
        return False

    print(f"Procurando o botão na tela usando '{template_name}'...")
    print("Certifique-se de que a janela onde o botão está localizado esteja visível na tela.\n")

    # Tenta localizar com diferentes níveis de confiança se OpenCV estiver disponível
    confidences = [0.85, 0.80, 0.75, 0.70]

    for attempt in range(1, max_retries + 1):
        print(f"Tentativa {attempt} de {max_retries}...")
        location = None

        for conf in confidences:
            try:
                # Com OpenCV instalado, suporta o parâmetro 'confidence'
                location = pyautogui.locateCenterOnScreen(template_path, confidence=conf)
            except Exception:
                try:
                    # Fallback sem parâmetro confidence caso opencv não esteja instalado
                    location = pyautogui.locateCenterOnScreen(template_path)
                except Exception:
                    pass

            if location:
                break

        if location:
            target_x, target_y = location
            print(f"\n[SUCESSO] Botão encontrado nas coordenadas: X={target_x}, Y={target_y}")
            
            # Move o cursor suavemente até a posição e clica
            pyautogui.moveTo(target_x, target_y, duration=0.3)
            time.sleep(0.1)
            pyautogui.click()
            print("[CONCLUÍDO] Clique disparado com sucesso!\n")
            return True

        time.sleep(delay)

    print("\n[AVISO] Botão não foi encontrado na tela após as tentativas.")
    print("Dicas:")
    print("1. Certifique-se de que a barra da Meta não está oculta ou minimizada.")
    print("2. Verifique se o botão não mudou de estado (por exemplo, se já está rodando ou foi concluído).")
    return False


if __name__ == "__main__":
    find_and_click()
