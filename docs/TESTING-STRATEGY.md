# Testing Strategy - Gym Routine Generator

## 📋 Tabla de Contenidos
1. [Visión General](#visión-general)
2. [Tipos de Tests](#tipos-de-tests)
3. [Estructura de Tests](#estructura-de-tests)
4. [Cobertura de Tests](#cobertura-de-tests)
5. [Convenciones y Mejores Prácticas](#convenciones-y-mejores-prácticas)
6. [Ejecución de Tests](#ejecución-de-tests)
7. [Tests Existentes](#tests-existentes)
8. [Roadmap de Testing](#roadmap-de-testing)

---

## Visión General

La estrategia de testing del proyecto sigue el principio de la **pirámide de tests**:

```
           /\
          /  \  E2E Tests (Pocos)
         /    \
        /------\
       /        \ Integration Tests (Moderados)
      /          \
     /------------\
    /              \ Unit Tests (Muchos)
   /________________\
```

### Objetivos
- ✅ Garantizar la calidad del código
- ✅ Facilitar refactorings seguros
- ✅ Documentar el comportamiento esperado
- ✅ Detectar regresiones tempranamente
- ✅ Permitir desarrollo incremental con confianza

### Framework de Testing
- **Test Runner**: xUnit
- **Mocking**: Moq
- **Assertions**: FluentAssertions
- **In-Memory Database**: Microsoft.EntityFrameworkCore.InMemory

---

## Tipos de Tests

### 1. Unit Tests
**Propósito**: Probar unidades individuales de código en aislamiento.

**Características**:
- Rápidos de ejecutar (< 100ms cada uno)
- No dependen de I/O (DB, archivos, red)
- Usan mocks para dependencias externas
- Siguen el patrón AAA (Arrange, Act, Assert)

**Qué se testea**:
- Lógica de dominio (agregados, value objects, domain services)
- Handlers de comandos/queries (con mocks)
- Validadores
- Mappers

**Ejemplo**:
```csharp
[Fact]
public void Exercise_Create_ShouldCreateExerciseWithValidData()
{
    // Arrange
    var name = "Press de Banca";
    var primaryMuscle = MuscleGroup.Pectorales;

    // Act
    var exercise = Exercise.Create(name, primaryMuscle, ...);

    // Assert
    exercise.Should().NotBeNull();
    exercise.Name.Should().Be(name);
}
```

### 2. Integration Tests
**Propósito**: Probar la integración entre componentes reales.

**Características**:
- Más lentos que unit tests (< 1s cada uno)
- Usan base de datos en memoria
- Prueban repositorios, DbContext, queries SQL
- No usan mocks para componentes bajo test

**Qué se testea**:
- Repositorios con EF Core
- Queries complejas
- Transacciones y Unit of Work
- Mapeo ORM

**Ejemplo**:
```csharp
[Fact]
public async Task GetAllAsync_ShouldReturnAllExercises()
{
    // Arrange
    var context = CreateInMemoryContext();
    var repository = new ExerciseRepository(context);

    // Act
    var exercises = await repository.GetAllAsync();

    // Assert
    exercises.Should().HaveCountGreaterThan(0);
}
```

### 3. End-to-End Tests (Futuro)
**Propósito**: Probar el sistema completo desde la perspectiva del usuario.

**Características**:
- Lentos (> 5s cada uno)
- Usan base de datos real
- Simulan interacciones de usuario
- Prueban el flujo completo

**Qué se testearía**:
- Flujos completos de generación de rutinas
- Exportación de documentos
- Integración con IA (Ollama)
- UI workflows (WinForms/WinUI)

---

## Estructura de Tests

### Proyecto de Tests: GymRoutineGenerator.Tests.CleanArchitecture

```
tests/
└── GymRoutineGenerator.Tests.CleanArchitecture/
    ├── Domain/                          # Unit tests para Domain layer
    │   ├── ExerciseTests.cs             # Tests de agregado Exercise
    │   ├── RoutineTests.cs              # Tests de agregado Routine
    │   ├── WorkoutPlanTests.cs          # Tests de agregado WorkoutPlan
    │   └── ValueObjects/
    │       ├── MuscleGroupTests.cs
    │       └── ExerciseSetTests.cs
    │
    ├── Application/                     # Unit tests para Application layer
    │   ├── Commands/
    │   │   └── CreateWorkoutPlanCommandHandlerTests.cs
    │   ├── Queries/
    │   │   └── GetWorkoutPlanByIdQueryHandlerTests.cs
    │   └── Validators/
    │       └── CreateWorkoutPlanCommandValidatorTests.cs
    │
    ├── Integration/                     # Integration tests
    │   ├── DomainExerciseRepositoryTests.cs
    │   ├── WorkoutPlanRepositoryTests.cs
    │   └── UnitOfWorkTests.cs
    │
    ├── Fixtures/                        # Test fixtures y helpers
    │   ├── DatabaseFixture.cs           # Setup para DB en memoria
    │   └── TestDataBuilder.cs           # Builder para datos de test
    │
    └── GymRoutineGenerator.Tests.CleanArchitecture.csproj
```

### Convención de Nombres
```
[ClassName]_[MethodName]_[ExpectedBehavior]

Ejemplos:
- Exercise_Create_ShouldCreateExerciseWithValidData
- CreateWorkoutPlanCommandHandler_Handle_ValidCommand_ShouldCreateWorkoutPlan
- ExerciseRepository_GetByIdAsync_NonExistingId_ShouldReturnNull
```

---

## Cobertura de Tests

### Estado Actual (Fase 4 - Diciembre 2024)

| Layer | Tests Unitarios | Tests Integración | Cobertura Estimada |
|-------|----------------|-------------------|-------------------|
| **Domain** | ✅ 4 tests | - | ~30% |
| **Application** | ✅ 4 tests | - | ~40% |
| **Infrastructure** | - | ✅ 8 tests | ~20% |
| **Total** | **8 tests** | **8 tests** | **~30%** |

### Tests Actuales

#### Domain Layer (4 tests) ✅
```csharp
// ExerciseTests.cs
✅ Exercise_Create_ShouldCreateExerciseWithValidData
✅ Exercise_AddTargetMuscle_ShouldAddMuscleToList
✅ Exercise_IsAppropriateForLevel_ShouldReturnTrueForSameOrHigherLevel
✅ Exercise_IsAppropriateForLevel_ShouldReturnFalseForLowerLevel
```

#### Application Layer (4 tests) ✅
```csharp
// CreateWorkoutPlanCommandHandlerTests.cs
✅ Handle_ValidCommand_ShouldCreateWorkoutPlan
✅ Handle_InvalidName_ShouldFail (Theory: null, "")
✅ Handle_SaveChangesFails_ShouldReturnFailure
```

#### Integration Layer (8 tests) ✅
```csharp
// DomainExerciseRepositoryTests.cs
✅ GetAllAsync_ShouldReturnAllExercises
✅ GetByIdAsync_ExistingId_ShouldReturnExercise
✅ GetByIdAsync_NonExistingId_ShouldReturnNull
✅ GetActiveExercisesAsync_ShouldReturnOnlyActiveExercises
✅ GetByMuscleGroupAsync_ShouldReturnExercisesForMuscleGroup
✅ GetByEquipmentAsync_ShouldReturnExercisesWithEquipment
✅ GetByDifficultyAsync_ShouldReturnExercisesWithDifficulty
✅ AddAsync_ShouldAddExerciseToDatabase
```

### Objetivos de Cobertura (Fase 5)

| Layer | Objetivo | Tests Faltantes |
|-------|----------|----------------|
| **Domain** | 80% | Routine, WorkoutPlan, Value Objects |
| **Application** | 75% | Queries, más Commands, Validators |
| **Infrastructure** | 60% | Más repositories, AI Services |
| **Total** | **70%** | ~50 tests adicionales |

---

## Convenciones y Mejores Prácticas

### 1. Patrón AAA (Arrange-Act-Assert)
```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateWorkoutPlan()
{
    // Arrange - Configurar el escenario
    var command = new CreateWorkoutPlanCommand(...);
    var mockRepo = new Mock<IWorkoutPlanRepository>();

    // Act - Ejecutar la acción
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert - Verificar el resultado
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
}
```

### 2. Test Naming
```csharp
// ✅ BUENO: Nombre descriptivo
[Fact]
public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()

// ❌ MALO: Nombre genérico
[Fact]
public async Task Test1()
```

### 3. FluentAssertions
```csharp
// ✅ BUENO: Assertions legibles
result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBeNull();
result.Value.Name.Should().Be("Rutina Principiante");

// ❌ MALO: Assertions básicas
Assert.True(result.IsSuccess);
Assert.NotNull(result.Value);
Assert.Equal("Rutina Principiante", result.Value.Name);
```

### 4. Teorías para Casos Múltiples
```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
public async Task Handle_InvalidName_ShouldFail(string? invalidName)
{
    // ...
}
```

### 5. Mocks con Moq
```csharp
// Setup de mock
_unitOfWorkMock
    .Setup(u => u.WorkoutPlans)
    .Returns(workoutPlanRepositoryMock.Object);

// Verificación
workoutPlanRepositoryMock.Verify(
    r => r.AddAsync(It.IsAny<WorkoutPlan>(), It.IsAny<CancellationToken>()),
    Times.Once
);
```

### 6. Database Fixture para Integration Tests
```csharp
public class DomainExerciseRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DomainExerciseRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllExercises()
    {
        using var context = _fixture.CreateContext();
        var repository = new ExerciseRepository(context);
        // ...
    }
}
```

### 7. Test Isolation
- Cada test debe ser independiente
- No compartir estado entre tests
- Usar base de datos en memoria con nombres únicos
- Cleanup después de cada test si es necesario

---

## Ejecución de Tests

### Comandos Principales

```bash
# Ejecutar TODOS los tests del proyecto
dotnet test

# Ejecutar solo tests de Clean Architecture
dotnet test tests/GymRoutineGenerator.Tests.CleanArchitecture/

# Ejecutar con output detallado
dotnet test --verbosity detailed

# Ejecutar tests específicos por nombre
dotnet test --filter "FullyQualifiedName~ExerciseTests"

# Ejecutar con cobertura de código (requiere coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Script PowerShell para Ejecutar Todos los Tests
```powershell
# scripts/run-all-tests.ps1
Write-Host "Ejecutando tests de Clean Architecture..." -ForegroundColor Cyan
dotnet test tests/GymRoutineGenerator.Tests.CleanArchitecture/ --verbosity normal

# Agregar más proyectos de test aquí...
```

### CI/CD Integration (Futuro)
```yaml
# .github/workflows/tests.yml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 8.0.x
      - name: Run tests
        run: dotnet test --verbosity normal
```

---

## Tests Existentes

### Proyectos de Test Legacy (a migrar o deprecar)

```
tests/
├── GymRoutineGenerator.Tests/              # Tests generales
├── GymRoutineGenerator.Tests.Complete/     # Tests completos
├── GymRoutineGenerator.Tests.Console/      # Tests de consola
├── GymRoutineGenerator.Tests.Epic2/        # Tests de Epic 2
├── GymRoutineGenerator.Tests.Export/       # Tests de exportación
├── GymRoutineGenerator.Tests.Images/       # Tests de imágenes
├── GymRoutineGenerator.Tests.ImportSystem/ # Tests de importación
├── GymRoutineGenerator.Tests.Management/   # Tests de gestión
├── GymRoutineGenerator.Tests.Ollama/       # Tests de IA
├── GymRoutineGenerator.Tests.Search/       # Tests de búsqueda
├── GymRoutineGenerator.Tests.UI.Tests/     # Tests de UI
└── GymRoutineGenerator.Tests.CleanArchitecture/ # ⭐ NUEVO: Tests principales
```

**Acción recomendada**: Consolidar tests útiles de proyectos legacy en `GymRoutineGenerator.Tests.CleanArchitecture`.

---

## Roadmap de Testing

### ✅ Fase 4: Fundamentos (Completada - Diciembre 2024)
- [x] Crear proyecto de tests `GymRoutineGenerator.Tests.CleanArchitecture`
- [x] Configurar xUnit + Moq + FluentAssertions
- [x] Tests unitarios para Domain Layer (Exercise)
- [x] Tests unitarios para Application Layer (CreateWorkoutPlanCommand)
- [x] Tests de integración para Repositories
- [x] Database Fixture para tests de integración
- [x] Documentar estrategia de testing

**Resultado**: 16 tests, 100% passing ✅

### 🔄 Fase 5: Expansión (Próximos pasos)
- [ ] Agregar tests para Routine agregado
- [ ] Agregar tests para WorkoutPlan agregado
- [ ] Agregar tests para Value Objects
- [ ] Agregar tests para Queries (GetWorkoutPlanById, GetAllExercises)
- [ ] Agregar tests para Validators
- [ ] Agregar tests para AutoMapper profiles
- [ ] Aumentar cobertura de repositories

**Objetivo**: ~40-50 tests, 60% cobertura

### 📅 Fase 6: Integration (Futuro)
- [ ] Tests de integración para AI Services (Ollama)
- [ ] Tests de integración para Document Export
- [ ] Tests de integración para Image Management
- [ ] Tests de integración completos end-to-end

**Objetivo**: ~70 tests, 70% cobertura

### 🚀 Fase 7: Automatización (Futuro)
- [ ] Configurar CI/CD con GitHub Actions
- [ ] Integrar Code Coverage reporting
- [ ] Tests de performance/benchmarks
- [ ] Tests de UI automatizados (WinForms/WinUI)

**Objetivo**: Pipeline automatizado, 80% cobertura

---

## Métricas de Calidad

### Objetivos

| Métrica | Objetivo | Actual |
|---------|----------|--------|
| **Unit Test Coverage** | 80% | ~35% |
| **Integration Test Coverage** | 60% | ~25% |
| **Test Pass Rate** | 100% | ✅ 100% |
| **Test Execution Time** | < 10s | ✅ ~3s |
| **Failed Tests on Build** | 0 | ✅ 0 |

### Indicadores de Éxito
- ✅ Todos los tests pasan en cada build
- ✅ Tests son rápidos (< 5s total)
- ✅ Nuevas features incluyen tests
- ⚠️ Coverage > 70% (pendiente)
- ⚠️ 0 warnings en tests (pendiente)

---

## Recursos y Referencias

### Documentación
- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [EF Core In-Memory Database](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database)

### Libros Recomendados
- "Unit Testing Principles, Practices, and Patterns" - Vladimir Khorikov
- "The Art of Unit Testing" - Roy Osherove
- "Test Driven Development: By Example" - Kent Beck

### Patrones de Testing
- AAA (Arrange-Act-Assert)
- Builder Pattern para test data
- Object Mother para test fixtures
- Test Data Builders

---

## Troubleshooting

### Problema: Tests fallan por base de datos compartida
**Solución**: Usar `DatabaseFixture` con nombre único por test.

```csharp
using var context = _fixture.CreateContext($"TestDb_{Guid.NewGuid()}");
```

### Problema: Tests lentos
**Solución**:
- Verificar que se usan mocks para dependencias externas
- Evitar sleeps o delays
- Usar base de datos en memoria

### Problema: Tests intermitentes (flaky)
**Solución**:
- Eliminar dependencias de tiempo (DateTime.Now → mock)
- Asegurar aislamiento entre tests
- No depender de orden de ejecución

---

**Última actualización**: Diciembre 2024
**Versión**: 1.0 - Testing Strategy para Clean Architecture
**Estado**: 16 tests implementados, 100% passing ✅
