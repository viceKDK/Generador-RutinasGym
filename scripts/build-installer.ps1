# Script para construir el instalador MSIX de Generador de Rutinas de Gimnasio
# Epic 6 Story 6.2: Windows Installer & Desktop Integration

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [Parameter(Mandatory=$false)]
    [ValidateSet("x64", "x86", "ARM64")]
    [string]$Platform = "x64",

    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,

    [Parameter(Mandatory=$false)]
    [switch]$Install,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ""
)

# Configuración
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "src\GymRoutineGenerator.UI.csproj"
$PackageName = "Generador de Rutinas de Gimnasio"

Write-Host "🏋️ Construyendo $PackageName" -ForegroundColor Green
Write-Host "Configuración: $Configuration | Plataforma: $Platform" -ForegroundColor Yellow
Write-Host "Directorio del proyecto: $ProjectRoot" -ForegroundColor Gray
Write-Host ""

# Verificar que el archivo del proyecto existe
if (-not (Test-Path $ProjectFile)) {
    Write-Error "No se encontró el archivo del proyecto: $ProjectFile"
    exit 1
}

try {
    # Paso 1: Limpiar compilación anterior
    Write-Host "🧹 Limpiando compilación anterior..." -ForegroundColor Yellow
    dotnet clean $ProjectFile -c $Configuration -p:Platform=$Platform --verbosity minimal

    if (-not $SkipBuild) {
        # Paso 2: Restaurar paquetes NuGet
        Write-Host "📦 Restaurando paquetes NuGet..." -ForegroundColor Yellow
        dotnet restore $ProjectFile --verbosity minimal

        # Paso 3: Compilar la aplicación
        Write-Host "🔨 Compilando aplicación..." -ForegroundColor Yellow
        dotnet build $ProjectFile -c $Configuration -p:Platform=$Platform --no-restore --verbosity minimal

        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Error al compilar la aplicación"
            exit $LASTEXITCODE
        }
    }

    # Paso 4: Crear el paquete MSIX
    Write-Host "📦 Creando paquete MSIX..." -ForegroundColor Yellow
    $PublishArgs = @(
        "publish", $ProjectFile
        "-c", $Configuration
        "-p:Platform=$Platform"
        "-p:PublishProfile=win-$Platform"
        "-p:GenerateAppxPackageOnBuild=true"
        "--self-contained", "false"
        "--verbosity", "minimal"
    )

    if ($OutputPath) {
        $PublishArgs += "-o", $OutputPath
    }

    & dotnet @PublishArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Error al crear el paquete MSIX"
        exit $LASTEXITCODE
    }

    # Paso 5: Localizar el paquete creado
    $PackageSearchPath = Join-Path $ProjectRoot "src\bin\$Configuration\net9.0-windows10.0.19041.0\$Platform\AppPackages"
    Write-Host "🔍 Buscando paquete en: $PackageSearchPath" -ForegroundColor Gray

    if (Test-Path $PackageSearchPath) {
        $MSIXFiles = Get-ChildItem -Path $PackageSearchPath -Filter "*.msix" -Recurse
        $MSIXBundles = Get-ChildItem -Path $PackageSearchPath -Filter "*.msixbundle" -Recurse

        if ($MSIXBundles.Count -gt 0) {
            $PackageFile = $MSIXBundles[0].FullName
            Write-Host "✅ Paquete bundle creado: $($MSIXBundles[0].Name)" -ForegroundColor Green
        } elseif ($MSIXFiles.Count -gt 0) {
            $PackageFile = $MSIXFiles[0].FullName
            Write-Host "✅ Paquete MSIX creado: $($MSIXFiles[0].Name)" -ForegroundColor Green
        } else {
            Write-Warning "⚠️ No se encontraron archivos .msix o .msixbundle en $PackageSearchPath"
            $PackageFile = $null
        }

        if ($PackageFile) {
            $PackageSize = (Get-Item $PackageFile).Length / 1MB
            Write-Host "📏 Tamaño del paquete: $([math]::Round($PackageSize, 2)) MB" -ForegroundColor Cyan
            Write-Host "📂 Ruta completa: $PackageFile" -ForegroundColor Gray
        }
    } else {
        Write-Warning "⚠️ No se encontró el directorio de paquetes: $PackageSearchPath"
    }

    # Paso 6: Instalar si se solicita
    if ($Install -and $PackageFile -and (Test-Path $PackageFile)) {
        Write-Host ""
        Write-Host "🚀 Instalando aplicación..." -ForegroundColor Yellow

        try {
            Add-AppxPackage -Path $PackageFile -ForceApplicationShutdown
            Write-Host "✅ Aplicación instalada correctamente" -ForegroundColor Green
            Write-Host "🎯 Busque '$PackageName' en el menú Inicio" -ForegroundColor Cyan
        } catch {
            Write-Warning "⚠️ Error al instalar: $($_.Exception.Message)"
            Write-Host "💡 Puede intentar instalar manualmente haciendo doble clic en: $PackageFile" -ForegroundColor Yellow
        }
    }

    Write-Host ""
    Write-Host "🎉 ¡Construcción completada exitosamente!" -ForegroundColor Green

    if (-not $Install -and $PackageFile) {
        Write-Host ""
        Write-Host "📋 Siguientes pasos:" -ForegroundColor Yellow
        Write-Host "   1. Para instalar: .\build-installer.ps1 -Install" -ForegroundColor White
        Write-Host "   2. Archivo de instalación: $PackageFile" -ForegroundColor White
        Write-Host "   3. Distribución: Comparta el archivo .msix/.msixbundle con los usuarios" -ForegroundColor White
    }

} catch {
    Write-Error "❌ Error durante la construcción: $($_.Exception.Message)"
    Write-Host "Detalles del error: $($_.Exception)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Construccion del Instalador Completada ===" -ForegroundColor Green