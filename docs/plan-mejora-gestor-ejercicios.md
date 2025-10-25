## Plan de Mejora — Gestor de Ejercicios e Imágenes

### 1. Objetivo
- Modernizar la pantalla donde se agregan y gestionan ejercicios en la base de datos.
- Simplificar y clarificar los flujos de alta, visualización, edición y gestión de imágenes.

### 2. Problemas Detectados
- Interfaz recargada y poco clara para los flujos “Agregar”, “Ver” y “Editar”.
- Falta de guía y validaciones al crear ejercicios y adjuntar imágenes.
- Navegación confusa entre lista y detalle; edición no evidente.

### 3. Alcance (MVP mejorado)
- Crear ejercicios con metadatos y al menos una imagen.
- Visualizar listado con búsqueda/filtrado y miniaturas.
- Editar metadatos e imágenes existentes.
- Validación básica de duplicados y consistencia de datos.
- Mantener compatibilidad con el esquema actual (migraciones menores si fueran necesarias).

### 4. Requisitos Funcionales
#### 4.1 Crear
- Campos: Nombre, Alias, Grupos musculares, Equipamiento, Dificultad, Fuente/URL, Etiquetas, Notas.
- Imágenes: subir archivo, arrastrar/soltar, pegar desde portapapeles; copiar a carpeta interna con renombre consistente.

#### 4.2 Listar y Buscar
- Lista virtualizada con miniaturas.
- Búsqueda por nombre/etiquetas y filtros por grupo muscular, equipamiento y dificultad.

#### 4.3 Ver Detalle
- Panel con vista amplia de la imagen principal y metadatos legibles.
- Acciones rápidas: abrir imagen, abrir carpeta de origen, copiar ruta.

#### 4.4 Editar
- Edición inline en panel derecho con botones Guardar/Cancelar.
- Validación en vivo de campos requeridos y formato.

#### 4.5 Gestión de Imágenes
- Reemplazar imagen principal, añadir secundarias, reordenar y quitar elementos.
- Recorte/ajuste básicos (opcional en Iteración 4).

#### 4.6 Validaciones y Duplicados
- Nombre requerido y único (slug).
- Al menos un grupo muscular.
- Imagen válida (formato, tamaño, existencia).
- Detección de duplicados por nombre similar y hash perceptual (si el sistema lo soporta).

#### 4.7 Atajos
- `Ctrl+N` (nuevo), `Ctrl+S` (guardar), `F2` (renombrar), `Del` (eliminar), `Ctrl+F` (buscar).

### 5. Requisitos No Funcionales
- Rendimiento: miniaturas cacheadas, carga asíncrona, UI no bloqueante, virtualización.
- Confiabilidad: operaciones de base de datos transaccionales con rollback en fallos, logging claro.
- Accesibilidad: soporte completo de teclado, contraste adecuado, estados vacíos/errores descriptivos.

### 6. Diseño de UI (WinForms)
- Layout maestro-detalle-editor: lista a la izquierda (thumbnails + texto), detalle central, editor a la derecha.
- Barra de acciones superior: Nuevo, Editar/Guardar, Duplicar, Eliminar, Añadir imagen, Abrir carpeta.
- Estilos: tipografía Segoe UI, ModernButton, ModernCard, status bar con feedback contextual.
- Estados: vacío (sin resultados), cargando, error con opción “Reintentar”.

### 7. Datos y Migración
- Tablas sugeridas:
  - `Exercises`: id, name, slug, primary_image, difficulty, equipment, source, tags, notes, created_at, updated_at.
  - `ExerciseImages`: id, exercise_id, path, width, height, p_hash, is_primary.
  - `ExerciseMuscles`: exercise_id, muscle (relación N–N).
- Migración: añadir columnas/tablas sin romper lecturas actuales; migrar imagen principal a `ExerciseImages`; generar slug/índices donde falten.

### 8. Arquitectura y Servicios
- Repositorio de ejercicios para CRUD, búsqueda, filtros y paginación.
- Servicio de imágenes para importar, copiar/renombrar, generar miniaturas y hash perceptual.
- Servicio de validación con reglas centralizadas y mensajes reutilizables.
- Caché de miniaturas en memoria/disco (`.thumbs`) para mejorar tiempos de carga.

### 9. Iteraciones de Implementación
1. **I1**: Estructurar UI (split containers), lista virtual + búsqueda, detalle de solo lectura.
2. **I2**: Crear ejercicio y adjuntar imagen (drag & drop/archivo/portapapeles) con validaciones.
3. **I3**: Editar/Guardar/Cancelar, duplicar ejercicio, confirmaciones al salir con cambios.
4. **I4**: Gestión avanzada de imágenes (multiples imágenes, principal, recorte básico, reordenar).
5. **I5**: Detección de duplicados, cache de miniaturas y mejoras de rendimiento.
6. **I6**: Eliminar (blando/duro según configuración), logging y telemetría local.
7. **I7**: Pulido visual, accesibilidad, documentación y material de onboarding (capturas/video corto).

### 10. Criterios de Aceptación (MVP)
- Crear y editar ejercicios sin bloquear la UI y con validaciones claras.
- Búsquedas y filtros responden en menos de 150 ms con más de 3 000 registros.
- Se avisa al usuario ante potenciales duplicados antes de guardar.
- Los cambios persisten en la base de datos y se reflejan en la galería utilizada por el resto de la aplicación.

### 11. Riesgos y Mitigación
- Esquema de base de datos variable o desconocido → fase de descubrimiento y adaptadores; migraciones opcionales.
- Rendimiento con listas grandes → virtualización y caché de miniaturas.
- Integridad de archivos de imagen → operaciones transaccionales y verificación post-copia con fallback.

### 12. Entregables
- Nueva pantalla de gestor con layout modernizado y servicios actualizados.
- Script/mecanismo de migración (si aplica), guía de uso y checklist de QA.
