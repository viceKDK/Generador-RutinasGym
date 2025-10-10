# Plan: Galeria de ejercicios (iteracion octubre 2025)

## 1. Resumen ejecutivo
- Crear una pantalla dedicada dentro de Herramientas que permita localizar ejercicios manualmente, ver su material visual y operar con ellos sin depender de la generacion automatica.
- Servir como fundamento para workflows manuales (p. ej. armar rutinas personalizadas, preparar material impreso o exportar imagenes).
- Reutilizar al maximo la infraestructura existente de busqueda e imagenes, agregando solo los contratos y UI necesarios para la experiencia manual.

## 2. Objetivos medibles
- Encontrar cualquier ejercicio presente en la base en < 2 s con busqueda incremental.
- Copiar o abrir la imagen seleccionada en <= 2 clics desde la galeria.
- Construir una lista temporal de hasta 20 ejercicios y exportarla/copiarla sin bloqueos en memoria o UI.

## 3. Problema actual y motivaciones
- La app solo permite generar rutinas via IA; usuarios que quieren navegar o elegir ejercicios concretos deben recurrir a soluciones externas.
- No existe una vista consolidada que muestre miniaturas, metadatos y acciones rapidas de cada ejercicio.
- El equipo necesita una base para flujos manuales futuros (p. ej. planificador de rutinas custom, generacion de PDFs).

## 4. Alcance de la iteracion
### 4.1 Funcionalidades obligatorias (MVP)
- Busqueda por nombre (coincidencias parciales, case-insensitive) con debounce de 300 ms.
- Listado tipo galeria con tarjetas que incluyen miniatura, nombre, grupos musculares y acciones rapidas.
- Acciones inmediatas: copiar imagen al portapapeles, abrir ubicacion en el explorador, agregar a lista de seleccion.
- Panel lateral con los ejercicios seleccionados, contador y opciones para quitar elementos y copiar lista (texto plano).
- Acceso directo desde la seccion Herramientas en `MainForm`.

### 4.2 Fuera de alcance (para iteraciones posteriores)
- Exportar directamente a la rutina activa o sincronizar con generador de rutinas.
- Filtros avanzados (equipamiento, dificultad, musculos secundarios).
- Edicion de metadatos o carga de imagenes nuevas.
- Guardar listas persistentes o favoritos.

## 5. Dependencias y datos existentes
- `ExerciseImageSearchService` (src/app-ui/ExerciseImageSearchService.cs) para consultas y metadatos.
- `SQLiteExerciseImageDatabase` como fuente principal de rutas y etiquetas.
- Componentes UI existentes (`ModernCard`, `ModernButton`, estilos comunes) para mantener consistencia visual.
- Helper `OllamaRoutineService` como referencia de integracion mediante servicios singleton.

## 6. Arquitectura propuesta
### 6.1 Vision general
```
UI (ExerciseGalleryForm)
    |
ManualExerciseLibraryService (nuevo)
    |
ExerciseImageSearchService + SQLiteExerciseImageDatabase (existente)
```

### 6.2 Capa de servicios
- `ManualExerciseLibraryService`
  - Constructor recibe una instancia de `ExerciseImageSearchService`.
  - Metodos:
    - `Search(string query, CancellationToken token)` -> `IEnumerable<ExerciseGalleryItem>`.
    - `CopyImageToClipboard(string id)` -> maneja STA y excepciones.
    - `OpenImageLocation(string id)` -> usa rutas absolutas y `Process.Start`.
    - `LoadThumbnail(string path, Size targetSize)` -> devuelve `Image` reducido.
- Maneja cache en memoria de miniaturas (Dictionary<string, Image>) con politicas de liberacion (`Dispose`).

### 6.3 Capa de UI
- Nueva `ExerciseGalleryForm` (WinForms) o `UserControl` embebible.
- Componentes:
  - Barra superior con `TextBox` (busqueda), boton limpiar, contador de resultados, toggle para mostrar solo seleccionados (opcional).
  - `FlowLayoutPanel` o `TableLayoutPanel` para tarjetas responsivas.
  - Panel lateral (`Panel` dockeado a la derecha) con lista de seleccion (`ListView`/`ListBox`) y acciones.
  - Dialogo de vista rapida (modal ligero) para ampliar imagen sin perder contexto (opcional MVP+, se puede dejar placeholder).

