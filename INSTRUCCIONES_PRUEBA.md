# Instrucciones para Probar la Generación de Rutinas con IA

## ✅ Cambios Realizados:

### 1. **Prompt Mejorado**
- Más específico y directo
- Solicita EXACTAMENTE 5 ejercicios
- Instrucciones claras sobre formato
- Ajusta series/reps según nivel y edad

### 2. **Logging Agregado**
- Ahora la aplicación muestra en el Debug Output:
  - La respuesta completa de la IA
  - Cuántos bloques de ejercicios se parsearon
  - Qué ejercicios se encontraron
  - Si tienen imágenes o no

### 3. **Parser Mejorado**
- Mejor manejo de espacios y saltos de línea
- Logging detallado de cada bloque
- Manejo de errores mejorado

## 🔧 Cómo Probar:

### Paso 1: Asegúrate de que Ollama esté corriendo

Abre una terminal y ejecuta:
```bash
ollama serve
```

Deja esta terminal abierta.

### Paso 2: Verifica que Mistral esté instalado

En otra terminal:
```bash
ollama pull mistral
```

### Paso 3: Ejecuta la aplicación

Doble click en el acceso directo del escritorio: **Rutina Gym**

### Paso 4: Llena el formulario

Ejemplo de prueba:
- Nombre: **Maria**
- Edad: **55**
- Género: **Mujer**
- Nivel: **Principiante**
- Días/semana: **3**
- Objetivos: Marcar algunos (ej: Fuerza, Movilidad)

### Paso 5: Genera la rutina

1. Click en "Generar Rutina"
2. Espera 1-2 minutos (la IA está pensando)
3. Verás el progreso: "Generando rutina con IA (Mistral)..."

### Paso 6: Revisa el Debug Output

Para ver qué está pasando internamente:

**Opción A - Visual Studio:**
1. Abre Visual Studio
2. Abre el proyecto
3. Ve a View → Output
4. En "Show output from:" selecciona "Debug"

**Opción B - VS Code:**
1. Abre VS Code
2. Terminal → Debug Console
3. Ejecuta la app con F5

**Opción C - DebugView (recomendado para esta prueba):**
1. Descarga DebugView de Sysinternals
2. Ejecuta como administrador
3. Verás todos los mensajes de Debug.WriteLine

## 🔍 Qué Ver en el Debug Output:

Deberías ver algo como:

```
=== RESPUESTA IA PARA Dia 1 - Cuerpo Completo A ===
[EJERCICIO]Press de Banca
[SERIES]3x12
[INSTRUCCIONES]Mantener postura correcta y respirar bien
[FIN]

[EJERCICIO]Sentadillas
[SERIES]3x12
[INSTRUCCIONES]Bajar hasta 90 grados, espalda recta
[FIN]
...
=== FIN RESPUESTA ===
Bloques encontrados: 5
Parseado: Press de Banca - 3x12
  Imagen encontrada: Database
Parseado: Sentadillas - 3x12
  Imagen encontrada: DocsFolder
Total ejercicios parseados para Dia 1 - Cuerpo Completo A: 5
```

## ✅ PROBLEMA RESUELTO (2025-10-03)

**El problema de las rutinas vacías ha sido identificado y corregido.**

### Causa Raíz:
El servicio `OllamaRoutineService` buscaba grupos musculares que **no existían** en la base de datos:
- Buscaba: "Cuadriceps", "Isquiotibiales", "Biceps", "Triceps", "Pantorrillas"
- Base de datos tiene: "Pecho", "Espalda", "Hombros", "Brazos", "Piernas", "Core", "Glúteos"

### Solución Aplicada:
Todos los muscle groups fueron actualizados para coincidir con los que existen en la BD.

**Ver**: `SOLUCION-RUTINAS-VACIAS.md` para detalles completos.

---

## ⚠️ Si TODAVÍA No Funciona:

### Problema: Rutina vacía después de la corrección

**Causa posible 1**: La IA no está respondiendo en el formato correcto

**Solución**:
- Revisa el Debug Output
- Busca la sección "=== RESPUESTA IA PARA ==="
- Copia esa respuesta y envíamela para que pueda ajustar el parser

**Causa posible 2**: Ollama no está corriendo

**Solución**:
- Abre terminal
- Ejecuta: `ollama serve`
- Verifica: `curl http://localhost:11434/api/tags`

**Causa posible 3**: Timeout de Ollama

**Solución**:
- Incrementa el timeout en OllamaRoutineService
- O usa un modelo más pequeño/rápido

### Problema: Ollama no disponible

**Error**: Muestra mensaje "Ollama no esta disponible"

**Solución**:
1. Abre terminal
2. Ejecuta: `ollama serve`
3. Verifica que responde: `curl http://localhost:11434/api/tags`
4. Intenta generar la rutina nuevamente

## 📧 Qué Enviarme si Hay Problemas:

1. **Debug Output completo** (desde "=== RESPUESTA IA ===" hasta "=== FIN RESPUESTA ===")
2. **Parámetros del usuario** (edad, nivel, días)
3. **Lo que muestra en pantalla** (la rutina vacía o incompleta)

Con esa información puedo ajustar:
- El prompt para que la IA responda mejor
- El parser para que entienda la respuesta
- El formato esperado

## 🎯 Resultado Esperado:

Una rutina completa como esta:

```
RUTINA PERSONALIZADA GENERADA CON IA

INFORMACION DEL CLIENTE:
  Nombre: Maria
  Edad: 55 anos
  Genero: Mujer
  Nivel: Principiante
  Frecuencia: 3 dias/semana

PLAN DE ENTRENAMIENTO:

 DIA 1 - CUERPO COMPLETO A

 Press de Banca
    3x12
    Mantener postura correcta y respirar bien
    Imagen: Disponible

 Sentadillas
    3x12
    Bajar hasta 90 grados, espalda recta
    Imagen: Disponible

 [... 3 ejercicios más ...]

 DIA 2 - CUERPO COMPLETO B
 [... 5 ejercicios ...]

 DIA 3 - CUERPO COMPLETO C
 [... 5 ejercicios ...]

RUTINA COMPLETADA!
Generada con IA (Mistral)
```

---

**Nota**: El problema que tuviste (rutina vacía) probablemente se debe a que la IA no está siguiendo el formato `[EJERCICIO]...[FIN]` correctamente. El logging que agregué nos ayudará a ver exactamente qué está respondiendo para ajustarlo.
