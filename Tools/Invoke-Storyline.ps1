param(
    [ValidateSet('create', 'verify', 'build', 'presentation', 'author-map', 'soil', 'corridor', 'indonesian', 'post-sign')]
    [string]$Command = 'verify',
    [string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe',
    [int]$TimeoutSeconds = 1800
)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
if (!(Test-Path -LiteralPath $Unity)) { throw 'Unity 6000.3.6f1 was not found.' }
$open = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" |
    Where-Object { $_.CommandLine -and $_.CommandLine.Contains($project) }
if ($open) { throw 'Close this project in Unity before running the batch storyline command.' }
$methods = @{ create = 'CreateChapter'; verify = 'VerifyChapter'; build = 'BuildChapter'; presentation = 'UpdatePresentation' }
$operation = if ($Command -eq 'author-map') { 'DisasterMapBuilder.BuildMap' } elseif ($Command -eq 'soil') { 'DisasterMapBuilder.RepairTerrainPalette' } elseif ($Command -eq 'corridor') { 'DisasterMapBuilder.ClearArrivalCorridor' } elseif ($Command -eq 'indonesian') { 'IndonesianSceneLocalization.Apply' } elseif ($Command -eq 'post-sign') { 'DisasterMapBuilder.RepairPostSign' } else { 'StorylineBuilder.' + $methods[$Command] }
$artifacts = Join-Path $project 'Artifacts'
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null
$log = Join-Path $artifacts ('storyline-' + $Command + '.log')
$arguments = @('-batchmode', '-quit', '-projectPath', ('"' + $project + '"'),
    '-executeMethod', ('Rimbara.Editor.' + $operation), '-logFile', ('"' + $log + '"'))
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru
if (!$process.WaitForExit($TimeoutSeconds * 1000)) {
    $process.Kill()
    throw ('Storyline command timed out: ' + $log)
}
if ($process.ExitCode -ne 0) { throw ('Storyline command failed: ' + $log) }
Write-Output ('Storyline ' + $Command + ': OK; ' + $log)
