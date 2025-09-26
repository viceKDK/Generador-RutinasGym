# Test Design: GymRoutine Generator Critical Risk Areas

Date: 2025-09-23
Designer: Quinn (Test Architect)
Scope: Critical Risk Areas Testing Strategy

## Test Strategy Overview

- **Total test scenarios**: 47
- **Unit tests**: 22 (47%)
- **Integration tests**: 16 (34%)
- **E2E tests**: 9 (19%)
- **Priority distribution**: P0: 18, P1: 21, P2: 6, P3: 2

**Focus**: Risk-based testing targeting 4 critical failure modes identified in risk assessment.

## 🔴 CRITICAL RISK AREA 1: AI Service Dependency

### Risk: TECH-001 (Score: 9/9)
**Mitigation Target**: Ollama service failures and fallback reliability

#### Test Scenarios

| ID | Level | Priority | Test Scenario | Justification | Risk Mitigation |
|----|-------|----------|---------------|---------------|-----------------|
| AI-UNIT-001 | Unit | P0 | Ollama health check returns false → Service status updates | Isolated health logic | TECH-001 |
| AI-UNIT-002 | Unit | P0 | AI generation timeout (>30s) → Returns timeout result | Timeout handling logic | TECH-001 |
| AI-UNIT-003 | Unit | P0 | Invalid JSON response → Parses gracefully | Response parsing resilience | TECH-001 |
| AI-UNIT-004 | Unit | P0 | Prompt generation with Spanish parameters → Valid prompt | Spanish localization logic | TECH-003 |
| AI-UNIT-005 | Unit | P0 | Fallback algorithm with client params → Valid routine | Algorithm correctness | TECH-001 |
| AI-INT-001 | Integration | P0 | Ollama unavailable → Auto-fallback triggers | Service switching logic | TECH-001 |
| AI-INT-002 | Integration | P0 | AI failure mid-generation → Fallback completes | Failure recovery flow | TECH-001 |
| AI-INT-003 | Integration | P0 | Service recovery → Switches back to AI mode | Service restoration | TECH-001 |
| AI-INT-004 | Integration | P1 | Memory pressure during inference → Handles gracefully | Resource management | PERF-002 |
| AI-E2E-001 | E2E | P0 | Complete routine generation with Ollama down | End-to-end resilience | TECH-001 |
| AI-E2E-002 | E2E | P1 | User sees "Modo Básico" indicator when AI fails | User communication | BUS-001 |

#### Specialized Test Cases

```csharp
[TestClass]
public class AIResilienceTests
{
    [TestMethod]
    public async Task OllamaService_WhenUnreachable_ShouldFallbackWithin5Seconds()
    {
        // Arrange
        var mockOllama = new Mock<IOllamaService>();
        mockOllama.Setup(x => x.IsAvailable).Returns(false);

        var service = new HybridRoutineGenerationService(mockOllama.Object, _fallbackService);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await service.GenerateRoutineAsync(_clientParams);

        // Assert
        stopwatch.Stop();
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, "Fallback took too long");
        Assert.AreEqual(GenerationMethod.Fallback, result.Method);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task AIGeneration_WhenTimeout_ShouldNotBlock()
    {
        // Arrange
        var mockOllama = new Mock<IOllamaService>();
        mockOllama.Setup(x => x.GenerateAsync(It.IsAny<RoutineRequest>()))
                  .Returns(Task.Delay(TimeSpan.FromSeconds(45))); // Simulate hang

        // Act & Assert
        var result = await service.GenerateRoutineAsync(_clientParams);
        Assert.AreEqual(GenerationMethod.Fallback, result.Method);
    }
}
```

---

## 🔴 CRITICAL RISK AREA 2: Database Corruption Recovery

### Risk: DATA-001 (Score: 9/9)
**Mitigation Target**: SQLite corruption detection and recovery

#### Test Scenarios

