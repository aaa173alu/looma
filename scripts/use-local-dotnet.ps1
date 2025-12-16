# Adds local ./.dotnet to PATH for the current session
$InstallDir = Join-Path $PSScriptRoot '..\.dotnet' | Resolve-Path -ErrorAction SilentlyContinue
$InstallDir = Join-Path $PSScriptRoot '..\.dotnet'
$InstallDir = [System.IO.Path]::GetFullPath($InstallDir)
if (-not (Test-Path $InstallDir)) {
	Write-Error "Local dotnet not found. Run scripts\install-dotnet.ps1 first."
	return
}
$dotnetPath = Join-Path $InstallDir 'dotnet.exe'
$dotnetBin = Join-Path $InstallDir 'dotnet.exe'
if (-not (Test-Path $dotnetBin)) {
	$dotnetBin = Join-Path $InstallDir 'dotnet' # posix fallback
}
$env:PATH = $InstallDir + ';' + $env:PATH
Write-Host "Adding local dotnet to PATH for this session..."
Write-Host "Local dotnet added. dotnet --info:`n"
try { dotnet --info } catch { Write-Warning "dotnet not found in PATH after adding. Ensure the install succeeded." }
Write-Host "Local dotnet added to PATH: $InstallDir"
Write-Host "dotnet version: $(dotnet --version)"
Write-Host "Local dotnet added to PATH: $InstallDir"
Write-Host "dotnet version: $(dotnet --version)"