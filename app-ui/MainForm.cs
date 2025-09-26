using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GymRoutineGenerator.Infrastructure.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymRoutineGenerator.UI
{
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm>? _logger;
        private readonly WordDocumentService _documentService;
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
        private IntelligentRoutineGenerator routineGenerator;

        // Modern UI Controls
        private ModernCard personalInfoCard;
        private ModernCard trainingCard;
        private ModernCard goalsCard;
        private ModernCard routineCard;
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;

        // Enhanced progress and preview
        private ProgressIndicatorHelper? progressHelper;
        private string? lastGeneratedRoutine;

        public MainForm()
        {
            // Configure services
            _host = ConfigureServices();
            _documentService = _host.Services.GetRequiredService<WordDocumentService>();
            _logger = _host.Services.GetService<ILogger<MainForm>>();
            exportService = new WordDocumentExporter();
            routineGenerator = new IntelligentRoutineGenerator();

            InitializeComponent();
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
                    services.AddTransient<WordDocumentService>();
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

        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            lastGeneratedRoutine = "Rutina Generada. Ajusta tus preferencias y exporta.";
            routineDisplayTextBox.Text = lastGeneratedRoutine;
            exportButton.Enabled = true;
            previewButton.Enabled = true;
            exportToPDFButton.Enabled = true;
        }

        private void PreviewButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Vista previa no disponible en esta versin mnima.", "Vista Previa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Exportar a Word no disponible en esta versin mnima.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportToPDFButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Exportar a PDF no disponible en esta versin mnima.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void ShowImageManager()
        {
            try
            {
                var imageManagerForm = new ExerciseImageManagerForm();
                imageManagerForm.Show();
                imageManagerForm.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error abriendo gestor de imágenes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        
