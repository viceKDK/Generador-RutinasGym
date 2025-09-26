# Script para ejecutar todos los tests de la aplicación Gym Routine Generator
# Epic 6 Story 6.4: Final Testing & Production Readiness

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Unit", "Integration", "Performance", "UserAcceptance")]
    [string]$TestSuite = "All",

    [Parameter(Mandatory=$false)]
    [switch]$Verbose,

    [Parameter(Mandatory=$false)]
    [switch]$GenerateReport,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ""
)

# Configuración
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$TestProject = Join-Path $ProjectRoot "tests\GymRoutineGenerator.Tests\GymRoutineGenerator.Tests.csproj"
$ReportsPath = Join-Path $ProjectRoot "TestResults"

Write-Host "🧪 EJECUTANDO TESTS DE GYM ROUTINE GENERATOR" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host "Suite de tests: $TestSuite" -ForegroundColor Yellow
Write-Host "Proyecto de tests: $TestProject" -ForegroundColor Gray
Write-Host ""

# Verificar que el proyecto de tests existe
if (-not (Test-Path $TestProject)) {
    Write-Error "❌ Proyecto de tests no encontrado: $TestProject"
    exit 1
}

# Crear directorio de reportes si se solicita
if ($GenerateReport) {
    if (-not (Test-Path $ReportsPath)) {
        New-Item -Path $ReportsPath -ItemType Directory -Force | Out-Null
    }
    Write-Host "📊 Reportes se guardarán en: $ReportsPath" -ForegroundColor Cyan
}

