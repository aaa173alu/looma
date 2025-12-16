<#
  install-and-build.ps1
  Installs local .NET (8) and builds the solution using the local dotnet.
  Usage:
    .\scripts\install-and-build.ps1
#>
Set-StrictMode -Version Latest

Write-Host "Installing .NET SDK locally (if needed)..."
try {
  . .\scripts\install-dotnet.ps1
}
catch {
  Write-Warning "install-dotnet.ps1 failed or was skipped: $_"
}

Write-Host "Adding local dotnet to PATH for this session (if installed)..."
try {
  . .\scripts\use-local-dotnet.ps1
}
catch {
  Write-Warning "use-local-dotnet.ps1 failed: $_"
}

Write-Host "Restoring and building solution..."
$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$SolutionPath = Join-Path $RepoRoot 'prac.sln'
if (-not (Test-Path $SolutionPath)) { Write-Error "Solution file not found: $SolutionPath"; exit 2 }

$testProj = Join-Path $RepoRoot 'Tests\Tests.csproj'

Write-Host "Solution path: $SolutionPath"

$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$proc = Start-Process -FilePath dotnet -ArgumentList @('restore') -WorkingDirectory $RepoRoot -NoNewWindow -Wait -PassThru
if ($proc.ExitCode -ne 0) { Write-Error "dotnet restore failed with exit code $($proc.ExitCode)"; exit $proc.ExitCode }

$proc = Start-Process -FilePath dotnet -ArgumentList @('build', '--no-restore', '-v', 'minimal') -WorkingDirectory $RepoRoot -NoNewWindow -Wait -PassThru
if ($proc.ExitCode -ne 0) { Write-Error "dotnet build failed with exit code $($proc.ExitCode)"; exit $proc.ExitCode }

if (-not (Test-Path $testProj)) { Write-Warning "Test project not found: $testProj. Skipping tests."; exit 0 }

Write-Host "Running tests..."
$testDir = Split-Path -Path $testProj -Parent
$proc = Start-Process -FilePath dotnet -ArgumentList @('test', '--no-build', '-v', 'minimal') -WorkingDirectory $testDir -NoNewWindow -Wait -PassThru
if ($proc.ExitCode -ne 0) { Write-Error "dotnet test failed with exit code $($proc.ExitCode)"; exit $proc.ExitCode }
