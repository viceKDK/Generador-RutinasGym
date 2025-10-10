using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// using GymRoutineGenerator.Infrastructure.Documents; // Comentado para MVP standalone
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymRoutineGenerator.UI
{
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm>? _logger;
        // private readonly WordDocumentService _documentService; // Comentado para MVP standalone
        private readonly IHost _host;

        // UI Controls
        private TextBox nameTextBox;
        private NumericUpDown ageNumericUpDown;
        private ComboBox genderComboBox;
        private ComboBox fitnessLevelComboBox;
        private TrackBar trainingDaysTrackBar;
        private ComboBox trainingDaysComboBox;
        private Label trainingDaysLabel;
        private CheckedListBox goalsCheckedListBox;
        private ModernButton generateButton;
        private ModernButton exportButton;
        private ModernButton previewButton;
        private ModernButton exportToPDFButton;
        private RichTextBox routineDisplayTextBox;
        private ProgressBar progressBar;
        private Label statusLabel;
        private WordDocumentExporter exportService;
        private readonly SQLiteExerciseImageDatabase imageDatabase = new SQLiteExerciseImageDatabase();
        private List<WorkoutDay> lastGeneratedPlan = new List<WorkoutDay>();
        private readonly OllamaRoutineService ollamaService = new OllamaRoutineService();
        private readonly ManualExerciseSelectionStore manualSelectionStore = new();

        // Modern UI Controls
        private ModernCard personalInfoCard;
        private ModernCard trainingCard;
        private ModernCard goalsCard;
        private ModernCard routineCard;
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel selectionStatusLabel;
        private ToolStripMenuItem? copySelectionMenuItem;

        private readonly ExerciseImageSearchService exerciseSearchService = new();
        private ManualExerciseLibraryService? manualExerciseLibraryService;
        private ExerciseGalleryForm? exerciseGalleryForm;

        public ManualExerciseSelectionStore ManualSelectionStore => manualSelectionStore;

        // Enhanced progress and preview
        private ProgressIndicatorHelper? progressHelper;
        private string? lastGeneratedRoutine;
        private UserProfile? lastGeneratedProfile;

        public MainForm()
        {
            // Configure services
            _host = ConfigureServices();
            // _documentService = _host.Services.GetRequiredService<WordDocumentService>(); // Comentado para MVP standalone
            _logger = _host.Services.GetService<ILogger<MainForm>>();
            exportService = new WordDocumentExporter();

            InitializeComponent();
            manualSelectionStore.SelectionChanged += ManualSelectionStore_SelectionChanged;
            UpdateManualSelectionStatus(manualSelectionStore.CurrentSelection.Count);
            InitializeUI();
            // Ensure clean window title without emoji
            this.Text = "Generador de Rutinas de Gimnasio";

            // Add resize event for responsive design
            this.Resize += MainForm_Resize;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Re-layout controls when form is resized for responsive design
            if (this.WindowState != FormWindowState.Minimized && this.Width > 0 && this.Height > 0)
            {
                LayoutControls();
            }
        }

        private IHost ConfigureServices()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging();
                    // services.AddTransient<WordDocumentService>(); // Comentado para MVP standalone
                })
                .Build();
        }

        private void InitializeComponent()
        {
            // Form properties with modern styling
            this.Text = "Generador de Rutinas de Gimnasio";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 800);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.WindowState = FormWindowState.Maximized;

            // Modern gradient background
            this.BackColor = Color.FromArgb(240, 242, 247);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Enable double buffering for smooth rendering
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            // Create menu and status bar
            CreateMenuAndStatusBar();

            // Create main layout
            CreateControls();
            LayoutControls();
        }

        private void CreateControls()
        {
            // Personal Information with enhanced styling
            var personalInfoLabel = new Label
            {
                Text = "Información Personal",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            nameTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(280, 35),
                PlaceholderText = "Ingresa tu nombre completo",
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                BorderStyle = BorderStyle.FixedSingle
            };
            AddModernTextBoxStyling(nameTextBox);

            ageNumericUpDown = new NumericUpDown
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(120, 35),
                Minimum = 16,
                Maximum = 80,
                Value = 25,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                BorderStyle = BorderStyle.FixedSingle
            };

            genderComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(180, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                FlatStyle = FlatStyle.Flat
            };
            genderComboBox.Items.Clear();
            genderComboBox.Items.AddRange(new[] { "Hombre", "Mujer", "Otro" });
            genderComboBox.SelectedIndex = 0;

            // Training Preferences with enhanced styling
            var trainingInfoLabel = new Label
            {
                Text = "Preferencias de Entrenamiento",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            fitnessLevelComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(220, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                FlatStyle = FlatStyle.Flat
            };
            fitnessLevelComboBox.Items.AddRange(new[] { "°¸¥â° Principiante", "°¸¥ Intermedio", "°¸¥â¡ Avanzado" });
            fitnessLevelComboBox.SelectedIndex = 1;
            // Normalize items without emojis/corruption
            fitnessLevelComboBox.Items.Clear();
            fitnessLevelComboBox.Items.AddRange(new[] { "Principiante", "Intermedio", "Avanzado" });

            // Days per week selector as ComboBox (replacing trackbar)
            trainingDaysComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(80, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                FlatStyle = FlatStyle.Flat
            };
            trainingDaysComboBox.Items.AddRange(new object[] { 1, 2, 3, 4, 5, 6, 7 });
            trainingDaysComboBox.SelectedItem = 3;
            trainingDaysComboBox.SelectedIndexChanged += TrainingDaysComboBox_SelectedIndexChanged;

            trainingDaysTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 7,
                Value = 3,
                TickStyle = TickStyle.Both,
                Size = new Size(280, 50),
                BackColor = Color.FromArgb(248, 249, 250)
            };
            trainingDaysTrackBar.ValueChanged += TrainingDaysTrackBar_ValueChanged;
            trainingDaysTrackBar.Visible = false;
            trainingDaysTrackBar.Visible = false;

            trainingDaysLabel = new Label
            {
                Text = "3 días por semana",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 110, 253),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Goals with enhanced styling
            var goalsLabel = new Label
            {
                Text = "Objetivos de Entrenamiento",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            goalsCheckedListBox = new CheckedListBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(350, 140),
                CheckOnClick = true,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                BorderStyle = BorderStyle.FixedSingle,
                ItemHeight = 24
            };
            goalsCheckedListBox.Items.AddRange(new object[]
            {
                "Ganar fuerza",
                "Ganar masa muscular",
                "Perder peso",
                "Mejorar resistencia",
                "Tonificar el cuerpo"
            });
            goalsCheckedListBox.SetItemChecked(0, true); // Default: Ganar fuerza

            // Action Buttons with modern styling
            generateButton = new ModernButton
            {
                Text = "Generar rutina",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(200, 60),
                NormalColor = Color.FromArgb(25, 135, 84),
                HoverColor = Color.FromArgb(20, 108, 67),
                PressedColor = Color.FromArgb(15, 81, 50),
                BorderRadius = 12
            };
            generateButton.Click += GenerateButton_Click;

            exportButton = new ModernButton
            {
                Text = "Exportar a Word",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(200, 60),
                NormalColor = Color.FromArgb(13, 110, 253),
                HoverColor = Color.FromArgb(10, 88, 202),
                PressedColor = Color.FromArgb(8, 66, 152),
                BorderRadius = 12,
                Enabled = false
            };
            exportButton.Click += ExportButton_Click;

            previewButton = new ModernButton
            {
                Text = "Vista previa",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(200, 60),
                NormalColor = Color.FromArgb(102, 16, 242),
                HoverColor = Color.FromArgb(81, 13, 194),
                PressedColor = Color.FromArgb(61, 10, 146),
                BorderRadius = 12,
                Enabled = false
            };
            previewButton.Click += PreviewButton_Click;

            exportToPDFButton = new ModernButton
            {
                Text = "Exportar a PDF",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(200, 60),
                NormalColor = Color.FromArgb(220, 53, 69),
                HoverColor = Color.FromArgb(176, 42, 55),
                PressedColor = Color.FromArgb(132, 32, 41),
                BorderRadius = 12,
                Enabled = false
            };
            exportToPDFButton.Click += ExportToPDFButton_Click;

            // Routine Display with enhanced styling
            var routineLabel = new Label
            {
                Text = "°¸ââ¹ Rutina Generada",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            routineDisplayTextBox = new RichTextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(Math.Max(400, this.Width / 2 - 60), Math.Max(300, this.Height - 400)),
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false
            };
            AddModernTextBoxStyling(routineDisplayTextBox);

            // Status and Progress with modern styling
            progressBar = new ProgressBar
            {
                Size = new Size(Math.Max(400, this.Width / 2 - 60), 30),
                Style = ProgressBarStyle.Continuous,
                Visible = false,
                ForeColor = Color.FromArgb(25, 135, 84),
                BackColor = Color.FromArgb(233, 236, 239)
            };

            statusLabel = new Label{Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                BackColor = Color.Transparent,
                Visible = false
            };

            // Create modern cards for better organization
            CreateModernCards();

            // Add all controls to their respective cards
            personalInfoCard.Controls.AddRange(new Control[]
            {
                nameTextBox, ageNumericUpDown, genderComboBox
            });

                        trainingCard.Controls.AddRange(new Control[]
            {
                fitnessLevelComboBox, trainingDaysLabel
            });
            if (trainingDaysComboBox != null && !trainingCard.Controls.Contains(trainingDaysComboBox))
                trainingCard.Controls.Add(trainingDaysComboBox);

            goalsCard.Controls.AddRange(new Control[]
            {
                goalsCheckedListBox
            });

            routineCard.Controls.AddRange(new Control[]
            {
                routineDisplayTextBox
            });

            // Add main controls to form
            this.Controls.AddRange(new Control[]
            {
                personalInfoCard, trainingCard, goalsCard, routineCard,
                generateButton, exportButton, previewButton, exportToPDFButton,
                progressBar, statusLabel
            });
        }

        private void LayoutControls()
        {
            // Enhanced responsive layout with modern card-based design
            int leftMargin = 10;
            int rightMargin = Math.Max(this.Width / 2 + 20, 500);
            int topMargin = menuStrip?.Height ?? 25;
            int cardSpacing = 20;
            int bottomMargin = statusStrip?.Height ?? 25;

            // Position modern cards
            personalInfoCard.Location = new Point(leftMargin, topMargin + 20);

            // Position controls within personal info card
            var nameLabel = CreateModernLabel("Nombre:");
            nameLabel.Location = new Point(20, 60);
            nameTextBox.Location = new Point(100, 57);
            personalInfoCard.Controls.Add(nameLabel);

            var ageLabel = CreateModernLabel("Edad:");
            ageLabel.Location = new Point(20, 105);
            ageNumericUpDown.Location = new Point(100, 102);
            personalInfoCard.Controls.Add(ageLabel);

            var genderLabel = CreateModernLabel("Género:");
            genderLabel.Location = new Point(20, 150);
            genderComboBox.Location = new Point(100, 147);
            personalInfoCard.Controls.Add(genderLabel);

            // Training card
            trainingCard.Location = new Point(leftMargin, personalInfoCard.Bottom + cardSpacing);

            var fitnessLabel = CreateModernLabel("Nivel:");
            fitnessLabel.Location = new Point(20, 60);
            fitnessLevelComboBox.Location = new Point(100, 57);
            trainingCard.Controls.Add(fitnessLabel);

            var daysLabel = CreateModernLabel("Días/semana:");
            daysLabel.Location = new Point(20, 105);
            trainingDaysTrackBar.Location = new Point(120, 102);
            if (trainingDaysComboBox != null) trainingDaysComboBox.Location = new Point(180, 102);
            trainingDaysLabel.Location = new Point(20, 135);
            trainingCard.Controls.Add(daysLabel);

            // Goals card
            goalsCard.Location = new Point(leftMargin, trainingCard.Bottom + cardSpacing);
            goalsCheckedListBox.Location = new Point(20, 60);

            // Buttons with improved layout
            int buttonsTop = goalsCard.Bottom + cardSpacing;
            generateButton.Location = new Point(leftMargin, buttonsTop);
            previewButton.Location = new Point(leftMargin + 220, buttonsTop);
            exportButton.Location = new Point(leftMargin, buttonsTop + 70);
            exportToPDFButton.Location = new Point(leftMargin + 220, buttonsTop + 70);

            // Status with better positioning
            statusLabel.Location = new Point(leftMargin, buttonsTop + 150);
            progressBar.Location = new Point(leftMargin, buttonsTop + 175);

            // Right Column - Routine Display card
            routineCard.Location = new Point(rightMargin, topMargin + 20);

            // Adjust routine card size for better responsiveness
            int availableWidth = this.Width - rightMargin - 50;
            int availableHeight = this.Height - (topMargin + bottomMargin + 100);

            if (availableWidth > 400 && availableHeight > 400)
            {
                routineCard.Size = new Size(availableWidth, availableHeight);
                routineDisplayTextBox.Size = new Size(availableWidth - 40, availableHeight - 80);
            }

            routineDisplayTextBox.Location = new Point(20, 60);
        }

        private Label CreateModernLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        private void TrainingDaysTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            if (sender is TrackBar trackBar)
            {
                trainingDaysLabel.Text = $"{trackBar.Value} días por semana";
            }
        }

        // Minimal reimplementations to restore UI functionality
        private void CreateMenuAndStatusBar()
        {
            menuStrip = new MenuStrip();

            // Menú Herramientas
            var toolsMenu = new ToolStripMenuItem("Herramientas");
            var imagesManagerItem = new ToolStripMenuItem("Gestor de imágenes...");
            imagesManagerItem.Click += (s, e) => ShowImageManager();
            toolsMenu.DropDownItems.Add(imagesManagerItem);

            var exerciseGalleryItem = new ToolStripMenuItem("Galería de ejercicios...");
            exerciseGalleryItem.Click += (s, e) => ShowExerciseGallery();
            toolsMenu.DropDownItems.Add(exerciseGalleryItem);

            copySelectionMenuItem = new ToolStripMenuItem("Copiar selección manual al portapapeles")
            {
                Enabled = manualSelectionStore.CurrentSelection.Count > 0
            };
            copySelectionMenuItem.Click += (s, e) => CopyManualSelectionToClipboard();
            toolsMenu.DropDownItems.Add(copySelectionMenuItem);

            // Menú Configuración
            var configMenu = new ToolStripMenuItem("Config");
            var settingsItem = new ToolStripMenuItem("Preferencias...");
            settingsItem.Click += (s, e) => ShowSettings();
            configMenu.DropDownItems.Add(settingsItem);

            // Menú Ayuda
            var helpMenu = new ToolStripMenuItem("Ayuda");
            var viewHelpItem = new ToolStripMenuItem("Ver ayuda");
            viewHelpItem.Click += (s, e) => ShowHelp();
            var aboutItem = new ToolStripMenuItem("Acerca de");
            aboutItem.Click += (s, e) => ShowAbout();
            helpMenu.DropDownItems.Add(viewHelpItem);
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(aboutItem);

            menuStrip.Items.Add(toolsMenu);
            menuStrip.Items.Add(configMenu);
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            statusStrip = new StatusStrip();
            selectionStatusLabel = new ToolStripStatusLabel
            {
                Text = "Selección manual vacía"
            };
            statusStrip.Items.Add(selectionStatusLabel);
            this.Controls.Add(statusStrip);
        }

        private void CreateModernCards()
        {
            personalInfoCard = new ModernCard
            {
                Title = "Información Personal",
                Size = new Size(456, 200),
                CardColor = Color.White
            };

            trainingCard = new ModernCard
            {
                Title = "Preferencias de Entrenamiento",
                Size = new Size(456, 180),
                CardColor = Color.White
            };

            goalsCard = new ModernCard
            {
                Title = "Objetivos de Entrenamiento",
                Size = new Size(456, 220),
                CardColor = Color.White
            };

            routineCard = new ModernCard
            {
                Title = "Rutina Generada",
                Size = new Size(500, 600),
                CardColor = Color.White
            };
        }

        private async Task ValidateUserInput()
        {
            await Task.CompletedTask;
        }

        private async void GenerateButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validar que el nivel de fitness esté seleccionado
                if (fitnessLevelComboBox.SelectedIndex == -1 || string.IsNullOrWhiteSpace(fitnessLevelComboBox.Text))
                {
                    MessageBox.Show(
                        "Por favor selecciona tu nivel de fitness (Principiante, Intermedio o Avanzado).",
                        "Nivel Requerido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    fitnessLevelComboBox.Focus();
                    return;
                }

                generateButton.Enabled = false;
                statusLabel.Text = "Verificando disponibilidad de IA...";
                statusLabel.Visible = true;
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;

                var profile = BuildUserProfileFromForm();

                // Verificar disponibilidad de Ollama
                bool isOllamaAvailable = await ollamaService.IsOllamaAvailable();

                if (!isOllamaAvailable)
                {
                    MessageBox.Show(
                        "Ollama no esta disponible. Por favor:\n\n" +
                        "1. Asegurate de que Ollama este instalado\n" +
                        "2. Ejecuta 'ollama serve' en una terminal\n" +
                        "3. Verifica que el modelo Mistral este instalado: 'ollama pull mistral'\n" +
                        "4. Intenta generar la rutina nuevamente",
                        "IA No Disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    statusLabel.Text = "Error: Ollama no esta disponible";
                    statusLabel.Visible = true;
                    return;
                }

                // Generar rutina con IA
                statusLabel.Text = "Generando rutina con IA (Mistral)... Esto puede tomar 1-2 minutos.";
                Application.DoEvents();

                var aiResponse = await ollamaService.GenerateRoutineWithAI(profile);

                if (aiResponse.Success && aiResponse.WorkoutDays != null && aiResponse.WorkoutDays.Count > 0)
                {
                    // DEBUG: Verificar cuántos ejercicios se generaron
                    System.Diagnostics.Debug.WriteLine($"=== RUTINA GENERADA ===");
                    System.Diagnostics.Debug.WriteLine($"Total días: {aiResponse.WorkoutDays.Count}");
                    foreach (var day in aiResponse.WorkoutDays)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {day.Name}: {day.Exercises?.Count ?? 0} ejercicios");
                        if (day.Exercises != null)
                        {
                            foreach (var ex in day.Exercises)
                            {
                                System.Diagnostics.Debug.WriteLine($"    - {ex.Name} ({ex.SetsAndReps})");
                            }
                        }
                    }

                    // Verificar si algún día tiene ejercicios
                    var totalExercises = aiResponse.WorkoutDays.Sum(d => d.Exercises?.Count ?? 0);
                    if (totalExercises == 0)
                    {
                        MessageBox.Show(
                            "La IA generó los días de entrenamiento pero sin ejercicios.\n\n" +
                            "Esto puede ocurrir si:\n" +
                            "- No hay ejercicios disponibles en la base de datos\n" +
                            "- El formato de respuesta de la IA no fue reconocido\n\n" +
                            "Revisa los archivos debug-respuesta-dia*.txt para más información.",
                            "Advertencia - Sin Ejercicios",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }

                    lastGeneratedPlan = aiResponse.WorkoutDays;
                    lastGeneratedProfile = profile;
                    lastGeneratedRoutine = FormatRoutineForDisplay(profile, aiResponse.WorkoutDays);

                    PopulateRichTextBoxWithImages(profile, aiResponse.WorkoutDays);
                    statusLabel.Text = $"Rutina generada con IA: {totalExercises} ejercicios totales";
                    statusLabel.Visible = true;

                    exportButton.Enabled = true;
                    previewButton.Enabled = true;
                    exportToPDFButton.Enabled = true;
                }
                else
                {
                    MessageBox.Show(
                        "La IA no pudo generar una rutina valida.\n\n" +
                        "Posibles causas:\n" +
                        "- El modelo Mistral no respondio correctamente\n" +
                        "- No hay suficientes ejercicios en la base de datos\n" +
                        "- Error en la comunicacion con Ollama\n\n" +
                        "Intenta nuevamente o revisa los logs.",
                        "Error de Generacion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    statusLabel.Text = "Error: La IA no pudo generar la rutina";
                    statusLabel.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al generar la rutina:\n\n{ex.Message}\n\n" +
                    "Verifica que Ollama este corriendo y el modelo Mistral este instalado.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                statusLabel.Text = "Error al generar rutina";
                statusLabel.Visible = true;
            }
            finally
            {
                generateButton.Enabled = true;
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Blocks;
            }
        }

        private string FormatRoutineForDisplay(UserProfile profile, List<WorkoutDay> workoutDays)
        {
            var sb = new StringBuilder();

            sb.AppendLine(" RUTINA PERSONALIZADA GENERADA CON IA");
            sb.AppendLine("".PadRight(50, ' '));
            sb.AppendLine();

            sb.AppendLine(" INFORMACION DEL CLIENTE:");
            sb.AppendLine("");
            sb.AppendLine($"  Nombre: {profile.Name}");
            sb.AppendLine($"  Edad: {profile.Age} anos");
            sb.AppendLine($"  Genero: {profile.Gender}");
            sb.AppendLine($"  Nivel: {profile.FitnessLevel}");
            sb.AppendLine($"  Frecuencia: {profile.TrainingDays} dias/semana");
            sb.AppendLine("");
            sb.AppendLine();

            sb.AppendLine(" PLAN DE ENTRENAMIENTO:");
            sb.AppendLine("".PadRight(50, ' '));
            sb.AppendLine();

            int dayNum = 1;
            foreach (var day in workoutDays)
            {
                sb.AppendLine($" {day.Name.ToUpper()}");
                sb.AppendLine("".PadRight(40, ' '));

                foreach (var exercise in day.Exercises)
                {
                    sb.AppendLine($" {exercise.Name}");
                    sb.AppendLine($"    {exercise.SetsAndReps}");
                    sb.AppendLine($"    {exercise.Instructions}");

                    if (exercise.ImageData != null && exercise.ImageData.Length > 0)
                    {
                        sb.AppendLine($"    Imagen: Disponible");
                    }

                    sb.AppendLine();
                }

                sb.AppendLine();
                dayNum++;
            }

            sb.AppendLine(" RUTINA COMPLETADA!");
            sb.AppendLine(" Generada con IA (Mistral)");
            sb.AppendLine("".PadRight(50, ' '));

            return sb.ToString();
        }

        private void PreviewButton_Click(object? sender, EventArgs e)
        {
            if (lastGeneratedPlan == null || lastGeneratedPlan.Count == 0)
            {
                MessageBox.Show("Genera una rutina antes de ver la vista previa.", "Vista previa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                ShowRoutinePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar la vista previa: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ExportButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lastGeneratedRoutine) || lastGeneratedPlan == null || lastGeneratedPlan.Count == 0)
            {
                MessageBox.Show("Genera una rutina antes de exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "Documento Word (*.doc)|*.doc|Documento HTML (*.html)|*.html",
                DefaultExt = "doc",
                FileName = $"Rutina_{SanitizeFileName(nameTextBox.Text)}_{DateTime.Now:yyyyMMdd_HHmm}"
            };

            if (saveDialog.ShowDialog(this) != DialogResult.OK)
            {
                statusLabel.Text = "Exportación cancelada";
                statusLabel.Visible = true;
                return;
            }

            try
            {
                statusLabel.Text = "Exportando rutina con imágenes...";
                statusLabel.Visible = true;

                var manualSelection = manualSelectionStore.CurrentSelection.ToArray();

                var success = await exportService.ExportRoutineWithImagesAsync(
                    saveDialog.FileName,
                    lastGeneratedRoutine,
                    lastGeneratedPlan,
                    imageDatabase,
                    manualSelection.Length == 0 ? null : manualSelection);

                if (!success)
                {
                    MessageBox.Show("No se pudo exportar la rutina con imágenes.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Error al exportar";
                    return;
                }

                var finalPath = Path.ChangeExtension(saveDialog.FileName, ".doc");
                MessageBox.Show($"Rutina exportada correctamente en:\n{finalPath}", "Exportación completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                statusLabel.Text = "Rutina exportada";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error al exportar";
            }
        }

        private void ExportToPDFButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Exportar a PDF no disponible en esta versin mnima.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private UserProfile BuildUserProfileFromForm()
        {
            var profile = new UserProfile
            {
                Name = string.IsNullOrWhiteSpace(nameTextBox?.Text) ? "Cliente" : nameTextBox.Text.Trim(),
                Age = (int)(ageNumericUpDown?.Value ?? 25),
                Gender = genderComboBox?.SelectedItem?.ToString() ?? genderComboBox?.Text ?? "Otro",
                FitnessLevel = fitnessLevelComboBox?.SelectedItem?.ToString() ?? fitnessLevelComboBox?.Text ?? "Principiante",
                TrainingDays = DetermineTrainingDays()
            };

            var goals = goalsCheckedListBox?.CheckedItems.Cast<object>()
                .Select(item => item?.ToString() ?? string.Empty)
                .Where(goal => !string.IsNullOrWhiteSpace(goal))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (goals.Count == 0)
            {
                goals.Add("Salud general");
            }

            profile.Goals = goals;
            return profile;
        }

        private int DetermineTrainingDays()
        {
            if (trainingDaysComboBox?.SelectedItem is int comboValue)
            {
                return comboValue;
            }

            if (trainingDaysComboBox?.SelectedItem != null && int.TryParse(trainingDaysComboBox.SelectedItem.ToString(), out var parsed))
            {
                return parsed;
            }

            if (trainingDaysTrackBar != null)
            {
                return trainingDaysTrackBar.Value;
            }

            return 3;
        }

        private void ShowRoutinePreview()
        {
            using var previewForm = new Form
            {
                Text = $"Vista previa - {(string.IsNullOrWhiteSpace(nameTextBox?.Text) ? "Cliente" : nameTextBox.Text.Trim())}",
                Size = new Size(1000, 720),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };

            var tabControl = new TabControl { Dock = DockStyle.Fill };

            var summaryTab = new TabPage("Resumen");
            var summaryBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.White
            };

            // Populate summary with images
            if (lastGeneratedProfile != null && lastGeneratedPlan != null && lastGeneratedPlan.Count > 0)
            {
                PopulateRichTextBoxWithImagesForPreview(summaryBox, lastGeneratedProfile, lastGeneratedPlan);
            }
            else
            {
                summaryBox.Text = lastGeneratedRoutine ?? string.Empty;
            }

            summaryTab.Controls.Add(summaryBox);

            var visualTab = new TabPage("Plan visual");
            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10)
            };

            for (int i = 0; i < lastGeneratedPlan.Count; i++)
            {
                var day = lastGeneratedPlan[i];
                var dayGroup = new GroupBox
                {
                    Text = $"Día {i + 1}: {day.Name}",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(12),
                    Width = 900
                };

                var dayFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };

                if (day.Exercises == null || day.Exercises.Count == 0)
                {
                    dayFlow.Controls.Add(new Label
                    {
                        Text = "Sin ejercicios registrados para este día.",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10f, FontStyle.Italic)
                    });
                }
                else
                {
                    foreach (var exercise in day.Exercises)
                    {
                        dayFlow.Controls.Add(CreateExerciseCard(exercise));
                    }
                }

                dayGroup.Controls.Add(dayFlow);
                flow.Controls.Add(dayGroup);
            }

            scrollPanel.Controls.Add(flow);
            visualTab.Controls.Add(scrollPanel);

            tabControl.TabPages.Add(summaryTab);
            tabControl.TabPages.Add(visualTab);

            previewForm.Controls.Add(tabControl);
            previewForm.ShowDialog(this);
        }

        private Control CreateExerciseCard(Exercise exercise)
        {
            var card = new Panel
            {
                Width = 860,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.FromArgb(245, 246, 250)
            };

            var picture = new PictureBox
            {
                Size = new Size(150, 150),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = LoadExerciseImage(exercise.Name)
            };

            var nameLabel = new Label
            {
                Text = exercise.Name,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = false,
                Location = new Point(170, 10),
                Size = new Size(670, 24)
            };

            var details = $"{exercise.SetsAndReps}\n{exercise.Instructions}";
            var imageInfo = imageDatabase.FindExerciseImage(exercise.Name);
            if (imageInfo != null)
            {
                if (imageInfo.MuscleGroups != null && imageInfo.MuscleGroups.Length > 0)
                {
                    details += $"\nMúsculos: {string.Join(", ", imageInfo.MuscleGroups)}";
                }
                if (!string.IsNullOrWhiteSpace(imageInfo.Description))
                {
                    details += $"\nNota: {imageInfo.Description}";
                }
            }
            if (!string.IsNullOrWhiteSpace(exercise.ImageUrl))
            {
                details += $"\nReferencia: {exercise.ImageUrl}";
            }

            var descriptionLabel = new Label
            {
                Text = details,
                Font = new Font("Segoe UI", 10f),
                AutoSize = false,
                Location = new Point(170, 40),
                Size = new Size(670, 120)
            };
            descriptionLabel.MaximumSize = new Size(670, 0);
            descriptionLabel.AutoSize = true;

            card.Controls.Add(picture);
            card.Controls.Add(nameLabel);
            card.Controls.Add(descriptionLabel);
            card.Height = Math.Max(170, descriptionLabel.Bottom + 10);

            return card;
        }

        private Image? LoadExerciseImage(string exerciseName)
        {
            try
            {
                // Usar ExerciseImageSearchService para búsqueda cascada
                var searchService = new ExerciseImageSearchService();
                var exerciseWithImage = searchService.FindExerciseWithImage(exerciseName);

                if (exerciseWithImage != null && exerciseWithImage.ImageData != null && exerciseWithImage.ImageData.Length > 0)
                {
                    using var ms = new MemoryStream(exerciseWithImage.ImageData);
                    // Clonar la imagen para que sobreviva al cierre del stream
                    var originalImage = Image.FromStream(ms);
                    var clonedImage = new Bitmap(originalImage);
                    originalImage.Dispose();
                    return clonedImage;
                }

                // Fallback: intentar con la BD antigua
                var path = imageDatabase.GetImagePath(exerciseName);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    using var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    var originalImage = Image.FromStream(ms);
                    var clonedImage = new Bitmap(originalImage);
                    originalImage.Dispose();
                    return clonedImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando imagen para {exerciseName}: {ex.Message}");
            }

            return CreatePlaceholderImage(exerciseName);
        }

        private Image CreatePlaceholderImage(string exerciseName)
        {
            var bmp = new Bitmap(300, 180);
            using var graphics = Graphics.FromImage(bmp);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.FromArgb(230, 232, 236));

            using var font = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(90, 90, 90));
            var rect = new RectangleF(20, 40, bmp.Width - 40, bmp.Height - 80);
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.DrawString(exerciseName, font, brush, rect, format);

            return bmp;
        }

        private void PopulateRichTextBoxWithImages(UserProfile profile, List<WorkoutDay> workoutDays)
        {
            routineDisplayTextBox.Clear();
            routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            routineDisplayTextBox.SelectionColor = Color.FromArgb(33, 37, 41);
            routineDisplayTextBox.AppendText("RUTINA PERSONALIZADA GENERADA CON IA\n\n");

            routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
            routineDisplayTextBox.AppendText("INFORMACIÓN DEL CLIENTE:\n");
            routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 10);
            routineDisplayTextBox.AppendText($"  Nombre: {profile.Name}\n");
            routineDisplayTextBox.AppendText($"  Edad: {profile.Age} años\n");
            routineDisplayTextBox.AppendText($"  Género: {profile.Gender}\n");
            routineDisplayTextBox.AppendText($"  Nivel: {profile.FitnessLevel}\n");
            routineDisplayTextBox.AppendText($"  Frecuencia: {profile.TrainingDays} días/semana\n\n");

            routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
            routineDisplayTextBox.AppendText("PLAN DE ENTRENAMIENTO:\n\n");

            for (int i = 0; i < workoutDays.Count; i++)
            {
                var day = workoutDays[i];
                routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 11, FontStyle.Bold);
                routineDisplayTextBox.SelectionColor = Color.FromArgb(33, 37, 41); // Color negro en lugar de azul

                // Mostrar día con grupos musculares
                var dayTitle = day.Name.ToUpper();
                if (day.MuscleGroups != null && day.MuscleGroups.Length > 0)
                {
                    dayTitle += $" - {string.Join(", ", day.MuscleGroups)}";
                }
                routineDisplayTextBox.AppendText($"{dayTitle}\n");

                if (day.Exercises == null || day.Exercises.Count == 0)
                {
                    routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Italic);
                    routineDisplayTextBox.AppendText("  Sin ejercicios asignados\n\n");
                    continue;
                }

                foreach (var exercise in day.Exercises)
                {
                    // Nombre del ejercicio
                    routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
                    routineDisplayTextBox.AppendText($"\n {exercise.Name}\n");

                    // Imagen del ejercicio
                    var image = LoadExerciseImage(exercise.Name);
                    if (image != null)
                    {
                        try
                        {
                            // Redimensionar imagen para que quepa en el RichTextBox
                            var resizedImage = ResizeImage(image, 250, 150);

                            // Insertar imagen usando Clipboard con manejo mejorado
                            var previousClipboard = Clipboard.GetDataObject();
                            Clipboard.SetDataObject(resizedImage);
                            routineDisplayTextBox.Paste();

                            // Restaurar clipboard anterior
                            if (previousClipboard != null)
                            {
                                Clipboard.SetDataObject(previousClipboard);
                            }

                            routineDisplayTextBox.AppendText("\n");

                            // Limpiar recursos
                            if (resizedImage != image)
                            {
                                resizedImage.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error insertando imagen: {ex.Message}");
                            routineDisplayTextBox.AppendText($"    [Imagen de {exercise.Name} no disponible]\n");
                        }
                    }

                    // Series y reps
                    routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 9);
                    routineDisplayTextBox.AppendText($"    {exercise.SetsAndReps}\n");

                    // Instrucciones
                    if (!string.IsNullOrWhiteSpace(exercise.Instructions))
                    {
                        routineDisplayTextBox.AppendText($"    {exercise.Instructions}\n");
                    }

                    routineDisplayTextBox.AppendText("\n");
                }
            }

            routineDisplayTextBox.SelectionFont = new Font("Segoe UI", 11, FontStyle.Bold);
            routineDisplayTextBox.SelectionColor = Color.Green;
            routineDisplayTextBox.AppendText("\nRUTINA COMPLETADA!\n");
            routineDisplayTextBox.AppendText("Generada con IA (Mistral)\n");
            routineDisplayTextBox.SelectionColor = Color.FromArgb(33, 37, 41);
        }

        private void PopulateRichTextBoxWithImagesForPreview(RichTextBox richTextBox, UserProfile profile, List<WorkoutDay> workoutDays)
        {
            richTextBox.Clear();
            richTextBox.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            richTextBox.SelectionColor = Color.FromArgb(33, 37, 41);
            richTextBox.AppendText("RUTINA PERSONALIZADA GENERADA CON IA\n\n");

            richTextBox.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
            richTextBox.AppendText("INFORMACIÓN DEL CLIENTE:\n");
            richTextBox.SelectionFont = new Font("Segoe UI", 10);
            richTextBox.AppendText($"  Nombre: {profile.Name}\n");
            richTextBox.AppendText($"  Edad: {profile.Age} años\n");
            richTextBox.AppendText($"  Género: {profile.Gender}\n");
            richTextBox.AppendText($"  Nivel: {profile.FitnessLevel}\n");
            richTextBox.AppendText($"  Frecuencia: {profile.TrainingDays} días/semana\n\n");

            richTextBox.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
            richTextBox.AppendText("PLAN DE ENTRENAMIENTO:\n\n");

            for (int i = 0; i < workoutDays.Count; i++)
            {
                var day = workoutDays[i];
                richTextBox.SelectionFont = new Font("Segoe UI", 11, FontStyle.Bold);
                richTextBox.SelectionColor = Color.FromArgb(33, 37, 41); // Color negro en lugar de azul

                // Mostrar día con grupos musculares
                var dayTitle = day.Name.ToUpper();
                if (day.MuscleGroups != null && day.MuscleGroups.Length > 0)
                {
                    dayTitle += $" - {string.Join(", ", day.MuscleGroups)}";
                }
                richTextBox.AppendText($"{dayTitle}\n");

                if (day.Exercises == null || day.Exercises.Count == 0)
                {
                    richTextBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Italic);
                    richTextBox.AppendText("  Sin ejercicios asignados\n\n");
                    continue;
                }

                foreach (var exercise in day.Exercises)
                {
                    // Nombre del ejercicio
                    richTextBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
                    richTextBox.AppendText($"\n {exercise.Name}\n");

                    // Imagen del ejercicio
                    var image = LoadExerciseImage(exercise.Name);
                    if (image != null)
                    {
                        try
                        {
                            // Redimensionar imagen para que quepa en el RichTextBox
                            var resizedImage = ResizeImage(image, 250, 150);

                            // Insertar imagen usando Clipboard con manejo mejorado
                            var previousClipboard = Clipboard.GetDataObject();
                            Clipboard.SetDataObject(resizedImage);
                            richTextBox.Paste();

                            // Restaurar clipboard anterior
                            if (previousClipboard != null)
                            {
                                Clipboard.SetDataObject(previousClipboard);
                            }

                            richTextBox.AppendText("\n");

                            // Limpiar recursos
                            if (resizedImage != image)
                            {
                                resizedImage.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error insertando imagen en preview: {ex.Message}");
                            richTextBox.AppendText($"    [Imagen de {exercise.Name} no disponible]\n");
                        }
                    }

                    // Series y reps
                    richTextBox.SelectionFont = new Font("Segoe UI", 9);
                    richTextBox.AppendText($"    {exercise.SetsAndReps}\n");

                    // Instrucciones
                    if (!string.IsNullOrWhiteSpace(exercise.Instructions))
                    {
                        richTextBox.AppendText($"    {exercise.Instructions}\n");
                    }

                    richTextBox.AppendText("\n");
                }
            }

            richTextBox.SelectionFont = new Font("Segoe UI", 11, FontStyle.Bold);
            richTextBox.SelectionColor = Color.Green;
            richTextBox.AppendText("\nRUTINA COMPLETADA!\n");
            richTextBox.AppendText("Generada con IA (Mistral)\n");
            richTextBox.SelectionColor = Color.FromArgb(33, 37, 41);
        }

        private Image ResizeImage(Image original, int maxWidth, int maxHeight)
        {
            if (original.Width <= maxWidth && original.Height <= maxHeight)
                return original;

            double ratioX = (double)maxWidth / original.Width;
            double ratioY = (double)maxHeight / original.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(original.Width * ratio);
            int newHeight = (int)(original.Height * ratio);

            var resized = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(original, 0, 0, newWidth, newHeight);
            }

            return resized;
        }

        private string SanitizeFileName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return "Rutina";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(rawName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray()).Trim();

            return string.IsNullOrWhiteSpace(cleaned) ? "Rutina" : cleaned;
        }


        private void TrainingDaysComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem != null)
            {
                trainingDaysLabel.Text = $"{combo.SelectedItem} días por semana";
            }
        }

        private void InitializeUI()
        {
            nameTextBox?.Focus();
        }

        private void AddModernTextBoxStyling(Control control)
        {
            // Minimal styling stub
            if (control is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
            if (control is RichTextBox rtb) rtb.BorderStyle = BorderStyle.FixedSingle;
        }

        // Simple handlers for menu actions
        private void CopyManualSelectionToClipboard()
        {
            var selection = manualSelectionStore.CurrentSelection;
            if (selection.Count == 0)
            {
                MessageBox.Show("No hay ejercicios en la selección manual.", "Selección manual", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var builder = new StringBuilder();
            foreach (var entry in selection)
            {
                var lineBuilder = new StringBuilder(entry.DisplayName);

                if (!string.IsNullOrWhiteSpace(entry.Source))
                {
                    lineBuilder.Append($" [{entry.Source}]");
                }

                if (!string.IsNullOrWhiteSpace(entry.ImagePath))
                {
                    lineBuilder.Append($" - {entry.ImagePath}");
                }

                if (entry.MuscleGroups.Count > 0)
                {
                    lineBuilder.Append($" (Grupos: {string.Join(", ", entry.MuscleGroups)})");
                }

                builder.AppendLine(lineBuilder.ToString());
            }

            try
            {
                Clipboard.SetText(builder.ToString().TrimEnd());
                statusLabel.Text = $"Selección manual copiada ({selection.Count} ejercicios)";
                statusLabel.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo copiar al portapapeles: {ex.Message}", "Portapapeles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[MainForm] Error copiando selección manual: {ex}");
            }
        }

        private void ManualSelectionStore_SelectionChanged(object? sender, ManualExerciseSelectionChangedEventArgs e)
        {
            if (selectionStatusLabel == null || selectionStatusLabel.IsDisposed)
            {
                return;
            }

            void UpdateStatus() => UpdateManualSelectionStatus(e.Count);

            if (InvokeRequired)
            {
                BeginInvoke((Action)UpdateStatus);
            }
            else
            {
                UpdateStatus();
            }
        }

        private void UpdateManualSelectionStatus(int count)
        {
            if (selectionStatusLabel == null)
            {
                return;
            }

            selectionStatusLabel.Text = count switch
            {
                0 => "Selección manual vacía",
                1 => "Selección manual: 1 ejercicio",
                _ => $"Selección manual: {count} ejercicios"
            };

            if (copySelectionMenuItem != null)
            {
                copySelectionMenuItem.Enabled = count > 0;
            }
        }

        private void ShowImageManager()
        {
            try
            {
                var imageManagerForm = new ImprovedExerciseImageManagerForm();
                imageManagerForm.Show();
                imageManagerForm.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error abriendo gestor de imágenes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowExerciseGallery()
        {
            try
            {
                if (exerciseGalleryForm == null || exerciseGalleryForm.IsDisposed)
                {
                    exerciseGalleryForm = new ExerciseGalleryForm(GetManualLibraryService(), manualSelectionStore)
                    {
                        Owner = this
                    };
                    exerciseGalleryForm.FormClosed += (_, _) => exerciseGalleryForm = null;
                    exerciseGalleryForm.Show(this);
                }
                else
                {
                    if (exerciseGalleryForm.WindowState == FormWindowState.Minimized)
                    {
                        exerciseGalleryForm.WindowState = FormWindowState.Normal;
                    }

                    exerciseGalleryForm.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir la galería de ejercicios. Revisa el log para más detalles.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[MainForm] Error al abrir ExerciseGalleryForm: {ex}");
            }
        }

        private ManualExerciseLibraryService GetManualLibraryService()
        {
            return manualExerciseLibraryService ??= new ManualExerciseLibraryService(exerciseSearchService);
        }

        private void ShowSettings()
        {
            MessageBox.Show("Configuración en desarrollo.", "Preferencias",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowHelp()
        {
            MessageBox.Show("Manual de usuario en preparación.", "Ayuda",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout()
        {
            MessageBox.Show("Generador de Rutinas de Gimnasio\nVersión 2.0", "Acerca de",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
        
