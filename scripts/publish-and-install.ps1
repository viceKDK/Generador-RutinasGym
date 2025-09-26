$ErrorActionPreference = 'Stop'

Write-Host "Publicando app WinUI 3 self-contained (x64)..."
dotnet publish "$(Join-Path $PSScriptRoot '..\src\GymRoutineGenerator.UI.csproj')" -c Release -r win-x64 -p:SelfContained=true -p:PublishSingleFile=false -p:PublishTrimmed=false | Out-Null

$publishDir = Join-Path $PSScriptRoot '..\src\bin\x64\Release\net8.0-windows10.0.19041.0\win-x64\publish'
if (-not (Test-Path $publishDir)) {
  # Fallback to our profile dir if default publish path differs
  $publishDir = Join-Path $PSScriptRoot '..\src\bin\SelfContained\win-x64'
}

if (-not (Test-Path $publishDir)) { throw "No se encontró la carpeta de publicación." }

# Instalar Windows App Runtime (WinAppSDK) si es necesario
function Install-WindowsAppRuntime {
  try {
    $local = Join-Path $PSScriptRoot '..\instalador\WindowsAppRuntimeInstall-x64.exe'
    $temp = Join-Path $env:TEMP 'WindowsAppRuntimeInstall-x64.exe'
    $urls = @(
      'https://aka.ms/windowsappsdk/1.8/windowsappruntimeinstall-x64.exe',
      'https://aka.ms/windowsappsdk/1.8/WindowsAppRuntimeInstall-x64.exe',
      'https://aka.ms/windowsappsdk/1.6/windowsappruntimeinstall-x64.exe'
    )
    $downloaded = $false
    foreach ($u in $urls) {
      try {
        Write-Host "Descargando Windows App Runtime desde $u ..."
        Invoke-WebRequest -Uri $u -OutFile $temp -UseBasicParsing -TimeoutSec 120
        $downloaded = Test-Path $temp
        if ($downloaded) { break }
      } catch { }
    }
    if (Test-Path $local) {
      Write-Host "Instalando Windows App Runtime desde paquete local..."
      Start-Process -FilePath $local -ArgumentList '/silent /norestart' -Wait
      return
    }
    if ($downloaded) {
      Write-Host "Ejecutando instalador WinAppRuntime (silencioso)..."
      Start-Process -FilePath $temp -ArgumentList '/silent /norestart' -Wait
    } else {
      Write-Warning "No se pudo descargar Windows App Runtime automáticamente. Si la app no arranca en PCs limpios, instálalo manualmente: https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads"
    }
  } catch {
    Write-Warning "Fallo instalando Windows App Runtime: $($_.Exception.Message)"
  }
}

Install-WindowsAppRuntime

# Copiar a carpeta de instalación del usuario
$installDir = Join-Path $env:LOCALAPPDATA 'GymRoutineGenerator'
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Write-Host "Copiando archivos a $installDir ..."
Get-ChildItem $publishDir -Recurse | Copy-Item -Destination $installDir -Recurse -Force

$exe = Join-Path $installDir 'GymRoutineGenerator.UI.exe'
if (-not (Test-Path $exe)) { throw "No se encontró el ejecutable en $installDir" }

# Crear acceso directo en el escritorio
$desktop = [Environment]::GetFolderPath('Desktop')
$lnkPath = Join-Path $desktop 'Generador de Rutinas.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($lnkPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = $installDir
$shortcut.WindowStyle = 1
$shortcut.Description = 'Generador de Rutinas de Gimnasio'
$shortcut.IconLocation = $exe
$shortcut.Save()

Write-Host "Listo: acceso directo creado en el escritorio: $lnkPath"
