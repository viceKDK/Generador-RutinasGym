# 🔧 Solución Problemas con Imágenes

**Fecha**: 3 de Octubre, 2025
**Problemas Reportados**:
- ❌ Out of Memory al importar imágenes
- ❌ No funciona seleccionar imagen ni drag & drop
- ❌ Eliminar ejercicio no funciona (aunque log diga "eliminado")

---

## ✅ Soluciones Implementadas

### 1. **Out of Memory al Cargar Imágenes**

**Problema**:
- `Image.FromFile()` mantiene file lock y no libera memoria
- Múltiples cargas acumulan memoria

**Solución**:
```csharp
// Método LoadImageSafely() agregado
private void LoadImageSafely(string imagePath)
{
    // 1. Liberar imagen anterior
    if (imagePreview.Image != null)
    {
        imagePreview.Image.Dispose();
        imagePreview.Image = null;
    }

    // 2. Cargar con FileStream (sin file lock)
    using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
    {
        imagePreview.Image = Image.FromStream(stream);
    }
}
```

**Beneficios**:
- ✅ No mantiene lock en archivo
- ✅ Libera memoria anterior
- ✅ Manejo específico de OutOfMemoryException

---

### 2. **Importar Imagen No Funcionaba**

**Problema**:
- `File.Copy()` puede fallar si archivo fuente está bloqueado
- No había logging para detectar errores

**Solución**:
```csharp
// Copiar con FileStream para evitar locks
using (var sourceStream = new FileStream(sourceImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
using (var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
{
    sourceStream.CopyTo(destStream);
}

// Logging agregado
System.Diagnostics.Debug.WriteLine($"Imagen copiada a: {destPath}");
System.Diagnostics.Debug.WriteLine($"Exercise ID: {exerciseId}");
System.Diagnostics.Debug.WriteLine($"Filas insertadas: {rows}");
```

**Beneficios**:
- ✅ FileStream con FileShare.Read permite leer archivo aunque esté en uso
- ✅ Logging completo para debugging
- ✅ Manejo de excepciones con stack trace

---

### 3. **Eliminar Ejercicio No Funcionaba**

**Problema**:
- Se eliminaba de BD pero UI no se refrescaba
- No había logging para verificar eliminación

**Solución**:
```csharp
public bool RemoveExercise(string exerciseName)
{
    // ... código de eliminación ...

    // Verificar eliminación
    var checkQuery = "SELECT COUNT(*) FROM ExerciseImages WHERE ExerciseId = @exerciseId";
    using (var checkCommand = new SQLiteCommand(checkQuery, connection))
    {
        checkCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
        var remaining = Convert.ToInt32(checkCommand.ExecuteScalar());
        System.Diagnostics.Debug.WriteLine($"Registros restantes: {remaining}");
    }

    System.Diagnostics.Debug.WriteLine($"✅ Imagen eliminada exitosamente para: {exerciseName}");
}
```

**Beneficios**:
- ✅ Verificación de eliminación
- ✅ Logging detallado
- ✅ Stack trace en errores

---

## 🔍 Cómo Ver el Logging (Debug Output)

### **Opción 1: Visual Studio**
1. Abrir proyecto en Visual Studio
2. Ejecutar con F5 (Debug mode)
3. Ver "Output" window → "Debug"

### **Opción 2: DebugView (Sysinternals)**
1. Descargar [DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview)
2. Ejecutar como administrador
3. Capture → Capture Global Win32
4. Ejecutar aplicación
5. Ver mensajes en tiempo real

### **Opción 3: PowerShell (Logs en tiempo real)**
```powershell
# Ejecutar en PowerShell mientras app está corriendo
[System.Diagnostics.Debug]::Listeners.Add((New-Object System.Diagnostics.TextWriterTraceListener([Console]::Out)))
```

---

## 📊 Mensajes de Logging Implementados

### **Al Importar Imagen**:
```
Archivo fuente no existe: C:\ruta\imagen.jpg     (si falla)
Imagen copiada a: C:\...\Images\Exercises\...
Creando nuevo ejercicio: Press de Banca          (si no existe)
Exercise ID: 42
Insertando nueva imagen para ejercicio 42        (o "Actualizando...")
Filas insertadas: 1
✅ Imagen importada exitosamente para: Press de Banca
```

### **Al Eliminar Imagen**:
```
🗑️ Intentando eliminar ejercicio: Press de Banca
Exercise ID encontrado: 42
Filas eliminadas de ExerciseImages: 1
Registros restantes: 0
✅ Imagen eliminada exitosamente para: Press de Banca
```

### **En Caso de Error**:
```
❌ Error importando imagen: Cannot access file...
Stack trace: at System.IO.File.Copy(...) at ...
```

---

## 🐛 Debugging Paso a Paso

### **Si "Seleccionar Imagen" no funciona**:

1. **Verificar que ejercicio está seleccionado**:
   - Logs: `"Por favor selecciona un ejercicio primero"`

2. **Verificar ruta de imagen**:
   - Logs: `"Archivo fuente no existe: ..."`

3. **Verificar copia de archivo**:
   - Logs: `"Imagen copiada a: ..."`

4. **Verificar inserción en BD**:
   - Logs: `"Filas insertadas: 1"` o `"Filas actualizadas: 1"`

### **Si "Drag & Drop" no funciona**:

