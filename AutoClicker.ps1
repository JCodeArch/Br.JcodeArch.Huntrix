# Auto Clicker Script - Monitors for button image and clicks when found
param(
    [string]$ButtonImage = "button.png",
    [int]$IntervalSeconds = 2,
    [int]$TimeoutSeconds = 300,
    [switch]$Debug
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Add mouse event
Add-Type -MemberDefinition @'
[DllImport("user32.dll")]
public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
'@ -Name MouseAPI -Namespace AutoClicker

$MOUSEEVENTF_LEFTDOWN = 0x0002
$MOUSEEVENTF_LEFTUP = 0x0004

function Find-ImageMatch {
    param(
        [System.Drawing.Bitmap]$SourceImage,
        [System.Drawing.Image]$SearchImage
    )
    
    $searchBmp = [System.Drawing.Bitmap]$SearchImage
    for ($x = 0; $x -lt ($SourceImage.Width - $searchBmp.Width); $x++) {
        for ($y = 0; $y -lt ($SourceImage.Height - $searchBmp.Height); $y++) {
            $match = $true
            for ($px = 0; $px -lt [Math]::Min($searchBmp.Width, 5); $px++) {
                for ($py = 0; $py -lt [Math]::Min($searchBmp.Height, 5); $py++) {
                    $srcColor = $SourceImage.GetPixel($x + $px, $y + $py)
                    $searchColor = $searchBmp.GetPixel($px, $py)
                    if ([Math]::Abs($srcColor.R - $searchColor.R) -gt 15 -or 
                        [Math]::Abs($srcColor.G - $searchColor.G) -gt 15 -or 
                        [Math]::Abs($srcColor.B - $searchColor.B) -gt 15) {
                        $match = $false
                        break
                    }
                }
                if (-not $match) { break }
            }
            if ($match) {
                return New-Object System.Drawing.Point($x, $y)
            }
        }
    }
    return $null
}

Write-Host "Auto Clicker iniciado" -ForegroundColor Green
Write-Host "Procurando imagem: $ButtonImage" -ForegroundColor Yellow
Write-Host "Intervalo: $IntervalSeconds segundos" -ForegroundColor Yellow

if (-not (Test-Path $ButtonImage)) {
    Write-Host "ERRO: Imagem '$ButtonImage' não encontrada!" -ForegroundColor Red
    Write-Host "Por favor, capture a imagem do botão e salve como $ButtonImage" -ForegroundColor Red
    exit 1
}

$buttonImage = [System.Drawing.Image]::FromFile((Resolve-Path $ButtonImage))
$buttonWidth = $buttonImage.Width
$buttonHeight = $buttonImage.Height

$startTime = Get-Date
$found = $false

while ((-not $found) -and ((Get-Date) -lt $startTime.AddSeconds($TimeoutSeconds))) {
    $screen = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $screenshot = New-Object System.Drawing.Bitmap $screen.Width, $screen.Height
    $graphics = [System.Drawing.Graphics]::FromImage($screenshot)
    $graphics.CopyFromScreen(0, 0, 0, 0, $screenshot.Size)
    
    $foundPosition = Find-ImageMatch $screenshot $buttonImage
    $screenshot.Dispose()
    $graphics.Dispose()
    
    if ($foundPosition -ne $null) {
        Write-Host "Botão encontrado em X:$($foundPosition.X) Y:$($foundPosition.Y)" -ForegroundColor Green
        $clickX = $foundPosition.X + ($buttonWidth / 2)
        $clickY = $foundPosition.Y + ($buttonHeight / 2)
        [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point($clickX, $clickY)
        [AutoClicker.MouseAPI]::mouse_event($MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0)
        [AutoClicker.MouseAPI]::mouse_event($MOUSEEVENTF_LEFTUP, 0, 0, 0, 0)
        Write-Host "Clique realizado!" -ForegroundColor Green
        $found = $true
        break
    }
    
    Start-Sleep -Seconds $IntervalSeconds
}

if (-not $found) {
    Write-Host "Timeout atingido após $TimeoutSeconds segundos" -ForegroundColor Red
} else {
    Write-Host "Concluído com sucesso!" -ForegroundColor Green
}

$buttonImage.Dispose()
