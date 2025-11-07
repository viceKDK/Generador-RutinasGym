# Guía de Desarrollo - Aplicación Electron

## Estado Actual del Proyecto

### ✅ Completado

1. **Estructura Base**
   - Proyecto Electron inicializado
   - Configuración de Vite + React + TypeScript
   - TailwindCSS configurado para estilos modernos

2. **Arquitectura**
   - Proceso principal de Electron (`electron/main.ts`)
   - Script de preload con IPC seguro (`electron/preload.ts`)
   - Tipos TypeScript completos
   - Componentes React modulares

3. **UI Implementada**
   - HomePage: Pantalla de bienvenida con guía rápida
   - RoutineGenerator: Wizard de 3 pasos para generar rutinas
   - ExerciseLibrary: Biblioteca con filtros y búsqueda
   - Settings: Configuración de Ollama y base de datos
   - Navegación con React Router
   - Diseño responsive y moderno

4. **Integraciones**
   - SQLite con better-sqlite3
   - Handlers IPC para operaciones de BD
   - Integración con Ollama API
   - Sistema de exportación (preparado)

## 📋 Tareas Pendientes

### Alta Prioridad

1. **Seed Data de Ejercicios** (1-2 horas)
   - Migrar ejercicios desde la BD .NET
   - Crear script de importación
   - Poblar la base de datos inicial

   ```typescript
   // Archivo sugerido: electron/seed-exercises.ts
   const exercises = [
     {
       name: 'Bench Press',
       spanish_name: 'Press de Banca',
       primary_muscle_group: 'Pecho',
       equipment_type: 'Barra',
       difficulty_level: 'Medio',
       // ... más campos
     },
     // ... más ejercicios
   ]
   ```

2. **Lógica de Generación de Rutinas** (3-4 horas)
   - Implementar algoritmo de selección de ejercicios
   - Parser de respuestas de Ollama
   - Validación de rutinas generadas
   - Modo fallback sin IA

   ```typescript
   // Archivo sugerido: src/services/RoutineService.ts
   export class RoutineService {
     async generateRoutine(profile: UserProfile): Promise<WorkoutPlan> {
       // Implementación
     }
   }
   ```

3. **Exportación a Word** (2-3 horas)
   - Implementar usando librería `docx`
   - Formateo profesional
   - Incluir imágenes de ejercicios
   - Tablas con series/reps

   ```typescript
   // Archivo sugerido: src/services/ExportService.ts
   import { Document, Packer, Paragraph } from 'docx'

   export class ExportService {
     async exportToWord(plan: WorkoutPlan): Promise<Blob> {
       // Implementación
     }
   }
   ```

### Media Prioridad

4. **Gestión de Imágenes** (2-3 horas)
   - Migrar imágenes de ejercicios
   - Almacenar en BD como BLOB
   - Mostrar en ExerciseLibrary
   - Incluir en exportación

5. **Mejoras de UI** (2-3 horas)
   - Animaciones de transición
   - Loading states mejorados
   - Feedback visual de errores
   - Tooltips informativos

6. **Testing** (3-4 horas)
   - Unit tests con Vitest
   - Integration tests
   - E2E tests con Playwright

### Baja Prioridad

7. **Features Adicionales**
   - Historial de rutinas generadas
   - Favoritos de ejercicios
   - Modo oscuro/claro
   - Múltiples idiomas

8. **Optimizaciones**
   - Cache de ejercicios
   - Lazy loading de imágenes
   - Code splitting
   - Performance monitoring

## 🚀 Cómo Continuar

### Paso 1: Instalar Dependencias

```bash
cd electron-app
npm install
```

### Paso 2: Verificar Configuración

```bash
# Verificar TypeScript
npm run type-check

# Iniciar en desarrollo
npm run electron:dev
```

### Paso 3: Agregar Seed Data

Crear archivo `electron/seed-exercises.json` con los ejercicios de la aplicación .NET.

### Paso 4: Implementar Servicios Faltantes

