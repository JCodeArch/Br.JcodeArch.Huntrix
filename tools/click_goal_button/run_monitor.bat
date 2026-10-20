@echo off
title Monitor de Limite de Uso - Retomada Automatica
cd /d "%~dp0"
echo Iniciando Monitor de Limite...
".venv\Scripts\python.exe" monitor_limit_and_click.py %*
pause
