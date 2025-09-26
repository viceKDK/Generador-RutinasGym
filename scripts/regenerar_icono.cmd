@echo off
REM Regenerar icono con fondo transparente y tamaños grandes (256 ICO + PNGs 512/1024)
REM Usa Python virtualenv local en .venv y los scripts en scripts\

setlocal ENABLEDELAYEDEXPANSION
cd /d "%~dp0.."  
REM Ahora CWD es la raíz del repo (scripts\..)

set SRC_PNG=icon_transparent.png
set RAW_PNG=20250926_1414_Luxurious Gym Icon_simple_compose_01k63g8kg4f7qr55w8agwxh2qx.png
set ICO_ROOT=gym_icon.ico
set PNG_512=gym_icon_512.png
set PNG_1024=gym_icon_1024.png

echo.
echo === Regenerando icono ===

REM Crear venv si no existe
if not exist .venv (
  echo Creando entorno virtual .venv...
  python -m venv .venv || goto :error
)

REM Asegurar Pillow instalado
call .venv\Scripts\python -m pip install --disable-pip-version-check -q pillow || goto :error

REM Si no existe el PNG transparente, intentar generarlo desde el RAW
if not exist "%SRC_PNG%" (
  if exist "%RAW_PNG%" (
    echo Generando PNG transparente desde "%RAW_PNG%"...
    call .venv\Scripts\python scripts\remove_bg_transparent.py --source "%RAW_PNG%" --out "%SRC_PNG%" --tolerance 30 --feather 16 || goto :error
  ) else (
    echo ERROR: No se encuentra "%SRC_PNG%" ni "%RAW_PNG%".
    goto :error
  )
)

REM Generar PNGs ajustados (recorte + padding) a 512 y 1024
call .venv\Scripts\python scripts\fit_icon.py --source "%SRC_PNG%" --out "%PNG_512%" --size 512 --padding 6 || goto :error
call .venv\Scripts\python scripts\fit_icon.py --source "%SRC_PNG%" --out "%PNG_1024%" --size 1024 --padding 6 || goto :error

echo Creando ICO desde 1024, solo 256...
call .venv\Scripts\python scripts\generate_icon.py --source "%PNG_1024%" --out "%ICO_ROOT%" --sizes 256 || goto :error

echo.
echo OK: Icono y PNGs generados:
echo  - %CD%\%ICO_ROOT% (ico 256)
echo  - %CD%\%PNG_512% (png 512)
echo  - %CD%\%PNG_1024% (png 1024)
echo.
echo Sugerencia: Cerrar/abrir Explorador o ejecutar refrescar_iconos.bat
exit /b 0

:error
echo.
echo ERROR durante la regeneracion del icono.
exit /b 1
