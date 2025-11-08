# Script final para crear acceso directo
$WshShell = New-Object -ComObject WScript.Shell

# Intentar crear en el escritorio primero
$DesktopPath = [Environment]::GetFolderPath('Desktop')
$ShortcutPath = Join-Path $DesktopPath "Rutina Gym.lnk"

Write-Host "Intentando crear acceso directo en: $ShortcutPath"

try {
    $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = "powershell.exe"
    $Shortcut.Arguments = '-ExecutionPolicy Bypass -WindowStyle Hidden -File "C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym\ejecutar_rutina_gym.ps1"'
    $Shortcut.WorkingDirectory = "C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym"
    $Shortcut.IconLocation = "C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym\gym_icon.ico,0"
    $Shortcut.Description = "Generador de Rutinas de Gimnasio"
    $Shortcut.Save()

    if (Test-Path $ShortcutPath) {
        Write-Host "EXITO: Acceso directo creado en el escritorio!" -ForegroundColor Green
    } else {
        throw "El archivo no se creo"
    }
}
catch {
    Write-Host "ERROR: No se pudo crear en el escritorio: $_" -ForegroundColor Red

    # Intentar en C:\Temp como alternativa
    Write-Host "Intentando crear en C:\Temp como alternativa..."

    if (-not (Test-Path "C:\Temp")) {
        New-Item -Path "C:\Temp" -ItemType Directory -Force | Out-Null
    }

    $AltPath = "C:\Temp\Rutina Gym.lnk"
    $Shortcut2 = $WshShell.CreateShortcut($AltPath)
    $Shortcut2.TargetPath = "powershell.exe"
    $Shortcut2.Arguments = '-ExecutionPolicy Bypass -WindowStyle Hidden -File "C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym\ejecutar_rutina_gym.ps1"'
    $Shortcut2.WorkingDirectory = "C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym"
    $Shortcut2.IconLocation = "C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym\gym_icon.ico,0"
    $Shortcut2.Description = "Generador de Rutinas de Gimnasio"
    $Shortcut2.Save()

    if (Test-Path $AltPath) {
        Write-Host "EXITO: Acceso directo creado en C:\Temp\" -ForegroundColor Yellow
        Write-Host "Por favor, MUEVE manualmente el archivo desde C:\Temp\Rutina Gym.lnk a tu escritorio" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
