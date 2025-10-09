# MainForm CQRS Migration Plan

**Fecha**: 2025-10-02
**Objetivo**: Migrar MainForm.cs de Service Locator pattern a CQRS con MediatR
**Archivo**: `UI/MainForm.cs` (1163 líneas)
**Prioridad**: 🔥 Alta (completar Fase 4 al 100%)

---

## 📊 Análisis Actual

### Servicios Utilizados (Service Locator)

```csharp
// Servicios actuales
private IDocumentExportService? exportService;         // Lazy init
private IRoutineGenerationService routineGenerator;    // Constructor DI

// Service Locator calls
AppServices.Get<IRoutineGenerationService>()           // 3 usos
AppServices.Get<IDocumentExportService>()              // 4 usos
AppServices.Get<GymRoutineContext>()                   // 1 uso
AppServices.Get<ExerciseCatalogManagerForm>()          // 1 uso
AppServices.Get<ExerciseExplorerForm>()                // 1 uso
```

### Operaciones Principales

| Operación | Método | Línea | Servicio Usado | Migración a CQRS |
|-----------|--------|-------|----------------|------------------|
| **Generar rutina** | `GenerateButton_Click` | 724 | `IRoutineGenerationService` | `CreateWorkoutPlanCommand` |
| **Generar alternativa** | `GenerateAlternativeButton_Click` | 1093 | `IRoutineGenerationService` | `GenerateAlternativeRoutineCommand` (nuevo) |
| **Exportar a Word** | `ExportButton_Click` | 837 | `IDocumentExportService` | `ExportWorkoutPlanToWordCommand` (nuevo) |
| **Exportar a PDF** | `ExportToPDFButton_Click` | 916 | `IDocumentExportService` | `ExportWorkoutPlanToPDFCommand` (nuevo) |
| **Verificar IA** | `CheckAIStatusButton_Click` | 1165 | `IRoutineGenerationService` | `GetAIStatusQuery` (nuevo) |
| **Explorar ejercicios** | `ExerciseExplorerButton_Click` | 1246 | Form con DI | Ya migrado ✅ |
| **Gestionar catálogo** | `ManageExercisesMenuItem_Click` | 1018 | Form con DI | Ya migrado ✅ |

---

## 🎯 Plan de Migración

### Fase 1: Crear Commands/Queries Faltantes (2-3 horas)

#### 1.1 Commands Nuevos

##### `GenerateAlternativeRoutineCommand`
```csharp
// Location: src/GymRoutineGenerator.Application/Commands/WorkoutPlans/GenerateAlternativeRoutineCommand.cs
public record GenerateAlternativeRoutineCommand : IRequest<Result<string>>
{
    public string UserName { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string FitnessLevel { get; init; } = string.Empty;
    public int TrainingDays { get; init; }
    public List<string> Goals { get; init; } = new();
}

// Handler
public class GenerateAlternativeRoutineCommandHandler
    : IRequestHandler<GenerateAlternativeRoutineCommand, Result<string>>
{
    private readonly IRoutineGenerationService _routineService;

    public async Task<Result<string>> Handle(
        GenerateAlternativeRoutineCommand request,
        CancellationToken cancellationToken)
    {
        // Convertir a UserProfile entity
        var userProfile = new Entities.UserProfile
        {
            Name = request.UserName,
            Age = request.Age,
            Gender = request.Gender,
            FitnessLevel = request.FitnessLevel,
            TrainingDays = request.TrainingDays,
            Goals = request.Goals
        };

        var routine = await _routineService.GenerateAlternativeRoutineAsync(userProfile);
        return Result<string>.Success(routine);
    }
}
```

