<#
.SYNOPSIS
    Agenda a execução do clique no botão Play para 1 minuto após o horário da mensagem de limite.
.DESCRIPTION
    Aceita a mensagem completa ou o horário (ex: "5 de out. de 2026, 00:38" ou "00:38").
.EXAMPLE
    .\track_and_click.ps1 -DateTime "5 de out. de 2026, 00:38"
    .\track_and_click.ps1 -DateTime "00:38"
    .\track_and_click.ps1
#>

param (
    [string]$DateTime = "",
    [int]$ExtraMinutes = 1
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$clickScript = Join-Path $scriptDir "click_goal_button.ps1"

function Parse-LimitDateTime([string]$inputStr) {
    $months = @{
        'jan'=1; 'fev'=2; 'mar'=3; 'abr'=4; 'mai'=5; 'jun'=6;
        'jul'=7; 'ago'=8; 'set'=9; 'out'=10; 'nov'=11; 'dez'=12
    }

    # Formato completo: "5 de out. de 2026, 00:38"
    if ($inputStr -match '(\d{1,2})\s+de\s+([a-zA-Zçáéíóú]+)\.?\s+de\s+(\d{4})[, ]+\s*(\d{1,2}):(\d{2})') {
        $d = [int]$matches[1]
        $mKey = $matches[2].ToLower().Substring(0, 3)
        $m = $months[$mKey]
        $y = [int]$matches[3]
        $h = [int]$matches[4]
        $min = [int]$matches[5]
        if ($m) {
            return (Get-Date -Year $y -Month $m -Day $d -Hour $h -Minute $min -Second 0)
        }
    }

    # Formato apenas horário: "00:38"
    if ($inputStr -match '(\d{1,2}):(\d{2})') {
        $h = [int]$matches[1]
        $min = [int]$matches[2]
        $now = Get-Date
        $t = Get-Date -Hour $h -Minute $min -Second 0
        if ($t -le $now) { $t = $t.AddDays(1) }
        return $t
    }

    return $null
}

if ($DateTime -eq "") {
    Write-Host "`n================================================================" -ForegroundColor Cyan
    Write-Host "   AGENDADOR DE CLIQUE - RETOMADA APOS FIM DO LIMITE" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan
    $DateTime = Read-Host "Cole a mensagem ou digite o horario (ex: '5 de out. de 2026, 00:38' ou '00:38')"
}

$resetTime = Parse-LimitDateTime $DateTime
if ($null -eq $resetTime) {
    Write-Error "Nao foi possivel identificar a data/hora em '$DateTime'."
    exit 1
}

$clickTime = $resetTime.AddMinutes($ExtraMinutes)
$now = Get-Date

Clear-Host
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  HORARIO ATUAL:               $($now.ToString('dd/MM/yyyy HH:mm:ss'))" -ForegroundColor White
Write-Host "  EXPIRACAO DO LIMITE:         $($resetTime.ToString('dd/MM/yyyy HH:mm:ss'))" -ForegroundColor Yellow
Write-Host "  DISPARO DO CLIQUE (+1 min):  $($clickTime.ToString('dd/MM/yyyy HH:mm:ss'))" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "`nAguardando horario programado... (Pressione Ctrl+C para cancelar)`n" -ForegroundColor DarkGray

while ($true) {
    $current = Get-Date
    $remaining = [int]($clickTime - $current).TotalSeconds

    if ($remaining -le 0) {
        break
    }

    $remH = [math]::Floor($remaining / 3600)
    $remM = [math]::Floor(($remaining % 3600) / 60)
    $remS = $remaining % 60

    $statusLine = "`r[TEMPO RESTANTE] $($remH.ToString('00'))h $($remM.ToString('00'))m $($remS.ToString('00'))s  |  Disparo as: $($clickTime.ToString('HH:mm:ss')) "
    Write-Host -NoNewline $statusLine -ForegroundColor Cyan

    if ($remaining -lt 60) {
        Start-Sleep -Seconds 1
    } else {
        Start-Sleep -Seconds 5
    }
}

Write-Host "`n`n================================================================" -ForegroundColor Green
Write-Host " [HORARIO ATINGIDO - $($clickTime.ToString('HH:mm:ss'))] Disparando clique agora!" -ForegroundColor Green
Write-Host "================================================================`n" -ForegroundColor Green

& powershell -ExecutionPolicy Bypass -File $clickScript
