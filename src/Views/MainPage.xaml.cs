using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using System;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Core.Services.Diagnostics;
using GymRoutineGenerator.Infrastructure.Documents;
using GymRoutineGenerator.Infrastructure.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GymRoutineGenerator.UI.Views
{
    /// <summary>
    /// Página principal con funcionalidad de exportación a Word
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly IExportService _exportService;
        private readonly IErrorHandlingService _errorService;
        private string _currentExportPath;

        public MainPage()
        {
            this.InitializeComponent();

            try
            {
                // Get services from DI container
                _exportService = App.ServiceProvider.GetRequiredService<IExportService>();
                _errorService = App.ServiceProvider.GetRequiredService<IErrorHandlingService>();
                _currentExportPath = "";

                // Registrar evento de error global
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

                // Verificar salud del sistema al inicializar
                this.Loaded += async (s, e) => await CheckSystemHealthOnStartupAsync();
            }
            catch (Exception ex)
            {
                // Fallback if DI fails
                var wordDocumentService = new WordDocumentService();
                var templateManagerService = new TemplateManagerService();
                _exportService = new SimpleExportService(wordDocumentService, templateManagerService);
                _errorService = new ErrorHandlingService();
                _currentExportPath = "";
            }
        }

        private async Task CheckSystemHealthOnStartupAsync()
        {
            await Task.Run(() => CheckSystemHealthOnStartup());
        }

        private async void OnExportClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                btnExport.IsEnabled = false;
                progressExport.Visibility = Visibility.Visible;
                txtStatus.Text = "🔄 Iniciando creación del documento...";
                txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);

                // Crear rutina de ejemplo
                var routine = CreateSampleRoutine();

                // Obtener plantilla seleccionada
                var templateNames = new[] { "basic", "standard", "professional", "gym", "rehabilitation" };
                var templateId = templateNames[cmbTemplate.SelectedIndex];

                // Configurar opciones de exportación
                var options = new ExportOptions
                {
                    OutputPath = !string.IsNullOrEmpty(_currentExportPath) ? _currentExportPath : null,
                    AutoOpenAfterExport = chkAutoOpen.IsChecked ?? false,
                    OverwriteExisting = true,
                    CreateBackup = false
                };

                // Configurar progreso
                var progress = new Progress<ExportProgress>(p =>
                {
                    progressExport.Value = p.PercentComplete;
                    txtStatus.Text = $"⏳ {p.CurrentOperation} ({p.CurrentStep}/{p.TotalSteps})";
                });

                // Realizar exportación
                var result = await _exportService.ExportRoutineToWordAsync(routine, templateId, options, progress);

                if (result.Success)
                {
                    txtStatus.Text = $"🎉 ¡Documento creado exitosamente! {System.IO.Path.GetFileName(result.FilePath)}";
                    txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);

                    // Actualizar historial con formato amigable
                    var resultText = $"✅ [{DateTime.Now:dd/MM/yyyy HH:mm}] EXITOSO\n" +
                                   $"📁 Archivo: {System.IO.Path.GetFileName(result.FilePath)}\n" +
                                   $"👤 Cliente: {routine.ClientName}\n" +
                                   $"📋 Plantilla: {GetTemplateName(templateId)}\n" +
                                   $"📏 Tamaño: {result.FileSizeBytes / 1024:N0} KB\n" +
                                   $"💪 Ejercicios incluidos: {result.ExerciseCount}\n" +
                                   $"⏱️ Tiempo de creación: {result.ExportDuration.TotalSeconds:F1} segundos\n" +
                                   $"📂 Guardado en: {result.FilePath}\n\n" +
                                   "─────────────────────────────────\n\n";

                    if (txtResults.Text.Contains("Aquí aparecerán"))
                    {
                        txtResults.Text = resultText;
                    }
                    else
                    {
                        txtResults.Text = resultText + txtResults.Text;
                    }
                    txtResults.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                    txtResults.TextAlignment = Microsoft.UI.Xaml.TextAlignment.Left;
                }
                else
                {
                    txtStatus.Text = $"❌ No se pudo crear el documento: {result.ErrorMessage}";
                    txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);

                    // Actualizar historial con formato amigable
                    var errorText = $"❌ [{DateTime.Now:dd/MM/yyyy HH:mm}] ERROR\n" +
                                   $"⚠️ Problema: {result.ErrorMessage}\n" +
                                   $"💡 Intente nuevamente o contacte soporte\n\n" +
                                   "─────────────────────────────────\n\n";

                    if (txtResults.Text.Contains("Aquí aparecerán"))
                    {
                        txtResults.Text = errorText;
                    }
                    else
                    {
                        txtResults.Text = errorText + txtResults.Text;
                    }
                    txtResults.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                    txtResults.TextAlignment = Microsoft.UI.Xaml.TextAlignment.Left;
                }
            }
            catch (Exception ex)
            {
                await HandleExportErrorAsync(ex, "Export");
            }
            finally
            {
                btnExport.IsEnabled = true;
                progressExport.Visibility = Visibility.Collapsed;
                progressExport.Value = 0;
            }
        }

        private async void OnSelectPathClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FolderPicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add("*");

                // Configurar para WinUI 3
                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(picker, hwnd);

                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    _currentExportPath = folder.Path;
                    txtStatus.Text = $"📁 Nueva ubicación seleccionada: {folder.Name}";
                    txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue);

                    await _errorService.LogInfoAsync($"User selected export path: {folder.Path}", "PathSelection");
                }
            }
            catch (Exception ex)
            {
                await HandleGeneralErrorAsync(ex, "PathSelection");
            }
        }

        private Routine CreateSampleRoutine()
        {
            var routine = new Routine
            {
                Id = 1,
                Name = txtRoutineName.Text,
                ClientName = txtClientName.Text,
                Description = "Rutina de entrenamiento de fuerza diseñada para desarrollo muscular progresivo.",
                Goal = "Desarrollo de fuerza y masa muscular",
                DurationWeeks = 4,
                CreatedDate = DateTime.Now
            };

            // Día 1 - Pecho y Tríceps
            var day1 = new RoutineDay
            {
                Id = 1,
                DayNumber = 1,
                Name = "Día 1 - Pecho y Tríceps",
                Description = "Enfoque en músculos del pecho y tríceps",
                FocusArea = "Tren Superior",
                TargetIntensity = "Alta",
                EstimatedDurationMinutes = 75
            };

            day1.Exercises.AddRange(new[]
            {
                new RoutineExercise
                {
                    Id = 1,
                    Order = 1,
                    Name = "Press de Banca",
                    Category = "Fuerza",
                    MuscleGroups = new List<string> { "Pectorales", "Tríceps", "Deltoides Anterior" },
                    Equipment = "Barra y Banco",
                    Instructions = "Acuéstate en el banco con los pies firmes en el suelo. Toma la barra con un agarre ligeramente más ancho que los hombros.",
                    SafetyTips = "Mantén los omóplatos retraídos. No rebotes la barra en el pecho.",
                    Sets = new List<ExerciseSet>
                    {
                        new ExerciseSet { Id = 1, SetNumber = 1, Reps = 12, Weight = 60, RestSeconds = 90 },
                        new ExerciseSet { Id = 2, SetNumber = 2, Reps = 10, Weight = 70, RestSeconds = 90 },
                        new ExerciseSet { Id = 3, SetNumber = 3, Reps = 8, Weight = 80, RestSeconds = 120 },
                        new ExerciseSet { Id = 4, SetNumber = 4, Reps = 6, Weight = 85, RestSeconds = 120 }
                    },
                    RestTimeSeconds = 90,
                    Difficulty = "Intermedio"
                },
                new RoutineExercise
                {
                    Id = 2,
                    Order = 2,
                    Name = "Press Inclinado con Mancuernas",
                    Category = "Fuerza",
                    MuscleGroups = new List<string> { "Pectorales Superior", "Deltoides Anterior" },
                    Equipment = "Mancuernas y Banco Inclinado",
                    Instructions = "Ajusta el banco a 30-45°. Toma las mancuernas con agarre neutro al inicio.",
                    SafetyTips = "Controla el descenso. No dejes caer las mancuernas.",
                    Sets = new List<ExerciseSet>
                    {
                        new ExerciseSet { Id = 5, SetNumber = 1, Reps = 12, Weight = 25, RestSeconds = 75 },
                        new ExerciseSet { Id = 6, SetNumber = 2, Reps = 10, Weight = 30, RestSeconds = 75 },
                        new ExerciseSet { Id = 7, SetNumber = 3, Reps = 8, Weight = 32.5m, RestSeconds = 90 }
                    },
                    RestTimeSeconds = 75,
                    Difficulty = "Intermedio"
                }
            });

            routine.Days.Add(day1);

            // Día 2 - Espalda y Bíceps
            var day2 = new RoutineDay
            {
                Id = 2,
                DayNumber = 2,
                Name = "Día 2 - Espalda y Bíceps",
                Description = "Enfoque en músculos de la espalda y bíceps",
                FocusArea = "Tren Superior",
                TargetIntensity = "Alta",
                EstimatedDurationMinutes = 80
            };

            day2.Exercises.AddRange(new[]
            {
                new RoutineExercise
                {
                    Id = 3,
                    Order = 1,
                    Name = "Dominadas",
                    Category = "Fuerza",
                    MuscleGroups = new List<string> { "Dorsales", "Bíceps", "Romboides" },
                    Equipment = "Barra de Dominadas",
                    Instructions = "Cuelga de la barra con agarre pronador. Tira del cuerpo hacia arriba hasta que la barbilla pase la barra.",
                    SafetyTips = "Controla el descenso. Si no puedes hacer dominadas completas, usa banda elástica.",
                    Sets = new List<ExerciseSet>
                    {
                        new ExerciseSet { Id = 8, SetNumber = 1, Reps = 8, Weight = 0, RestSeconds = 90 },
                        new ExerciseSet { Id = 9, SetNumber = 2, Reps = 6, Weight = 0, RestSeconds = 90 },
                        new ExerciseSet { Id = 10, SetNumber = 3, Reps = 5, Weight = 0, RestSeconds = 120 }
                    },
                    RestTimeSeconds = 90,
                    Difficulty = "Avanzado"
                }
            });

            routine.Days.Add(day2);

            // Calcular métricas
            routine.Metrics = new RoutineMetrics
            {
                TotalExercises = routine.Days.SelectMany(d => d.Exercises).Count(),
                TotalSets = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.Sets).Count(),
                EstimatedDurationMinutes = routine.Days.Sum(d => d.EstimatedDurationMinutes),
                MuscleGroupsCovered = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.MuscleGroups).Distinct().ToList(),
                EquipmentRequired = routine.Days.SelectMany(d => d.Exercises).Select(e => e.Equipment).Distinct().ToList(),
                DifficultyLevel = "Intermedio",
                CaloriesBurnedEstimate = 350
            };

            return routine;
        }

        private string GetTemplateName(string templateId)
        {
            return templateId switch
            {
                "basic" => "Básica",
                "standard" => "Estándar",
                "professional" => "Profesional",
                "gym" => "Gimnasio",
                "rehabilitation" => "Rehabilitación",
                _ => "Desconocida"
            };
        }

        #region Error Handling

        private async Task HandleExportErrorAsync(Exception ex, string context)
        {
            var errorResult = await _errorService.HandleErrorAsync(ex, context);

            txtStatus.Text = $"❌ {errorResult.UserMessage}";
            txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);

            // Mostrar acciones sugeridas en el historial
            var errorText = $"❌ [{DateTime.Now:dd/MM/yyyy HH:mm}] ERROR DE EXPORTACIÓN\n" +
                           $"🔴 Código: {errorResult.ErrorCode}\n" +
                           $"⚠️ Problema: {errorResult.UserMessage}\n" +
                           $"🔄 Recuperable: {(errorResult.IsRecoverable ? "SÍ" : "NO")}\n" +
                           $"💡 Acciones sugeridas:\n";

            foreach (var action in errorResult.SuggestedActions)
            {
                errorText += $"   • {action}\n";
            }

            errorText += "\n─────────────────────────────────\n\n";

            if (txtResults.Text.Contains("Aquí aparecerán"))
            {
                txtResults.Text = errorText;
            }
            else
            {
                txtResults.Text = errorText + txtResults.Text;
            }

            // Mostrar diálogo si es error crítico
            if (!errorResult.IsRecoverable)
            {
                await ShowCriticalErrorDialogAsync(errorResult);
            }
        }

        private async Task HandleGeneralErrorAsync(Exception ex, string context)
        {
            var errorResult = await _errorService.HandleErrorAsync(ex, context);

            txtStatus.Text = $"⚠️ {errorResult.UserMessage}";
            txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);

            if (!errorResult.IsRecoverable)
            {
                await ShowCriticalErrorDialogAsync(errorResult);
            }
        }

        private async Task ShowCriticalErrorDialogAsync(ErrorResult errorResult)
        {
            var dialog = new ContentDialog
            {
                Title = "❌ Error Crítico",
                Content = $"{errorResult.UserMessage}\n\n" +
                         $"Código de error: {errorResult.ErrorCode}\n\n" +
                         $"Acciones recomendadas:\n" +
                         string.Join("\n• ", errorResult.SuggestedActions.Select(a => $"• {a}")),
                PrimaryButtonText = "Reintentar",
                SecondaryButtonText = "Continuar",
                CloseButtonText = "Cerrar Aplicación",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Lógica de reintento si aplica
                await _errorService.LogInfoAsync("User chose to retry after critical error", "ErrorDialog");
            }
            else if (result == ContentDialogResult.None) // Close button
            {
                await _errorService.LogInfoAsync("User chose to close application after critical error", "ErrorDialog");
                App.Current.Exit();
            }
        }

        private async void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                await _errorService.HandleErrorAsync(ex, "UnhandledException");
            }
        }

        private async void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            await _errorService.HandleErrorAsync(e.Exception, "UnobservedTask");
            e.SetObserved(); // Previene el crash de la aplicación
        }

        #endregion

        #region System Health Monitoring

        private async void CheckSystemHealthOnStartup()
        {
            try
            {
                var health = await _errorService.GetSystemHealthAsync();

                if (!health.IsHealthy)
                {
                    var issuesText = string.Join("\n", health.Issues);
                    txtStatus.Text = $"⚠️ Sistema con problemas: {health.Issues.Count} detectados";
                    txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);

                    // Mostrar notificación de degradación elegante
                    if (_errorService.IsServiceDegraded("Ollama"))
                    {
                        ShowDegradationNotification("🤖 IA", "La inteligencia artificial no está disponible, pero puede usar las funciones básicas.");
                    }

                    if (_errorService.IsServiceDegraded("WordGeneration"))
                    {
                        ShowDegradationNotification("📄 Word", "Problemas con generación de Word. Puede que algunos documentos no se exporten correctamente.");
                    }
                }
                else
                {
                    await _errorService.LogInfoAsync("System health check passed - all services operational", "HealthCheck");
                }
            }
            catch (Exception ex)
            {
                await _errorService.HandleErrorAsync(ex, "SystemHealthCheck");
            }
        }

        private void ShowDegradationNotification(string service, string message)
        {
            // En una implementación real, esto sería una notificación toast o un InfoBar
            txtStatus.Text = $"ℹ️ {service}: {message}";
            txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue);
        }

        // Navegación y vistas secundarias
        private UserControl? _currentView;
        private SummaryView? _summaryView;
        private HistoryView? _historyView;
        private UserInputWizard? _wizardView;
        private EquipmentPreferencesForm? _equipmentView;
        private MuscleGroupFocusForm? _muscleView;
        private PhysicalLimitationsForm? _limitsView;
        private UserDemographicsForm? _demographicsView;

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Inicializar vistas si no existen
            _wizardView ??= new UserInputWizard();
            _equipmentView ??= new EquipmentPreferencesForm();
            _muscleView ??= new MuscleGroupFocusForm();
            _limitsView ??= new PhysicalLimitationsForm();
            _demographicsView ??= new UserDemographicsForm();
            _summaryView ??= new SummaryView();
            _historyView ??= new HistoryView();
            _summaryView ??= new SummaryView();

            // Vincular flujo del asistente
            try
            {
                if (_wizardView.FindName("NextButton") is Button nextBtn)
                {
                    nextBtn.Click += (s2, e2) => SelectNavByTag("equipment");
                }

                _equipmentView.PreferencesSaved += (s2, e2) => SelectNavByTag("muscle");
                _muscleView.FocusSaved += (s2, e2) => SelectNavByTag("limits");
                _limitsView.LimitationsSaved += (s2, e2) => SelectNavByTag("demographics");
                _demographicsView.DemographicsSaved += (s2, e2) => SelectNavByTag("export");
            }
            catch { }

            // Seleccionar vista por defecto
            try
            {
                ShowExportView();
                if (NavView != null && NavView.MenuItems.Count > 0)
                {
                    NavView.SelectedItem = NavView.MenuItems[0];
                }
            }
            catch { }
        }

        private void SelectNavByTag(string tag)
        {
            if (NavView == null) return;
            foreach (var mi in NavView.MenuItems)
            {
                if (mi is NavigationViewItem nvi && (nvi.Tag?.ToString() == tag))
                {
                    NavView.SelectedItem = nvi;
                    break;
                }
            }
        }

        private void ShowExportView()
        {
            if (ExportView != null && ContentHost != null)
            {
                ExportView.Visibility = Visibility.Visible;
                ContentHost.Content = null;
            }
        }

        private void ShowContent(UserControl? view)
        {
            if (ExportView != null && ContentHost != null)
            {
                ExportView.Visibility = Visibility.Collapsed;
                _currentView = view;
                ContentHost.Content = _currentView;
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "export":
                        ShowExportView();
                        break;
                    case "wizard":
                        ShowContent(_wizardView);
                        break;
                    case "equipment":
                        ShowContent(_equipmentView);
                        break;
                    case "muscle":
                        ShowContent(_muscleView);
                        break;
                    case "limits":
                        ShowContent(_limitsView);
                        break;
                    case "demographics":
                        ShowContent(_demographicsView);
                        break;
                    case "summary":
                        if (_summaryView != null && _wizardView != null && _equipmentView != null && _muscleView != null && _limitsView != null && _demographicsView != null)
                        {
                            _summaryView.RefreshSummary(_wizardView, _equipmentView, _muscleView, _limitsView, _demographicsView);
                        }
                        ShowContent(_summaryView);
                        break;
                    case "history":
                        _historyView ??= new HistoryView();
                        _historyView.LoadHistory();
                        ShowContent(_historyView);
                        break;
                    case "settings":
                        // TODO: Create SettingsView
                        txtStatus.Text = "ℹ️ Configuración no implementada aún";
                        txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue);
                        break;
                    case "help":
                        // TODO: Create HelpView
                        txtStatus.Text = "ℹ️ Ayuda no implementada aún";
                        txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue);
                        break;
                    case "about":
                        // TODO: Create AboutView
                        txtStatus.Text = "ℹ️ Acerca de no implementado aún";
                        txtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue);
                        break;
                    default:
                        ShowExportView();
                        break;
                }
            }
        }

        #endregion
    }
}