Crear los siguientes servicios en `src/services/`:
- `RoutineService.ts` - Generación de rutinas
- `ExportService.ts` - Exportación a Word/PDF
- `ExerciseService.ts` - Gestión de ejercicios
- `ImageService.ts` - Gestión de imágenes

### Paso 5: Testing

```bash
# Instalar dependencias de testing
npm install -D vitest @testing-library/react @testing-library/jest-dom

# Ejecutar tests
npm run test
```

### Paso 6: Build de Producción

```bash
npm run electron:build
```

## 📁 Estructura de Archivos Sugerida

```
electron-app/
├── electron/
│   ├── main.ts              ✅ Completado
│   ├── preload.ts           ✅ Completado
│   ├── seed-exercises.ts    ❌ Pendiente
│   └── seed-exercises.json  ❌ Pendiente
├── src/
│   ├── components/
│   │   ├── HomePage.tsx            ✅ Completado
│   │   ├── RoutineGenerator.tsx    ✅ Completado (parcial)
│   │   ├── ExerciseLibrary.tsx     ✅ Completado (parcial)
│   │   └── Settings.tsx            ✅ Completado
│   ├── services/
│   │   ├── RoutineService.ts       ❌ Pendiente
│   │   ├── ExportService.ts        ❌ Pendiente
│   │   ├── ExerciseService.ts      ❌ Pendiente
│   │   └── ImageService.ts         ❌ Pendiente
│   ├── hooks/
│   │   ├── useExercises.ts         ❌ Pendiente
│   │   ├── useRoutineGenerator.ts  ❌ Pendiente
│   │   └── useOllama.ts            ❌ Pendiente
│   ├── utils/
│   │   ├── validation.ts           ❌ Pendiente
│   │   └── formatters.ts           ❌ Pendiente
│   └── models/
│       └── types.ts                ✅ Completado
└── tests/                          ❌ Pendiente
```

## 🔧 Comandos Útiles

```bash
# Desarrollo
npm run electron:dev          # Inicia app con hot reload

# Build
npm run build                 # Build solo del frontend
npm run electron:build        # Build completo con empaquetado

# Testing
npm run test                  # Ejecutar tests
npm run test:watch           # Tests en modo watch
npm run type-check           # Verificar tipos TypeScript

# Linting
npm run lint                 # Verificar código
npm run lint:fix            # Corregir problemas automáticamente
```

## 🎨 Guía de Estilos

### Componentes

- Usar componentes funcionales con hooks
- Props tipadas con TypeScript
- Exportar como default para componentes principales
- Componentes auxiliares al final del archivo

### Estilos

- Usar clases de TailwindCSS
- Custom classes en `styles/index.css`
- Mantener consistencia con el tema oscuro
- Usar variables CSS para colores

### Estado

- useState para estado local
- Context API para estado global (si es necesario)
- Evitar prop drilling
- Considerar Zustand para estado complejo

## 📝 Notas Importantes

1. **Base de Datos**: La BD se crea automáticamente en `userData/gymroutine.db`
2. **Ollama**: Debe estar corriendo en `http://localhost:11434`
3. **Imágenes**: Guardar en BD como BLOB o en carpeta de assets
4. **Tipos**: Mantener sincronizados con la BD
5. **IPC**: Siempre validar datos en el proceso principal

## 🐛 Problemas Conocidos

1. La exportación a Word está preparada pero no implementada
2. El parser de respuestas de Ollama necesita implementación
3. Las imágenes de ejercicios no están migrando
4. Falta validación de formularios

## 📚 Recursos

- [Electron Docs](https://www.electronjs.org/docs)
- [React Docs](https://react.dev)
- [TailwindCSS](https://tailwindcss.com)
- [Vite](https://vitejs.dev)
- [better-sqlite3](https://github.com/WiseLibs/better-sqlite3)
- [docx](https://docx.js.org)

## 🤝 Contribuir

1. Crear feature branch desde `claude/electron-ui-*`
2. Implementar cambios
3. Ejecutar tests y type-check
4. Commit con mensaje descriptivo
5. Push y crear PR

---

**Última actualización**: 2025-11-07
**Estado**: Base funcional completada, listo para desarrollo de features
