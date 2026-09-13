<#
.SYNOPSIS
    Local verification gate: runs the Unity Test Framework in batchmode.

.DESCRIPTION
    Wraps Unity -batchmode -runTests for EditMode/PlayMode tests.
      - Locates Unity.exe (fixed default, falls back to newest Unity Hub install)
      - Aborts if the project is locked by an open Editor (override with -Force)
      - Writes results XML + full log into TestResults/ (gitignored)
      - Fails on nonzero Unity exit code OR on failed tests parsed from the results XML
      - Does NOT pass -quit: with -runTests the Unity process terminates itself;
        combining the two is a known source of flaky runs.

    Verified against Unity 6000.6.0f1 / Test Framework 1.8.0 (see CLAUDE.md).

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/run-tests.ps1 -Mode EditMode

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/run-tests.ps1 -Mode All
#>
param(
    [ValidateSet('EditMode', 'PlayMode', 'All')]
    [string]$Mode = 'EditMode',

    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe',

    [int]$TimeoutMinutes = 45,

    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $scriptDir
$TestResultsDir = Join-Path $ProjectRoot 'TestResults'

function Find-UnityExe {
    param([string]$Preferred)

    if (Test-Path $Preferred) { return $Preferred }

    Write-Host "Unity not found at '$Preferred' - scanning Unity Hub installs..."
    $candidates = Get-ChildItem 'C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe' -ErrorAction SilentlyContinue
    $bestPath = $null
    $bestVersion = [version]'0.0'
    foreach ($candidate in $candidates) {
        if ($candidate.FullName -match 'Editor\\([0-9]+(?:\.[0-9]+)*)\\Editor\\Unity\.exe$') {
            try { $version = [version]$Matches[1] } catch { continue }
            if ($version -gt $bestVersion) {
                $bestVersion = $version
                $bestPath = $candidate.FullName
            }
        }
    }
    return $bestPath
}

function Invoke-UnityTestRun {
    param(
        [string]$TestPlatform,
        [string]$UnityExe,
        [string]$ProjRoot,
        [string]$ResultsDir,
        [string]$Stamp,
        [int]$TimeoutMin
    )

    $tag        = $TestPlatform.ToLower()
    $resultsXml = Join-Path $ResultsDir ("results-{0}-{1}.xml" -f $tag, $Stamp)
    $logFile    = Join-Path $ResultsDir ("unity-{0}-{1}.log" -f $tag, $Stamp)
    $consoleOut = Join-Path $ResultsDir ("console-{0}-{1}.txt" -f $tag, $Stamp)
    $consoleErr = Join-Path $ResultsDir ("console-{0}-{1}.err.txt" -f $tag, $Stamp)

    Write-Host ""
    Write-Host "=== Unity $TestPlatform tests ==="
    Write-Host "Results: $resultsXml"
    Write-Host "Log:     $logFile"

    # NOTE: no -quit here. -runTests terminates the Editor process itself;
    # adding -quit is a known cause of flaky/aborted test runs.
    $unityArgs = '-batchmode -projectPath "{0}" -runTests -testPlatform {1} -testResults "{2}" -logFile "{3}"' -f `
        $ProjRoot, $TestPlatform, $resultsXml, $logFile

    # Launched via the raw .NET Process API, not Start-Process: the PS 5.1
    # cmdlet does not reliably populate .ExitCode (observed empty even after
    # WaitForExit), while the .NET API guarantees it once WaitForExit(int)
    # returns true. Async readers drain stdout/stderr so the pipes cannot
    # fill up and deadlock the child process.
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $UnityExe
    $psi.Arguments = $unityArgs
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    $null = $process.Start()

    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()

    $timedOut = -not $process.WaitForExit($TimeoutMin * 60000)

    if ($timedOut) {
        Write-Host "TIMEOUT after $TimeoutMin minutes - killing Unity (possible import/compile stall)." -ForegroundColor Yellow
        Write-Host "Inspect the log before re-running: $logFile"
        try { $process.Kill() } catch { }
        return $false
    }

    # Flush redirected output to files for post-mortem inspection.
    $stdoutTask.Result | Set-Content -Path $consoleOut
    $stderrTask.Result | Set-Content -Path $consoleErr

    $exitCode = $process.ExitCode
    Write-Host "Unity exit code: $exitCode"

    if (-not (Test-Path $resultsXml)) {
        Write-Host "FAIL: results XML was not created (likely a compile error)." -ForegroundColor Red
        Write-Host "Inspect the log: $logFile"
        return $false
    }

    try {
        [xml]$xml = Get-Content -Path $resultsXml -Raw
    } catch {
        Write-Host "FAIL: could not parse results XML: $resultsXml" -ForegroundColor Red
        return $false
    }

    $root = $xml.DocumentElement
    if ($null -eq $root -or $root.Name -ne 'test-run') {
        $rootName = 'missing'
        if ($null -ne $root) { $rootName = $root.Name }
        Write-Host "FAIL: unexpected results XML format (root element: '$rootName')." -ForegroundColor Red
        Write-Host "Inspect: $resultsXml"
        return $false
    }

    $total    = $root.GetAttribute('testcasecount')
    $passed   = $root.GetAttribute('passed')
    $failed   = $root.GetAttribute('failed')
    $skipped  = $root.GetAttribute('skipped')
    $result   = $root.GetAttribute('result')
    $duration = $root.GetAttribute('duration')

    $failedCount = 0
    [int]::TryParse($failed, [ref]$failedCount) | Out-Null

    Write-Host ("Tests: {0} total | {1} passed | {2} failed | {3} skipped | result={4} | {5}s" -f `
        $total, $passed, $failed, $skipped, $result, $duration)

    if (($exitCode -eq 0) -and ($result -eq 'Passed') -and ($failedCount -eq 0)) {
        Write-Host "PASS: $TestPlatform gate green." -ForegroundColor Green
        return $true
    }

    Write-Host "FAIL: $TestPlatform gate failed (exit=$exitCode, result=$result, failed=$failedCount)." -ForegroundColor Red
    Write-Host "See: $logFile"
    return $false
}

