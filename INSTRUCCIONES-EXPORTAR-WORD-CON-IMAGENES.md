# ✅ Exportación a Word con Imágenes AUTOMÁTICA - IMPLEMENTADO

## 🎉 ¡Funcionalidad Completada con Búsqueda Automática de Imágenes!

Tu aplicación ahora **exporta rutinas a Word (.docx) con imágenes automáticas** desde:
- 🗄️ Base de datos SQLite (tabla ExerciseImages)
- 📁 Carpeta `docs/ejercicios/` (búsqueda inteligente por nombre)

---

## 📍 Ubicación del Ejecutable

```
src/app-ui/bin/x64/Debug/net8.0-windows/GeneradorRutinasGimnasio.exe
```

---

## 🚀 Cómo Usar

### 1. **Ejecutar la Aplicación**
```bash
cd "src/app-ui/bin/x64/Debug/net8.0-windows"
./GeneradorRutinasGimnasio.exe
```

O hacer doble clic en el archivo `.exe`

### 2. **Generar una Rutina**
1. Llena la información personal:
   - Nombre
   - Edad
   - Género
   - Nivel de fitness (Principiante/Intermedio/Avanzado)

2. Selecciona días de entrenamiento (2-6 días)

3. Marca objetivos:
   - Pérdida de peso
   - Ganar músculo
   - Mejorar resistencia
   - Salud general
   - Etc.

4. Haz clic en **"Generar Rutina"**

### 3. **Exportar a Word con Imágenes**
1. Una vez generada la rutina, haz clic en **"Exportar a Word"**

2. Elige dónde guardar el archivo

3. El sistema generará un archivo `.docx` con:
   - ✅ Título profesional
   - ✅ Fecha de generación
   - ✅ Información personal
   - ✅ Objetivos en viñetas
   - ✅ Rutina por día
   - ✅ **Imágenes de ejercicios AUTOMÁTICAS** (desde BD o carpeta docs/ejercicios)
   - ✅ Instrucciones de cada ejercicio
   - ✅ Series y repeticiones
   - ✅ Recomendaciones importantes

---

## 🤖 Sistema de Búsqueda Automática de Imágenes

### ¿Cómo Funciona?

El sistema busca imágenes en este orden:

1. **Base de Datos** (`gymroutine.db` → tabla `ExerciseImages`)
   - Si el ejercicio tiene imagen en BD, la usa