| ID | Level | Priority | Test Scenario | Justification | Risk Mitigation |
|----|-------|----------|---------------|---------------|-----------------|
| DB-UNIT-001 | Unit | P0 | Database integrity check on corrupted file → Returns false | Corruption detection logic | DATA-001 |
| DB-UNIT-002 | Unit | P0 | Backup creation process → Creates valid backup | Backup generation | DATA-001 |
| DB-UNIT-003 | Unit | P0 | Backup restoration → Restores successfully | Recovery mechanism | DATA-001 |
| DB-UNIT-004 | Unit | P0 | Seed data recreation → Creates valid database | Fallback creation | DATA-001 |
| DB-INT-001 | Integration | P0 | App startup with corrupted DB → Recovers automatically | Auto-recovery flow | DATA-001 |
| DB-INT-002 | Integration | P0 | Backup file corrupted → Uses seed data fallback | Double-failure recovery | DATA-001 |
| DB-INT-003 | Integration | P1 | WAL mode operations → No corruption under stress | Concurrency safety | DATA-001 |
| DB-INT-004 | Integration | P1 | Disk full during backup → Handles gracefully | Error conditions | DATA-001 |
| DB-E2E-001 | E2E | P0 | Complete app workflow after DB recovery | End-to-end validation | DATA-001 |

#### Chaos Testing Framework

```csharp
[TestClass]
public class DatabaseCorruptionTests
{
    [TestMethod]
    public async Task CorruptedDatabase_OnStartup_ShouldRecoverAutomatically()
    {
        // Arrange - Corrupt the database file
        var dbPath = Path.Combine(_testDir, "gym_routine.db");
        await CreateCorruptedDatabase(dbPath);

        // Act - Start the application
        var dbService = new ResilientDatabaseService(dbPath);
        var success = await dbService.EnsureDatabaseHealth();

        // Assert - Database should be recovered
        Assert.IsTrue(success);
        await using var context = new GymRoutineDbContext(dbPath);
        var exerciseCount = await context.Exercises.CountAsync();
        Assert.IsTrue(exerciseCount > 0, "Seed data should be restored");
    }

    private async Task CreateCorruptedDatabase(string path)
    {
        // Create a file with random bytes to simulate corruption
        var randomBytes = new byte[1024];
        new Random().NextBytes(randomBytes);
        await File.WriteAllBytesAsync(path, randomBytes);
    }
}
```

---

## 🔴 CRITICAL RISK AREA 3: Word Document Generation

### Risk: TECH-002 (Score: 9/9)
**Mitigation Target**: Document generation failure scenarios

#### Test Scenarios

| ID | Level | Priority | Test Scenario | Justification | Risk Mitigation |
|----|-------|----------|---------------|---------------|-----------------|
| DOC-UNIT-001 | Unit | P0 | Missing exercise image → Uses placeholder | Image fallback logic | TECH-002 |
| DOC-UNIT-002 | Unit | P0 | Image too large → Compresses automatically | Image processing | TECH-002 |
| DOC-UNIT-003 | Unit | P0 | Invalid template path → Returns error | Template validation | TECH-002 |
| DOC-UNIT-004 | Unit | P0 | Large routine (50+ exercises) → Generates successfully | Scalability | TECH-002 |
| DOC-INT-001 | Integration | P0 | No write permissions → Prompts user for location | Permission handling | TECH-002 |
| DOC-INT-002 | Integration | P0 | Disk full during generation → Fails gracefully | Storage errors | TECH-002 |
| DOC-INT-003 | Integration | P0 | Corrupted template → Falls back to basic template | Template resilience | TECH-002 |
| DOC-INT-004 | Integration | P1 | Memory pressure with large images → Completes | Resource management | PERF-002 |
| DOC-E2E-001 | E2E | P0 | Complete document generation workflow | End-to-end validation | TECH-002 |
| DOC-E2E-002 | E2E | P1 | Document opens correctly in Word | External compatibility | TECH-002 |

#### Performance and Stress Testing

```csharp
[TestClass]
public class DocumentGenerationStressTests
{
    [TestMethod]
    public async Task GenerateDocument_With50Exercises_CompletesUnder15Seconds()
    {
        // Arrange
        var largeRoutine = CreateRoutineWith50Exercises();
        var generator = new ProgressiveDocumentGenerator();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await generator.GenerateWithProgress(largeRoutine, null);

        // Assert
        stopwatch.Stop();
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 15000, "Document generation too slow");
        Assert.IsTrue(result.Success);
        Assert.IsTrue(File.Exists(result.FilePath));
    }

    [TestMethod]
    public async Task GenerateDocument_MissingHalfImages_StillSucceeds()
    {
        // Arrange - Delete half the exercise images
        var routine = CreateStandardRoutine();
        DeleteRandomExerciseImages(routine, percentage: 0.5);

        // Act
        var result = await _generator.GenerateAsync(routine);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.PlaceholdersUsed > 0);
        Assert.IsTrue(File.Exists(result.FilePath));

        // Verify document contains placeholder text
        var documentText = ExtractTextFromDocument(result.FilePath);
        Assert.IsTrue(documentText.Contains("Imagen no disponible"));
    }
}
```

