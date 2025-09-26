# AI Development Guidelines for the Project

## References and Sources
- [Common Design Errors Guide](https://sites.google.com/view/erroresdiseodeaplicaciones1/home)
- Clean Code (Robert C. Martin)
- Design Patterns (GoF)
- Refactoring (Martin Fowler)
- Growing Object-Oriented Software, Guided by Tests (Freeman & Pryce)

## Core Architectural Principles

### Data Transfer Objects (DTOs)
- **Purpose**: Separar dominio de exposición
- **Benefits**:
  - Desacoplamiento de información
  - Seguridad y encapsulamiento
  - Representaciones personalizadas
  - Mejora en eficiencia

### Entity Framework Core Inheritance
- **TPH (Table per Hierarchy)**: 1 tabla + columna discriminadora
  - ✅ Use for: Consultas rápidas, simplicidad en modelo
  - ❌ Avoid when: Subclases muy distintas (muchos campos nulos)
- **TPT (Table per Type)**: Tabla por cada clase
  - ✅ Use for: Modelo limpio y normalizado
  - ❌ Avoid when: Performance es crítico (más JOINs)
- **TPC (Table per Concrete Type)**: Tabla por clase concreta
  - ✅ Use for: Consultas muy rápidas
  - ❌ Avoid when: Clases comparten mucha lógica base

### Data Loading Strategies
- **Eager Loading**: `Include()` - Use when you always need related data (1 query)
- **Lazy Loading**: Automatic loading - Use when you don't always need data
- **Explicit Loading**: `Load()` - Use for total control (at least 2 queries)

### Dependency Injection & Service Lifetimes
- **Singleton**: Una instancia durante toda la aplicación
- **Scoped**: Una instancia por request (recomendado para DbContext)
- **Transient**: Nueva instancia cada vez que se solicita

## Clean Code Principles

### Four Core Rules Applied
1. **Evitar comentarios innecesarios**: Código autodescriptivo
2. **Nombres significativos**: Variables y funciones claras
3. **Funciones pequeñas (SRP)**: Una responsabilidad por función
4. **Eliminar números mágicos**: Usar constantes o parámetros

### Error Handling Best Practices
1. **Use excepciones específicas**, no genéricas
2. **Capturar solo donde tenga sentido** manejar el error
3. **No ocultar la excepción original** (usar InnerException)
4. **Mantener flujo limpio** (try-catch separado de lógica principal)

## SOLID Principles Quick Reference

| Principle | What | How to Identify Violations |
|-----------|------|----------------------------|
| **S**RP | Una responsabilidad por clase | Clases que hacen demasiadas cosas |
| **O**CP | Extensible sin modificar | `if/else` según tipo, `switch` statements |
| **L**SP | Subtipos reemplazan al padre | Hijas que lanzan excepciones no esperadas |
| **I**SP | Interfaces pequeñas | Interfaces con métodos inútiles |
| **D**IP | Depender de abstracciones | `new` directo en clases |

## GRASP Principles

1. **Creator**: Asigna creación a quien tiene la información
2. **Information Expert**: Responsabilidad a quien tiene los datos
3. **Controller**: Maneja eventos del sistema
4. **High Cohesion**: Clases enfocadas en responsabilidades específicas
5. **Low Coupling**: Clases poco dependientes entre sí
6. **Polymorphism**: Usa polimorfismo para variaciones
7. **Indirection**: Objeto intermedio para desacoplar
8. **Protected Variations**: Interfaces para proteger de cambios
9. **Pure Fabrication**: Clases artificiales para mejorar diseño

## Design Patterns (Recommended)

### Behavioral Patterns
- **Strategy**: Intercambiar algoritmos dinámicamente
- **Template Method**: Esqueleto de algoritmo con pasos variables

### Structural Patterns
- **Facade**: Interfaz simplificada para subsistemas complejos

### Creational Patterns
- **Factory/Abstract Factory**: Crear familias de objetos relacionados
- **❌ Avoid Singleton**: Causa problemas de testing y acoplamiento

## Testing Standards

### TDD Methodology
Apply **Test-Driven Development** strictly following Red-Green-Refactor cycle

### FIRST Principles (MANDATORY)
**ALL tests MUST comply with FIRST principles - no exceptions:**

- **F**ast: Pruebas ejecutan en < 1 segundo (usar mocks para dependencias externas)
- **I**ndependent: Cada test es completamente independiente (fresh setup, no estado compartido)
- **R**epeatable: Mismos resultados en cualquier ambiente (datos determinísticos, sin randomness)
- **S**elf-validating: Assert claros con criterios pass/fail específicos (no verificación manual)
- **T**imely: Tests escritos ANTES del código de producción (evidencia TDD en commits)

### FIRST Implementation Requirements
1. **Fast Implementation:**
   - Use in-memory databases/collections for testing
   - Mock all external dependencies (files, databases, web services)
   - Target: Unit tests < 1 second, Integration tests < 5 seconds

2. **Independence Implementation:**
   - Each test has [TestInitialize] setup
   - No static shared state between tests
   - Tests can run in any order

3. **Repeatability Implementation:**
   - No DateTime.Now - use IDateTimeProvider
   - No random values - use fixed test data
   - No file system dependencies in unit tests

4. **Self-Validation Implementation:**
   - Specific Assert messages
   - Clear expected vs actual comparisons
   - No manual verification steps

5. **Timeliness Implementation:**
   - Write failing test first (Red)
   - Write minimum code to pass (Green)
   - Refactor while keeping tests green (Refactor)
   - Commit evidence of TDD cycle

## UML Documentation Standards

### Class Diagrams
- Include all UML annotations (multiplicities, navigability)
- Show Forms/UserControls and domain relationships
- Match code structure exactly

### Relationships
- **Herencia**: ▲ (triángulo blanco) - "es un tipo de"
- **Composición**: ♦—— (rombo negro) - "está compuesto por" (relación fuerte)
- **Agregación**: ◊—— (rombo blanco) - "tiene a" (relación débil)
- **Asociación**: → (línea simple) - relación simple

### Package Diagrams
- Must match code namespaces
- Include package descriptions and dependencies

## Code Quality Standards

### Simplicity Principles
- **KISS** (Keep It Simple, Stupid)
- **YAGNI** (You Aren't Gonna Need It)
- **DRY** (Don't Repeat Yourself)

### Polimorfismo Implementation
- **Por Herencia**: Override methods in base class
- **Por Interfaces**: Implement interface methods
- **Por Sobrecarga**: Multiple methods with same name
- **Por Casting**: Convert base type to derived

## Code Smells to Reject

### Structural Issues
- ❌ Methods with multiple responsibilities
- ❌ Business logic in UI projects
- ❌ Exception classes without unique information
- ❌ Interfaces without actual clients
- ❌ Magic numbers and unclear variable names

### Error Handling Issues
- ❌ Generic `throw new Exception()`
- ❌ Empty catch blocks
- ❌ Catching exceptions without handling them
- ❌ Return codes instead of exceptions

### Design Issues
- ❌ God classes (too many responsibilities)
- ❌ Hard-coded dependencies (`new` keywords in business logic)
- ❌ `switch/if-else` chains for type checking
- ❌ Long parameter lists

### Documentation Issues
- ❌ Package diagrams not matching namespaces
- ❌ Missing UML annotations
- ❌ Outdated documentation
- ❌ No index or page numbers

### UI/UX Issues
- ❌ Incorrect tab order in forms
- ❌ Data truncation without scrolling
- ❌ Generic error messages
- ❌ No data validation feedback

## Git Workflow Requirements

### Mandatory Git Workflow
- **MUST use Git for version control** in all projects
- **Branching Strategy**: Follow Git Flow or GitHub Flow
- **Commit Standards**:
  - Clear, descriptive commit messages
  - Atomic commits (one logical change per commit)
  - Reference issues/tickets when applicable
- **Pull Request Process**:
  - All changes must go through pull requests
  - Require code review approval before merging
  - Run automated tests before merge
- **Branch Protection**:
  - Protect main/master branch from direct pushes
  - Require status checks to pass
- **Documentation**: Keep README and documentation updated with each PR

### Git Best Practices
- **Commit Messages**: Use conventional commit format
- **Branch Naming**: Use descriptive names (feature/, bugfix/, hotfix/)
- **History Management**: Keep clean commit history
- **Tagging**: Tag releases appropriately
- **Merge Strategy**: Use appropriate merge/squash/rebase strategy

## Enforcement Guidelines

1. **Code Reviews**: Check for SOLID/GRASP compliance
2. **Architecture Reviews**: Verify pattern usage and separation of concerns
3. **Testing Coverage**: Ensure business logic packages have adequate unit tests
4. **FIRST Compliance**: Verify all tests meet FIRST criteria (MANDATORY)
5. **TDD Evidence**: Check Git history for Red-Green-Refactor commit patterns
6. **Documentation Updates**: Keep UML diagrams synchronized with code
7. **Refactoring**: Apply when code smells are detected
8. **Git Workflow Compliance**: Ensure all changes follow Git workflow requirements

### Testing Violations to Reject Immediately
❌ **Tests that violate FIRST principles:**
- Slow tests (>1 second for unit tests)
- Tests that depend on other tests or external state
- Tests with random or environment-dependent behavior
- Tests requiring manual verification
- Tests written after production code without TDD evidence

❌ **Missing TDD Evidence:**
- No failing tests committed before implementation
- Large commits without incremental test-driven changes
- Production code without corresponding unit tests
- Tests that only verify implementation details, not behavior

**All code must follow these guidelines. Reject any implementation that violates these principles, especially FIRST compliance.**
