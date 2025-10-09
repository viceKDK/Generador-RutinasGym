# MVP - Generador de Rutinas de Gimnasio

## Descripción
MVP (Minimum Viable Product) funcional e independiente del generador de rutinas de gimnasio.
Esta versión está completamente autocontenida en la carpeta `app-ui` y no depende de otros proyectos.

## Estructura del Proyecto

```
app-ui/
├── Models/                          # Modelos de datos
│   ├── Exercise.cs                  # Modelo de ejercicio
│   └── WorkoutDay.cs               # Modelo de día de entrenamiento
│
├── Enums/                           # Enumeraciones
│   ├── DifficultyLevel.cs          # Niveles de dificultad
│   ├── EquipmentType.cs            # Tipos de equipo
│   ├── ExerciseType.cs             # Tipos de ejercicio
│   ├── Gender.cs                   # Género
│   ├── MuscleGroup.cs              # Grupos musculares
│   └── RoutineStatus.cs            # Estados de rutina
│
├── Forms/                           # Formularios principales
│   ├── MainForm.cs                  # Formulario principal
│   ├── RoutinePreviewForm.cs       # Vista previa de rutina
│   ├── AboutForm.cs                # Acerca de
│   ├── HelpForm.cs                 # Ayuda
│   └── SettingsForm.cs             # Configuración
│
├── Dialogs/
│   └── AddExerciseDialog.cs        # Diálogo para agregar ejercicio
│
├── Services/                        # Servicios de negocio
│   ├── SQLiteExerciseImageDatabase.cs    # Acceso a BD SQLite
│   ├── ExerciseImageDatabase.cs          # Base de datos de imágenes
│   ├── IntelligentRoutineGenerator.cs    # Generador inteligente
│   ├── EnhancedWordExport.cs            # Exportación Word mejorada
│   ├── ImprovedExportService.cs         # Servicio de exportación
│   ├── WordDocumentExporter.cs          # Exportador Word
│   └── AutomaticImageFinder.cs          # Buscador automático de imágenes
│
├── Forms (Gestión)/
│   ├── ExerciseImageManagerForm.cs       # Gestor de imágenes (versión 1)
│   └── ImprovedExerciseImageManagerForm.cs  # Gestor de imágenes (versión 2 - ACTUAL)
│
├── Controls/                        # Controles personalizados
│   ├── ModernButton.cs             # Botón moderno
│   └── ModernCard.cs               # Tarjeta moderna
│
├── Helpers/
│   └── ProgressIndicatorHelper.cs  # Helper de progreso
│
├── Program.cs                       # Punto de entrada
└── GymRoutineUI.csproj             # Archivo de proyecto
```

## Base de Datos

**Ubicación**: `gymroutine.db` en la raíz del proyecto

### Tablas Principales:

#### Exercises
```sql
- Id (INTEGER PRIMARY KEY)
- Name (TEXT)
- SpanishName (TEXT)
- Description (TEXT)
- PrimaryMuscleGroupId (INTEGER)
- EquipmentTypeId (INTEGER)
- DifficultyLevel (INTEGER)
- ExerciseType (INTEGER)
- IsActive (BOOLEAN)
```

#### ExerciseImages
```sql
- Id (INTEGER PRIMARY KEY)
- ExerciseId (INTEGER FK)
- ImageData (BLOB)          -- Imagen almacenada como BLOB
- ImagePath (TEXT)           -- Ruta de imagen (opcional)
- IsPrimary (BOOLEAN)
- Description (TEXT)
```

## Funcionalidades Principales

### 1. Gestión de Ejercicios con Imágenes
- **Formulario**: `ImprovedExerciseImageManagerForm`
- Ver todos los ejercicios disponibles
- Importar imágenes para ejercicios (drag & drop o selección)
- Eliminar imágenes de ejercicios
- Búsqueda y filtrado de ejercicios
- Visualización de detalles de ejercicio
- Edición de grupos musculares (multiselect)

### 2. Generación de Rutinas
- **Formulario**: `MainForm`
- Generación inteligente de rutinas
- Personalización según preferencias del usuario
- Vista previa de rutinas generadas

### 3. Exportación a Word
- **Servicio**: `EnhancedWordExport`, `ImprovedExportService`
- Exportación de rutinas a formato .docx
- Inclusión de imágenes de ejercicios
- Formato profesional y personalizable

## Compilación y Ejecución

### Compilar
```bash
cd src/app-ui
dotnet build -c Debug
```

### Ejecutar desde código
```bash
cd src/app-ui
dotnet run
```

### Ejecutar desde acceso directo
- Doble clic en `Rutina Gym.lnk` en el escritorio
- Apunta a: `src\app-ui\bin\x64\Debug\net8.0-windows\GeneradorRutinasGimnasio.exe`
- Working Directory: Raíz del proyecto (donde está `gymroutine.db`)

