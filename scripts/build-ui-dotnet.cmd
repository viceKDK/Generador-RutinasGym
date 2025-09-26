@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Build GymRoutineGenerator UI (WinUI 3) using dotnet, but with VS MSBuild tasks
REM Usage: build-ui-dotnet.cmd [Debug|Release]

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Debug

set VSROOT=C:\Program Files\Microsoft Visual Studio\2022\Community
set VSMSBUILD=%VSROOT%\MSBuild
set VSMSBUILD_BIN=%VSROOT%\MSBuild\Current\Bin
set VSMSBUILD_SDK=%VSROOT%\MSBuild\Current\Sdk
set APPX_TASK=%VSMSBUILD%\Microsoft\VisualStudio\v17.0\AppxPackage\Microsoft.Build.Packaging.Pri.Tasks.dll

if not exist "%APPX_TASK%" (
  echo [!] No se encontro el task de AppxPackage en:
  echo     %APPX_TASK%
  echo     Asegurate de tener instalada la carga de trabajo "MSIX Packaging Tools" en Visual Studio 2022.
  exit /b 1
)

REM Establecer variables para que dotnet/msbuild resuelvan los targets/tareas desde VS
set MSBuildExtensionsPath=%VSMSBUILD%
set MSBuildExtensionsPath32=%VSMSBUILD%
set VisualStudioVersion=17.0
set VSINSTALLDIR=%VSROOT%\
set Path=%VSMSBUILD_BIN%;%Path%

echo [i] MSBuildExtensionsPath  = %MSBuildExtensionsPath%
echo [i] MSBuildSDKsPath       = %MSBuildSDKsPath%
echo [i] Usando configuracion   = %CONFIG%

REM Restaurar y compilar usando estas rutas
dotnet restore src\GymRoutineGenerator.UI.csproj || exit /b 1
dotnet build src\GymRoutineGenerator.UI.csproj -c %CONFIG% -p:Platform=x64 -p:MSBuildExtensionsPath="%MSBuildExtensionsPath%" -p:MSBuildExtensionsPath32="%MSBuildExtensionsPath32%" -p:VisualStudioVersion=%VisualStudioVersion% -p:VSINSTALLDIR="%VSINSTALLDIR%" || exit /b 1

echo.
echo [ok] Compilacion completada.
echo     Binarios en: src\bin\x64\%CONFIG%\net8.0-windows10.0.22621.0\
exit /b 0

