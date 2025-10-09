# CLAUDE.md

Este archivo proporciona guía completa para Claude Code cuando trabaja en este repositorio.

---

## 📋 Tabla de Contenidos
1. [Comandos de Desarrollo](#comandos-de-desarrollo)
2. [Arquitectura del Proyecto](#arquitectura-del-proyecto)
3. [Estructura de Carpetas](#estructura-de-carpetas)
4. [Estado Actual](#estado-actual)
5. [Documentación de Arquitectura](#documentación-de-arquitectura)
6. [Consideraciones Técnicas Clave](#consideraciones-técnicas-clave)
7. [Guías de Desarrollo](#guías-de-desarrollo)

---

## Comandos de Desarrollo

### Building
```bash
# Solución principal (src)
cd src && dotnet build GymRoutineGenerator.sln

# UI WinForms (principal)
cd UI && dotnet build GymRoutineUI.csproj

# Build Release
cd UI && dotnet build -c Release
```

### Running
```bash
# Aplicación WinForms (principal)
cd UI && dotnet run

# Con configuración Release
cd UI && dotnet run -c Release

# Ejecutable directo
UI/bin/x64/Release/net8.0-windows/win-x64/GeneradorRutinasGimnasio.exe
```

### Testing
```bash
# Todos los tests
cd tests && dotnet test

# Tests Clean Architecture (PRINCIPAL - 123 tests)
cd tests && dotnet test GymRoutineGenerator.Tests.CleanArchitecture/

# Tests específicos por epic (legacy)
cd src && dotnet test GymRoutineGenerator.Tests.Epic2
```

### Database
```bash
# Ubicación: gymroutine.db en directorio raíz
# Migraciones
cd src/GymRoutineGenerator.Data && dotnet ef migrations add [MigrationName]

# Actualizar BD
cd src/GymRoutineGenerator.Data && dotnet ef database update
```

---

## Arquitectura del Proyecto

### Visión General
Aplicación de escritorio .NET 8 que sigue principios de **Clean Architecture** con las siguientes capas:

```
┌─────────────────────────────────────────┐
│      Presentation Layer (UI)            │
│  - WinForms (principal en UI/)          │
│  - WinUI 3 (alternativo en src/)        │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      Application Layer                  │
│  - Use Cases (parcialmente en Business) │
│  - DTOs, Validators                     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         Domain Layer (Core)             │
│  - Entities, Value Objects              │
│  - Domain Services                      │
│  - Repository Interfaces                │
└─────────────────────────────────────────┘
                    ↑
┌─────────────────────────────────────────┐
│    Infrastructure Layer                 │
│  - Data (Repositories, EF Core)         │
│  - AI Services (Ollama)                 │
│  - Export Services (Word, PDF)          │
└─────────────────────────────────────────┘
```

### Tecnologías Principales
- **.NET 8** - Framework principal
- **WinForms** - UI principal (UI/)
- **WinUI 3** - UI alternativa (src/UI)
- **Entity Framework Core** - ORM
- **SQLite** - Base de datos
- **Ollama + Mistral 7B** - IA local
- **DocumentFormat.OpenXml** - Exportación Word
- **AutoMapper** - Mapeo de objetos
- **FluentValidation** - Validaciones (futuro)
- **MediatR** - CQRS pattern (futuro)

---

## Estructura de Carpetas

### Estructura Actual (Diciembre 2024)

```
gym-routine-generator/
├── src/                                    # Proyectos principales
│   ├── GymRoutineGenerator.Domain/         # ⭐ NEW: Clean Domain Layer
│   │   ├── Aggregates/                     # Agregados raíz
│   │   │   ├── Exercise.cs                 # Agregado de Ejercicio
│   │   │   ├── Routine.cs                  # Agregado de Rutina (1 día)
│   │   │   └── WorkoutPlan.cs              # Agregado de Plan de Entrenamiento
│   │   ├── ValueObjects/                   # Value Objects
│   │   │   ├── MuscleGroup.cs
│   │   │   ├── EquipmentType.cs
│   │   │   ├── ExerciseSet.cs
│   │   │   └── DifficultyLevel.cs
│   │   ├── Repositories/                   # Interfaces de repositorios
│   │   │   ├── IExerciseRepository.cs
│   │   │   ├── IWorkoutPlanRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   ├── Services/                       # Servicios de dominio
│   │   │   ├── IRoutineSafetyValidator.cs
│   │   │   └── IExerciseSelector.cs
│   │   ├── Events/                         # Eventos de dominio
│   │   │   ├── WorkoutPlanCreatedEvent.cs
│   │   │   └── ExerciseCreatedEvent.cs
│   │   └── Common/                         # Clases base
│   │       ├── Entity.cs
│   │       ├── ValueObject.cs
│   │       └── IDomainEvent.cs
│   │
│   ├── GymRoutineGenerator.Core/           # ⭐ Domain Layer (LEGACY - migrar)
│   │   ├── Models/                         # Entidades y Value Objects
│   │   │   ├── Exercise.cs
│   │   │   ├── WorkoutDay.cs
│   │   │   └── UserRoutineParameters.cs
│   │   ├── Enums/                          # Enumeraciones de dominio
│   │   └── Services/                       # Interfaces de servicios
│   │
│   ├── GymRoutineGenerator.Data/           # ⭐ Infrastructure - Persistence
│   │   ├── Context/                        # DbContext
│   │   │   └── GymRoutineContext.cs
│   │   ├── Entities/                       # EF Core Entities
│   │   │   ├── Exercise.cs
│   │   │   └── UserProfile.cs
│   │   ├── Repositories/                   # Implementaciones Repository
│   │   │   ├── ExerciseRepository.cs
│   │   │   └── IExerciseRepository.cs
│   │   ├── Management/                     # Servicios de gestión
│   │   ├── Import/                         # Importación de datos
│   │   └── Seeds/                          # Seed data
│   │
│   ├── GymRoutineGenerator.Infrastructure/ # ⭐ Infrastructure - Services
│   │   ├── AI/                             # Servicios de IA
│   │   │   ├── OllamaService.cs
│   │   │   ├── IntelligentRoutineService.cs
│   │   │   ├── PromptTemplateService.cs
│   │   │   ├── SpanishResponseProcessor.cs
│   │   │   ├── ConversationalRoutineService.cs
│   │   │   └── ConversationMemoryService.cs
│   │   ├── Documents/                      # Exportación documentos
│   │   │   ├── IntelligentExportService.cs
│   │   │   └── (otros servicios export)
│   │   ├── Images/                         # Gestión de imágenes
│   │   ├── Search/                         # Búsqueda de ejercicios
│   │   └── Services/                       # Servicios generales
│   │       ├── RoutineGenerationService.cs
│   │       └── DocumentExportService.cs
│   │
│   ├── GymRoutineGenerator.Application/    # ⭐ NEW: Application Layer (CQRS)
│   │   ├── Common/                         # Result pattern, ICommand, IQuery
│   │   │   ├── Result.cs
│   │   │   ├── ICommand.cs
│   │   │   └── IQuery.cs
│   │   ├── Commands/                       # CQRS Commands
│   │   │   └── WorkoutPlans/
│   │   │       ├── CreateWorkoutPlanCommand.cs
│   │   │       └── CreateWorkoutPlanCommandHandler.cs
│   │   ├── Queries/                        # CQRS Queries
│   │   │   ├── WorkoutPlans/
│   │   │   │   ├── GetWorkoutPlanByIdQuery.cs
│   │   │   │   └── GetWorkoutPlanByIdQueryHandler.cs
│   │   │   └── Exercises/
│   │   │       ├── GetAllExercisesQuery.cs
│   │   │       └── GetAllExercisesQueryHandler.cs
│   │   ├── DTOs/                           # Data Transfer Objects
│   │   │   ├── ExerciseDto.cs
│   │   │   ├── RoutineDto.cs
│   │   │   ├── WorkoutPlanDto.cs
│   │   │   └── ExerciseSetDto.cs
│   │   ├── Mappings/                       # AutoMapper Profiles
│   │   │   └── MappingProfile.cs
│   │   ├── Validators/                     # FluentValidation
│   │   │   └── CreateWorkoutPlanCommandValidator.cs
│   │   ├── Behaviors/                      # MediatR Pipeline Behaviors
│   │   │   └── ValidationBehavior.cs
│   │   └── DependencyInjection.cs          # DI Configuration
│   │
│   ├── GymRoutineGenerator.Business/       # Application Layer (LEGACY - deprecar)
│   │   └── Services/
│   │
│   └── GymRoutineGenerator.UI.csproj       # WinUI 3 (alternativo)
│
├── UI/                                     # ⭐ WinForms UI (PRINCIPAL)
│   ├── Controls/                           # Controles personalizados
│   │   ├── ModernButton.cs
│   │   ├── ModernCard.cs
│   │   └── ChatControl.cs
│   ├── Forms/                              # Formularios
│   │   ├── AboutForm.cs
│   │   ├── AddExerciseDialog.cs
│   │   ├── ExerciseImageManagerForm.cs
│   │   ├── HelpForm.cs
│   │   ├── MuscleGroupsEditorForm.cs
│   │   ├── RoutinePreviewForm.cs
│   │   └── SettingsForm.cs
│   ├── Helpers/                            # Clases auxiliares
│   │   ├── MuscleGroupCatalog.cs
│   │   └── ProgressIndicatorHelper.cs
│   ├── Services/                           # Servicios de UI
│   │   ├── AppServices.cs                  # DI Container
│   │   ├── EnhancedWordExport.cs
│   │   └── ImprovedExportService.cs
│   ├── Images/                             # Recursos de imágenes
│   │   └── Exercises/                      # Imágenes de ejercicios
│   ├── MainForm.cs                         # Formulario principal
│   ├── Program.cs                          # Punto de entrada
│   └── GymRoutineUI.csproj                 # Proyecto WinForms
│
├── scripts/                                # Scripts de build/deploy
│   ├── build-simple.ps1
│   ├── build-ui-dotnet.ps1
│   ├── crear_acceso_directo_mejorado.ps1
│   ├── make-portable.ps1
│   ├── publish.ps1
│   └── run-all-tests.ps1
│
├── docs/                                   # ⭐ Documentación
│   ├── ARQUITECTURA-MEJORAS-PROPUESTAS.md  # 📖 Guía de arquitectura
│   ├── ARQUITECTURA-EJEMPLOS-CODIGO.md     # 📖 Ejemplos de código
│   └── (otros documentos)
│
├── tests/                                  # ⭐ Proyectos de testing
│   ├── GymRoutineGenerator.Tests.CleanArchitecture/  # Tests principales (NEW)
│   │   ├── Domain/                         # Unit tests Domain Layer
│   │   │   ├── ExerciseTests.cs            # ✅ 4 tests
│   │   │   ├── RoutineTests.cs             # 📝 Pendiente
│   │   │   └── WorkoutPlanTests.cs         # 📝 Pendiente
│   │   ├── Application/                    # Unit tests Application Layer
│   │   │   └── CreateWorkoutPlanCommandHandlerTests.cs  # ✅ 4 tests
│   │   ├── Integration/                    # Integration tests
│   │   │   └── DomainExerciseRepositoryTests.cs  # ✅ 8 tests
│   │   └── Fixtures/                       # Test helpers
│   │       └── DatabaseFixture.cs          # DB en memoria
│   └── [otros proyectos legacy]/           # Proyectos de test antiguos
│
├── demo/                                   # Proyectos demo
├── gymroutine.db                           # Base de datos SQLite
└── CLAUDE.md                               # Este archivo
```

---

## Estado Actual

### ✅ Funcionalidades Implementadas
- **Generación de rutinas básicas** con selección de ejercicios
- **Gesti�n de ejercicios CQRS** (cat�logo + im�genes via MediatR)
- **Exportación a Word** con imágenes embebidas
- **Integración con IA local** (Ollama + Mistral 7B)
- **Chat conversacional** para modificar rutinas
- **Validación de seguridad** basada en limitaciones físicas
- **Gesti�n de im�genes** con base de datos + CQRS
- **Múltiples UIs** (WinForms principal, WinUI alternativo)

### 🆕 NUEVO: Clean Architecture Implementation (Diciembre 2024)

> **?? Estado Global**: 90% completado | **?? Tests**: 123/123 passing (~0.7s)
>
> **?? Documentaci�n completa**: Ver `docs/PROGRESO-CLEAN-ARCHITECTURE.md` para detalles exhaustivos

#### ✅ Fase 1: Domain Layer - COMPLETADO (100%)
- ✅ **Proyecto Domain creado** (`GymRoutineGenerator.Domain`)
- ✅ **Aggregates implementados**: Exercise, Routine, WorkoutPlan
- ✅ **Value Objects biling�es**: MuscleGroup, EquipmentType (espa�ol/ingl�s)
- ✅ **Repository Interfaces**: IExerciseRepository, IWorkoutPlanRepository, IUnitOfWork
- ✅ **Domain Services**: IRoutineSafetyValidator, IExerciseSelector
- ✅ **Domain Events**: WorkoutPlanCreatedEvent, ExerciseCreatedEvent
- ✅ **Base Classes**: Entity, ValueObject, IDomainEvent
- �"� **Estado**: Compilando sin errores | **Tests**: 45/45 passing

#### ✅ Fase 2: Application Layer - COMPLETADO (100%)
- ✅ **Proyecto Application creado** (`GymRoutineGenerator.Application`)
- ✅ **CQRS completo** con MediatR
  - Commands: CreateExerciseCommand, UpdateExerciseCommand, DeleteExerciseCommand, CreateWorkoutPlanCommand
  - Queries: GetAllExercisesQuery, GetExerciseCatalogQuery, GetExerciseByIdQuery, GetWorkoutPlanByIdQuery
  - Handlers para cada Command/Query
- ✅ **DTOs**: ExerciseDto, RoutineDto, WorkoutPlanDto, ExerciseSetDto
- ✅ **AutoMapper 12.x**: MappingProfile para mapeo Domain �' DTOs
- ✅ **FluentValidation**: Validators con pipeline behavior automático
- ✅ **Result Pattern**: Manejo de errores funcional
- ✅ **DependencyInjection**: Configuración con extension methods
- �"� **Estado**: Compilando sin errores | **Tests**: 52/52 passing

#### ✅ Fase 3: Infrastructure Layer - COMPLETADO (95%)
- ✅ **Persistencia WorkoutPlan** en SQLite con EF Core
- ✅ **Entidades EF**: WorkoutPlan, WorkoutPlanRoutine, WorkoutPlanRoutineExercise
- ✅ **Migración**: `20251002183929_AddWorkoutPlanPersistence`
- ✅ **Repositories** delegando a Data layer (eliminada duplicación in-memory)
- ✅ **UnitOfWork** con transacciones coordinadas
- ✅ **Mapeo mejorado** categorías musculares y validación espa�ol
- � � **Pendiente**: Limpieza warnings (~25 nullability/async)
- �"� **Estado**: Compilando sin errores | **Tests**: 26/26 passing

#### �"� Fase 4: UI Integration - EN PROGRESO (75%)
- ✅ **DI configurado** en AppServices.cs
- ✅ **2 formularios migrados a CQRS**:
  - `ExerciseExplorerForm`: Query-based con MediatR
  - `ExerciseCatalogManagerForm`: CRUD completo via Commands/Queries
- � � **Pendiente**: Migrar MainForm, RoutinePreviewForm, AddExerciseDialog
- �"� **Estado**: Funcional | **Tests UI**: Pendiente

#### ✅ Fase 5: Testing - COMPLETADO (100%)
- ✅ **123 tests automatizados** (100% passing, ~0.7s)
- ✅ **Cobertura**: Domain (45), Application (52), Integration (26)
- ✅ **Base de datos en memoria** para tests de integración
- ✅ **XUnit + FluentAssertions + Moq**
- �"� **Estado**: Suite completa ejecutándose

#### �� Fase 6: Production - NO INICIADA (0%)
- ⬜ Pipeline CI/CD (GitHub Actions)
- ⬜ Coverage reporting (Coverlet + ReportGenerator)
- ⬜ Empaquetado Release (MSI/ClickOnce)
- ⬜ Documentación usuario final

### ⚠️ Problemas Conocidos (Resueltos)
- ✅ **Ambigüedad UserProfile**: Resuelto usando aliases en namespaces
- ✅ **IExerciseRepository no registrado**: Resuelto en AppServices.cs
- ✅ **Archivos .old duplicados**: Eliminados durante reorganización
- ✅ **Scripts dispersos**: Consolidados en carpeta scripts/

### �"� Estado de Compilación
- **Build Status**: ✅ Exitoso (0 errores)
- **Warnings**: ~25 (reducidos desde ~150, principalmente nullability)
- **Target Framework**: .NET 8.0
- **Platforms**: Windows x64
- **Clean Architecture**: ✅ 90% completado
- **Tests**: ✅ 123/123 passing (100%, ~0.7s)

### 🚀 Ejecutable
- **Ubicación**: `UI/bin/x64/Release/net8.0-windows/win-x64/GeneradorRutinasGimnasio.exe`
- **Acceso Directo**: `C:\Users\vicen\OneDrive\Escritorio\Rutina Gym.lnk`
- **Estado**: ✅ Funcional

---

## Documentación de Arquitectura

### 📚 Documentos de Referencia Obligatorios

#### 1. ARQUITECTURA-MEJORAS-PROPUESTAS.md
**📍 Ubicación**: `docs/ARQUITECTURA-MEJORAS-PROPUESTAS.md`

**Contenido**:
- Análisis detallado del estado actual
- 8 problemas identificados (críticos y moderados)
- Propuestas de mejora siguiendo Clean Architecture
- Aplicación de principios SOLID (SRP, OCP, LSP, ISP, DIP)
- Aplicación de patrones GRASP (Information Expert, Creator, Controller, etc.)
- Patrones adicionales (CQRS, Repository, Unit of Work, Specification, Builder, Strategy)
- Estructura propuesta de proyectos
- Plan de implementación en 4 fases (4-6 semanas)

**⚠️ IMPORTANTE**:
- **Leer este documento ANTES de hacer refactorings grandes**
- Seguir el plan de implementación propuesto
- No romper compatibilidad con código existente durante transición

#### 2. ARQUITECTURA-EJEMPLOS-CODIGO.md
**📍 Ubicación**: `docs/ARQUITECTURA-EJEMPLOS-CODIGO.md`

**Contenido**:
- Ejemplos concretos de código para cada capa
- Domain Layer: Aggregates, Entities, Value Objects, Domain Services
- Application Layer: Commands/Queries (CQRS), Handlers, DTOs, Mappers
- Infrastructure Layer: Repositories, Unit of Work, EF Core Configurations
- Presentation Layer: UI con DI, MediatR
- Caso de uso completo end-to-end

**⚠️ IMPORTANTE**:
- **Usar estos ejemplos como referencia** al implementar nuevas features
- Copiar y adaptar patrones mostrados
- Mantener consistencia con los ejemplos

### 🎯 Cuándo Consultar la Documentación

| Situación | Documento a Consultar |
|-----------|----------------------|
| Agregar nueva feature | Ambos documentos |
| Refactorizar código existente | ARQUITECTURA-MEJORAS-PROPUESTAS.md |
| Implementar nuevo Use Case | ARQUITECTURA-EJEMPLOS-CODIGO.md |
| Agregar nueva entidad de dominio | ARQUITECTURA-EJEMPLOS-CODIGO.md (Domain Layer) |
| Agregar nuevo servicio | ARQUITECTURA-MEJORAS-PROPUESTAS.md (Sección 3.4) |
| Problemas de arquitectura | ARQUITECTURA-MEJORAS-PROPUESTAS.md (Sección 2) |
| Duda sobre patrones | Ambos documentos |
| Escribir tests | TESTING-STRATEGY.md |
| Ver plan de testing | FASE-5-TESTING-COMPLETO.md |

---

## Consideraciones Técnicas Clave

### 🔧 Dependencias y Servicios

#### Inyección de Dependencias
- **Container**: Microsoft.Extensions.DependencyInjection
- **Configuración**: `UI/Services/AppServices.cs`
- **Patrón**: Service Locator (actual) → DI puro (futuro)

```csharp
// Ejemplo actual
var service = AppServices.Get<IRoutineGenerationService>();

// Futuro (con constructor injection)
public MainForm(IRoutineGenerationService routineService)
{
    _routineService = routineService;
}
```

#### Servicios Registrados
```csharp
// Repositories
services.AddScoped<IExerciseRepository, ExerciseRepository>();

// Domain Services
services.AddScoped<IRoutineSafetyValidator, RoutineSafetyValidator>();

// Application Services
services.AddScoped<IRoutineGenerationService, RoutineGenerationService>();
services.AddScoped<IDocumentExportService, DocumentExportService>();

// Infrastructure Services
services.AddSingleton<IOllamaService, OllamaService>();
services.AddScoped<IPromptTemplateService, PromptTemplateService>();
services.AddScoped<ISpanishResponseProcessor, SpanishResponseProcessor>();
```

### 🗄️ Base de Datos

#### Esquema Principal
```sql
-- Exercises
CREATE TABLE Exercises (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    SpanishName TEXT NOT NULL,
    Description TEXT,
    Instructions TEXT,
    PrimaryMuscleGroupId INTEGER,
    SecondaryMuscleGroupId INTEGER,
    EquipmentTypeId INTEGER,
    DifficultyLevel INTEGER,
    ExerciseType INTEGER,
    IsActive BOOLEAN
);

-- UserProfiles
CREATE TABLE UserProfiles (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Age INTEGER,
    Gender TEXT,
    FitnessLevel TEXT,
    TrainingDays INTEGER,
    Goals TEXT
);

-- ExerciseImages
CREATE TABLE ExerciseImages (
    Id INTEGER PRIMARY KEY,
    ExerciseId INTEGER,
    ImagePath TEXT,
    ImageData BLOB,
    IsPrimary BOOLEAN
);
```

#### Migraciones
- **Estado**: Migrations disponibles en `src/GymRoutineGenerator.Data/Migrations/`
- **Aplicación**: Auto-aplicadas en `AppServices.Configure()`
- **Seed Data**: Automático si tablas vacías

### 🤖 Sistema de IA

#### Configuración Ollama
```bash
# Verificar disponibilidad
curl http://localhost:11434/api/tags

# Modelo requerido: mistral:latest
ollama pull mistral
```

#### Servicios de IA
1. **OllamaService**: Comunicación con Ollama
2. **IntelligentRoutineService**: Generación de rutinas con IA
3. **PromptTemplateService**: Templates para prompts
4. **SpanishResponseProcessor**: Procesa respuestas en español
5. **ConversationalRoutineService**: Chat conversacional
6. **FallbackRoutineService**: Fallback sin IA

#### Modo Offline
- Si Ollama no disponible → usa FallbackRoutineService
- Genera rutinas basadas en reglas simples
- No requiere conexión

### 📄 Exportación de Documentos

#### Formatos Soportados
- ✅ **Word (.docx)** - Implementado
- ⚠️ **PDF** - Parcial


#### Servicios de Exportación
- `DocumentExportService`: Servicio principal
- `IntelligentExportService`: Exportación con IA
- `EnhancedWordExport`: Exportación Word avanzada

---

## Guías de Desarrollo

### 🆕 Agregar Nueva Feature

#### 1. Planificación
- [ ] Revisar `ARQUITECTURA-MEJORAS-PROPUESTAS.md`
- [ ] Identificar capa apropiada (Domain, Application, Infrastructure, UI)
- [ ] Verificar que sigue principios SOLID/GRASP

#### 2. Implementación Recomendada

**Para Use Cases (Application Layer)**:
```csharp
// 1. Crear Command/Query
public record CreateRoutineCommand : IRequest<Result<RoutineDto>>
{
    public string UserId { get; init; }
    public string Name { get; init; }
    // ... otras propiedades
}

// 2. Crear Validator
public class CreateRoutineValidator : AbstractValidator<CreateRoutineCommand>
{
    // ... reglas de validación
}

// 3. Crear Handler
public class CreateRoutineHandler : IRequestHandler<CreateRoutineCommand, Result<RoutineDto>>
{
    // ... implementación
}
```

**Para Entidades de Dominio**:
```csharp
// Seguir patrón de ARQUITECTURA-EJEMPLOS-CODIGO.md
public class Routine : Entity<RoutineId>
{
    // Constructor privado
    private Routine() { }

    // Factory method
    public static Routine Create(...) { }

    // Métodos de negocio
    public void AddExercise(...) { }
}
```

#### 3. Testing
```bash
# Crear tests unitarios
cd tests
dotnet new xunit -n GymRoutineGenerator.Tests.[Feature]

# Ejecutar tests
dotnet test
```

### 🔄 Refactoring Seguro

#### Reglas de Oro
1. **No romper la UI existente** - Usuarios dependen de ella
2. **Mantener compatibilidad backward** durante transición
3. **Usar Adapter pattern** para código legacy
4. **Tests antes de refactorizar**
5. **Commits pequeños y frecuentes**

#### Proceso Recomendado
```
1. Identificar código a refactorizar
2. Escribir tests para comportamiento actual
3. Crear nueva implementación siguiendo arquitectura propuesta
4. Crear adapter entre código viejo y nuevo
5. Migrar gradualmente
6. Eliminar código viejo cuando 100% migrado
```

### 🐛 Debugging

#### Logs
```csharp
// Los servicios usan ILogger
private readonly ILogger<MyService> _logger;

_logger.LogInformation("Generating routine for user {UserId}", userId);
_logger.LogWarning("AI service unavailable, using fallback");
_logger.LogError(ex, "Error generating routine");
```

#### Puntos de Debug Comunes
- `UI/MainForm.cs` - Eventos de UI
- `UI/Services/AppServices.cs` - Configuración DI
- `Infrastructure/AI/IntelligentRoutineService.cs` - Generación rutinas
- `Infrastructure/Services/RoutineGenerationService.cs` - Servicio principal
- `Data/Repositories/ExerciseRepository.cs` - Acceso a BD

### 📝 Convenciones de Código

#### Naming
- **Clases**: PascalCase - `ExerciseRepository`
- **Métodos**: PascalCase - `GetExerciseAsync`
- **Variables**: camelCase - `exerciseList`
- **Privados**: _camelCase - `_exerciseRepository`
- **Constantes**: UPPER_SNAKE_CASE - `MAX_EXERCISES_PER_ROUTINE`

#### Async/Await
```csharp
// ✅ Correcto
public async Task<Exercise> GetExerciseAsync(int id, CancellationToken ct = default)
{
    return await _context.Exercises.FindAsync(id, ct);
}

// ❌ Incorrecto
public async Task<Exercise> GetExercise(int id)
{
    return _context.Exercises.Find(id); // No async
}
```

#### Namespaces
```csharp
// Domain
namespace GymRoutineGenerator.Domain.Aggregates.WorkoutPlan;

// Application
namespace GymRoutineGenerator.Application.UseCases.Routines.Commands;

// Infrastructure
namespace GymRoutineGenerator.Infrastructure.Persistence.Repositories;

// UI
namespace GymRoutineGenerator.UI.WinForms.Features.Routines;
```

---

## 🚦 Reglas Importantes

### ❗ DEBE HACER
1. ✅ Leer `ARQUITECTURA-MEJORAS-PROPUESTAS.md` antes de refactorings grandes
2. ✅ Seguir ejemplos de `ARQUITECTURA-EJEMPLOS-CODIGO.md` para código nuevo
3. ✅ Usar Result<T> para manejo de errores (ver ejemplos)
4. ✅ Validar con FluentValidation en Application Layer
5. ✅ Inyectar dependencias via constructor
6. ✅ Hacer commits pequeños y descriptivos
7. ✅ Escribir tests para lógica de negocio crítica
8. ✅ Documentar decisiones de arquitectura importantes

### ❌ NO DEBE HACER
1. ❌ No mezclar lógica de negocio en UI
2. ❌ No crear dependencias de Infrastructure → Domain
3. ❌ No usar `new` para servicios (usar DI)
4. ❌ No ignorar excepciones sin loggear
5. ❌ No hacer refactorings masivos sin plan
6. ❌ No usar Entity Framework entities en UI (usar DTOs)
7. ❌ No hardcodear strings (usar constantes/resources)
8. ❌ No commitear código que no compila

---

## �"� Roadmap Futuro

> **?? Progreso Global**: 90% | **?? Milestone Actual**: Fase 4 (UI Integration) - 75%
>
> **?? Roadmap detallado**: Ver `docs/PROGRESO-CLEAN-ARCHITECTURE.md` secci�n "Roadmap de Implementaci�n"

### ✅ Fase 1: Domain Layer - COMPLETADO (100%)
- [x] Proyecto Domain limpio con Aggregates, Value Objects, Repository Interfaces
- [x] Domain Services (RoutineSafetyValidator, ExerciseSelector)
- [x] Domain Events (WorkoutPlanCreatedEvent, ExerciseCreatedEvent)
- [x] Base Classes (Entity, ValueObject, IDomainEvent)
- [x] **45 tests** automatizados (100% passing)

### ✅ Fase 2: Application Layer - COMPLETADO (100%)
- [x] CQRS completo con MediatR (8+ Commands/Queries)
- [x] DTOs, AutoMapper profiles, FluentValidation
- [x] Result Pattern, Pipeline Behaviors, DependencyInjection
- [x] **52 tests** automatizados (100% passing)

### ✅ Fase 3: Infrastructure Layer - COMPLETADO (95%)
- [x] IUnitOfWork con EF Core implementado
- [x] IExerciseRepository con Domain entities
- [x] IWorkoutPlanRepository con persistencia SQLite
- [x] Migración EF Core para WorkoutPlan
- [x] **26 tests** de integración (100% passing)
- [ ] Limpieza de warnings (~25 restantes)

### �"� Fase 4: UI Integration - EN PROGRESO (75%)
- [x] MediatR integrado en UI
- [x] 2 formularios migrados (ExerciseExplorerForm, ExerciseCatalogManagerForm)
- [ ] **Próximo**: Migrar MainForm a CQRS
- [ ] Migrar RoutinePreviewForm, AddExerciseDialog, SettingsForm
- [ ] Tests de UI (formularios CQRS)

### ✅ Fase 5: Testing - COMPLETADO (100%)
- [x] **123 tests automatizados** (Domain + Application + Integration)
- [x] Suite ejecuta en ~0.7s
- [x] Base de datos en memoria
- [x] XUnit + FluentAssertions + Moq

### �� Fase 6: Production Ready - NO INICIADA (0%)
- [ ] Pipeline CI/CD (GitHub Actions o Azure DevOps)
- [ ] Coverage reporting (Coverlet + ReportGenerator)
- [ ] Empaquetado Release (MSI o ClickOnce)
- [ ] Documentación usuario final
- [ ] Guía de despliegue

### 🚀 Features Futuras (Post-Production)
- [ ] Exportación PDF nativa
- [ ] Sistema de plantillas de rutinas
- [ ] Performance profiling y optimización
- [ ] Cache inteligente
- [ ] Logging estructurado avanzado

---

## 📚 Recursos de Aprendizaje

### Clean Architecture
- **Libro**: "Clean Architecture" - Robert C. Martin
- **Referencia**: `docs/ARQUITECTURA-MEJORAS-PROPUESTAS.md`

### SOLID Principles
- **Aplicación en proyecto**: Ver sección 3.2 de ARQUITECTURA-MEJORAS-PROPUESTAS.md

### CQRS Pattern
- **Ejemplos**: `docs/ARQUITECTURA-EJEMPLOS-CODIGO.md` - Sección 2

### Domain-Driven Design
- **Libro**: "Domain-Driven Design" - Eric Evans
- **Aplicación**: Ver Domain Layer en ARQUITECTURA-EJEMPLOS-CODIGO.md

---

## 🆘 Troubleshooting

### Build Fails
```bash
# Limpiar y rebuild
cd UI
dotnet clean
dotnet build
```

### Database Issues
```bash
# Eliminar BD y recrear
rm gymroutine.db
# Ejecutar app - auto-crea BD
cd UI && dotnet run
```

### Ollama No Responde
```bash
# Verificar servicio
curl http://localhost:11434/api/tags

# Reiniciar Ollama
# (depende de cómo está instalado)
```

### DI Errors
- Verificar que servicio está registrado en `AppServices.cs`
- Verificar que interfaz y clase están en namespaces correctos
- Verificar lifetime (Singleton, Scoped, Transient)

---

## 📞 Contacto y Contribución

### Para Futuros Desarrolladores
1. Lee este archivo completo primero
2. Lee `ARQUITECTURA-MEJORAS-PROPUESTAS.md`
3. Revisa ejemplos en `ARQUITECTURA-EJEMPLOS-CODIGO.md`
4. Pregunta antes de hacer cambios grandes
5. Documenta tus decisiones

### Mantener Este Documento
- Actualizar cuando cambie estructura del proyecto
- Agregar nuevas secciones para features importantes
- Mantener sincronizado con documentos de arquitectura
- Versionar junto con el código

---

## ?? Historial de Cambios

### Versi�n 2.1 - Octubre 2025
- ? Actualizado estado Clean Architecture (90% completado)
- ? Sincronizado informaci�n de testing (123 tests)
- ? Actualizadas todas las fases con m�tricas reales
- ? Consolidada documentaci�n en PROGRESO-CLEAN-ARCHITECTURE.md
- ? Reducidos warnings de ~150 a ~25

### Versi�n 2.0 - Diciembre 2024
- ? Reestructuraci�n completa de arquitectura
- ? Documentaci�n exhaustiva de Clean Architecture

---

**�ltima actualizaci�n**: 2025-10-02
**Versi�n**: 2.1 - Clean Architecture al 90%