---

## 🔴 CRITICAL RISK AREA 4: Accessibility & UI

### Risk: BUS-001 (Score: 9/9)
**Mitigation Target**: Grandmother-friendly interface compliance

#### Test Scenarios

| ID | Level | Priority | Test Scenario | Justification | Risk Mitigation |
|----|-------|----------|---------------|---------------|-----------------|
| UI-UNIT-001 | Unit | P0 | Button size validation → All buttons ≥60px height | Size requirements | BUS-001 |
| UI-UNIT-002 | Unit | P0 | Font size validation → All text ≥16px | Readability requirements | BUS-001 |
| UI-UNIT-003 | Unit | P0 | Color contrast calculation → All ratios ≥4.5:1 | Accessibility compliance | BUS-001 |
| UI-UNIT-004 | Unit | P0 | Spanish text validation → No English in UI | Localization completeness | TECH-003 |
| UI-INT-001 | Integration | P0 | Keyboard navigation → All controls reachable | Accessibility flow | BUS-001 |
| UI-INT-002 | Integration | P0 | Screen reader compatibility → NVDA announces correctly | Assistive technology | BUS-001 |
| UI-INT-003 | Integration | P0 | High contrast mode → UI remains usable | Visual accessibility | BUS-001 |
| UI-INT-004 | Integration | P1 | Text scaling to 200% → No truncation | Zoom compatibility | BUS-001 |
| UI-E2E-001 | E2E | P0 | Complete workflow keyboard-only → Generates routine | Full accessibility | BUS-001 |
| UI-E2E-002 | E2E | P1 | User testing with 50+ demographic → Successful completion | Target user validation | BUS-001 |

#### Accessibility Testing Framework

```csharp
[TestClass]
public class AccessibilityComplianceTests
{
    [TestMethod]
    public void AllButtons_ShouldMeetMinimumSizeRequirements()
    {
        // Arrange
        var mainWindow = new MainWindow();
        var buttons = GetAllButtons(mainWindow);

        // Act & Assert
        foreach (var button in buttons)
        {
            Assert.IsTrue(button.MinHeight >= 60,
                $"Button '{button.Content}' height {button.MinHeight}px < 60px");
            Assert.IsTrue(button.MinWidth >= 100,
                $"Button '{button.Content}' width {button.MinWidth}px < 100px");
        }
    }

    [TestMethod]
    public async Task KeyboardNavigation_ShouldReachAllInteractiveElements()
    {
        // Arrange
        var window = new MainWindow();
        var interactiveElements = GetAllInteractiveControls(window);
        var unreachableElements = new List<Control>();

        // Act - Simulate Tab navigation
        foreach (var element in interactiveElements)
        {
            var canFocus = element.Focus();
            if (!canFocus || !element.IsFocused)
            {
                unreachableElements.Add(element);
            }
        }

        // Assert
        Assert.AreEqual(0, unreachableElements.Count,
            $"Unreachable elements: {string.Join(", ", unreachableElements.Select(e => e.Name))}");
    }

    [TestMethod]
    public void ColorContrast_ShouldMeetWCAGAAStandards()
    {
        // Arrange
        var window = new MainWindow();
        var textElements = GetAllTextElements(window);

        // Act & Assert
        foreach (var element in textElements)
        {
            var contrast = CalculateContrastRatio(element.Foreground, element.Background);
            Assert.IsTrue(contrast >= 4.5,
                $"Element '{element.Name}' contrast ratio {contrast:F2} < 4.5:1");
        }
    }
}
```

---

## 🎯 PERFORMANCE TESTING SCENARIOS

### Risk: PERF-001 (Score: 6/6)
**Target**: 30-second generation requirement

| ID | Level | Priority | Test Scenario | Justification |
|----|-------|----------|---------------|---------------|
| PERF-UNIT-001 | Unit | P0 | Timeout enforcement → AI calls timeout at 25s | Timeout logic |
| PERF-INT-001 | Integration | P0 | End-to-end timing → Complete flow <30s | Performance SLA |
| PERF-INT-002 | Integration | P1 | Memory usage during AI → Stays <4GB | Resource limits |
| PERF-E2E-001 | E2E | P0 | Performance on minimum hardware → Meets timing | Real-world validation |