##### `ExportWorkoutPlanToWordCommand`
```csharp
// Location: src/GymRoutineGenerator.Application/Commands/Documents/ExportWorkoutPlanToWordCommand.cs
public record ExportWorkoutPlanToWordCommand : IRequest<Result<string>>
{
    public string UserName { get; init; } = string.Empty;
    public string RoutineText { get; init; } = string.Empty;
    public List<WorkoutDay>? Workouts { get; init; }
    public string OutputPath { get; init; } = string.Empty;
}

// Handler
public class ExportWorkoutPlanToWordCommandHandler
    : IRequestHandler<ExportWorkoutPlanToWordCommand, Result<string>>
{
    private readonly IDocumentExportService _exportService;

    public async Task<Result<string>> Handle(
        ExportWorkoutPlanToWordCommand request,
        CancellationToken cancellationToken)
    {
        var success = await _exportService.ExportToWordAsync(
            request.UserName,
            request.RoutineText,
            request.Workouts,
            request.OutputPath
        );

        return success
            ? Result<string>.Success(request.OutputPath)
            : Result<string>.Failure("Error al exportar a Word");
    }
}
```

##### `ExportWorkoutPlanToPDFCommand`
```csharp
// Location: src/GymRoutineGenerator.Application/Commands/Documents/ExportWorkoutPlanToPDFCommand.cs
public record ExportWorkoutPlanToPDFCommand : IRequest<Result<string>>
{
    public string UserName { get; init; } = string.Empty;
    public string RoutineText { get; init; } = string.Empty;
    public List<WorkoutDay>? Workouts { get; init; }
    public string OutputPath { get; init; } = string.Empty;
}

// Handler similar a Word
```

#### 1.2 Queries Nuevos

##### `GetAIStatusQuery`
```csharp
// Location: src/GymRoutineGenerator.Application/Queries/AI/GetAIStatusQuery.cs
public record GetAIStatusQuery : IRequest<Result<AIStatusDto>>
{
}

public record AIStatusDto
{
    public bool IsAvailable { get; init; }
    public string StatusMessage { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;
    public bool IsOnline { get; init; }
}

// Handler
public class GetAIStatusQueryHandler
    : IRequestHandler<GetAIStatusQuery, Result<AIStatusDto>>
{
    private readonly IRoutineGenerationService _routineService;

    public async Task<Result<AIStatusDto>> Handle(
        GetAIStatusQuery request,
        CancellationToken cancellationToken)
    {
        var isAvailable = await _routineService.IsAIAvailableAsync();
        var statusInfo = await _routineService.GetAIStatusAsync();

        var dto = new AIStatusDto
        {
            IsAvailable = isAvailable,
            StatusMessage = statusInfo,
            ModelName = "Mistral 7B",
            IsOnline = isAvailable
        };

        return Result<AIStatusDto>.Success(dto);
    }
}
```

---

### Fase 2: Inyectar IMediator en MainForm (30 minutos)

#### 2.1 Modificar Constructor

**Antes**:
```csharp
public MainForm()
{
    if (AppServices.Provider == null)
    {
        AppServices.Configure();
    }

    exportService = null;
    routineGenerator = AppServices.Get<IRoutineGenerationService>();

    InitializeComponent();
    InitializeUI();
}
```

**Después**:
```csharp
private readonly IMediator _mediator;

public MainForm(IMediator mediator)
{
    _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    if (AppServices.Provider == null)
    {
        AppServices.Configure();
    }

    InitializeComponent();
    InitializeUI();
}
```

#### 2.2 Actualizar Program.cs

**Antes**:
```csharp
Application.Run(new MainForm());
```

**Después**:
```csharp
var mediator = AppServices.Get<IMediator>();
Application.Run(new MainForm(mediator));
```

---

### Fase 3: Migrar Métodos a CQRS (3-4 horas)

#### 3.1 Generar Rutina → CreateWorkoutPlanCommand

**Antes** (línea 700-752):
```csharp
private async void GenerateButton_Click(object? sender, EventArgs e)
{
    // ... validaciones ...

    var userProfile = new Entities.UserProfile { /* ... */ };

    if (routineGenerator == null)
    {
        routineGenerator = AppServices.Get<IRoutineGenerationService>();
    }

    var result = await routineGenerator.GeneratePersonalizedRoutineWithStructureAsync(userProfile);
    lastGeneratedRoutine = result.text;
    lastGeneratedWorkoutPlan = result.workouts;

    routineDisplayTextBox.Text = lastGeneratedRoutine;
}
```

