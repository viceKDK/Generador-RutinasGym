# Script mejorado para crear acceso directo de Rutina Gym con icono

# Rutas absolutas
$escritorio = [Environment]::GetFolderPath("Desktop")
$rutaProyecto = Split-Path -Parent $MyInvocation.MyCommand.Path
$rutaScript = Join-Path $rutaProyecto "ejecutar_rutina_gym.vbs"
$rutaIconoAppUI = Join-Path $rutaProyecto "gym_icon.ico"
$rutaAccesoDirecto = Join-Path $escritorio "Rutina Gym.lnk"

Write-Host "Verificando archivos..."
Write-Host "Ruta del proyecto: $rutaProyecto"
Write-Host "Ruta del script: $rutaScript"
Write-Host "Ruta del icono: $rutaIconoAppUI"

# Verificar que el script VBS existe
if (-not (Test-Path $rutaScript)) {
    Write-Host "ERROR: No se encuentra el script VBS en: $rutaScript"
    exit 1
}

# Verificar que el icono existe
if (-not (Test-Path $rutaIconoAppUI)) {
    Write-Host "ADVERTENCIA: No se encuentra el icono en: $rutaIconoAppUI"
    Write-Host "Regenerando icono multiresolucion..."
    $regenerar = Join-Path $rutaProyecto "scripts\regenerar_icono.cmd"
    if (Test-Path $regenerar) {
        & $regenerar | Write-Host
    } else {
        Write-Host "ADVERTENCIA: Falta $regenerar"
    }
}

# Eliminar acceso directo anterior si existe
if (Test-Path $rutaAccesoDirecto) {
    Remove-Item $rutaAccesoDirecto -Force
    Write-Host "Acceso directo anterior eliminado"
}

# Crear objeto WScript.Shell
$shell = New-Object -ComObject WScript.Shell

# Crear acceso directo
Write-Host "Creando acceso directo..."
$accesoDirecto = $shell.CreateShortcut($rutaAccesoDirecto)
$accesoDirecto.TargetPath = $rutaScript
$accesoDirecto.WorkingDirectory = $rutaProyecto
$accesoDirecto.Description = "Generador de Rutinas de Gimnasio"
$accesoDirecto.WindowStyle = 1

# Configurar icono con ruta absoluta
if (Test-Path $rutaIconoAppUI) {
    $accesoDirecto.IconLocation = "$rutaIconoAppUI,0"
    Write-Host "Icono configurado: $rutaIconoAppUI"
} else {
    Write-Host "ADVERTENCIA: No se pudo configurar el icono"
}

# Guardar acceso directo
$accesoDirecto.Save()

# Limpiar objeto COM
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($shell) | Out-Null

Write-Host ""
Write-Host "=== RESULTADO ==="
Write-Host "Acceso directo creado: $rutaAccesoDirecto"
Write-Host "Script objetivo: $rutaScript"
Write-Host "Icono: $rutaIconoAppUI"

# Verificar propiedades del acceso directo
if (Test-Path $rutaAccesoDirecto) {
    $shellApp = New-Object -ComObject Shell.Application
    $folder = $shellApp.NameSpace((Split-Path $rutaAccesoDirecto))
    $item = $folder.ParseName((Split-Path $rutaAccesoDirecto -Leaf))

    Write-Host ""
    Write-Host "Propiedades del acceso directo:"
    Write-Host "- Destino: $($item.GetLink.TargetPath)"
    Write-Host "- Icono: $($item.GetLink.IconLocation)"

    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($shellApp) | Out-Null
}

Write-Host ""
Write-Host "¡Acceso directo 'Rutina Gym' creado exitosamente!"
Write-Host "Reinicia el Explorador de Windows si no ves el icono inmediatamente."
