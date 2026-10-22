@echo off
title Agente Watchdog - ChatGPT Auto-Resumer
cd /d "%~dp0"
echo ===================================================
echo     Iniciando Agente Watchdog do ChatGPT...
echo ===================================================
".venv\Scripts\python.exe" chatgpt_agent.py
pause
