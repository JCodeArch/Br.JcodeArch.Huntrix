# Monitors C:\temp for files and can trigger actions
param(
    [string]$WatchPath = "C:\temp",
    [string]$Filter = "*",
    [int]$IntervalSeconds = 1,
    [switch]$Debug
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -MemberDefinition @'
[DllImport("user32.dll")]
public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
'@ -Name MouseAPI -Namespace AutoClicker

$MOUSEEVENTF_LEFTDOWN = 0x0002
$MOUSEEVENTF_LEFTUP = 0x0004

Write-Host "Monitorando: $WatchPath" -ForegroundColor Green

$knownFiles = @{}
while ($true) {
    $files = Get-ChildItem -Path $WatchPath -Filter $Filter -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        $key = $file.FullName
        if (-not $knownFiles.ContainsKey($key)) {
            Write-Host "Arquivo detectado: $key" -ForegroundColor Yellow
            $knownFiles[$key] = $file.LastWriteTime
            # Example: click at current cursor position or specific coordinates
            # Uncomment and modify coordinates as needed
            # [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point(100, 100)
            # [AutoClicker.MouseAPI]::mouse_event($MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0)
            # [AutoClicker.MouseAPI]::mouse_event($MOUSEEVENTF_LEFTUP, 0, 0, 0, 0)
            # Write-Host "Clique realizado!" -ForegroundColor Green
        }
    }
    Start-Sleep -Seconds $IntervalSeconds
}
