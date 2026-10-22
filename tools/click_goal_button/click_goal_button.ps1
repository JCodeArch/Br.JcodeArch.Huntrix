<#
.SYNOPSIS
    Localiza a imagem do botão Play/Retomar na tela e executa um clique do mouse.
.DESCRIPTION
    Utiliza APIs nativas do Windows (.NET System.Drawing e User32.dll), não exigindo bibliotecas externas.
.EXAMPLE
    .\click_goal_button.ps1
#>

param (
    [string]$TemplateName = "btn_play.png",
    [double]$Tolerance = 0.22,
    [int]$MaxRetries = 5,
    [int]$DelaySeconds = 1
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$templatePath = Join-Path $scriptDir $TemplateName

if (!(Test-Path $templatePath)) {
    Write-Error "[ERRO] Arquivo de imagem não encontrado: $templatePath"
    exit 1
}

$csharpSource = @"
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class NativeScreenClicker {
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP   = 0x0004;

    public static Point FindAndClick(string templatePath, double tolerance = 0.22) {
        using (Bitmap template = new Bitmap(templatePath)) {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            using (Bitmap screen = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb)) {
                using (Graphics g = Graphics.FromImage(screen)) {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }

                BitmapData sData = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData tData = template.LockBits(new Rectangle(0, 0, template.Width, template.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                int sBytes = Math.Abs(sData.Stride) * screen.Height;
                int tBytes = Math.Abs(tData.Stride) * template.Height;

                byte[] sRgb = new byte[sBytes];
                byte[] tRgb = new byte[tBytes];

                Marshal.Copy(sData.Scan0, sRgb, 0, sBytes);
                Marshal.Copy(tData.Scan0, tRgb, 0, tBytes);

                screen.UnlockBits(sData);
                template.UnlockBits(tData);

                int sw = screen.Width, sh = screen.Height, sStride = sData.Stride;
                int tw = template.Width, th = template.Height, tStride = tData.Stride;

                long maxAllowedDiff = (long)((tw * th * 3 * 255) * tolerance);

                for (int sy = 0; sy <= sh - th; sy += 2) {
                    for (int sx = 0; sx <= sw - tw; sx += 2) {
                        long totalDiff = 0;
                        bool match = true;

                        for (int ty = 0; ty < th; ty += 2) {
                            int sRow = (sy + ty) * sStride + sx * 3;
                            int tRow = ty * tStride;

                            for (int tx = 0; tx < tw; tx += 2) {
                                int sIdx = sRow + tx * 3;
                                int tIdx = tRow + tx * 3;

                                int diff = Math.Abs(sRgb[sIdx] - tRgb[tIdx]) +
                                           Math.Abs(sRgb[sIdx + 1] - tRgb[tIdx + 1]) +
                                           Math.Abs(sRgb[sIdx + 2] - tRgb[tIdx + 2]);
                                totalDiff += diff;
                                if (totalDiff > maxAllowedDiff) {
                                    match = false;
                                    break;
                                }
                            }
                            if (!match) break;
                        }

                        if (match) {
                            int targetX = sx + tw / 2;
                            int targetY = sy + th / 2;
                            SetCursorPos(targetX, targetY);
                            System.Threading.Thread.Sleep(80);
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            System.Threading.Thread.Sleep(80);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            return new Point(targetX, targetY);
                        }
                    }
                }
            }
        }
        return Point.Empty;
    }
}
"@

Write-Host "Compilando módulo de automação de tela..." -ForegroundColor Cyan
Add-Type -TypeDefinition $csharpSource -ReferencedAssemblies System.Drawing, System.Windows.Forms -ErrorAction SilentlyContinue

Write-Host "Procurando o botão '$TemplateName' na tela..." -ForegroundColor Yellow

for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
    Write-Host "Tentativa $attempt de $MaxRetries..."
    try {
        $point = [NativeScreenClicker]::FindAndClick($templatePath, $Tolerance)
        if ($point -and ($point.X -gt 0 -or $point.Y -gt 0)) {
            Write-Host "`n[SUCESSO] Botão encontrado em X=$($point.X), Y=$($point.Y)" -ForegroundColor Green
            Write-Host "[CONCLUÍDO] Clique do mouse efetuado com sucesso!`n" -ForegroundColor Green
            exit 0
        }
    } catch {
        Write-Warning "Falha na captura ou pesquisa: $_"
    }
    Start-Sleep -Seconds $DelaySeconds
}

Write-Host "`n[AVISO] Botão não foi encontrado na tela." -ForegroundColor Red
Write-Host "Dica: Garanta que o botão ou janela esteja visível e não minimizado/coberto por outra janela."