## Dependencias

### NuGet Packages
- `Microsoft.Extensions.Hosting` (8.0.1)
- `Microsoft.Extensions.DependencyInjection` (8.0.1)
- `Microsoft.Extensions.Logging` (8.0.1)
- `DocumentFormat.OpenXml` (3.3.0) - Para exportación Word
- `System.Data.SQLite.Core` (1.0.118) - Para acceso a BD

### .NET
- **.NET 8.0 Windows** (net8.0-windows)
- **Windows Forms** habilitado

## Características del MVP

### ✅ Implementado
1. Gestión completa de ejercicios con imágenes (CRUD)
2. Almacenamiento de imágenes como BLOB en SQLite
3. Búsqueda y filtrado de ejercicios
4. Grupos musculares multiselect
5. Drag & Drop para importar imágenes
6. Vista previa de imágenes
7. Exportación a Word con imágenes
8. Interfaz moderna con controles personalizados
9. Logging detallado para debugging

### 🔧 En Desarrollo
- Generación de rutinas con IA (Ollama)
- Sistema de chat conversacional
- Validación de seguridad de rutinas
- Progresión automática

## Debugging

### Logs de Debug
Los logs se escriben usando `System.Diagnostics.Debug.WriteLine()`.

**Para verlos**:
1. **Visual Studio**: Output Window → Debug
2. **DebugView** (Sysinternals): Captura todos los logs de debug
3. **VS Code**: Debug Console cuando se ejecuta con debugger

### Logs Clave
- `SQLiteExerciseImageDatabase` constructor: Muestra ruta de BD
- `FindDatabasePath()`: Muestra búsqueda de gymroutine.db
- `GetAllExercises()`: Muestra ejercicios cargados
- `LoadExercises()`: Muestra recarga de lista
- `ImportImage()`: Muestra proceso de importación de imagen

## Problemas Conocidos y Soluciones

### Problema: "Press de Banca no aparece en lista"
**Causa**: Base de datos correcta pero UI no refresca
**Solución**:
- Agregado `exerciseListBox.Refresh()` explícito
- Logging detallado para verificar datos cargados
- Verificar que `LoadExercises()` se llama después de cambios

### Problema: "Imagen importada pero no se ve en vista previa"
**Causa**: `Image.FromStream()` requiere stream abierto
**Solución**: Crear copia independiente con `new Bitmap(tempImage)`

### Problema: "UI no actualiza después de cambios"
**Causa**: Aplicación cargando ejecutable viejo en caché
**Solución**:
```bash
dotnet clean
dotnet build
# Y actualizar acceso directo si es necesario
```

## Testing

### Insertar Imagen de Prueba
Desde raíz del proyecto:
```bash
cd TestImageTool
dotnet run
```

Esto inserta una imagen de prueba (cuadrado rojo con "TEST") para "Press de Banca".

### Verificar BD
```bash
cd "raiz del proyecto"
sqlite3 gymroutine.db "SELECT e.SpanishName, LENGTH(ei.ImageData) FROM Exercises e LEFT JOIN ExerciseImages ei ON e.Id = ei.ExerciseId WHERE ei.ImageData IS NOT NULL;"
```

## Arquitectura Futura

Este MVP está diseñado para ser **independiente y funcional**.

### Mejoras Planificadas
1. **Integrar con Clean Architecture**
   - Conectar con `GymRoutineGenerator.Domain`
   - Usar `GymRoutineGenerator.Data` para persistencia
   - Implementar CQRS con MediatR

2. **Separar Responsabilidades**
   - Mover lógica de negocio a `Application` layer
   - Usar repositorios de `Data` layer
   - DTOs en lugar de entidades directas

3. **Testing**
   - Unit tests para servicios
   - Integration tests para BD
   - UI tests automatizados

## Notas de Desarrollo

### Por Qué app-ui es Independiente
- Proyecto Data tiene 33 errores de compilación
- Proyecto Infrastructure depende de Data
- MVP necesita funcionar AHORA sin esperar refactoring completo
- Permite desarrollo ágil: funcionalidad primero, arquitectura después

### Próximos Pasos
1. ✅ MVP funcional independiente
2. 🔄 Arreglar errores en Data layer
3. 🔄 Conectar app-ui con arquitectura limpia
4. 🔄 Migrar a CQRS pattern
5. 🔄 Agregar testing completo

## Contacto y Contribución

Para reportar problemas o sugerir mejoras, contactar al equipo de desarrollo.

---

**Versión MVP**: 1.0
**Fecha**: Octubre 2025
**Estado**: ✅ Funcional e Independiente