### 6.4 Integracion
- `MainForm` agrega entrada en el menu Herramientas y boton en la botonera principal.
- Instanciacion unica de `ExerciseImageSearchService` y `ManualExerciseLibraryService` para reciclar cache.
- Navegacion no modal (ventana hija) para permitir comparar ejercicios mientras se trabaja en otras pantallas.

## 7. Componentes y contratos
| Componente | Tipo | Responsabilidad principal |
|-----------|------|---------------------------|
| `ExerciseGalleryItem` | DTO | Transporte de nombre, identificador, grupos musculares, ruta de imagen, tags opcionales. |
| `ExerciseSelectionEntry` | DTO | Representa elementos seleccionados, incluye timestamp de agregado para ordenar. |
| `ManualExerciseLibraryService` | Servicio | Orquestar busquedas, carga de miniaturas y acciones del portapapeles/explorador. |
| `ExerciseGalleryForm` | UI | Renderizar layout, manejar estados y delegar acciones al servicio. |
| `ExerciseCardControl` (nuevo opcional) | UserControl | Encapsular rendering de tarjeta para reuso y mantenimiento. |

## 8. Flujos de usuario
### 8.1 Flujo principal
1. Usuario abre Herramientas -> Galeria de ejercicios.
2. Pantalla inicia mostrando los ultimos ejercicios vistos o un estado vacio con instrucciones rapidas.
3. Usuario escribe 3+ caracteres; el debounce dispara consulta y muestra resultados progresivamente.
4. Usuario revisa tarjetas, hace hover y ejecuta acciones rapidas:
   - Copia imagen al portapapeles para pegar en otra aplicacion.
   - Abre la ubicacion de archivo en el explorador.
   - Agrega a la lista temporal.
5. Panel lateral actualiza conteo y muestra lista en orden de agregado.
6. Usuario copia la lista (texto) o limpia seleccion antes de cerrar la ventana.

### 8.2 Flujos secundarios
- Limpiar busqueda para volver a estado inicial.
- Remover un ejercicio desde la lista lateral (boton `Quitar`).
- Manejar errores (sin resultados, archivos faltantes, portapapeles bloqueado).

## 9. UX y layout
### 9.1 Distribucion general
- Barra superior fija con busqueda, contador, boton limpiar y toggle `Solo seleccionados`.
- Contenido principal con scroll vertical; tarjetas acomodadas en columnas adaptativas (min 280 px de ancho).
- Panel lateral de 320 px (default) con lista y acciones (`Copiar nombres`, `Copiar rutas`, `Limpiar`).

### 9.2 Tarjeta de ejercicio
- `PictureBox` con thumbnail 200x200 (cover, bordes redondeados).
- Nombre en negritas, grupos musculares en texto secundario.
- Botonera horizontal (`Copiar`, `Abrir`, `Seleccionar`) usando `ModernButton`.
- Iconos simples (FontAwesome/Glyphs ya en el proyecto) para reducir ancho.

### 9.3 Lista lateral de seleccion
- `ListView` en modo detalle mostrando nombre y grupos.
- Acciones por item en hover (boton `Abrir`, `Quitar`).
- Texto inferior que describe como usar la lista en otros flujos (placeholder).

### 9.4 Estados UI
- `Loading`: overlay semitransparente con spinner mientras se espera respuesta inicial.
- `Empty`: mensaje "Escribe al menos 3 caracteres para buscar" + link a documentacion.
- `NoResults`: mensaje con sugerencias (p. ej. revisar plural/singular).
- `Error`: alert bar con opcion "Ver detalle" para copiar stacktrace/log.

## 10. Interacciones clave
- **Busqueda**: debounce 300 ms, cancela consulta previa si el usuario sigue escribiendo.
- **Filtrado rapido**: tecla `Ctrl+F` enfoca el textbox; tecla `Esc` limpia busqueda.
- **Seleccion**: click en `Seleccionar` agrega; repetir click muestra mensaje indicando que ya esta en la lista.
- **Copiar imagen**: servicio asegura ejecucion en hilo STA y muestra `Toast` o `StatusLabel` confirmando.
- **Abrir carpeta**: usa `explorer.exe /select, "path"`, validando existencia del archivo.
- **Copiar lista**: dos formatos disponibles (solo nombres / nombres con rutas) para pegar en otras herramientas.
- **Vista rapida** (MVP+ opcional): doble click sobre tarjeta abre modal con imagen a mayor tamano.