2. **Carpeta docs/ejercicios/** (búsqueda inteligente)
   - Busca por nombre exacto del ejercicio
   - Busca usando mapeo de nombres comunes (español ↔ inglés)
   - Búsqueda fuzzy por palabras clave (al menos 2 coincidencias)
   - Soporta JPG, PNG, WEBP, GIF, BMP

### Ubicaciones de Imágenes

#### Carpeta Principal de Ejercicios
```
docs/ejercicios/
├── Abdominales/
│   ├── 34 sentadilla/
│   │   └── imagen.jpg
│   ├── AB Roller Crunch/
│   └── ...
├── Pecho/
│   ├── Bench Press/
│   ├── Barbell Larsen Press/
│   └── ...
├── Espalda/
├── Piernas/
└── ...
```

#### Base de Datos (opcional)
```
src/app-ui/bin/x64/Debug/net8.0-windows/Images/Exercises/
```

### Agregar Imágenes de Ejercicios
1. En la aplicación, ve al menú **"Herramientas" → "Gestor de Imágenes"**

2. Haz clic en **"Agregar Nuevo Ejercicio"**

3. Completa la información:
   - Nombre del ejercicio
   - Palabras clave (para búsqueda)
   - Grupos musculares
   - Descripción

4. Haz clic en **"Seleccionar Imagen"** y elige una foto del ejercicio

5. Guarda los cambios

### Formato de Imágenes Recomendado
- **Formato**: JPG, PNG
- **Tamaño**: 400x300 píxeles (se redimensiona automáticamente)
- **Peso**: Menor a 500KB para documentos ligeros

---

## 🎨 Características del Documento Word Generado

### Estilos y Formato
- **Título principal**: Verde (16pt, negrita)
- **Secciones**: Azul (14pt, negrita)
- **Días de entrenamiento**: Verde claro (12pt, negrita)
- **Ejercicios**: Negrita
- **Instrucciones**: Cursiva
- **Imágenes**: Centradas, 400x300px

### Estructura del Documento
```
RUTINA DE GIMNASIO PERSONALIZADA
  Generado el: DD/MM/YYYY HH:mm

INFORMACIÓN PERSONAL
  - Nombre: ...
  - Edad: ...
  - Nivel: ...

OBJETIVOS SELECCIONADOS
  • Objetivo 1
  • Objetivo 2

RUTINA DE ENTRENAMIENTO
  DÍA 1: TREN SUPERIOR
    Press de Banca
      3 series x 10 repeticiones
      Instrucciones detalladas...
      [IMAGEN DEL EJERCICIO]

    Remo con Barra
      ...

  DÍA 2: TREN INFERIOR
    ...

RECOMENDACIONES IMPORTANTES
  • Calienta adecuadamente (5-10 min)
  • Mantén técnica correcta
  • ...
```

---

## 🔧 Compilar desde Código Fuente

Si necesitas recompilar:

```bash
cd src/app-ui
dotnet build -c Debug
```

O para versión Release (optimizada):

```bash
cd src/app-ui
dotnet build -c Release
```

El ejecutable quedará en:
- Debug: `bin/x64/Debug/net8.0-windows/`
- Release: `bin/x64/Release/net8.0-windows/`

---

## 📝 Notas Técnicas

### Tecnologías Utilizadas
- **DocumentFormat.OpenXml 3.3.0**: Generación de archivos .docx reales
- **System.Drawing.Common**: Manejo de imágenes
- **.NET 8.0**: Framework base

### Características Implementadas
✅ Exportación a formato .docx nativo de Word
✅ **Búsqueda automática de imágenes** desde docs/ejercicios
✅ **Mapeo inteligente** de nombres español ↔ inglés
✅ **Búsqueda fuzzy** por palabras clave
✅ Soporta múltiples formatos: JPG, PNG, WEBP, GIF, BMP
✅ Inserción de imágenes con dimensiones controladas (400x300px)
✅ Estilos de texto (negrita, cursiva, colores)
✅ Encabezados con jerarquía (H1, H2, H3)
✅ Viñetas y listas
✅ Centrado de imágenes
✅ Espaciado entre secciones

### Mapeo de Nombres Incluido
El sistema reconoce automáticamente estos nombres comunes:

**Pecho:**
- Press de Banca → Bench Press
- Press Banca → Bench Press
- Flexiones → Push Up
- Aperturas → Fly

**Espalda:**
- Remo con Barra → Barbell Row
- Dominadas → Pull Up
- Jalones → Lat Pulldown
- Peso Muerto → Deadlift

**Piernas:**
- Sentadilla/Sentadillas → Squat
- Prensa → Leg Press
- Zancadas → Lunge
- Curl Femoral → Leg Curl
- Elevaciones de Pantorrilla → Calf Raise

**Hombros:**
- Press Militar → Military Press
- Elevaciones Laterales → Lateral Raise

**Brazos:**
- Curl de Bíceps → Bicep Curl
- Extensiones de Tríceps → Tricep Extension
- Fondos → Dip

**Abdominales:**
- Abdominales → Crunch
- Plancha → Plank
- Elevación de Piernas → Leg Raise

### Limitaciones Actuales
✅ Archivos WEBP se convierten automáticamente a PNG para compatibilidad
✅ Si no hay imagen para un ejercicio, continúa sin error
⚠️ El documento se genera en formato .docx (Word 2013+)

---

## 🐛 Solución de Problemas

### La aplicación no abre
- Verifica que tengas .NET 8.0 Desktop Runtime instalado
- Descarga desde: https://dotnet.microsoft.com/download/dotnet/8.0

### No se exportan las imágenes
1. **Verifica que exista la carpeta** `docs/ejercicios/` en la raíz del proyecto
2. **Estructura correcta**: Las imágenes deben estar dentro de carpetas por grupo muscular
   ```
   docs/ejercicios/
   ├── Pecho/
   │   └── Press de Banca/
   │       └── imagen.jpg
   ```
3. **Nombres de ejercicios**: El sistema buscará automáticamente por:
   - Nombre exacto
   - Nombre traducido (usa el mapeo incluido)
   - Palabras clave (búsqueda fuzzy)
4. **Verifica que las imágenes sean** JPG, PNG, WEBP, GIF o BMP
5. Si tienes muchas imágenes pero no aparecen, revisa que la ruta relativa `docs/ejercicios` sea correcta desde el ejecutable

### El documento Word se ve mal
- Abre el documento en Microsoft Word (no WordPad)
- Compatible con Word 2013 o superior
- También funciona en LibreOffice Writer y Google Docs

---

## 📞 Contacto y Soporte

Si encuentras algún problema:
1. Revisa este documento primero
2. Verifica los logs en la consola de la aplicación
3. Asegúrate de tener las imágenes en la ubicación correcta

---

## 🎯 Próximos Pasos (Opcional)

Para mejorar la aplicación, podrías:
- [ ] Agregar más imágenes de ejercicios
- [ ] Crear plantillas personalizadas
- [ ] Exportar a PDF adicional
- [ ] Agregar gráficos de progreso
- [ ] Sincronizar con base de datos online

---

**✨ ¡Disfruta de tu generador de rutinas con exportación profesional a Word!**
