# Gym Routine Generator UI Tests

Este proyecto contiene las pruebas unitarias e de integración para los componentes de interfaz de usuario del Generador de Rutinas de Gimnasio.

## Estructura del Proyecto

```
GymRoutineGenerator.UI.Tests/
├── Forms/                          # Tests para formularios principales
│   ├── MainFormTests.cs           # Tests para el formulario principal
│   └── RoutinePreviewFormTests.cs # Tests para el formulario de vista previa
├── Helpers/                       # Tests para clases de ayuda
│   ├── UITestHelper.cs           # Utilidades para testing de UI
│   └── ProgressIndicatorHelperTests.cs # Tests para indicadores de progreso
├── Integration/                   # Tests de integración
│   └── UIIntegrationTests.cs     # Tests de integración de UI
└── README.md                     # Este archivo
```

## Tecnologías de Testing

- **xUnit**: Framework de testing principal
- **FluentAssertions**: Para assertions más legibles
- **Moq**: Para mocking cuando sea necesario
- **Windows Forms**: Testing de controles WinForms

## Características del Framework de Testing

### UITestHelper
Clase utilitaria que proporciona:
- Creación de formularios de test
- Simulación de interacciones de usuario
- Configuración de controles
- Manejo seguro de recursos
- Validaciones de estado de controles

### Tipos de Tests

#### 1. Tests Unitarios de Formularios
- Verificación de inicialización correcta
- Validación de controles existentes
- Estados iniciales de botones
- Manejo de entrada de usuario

#### 2. Tests de Helpers
- ProgressIndicatorHelper funcionalidad
- Animaciones y transiciones
- Actualizaciones de estado
- Manejo de errores

#### 3. Tests de Integración
- Flujos completos de usuario
- Interacción entre formularios
- Validación de datos
- Manejo de errores

## Ejecutar los Tests

### Desde la línea de comandos:
```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests específicos
dotnet test --filter "MainFormTests"

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar con output detallado
dotnet test --logger "console;verbosity=detailed"
```

### Desde Visual Studio:
1. Abrir el Test Explorer (Test > Test Explorer)
2. Hacer clic en "Run All Tests" o ejecutar tests específicos

## Convenciones de Testing

### Naming
- **Test Class**: `{ClassUnderTest}Tests`
- **Test Method**: `{MethodUnderTest}_{Scenario}_{ExpectedBehavior}`

### Structure (Arrange-Act-Assert)
```csharp
[Fact]
public void Method_Scenario_ExpectedBehavior()
{
    // Arrange - Setup test data and dependencies
    var form = UITestHelper.CreateTestMainForm();

    // Act - Execute the method under test
    var result = form.SomeMethod();

    // Assert - Verify the expected outcome
    result.Should().NotBeNull();
}
```

### Test Categories
- `[Fact]`: Tests simples sin parámetros
- `[Theory]`: Tests parametrizados
- `[Collection("UI Tests")]`: Tests de UI que deben ejecutarse secuencialmente

## Consideraciones Especiales para UI Testing

### 1. Thread Safety
Los tests de UI en WinForms requieren cuidado especial con threading:
```csharp
// Inicializar formularios correctamente
UITestHelper.InitializeFormForTesting(form);

// Procesar mensajes pendientes
Application.DoEvents();
```

### 2. Resource Management
Siempre limpiar recursos:
```csharp
public void Dispose()
{
    UITestHelper.SafeDisposeForm(_form);
}
```

### 3. Timing Issues
Para operaciones asíncronas:
```csharp
await Task.Delay(100);
Application.DoEvents();
```

## Coverage Goals

- **Unit Tests**: >80% code coverage
- **Integration Tests**: Flujos principales cubiertos
- **Critical Paths**: 100% coverage para rutinas de generación y exportación

## Mejores Prácticas

1. **Aislamiento**: Cada test debe ser independiente
2. **Limpieza**: Siempre limpiar recursos en `Dispose()`
3. **Nombres Descriptivos**: Tests deben ser auto-documentados
4. **Setup Mínimo**: Solo configurar lo necesario para cada test
5. **Assertions Específicas**: Usar assertions específicas y descriptivas

## Troubleshooting

### Problemas Comunes

#### Tests fallan por handles de ventana
**Solución**: Usar `UITestHelper.InitializeFormForTesting()`

#### Tests intermitentes en CI
**Solución**: Agregar `Application.DoEvents()` después de operaciones de UI

#### Memory leaks en tests
**Solución**: Implementar `IDisposable` y usar `UITestHelper.SafeDisposeForm()`

## Contribuir

Al agregar nuevos tests:

1. Seguir las convenciones de naming
2. Usar `UITestHelper` para operaciones comunes
3. Agregar tests tanto unitarios como de integración
4. Verificar que los tests pasan en diferentes resoluciones de pantalla
5. Documentar cualquier setup especial requerido