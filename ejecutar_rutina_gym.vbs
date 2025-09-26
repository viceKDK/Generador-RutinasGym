Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")

' Obtener la ruta del directorio del script
scriptPath = objFSO.GetParentFolderName(WScript.ScriptFullName)
appPath = scriptPath & "\app-ui"

' Ejecutar la aplicación sin mostrar ventana de terminal
objShell.Run "cmd /c ""cd /d """ & appPath & """ && dotnet run""", 0, False