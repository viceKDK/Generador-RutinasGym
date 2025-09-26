param(
  [ValidateSet('Debug','Release')]
  [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$vsRoot = 'C:\Program Files\Microsoft Visual Studio\2022\Community'
$vsMsbuild = Join-Path $vsRoot 'MSBuild'
$vsMsbuildBin = Join-Path $vsMsbuild 'Current\Bin'
$vsMsbuildSdk = Join-Path $vsMsbuild 'Current\Sdk'
$appxTask = Join-Path $vsMsbuild 'Microsoft\VisualStudio\v17.0\AppxPackage\Microsoft.Build.Packaging.Pri.Tasks.dll'

if (-not (Test-Path $appxTask)) {
  Write-Host "[!] No se encontró el task de AppxPackage en:`n    $appxTask" -ForegroundColor Yellow
  Write-Host '    Asegúrate de instalar "MSIX Packaging Tools" en Visual Studio 2022.'
  exit 1
}

# Export env vars for this process
$env:MSBuildExtensionsPath = $vsMsbuild
$env:MSBuildExtensionsPath32 = $vsMsbuild
$env:VisualStudioVersion = '17.0'
$env:VSINSTALLDIR = "$vsRoot\"
$env:Path = "$vsMsbuildBin;$($env:Path)"

Write-Host "[i] MSBuildExtensionsPath = $($env:MSBuildExtensionsPath)"
Write-Host "[i] MSBuildSDKsPath      = $($env:MSBuildSDKsPath)"
Write-Host "[i] Configuración        = $Configuration"

dotnet restore 'src/GymRoutineGenerator.UI.csproj'
dotnet build 'src/GymRoutineGenerator.UI.csproj' -c $Configuration -p:Platform=x64

Write-Host "[ok] Build completado. Salida en: src\bin\x64\$Configuration\net8.0-windows10.0.22621.0\"
