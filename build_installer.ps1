$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$outExe = "C:\Users\Administrator\.gemini\antigravity-ide\scratch\VoicePure-Windows\Jhonlloyd-Noise-cancellation.exe"
$res1 = "/resource:C:\Users\Administrator\.gemini\antigravity-ide\scratch\VoicePure-Windows\bin\EqualizerAPO-x64.exe,EqualizerAPO-x64.exe"
$res2 = "/resource:C:\Users\Administrator\.gemini\antigravity-ide\scratch\VoicePure-Windows\bin\rnnoise_mono.dll,rnnoise_mono.dll"
$res3 = "/resource:C:\Users\Administrator\.gemini\antigravity-ide\scratch\VoicePure-Windows\presets\deep-crisp.txt,config.txt"
$res4 = "/resource:C:\Users\Administrator\.gemini\antigravity-ide\scratch\VoicePure-Windows\src\MicLock\Program.cs,MicLock.cs"
$src1 = "C:\Users\Administrator\.gemini\antigravity-ide\scratch\VoicePure-Windows\src\Installer\InstallerMain.cs"
$src2 = "C:\Users\Administrator\.gemini\antigravity-ide\scratch\VoicePure-Windows\src\Installer\Configurator.cs"

$args = @(
    "/nologo",
    "/target:exe",
    "/out:$outExe",
    $res1,
    $res2,
    $res3,
    $res4,
    $src1,
    $src2
)

Write-Output "Compiling standalone installer..."
& $csc $args
if ($LASTEXITCODE -eq 0) {
    Write-Output "SUCCESS: Compiled $outExe"
} else {
    Write-Output "FAILED with exit code $LASTEXITCODE"
}
