# Critical Risk Mitigation Strategies
# GymRoutine Generator - Risk Mitigation Plan

Date: 2025-09-23
Author: Quinn (Test Architect)
Status: MANDATORY - Must implement before Epic development

## 🔴 CRITICAL RISK MITIGATIONS

### TECH-001: Ollama AI Service Dependency Failure
**Risk Score: 9/9 - CRITICAL**

#### Immediate Mitigation Strategy:

**1. Robust Fallback Algorithm (MANDATORY)**
```csharp
public interface IRoutineGenerationStrategy
{
    Task<RoutineResponse> GenerateAsync(ClientParameters parameters);
    bool IsAvailable { get; }
    string Name { get; }
}

public class HybridRoutineGenerationService
{
    private readonly IOllamaStrategy _aiStrategy;
    private readonly IAlgorithmicStrategy _fallbackStrategy;

    public async Task<RoutineResponse> GenerateRoutineAsync(ClientParameters parameters)
    {
        // Primary: Try AI generation
        if (_aiStrategy.IsAvailable)
        {
            try
            {
                var result = await _aiStrategy.GenerateAsync(parameters)
                    .TimeoutAfter(TimeSpan.FromSeconds(25));

                if (result.IsValid) return result;
            }
            catch (Exception ex)
            {
                _logger.Warning("AI generation failed: {Error}", ex.Message);
            }
        }

        // Fallback: Use algorithmic generation
        _logger.Information("Using algorithmic fallback generation");
        return await _fallbackStrategy.GenerateAsync(parameters);
    }
}
```

**2. Health Monitoring System**
```csharp
public class OllamaHealthMonitor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var isHealthy = await CheckOllamaHealth();
            _serviceStatus.UpdateStatus(isHealthy ? ServiceStatus.Available : ServiceStatus.Degraded);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task<bool> CheckOllamaHealth()
    {
        try
        {
            var response = await _httpClient.GetAsync("http://localhost:11434/",
                timeout: TimeSpan.FromSeconds(5));
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
```

**3. User Communication Strategy**
```csharp
public enum GenerationMode
{
    AIEnhanced,    // "IA Mejorada" - Ollama working
    BasicMode      // "Modo Básico" - Fallback active
}

// UI Implementation
public void UpdateGenerationModeIndicator(GenerationMode mode)
{
    if (mode == GenerationMode.BasicMode)
    {
        ModeIndicator.Text = "🔧 Modo Básico";
        ModeIndicator.ToolTip = "Generación usando algoritmo básico. Rutinas funcionales pero menos personalizadas.";
        ModeIndicator.Background = Brushes.Orange;
    }
    else
    {
        ModeIndicator.Text = "🤖 IA Mejorada";
        ModeIndicator.ToolTip = "Generación inteligente usando IA local activa.";
        ModeIndicator.Background = Brushes.Green;
    }
}
```

**Implementation Timeline: WEEK 1 (BLOCKING)**

---

### DATA-001: SQLite Database Corruption
**Risk Score: 9/9 - CRITICAL**

#### Immediate Mitigation Strategy:

**1. Database Resilience Architecture**
```csharp
public class ResilientDatabaseService
{
    private readonly string _primaryDbPath;
    private readonly string _backupDbPath;
    private readonly string _recoveryDbPath;

    public async Task<bool> EnsureDatabaseHealth()
    {
        // 1. Integrity check on startup
        if (!await ValidateDatabaseIntegrity(_primaryDbPath))
        {
            _logger.Warning("Primary database corrupted, attempting recovery");

            // 2. Try backup restoration
            if (File.Exists(_backupDbPath) && await ValidateDatabaseIntegrity(_backupDbPath))
            {
                File.Copy(_backupDbPath, _primaryDbPath, overwrite: true);
                _logger.Information("Database restored from backup");
                return true;
            }

            // 3. Last resort: Recreate with seed data
            await RecreateDatabase();
            _logger.Warning("Database recreated with seed data");
        }

        return true;
    }

    private async Task<bool> ValidateDatabaseIntegrity(string dbPath)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            await connection.OpenAsync();

            // Quick integrity check
            var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check(10)";
            var result = await command.ExecuteScalarAsync() as string;

            return result == "ok";
        }
        catch { return false; }
    }
}
```

**2. Automatic Backup System**
```csharp
public class DatabaseBackupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CreateBackup();

            // Daily backup schedule
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task CreateBackup()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var backupPath = Path.Combine(_backupDirectory, $"gym_routine_backup_{timestamp}.db");

            // SQLite backup API
            using var source = new SqliteConnection(_primaryConnectionString);
            using var backup = new SqliteConnection($"Data Source={backupPath}");

            await source.OpenAsync();
            await backup.OpenAsync();

            source.BackupDatabase(backup);

            _logger.Information("Database backup created: {BackupPath}", backupPath);

            // Clean old backups (keep 7 days)
            await CleanOldBackups();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database backup failed");
        }
    }
}
```