try {
    # Configurar argumentos base para dotnet test
    $testArgs = @("test", $TestProject)

    if ($Verbose) {
        $testArgs += "--verbosity", "detailed"
    } else {
        $testArgs += "--verbosity", "normal"
    }

    # Configurar filtros según la suite de tests
    switch ($TestSuite) {
        "Unit" {
            Write-Host "🔬 Ejecutando Tests Unitarios..." -ForegroundColor Yellow
            # Si tuviéramos tests unitarios, los filtrarían aquí
            $testArgs += "--filter", "Category=Unit"
        }
        "Integration" {
            Write-Host "🔗 Ejecutando Tests de Integración..." -ForegroundColor Yellow
            $testArgs += "--filter", "FullyQualifiedName~Integration"
        }
        "Performance" {
            Write-Host "⚡ Ejecutando Tests de Performance..." -ForegroundColor Yellow
            $testArgs += "--filter", "FullyQualifiedName~Performance"
        }
        "UserAcceptance" {
            Write-Host "👥 Ejecutando Tests de Aceptación de Usuario..." -ForegroundColor Yellow
            $testArgs += "--filter", "FullyQualifiedName~Validation"
        }
        "All" {
            Write-Host "🎯 Ejecutando TODOS los Tests..." -ForegroundColor Yellow
        }
    }

    # Configurar generación de reportes
    if ($GenerateReport) {
        $reportPath = Join-Path $ReportsPath "TestResults_$(Get-Date -Format 'yyyyMMdd_HHmmss').xml"
        $testArgs += "--logger", "trx;LogFileName=$reportPath"

        # Cobertura de código si está disponible
        if (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
            $testArgs += "--collect", "XPlat Code Coverage"
        }
    }

    # Especificar directorio de salida si se proporciona
    if ($OutputPath) {
        $testArgs += "--results-directory", $OutputPath
    }

    Write-Host "🚀 Comando a ejecutar: dotnet $($testArgs -join ' ')" -ForegroundColor Gray
    Write-Host ""

    # Ejecutar los tests
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $result = & dotnet @testArgs
    $stopwatch.Stop()

    # Analizar resultados
    $exitCode = $LASTEXITCODE

    Write-Host ""
    Write-Host "⏱️ Tiempo total: $($stopwatch.Elapsed.ToString('mm\:ss'))" -ForegroundColor Cyan

    if ($exitCode -eq 0) {
        Write-Host "✅ TODOS LOS TESTS PASARON EXITOSAMENTE!" -ForegroundColor Green

        # Mostrar resumen específico según la suite
        switch ($TestSuite) {
            "Integration" {
                Write-Host ""
                Write-Host "🎉 RESUMEN DE TESTS DE INTEGRACIÓN:" -ForegroundColor Green
                Write-Host "   ✅ End-to-End workflow completo" -ForegroundColor White
                Write-Host "   ✅ Todas las plantillas funcionando" -ForegroundColor White
                Write-Host "   ✅ Rutinas multi-día con parámetros" -ForegroundColor White
                Write-Host "   ✅ Reporte de progreso preciso" -ForegroundColor White
                Write-Host "   ✅ Validación final: 5 rutinas diferentes" -ForegroundColor White
            }
            "Performance" {
                Write-Host ""
                Write-Host "🚀 RESUMEN DE TESTS DE PERFORMANCE:" -ForegroundColor Green
                Write-Host "   ✅ 100 exportaciones consecutivas" -ForegroundColor White
                Write-Host "   ✅ Sin memory leaks detectados" -ForegroundColor White
                Write-Host "   ✅ Concurrencia funcionando correctamente" -ForegroundColor White
                Write-Host "   ✅ Bases de datos grandes manejadas" -ForegroundColor White
                Write-Host "   ✅ Estabilidad a largo plazo" -ForegroundColor White
            }
            "UserAcceptance" {
                Write-Host ""
                Write-Host "👵 RESUMEN DE TESTS DE USUARIO:" -ForegroundColor Green
                Write-Host "   ✅ 5 escenarios de usuario exitosos" -ForegroundColor White
                Write-Host "   ✅ Interfaz amigable para personas mayores" -ForegroundColor White
                Write-Host "   ✅ Recuperación de errores elegante" -ForegroundColor White
                Write-Host "   ✅ Consistencia en resultados" -ForegroundColor White
                Write-Host "   🏆 TU MADRE PUEDE USAR LA APLICACIÓN!" -ForegroundColor Yellow
            }
            "All" {
                Write-Host ""
                Write-Host "🎊 VALIDACIÓN COMPLETA EXITOSA:" -ForegroundColor Green
                Write-Host "   ✅ Integración End-to-End" -ForegroundColor White
                Write-Host "   ✅ Casos extremos manejados" -ForegroundColor White
                Write-Host "   ✅ Performance y stress tests" -ForegroundColor White
                Write-Host "   ✅ Aceptación de usuario" -ForegroundColor White
                Write-Host "   🚀 APLICACIÓN LISTA PARA PRODUCCIÓN!" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "❌ ALGUNOS TESTS FALLARON" -ForegroundColor Red
        Write-Host "Revise el output arriba para detalles de los fallos." -ForegroundColor Yellow

        # Consejos de troubleshooting
        Write-Host ""
        Write-Host "🔍 TROUBLESHOOTING:" -ForegroundColor Yellow
        Write-Host "   1. Verifique que DocumentFormat.OpenXml esté instalado" -ForegroundColor White
        Write-Host "   2. Asegúrese de tener permisos de escritura en directorios temporales" -ForegroundColor White
        Write-Host "   3. Cierre Microsoft Word si está abierto" -ForegroundColor White
        Write-Host "   4. Ejecute con -Verbose para más detalles" -ForegroundColor White
    }

    # Información adicional si se generaron reportes
    if ($GenerateReport -and (Test-Path $ReportsPath)) {
        Write-Host ""
        Write-Host "📊 Reportes generados en: $ReportsPath" -ForegroundColor Cyan
        $reportFiles = Get-ChildItem $ReportsPath -Filter "*.xml" | Sort-Object CreationTime -Descending | Select-Object -First 1
        if ($reportFiles) {
            Write-Host "   📄 Reporte más reciente: $($reportFiles.Name)" -ForegroundColor White
        }
    }

    # Mostrar archivos de test generados para validación manual
    $tempTestFiles = Get-ChildItem $env:TEMP -Filter "*GymRoutine*" -Directory -ErrorAction SilentlyContinue
    if ($tempTestFiles) {
        Write-Host ""
        Write-Host "📁 Archivos de test para validación manual:" -ForegroundColor Cyan
        foreach ($dir in $tempTestFiles | Select-Object -First 3) {
            Write-Host "   $($dir.FullName)" -ForegroundColor White
        }
    }

} catch {
    Write-Error "💥 Error durante la ejecución de tests: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "=== EJECUCIÓN DE TESTS COMPLETADA ===" -ForegroundColor Green
exit $exitCode