# --- main ---

if (-not (Test-Path (Join-Path $ProjectRoot 'ProjectSettings\ProjectVersion.txt'))) {
    Write-Host "ERROR: '$ProjectRoot' does not look like a Unity project." -ForegroundColor Red
    exit 1
}

$lockFile = Join-Path $ProjectRoot 'Temp\UnityLockfile'
if ((Test-Path $lockFile) -and -not $Force) {
    Write-Host "ABORT: '$lockFile' exists - the Unity Editor is probably open with this project." -ForegroundColor Red
    Write-Host "Close the Editor and re-run. (If this is a stale lock from a crashed run, re-run with -Force.)"
    exit 1
}

$unityExe = Find-UnityExe -Preferred $UnityPath
if (-not $unityExe) {
    Write-Host "ERROR: Unity.exe not found. Install via Unity Hub or pass -UnityPath." -ForegroundColor Red
    exit 1
}
Write-Host "Unity: $unityExe"
Write-Host "Project: $ProjectRoot"

if (-not (Test-Path $TestResultsDir)) {
    New-Item -ItemType Directory -Path $TestResultsDir | Out-Null
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'

$modes = @()
if ($Mode -eq 'All') { $modes = @('EditMode', 'PlayMode') } else { $modes = @($Mode) }

$allPassed = $true
foreach ($m in $modes) {
    $runOk = Invoke-UnityTestRun -TestPlatform $m -UnityExe $unityExe -ProjRoot $ProjectRoot `
        -ResultsDir $TestResultsDir -Stamp $stamp -TimeoutMin $TimeoutMinutes
    if (-not $runOk) { $allPassed = $false }
}

Write-Host ""
if ($allPassed) {
    Write-Host "ALL GATES GREEN ($($modes -join ', '))" -ForegroundColor Green
    exit 0
}

Write-Host "GATE FAILED ($($modes -join ', '))" -ForegroundColor Red
exit 1