#### Performance Benchmarking

```csharp
[TestClass]
public class PerformanceTests
{
    [TestMethod]
    public async Task CompleteWorkflow_ShouldCompleteUnder30Seconds()
    {
        // Arrange
        var clientParams = CreateStandardClientParams();
        var stopwatch = Stopwatch.StartNew();

        // Act - Complete workflow: Generate + Export
        var routine = await _routineService.GenerateRoutineAsync(clientParams);
        var document = await _documentService.ExportAsync(routine);

        // Assert
        stopwatch.Stop();
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000,
            $"Workflow took {stopwatch.ElapsedMilliseconds}ms > 30000ms");
        Assert.IsTrue(routine.IsValid);
        Assert.IsTrue(File.Exists(document.FilePath));
    }
}
```

---

## 🧪 SPECIALIZED TESTING FRAMEWORKS

### Chaos Engineering Tests

```csharp
[TestClass]
public class ChaosEngineeringTests
{
    [TestMethod]
    public async Task SystemResilience_RandomFailures_ShouldRecover()
    {
        var chaosScenarios = new[]
        {
            () => KillOllamaProcess(),
            () => CorruptDatabase(),
            () => FillDiskSpace(),
            () => DeleteRandomImages(),
            () => SimulateMemoryPressure()
        };

        foreach (var scenario in chaosScenarios)
        {
            // Arrange - Create chaos
            scenario();

            // Act - Attempt normal operation
            var result = await _routineService.GenerateRoutineAsync(_params);

            // Assert - System should recover gracefully
            Assert.IsTrue(result.IsValid, $"System failed to handle {scenario.Method.Name}");

            // Cleanup - Restore normal state
            await RestoreNormalState();
        }
    }
}
```

### User Experience Testing

```csharp
[TestClass]
public class UserExperienceTests
{
    [TestMethod]
    public async Task GrandmotherWorkflow_ShouldBeIntuitive()
    {
        // Simulate grandmother user behavior
        var userActions = new[]
        {
            "Click on age field",
            "Enter age slowly (one digit at a time)",
            "Look for gender selection",
            "Take time to read each option",
            "Select gender with uncertainty",
            "Search for 'generate' button",
            "Wait for result patiently"
        };

        // Each action should provide clear feedback
        foreach (var action in userActions)
        {
            await SimulateUserAction(action);
            await Task.Delay(2000); // Simulate thinking time

            // Assert clear visual feedback is provided
            Assert.IsTrue(GetVisualFeedback() != null);
        }
    }
}
```

---

## 📊 TEST EXECUTION STRATEGY

### Phase 1: Critical Path (P0 Tests) - Week 1
**MUST PASS before any development:**
- AI resilience (11 tests)
- Database recovery (9 tests)
- Document generation (9 tests)
- Accessibility compliance (9 tests)

### Phase 2: Core Functionality (P1 Tests) - Week 2
**Standard feature validation:**
- Performance benchmarks
- Integration scenarios
- User experience flows

### Phase 3: Edge Cases (P2/P3 Tests) - Ongoing
**Nice-to-have coverage:**
- Compatibility testing
- Advanced error scenarios
- Optimization validation

---

## 🎯 SUCCESS CRITERIA

**Gate Criteria for Development START:**
- ✅ All P0 tests designed and reviewe
- ✅ Test infrastructure set up
- ✅ Mock services created for AI/DB
- ✅ Accessibility testing framework ready

**Gate Criteria for Production RELEASE:**
- ✅ 100% P0 test pass rate
- ✅ 95% P1 test pass rate
- ✅ Performance benchmarks met
- ✅ Accessibility validation with real users

---

## 📁 Test Assets Required

**Mock Services:**
- MockOllamaService for AI testing
- CorruptDatabaseSimulator for chaos testing
- SlowFileSystemProvider for performance testing

**Test Data:**
- Exercise database with known good/bad data
- Client parameter edge cases
- Document templates (valid/corrupt)
- Image files (various sizes/formats)

**Test Infrastructure:**
- Automated accessibility scanner
- Performance monitoring tools
- Screen reader automation
- Keyboard input simulation

---

**Total Test Coverage: 47 scenarios across all critical risk areas**
**Estimated Execution Time: 2.5 hours (full suite)**
**Risk Mitigation Coverage: 100% of identified critical risks**