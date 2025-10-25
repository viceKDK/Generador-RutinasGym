On Error Resume Next
Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")

' Matar todas las instancias existentes antes de lanzar una nueva
' Verificar si hay instancias ejecutandose
strComputer = "."
Set objWMIService = GetObject("winmgmts:\\" & strComputer & "\root\cimv2")

If Not IsNull(objWMIService) Then
    Set colProcesses = objWMIService.ExecQuery("Select * from Win32_Process Where Name = 'GeneradorRutinasGimnasio.exe'")

    If colProcesses.Count > 0 Then
        ' Usar taskkill para cerrar todas las instancias
        objShell.Run "taskkill /F /IM GeneradorRutinasGimnasio.exe", 0, True
        ' Esperar a que se cierren
        WScript.Sleep 1000
    End If
End If

' Obtener la ruta del directorio del script
scriptPath = objFSO.GetParentFolderName(WScript.ScriptFullName)

' Intentar primero con Release, luego Debug
exePathRelease = scriptPath & "\src\app-ui\bin\x64\Release\net8.0-windows\GeneradorRutinasGimnasio.exe"
exePathDebug = scriptPath & "\src\app-ui\bin\x64\Debug\net8.0-windows\GeneradorRutinasGimnasio.exe"

If objFSO.FileExists(exePathRelease) Then
    exePath = exePathRelease
ElseIf objFSO.FileExists(exePathDebug) Then
    exePath = exePathDebug
Else
    MsgBox "No se encontro el ejecutable en ninguna ubicacion." & vbCrLf & vbCrLf & _
           "Buscado en:" & vbCrLf & _
           "- " & exePathRelease & vbCrLf & _
           "- " & exePathDebug, vbCritical, "Error - Rutina Gym"
    WScript.Quit
End If

' Ejecutar la aplicacion maximizada (3 = ventana maximizada)
objShell.Run """" & exePath & """", 3, False
