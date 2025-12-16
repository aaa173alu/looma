<#
  install-dotnet.ps1
  Downloads the official dotnet-install.ps1 and installs .NET SDK 8 locally into ./.dotnet
  Usage (PowerShell):
    .\scripts\install-dotnet.ps1

  This is a non-admin local installation that does not modify system-wide state.
#>

Set-StrictMode -Version Latest

$Channel = '8.0'
$InstallDir = Join-Path $PSScriptRoot '..\.dotnet'
$InstallDir = [System.IO.Path]::GetFullPath($InstallDir)
if (-not (Test-Path $InstallDir)) { New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null }

$DotNetInstallScript = Join-Path $PSScriptRoot 'dotnet-install.ps1'
if (-not (Test-Path $DotNetInstallScript)) {
  Write-Host "Downloading dotnet-install.ps1..."
  Invoke-WebRequest -UseBasicParsing -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $DotNetInstallScript
}

Write-Host "Installing .NET SDK $Channel to $InstallDir"
try {
  & $DotNetInstallScript -Channel $Channel -InstallDir $InstallDir -NoPath
  $exit = $LASTEXITCODE
}
catch {
  Write-Error "dotnet-install failed: $_"
  exit 1
}

if ($exit -ne 0) {
  Write-Error "dotnet-install failed with exit code $exit"
  exit $exit
}

Write-Host "Installation complete. To use the locally installed dotnet for this PowerShell session run:"
Write-Host "  .\scripts\use-local-dotnet.ps1"