**Después**:
```csharp
private async void GenerateButton_Click(object? sender, EventArgs e)
{
    // ... validaciones ...

    var command = new CreateWorkoutPlanCommand
    {
        UserName = nameTextBox.Text,
        Age = (int)ageNumericUpDown.Value,
        Gender = genderComboBox.SelectedItem?.ToString() ?? "No especificado",
        FitnessLevel = fitnessLevelComboBox.SelectedItem?.ToString() ?? "Principiante",
        TrainingDays = trainingDaysTrackBar.Value,
        Goals = goalsCheckedListBox.CheckedItems.Cast<string>().ToList()
    };

    var result = await _mediator.Send(command);

    if (result.IsSuccess)
    {
        var dto = result.Value;
        lastGeneratedRoutine = FormatWorkoutPlanDto(dto); // Helper method
        lastGeneratedWorkoutPlan = ConvertToWorkoutDays(dto); // Helper method
        routineDisplayTextBox.Text = lastGeneratedRoutine;

        statusLabel.Text = "Rutina generada exitosamente!";
    }
    else
    {
        MessageBox.Show(result.Error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

#### 3.2 Generar Alternativa → GenerateAlternativeRoutineCommand

**Antes** (línea 1080-1110):
```csharp
private async void GenerateAlternativeButton_Click(object? sender, EventArgs e)
{
    var userProfile = new Entities.UserProfile { /* ... */ };

    if (routineGenerator == null)
    {
        routineGenerator = AppServices.Get<IRoutineGenerationService>();
    }

    lastGeneratedRoutine = await routineGenerator.GenerateAlternativeRoutineAsync(userProfile);
}
```

**Después**:
```csharp
private async void GenerateAlternativeButton_Click(object? sender, EventArgs e)
{
    var command = new GenerateAlternativeRoutineCommand
    {
        UserName = nameTextBox.Text,
        Age = (int)ageNumericUpDown.Value,
        Gender = genderComboBox.SelectedItem?.ToString() ?? "No especificado",
        FitnessLevel = fitnessLevelComboBox.SelectedItem?.ToString() ?? "Principiante",
        TrainingDays = trainingDaysTrackBar.Value,
        Goals = goalsCheckedListBox.CheckedItems.Cast<string>().ToList()
    };

    var result = await _mediator.Send(command);

    if (result.IsSuccess)
    {
        lastGeneratedRoutine = result.Value;
        routineDisplayTextBox.Text = lastGeneratedRoutine;
        statusLabel.Text = "Rutina alternativa generada!";
    }
}
```

#### 3.3 Exportar Word → ExportWorkoutPlanToWordCommand

**Antes** (línea 830-850):
```csharp
private async void ExportButton_Click(object? sender, EventArgs e)
{
    if (exportService == null)
        exportService = AppServices.Get<IDocumentExportService>();

    var success = await exportService.ExportToWordAsync(
        nameTextBox.Text,
        lastGeneratedRoutine,
        lastGeneratedWorkoutPlan,
        filePath
    );
}
```

**Después**:
```csharp
private async void ExportButton_Click(object? sender, EventArgs e)
{
    var command = new ExportWorkoutPlanToWordCommand
    {
        UserName = nameTextBox.Text,
        RoutineText = lastGeneratedRoutine ?? string.Empty,
        Workouts = lastGeneratedWorkoutPlan,
        OutputPath = filePath
    };

    var result = await _mediator.Send(command);

    if (result.IsSuccess)
    {
        MessageBox.Show($"Rutina exportada exitosamente a:\n{result.Value}");
    }
}
```

#### 3.4 Verificar IA → GetAIStatusQuery

**Antes** (línea 1160-1180):
```csharp
private async void CheckAIStatusButton_Click(object? sender, EventArgs e)
{
    if (routineGenerator == null)
    {
        routineGenerator = AppServices.Get<IRoutineGenerationService>();
    }

    var isAvailable = await routineGenerator.IsAIAvailableAsync();
    var statusInfo = await routineGenerator.GetAIStatusAsync();

    MessageBox.Show(/* ... */);
}
```

**Después**:
```csharp
private async void CheckAIStatusButton_Click(object? sender, EventArgs e)
{
    var query = new GetAIStatusQuery();
    var result = await _mediator.Send(query);

    if (result.IsSuccess)
    {
        var status = result.Value;
        var icon = status.IsAvailable ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
        MessageBox.Show(
            $"Estado de IA:\n\n{status.StatusMessage}\n\nModelo: {status.ModelName}",
            "Estado del Servicio de IA",
            MessageBoxButtons.OK,
            icon
        );
    }
}
```

---

### Fase 4: Eliminar Service Locator Calls (30 minutos)

#### 4.1 Remover campos obsoletos
```csharp
// ELIMINAR
private IDocumentExportService? exportService;
private IRoutineGenerationService routineGenerator;

