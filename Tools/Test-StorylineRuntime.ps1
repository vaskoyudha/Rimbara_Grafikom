param(
    [string]$Executable = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Builds\Storyline-Windows\RIMBARA-FromAshes.exe'),
    [int]$TimeoutSeconds = 600,
    [switch]$SkipEnding
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root $(if ($SkipEnding) { 'Artifacts\storyline-runtime-skip' } else { 'Artifacts\storyline-runtime' })
if (!(Test-Path -LiteralPath $Executable)) { throw 'Build the Disaster Windows executable first.' }
New-Item -ItemType Directory -Force -Path $output | Out-Null
$log = Join-Path $output 'player.log'
$reportPath = Join-Path $output 'report.json'
$arguments = @('--rimbara-smoke', ('--rimbara-output="' + $output + '"'),
    '-screen-width', '1600', '-screen-height', '900', '-screen-fullscreen', '0', '-logFile', ('"' + $log + '"'))
if ($SkipEnding) { $arguments += '--rimbara-ending-skip' }
$started = [DateTime]::UtcNow
$process = Start-Process -FilePath $Executable -ArgumentList $arguments -PassThru
if (!$process.WaitForExit($TimeoutSeconds * 1000)) {
    $process.Kill()
    throw ('Storyline runtime test timed out: ' + $log)
}
if (!(Test-Path -LiteralPath $reportPath) -or (Get-Item -LiteralPath $reportPath).LastWriteTimeUtc -lt $started) {
    throw ('No fresh storyline report; exit code ' + $process.ExitCode + ': ' + $log)
}
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
if ($process.ExitCode -ne 0 -or !$report.Passed) { throw ('Storyline runtime tests failed: ' + $reportPath) }
Write-Output ('Storyline runtime passed: ' + $report.Checks.Count + ' checks; ' + $reportPath)
