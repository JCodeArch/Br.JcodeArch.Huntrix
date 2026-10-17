<#
.SYNOPSIS
    Auto Clicker - monitora a tela e clica quando encontra qualquer uma das imagens de botão informadas.

.EXAMPLE
    .\AutoClicker.ps1 -ButtonImages .\buttons
    Usa todas as imagens (png/jpg/bmp) da pasta "buttons".

.EXAMPLE
    .\AutoClicker.ps1 -ButtonImages ok.png, aceitar.png, continuar.png -TimeoutSeconds 0
    Usa uma lista de arquivos e roda sem limite de tempo (Ctrl+C para parar).

.EXAMPLE
    .\AutoClicker.ps1 -ButtonImages .\buttons -StopAfterFirstClick
    Comportamento do script original: clica uma vez e encerra.
#>
param(
    # Arquivos, pastas ou wildcards (ex.: *.png). Aceita vários valores.
    [string[]]$ButtonImages = @('button.png'),

    [double]$IntervalSeconds = 2,

    # 0 = sem limite de tempo
    [int]$TimeoutSeconds = 300,

    # Tempo de espera após um clique (evita clicar duas vezes no mesmo botão)
    [double]$CooldownSeconds = 3,

    # Tolerância de cor por canal (0-255)
    [ValidateRange(0, 255)]
    [int]$Tolerance = 20,

    # Fração mínima de pixels que precisam bater (0.0 a 1.0)
    [ValidateRange(0.1, 1.0)]
    [double]$MinMatch = 0.9,

    # Procura em todos os monitores (padrão: apenas o principal)
    [switch]$AllScreens,

    # Encerra depois do primeiro clique
    [switch]$StopAfterFirstClick,

    # Mostra informações de diagnóstico
    [switch]$ShowDebug
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# ---------------------------------------------------------------------------
# APIs nativas (mouse + DPI)
# ---------------------------------------------------------------------------
if (-not ('AutoClicker.NativeInput' -as [type])) {
    Add-Type -Namespace AutoClicker -Name NativeInput -MemberDefinition @'
[StructLayout(LayoutKind.Sequential)]
public struct MOUSEINPUT {
    public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public UIntPtr dwExtraInfo;
}
[StructLayout(LayoutKind.Sequential)]
public struct INPUT {
    public uint type; public MOUSEINPUT mi;
}
[StructLayout(LayoutKind.Sequential)]
public struct POINT { public int X; public int Y; }

[DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
[DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT p);
[DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
[DllImport("user32.dll", SetLastError = true)]
public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

// Retorna a quantidade de eventos injetados (esperado: 2)
public static uint LeftClick() {
    INPUT[] inputs = new INPUT[2];
    inputs[0].type = 0; inputs[0].mi.dwFlags = 0x0002; // LEFTDOWN
    inputs[1].type = 0; inputs[1].mi.dwFlags = 0x0004; // LEFTUP
    return SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
}

public static int[] GetPos() {
    POINT p; GetCursorPos(out p);
    return new int[] { p.X, p.Y };
}
'@
}

# ---------------------------------------------------------------------------
# Comparador de imagens em C# (muito mais rápido que GetPixel no PowerShell)
# Trabalha apenas com byte[] BGRA, sem depender de System.Drawing.
# ---------------------------------------------------------------------------
if (-not ('AutoClicker.ImageMatcher' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;

namespace AutoClicker
{
    public static class ImageMatcher
    {
        // Retorna {x, y} do canto superior esquerdo da primeira ocorrência, ou null.
        public static int[] Find(byte[] src, int sw, int sh,
                                 byte[] tpl, int tw, int th,
                                 int tolerance, double minMatch)
        {
            if (tw > sw || th > sh) return null;

            // Amostra uma grade de pixels do template (~400 pontos), ignorando transparentes
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
try { [void][AutoClicker.NativeInput]::SetProcessDPIAware() } catch { }

# Aviso: se o app alvo roda como Administrador, um PowerShell sem elevação NÃO consegue clicar nele (UIPI)
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host 'Aviso: PowerShell sem privilégios de administrador. Se o botão estiver em um app executado como admin, o clique será ignorado.' -ForegroundColor DarkYellow
}

# ---------------------------------------------------------------------------
# Funções auxiliares
# ---------------------------------------------------------------------------
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

function Get-ButtonTemplates {
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

function Invoke-LeftClick {
    param([int]$X, [int]$Y)

    [void][AutoClicker.NativeInput]::SetCursorPos($X, $Y)
    Start-Sleep -Milliseconds 150   # dá tempo do app registrar o hover

    $actual = [AutoClicker.NativeInput]::GetPos()
    if ($ShowDebug -or [Math]::Abs($actual[0] - $X) -gt 2 -or [Math]::Abs($actual[1] - $Y) -gt 2) {
        Write-Host "Debug: alvo X:$X Y:$Y | cursor real X:$($actual[0]) Y:$($actual[1])" -ForegroundColor DarkGray
    }

    $sent = [AutoClicker.NativeInput]::LeftClick()
    if ($sent -ne 2) {
        Write-Host "Aviso: SendInput injetou $sent de 2 eventos (erro Win32: $([Runtime.InteropServices.Marshal]::GetLastWin32Error()))" -ForegroundColor Red
    }
}

# ---------------------------------------------------------------------------
# Início
# ---------------------------------------------------------------------------
Write-Host 'Auto Clicker iniciado' -ForegroundColor Green

$templates = @(Get-ButtonTemplates -Paths $ButtonImages)

if ($templates.Count -eq 0) {
    Write-Host 'ERRO: nenhuma imagem de botão encontrada!' -ForegroundColor Red
    Write-Host "Informado: $($ButtonImages -join ', ')" -ForegroundColor Red
    Write-Host 'Capture os botões e salve como .png (ou passe uma pasta em -ButtonImages).' -ForegroundColor Red
    exit 1
}

Write-Host "Imagens carregadas ($($templates.Count)):" -ForegroundColor Yellow
foreach ($t in $templates) {
    Write-Host "  - $($t.Name) ($($t.Width)x$($t.Height))" -ForegroundColor Yellow
}
Write-Host "Intervalo: $IntervalSeconds s | Tolerância: $Tolerance | Match mínimo: $([int]($MinMatch * 100))%" -ForegroundColor Yellow
if ($TimeoutSeconds -gt 0) {
    Write-Host "Timeout: $TimeoutSeconds s" -ForegroundColor Yellow
}
else {
    Write-Host 'Timeout: desativado (Ctrl+C para parar)' -ForegroundColor Yellow
}

$startTime = Get-Date
$clicks = 0
$stop = $false

try {
    while (-not $stop) {
        if ($TimeoutSeconds -gt 0 -and ((Get-Date) - $startTime).TotalSeconds -ge $TimeoutSeconds) {
            Write-Host "Timeout atingido após $TimeoutSeconds segundos" -ForegroundColor Red
            break
        }

        $area = if ($AllScreens) {
            [System.Windows.Forms.SystemInformation]::VirtualScreen
        }
        else {
            [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
        }

        $sw = [System.Diagnostics.Stopwatch]::StartNew()

        $shot = Get-ScreenCapture -Area $area
        try {
            if ($ShowDebug) {
                $debugPath = Join-Path $env:TEMP 'autoclicker-debug.png'
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

        $clicked = $false
        foreach ($t in $templates) {
            $pos = [AutoClicker.ImageMatcher]::Find(
                $srcBytes, $srcW, $srcH,
                $t.Bytes, $t.Width, $t.Height,
                $Tolerance, $MinMatch)

            if ($null -ne $pos) {
                $clickX = $area.X + $pos[0] + [int]($t.Width / 2)
                $clickY = $area.Y + $pos[1] + [int]($t.Height / 2)

                Write-Host "[$(Get-Date -Format 'HH:mm:ss')] '$($t.Name)' encontrado em X:$($pos[0]) Y:$($pos[1])" -ForegroundColor Green
                Invoke-LeftClick -X $clickX -Y $clickY
                $clicks++
                Write-Host "Clique realizado! (total: $clicks)" -ForegroundColor Green

                $clicked = $true
                break
            }
        }

        $sw.Stop()
        if ($ShowDebug) {
            Write-Host "Debug: varredura levou $($sw.ElapsedMilliseconds) ms" -ForegroundColor DarkGray
        }

        if ($clicked) {
            if ($StopAfterFirstClick) { $stop = $true; break }
            Start-Sleep -Seconds $CooldownSeconds
        }
        else {
            Start-Sleep -Seconds $IntervalSeconds
        }
    }
}
finally {
    if ($clicks -gt 0) {
        Write-Host "Concluído. Total de cliques: $clicks" -ForegroundColor Green
    }
    else {
        Write-Host 'Encerrado sem cliques.' -ForegroundColor Yellow
    }
}