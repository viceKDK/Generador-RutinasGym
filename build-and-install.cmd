@echo off
REM Script simple para construir e instalar Generador de Rutinas de Gimnasio
REM Epic 6 Story 6.2: Windows Installer & Desktop Integration

echo.
echo ====================================================
echo    🏋️ GENERADOR DE RUTINAS DE GIMNASIO
echo    Construcción e Instalación Automática
echo ====================================================
echo.

REM Verificar que PowerShell está disponible
powershell -Command "Get-Host" >nul 2>&1
if errorlevel 1 (
    echo ❌ PowerShell no está disponible. Se requiere PowerShell para continuar.
    pause
    exit /b 1
)

REM Verificar que .NET está instalado
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET SDK no está instalado.
    echo 💡 Descárguelo desde: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo ✅ Requisitos verificados
echo.

REM Preguntar al usuario si desea instalar después de construir
set /p INSTALL_CHOICE="¿Desea instalar la aplicación después de construir? (S/n): "
if /i "%INSTALL_CHOICE%"=="n" (
    set INSTALL_FLAG=
) else (
    set INSTALL_FLAG=-Install
)

echo.
echo 🔨 Iniciando construcción del instalador...
echo.

REM Cambiar al directorio del script
cd /d "%~dp0"

REM Ejecutar el script de PowerShell
powershell -ExecutionPolicy Bypass -File "scripts\build-installer.ps1" -Configuration Release -Platform x64 %INSTALL_FLAG%

if errorlevel 1 (
    echo.
    echo ❌ Error durante la construcción
    pause
    exit /b 1
)

echo.
echo ✅ Proceso completado exitosamente
echo.

if "%INSTALL_FLAG%"=="-Install" (
    echo 🎉 La aplicación ha sido instalada.
    echo 🔍 Búsquela en el Menú Inicio como 'Generador de Rutinas de Gimnasio'
) else (
    echo 📦 El instalador se ha creado correctamente.
    echo 💡 Ejecute nuevamente con instalación para instalar la aplicación.
)

echo.
pause