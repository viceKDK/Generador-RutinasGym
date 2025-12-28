using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using GymRoutineGenerator.Domain;
using GymRoutineGenerator.Infrastructure;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Gestor hibrido mejorado con botones alineados y funciones de carpeta integradas.
    /// </summary>
    public partial class HybridExerciseManagerForm : Form
    {
        private static readonly Size PreviewImageSize = new Size(320, 320);

        // Singleton instance
        private static HybridExerciseManagerForm? _instance;

        private readonly SQLiteExerciseImageDatabase _imageDatabase;
        private readonly ExerciseMetadataStore _metadataStore;
        private readonly string[] _defaultMuscleGroups =
        {
            "Pecho", "Espalda", "Hombros", "Biceps", "Triceps",
            "Antebrazos", "Abdominales", "Core", "Cuadriceps", "Isquiotibiales",
            "Gluteos", "Gemelos", "Aductores", "Abductores", "Trapecio",
            "Dorsales", "Lumbar", "Cuello", "Cardio"
        };

        // Controles UI
        private ListBox exerciseListBox = null!;
        private TextBox searchBox = null!;
        private PictureBox previewPictureBox = null!;
        private WebBrowser videoWebBrowser = null!;
        private Panel videoPanel = null!;
        private Label videoLabel = null!;
        private TextBox editNameTextBox = null!;
        private TextBox editDescriptionTextBox = null!;
        private TextBox editVideoUrlTextBox = null!;
        private CheckedListBox muscleGroupsCheckedListBox = null!;
        private Panel musclesContentPanel = null!;
        private Panel descriptionContentPanel = null!;
        private Panel videoContentPanel = null!;
        private Button musclesToggleButton = null!;
        private Button descriptionToggleButton = null!;
        private Button videoToggleButton = null!;
        private Button saveButton = null!;
        private Button cancelButton = null!;
        private Button changeImageButton = null!;
        private Button changeVideoButton = null!;
        private Button newExerciseButton = null!;
        private Button deleteExerciseButton = null!;
        private ToolStripStatusLabel statusLabel = null!;

        private List<ExerciseImageInfo> _allExercises = new();
        private ExerciseImageInfo? _currentExercise;
        private bool _isEditing;
        private bool _isLoadingEditor;
        private bool _uiInitialized;
        private Image? _placeholderImage;
        private string? _exportedTempImage;
        
        // Tracking de cambios pendientes
        private string? _pendingImagePath;
        private bool _imageChanged;

        private HybridExerciseManagerForm()
        {
            SetBrowserEmulation();
            _imageDatabase = new SQLiteExerciseImageDatabase();
            _metadataStore = new ExerciseMetadataStore(AppDomain.CurrentDomain.BaseDirectory);
            InitializeComponent();

            TaskbarGroupingHelper.ConfigureFormGrouping(this);
            LoadExercises();
        }

        /// <summary>
        /// Fuerza al control WebBrowser a usar el motor de IE 11 en lugar de IE 7.
        /// Esto corrige el Error 153 de YouTube.
        /// </summary>
        private void SetBrowserEmulation()
        {
            try
            {
                string fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "GeneradorRutinasGimnasio.exe");
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
                {
                    if (key != null)
                    {
                        key.SetValue(fileName, 11001, Microsoft.Win32.RegistryValueKind.DWord);
                    }
                }
            }
            catch { /* Ignorar si no hay permisos de registro */ }
        }

        public static void ShowSingleton()
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new HybridExerciseManagerForm();
                _instance.FormClosed += (s, e) => _instance = null;
                var owner = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                _instance.ShowInTaskbar = false;
                _instance.WindowState = FormWindowState.Maximized;
                if (owner != null)
                {
                    _instance.StartPosition = FormStartPosition.CenterParent;
                    _instance.ShowDialog(owner);
                }
                else
                {
                    _instance.ShowDialog();
                }
            }
            else
            {
                if (_instance.WindowState == FormWindowState.Minimized)
                    _instance.WindowState = FormWindowState.Normal;

                _instance.Activate();
                _instance.BringToFront();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _placeholderImage?.Dispose();
                if (_exportedTempImage != null && File.Exists(_exportedTempImage))
                {
                    try { File.Delete(_exportedTempImage); } catch { }
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Text = "Gestor de ejercicios";
            Size = new Size(1400, 800);
            MinimumSize = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(240, 242, 247);  
            Font = new Font("Segoe UI", 10F);

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = true;
                    Close();
                }
            };

            _placeholderImage = CreatePlaceholderImage();

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(20, 16, 20, 16)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400));  
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));    
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));    

            mainLayout.Controls.Add(BuildListColumn(), 0, 0);
            mainLayout.Controls.Add(BuildPreviewColumn(), 1, 0);
            mainLayout.Controls.Add(BuildEditColumn(), 2, 0);

            Controls.Add(mainLayout);

            var statusStrip = new StatusStrip { BackColor = Color.White, SizingGrip = false };
            statusLabel = new ToolStripStatusLabel { Text = "Selecciona un ejercicio para ver detalles", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            statusStrip.Items.Add(statusLabel);
            Controls.Add(statusStrip);

            _uiInitialized = true;
        }

        #region Column Builders

        private Control BuildListColumn()
        {
            var card = CreateCard("Ejercicios");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(16, 12, 16, 12)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Search
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // ListBox
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Button 1
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Button 2

            searchBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Buscar ejercicio...", Font = new Font("Segoe UI", 10F), Height = 32, Margin = new Padding(0, 0, 0, 8), BorderStyle = BorderStyle.FixedSingle };
            searchBox.TextChanged += (s, e) => FilterExercises();
            layout.Controls.Add(searchBox, 0, 0);

            exerciseListBox = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, IntegralHeight = false, Font = new Font("Segoe UI", 10.5F), DisplayMember = "ExerciseName", BackColor = Color.White, ForeColor = Color.FromArgb(33, 37, 41), Margin = new Padding(0, 0, 0, 10) };
            exerciseListBox.SelectedIndexChanged += ExerciseListBox_SelectedIndexChanged;
            layout.Controls.Add(exerciseListBox, 0, 1);

            newExerciseButton = CreateButton("Nuevo ejercicio", Color.FromArgb(25, 135, 84), Color.White);
            newExerciseButton.Height = 42;
            newExerciseButton.Click += (s, e) => CreateNewExercise();
            newExerciseButton.Margin = new Padding(0, 0, 0, 6);
            layout.Controls.Add(newExerciseButton, 0, 2);

            deleteExerciseButton = CreateButton("Eliminar", Color.FromArgb(220, 53, 69), Color.White);
            deleteExerciseButton.Height = 42;
            deleteExerciseButton.Click += (s, e) => DeleteCurrentExercise();
            deleteExerciseButton.Enabled = false;
            deleteExerciseButton.Margin = new Padding(0);
            layout.Controls.Add(deleteExerciseButton, 0, 3);

            card.Controls.Add(layout, 0, 1);
            return card;
        }

        private Control BuildPreviewColumn()
        {
            var card = CreateCard("Vista Previa");

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(16, 12, 16, 12)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Content (Image + Video)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Button 1
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Button 2

            var detailsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0) };
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Image
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Label Video
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Video

            previewPictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Image = _placeholderImage, AllowDrop = true, Margin = new Padding(0, 0, 0, 5) };
            previewPictureBox.DragEnter += PreviewPictureBox_DragEnter;
            previewPictureBox.DragDrop += PreviewPictureBox_DragDrop;
            detailsLayout.Controls.Add(previewPictureBox, 0, 0);

            videoLabel = new Label { Text = "Video del ejercicio", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
            detailsLayout.Controls.Add(videoLabel, 0, 1);

            videoPanel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, Padding = new Padding(0), Margin = new Padding(0, 5, 0, 10) };
            videoWebBrowser = new WebBrowser { Dock = DockStyle.Fill, ScriptErrorsSuppressed = true, ScrollBarsEnabled = false, AllowNavigation = true, IsWebBrowserContextMenuEnabled = false };
            
            // Bloquear apertura de ventanas externas y redirigir al navegador real
            videoWebBrowser.Navigating += (s, e) => {
                if (e.Url.ToString().StartsWith("http") && e.Url.ToString() != "about:blank") {
                    e.Cancel = true;
                    try { Process.Start(new ProcessStartInfo { FileName = e.Url.ToString(), UseShellExecute = true }); } catch { }
                }
            };
            
            videoWebBrowser.Navigate("about:blank");
            
            var refreshVideoButton = new Button { Text = "⟳", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Size = new Size(40, 40), BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            refreshVideoButton.FlatAppearance.BorderSize = 0;
            refreshVideoButton.Click += (s, e) => LoadVideoInBrowser(_currentExercise?.VideoUrl);
            refreshVideoButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            videoPanel.SizeChanged += (s, e) => { try { refreshVideoButton.Location = new Point(Math.Max(10, videoPanel.ClientSize.Width - refreshVideoButton.Width - 10), 10); } catch { } };
            videoPanel.Controls.Add(videoWebBrowser);
            videoPanel.Controls.Add(refreshVideoButton);
            detailsLayout.Controls.Add(videoPanel, 0, 2);

            mainLayout.Controls.Add(detailsLayout, 0, 0);

            changeImageButton = CreateButton("Cambiar imagen", Color.FromArgb(13, 110, 253), Color.White);
            changeImageButton.Height = 42;
            changeImageButton.Margin = new Padding(0, 0, 0, 6); 
            changeImageButton.Click += (s, e) => OpenImageFolder();
            changeImageButton.Enabled = false;
            mainLayout.Controls.Add(changeImageButton, 0, 1);

            changeVideoButton = CreateButton("Cambiar video", Color.FromArgb(102, 16, 242), Color.White);
            changeVideoButton.Height = 42;
            changeVideoButton.Margin = new Padding(0);
            changeVideoButton.Click += (s, e) => OpenVideoFolder();
            changeVideoButton.Enabled = false;
            mainLayout.Controls.Add(changeVideoButton, 0, 2);

            card.Controls.Add(mainLayout, 0, 1);
            return card;
        }

        private Control BuildEditColumn()
        {
            var card = CreateCard("Edición");

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(16, 12, 16, 12)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Content (Fields)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Button 1
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Button 2

            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(0), Margin = new Padding(0, 0, 0, 10) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0) };

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(CreateLabel("Nombre del ejercicio:"), 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            editNameTextBox = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 11F), Height = 32, Margin = new Padding(0, 4, 0, 12) };
            editNameTextBox.TextChanged += OnFieldChanged;
            layout.Controls.Add(editNameTextBox, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            musclesToggleButton = CreateToggleButton("▼ Grupos musculares");
            musclesToggleButton.Click += (s, e) => ToggleSection(musclesToggleButton, musclesContentPanel);
            layout.Controls.Add(musclesToggleButton, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            musclesContentPanel = new Panel { Dock = DockStyle.Top, AutoSize = true, Visible = true };
            muscleGroupsCheckedListBox = new CheckedListBox { Dock = DockStyle.Top, CheckOnClick = true, IntegralHeight = false, Height = 180, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F), Margin = new Padding(0, 4, 0, 12) };
            muscleGroupsCheckedListBox.Items.AddRange(_defaultMuscleGroups.Cast<object>().ToArray());
            muscleGroupsCheckedListBox.ItemCheck += OnMuscleGroupCheck;
            musclesContentPanel.Controls.Add(muscleGroupsCheckedListBox);
            layout.Controls.Add(musclesContentPanel, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            descriptionToggleButton = CreateToggleButton("▼ Descripción");
            descriptionToggleButton.Click += (s, e) => ToggleSection(descriptionToggleButton, descriptionContentPanel);
            layout.Controls.Add(descriptionToggleButton, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            descriptionContentPanel = new Panel { Dock = DockStyle.Top, AutoSize = true, Visible = true };
            editDescriptionTextBox = new TextBox { Dock = DockStyle.Top, Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F), Margin = new Padding(0, 4, 0, 12) };
            editDescriptionTextBox.TextChanged += OnFieldChanged;
            descriptionContentPanel.Controls.Add(editDescriptionTextBox);
            layout.Controls.Add(descriptionContentPanel, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            videoToggleButton = CreateToggleButton("▼ Link de video (opcional)");
            videoToggleButton.Click += (s, e) => ToggleSection(videoToggleButton, videoContentPanel);
            layout.Controls.Add(videoToggleButton, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            videoContentPanel = new Panel { Dock = DockStyle.Top, AutoSize = true, Visible = true };
            editVideoUrlTextBox = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 10F), Height = 32, PlaceholderText = "https://www.youtube.com/watch?v=...", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 0, 12) };
            editVideoUrlTextBox.TextChanged += OnFieldChanged;
            editVideoUrlTextBox.Leave += (s, e) => LoadVideoInBrowser(editVideoUrlTextBox.Text.Trim());
            videoContentPanel.Controls.Add(editVideoUrlTextBox);
            layout.Controls.Add(videoContentPanel, 0, layout.RowCount - 1);

            scrollPanel.Controls.Add(layout);
            mainLayout.Controls.Add(scrollPanel, 0, 0);

            saveButton = CreateButton("Guardar cambios", Color.FromArgb(25, 135, 84), Color.White);
            saveButton.Height = 42;
            saveButton.Margin = new Padding(0, 0, 0, 6);
            saveButton.Click += (s, e) => SaveCurrentExercise();
            saveButton.Enabled = false;
            mainLayout.Controls.Add(saveButton, 0, 1);

            cancelButton = CreateButton("Cancelar", Color.FromArgb(108, 117, 125), Color.White);
            cancelButton.Height = 42;
            cancelButton.Margin = new Padding(0);
            cancelButton.Click += (s, e) => CancelEdit();
            cancelButton.Enabled = false;
            mainLayout.Controls.Add(cancelButton, 0, 2);

            card.Controls.Add(mainLayout, 0, 1);
            return card;
        }

        private void ToggleSection(Button button, Panel panel)
        {
            panel.Visible = !panel.Visible;
            button.Text = panel.Visible ? button.Text.Replace("▸", "▼") : button.Text.Replace("▼", "▸");
        }

        #endregion

        #region UI Helpers

        private TableLayoutPanel CreateCard(string title)
        {
            var container = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.None, Margin = new Padding(8, 6, 8, 6), Padding = new Padding(0), ColumnCount = 1, RowCount = 2 };
            container.Paint += (s, e) => {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0))) g.FillRectangle(shadowBrush, new Rectangle(3, 3, container.Width - 3, container.Height - 3));
                using (var bgBrush = new SolidBrush(Color.White)) g.FillRectangle(bgBrush, new Rectangle(0, 0, container.Width - 3, container.Height - 3));
                using (var borderPen = new Pen(Color.FromArgb(30, 0, 0, 0), 1)) g.DrawRectangle(borderPen, new Rectangle(0, 0, container.Width - 4, container.Height - 4));
            };
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var titleBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250) };
            titleBar.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0) });
            container.Controls.Add(titleBar, 0, 0);
            return container;
        }

        private Button CreateButton(string text, Color backColor, Color foreColor)
        {
            var button = new Button { Text = text, Dock = DockStyle.Fill, Height = 44, BackColor = backColor, ForeColor = foreColor, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Margin = new Padding(0, 6, 0, 0), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0;
            button.MouseEnter += (s, e) => { if (button.Enabled) button.BackColor = ControlPaint.Dark(backColor, 0.1f); };
            button.MouseLeave += (s, e) => { button.BackColor = backColor; };
            return button;
        }

        private Label CreateLabel(string text) => new Label { Text = text, AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(52, 58, 64), Padding = new Padding(0, 8, 0, 4) };

        private Button CreateToggleButton(string text)
        {
            var button = new Button { Text = text, Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(245, 247, 250), ForeColor = Color.FromArgb(52, 58, 64), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0), Margin = new Padding(0, 6, 0, 4), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 212, 218);
            button.MouseEnter += (s, e) => { button.BackColor = Color.FromArgb(233, 236, 239); };
            button.MouseLeave += (s, e) => { button.BackColor = Color.FromArgb(245, 247, 250); };
            return button;
        }

        private Image CreatePlaceholderImage()
        {
            var bitmap = new Bitmap(PreviewImageSize.Width, PreviewImageSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(248, 249, 250));
                using var pen = new Pen(Color.FromArgb(206, 212, 218), 3);
                g.DrawRectangle(pen, 20, 20, PreviewImageSize.Width - 40, PreviewImageSize.Height - 40);
                using var font = new Font("Segoe UI", 16F, FontStyle.Bold);
                var text = "Sin imagen";
                var textSize = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.Gray, new PointF((PreviewImageSize.Width - textSize.Width) / 2, (PreviewImageSize.Height - textSize.Height) / 2));
            }
            return bitmap;
        }

        #endregion

        #region Data Loading

        private void LoadExercises(string? selectExerciseName = null)
        {
            _allExercises = _imageDatabase.GetAllExercises();
            MergeMetadata();
            FilterExercises(selectExerciseName);
        }

        private void MergeMetadata()
        {
            foreach (var exercise in _allExercises)
            {
                var metadata = _metadataStore.Get(exercise.ExerciseName);
                if (metadata == null) continue;
                if (!string.IsNullOrWhiteSpace(metadata.Description)) exercise.Description = metadata.Description;
                if (metadata.MuscleGroups is { Length: > 0 }) exercise.MuscleGroups = metadata.MuscleGroups;
            }
        }

        private void FilterExercises(string? selectExerciseName = null)
        {
            var query = searchBox.Text.Trim().ToLowerInvariant();
            var filtered = string.IsNullOrWhiteSpace(query) ? _allExercises : _allExercises.Where(ex => ex.ExerciseName.ToLowerInvariant().Contains(query) || (ex.MuscleGroups?.Any(m => m.ToLowerInvariant().Contains(query)) ?? false));
            var sorted = filtered.OrderBy(ex => ex.ExerciseName, StringComparer.OrdinalIgnoreCase).ToList();
            exerciseListBox.BeginUpdate();
            exerciseListBox.Items.Clear();
            foreach (var exercise in sorted) exerciseListBox.Items.Add(exercise);
            exerciseListBox.EndUpdate();
            if (!string.IsNullOrWhiteSpace(selectExerciseName))
            {
                for (int i = 0; i < exerciseListBox.Items.Count; i++)
                {
                    var ex = (ExerciseImageInfo)exerciseListBox.Items[i];
                    if (string.Equals(ex.ExerciseName, selectExerciseName, StringComparison.OrdinalIgnoreCase)) { exerciseListBox.SelectedIndex = i; return; }
                }
            }
            UpdateStatus($"{sorted.Count} ejercicio(s) encontrado(s)");
        }

        #endregion

        #region Event Handlers

        private void ExerciseListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_uiInitialized) return;
            if (exerciseListBox == null || exerciseListBox.SelectedItem is not ExerciseImageInfo exercise) { ShowExerciseDetails(null); return; }
            if (_isEditing)
            {
                var result = MessageBox.Show("Hay cambios sin guardar. Â¿Deseas descartarlos?", "Cambios sin guardar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) { _isLoadingEditor = true; exerciseListBox.SelectedItem = _currentExercise; _isLoadingEditor = false; return; }
            }
            ShowExerciseDetails(exercise);
            LoadExerciseIntoEditor(exercise);
        }

        private void ShowExerciseDetails(ExerciseImageInfo? exercise)
        {
            if (!_uiInitialized) return;
            _currentExercise = exercise;
            _isEditing = false;
            _imageChanged = false;
            _pendingImagePath = null;

            if (previewPictureBox == null) return;

            if (exercise == null)
            {
                previewPictureBox.Image = _placeholderImage;
                if (changeImageButton != null) changeImageButton.Enabled = false;
                if (changeVideoButton != null) changeVideoButton.Enabled = false;
                if (deleteExerciseButton != null) deleteExerciseButton.Enabled = false;
                DisableEditing();
                if (videoLabel != null) videoLabel.Visible = true;
                if (videoPanel != null)
                {
                    videoPanel.Visible = true;
                    LoadVideoInBrowser(null);
                }
                return;
            }

            LoadPreviewImage(exercise);
            if (changeImageButton != null) changeImageButton.Enabled = true;
            if (changeVideoButton != null) changeVideoButton.Enabled = true;
            if (deleteExerciseButton != null) deleteExerciseButton.Enabled = true;
            EnableEditing();
            
            if (videoPanel != null)
            {
                if (videoLabel != null) videoLabel.Visible = true;
                videoPanel.Visible = true;
                LoadVideoInBrowser(exercise.VideoUrl);
                try { videoPanel.BringToFront(); } catch { }
            }
        }

        private void LoadPreviewImage(ExerciseImageInfo exercise)
        {
            try
            {
                // Liberar imagen anterior si no es el placeholder
                var oldImage = previewPictureBox.Image;
                if (oldImage != null && oldImage != _placeholderImage)
                {
                    previewPictureBox.Image = _placeholderImage;
                    oldImage.Dispose();
                }

                // Intentar cargar la imagen
                if (exercise.ImageData != null && exercise.ImageData.Length > 0)
                {
                    // Mantener el MemoryStream vivo - NO usar using aquí
                    var ms = new MemoryStream(exercise.ImageData);
                    var img = Image.FromStream(ms);
                    previewPictureBox.Image = img;
                }
                else if (!string.IsNullOrWhiteSpace(exercise.ImagePath) && File.Exists(exercise.ImagePath))
                {
                    var bytes = File.ReadAllBytes(exercise.ImagePath);
                    var ms = new MemoryStream(bytes);
                    var img = Image.FromStream(ms);
                    previewPictureBox.Image = img;
                }
                else
                {
                    previewPictureBox.Image = _placeholderImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridManager] Error loading preview: {ex.Message}");
                try { previewPictureBox.Image = _placeholderImage; } catch { }
            }
        }

        private void LoadVideoInBrowser(string? videoUrl)
        {
            if (videoWebBrowser == null) return;
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                videoWebBrowser.DocumentText = @"
                <html>
                <head>
                    <style>
                        body { margin: 0; padding: 0; background-color: #121212; display: flex; align-items: center; justify-content: center; height: 100vh; font-family: 'Segoe UI', sans-serif; color: #555; }
                        .content { text-align: center; }
                        .icon { font-size: 50px; margin-bottom: 10px; display: block; opacity: 0.3; }
                    </style>
                </head>
                <body>
                    <div class='content'>
                        <span class='icon'>📹</span>
                        <span>Sin video disponible</span>
                    </div>
                </body>
                </html>";
                return;
            }
            try
            {
                string videoId = GetYouTubeVideoId(videoUrl);
                bool isInstagram = videoUrl.Contains("instagram.com");
                
                string thumbUrl = "";
                string platformName = "YouTube";
                string brandColor = "#FF0000"; 
                string buttonHtml = "<div class='play-triangle'></div>";
                string buttonExtraStyle = "";

                if (!string.IsNullOrEmpty(videoId)) {
                    thumbUrl = $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
                } else if (isInstagram) {
                    thumbUrl = "https://www.instagram.com/static/images/ico/favicon-192.png/b4071dc4fab7.png";
                    platformName = "Instagram Reel";
                    brandColor = "#E1306C"; 
                    buttonHtml = @"<svg viewBox='0 0 24 24' width='32' height='32' fill='white'><path d='M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-5.838 2.435-5.838 5.838s2.435 5.838 5.838 5.838 5.838-2.435 5.838-5.838-2.435-5.838-5.838-5.838zm0 9.513c-2.03 0-3.675-1.645-3.675-3.675 0-2.03 1.645-3.675 3.675-3.675 2.03 0 3.675 1.645 3.675 3.675 0 2.03-1.645 3.675-3.675 3.675zm4.961-10.405c.73 0 1.322.592 1.322 1.322 0 .73-.592 1.322-1.322 1.322-.73 0-1.322-.592-1.322-1.322 0-.73.592-1.322 1.322-1.322z'/></svg>";
                    buttonExtraStyle = "width: 65px; height: 65px; border-radius: 20px; background: linear-gradient(45deg, #f09433 0%,#e6683c 25%,#dc2743 50%,#cc2366 75%,#bc1888 100%);";
                } else {
                    thumbUrl = "https://www.youtube.com/img/desktop/yt_1200.png";
                }

                string html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                    <meta charset='utf-8' />
                    <style>
                        html, body {{ margin: 0; padding: 0; width: 100%; height: 100%; overflow: hidden; background-color: #000; cursor: pointer; font-family: 'Segoe UI', sans-serif; }}
                        .wrapper {{ position: relative; width: 100%; height: 100%; background: #000; display: flex; align-items: center; justify-content: center; }}
                        .bg-blur {{ position: absolute; width: 110%; height: 110%; top: -5%; left: -5%; background-image: url('{thumbUrl}'); background-size: cover; background-position: center; filter: blur(20px) brightness(0.3); opacity: 0.5; }}
                        .thumbnail {{ position: relative; z-index: 2; max-width: 100%; max-height: 100%; object-fit: contain; transition: all 0.3s ease; box-shadow: 0 0 30px rgba(0,0,0,0.5); }}
                        .overlay {{ position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 10; display: flex; flex-direction: column; align-items: center; justify-content: center; background: rgba(0,0,0,0.2); transition: background 0.3s; }}
                        .wrapper:hover .overlay {{ background: rgba(0,0,0,0); }}
                        .wrapper:hover .thumbnail {{ transform: scale(1.02); }}
                        
                        .play-button {{ 
                            width: 70px; height: 50px; 
                            background: {brandColor}; border-radius: 12px; 
                            display: flex; align-items: center; justify-content: center;
                            transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
                            box-shadow: 0 4px 15px rgba(0,0,0,0.4);
                            {buttonExtraStyle}
                        }}
                        .wrapper:hover .play-button {{ transform: scale(1.15); box-shadow: 0 0 25px {brandColor}80; }}
                        
                        .play-triangle {{ 
                            width: 0; height: 0; 
                            border-top: 10px solid transparent; border-bottom: 10px solid transparent; 
                            border-left: 18px solid white; margin-left: 4px;
                        }}
                        
                        .brand-tag {{ 
                            margin-top: 15px; color: white; font-weight: bold; 
                            font-size: 12px; text-transform: uppercase; letter-spacing: 2px;
                            background: rgba(0,0,0,0.6); padding: 5px 15px; border-radius: 20px;
                            border: 1px solid rgba(255,255,255,0.2);
                        }}
                        
                        .hint {{ position: absolute; bottom: 15px; color: rgba(255,255,255,0.6); font-size: 11px; z-index: 11; letter-spacing: 0.5px; text-shadow: 0 1px 2px #000; }}
                    </style>
                </head>
                <body onclick=""window.location.href='{videoUrl.Replace("'", "\\'")}'"">
                    <div class='wrapper'>
                        <div class='bg-blur'></div>
                        <img class='thumbnail' src='{thumbUrl}'>
                        <div class='overlay'>
                            <div class='play-button'>{buttonHtml}</div>
                            <div class='brand-tag'>{platformName}</div>
                            <div class='hint'>CLICK PARA REPRODUCIR</div>
                        </div>
                    </div>
                </body>
                </html>";
                
                videoWebBrowser.DocumentText = html;
            }
            catch (Exception ex) { Debug.WriteLine($"[HybridExerciseManagerForm] Error cargando preview: {ex.Message}"); }
        }

        private string GetYouTubeVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            try {
                var match = System.Text.RegularExpressions.Regex.Match(url, @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/|youtube\.com\/shorts\/)([^""&?\/\s]{11})");
                return match.Success ? match.Groups[1].Value : "";
            } catch { return ""; }
        }

        private string ConvertToEmbedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            url = url.Trim();

            if (url.Contains("youtube.com") || url.Contains("youtu.be"))
            {
                string videoId = "";
                if (url.Contains("youtube.com/watch?v=")) 
                { 
                    var startIndex = url.IndexOf("?v=") + 3; 
                    var endIndex = url.IndexOf("&", startIndex);
                    videoId = endIndex > startIndex ? url.Substring(startIndex, endIndex - startIndex) : url.Substring(startIndex);
                } 
                else if (url.Contains("youtu.be/")) 
                { 
                    var startIndex = url.LastIndexOf('/') + 1; 
                    var endIndex = url.IndexOf("?", startIndex);
                    videoId = endIndex > startIndex ? url.Substring(startIndex, endIndex - startIndex) : url.Substring(startIndex);
                } 
                else if (url.Contains("youtube.com/shorts/"))
                {
                    var startIndex = url.IndexOf("/shorts/") + 8;
                    var endIndex = url.IndexOf("?", startIndex);
                    videoId = endIndex > startIndex ? url.Substring(startIndex, endIndex - startIndex) : url.Substring(startIndex);
                }
                else if (url.Contains("youtube.com/embed/")) return url;

                if (!string.IsNullOrEmpty(videoId)) 
                    // Usar youtube-nocookie para evitar bloqueos de compatibilidad
                    return $"https://www.youtube-nocookie.com/embed/{videoId.Trim()}?rel=0";
            }

            if (url.Contains("vimeo.com")) 
            { 
                var parts = url.Split('/'); 
                var videoId = parts[^1].Split('?')[0]; 
                if (!string.IsNullOrEmpty(videoId) && long.TryParse(videoId, out _)) 
                    return $"https://player.vimeo.com/video/{videoId}"; 
            }

            return url;
        }

        #endregion

        #region Editing Actions

        private void EnableEditing()
        {
            if (editNameTextBox != null) editNameTextBox.Enabled = true;
            if (editDescriptionTextBox != null) editDescriptionTextBox.Enabled = true;
            if (muscleGroupsCheckedListBox != null) muscleGroupsCheckedListBox.Enabled = true;
            if (editVideoUrlTextBox != null) editVideoUrlTextBox.Enabled = true;
            if (saveButton != null) saveButton.Enabled = false;
            if (cancelButton != null) cancelButton.Enabled = true;
        }

        private void DisableEditing()
        {
            if (editNameTextBox != null) { editNameTextBox.Enabled = false; editNameTextBox.Text = ""; }
            if (editDescriptionTextBox != null) { editDescriptionTextBox.Enabled = false; editDescriptionTextBox.Text = ""; }
            if (muscleGroupsCheckedListBox != null) { muscleGroupsCheckedListBox.Enabled = false; for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++) muscleGroupsCheckedListBox.SetItemChecked(i, false); }
            if (editVideoUrlTextBox != null) { editVideoUrlTextBox.Enabled = false; }
            if (saveButton != null) saveButton.Enabled = false;
            if (cancelButton != null) cancelButton.Enabled = false;
        }

        private void OnFieldChanged(object? sender, EventArgs e) { if (!_isLoadingEditor) EnableSaveButtons(); }
        private void OnMuscleGroupCheck(object? sender, ItemCheckEventArgs e) { if (!_isLoadingEditor) BeginInvoke(new Action(() => EnableSaveButtons())); }

        private void EnableSaveButtons() { _isEditing = true; if (saveButton != null) saveButton.Enabled = true; if (cancelButton != null) cancelButton.Enabled = true; }

        private void SaveCurrentExercise()
        {
            if (_currentExercise == null) return;
            var newName = editNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName)) { MessageBox.Show("El nombre del ejercicio no puede estar vacio.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning); editNameTextBox.Focus(); return; }
            var description = editDescriptionTextBox.Text.Trim();
            var muscles = muscleGroupsCheckedListBox.CheckedItems.Cast<string>().ToArray();
            var videoUrl = editVideoUrlTextBox.Text.Trim();

            // Guardar imagen si cambio
            if (_imageChanged && !string.IsNullOrEmpty(_pendingImagePath))
            {
                _imageDatabase.ImportImageForExercise(_currentExercise.ExerciseName, _pendingImagePath);
            }

            _isEditing = false; _imageChanged = false; _pendingImagePath = null;
            
            var success = _imageDatabase.UpdateExerciseDetails(_currentExercise.ExerciseName, newName, description, muscles, Array.Empty<string>(), string.Empty, videoUrl);
            if (!success) { MessageBox.Show("No se pudieron guardar los cambios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            _metadataStore.Upsert(new ExerciseMetadataRecord { Name = newName, Description = description, MuscleGroups = muscles, Keywords = Array.Empty<string>(), Source = string.Empty }, _currentExercise.ExerciseName);
            UpdateStatus("✓ Cambios guardados correctamente");
            
            LoadExercises(newName);
            if (saveButton != null) saveButton.Enabled = false;
        }

        private void CancelEdit() { if (_currentExercise == null) return; _isEditing = false; _imageChanged = false; _pendingImagePath = null; LoadExerciseIntoEditor(_currentExercise); LoadPreviewImage(_currentExercise); if (saveButton != null) saveButton.Enabled = false; UpdateStatus("Cambios descartados"); }

        private void LoadExerciseIntoEditor(ExerciseImageInfo exercise)
        {
            _isLoadingEditor = true;
            if (editNameTextBox != null) editNameTextBox.Text = exercise.ExerciseName;
            if (editDescriptionTextBox != null) editDescriptionTextBox.Text = exercise.Description ?? "";
            if (editVideoUrlTextBox != null) editVideoUrlTextBox.Text = exercise.VideoUrl ?? "";
            if (muscleGroupsCheckedListBox != null)
            {
                for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++) muscleGroupsCheckedListBox.SetItemChecked(i, false);
                if (exercise.MuscleGroups != null && exercise.MuscleGroups.Length > 0) foreach (var muscle in exercise.MuscleGroups) { var index = muscleGroupsCheckedListBox.Items.IndexOf(muscle); if (index >= 0) muscleGroupsCheckedListBox.SetItemChecked(index, true); }
            }
            _isLoadingEditor = false;
        }

        #endregion

        #region Exercise Management

        private void CreateNewExercise()
        {
            using var dialog = new AddExerciseDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(dialog.ExerciseName)) return;
            var name = dialog.ExerciseName.Trim();
            if (!_imageDatabase.AddOrUpdateExercise(name, string.Empty, dialog.Keywords, dialog.MuscleGroups, dialog.Description)) { MessageBox.Show("No se pudo crear el ejercicio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            _metadataStore.Upsert(new ExerciseMetadataRecord { Name = name, Description = dialog.Description, MuscleGroups = dialog.MuscleGroups, Keywords = Array.Empty<string>(), Source = string.Empty });
            UpdateStatus($"✓ Ejercicio '{name}' creado correctamente");
            LoadExercises(name);
        }

        private void DeleteCurrentExercise()
        {
            if (_currentExercise == null) return;
            var result = MessageBox.Show($"¿Estas seguro de eliminar '{_currentExercise.ExerciseName}'?\n\nEsta accion no se puede deshacer.", "Confirmar eliminacion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No) return;
            if (_imageDatabase.RemoveExercise(_currentExercise.ExerciseName)) { _metadataStore.Delete(_currentExercise.ExerciseName); UpdateStatus($"✓ Ejercicio '{_currentExercise.ExerciseName}' eliminado"); LoadExercises(); }
            else MessageBox.Show("No se pudo eliminar el ejercicio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ImportImageFromDisk()
        {
            if (_currentExercise == null) return;
            using var dialog = new OpenFileDialog { Title = "Seleccionar imagen del ejercicio", Filter = "Imagenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|Todos los archivos|*.*", FilterIndex = 1 };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            if (!SafeLoadImageToPreview(dialog.FileName)) return;

            _pendingImagePath = dialog.FileName;
            _imageChanged = true;
            EnableSaveButtons();
            UpdateStatus("Imagen seleccionada (haz clic en Guardar para confirmar)");
        }

        private void OpenVideoFolder()
        {
            if (_currentExercise == null) return;
            string? path = _currentExercise.VideoUrl;

            // Si no hay video o es una URL (no archivo local), abrir diálogo para seleccionar video
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                using var dialog = new OpenFileDialog
                {
                    Title = "Seleccionar video del ejercicio",
                    Filter = "Videos|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.webm|Todos los archivos|*.*",
                    FilterIndex = 1
                };
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                editVideoUrlTextBox.Text = dialog.FileName;
                LoadVideoInBrowser(dialog.FileName);
                EnableSaveButtons();
                UpdateStatus("Video seleccionado (haz clic en Guardar para confirmar)");
                return;
            }
            Process.Start("explorer.exe", $"/select,\"{path}\"");
        }

        private void OpenImageFolder()
        {
            if (_currentExercise == null) return;
            var path = _currentExercise.ImagePath;
            bool hasImage = false;

            // Verificar si tiene imagen en path o en ImageData
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                hasImage = true;
            }
            else if (_currentExercise.ImageData != null && _currentExercise.ImageData.Length > 0)
            {
                _exportedTempImage = ExportImageToTemp(_currentExercise.ImageData, _currentExercise.ExerciseName);
                path = _exportedTempImage;
                hasImage = !string.IsNullOrWhiteSpace(path) && File.Exists(path);
            }

            // Si no hay imagen, abrir diálogo para seleccionar una
            if (!hasImage)
            {
                ImportImageFromDisk();
                return;
            }

            Process.Start("explorer.exe", $"/select,\"{path}\"");
        }

        private void OpenVideoButton_Click()
        {
            if (_currentExercise == null || string.IsNullOrWhiteSpace(editVideoUrlTextBox.Text)) { MessageBox.Show("No hay video disponible para este ejercicio.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            try { Process.Start(new ProcessStartInfo { FileName = editVideoUrlTextBox.Text, UseShellExecute = true }); } 
            catch (Exception ex) { MessageBox.Show($"No se pudo abrir el video.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        #endregion

        #region Drag & Drop de Imagenes

        private void PreviewPictureBox_DragEnter(object? sender, DragEventArgs e) { if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) { var files = (string[]?)e.Data.GetData(DataFormats.FileDrop); if (files != null && files.Length > 0) { var ext = Path.GetExtension(files[0]).ToLowerInvariant(); if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif" || ext == ".webp") { e.Effect = DragDropEffects.Copy; return; } } } e.Effect = DragDropEffects.None; }

        private void PreviewPictureBox_DragDrop(object? sender, DragEventArgs e)
        {
            if (_currentExercise == null) { MessageBox.Show("Selecciona un ejercicio primero.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) != true) return;
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            var imagePath = files[0];

            if (!SafeLoadImageToPreview(imagePath)) return;

            _pendingImagePath = imagePath;
            _imageChanged = true;
            EnableSaveButtons();
            UpdateStatus("Imagen seleccionada (arrastrar y soltar). Haz clic en Guardar.");
        }

        #endregion

        #region Status

        private void UpdateStatus(string message) { statusLabel.Text = message; }

        private string? ExportImageToTemp(byte[] imageData, string exerciseName) { try { var tempPath = Path.Combine(Path.GetTempPath(), $"{exerciseName}_{Guid.NewGuid()}.jpg"); File.WriteAllBytes(tempPath, imageData); return tempPath; } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error exporting image to temp: {ex.Message}"); return null; } }

        #endregion

        #region Image Loading Helpers

        /// <summary>
        /// Carga una imagen de forma segura, soportando WebP y otros formatos.
        /// </summary>
        private bool SafeLoadImageToPreview(string imagePath)
        {
            string errorDetail = "";
            try
            {
                // Liberar imagen anterior
                var oldImage = previewPictureBox.Image;
                if (oldImage != null && oldImage != _placeholderImage)
                {
                    previewPictureBox.Image = _placeholderImage;
                    oldImage.Dispose();
                }

                // Intentar cargar la imagen
                var (image, error) = LoadImageWithWebPSupport(imagePath);
                errorDetail = error;

                if (image == null)
                {
                    MessageBox.Show($"No se pudo cargar la imagen.\n\n{errorDetail}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    previewPictureBox.Image = _placeholderImage;
                    return false;
                }

                previewPictureBox.Image = image;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar la imagen:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                previewPictureBox.Image = _placeholderImage;
                return false;
            }
        }

        /// <summary>
        /// Carga imagen con soporte para WebP usando ImageSharp.
        /// </summary>
        private (Image? image, string error) LoadImageWithWebPSupport(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            var ext = Path.GetExtension(imagePath).ToLowerInvariant();
            string lastError = "";

            // Primero intentar con System.Drawing estándar (más rápido para JPG/PNG)
            if (ext != ".webp")
            {
                try
                {
                    var ms = new MemoryStream(bytes);
                    var img = Image.FromStream(ms);
                    return (img, "");
                }
                catch (Exception ex)
                {
                    lastError = $"System.Drawing: {ex.Message}";
                }
            }

            // Usar ImageSharp para WebP y como fallback
            try
            {
                var img = LoadImageUsingImageSharp(bytes);
                if (img != null) return (img, "");
                lastError += "\nImageSharp: No pudo decodificar";
            }
            catch (Exception ex)
            {
                lastError += $"\nImageSharp: {ex.Message}";
            }

            return (null, $"Formato: {ext}\nTamaño: {bytes.Length} bytes\n\nErrores:\n{lastError}");
        }

        /// <summary>
        /// Carga imagen usando ImageSharp (soporta WebP, PNG, JPG, BMP, GIF, etc).
        /// </summary>
        private Image? LoadImageUsingImageSharp(byte[] imageBytes)
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var image = SixLabors.ImageSharp.Image.Load(inputStream);

            // Convertir a PNG en memoria para System.Drawing
            using var outputStream = new MemoryStream();
            image.Save(outputStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());

            return Image.FromStream(new MemoryStream(outputStream.ToArray()));
        }

        #endregion
    }
}