// MANTENER
private readonly IMediator _mediator;
```

#### 4.2 Remover AppServices.Get calls
- ❌ Línea 81: `routineGenerator = AppServices.Get<IRoutineGenerationService>()`
- ❌ Línea 721: `routineGenerator = AppServices.Get<IRoutineGenerationService>()`
- ❌ Línea 835: `exportService = AppServices.Get<IDocumentExportService>()`
- ❌ Línea 883: `exportService = AppServices.Get<IDocumentExportService>()`
- ❌ Línea 911: `exportService = AppServices.Get<IDocumentExportService>()`
- ❌ Línea 971: `ctx = AppServices.Get<GymRoutineContext>()`
- ❌ Línea 1090: `routineGenerator = AppServices.Get<IRoutineGenerationService>()`
- ❌ Línea 1161: `routineGenerator = AppServices.Get<IRoutineGenerationService>()`

#### 4.3 Mantener DI para Forms (ya migrados)
- ✅ Línea 1018: `AppServices.Get<ExerciseCatalogManagerForm>()` - OK (Form con DI)
- ✅ Línea 1246: `AppServices.Get<ExerciseExplorerForm>()` - OK (Form con DI)

---

### Fase 5: Testing (1-2 horas)

#### 5.1 Unit Tests
```csharp
// Location: tests/GymRoutineGenerator.Tests.CleanArchitecture/UI/MainFormTests.cs
public class MainFormTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MainForm _sut;

    public MainFormTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _sut = new MainForm(_mediatorMock.Object);
    }

    [Fact]
    public async Task GenerateButton_SendsCreateWorkoutPlanCommand()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateWorkoutPlanCommand>(), default))
            .ReturnsAsync(Result<WorkoutPlanDto>.Success(new WorkoutPlanDto()));

        // Act
        // Simular click (requiere reflexión o método helper público)

        // Assert
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CreateWorkoutPlanCommand>(), default),
            Times.Once
        );
    }
}
```

#### 5.2 Integration Tests
```csharp
[Fact]
public async Task MainForm_FullWorkflow_GeneratesAndExportsRoutine()
{
    // Arrange - DI container real
    var mediator = AppServices.Get<IMediator>();
    var form = new MainForm(mediator);

    // Act - Simular workflow completo
    // 1. Llenar campos
    // 2. Generar rutina
    // 3. Exportar a Word

    // Assert
    Assert.True(File.Exists(outputPath));
}
```

---

## 📋 Checklist de Migración

### Preparación
- [ ] Crear branch `feature/mainform-cqrs-migration`
- [ ] Backup de MainForm.cs actual
- [ ] Revisar Commands/Queries existentes en Application layer

### Implementación
- [ ] Crear `GenerateAlternativeRoutineCommand` + Handler
- [ ] Crear `ExportWorkoutPlanToWordCommand` + Handler
- [ ] Crear `ExportWorkoutPlanToPDFCommand` + Handler
- [ ] Crear `GetAIStatusQuery` + Handler
- [ ] Registrar nuevos handlers en DI (`DependencyInjection.cs`)
- [ ] Inyectar `IMediator` en constructor de MainForm
- [ ] Actualizar `Program.cs` para resolver MainForm con DI
- [ ] Migrar `GenerateButton_Click` a CQRS
- [ ] Migrar `GenerateAlternativeButton_Click` a CQRS
- [ ] Migrar `ExportButton_Click` a CQRS
- [ ] Migrar `ExportToPDFButton_Click` a CQRS
- [ ] Migrar `CheckAIStatusButton_Click` a CQRS
- [ ] Crear métodos helper: `FormatWorkoutPlanDto`, `ConvertToWorkoutDays`
- [ ] Remover campos: `exportService`, `routineGenerator`
- [ ] Remover todos los `AppServices.Get` (excepto Forms)

### Testing
- [ ] Escribir unit tests para MainForm
- [ ] Escribir integration tests para workflow completo
- [ ] Probar generación de rutina manual
- [ ] Probar generación alternativa
- [ ] Probar exportación Word
- [ ] Probar exportación PDF
- [ ] Probar verificación estado IA
- [ ] Probar chat conversacional

### Validación
- [ ] Build exitoso sin warnings nuevos
- [ ] Todos los tests passing
- [ ] UI funcional (prueba manual completa)
- [ ] No hay Service Locator calls en MainForm (excepto Forms)
- [ ] Code review con checklist SOLID

### Documentación
- [ ] Actualizar `docs/PROGRESO-CLEAN-ARCHITECTURE.md` (Fase 4 al 90%)
- [ ] Actualizar `CLAUDE.md` con progreso
- [ ] Documentar decisiones de diseño en este plan
- [ ] Crear PR con descripción detallada

---

## 🎯 Resultado Esperado

### Antes (Service Locator)
```csharp
public MainForm()
{
    routineGenerator = AppServices.Get<IRoutineGenerationService>();
}