**3. WAL Mode Configuration**
```csharp
public class GymRoutineDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString, options =>
        {
            options.CommandTimeout(30);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enable WAL mode for better concurrency
        Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL");
        Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL");
        Database.ExecuteSqlRaw("PRAGMA cache_size=10000");
        Database.ExecuteSqlRaw("PRAGMA temp_store=memory");

        base.OnModelCreating(modelBuilder);
    }
}
```

**Implementation Timeline: WEEK 1 (BLOCKING)**

---

### TECH-002: Word Document Generation Failure
**Risk Score: 9/9 - CRITICAL**

#### Immediate Mitigation Strategy:

**1. Resilient Document Generation**
```csharp
public class ResilientWordDocumentGenerator : IDocumentGenerator
{
    public async Task<DocumentGenerationResult> GenerateAsync(Routine routine)
    {
        var attempts = new[]
        {
            () => GenerateFullDocument(routine),      // Full featured
            () => GenerateSimplifiedDocument(routine), // No images
            () => GenerateTextOnlyDocument(routine)    // Basic fallback
        };

        foreach (var attempt in attempts)
        {
            try
            {
                var result = await attempt();
                if (result.Success) return result;
            }
            catch (Exception ex)
            {
                _logger.Warning("Document generation attempt failed: {Error}", ex.Message);
            }
        }

        throw new DocumentGenerationException("All document generation methods failed");
    }

    private async Task<DocumentGenerationResult> GenerateFullDocument(Routine routine)
    {
        using var document = WordprocessingDocument.Create(_outputPath, WordprocessingDocumentType.Document);

        // Validate all images exist before starting
        var missingImages = ValidateExerciseImages(routine);
        if (missingImages.Any())
        {
            _logger.Warning("Missing images detected: {Count}", missingImages.Count);
            return DocumentGenerationResult.Failed("Missing exercise images");
        }

        // Validate template availability
        if (!File.Exists(_templatePath))
        {
            _logger.Warning("Template file missing: {TemplatePath}", _templatePath);
            return DocumentGenerationResult.Failed("Template not found");
        }

        // Generate with full features
        return await CreateDocumentWithImages(document, routine);
    }
}
```

**2. Image Processing Pipeline**
```csharp
public class ExerciseImageProcessor
{
    public async Task<ProcessedImage> ProcessImageForDocument(string imagePath)
    {
        try
        {
            if (!File.Exists(imagePath))
            {
                return await GetPlaceholderImage();
            }

            using var image = await Image.LoadAsync(imagePath);

            // Resize if too large (max 800x600 for documents)
            if (image.Width > 800 || image.Height > 600)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 600),
                    Mode = ResizeMode.Max
                }));
            }

            // Compress to reduce document size
            using var stream = new MemoryStream();
            await image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = 85 });

            return new ProcessedImage
            {
                Data = stream.ToArray(),
                Width = image.Width,
                Height = image.Height,
                IsPlaceholder = false
            };
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Image processing failed for {ImagePath}", imagePath);
            return await GetPlaceholderImage();
        }
    }

    private async Task<ProcessedImage> GetPlaceholderImage()
    {
        // Generate simple placeholder with exercise name
        using var image = new Image<Rgba32>(400, 300, Color.LightGray);

        image.Mutate(x => x.DrawText(
            "Imagen no\ndisponible",
            _font,
            Color.DarkGray,
            new PointF(200, 150)
        ));

        using var stream = new MemoryStream();
        await image.SaveAsJpegAsync(stream);

        return new ProcessedImage
        {
            Data = stream.ToArray(),
            Width = 400,
            Height = 300,
            IsPlaceholder = true
        };
    }
}
```

**3. Progressive Document Generation with User Feedback**
```csharp
public class ProgressiveDocumentGenerator
{
    public async Task<DocumentGenerationResult> GenerateWithProgress(
        Routine routine,
        IProgress<DocumentGenerationProgress> progress)
    {
        var totalSteps = 5;
        var currentStep = 0;

        try
        {
            // Step 1: Validate inputs
            progress?.Report(new DocumentGenerationProgress(++currentStep, totalSteps, "Validando datos..."));
            await ValidateInputs(routine);

            // Step 2: Process images
            progress?.Report(new DocumentGenerationProgress(++currentStep, totalSteps, "Procesando imágenes..."));
            var processedImages = await ProcessAllImages(routine);

            // Step 3: Create document structure
            progress?.Report(new DocumentGenerationProgress(++currentStep, totalSteps, "Creando estructura..."));
            var document = await CreateDocumentStructure();

            // Step 4: Add content
            progress?.Report(new DocumentGenerationProgress(++currentStep, totalSteps, "Añadiendo contenido..."));
            await AddContentToDocument(document, routine, processedImages);

            // Step 5: Finalize
            progress?.Report(new DocumentGenerationProgress(++currentStep, totalSteps, "Finalizando documento..."));
            await FinalizeDocument(document);

            return DocumentGenerationResult.Success(_outputPath);
        }
        catch (Exception ex)
        {
            progress?.Report(new DocumentGenerationProgress(currentStep, totalSteps, $"Error: {ex.Message}"));
            throw;
        }
    }
}
```

