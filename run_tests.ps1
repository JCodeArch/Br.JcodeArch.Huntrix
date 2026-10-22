$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe"
$projectPath = "D:\Workspace\Br.JcodeArch.Huntrix"
$resultsPath = "D:\Workspace\Br.JcodeArch.Huntrix\playmode-new-results.xml"
$logPath = "D:\Workspace\Br.JcodeArch.Huntrix\playmode-new-log.txt"

& $unityPath -batchmode -nographics -projectPath $projectPath -runTests -testPlatform playmode -testResults $resultsPath -logFile $logPath