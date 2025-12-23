@echo off
title Generador de Rutinas de Gimnasio
color 0A

echo ========================================
echo  Generador de Rutinas de Gimnasio
echo ========================================
echo.

cd electron-app

echo Verificando dependencias...
if not exist "node_modules\" (
    echo.
    echo [!] No se encontraron dependencias instaladas
    echo [+] Instalando dependencias... Esto puede tardar unos minutos
    echo.
    call npm install
    if errorlevel 1 (
        echo.
        echo [ERROR] Hubo un problema al instalar las dependencias
        echo Presiona cualquier tecla para salir...
        pause > nul
        exit /b 1
    )
    echo.
    echo [OK] Dependencias instaladas correctamente
) else (
    echo [OK] Dependencias ya instaladas
)

echo.
echo [+] Iniciando aplicacion...
echo.
call npm run electron:dev

if errorlevel 1 (
    echo.
    echo [ERROR] Hubo un problema al iniciar la aplicacion
    echo Presiona cualquier tecla para salir...
    pause > nul
    exit /b 1
)
