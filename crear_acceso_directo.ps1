# Script para crear acceso directo de Rutina Gym en el escritorio

# Rutas
$escritorio = [Environment]::GetFolderPath("Desktop")
$rutaProyecto = Split-Path -Parent $MyInvocation.MyCommand.Path
$rutaScript = Join-Path $rutaProyecto "ejecutar_rutina_gym.vbs"
$rutaIconoIco = Join-Path $rutaProyecto "gym_icon.ico"
$rutaAccesoDirecto = Join-Path $escritorio "Rutina Gym.lnk"

# Eliminar acceso directo anterior si existe
if (Test-Path $rutaAccesoDirecto) {
    Remove-Item $rutaAccesoDirecto -Force
    Write-Host "Acceso directo anterior eliminado"
}

Write-Host "Verificando icono multiresolucion..."
if (-not (Test-Path $rutaIconoIco)) {
    Write-Host "Icono no encontrado. Regenerando..."
    $regenerar = Join-Path $rutaProyecto "scripts\regenerar_icono.cmd"
    if (Test-Path $regenerar) {
        & $regenerar | Write-Host
    } else {
        Write-Host "ADVERTENCIA: Falta $regenerar"
    }
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

# Configurar icono
if (Test-Path $rutaIconoIco) {
    $accesoDirecto.IconLocation = "$rutaIconoIco,0"
    Write-Host "Icono configurado: $rutaIconoIco"
} else {
    Write-Host "No se encontro el archivo de icono"
}

# Guardar acceso directo
$accesoDirecto.Save()

Write-Host "Acceso directo 'Rutina Gym' creado en el escritorio exitosamente!"
Write-Host "Ruta: $rutaAccesoDirecto"

# Limpiar objeto COM
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($shell) | Out-Null

Write-Host "Proceso completado"
