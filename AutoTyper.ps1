<#
.SYNOPSIS
    Espera um horário, localiza uma caixa de texto na tela pela imagem, clica nela e digita o texto.

.EXAMPLE
    .\AutoTyper.ps1 -Time 14:30 -Text "Olá, mundo" -TextBoxImage .\textbox.png
    Às 14:30 procura a caixa, clica e digita o texto.

.EXAMPLE
    .\AutoTyper.ps1 -Time 08:00:00 -Text "Bom dia" -TextBoxImage .\caixas -PressEnter
    Usa todas as imagens da pasta "caixas" e pressiona Enter depois de digitar.

.EXAMPLE
    .\AutoTyper.ps1 -Time 14:30 -Text "teste" -TextBoxImage .\textbox.png -DryRun
    Teste: não espera o horário, só procura a caixa e move o mouse até ela (sem clicar nem digitar).

.EXAMPLE
    .\AutoTyper.ps1 -Time "2026-10-17 09:15" -Text "Linha 1`nLinha 2" -TextBoxImage .\textbox.png
    Data e hora completas; `n gera uma quebra de linha (Enter).
#>
param(
    # "HH:mm", "HH:mm:ss", "yyyy-MM-dd HH:mm[:ss]" ou "dd/MM/yyyy HH:mm[:ss]"
    [Parameter(Mandatory)]
    [string]$Time,

    # Texto a ser digitado (use `n para quebra de linha)
    [Parameter(Mandatory)]
    [string]$Text,

    # Arquivos, pastas ou wildcards com imagens da caixa de texto
    [string[]]$TextBoxImage = @('textbox.png'),

    # Ajuste do ponto de clique em relação ao centro da imagem (pixels)
    [int]$OffsetX = 0,
    [int]$OffsetY = 0,

    # Quanto tempo continuar procurando a caixa depois que o horário chegar
    [int]$SearchTimeoutSeconds = 30,

    # Intervalo entre tentativas de busca
    [double]$IntervalSeconds = 0.5,

    # Tolerância de cor por canal (0-255) e fração mínima de pixels que devem bater
    [ValidateRange(0, 255)]
    [int]$Tolerance = 20,
    [ValidateRange(0.1, 1.0)]
    [double]$MinMatch = 0.9,

    # Atraso entre caracteres digitados (ms)
    [int]$TypeDelayMs = 10,

    # Seleciona tudo (Ctrl+A) antes de digitar, substituindo o conteúdo atual
    [switch]$ReplaceExisting,

    # Pressiona Enter depois de digitar
    [switch]$PressEnter,

    # Em vez de digitar caractere a caractere, cola via área de transferência (Ctrl+V).
    # Útil em apps que ignoram entrada Unicode. Restaura o texto anterior da área de transferência.
    [switch]$Paste,

    # Procura em todos os monitores (padrão: apenas o principal)
    [switch]$AllScreens,

    # Não espera o horário e não clica/digita: só localiza a caixa e move o mouse
    [switch]$DryRun,

    # Diagnóstico (salva captura em %TEMP%\autotyper-debug.png)
    [switch]$ShowDebug
)

$ErrorActionPreference = 'Stop'

