# 💪 Generador de Rutinas de Gimnasio

Una aplicación de escritorio en C# para generar rutinas de entrenamiento personalizadas con exportación profesional a Word, gestión de imágenes de ejercicios y base de datos SQLite.

![Versión](https://img.shields.io/badge/versión-1.2-green)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Licencia](https://img.shields.io/badge/licencia-MIT-blue)
![Estado](https://img.shields.io/badge/estado-producción-brightgreen)

---

## 📋 Descripción

**Generador-RutinasGym** es una aplicación de escritorio profesional que permite crear rutinas de entrenamiento personalizadas basadas en el perfil del usuario (edad, nivel de fitness, objetivos). Incluye un sistema inteligente de búsqueda de imágenes de ejercicios y exportación automática a documentos Word con formato profesional.

---

## ✨ Características Principales

### 🎯 1. Generación Inteligente de Rutinas

- ✅ **Personalización total**: Basada en edad, género, nivel de fitness y objetivos
- ✅ **Validación automática**: Formulario con validación en tiempo real
- ✅ **Rutinas adaptativas**: Ajustadas a días de entrenamiento disponibles (2-6 días/semana)
- ✅ **15+ ejercicios base**: Catálogo inicial con ejercicios fundamentales
- ✅ **Previsualización**: Vista previa de la rutina antes de exportar

### 🖼️ 2. Gestor de Imágenes de Ejercicios

- ✅ **Drag & Drop**: Arrastra imágenes directamente sobre la interfaz
- ✅ **Multiselección de grupos musculares**: Checkboxes para asignar múltiples grupos
- ✅ **Búsqueda filtrada**: Encuentra ejercicios y grupos musculares rápidamente
- ✅ **Formatos soportados**: JPG, PNG, WEBP, GIF, BMP
- ✅ **Almacenamiento en BD SQLite**: Imágenes embebidas en base de datos
- ✅ **Indicadores visuales**: Marca ejercicios con/sin imagen
- ✅ **Panel colapsible**: Información avanzada con opción de ocultar/mostrar

### 📄 3. Exportación Profesional a Word

- ✅ **Formato nativo .docx**: Compatible con Microsoft Word
- ✅ **Imágenes automáticas**: Búsqueda inteligente de 5 niveles
- ✅ **Estilos profesionales**: Colores, tamaños y formato estructurado
- ✅ **Información completa**:
  - Datos personales
  - Objetivos seleccionados
  - Rutina por día con ejercicios
  - Series y repeticiones
  - Instrucciones detalladas
  - Recomendaciones importantes
- ✅ **Imágenes ajustadas**: 400x300px centradas automáticamente

### 🔍 4. Sistema de Búsqueda Automática de Imágenes (5 Niveles)

#### **Nivel 1: Base de Datos SQLite**
```sql
SELECT ImagePath FROM ExerciseImages 
WHERE ExerciseId = (SELECT Id FROM Exercises WHERE Name = ?)
```

#### **Nivel 2: Cache en Memoria**
- Búsqueda por nombre exacto
- Rendimiento: <1ms

#### **Nivel 3: Mapeo Español ↔ Inglés**
- 40+ ejercicios comunes mapeados
- Ejemplos: "Press de Banca" → "Bench Press"

#### **Nivel 4: Búsqueda Fuzzy (Palabras Clave)**
- Coincidencia por palabras individuales
- Ejemplo: "Press de Banca" → ["press", "banca"]

#### **Nivel 5: Búsqueda en Sistema de Archivos**
- Búsqueda recursiva en `docs/ejercicios/`
- Cacheo automático para optimización

### 🗄️ 5. Base de Datos SQLite Integrada

- ✅ **Conexión automática**: Busca `gymroutine.db` hasta 10 niveles arriba
- ✅ **Tablas principales**:
  - `Exercises` - Catálogo de ejercicios
  - `ExerciseImages` - Imágenes asociadas
  - `MuscleGroups` - Grupos musculares
  - `EquipmentTypes` - Tipos de equipo
  - `UserProfiles` - Perfiles de usuario (opcional)
- ✅ **Sin configuración**: Detección automática de ruta

### 🎨 6. Interfaz de Usuario Moderna

- ✅ **UI intuitiva**: Diseño limpio con WinForms
- ✅ **Responsive**: Ajuste automático de controles
- ✅ **Acceso directo en escritorio**: Icono personalizado
- ✅ **Sin ventana de consola**: Ejecución limpia con VBScript
- ✅ **Portable**: Rutas relativas, funciona desde cualquier ubicación

---

## 🚀 Instalación

### Requisitos Previos

- Windows 10/11
- .NET 8.0 Runtime (se instala automáticamente si no existe)
- 50 MB de espacio en disco

### Opción 1: Instalador Automático (Recomendado)

```bash
# 1. Clona el repositorio
git clone https://github.com/viceKDK/Generador-RutinasGym.git

# 2. Ejecuta el script de instalación
cd Generador-RutinasGym
build-and-install.cmd
```

Esto creará automáticamente:
- Ejecutable compilado
- Acceso directo en el escritorio
- Configuración de base de datos

### Opción 2: Manual

```bash
# 1. Clona el repositorio
git clone https://github.com/viceKDK/Generador-RutinasGym.git

# 2. Compila el proyecto
cd Generador-RutinasGym/src/app-ui
dotnet build -c Debug

# 3. Ejecuta la aplicación
dotnet run
```

---

## 💻 Uso

### 1️⃣ Agregar Imágenes a Ejercicios

#### **Método 1: Drag & Drop (Recomendado)**
1. Abre la aplicación
2. Ve a: **Herramientas → Gestor de Imágenes de Ejercicios**
3. Selecciona un ejercicio de la lista
4. **Arrastra** una imagen desde el explorador de archivos
5. Suelta sobre la vista previa → ✅ **Guardado automático en BD**

#### **Método 2: Selección Manual**
1. Abre la aplicación
2. Ve a: **Herramientas → Gestor de Imágenes de Ejercicios**
3. Selecciona un ejercicio
4. Click en **"📁 Seleccionar Imagen"**
5. Elige archivo (.jpg, .png, .webp, .gif, .bmp)
6. ✅ **Guardado en BD SQLite**

### 2️⃣ Asignar Grupos Musculares

1. En el Gestor de Imágenes, click en **"▼ Mostrar Info Avanzada"**
2. Usa el **buscador** para filtrar grupos musculares
3. Marca **checkboxes** (ej: Pecho, Tríceps, Hombros)
4. Click **"💾 Guardar"** → Asociación guardada

### 3️⃣ Generar Rutina Personalizada

1. Completa el formulario principal:
   - Nombre
   - Edad
   - Género
   - Nivel de fitness (Principiante/Intermedio/Avanzado)
   - Días de entrenamiento (2-6 días/semana)
   - Objetivos (checkboxes múltiples)
2. Click en **"Generar Rutina"**
3. ✅ **Rutina mostrada con imágenes**

### 4️⃣ Exportar a Word

1. Genera una rutina primero
2. Click en **"Exportar a Word"**
3. Elige ubicación (ej: Escritorio)
4. Asigna nombre (ej: `Rutina_Juan_Enero_2026.docx`)
5. ✅ **Documento creado con imágenes y formato profesional**

---

## 📁 Estructura del Proyecto

```
Generador-RutinasGym/
├── src/
│   └── app-ui/
│       ├── MainForm.cs                          # Formulario principal
│       ├── ExerciseImageManagerForm.cs          # Gestor de imágenes
│       ├── IntelligentRoutineGenerator.cs       # Generador de rutinas
│       ├── WordDocumentExporter.cs              # Exportador a Word
│       ├── AutomaticImageFinder.cs              # Búsqueda inteligente
│       ├── SQLiteExerciseImageDatabase.cs       # Conexión SQLite
│       └── GymRoutineUI.csproj                  # Proyecto principal
├── docs/
│   └── ejercicios/                              # Imágenes de ejercicios
│       ├── Abdominales/
│       ├── Pecho/
│       ├── Espalda/
│       ├── Piernas/
│       └── ... (20+ grupos)
├── gymroutine.db                                # Base de datos SQLite
├── gym_icon.ico                                 # Icono de la app
├── ejecutar_rutina_gym.bat                      # Script de ejecución
├── build-and-install.cmd                        # Instalador
└── README.md                                    # Este archivo
```

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|-----------|---------|-----------|
| C# | 12.0 | Lenguaje principal |
| .NET | 8.0 | Framework |
| WinForms | - | Interfaz de usuario |
| SQLite | 3.46 | Base de datos |
| DocumentFormat.OpenXml | 3.3.0 | Generación de .docx |
| System.Data.SQLite.Core | 1.0.118 | Conexión SQLite |

---

## 📊 Rendimiento

- **Primera carga de imágenes**: ~500ms para 500+ archivos
- **Búsquedas posteriores**: <1ms (cache en memoria)
- **Exportación a Word**: 2-5 segundos para rutina completa
- **Tamaño ejecutable**: ~136 KB
- **Compilación**: ✅ 0 errores, 0 warnings críticos

---

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Si deseas mejorar el proyecto:

1. Haz fork del repositorio
2. Crea una rama para tu característica:
   ```bash
   git checkout -b feature/MejoraBuscadorImagenes
   ```
3. Commit tus cambios:
   ```bash
   git commit -m 'feat: Agregar búsqueda por tags'
   ```
4. Push a la rama:
   ```bash
   git push origin feature/MejoraBuscadorImagenes
   ```
5. Abre un Pull Request

---

## 📝 Licencia

Este proyecto está bajo la Licencia MIT - mira el archivo [LICENSE](LICENSE) para más detalles.

---

## ✒️ Autor

**Vicente Lavega** - [@viceKDK](https://github.com/viceKDK)

---

## 🎯 Roadmap

- [ ] Soporte para múltiples idiomas (inglés, francés)
- [ ] Exportación a PDF nativo
- [ ] Sincronización con Google Drive
- [ ] App móvil complementaria (Android/iOS)
- [ ] Integración con wearables (seguimiento de progreso)
- [ ] Sistema de recomendaciones con IA
- [ ] Comunidad de usuarios (compartir rutinas)

---

## 📞 Soporte

Si encuentras algún problema o tienes sugerencias:

1. **Issues**: [Abrir un issue](https://github.com/viceKDK/Generador-RutinasGym/issues)
2. **Documentación**: Ver carpeta `docs/` para guías detalladas
3. **Email**: [Contactar al desarrollador]

---

## 🏆 Características Destacadas

### 🆕 Versión 1.2 (Última)
- ✅ **Drag & Drop** de imágenes en gestor de ejercicios
- ✅ **Multiselect con checkboxes** para grupos musculares
- ✅ **Búsqueda filtrada** de grupos en tiempo real
- ✅ **Panel colapsible** para información avanzada
- ✅ **UI más limpia** y productiva

### ⭐ Versión 1.1
- ✅ Sistema de búsqueda automática de imágenes (5 niveles)
- ✅ Exportación a Word con imágenes embebidas
- ✅ Mapeo español ↔ inglés de ejercicios
- ✅ Base de datos SQLite integrada

### 🎉 Versión 1.0
- ✅ Generador de rutinas personalizadas
- ✅ Gestor de imágenes de ejercicios
- ✅ Formulario con validación
- ✅ Acceso directo en escritorio

---

## 📸 Capturas de Pantalla

### Generador de Rutinas
```
┌─────────────────────────────────────────┐
│  GENERADOR DE RUTINAS DE GIMNASIO      │
├─────────────────────────────────────────┤
│  Nombre: _______________               │
│  Edad: ____  Género: [Masculino ▼]    │
│  Nivel: [Intermedio ▼]                 │
│  Días/semana: [4 ▼]                    │
│                                         │
│  ☑ Ganar músculo                       │
│  ☑ Mejorar fuerza                      │
│  ☐ Perder grasa                        │
│  ☐ Salud general                       │
│                                         │
│  [Generar Rutina] [Exportar a Word]   │
└─────────────────────────────────────────┘
```

### Gestor de Imágenes
```
┌─────────────────────────────────────────┐
│  GESTOR DE IMÁGENES DE EJERCICIOS      │
├─────────────────────────────────────────┤
│  Buscar: [_______________] 🔍          │
│                                         │
│  ✅ Press de Banca                     │
│  ✅ Sentadilla                         │
│  ❌ Peso Muerto (sin imagen)           │
│  ✅ Dominadas                          │
│                                         │
│  ┌───────────────────────┐             │
│  │   [Vista Previa]      │             │
│  │   [Arrastrar aquí]    │             │
│  └───────────────────────┘             │
│                                         │
│  [📁 Seleccionar] [🗑️ Eliminar]       │
│  [▼ Mostrar Info Avanzada]            │
└─────────────────────────────────────────┘
```

---

## 🎓 Patrones de Diseño Utilizados

- ✅ **Strategy Pattern**: Diferentes estrategias de búsqueda de imágenes
- ✅ **Chain of Responsibility**: Búsqueda secuencial por niveles
- ✅ **Facade Pattern**: Simplificación de complejidad en `AutomaticImageFinder`
- ✅ **Factory Pattern**: Creación de `ImagePart` según extensión
- ✅ **Repository Pattern**: Acceso a datos con `SQLiteExerciseImageDatabase`

---

## 🌟 ¿Por qué usar este Generador?

1. ✅ **Gratis y Open Source**: Sin costos ocultos
2. ✅ **Offline**: No requiere conexión a internet
3. ✅ **Profesional**: Documentos Word con formato de calidad
4. ✅ **Personalizable**: Rutinas adaptadas a TU perfil
5. ✅ **Fácil de usar**: Interfaz intuitiva, drag & drop
6. ✅ **Portable**: Llévalo en USB, funciona en cualquier PC
7. ✅ **Extensible**: Código abierto para añadir funcionalidades
8. ✅ **Sin publicidad**: Experiencia limpia

---

**⭐ Si te gusta el proyecto, regala una estrella en GitHub!**

**🎉 ¡APLICACIÓN COMPLETAMENTE FUNCIONAL Y LISTA PARA USAR!**

Última actualización: Diciembre 2025  
Versión: 1.2  
Estado: ✅ Producción