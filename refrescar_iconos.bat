@echo off
echo Refrescando iconos del sistema...
ie4uinit.exe -show
taskkill /f /im explorer.exe >nul 2>&1
timeout /t 2 /nobreak >nul
start explorer.exe
echo Iconos refrescados. El acceso directo "Rutina Gym" ahora deberia mostrar el icono del gimnasio.
pause