# Futuras Mejoras del Generador de Rutinas

## 🎥 Feature: Links de Video para Ejercicios

### Descripción
Agregar la capacidad de vincular videos explicativos a cada ejercicio para que los usuarios puedan ver cómo realizar correctamente cada movimiento.

### Estructura de Datos

#### BD Secundaria (ya incluida)
```sql
CREATE TABLE Ejercicios (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    GrupoMuscular TEXT NOT NULL,
    RutaImagen TEXT,
    LinkVideo TEXT,  -- ⭐ CAMPO PARA VIDEOS
    Descripcion TEXT
);
```

### Implementación Futura

#### 1. **En la UI de Gestión de Ejercicios**
- Agregar campo de texto para "Link de Video (YouTube, Vimeo, etc.)"
- Validar que sea una URL válida
- Guardar en BD principal y secundaria

#### 2. **En la Vista de Rutina**
- Mostrar un ícono/botón "▶ Ver Video" junto a cada ejercicio
- Al hacer clic, abrir el video en el navegador predeterminado
- Si no hay video, ocultar el botón

#### 3. **En la Exportación a Word**
- Incluir el link del video como hipervínculo
- Formato: "Ver video demostrativo: [LINK]"

### Código de Ejemplo

```csharp
// En MainForm.cs - Mostrar botón de video
if (!string.IsNullOrWhiteSpace(exercise.VideoLink))
{
    var videoButton = new LinkLabel
    {
        Text = "▶ Ver Video",
        Tag = exercise.VideoLink
    };
    videoButton.LinkClicked += (s, e) => {
        var link = ((LinkLabel)s).Tag.ToString();
        System.Diagnostics.Process.Start(new ProcessStartInfo
        {
            FileName = link,
            UseShellExecute = true
        });
    };
}

// En WordDocumentExporter.cs - Agregar link al documento
if (!string.IsNullOrWhiteSpace(exercise.VideoLink))
{
    var hyperlink = new Hyperlink
    {
        Anchor = exercise.VideoLink,
        InnerXml = "Ver video demostrativo"
    };
    // Agregar al documento
}
```

### Prioridad
- **Baja** - Mejora de calidad de vida
- Se puede implementar después de tener la BD secundaria funcionando

### Estimación de Tiempo
- 2-3 horas de desarrollo
- 1 hora de testing
- Total: ~4 horas

---

## 🗄️ Feature: Base de Datos Secundaria (EN DESARROLLO)

### Estado
- ⏳ En progreso
- Ver `TODO.md` para detalles de implementación

### Descripción
Crear una segunda base de datos que se puebla automáticamente desde `docs/ejercicios`, organizada por grupos musculares.

### Beneficios
1. **Búsqueda en cascada** - Primero BD principal, luego secundaria
2. **Más ejercicios disponibles** - Sin necesidad de importar manualmente
3. **Organización automática** - Basada en estructura de carpetas
4. **Actualización fácil** - Solo agregar carpetas con imágenes

---

## 📊 Otras Mejoras Futuras

### 1. **Estadísticas de Uso**
- Ejercicios más usados
- Rutinas generadas por período
- Tiempo promedio de generación

### 2. **Plantillas de Rutinas**
- Guardar rutinas favoritas como plantillas
- Compartir plantillas entre usuarios
- Importar/Exportar plantillas

### 3. **Historial de Rutinas**
- Guardar historial de rutinas generadas
- Ver evolución temporal
- Replicar rutinas anteriores

### 4. **Integración con Calendario**
- Planificar rutinas semanales/mensuales
- Recordatorios de entrenamiento
- Seguimiento de progreso

### 5. **Modo Offline Mejorado**
- Caché de respuestas de IA comunes
- Generación basada en reglas cuando Ollama no está disponible
- Sincronización cuando vuelve la conexión

---

**Última actualización:** 2025-10-03
**Versión del documento:** 1.0