if (-not [Environment]::Is64BitProcess) {
    Write-Host 'ERRO: este script requer PowerShell 7 de 64 bits.' -ForegroundColor Red
    exit 1
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# ---------------------------------------------------------------------------
# APIs nativas: mouse, teclado (SendInput) e DPI
# ---------------------------------------------------------------------------
if (-not ('AutoTyper.NativeInput' -as [type])) {
    Add-Type -Namespace AutoTyper -Name NativeInput -MemberDefinition @'
[StructLayout(LayoutKind.Sequential)]
public struct MOUSEINPUT {
    public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public UIntPtr dwExtraInfo;
}
[StructLayout(LayoutKind.Sequential)]
public struct KEYBDINPUT {
    public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public UIntPtr dwExtraInfo;
}
[StructLayout(LayoutKind.Explicit)]
public struct INPUT {
    [FieldOffset(0)] public uint type;
    [FieldOffset(8)] public MOUSEINPUT mi;
    [FieldOffset(8)] public KEYBDINPUT ki;
}
[StructLayout(LayoutKind.Sequential)]
public struct POINT { public int X; public int Y; }

[DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
[DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT p);
[DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
[DllImport("user32.dll", SetLastError = true)]
public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

public static uint LeftClick() {
    INPUT[] i = new INPUT[2];
    i[0].type = 0; i[0].mi.dwFlags = 0x0002; // LEFTDOWN
    i[1].type = 0; i[1].mi.dwFlags = 0x0004; // LEFTUP
    return SendInput(2, i, Marshal.SizeOf(typeof(INPUT)));
}

public static void KeyEvent(ushort vk, bool up) {
    INPUT[] i = new INPUT[1];
    i[0].type = 1;
    i[0].ki.wVk = vk;
    i[0].ki.dwFlags = up ? 0x0002u : 0u;
    SendInput(1, i, Marshal.SizeOf(typeof(INPUT)));
}

public static void PressKey(ushort vk) {
    KeyEvent(vk, false);
    KeyEvent(vk, true);
}

// Digita via KEYEVENTF_UNICODE: funciona com acentos e símbolos, independente do layout do teclado
public static void TypeText(string text, int delayMs) {
    int size = Marshal.SizeOf(typeof(INPUT));
    foreach (char c in text) {
        if (c == '\r') continue;
        if (c == '\n') {
            PressKey(0x0D); // ENTER
        } else {
            INPUT[] i = new INPUT[2];
            i[0].type = 1; i[0].ki.wScan = c; i[0].ki.dwFlags = 0x0004;          // UNICODE
            i[1].type = 1; i[1].ki.wScan = c; i[1].ki.dwFlags = 0x0004 | 0x0002; // UNICODE | KEYUP
            SendInput(2, i, size);
        }
        if (delayMs > 0) System.Threading.Thread.Sleep(delayMs);
    }
}

public static int[] GetPos() {
    POINT p; GetCursorPos(out p);
    return new int[] { p.X, p.Y };
}
'@
}

# ---------------------------------------------------------------------------
# Comparador de imagens em C# (trabalha com byte[] BGRA)
# ---------------------------------------------------------------------------
if (-not ('AutoTyper.ImageMatcher' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;

namespace AutoTyper
{
    public static class ImageMatcher
    {
        public static int[] Find(byte[] src, int sw, int sh,
                                 byte[] tpl, int tw, int th,
                                 int tolerance, double minMatch)
        {
            if (tw > sw || th > sh) return null;

            int step = Math.Max(1, (int)Math.Sqrt((double)(tw * th) / 400.0));
            var sx = new List<int>(); var sy = new List<int>();
            var sb = new List<byte>(); var sg = new List<byte>(); var sr = new List<byte>();

            for (int y = 0; y < th; y += step)
            {
                for (int x = 0; x < tw; x += step)
                {
                    int i = (y * tw + x) * 4;
                    if (tpl[i + 3] < 128) continue;
                    sx.Add(x); sy.Add(y);
                    sb.Add(tpl[i]); sg.Add(tpl[i + 1]); sr.Add(tpl[i + 2]);
                }
            }

            int n = sx.Count;
            if (n == 0) return null;

            int[] ax = sx.ToArray(), ay = sy.ToArray();
            byte[] ab = sb.ToArray(), ag = sg.ToArray(), ar = sr.ToArray();
            int allowed = (int)Math.Floor(n * (1.0 - minMatch));

            for (int y = 0; y <= sh - th; y++)
            {
                for (int x = 0; x <= sw - tw; x++)
                {
                    int miss = 0;
                    bool ok = true;

                    for (int k = 0; k < n; k++)
                    {
                        int si = ((y + ay[k]) * sw + (x + ax[k])) * 4;
                        if (Math.Abs(src[si] - ab[k]) > tolerance ||
                            Math.Abs(src[si + 1] - ag[k]) > tolerance ||
                            Math.Abs(src[si + 2] - ar[k]) > tolerance)
                        {
                            if (++miss > allowed) { ok = false; break; }
                        }
                    }

                    if (ok) return new int[] { x, y };
                }
            }
            return null;
        }
    }
}
'@
}

# Evita coordenadas erradas em telas com escala (125%, 150%...)
try { [void][AutoTyper.NativeInput]::SetProcessDPIAware() } catch { }

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host 'Aviso: PowerShell sem privilégios de administrador. Se a caixa estiver em um app executado como admin, cliques e digitação serão ignorados.' -ForegroundColor DarkYellow
}

# ---------------------------------------------------------------------------
# Funções auxiliares
# ---------------------------------------------------------------------------
function Resolve-TargetTime {
    param([string]$Value)

    $value = $Value.Trim()
    $culture = [System.Globalization.CultureInfo]::InvariantCulture
    $formats = [string[]]@(
        'H:mm', 'H:mm:ss',
        'yyyy-MM-dd H:mm', 'yyyy-MM-dd H:mm:ss',
        'dd/MM/yyyy H:mm', 'dd/MM/yyyy H:mm:ss')

    $parsed = [datetime]::MinValue
    if (-not [datetime]::TryParseExact($value, $formats, $culture, [System.Globalization.DateTimeStyles]::None, [ref]$parsed)) {
        throw "Horário inválido: '$Value'. Use HH:mm, HH:mm:ss, yyyy-MM-dd HH:mm ou dd/MM/yyyy HH:mm."
    }

    $timeOnly = $value -notmatch '[-/]'
    $rolled = $false

    if ($timeOnly) {
        $target = (Get-Date).Date + $parsed.TimeOfDay
        if ($target -le (Get-Date)) {
            $target = $target.AddDays(1)
            $rolled = $true
        }
    }
    else {
        $target = $parsed
        if ($target -le (Get-Date)) {
            throw "O horário informado ($($target.ToString('dd/MM/yyyy HH:mm:ss'))) já passou."
        }
    }

    [pscustomobject]@{ Target = $target; Rolled = $rolled }
}

function Wait-Until {
    param([datetime]$Target)

    while ($true) {
        $remaining = $Target - (Get-Date)
        if ($remaining.TotalMilliseconds -le 0) { break }

        if ($remaining.TotalSeconds -gt 2) {
            $r = [timespan]::FromSeconds([Math]::Ceiling($remaining.TotalSeconds))
            Write-Host ("`rAguardando... faltam {0:00}:{1:00}:{2:00}   " -f [int]$r.TotalHours, $r.Minutes, $r.Seconds) -NoNewline -ForegroundColor Cyan
            Start-Sleep -Milliseconds ([int][Math]::Max(10, [Math]::Min(1000, ($remaining.TotalSeconds - 2) * 1000)))
        }
        else {
            Start-Sleep -Milliseconds 5   # reta final com precisão de milissegundos
        }
    }
    Write-Host ''
}

function Get-BitmapBytes {
    param([System.Drawing.Bitmap]$Bitmap)

    $rect = [System.Drawing.Rectangle]::new(0, 0, $Bitmap.Width, $Bitmap.Height)
    $data = $Bitmap.LockBits(
        $rect,
        [System.Drawing.Imaging.ImageLockMode]::ReadOnly,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $bytes = New-Object byte[] ($data.Stride * $Bitmap.Height)
        [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)
    }
    finally {
        $Bitmap.UnlockBits($data)
    }
    return , $bytes
}

function Get-ImageTemplates {
    param([string[]]$Paths)

    $extensions = '.png', '.jpg', '.jpeg', '.bmp'
    $files = foreach ($p in $Paths) {
        if (Test-Path -LiteralPath $p -PathType Container) {
            Get-ChildItem -LiteralPath $p -File | Where-Object { $_.Extension -in $extensions }
        }
        else {
            Get-ChildItem -Path $p -File -ErrorAction SilentlyContinue
        }
    }

    foreach ($file in ($files | Sort-Object FullName -Unique)) {
        $bmp = $null
        try {
            $bmp = [System.Drawing.Bitmap]::new($file.FullName)
            [pscustomobject]@{
                Name   = $file.Name
                Width  = $bmp.Width
                Height = $bmp.Height
                Bytes  = Get-BitmapBytes $bmp
            }
        }
        catch {
            Write-Host "Aviso: não foi possível carregar '$($file.Name)': $($_.Exception.Message)" -ForegroundColor DarkYellow
        }
        finally {
            if ($bmp) { $bmp.Dispose() }
        }
    }
}

function Get-ScreenCapture {
    param($Area)

    $bmp = [System.Drawing.Bitmap]::new($Area.Width, $Area.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    try {
        $g.CopyFromScreen($Area.X, $Area.Y, 0, 0, $bmp.Size)
    }
    finally {
        $g.Dispose()
    }
    return $bmp
}

function Find-TextBox {
    param($Templates, [bool]$SaveDebug)

    $area = if ($AllScreens) {
        [System.Windows.Forms.SystemInformation]::VirtualScreen
    }
    else {
        [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    }

    $shot = Get-ScreenCapture -Area $area
    try {
        if ($SaveDebug) {
            $debugPath = Join-Path $env:TEMP 'autotyper-debug.png'
            $shot.Save($debugPath, [System.Drawing.Imaging.ImageFormat]::Png)
            Write-Host "Debug: captura $($shot.Width)x$($shot.Height) salva em $debugPath" -ForegroundColor DarkGray
        }
        $srcBytes = Get-BitmapBytes $shot
        $srcW = $shot.Width
        $srcH = $shot.Height
    }
    finally {
        $shot.Dispose()
    }

    foreach ($t in $Templates) {
        $pos = [AutoTyper.ImageMatcher]::Find($srcBytes, $srcW, $srcH, $t.Bytes, $t.Width, $t.Height, $Tolerance, $MinMatch)
        if ($null -ne $pos) {
            return [pscustomobject]@{
                Name = $t.Name
                X    = $area.X + $pos[0]
                Y    = $area.Y + $pos[1]
                W    = $t.Width
                H    = $t.Height
            }
        }
    }
    return $null
}

function Move-CursorTo {
    param([int]$X, [int]$Y)

    [void][AutoTyper.NativeInput]::SetCursorPos($X, $Y)
    Start-Sleep -Milliseconds 150   # dá tempo do app registrar o hover

    $actual = [AutoTyper.NativeInput]::GetPos()
    if ($ShowDebug -or [Math]::Abs($actual[0] - $X) -gt 2 -or [Math]::Abs($actual[1] - $Y) -gt 2) {
        Write-Host "Debug: alvo X:$X Y:$Y | cursor real X:$($actual[0]) Y:$($actual[1])" -ForegroundColor DarkGray
    }
}

function Send-TextToField {
    param([string]$Value)

    if ($ReplaceExisting) {
        [AutoTyper.NativeInput]::KeyEvent(0x11, $false)   # Ctrl
        [AutoTyper.NativeInput]::PressKey(0x41)           # A
        [AutoTyper.NativeInput]::KeyEvent(0x11, $true)
        Start-Sleep -Milliseconds 80
    }

    if ($Paste) {
        $old = $null
        try { $old = Get-Clipboard -Raw } catch { }
        Set-Clipboard -Value $Value
        Start-Sleep -Milliseconds 100
        [AutoTyper.NativeInput]::KeyEvent(0x11, $false)   # Ctrl
        [AutoTyper.NativeInput]::PressKey(0x56)           # V
        [AutoTyper.NativeInput]::KeyEvent(0x11, $true)
        Start-Sleep -Milliseconds 300
        if ($null -ne $old) { Set-Clipboard -Value $old }
    }
    else {
        [AutoTyper.NativeInput]::TypeText($Value, $TypeDelayMs)
    }

    if ($PressEnter) {
        Start-Sleep -Milliseconds 80
        [AutoTyper.NativeInput]::PressKey(0x0D)
    }
}

# ---------------------------------------------------------------------------
# Início
# ---------------------------------------------------------------------------
Write-Host 'Auto Typer iniciado' -ForegroundColor Green

$schedule = Resolve-TargetTime -Value $Time
$target = $schedule.Target

$templates = @(Get-ImageTemplates -Paths $TextBoxImage)
if ($templates.Count -eq 0) {
    Write-Host 'ERRO: nenhuma imagem de caixa de texto encontrada!' -ForegroundColor Red
    Write-Host "Informado: $($TextBoxImage -join ', ')" -ForegroundColor Red
    exit 1
}

Write-Host "Imagens carregadas ($($templates.Count)):" -ForegroundColor Yellow
foreach ($t in $templates) {
    Write-Host "  - $($t.Name) ($($t.Width)x$($t.Height))" -ForegroundColor Yellow
}
Write-Host "Texto: $($Text.Length) caractere(s) | Modo: $(if ($Paste) { 'colar (Ctrl+V)' } else { 'digitar' })" -ForegroundColor Yellow

if ($DryRun) {
    Write-Host 'DryRun: não espera o horário e não clica nem digita.' -ForegroundColor Magenta
}
else {
    if ($schedule.Rolled) {
        Write-Host 'Aviso: o horário de hoje já passou; agendado para amanhã.' -ForegroundColor DarkYellow
    }
    Write-Host "Execução agendada para: $($target.ToString('dd/MM/yyyy HH:mm:ss'))" -ForegroundColor Yellow
    Write-Host 'Mantenha o computador ligado, desbloqueado e com a caixa de texto visível na tela.' -ForegroundColor DarkGray
    Wait-Until -Target $target
}

# Procura a caixa (com novas tentativas até o timeout)
$deadline = (Get-Date).AddSeconds($SearchTimeoutSeconds)
$match = $null
$firstTry = $true

while ($true) {
    $match = Find-TextBox -Templates $templates -SaveDebug:($ShowDebug -and $firstTry)
    $firstTry = $false
    if ($null -ne $match) { break }
    if ((Get-Date) -ge $deadline) { break }
    Start-Sleep -Milliseconds ([int]($IntervalSeconds * 1000))
}

if ($null -eq $match) {
    Write-Host "ERRO: caixa de texto não encontrada em $SearchTimeoutSeconds segundos." -ForegroundColor Red
    Write-Host 'Dica: rode com -ShowDebug -DryRun, compare %TEMP%\autotyper-debug.png com a sua imagem e ajuste -Tolerance / -MinMatch.' -ForegroundColor DarkYellow
    exit 2
}

$clickX = $match.X + [int]($match.W / 2) + $OffsetX
$clickY = $match.Y + [int]($match.H / 2) + $OffsetY

Write-Host "[$(Get-Date -Format 'HH:mm:ss.fff')] '$($match.Name)' encontrada em X:$($match.X) Y:$($match.Y)" -ForegroundColor Green

if ($DryRun) {
    Move-CursorTo -X $clickX -Y $clickY
    Write-Host 'DryRun concluído: o mouse foi movido até o ponto de clique.' -ForegroundColor Magenta
    exit 0
}

Move-CursorTo -X $clickX -Y $clickY

$sent = [AutoTyper.NativeInput]::LeftClick()
if ($sent -ne 2) {
    Write-Host "Aviso: SendInput injetou $sent de 2 eventos do clique (erro Win32: $([Runtime.InteropServices.Marshal]::GetLastWin32Error()))" -ForegroundColor Red
}
Start-Sleep -Milliseconds 250   # tempo para o campo receber o foco

Send-TextToField -Value $Text

Write-Host "[$(Get-Date -Format 'HH:mm:ss.fff')] Texto enviado. Concluído!" -ForegroundColor Green
