$ErrorActionPreference = 'Stop'

Write-Host "Creando paquete portable..."
dotnet publish "$(Join-Path $PSScriptRoot '..\src\GymRoutineGenerator.UI.csproj')" -c Release -r win-x64 -p:SelfContained=true -p:PublishSingleFile=false -p:PublishTrimmed=false | Out-Null

$publishDir = Join-Path $PSScriptRoot '..\src\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\publish'
$outDir = Join-Path $PSScriptRoot '..\dist\GeneradorRutinas_Portable_win-x64'

if (Test-Path $outDir) { Remove-Item -Recurse -Force $outDir }
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Copy-Item -Recurse -Force "$publishDir\*" $outDir

# Incluir instalador del Windows App Runtime si está disponible
$warLocal = Join-Path $PSScriptRoot '..\instalador\WindowsAppRuntimeInstall-x64.exe'
if (Test-Path $warLocal) { Copy-Item $warLocal $outDir }

# Crear script para acceso directo
$shortcutCmd = @'
@echo off
setlocal
set APPDIR=%~dp0
set EXE=%APPDIR%GymRoutineGenerator.UI.exe
for /f "tokens=2,*" %%A in ('reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" /v Desktop ^| find "Desktop"') do set DESKTOP=%%B

echo Creando acceso directo en el escritorio...
powershell -NoProfile -ExecutionPolicy Bypass -Command "\
  $shell = New-Object -ComObject WScript.Shell; \
  $lnk = $shell.CreateShortcut(\"$env:USERPROFILE\\Desktop\\Generador de Rutinas.lnk\"); \
  $lnk.TargetPath = \"%EXE%\"; \
  $lnk.WorkingDirectory = \"%APPDIR%\"; \
  $lnk.IconLocation = \"%EXE%\"; \
  $lnk.WindowStyle = 1; \
  $lnk.Save()"
echo Listo.
pause
'@

Set-Content -Path (Join-Path $outDir 'CrearAccesoDirecto.cmd') -Value $shortcutCmd -Encoding Ascii

Write-Host "Paquete portable creado en: $outDir"