**Implementation Timeline: WEEK 1 (BLOCKING)**

---

### BUS-001: Grandmother-Friendly UI Complexity
**Risk Score: 9/9 - CRITICAL**

#### Immediate Mitigation Strategy:

**1. Accessibility-First UI Architecture**
```xml
<!-- MainWindow.xaml - Accessibility Standards -->
<Window x:Class="GymRoutineGenerator.MainWindow"
        MinWidth="1024" MinHeight="768"
        FontSize="16"
        Background="{StaticResource HighContrastBackground}">

    <Grid Margin="20">
        <!-- Large, clearly labeled sections -->
        <StackPanel Spacing="24">

            <!-- Client Information Section -->
            <Border Style="{StaticResource AccessibleSectionBorder}">
                <StackPanel Spacing="16">
                    <TextBlock Text="Información del Cliente"
                               Style="{StaticResource SectionHeaderStyle}" />

                    <!-- Gender Selection - Extra Large Radio Buttons -->
                    <StackPanel Orientation="Horizontal" Spacing="24">
                        <RadioButton Content="Hombre"
                                     MinWidth="100" MinHeight="60"
                                     FontSize="18" FontWeight="SemiBold" />
                        <RadioButton Content="Mujer"
                                     MinWidth="100" MinHeight="60"
                                     FontSize="18" FontWeight="SemiBold" />
                    </StackPanel>

                    <!-- Age Input - Large, Clear -->
                    <StackPanel Spacing="8">
                        <TextBlock Text="Edad (16-100 años)"
                                   FontSize="16" FontWeight="SemiBold" />
                        <NumberBox Value="{Binding Age}"
                                   MinWidth="150" Height="60"
                                   FontSize="20"
                                   Minimum="16" Maximum="100" />
                    </StackPanel>

                </StackPanel>
            </Border>

            <!-- Generate Button - Extra Large and Prominent -->
            <Button Command="{Binding GenerateRoutineCommand}"
                    Content="GENERAR RUTINA"
                    MinHeight="80" MinWidth="300"
                    FontSize="24" FontWeight="Bold"
                    HorizontalAlignment="Center"
                    Style="{StaticResource PrimaryActionButtonStyle}" />

        </StackPanel>
    </Grid>
</Window>
```

**2. Accessibility Styles and Resources**
```xml
<!-- Styles/AccessibilityStyles.xaml -->
<ResourceDictionary>

    <!-- High Contrast Color Scheme -->
    <SolidColorBrush x:Key="HighContrastBackground" Color="#FFFFFF" />
    <SolidColorBrush x:Key="HighContrastForeground" Color="#000000" />
    <SolidColorBrush x:Key="PrimaryActionColor" Color="#2E7D32" />
    <SolidColorBrush x:Key="SecondaryActionColor" Color="#1976D2" />

    <!-- Section Header Style -->
    <Style x:Key="SectionHeaderStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="24" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Foreground" Value="{StaticResource HighContrastForeground}" />
        <Setter Property="Margin" Value="0,0,0,16" />
    </Style>

    <!-- Accessible Section Border -->
    <Style x:Key="AccessibleSectionBorder" TargetType="Border">
        <Setter Property="BorderBrush" Value="{StaticResource HighContrastForeground}" />
        <Setter Property="BorderThickness" Value="2" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="Padding" Value="20" />
        <Setter Property="Margin" Value="0,0,0,24" />
        <Setter Property="Background" Value="#F8F9FA" />
    </Style>

    <!-- Primary Action Button -->
    <Style x:Key="PrimaryActionButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryActionColor}" />
        <Setter Property="Foreground" Value="White" />
        <Setter Property="BorderThickness" Value="3" />
        <Setter Property="BorderBrush" Value="{StaticResource PrimaryActionColor}" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="Cursor" Value="Hand" />

        <!-- Focus and Hover States -->
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="8">
                        <ContentPresenter HorizontalAlignment="Center"
                                          VerticalAlignment="Center" />
                    </Border>
                    <ControlTemplate.Triggers>
                        <!-- Keyboard Focus -->
                        <Trigger Property="IsFocused" Value="True">
                            <Setter Property="BorderBrush" Value="#FF6F00" />
                            <Setter Property="BorderThickness" Value="5" />
                        </Trigger>
                        <!-- Mouse Hover -->
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="#388E3C" />
                        </Trigger>
                        <!-- Pressed State -->
                        <Trigger Property="IsPressed" Value="True">
                            <Setter Property="Background" Value="#1B5E20" />
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

**3. Keyboard Navigation and Screen Reader Support**
```csharp
public partial class MainWindow : Window
{
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Set up keyboard navigation
        SetupKeyboardNavigation();

