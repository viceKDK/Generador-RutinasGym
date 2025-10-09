Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")

' Obtener la ruta del directorio del script
scriptPath = objFSO.GetParentFolderName(WScript.ScriptFullName)
exePath = scriptPath & "\src\app-ui\bin\x64\Debug\net8.0-windows\GeneradorRutinasGimnasio.exe"

' Verificar si existe el ejecutable
If objFSO.FileExists(exePath) Then
    ' Ejecutar la aplicación directamente sin mostrar ventana de terminal
    objShell.Run """" & exePath & """", 1, False
Else
    ' Si no existe el ejecutable, mostrar error
    MsgBox "No se encontró el ejecutable en: " & vbCrLf & exePath, vbCritical, "Error"
End If