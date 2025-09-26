@echo off
setlocal
set APPDIR=%~dp0
set EXE=%APPDIR%GymRoutineGenerator.UI.exe
for /f "tokens=2,*" %%A in ('reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" /v Desktop ^| find "Desktop"') do set DESKTOP=%%B

echo Creando acceso directo en el escritorio...
powershell -NoProfile -ExecutionPolicy Bypass -Command "\
  $shell = New-Object -ComObject WScript.Shell; \
  $lnk = $shell.CreateShortcut(\"$env:USERPROFILE\\Desktop\\Generador de Rutinas.lnk\"); \
  $lnk.TargetPath = \"%EXE%\"; \
  $lnk.WorkingDirectory = \"%APPDIR%\"; \
  $lnk.IconLocation = \"%EXE%\"; \
  $lnk.WindowStyle = 1; \
  $lnk.Save()"
echo Listo.
pause