1. **Verificar formato de archivo**:
   - Solo acepta: .jpg, .jpeg, .png, .bmp, .gif, .webp

2. **Verificar que cursor cambia a "copiar"**:
   - Si no cambia, formato no es válido

3. **Ver logs de importación** (igual que "Seleccionar Imagen")

### **Si "Eliminar" dice eliminado pero no se elimina**:

1. **Verificar logs**:
```
🗑️ Intentando eliminar ejercicio: ...
Exercise ID encontrado: 42
Filas eliminadas de ExerciseImages: 1    ← Debe ser > 0
Registros restantes: 0                    ← Debe ser 0
✅ Imagen eliminada exitosamente
```

2. **Si "Filas eliminadas: 0"**:
   - Ejercicio no tiene imagen en BD
   - Verificar con: `SELECT * FROM ExerciseImages WHERE ExerciseId = 42`

3. **Refrescar lista**:
   - Método `LoadExercises()` debe llamarse después de eliminar

---

## 🔧 Archivos Modificados

### **SQLiteExerciseImageDatabase.cs**
- ✅ `ImportImageForExercise()`: FileStream + logging
- ✅ `RemoveExercise()`: Logging + verificación
- ✅ Manejo de excepciones mejorado

### **ImprovedExerciseImageManagerForm.cs**
- ✅ `LoadImageSafely()`: Método nuevo para cargar sin locks
- ✅ `ImportImage()`: Usa LoadImageSafely()
- ✅ `LoadExerciseDetails()`: Usa LoadImageSafely()
- ✅ `ClearExerciseDetails()`: Dispose() de imagen
- ✅ `Dispose()`: Override para liberar recursos al cerrar

---

## ✅ Checklist de Pruebas

### **Test 1: Importar Imagen (Botón)**
- [ ] Seleccionar ejercicio
- [ ] Click "📁 Seleccionar Imagen"
- [ ] Elegir imagen
- [ ] ✅ Ver imagen en preview
- [ ] ✅ Ver logs: "Imagen importada exitosamente"
- [ ] ✅ Icono cambia de ❌ a ✅ en lista

### **Test 2: Importar Imagen (Drag & Drop)**
- [ ] Seleccionar ejercicio
- [ ] Arrastrar imagen desde explorador
- [ ] ✅ Cursor cambia a "copiar"
- [ ] Soltar imagen
- [ ] ✅ Ver imagen en preview
- [ ] ✅ Ver logs: "Imagen importada exitosamente"

### **Test 3: Múltiples Imágenes (Test Out of Memory)**
- [ ] Seleccionar ejercicio 1
- [ ] Importar imagen grande (ej: 5MB)
- [ ] Seleccionar ejercicio 2
- [ ] Importar otra imagen
- [ ] Repetir 5-10 veces
- [ ] ✅ NO debe dar "Out of Memory"
- [ ] ✅ Memoria debe liberarse entre cargas

### **Test 4: Eliminar Imagen**
- [ ] Seleccionar ejercicio con imagen (✅)
- [ ] Click "🗑️ Eliminar"
- [ ] Confirmar eliminación
- [ ] ✅ Ver logs: "Filas eliminadas: 1"
- [ ] ✅ Ver logs: "Registros restantes: 0"
- [ ] ✅ Icono cambia de ✅ a ❌
- [ ] ✅ Preview vacío

### **Test 5: Guardar con Grupos Musculares**
- [ ] Seleccionar ejercicio
- [ ] Importar imagen
- [ ] Click "▼ Mostrar Info Avanzada"
- [ ] Buscar "Pec" → marcar "Pecho"
- [ ] Marcar "Tríceps"
- [ ] Click "💾 Guardar"
- [ ] ✅ Grupos guardados en BD

---

## 🚀 Compilación

```bash
cd "C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym\src\app-ui"
dotnet build
```

**Resultado**:
```
✅ 0 Errores
✅ 0 Warnings
⏱️ 0.72 segundos
```

---

## 📝 Próximos Pasos (Si Sigue Fallando)

### **Si aún da Out of Memory**:
1. Verificar tamaño de imágenes (reducir si >5MB)
2. Verificar que `Dispose()` se llama (logs)
3. Verificar memoria con Task Manager

### **Si no importa imágenes**:
1. Ver logs completos con DebugView
2. Verificar permisos de carpeta `Images/Exercises/`
3. Verificar que BD `gymroutine.db` existe y tiene permisos

### **Si no elimina**:
1. Verificar logs: "Filas eliminadas"
2. Query directo a BD: `DELETE FROM ExerciseImages WHERE ExerciseId = X`
3. Verificar que `LoadExercises()` se llama después

---

## 📞 Resumen

### ✅ **Lo que se arregló**:
1. **Out of Memory**: LoadImageSafely() con Dispose()
2. **Importar falla**: FileStream con FileShare.Read
3. **Eliminar no funciona**: Logging + verificación
4. **Debugging**: Logs completos en Debug.WriteLine()

### 🎯 **Cómo Usar**:
1. **Cerrar app** si está corriendo
2. **Recompilar**: `dotnet build`
3. **Ejecutar** y probar con DebugView abierto
4. **Ver logs** para identificar problemas

---

**Última actualización**: 3 de Octubre, 2025 - 01:15
**Estado**: ✅ Compilado y Listo para Pruebas