## 11. Manejo de estados y errores
- Validar archivos inexistentes previo a mostrar tarjeta; si falta la imagen, mostrar placeholder y loggear advertencia.
- Portapapeles: capturar `ExternalException` y mostrar mensaje "Portapapeles en uso, intente nuevamente".
- Cuando la consulta regresa > 200 resultados, mostrar aviso para refinar busqueda (no paginar por ahora).
- Registrar eventos en log (nivel Info) para busquedas y acciones, facilitando diagnostico en QA.

## 12. Performance y recursos
- Cache de miniaturas limitada (p. ej. 100 elementos) con estrategia LRU simple.
- Liberar recursos de `Image` cuando se reciclan tarjetas o se cierra la ventana.
- Evitar bloquear el hilo UI: usar `Task.Run` y `SynchronizationContext` para regresar datos.
- Mantener datos en memoria (DTOs) ligeros sin incluir el bitmap hasta que se renderice.

## 13. Integracion con MainForm y navegacion
- Boton `Galeria de ejercicios` en la seccion Herramientas junto a las utilidades ya existentes.
- `MainForm` administra instancia unica del servicio para compartir cache con otras pantallas futuras.
- Al cerrar la galeria, la lista temporal se limpia (no persiste fuera de la sesion actual).

## 14. Roadmap de implementacion
1. **Fundamentos del servicio**
   - Crear DTOs (`ExerciseGalleryItem`, `ExerciseSelectionEntry`).
   - Implementar `ManualExerciseLibraryService` con busqueda y acciones basicas.
2. **UI base**
   - Crear `ExerciseGalleryForm` con layout y carga de resultados sin acciones.
   - Renderizar miniaturas con cache y estados `Loading/Empty`.
3. **Acciones y seleccion**
   - Implementar botones por tarjeta y panel lateral.
   - Soportar copiar lista, limpiar, quitar elementos.
4. **Integracion y pulido**
   - Agregar entrada en `MainForm`, hotkeys, mensajes de feedback.
   - Validar performance con dataset completo y ajustar cache.
5. **QA y documentacion**
   - Checklist manual, captura de pantallas para docs, actualizar README/Herramientas.

## 15. QA y validacion
- Pruebas manuales en Windows 11 con dataset completo (3000+ imagenes).
- Caso negativo: buscar texto vacio, caracteres especiales, nombres inexistentes.
- Validar operaciones con portapapeles, incluyendo conflicto con otras apps.
- Verificar apertura de carpeta tanto con rutas cortas como largas (> 260 caracteres).
- Revisar uso de memoria con Performance Profiler (objetivo < 250 MB adicionales con 100 miniaturas).
- Registrar hallazgos en `docs/qa` (nueva sesion para esta funcionalidad).

## 16. Riesgos y mitigacion
| Riesgo | Impacto | Mitigacion |
|--------|---------|------------|
| Carga lenta de miniaturas | Medio | Generar thumbnails diferidos y cache limitada; mostrar placeholders. |
| Bloqueo de portapapeles | Alto | Reintentar con delay y mostrar mensaje claro al usuario. |
| Rutas inconsistentes en base | Medio | Validar existencia al cargar y registrar faltantes para reparacion. |
| Saturacion del FlowLayoutPanel | Bajo | Configurar virtualizacion manual (lazy load) si supera limites. |

## 17. Telemetria y logging (opcional, MVP+)
- Registrar busquedas y acciones rapidas a nivel debug para diagnosticar usos frecuentes.
- Guardar recuento de ejercicios mas seleccionados para priorizar mejoras futuras.
- Considerar integracion ligera con `Serilog` o logger actual para reportes.

## 18. Preguntas abiertas
- Se requiere persistir la lista de seleccion entre sesiones?
- La galeria debe poder abrir multiples ventanas simultaneas?
- Necesitamos internacionalizacion inmediata o bastan cadenas en castellano?

## 19. Proximas iteraciones sugeridas
- Integrar la lista de seleccion con el generador manual de rutinas (drag & drop).
- Agregar filtros por musculo primario/secundario y equipo necesario.
- Permitir marcar favoritos y guardarlos en SQLite.
- Implementar vista comparativa (dos ejercicios lado a lado).

## 20. Referencias
- `src/app-ui/ExerciseImageSearchService.cs`
- `docs/TESTING-STRATEGY.md` para lineamientos de QA.
- `docs/ai-generation-prompts.md` para comprender el dataset de imagenes existente.