private async void GenerateButton_Click(...)
{
    var result = await routineGenerator.GeneratePersonalizedRoutineWithStructureAsync(userProfile);
}
```

### Después (CQRS + MediatR)
```csharp
public MainForm(IMediator mediator)
{
    _mediator = mediator;
}

private async void GenerateButton_Click(...)
{
    var command = new CreateWorkoutPlanCommand { /* ... */ };
    var result = await _mediator.Send(command);
}
```

### Métricas de Éxito
- ✅ 0 llamadas a `AppServices.Get<IRoutineGenerationService>()`
- ✅ 0 llamadas a `AppServices.Get<IDocumentExportService>()`
- ✅ 1 campo privado: `IMediator _mediator`
- ✅ 100% operaciones via MediatR
- ✅ Fase 4 completada al 100%
- ✅ Progreso global: 95%

---

## ⚠️ Consideraciones

### Riesgos
1. **Breaking changes en UI**: MainForm es el formulario principal, cualquier error bloquea la app.
   - **Mitigación**: Pruebas exhaustivas manuales + tests automatizados.

2. **Backward compatibility**: Código legacy puede depender de Service Locator.
   - **Mitigación**: Mantener AppServices.Get para Forms hasta migración completa.

3. **Performance**: MediatR agrega overhead mínimo.
   - **Mitigación**: Aceptable (<5ms por operación).

### Dependencias
- `MediatR` ya registrado en DI ✅
- `AutoMapper` configurado ✅
- Commands/Queries base ya existen (`CreateWorkoutPlanCommand`) ✅
- Falta crear 4 nuevos Commands/Queries

### Timeline Estimado
- **Preparación**: 30 min
- **Crear Commands/Queries**: 2-3 horas
- **Migrar MainForm**: 3-4 horas
- **Testing**: 1-2 horas
- **Documentación**: 1 hora
- **Total**: 8-11 horas (~1.5 días)

---

**Próximo paso**: Implementar Commands/Queries nuevos (Fase 1)
