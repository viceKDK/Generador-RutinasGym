# Epic 3: User Input & Preference Engine

**Epic Goal:** Implementar la interfaz principal para recopilar parámetros completos del cliente incluyendo demografía, preferencias de entrenamiento, disponibilidad de equipamiento, áreas de enfoque muscular y limitaciones físicas. Este epic crea la interfaz ultra-simple en español, amigable para abuela, que captura todos los datos necesarios para personalización de rutinas con AI.

## Story 3.1: Formulario Básico de Demografía del Cliente
As a gym owner,
I want to input basic client information (gender, age, training days) in Spanish,
so that routines can be tailored to demographic-appropriate exercise selection.

### Acceptance Criteria
1. Formulario limpio con campos de entrada grandes y claramente etiquetados en español
2. Selección de género con botones de radio (Hombre/Mujer/Otro)
3. Entrada de edad con validación numérica (16-100 años) con etiqueta "Edad"
4. Selector de días de entrenamiento por semana (1-7 días) etiquetado "Días por semana" con indicadores visuales
5. Validación de formulario con mensajes de error claros y útiles en español

## Story 3.2: Interfaz de Preferencias de Equipamiento
As a gym owner,
I want to specify available equipment for each client in Spanish,
so that generated routines only include accessible exercises.

### Acceptance Criteria
1. Selección de equipamiento con checkboxes grandes y etiquetas en español
2. Categorías: "Pesas Libres", "Máquinas", "Peso Corporal", "Bandas Elásticas", "Otros"
3. Opciones de conveniencia "Seleccionar Todo" y "Limpiar Todo"
4. Íconos visuales de equipamiento para fácil reconocimiento
5. Selección predeterminada se guarda para generaciones posteriores de rutinas

## Story 3.3: Enfoque Muscular y Objetivos de Entrenamiento
As a gym owner,
I want to specify which muscle groups to emphasize in Spanish,
so that routines align with client-specific fitness goals.

### Acceptance Criteria
1. Diagrama corporal con regiones de grupos musculares clickeables etiquetadas en español
2. Controles deslizantes para nivel de énfasis (Bajo, Medio, Alto) por grupo muscular
3. Plantillas de objetivos predefinidas ("Pérdida de Peso", "Ganancia Muscular", "Fitness General")
4. Selección de múltiples grupos musculares con ordenamiento de prioridad
5. Retroalimentación visual clara mostrando áreas de enfoque seleccionadas

## Story 3.4: Limitaciones Físicas y Restricciones
As a gym owner,
I want to record client limitations and exercise restrictions in Spanish,
so that generated routines are safe and appropriate.

### Acceptance Criteria
1. Checkboxes de limitaciones comunes ("Problemas de Espalda", "Problemas de Rodilla", "Problemas de Hombro", etc.)
2. Entrada de texto personalizada para restricciones específicas en español
3. Lista de exclusión de ejercicios con búsqueda y selección en español
4. Ajuste de nivel de intensidad basado en limitaciones
5. Descargo médico y recordatorios de seguridad en español

## Story 3.5: Pulido de UI Amigable para Abuela (Español)
As a non-technical user,
I want an interface in Spanish so simple that I can use it without training,
so that I can generate routines independently.

### Acceptance Criteria
1. Botones extra-grandes (mínimo 60px de altura) con etiquetas claras en español
2. Colores de alto contraste y fuentes legibles (mínimo 16px)
3. Flujo de trabajo de una sola página evitando complejidad de navegación
4. Indicadores de progreso mostrando estado de completación en español
5. Todo el texto de UI, tooltips y contenido de ayuda en español apropiado