        // Configure screen reader support
        ConfigureScreenReaderSupport();
    }

    private void SetupKeyboardNavigation()
    {
        // Ensure tab order is logical
        var controls = GetAllInteractiveControls();
        for (int i = 0; i < controls.Count; i++)
        {
            controls[i].TabIndex = i;
        }

        // Add keyboard shortcuts
        var generateShortcut = new KeyBinding(
            ViewModel.GenerateRoutineCommand,
            new KeyGesture(Key.G, ModifierKeys.Control));
        InputBindings.Add(generateShortcut);
    }

    private void ConfigureScreenReaderSupport()
    {
        // Set automation properties for screen readers
        AutomationProperties.SetName(GenerateButton, "Generar rutina de ejercicio");
        AutomationProperties.SetHelpText(GenerateButton,
            "Presione para generar una rutina personalizada basada en los parámetros ingresados");

        AutomationProperties.SetName(AgeNumberBox, "Edad del cliente");
        AutomationProperties.SetHelpText(AgeNumberBox,
            "Ingrese la edad entre 16 y 100 años");

        // Live region for status updates
        AutomationProperties.SetLiveSetting(StatusTextBlock,
            AutomationLiveSetting.Polite);
    }
}
```

**4. User Testing Framework**
```csharp
public class AccessibilityTestingFramework
{
    public async Task<AccessibilityTestResult> RunComprehensiveTests()
    {
        var results = new List<AccessibilityTestResult>();

        // Test 1: Keyboard Navigation
        results.Add(await TestKeyboardNavigation());

        // Test 2: Screen Reader Compatibility
        results.Add(await TestScreenReaderCompatibility());

        // Test 3: High Contrast Mode
        results.Add(await TestHighContrastMode());

        // Test 4: Text Scaling
        results.Add(await TestTextScaling());

        // Test 5: Color Contrast Ratios
        results.Add(await TestColorContrast());

        return AccessibilityTestResult.Combine(results);
    }

    private async Task<AccessibilityTestResult> TestKeyboardNavigation()
    {
        // Simulate Tab navigation through all controls
        var mainWindow = Application.Current.MainWindow;
        var controls = GetAllFocusableControls(mainWindow);

        foreach (var control in controls)
        {
            control.Focus();
            await Task.Delay(100); // Simulate user pause

            // Verify control is properly focused
            if (!control.IsFocused)
            {
                return AccessibilityTestResult.Failed(
                    $"Control {control.Name} not reachable via keyboard");
            }
        }

        return AccessibilityTestResult.Passed("All controls keyboard accessible");
    }
}
```

**Implementation Timeline: WEEK 1 (BLOCKING)**

---

## 🧪 MITIGATION VALIDATION TESTS

Each mitigation strategy MUST be validated with specific tests:

**AI Fallback Validation:**
```csharp
[TestMethod]
public async Task WhenOllamaUnavailable_ShouldFallbackAndNotifyUser()
{
    // Arrange: Mock Ollama as unavailable
    // Act: Generate routine
    // Assert: Fallback used, UI shows "Modo Básico"
}
```

**Database Recovery Validation:**
```csharp
[TestMethod]
public async Task WhenDatabaseCorrupted_ShouldRecoverFromBackup()
{
    // Arrange: Corrupt primary database
    // Act: Start application
    // Assert: App recovers and functions normally
}
```

**Document Generation Validation:**
```csharp
[TestMethod]
public async Task WhenImagesMissing_ShouldGenerateWithPlaceholders()
{
    // Arrange: Delete exercise images
    // Act: Generate document
    // Assert: Document created with placeholders
}
```

**UI Accessibility Validation:**
```csharp
[TestMethod]
public async Task CompleteWorkflow_UsingOnlyKeyboard_ShouldSucceed()
{
    // Arrange: Disable mouse input
    // Act: Complete full routine generation via keyboard
    // Assert: All functions accessible, routine generated
}
```

## ⏱️ IMPLEMENTATION TIMELINE

**WEEK 1 (BLOCKING DEVELOPMENT):**
- ✅ AI Fallback Algorithm
- ✅ Database Recovery System
- ✅ Document Generation Error Handling
- ✅ UI Accessibility Foundation

**WEEK 2 (HIGH PRIORITY):**
- Performance monitoring
- Security implementation
- Installation process design

**All critical mitigations MUST be completed before any Epic development begins.**