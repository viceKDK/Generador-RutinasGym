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
    /// Gestor híbrido: Lista+Preview de SimplifiedForm + Editor avanzado de ImprovedForm
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
        private TextBox searchBox = null!;  // Barra búsqueda ejercicios
        private PictureBox previewPictureBox = null!;
        private WebBrowser videoWebBrowser = null!;  // NUEVO: WebBrowser para video embebido
        private Panel videoPanel = null!;            // NUEVO: Panel contenedor del video
        private Label videoLabel = null!;             // NUEVO: Label "Video"
        private Label exerciseNameLabel = null!;
        private Label muscleGroupsLabel = null!;
        private TextBox editNameTextBox = null!;
        private TextBox editDescriptionTextBox = null!;
        private TextBox editVideoUrlTextBox = null!;  // NUEVO: Campo para link de video
        private CheckedListBox muscleGroupsCheckedListBox = null!;  // Editor avanzado
        private Panel musclesContentPanel = null!;
        private Panel descriptionContentPanel = null!;
        private Panel videoContentPanel = null!;  // NUEVO: Panel para video URL
        private Button musclesToggleButton = null!;
        private Button descriptionToggleButton = null!;
        private Button videoToggleButton = null!;  // NUEVO: Toggle para video
        private Button saveButton = null!;
        private Button cancelButton = null!;
        private Button changeImageButton = null!;
        private Button openFolderButton = null!;
        private Button newExerciseButton = null!;
        private Button deleteExerciseButton = null!;
        private Button openVideoButton = null!;  // NUEVO: Botón para abrir video
        private ToolStripStatusLabel statusLabel = null!;

        private List<ExerciseImageInfo> _allExercises = new();
        private ExerciseImageInfo? _currentExercise;
        private bool _isEditing;
        private bool _isLoadingEditor;
        private Image? _placeholderImage;
        private string? _exportedTempImage;

        private HybridExerciseManagerForm()
        {
            _imageDatabase = new SQLiteExerciseImageDatabase();
            _metadataStore = new ExerciseMetadataStore(AppDomain.CurrentDomain.BaseDirectory);
            InitializeComponent();

            // Configurar agrupamiento en barra de tareas
            TaskbarGroupingHelper.ConfigureFormGrouping(this);

            LoadExercises();
        }

        /// <summary>
        /// Muestra la única instancia del formulario o la crea si no existe
        /// </summary>
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
                // Si ya está abierto, traerlo al frente
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
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 10F);

            // Atajo: ESC para volver
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

            // Layout principal - 3 columnas
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(12)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));  // Lista
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));    // Preview
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));    // Edición

            mainLayout.Controls.Add(BuildListColumn(), 0, 0);
            mainLayout.Controls.Add(BuildPreviewColumn(), 1, 0);
            mainLayout.Controls.Add(BuildEditColumn(), 2, 0);

            Controls.Add(mainLayout);

            // Status bar
            var statusStrip = new StatusStrip
            {
                BackColor = Color.White,
                SizingGrip = false
            };
            statusLabel = new ToolStripStatusLabel
            {
                Text = "Selecciona un ejercicio para ver detalles",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusStrip.Items.Add(statusLabel);
            Controls.Add(statusStrip);
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
                Padding = new Padding(8, 8, 8, 8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));  // Búsqueda (altura fija)
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Lista
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // Botón nuevo (altura fija)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // Botón eliminar (altura fija)

            // Búsqueda de ejercicios
            searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Buscar ejercicio...",
                Font = new Font("Segoe UI", 11F),
                Height = 35,
                Margin = new Padding(0, 4, 0, 4)
            };
            searchBox.TextChanged += (s, e) => FilterExercises();
            layout.Controls.Add(searchBox, 0, 0);

            // Lista
            exerciseListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "ExerciseName"
            };
            exerciseListBox.SelectedIndexChanged += ExerciseListBox_SelectedIndexChanged;
            layout.Controls.Add(exerciseListBox, 0, 1);

            // Botón nuevo
            newExerciseButton = CreateButton("Nuevo ejercicio", Color.FromArgb(40, 167, 69), Color.White);
            newExerciseButton.Click += (s, e) => CreateNewExercise();
            layout.Controls.Add(newExerciseButton, 0, 2);

            // Botón eliminar
            deleteExerciseButton = CreateButton("Eliminar", Color.FromArgb(220, 53, 69), Color.White);
            deleteExerciseButton.Click += (s, e) => DeleteCurrentExercise();
            deleteExerciseButton.Enabled = false;
            layout.Controls.Add(deleteExerciseButton, 0, 3);

            card.Controls.Add(layout, 0, 1);  // Agregar al row 1 (contenido)
            return card;
        }

        private Control BuildPreviewColumn()
        {
            var card = CreateCard("Vista Previa");

            // Panel con scroll para evitar que los componentes se escondan
            var scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                RowCount = 7,
                ColumnCount = 1,
                Padding = new Padding(12)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));  // Imagen (altura reducida)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Nombre
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Músculos
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // NUEVO: Label "Video"
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 280));  // NUEVO: WebBrowser video (altura ajustada)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Botón cambiar imagen
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Botón carpeta

            // Imagen preview con drag & drop
            previewPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Image = _placeholderImage,
                AllowDrop = true
            };
            previewPictureBox.DragEnter += PreviewPictureBox_DragEnter;
            previewPictureBox.DragDrop += PreviewPictureBox_DragDrop;
            layout.Controls.Add(previewPictureBox, 0, 0);

            // Nombre del ejercicio
            exerciseNameLabel = new Label
            {
                Text = "Selecciona un ejercicio",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 6, 0, 4)
            };
            layout.Controls.Add(exerciseNameLabel, 0, 1);

            // Grupos musculares
            muscleGroupsLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(108, 117, 125),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 6)
            };
            layout.Controls.Add(muscleGroupsLabel, 0, 2);

            // NUEVO: Label "Video"
            videoLabel = new Label
            {
                Text = "Video del ejercicio",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 6, 0, 4),
                Visible = false  // Oculto por defecto
            };
            layout.Controls.Add(videoLabel, 0, 3);

            // NUEVO: Panel contenedor del video con WebBrowser
            videoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                Visible = false,  // Oculto por defecto
                Padding = new Padding(0)
            };

            videoWebBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
                ScrollBarsEnabled = false,
                AllowNavigation = true,
                IsWebBrowserContextMenuEnabled = false
            };

            // Botón para recargar video (superpuesto en esquina superior derecha)
            var refreshVideoButton = new Button
            {
                Text = "⟳",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Size = new Size(40, 40),
                BackColor = Color.FromArgb(13, 110, 253),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            refreshVideoButton.FlatAppearance.BorderSize = 0;
            refreshVideoButton.Click += (s, e) => LoadVideoInBrowser(_currentExercise?.VideoUrl);
            // Mantener el botón anclado a la esquina superior derecha del panel de video.
            refreshVideoButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Posicionar correctamente aún cuando el panel cambie de tamaño.
            videoPanel.SizeChanged += (s, e) =>
            {
                try
                {
                    // Recalcular la posición dentro del panel (10px de margen desde la derecha y arriba)
                    refreshVideoButton.Location = new Point(Math.Max(10, videoPanel.ClientSize.Width - refreshVideoButton.Width - 10), 10);
                    refreshVideoButton.BringToFront();
                }
                catch { /* tolerar excepciones de layout durante inicialización */ }
            };

            videoPanel.Controls.Add(videoWebBrowser);
            videoPanel.Controls.Add(refreshVideoButton);
            layout.Controls.Add(videoPanel, 0, 4);
            // Asegurar z-order: traer al frente inmediatamente y cuando cambie visibilidad o tamaño del contenedor
            try { videoPanel.BringToFront(); videoLabel.BringToFront(); } catch { }
            videoPanel.VisibleChanged += (s, e) => { try { if (videoPanel.Visible) { videoPanel.BringToFront(); videoLabel.BringToFront(); } } catch { } };
            // Cuando el contenedor cambie de tamaño (scrollContainer/layout), asegurar el panel en primer plano
            scrollContainer.SizeChanged += (s, e) => { try { if (videoPanel.Visible) videoPanel.BringToFront(); } catch { } };
            layout.Controls.Add(videoPanel, 0, 4);

            // Botón cambiar imagen (ahora después del video)
            changeImageButton = CreateButton("Cambiar imagen", Color.FromArgb(13, 110, 253), Color.White);
            // No usar Fill para que no ocupe espacio extra y potencialmente solape el panel de video.
            changeImageButton.Dock = DockStyle.Top;
            changeImageButton.Height = 36;
            changeImageButton.Margin = new Padding(0, 8, 0, 6);
            changeImageButton.Click += (s, e) => ImportImageFromDisk();
            changeImageButton.Enabled = false;
            layout.Controls.Add(changeImageButton, 0, 5);

            // Botón abrir carpeta (ahora después del video)
            openFolderButton = CreateButton("Abrir carpeta de imágenes", Color.FromArgb(108, 117, 125), Color.White);
            openFolderButton.Dock = DockStyle.Top;
            openFolderButton.Height = 36;
            openFolderButton.Margin = new Padding(0, 2, 0, 8);
            openFolderButton.Click += (s, e) => OpenImageFolder();
            openFolderButton.Enabled = false;
            layout.Controls.Add(openFolderButton, 0, 6);

            // Agregar layout al scroll container
            scrollContainer.Controls.Add(layout);

            // Agregar scroll container al card
            card.Controls.Add(scrollContainer, 0, 1);
            return card;
        }

        private Control BuildEditColumn()
        {
            // EDITOR AVANZADO con secciones colapsables (como ImprovedForm)
            var card = CreateCard("Edición");

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0)
            };

            // Nombre
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var nameLabel = CreateLabel("Nombre del ejercicio:");
            layout.Controls.Add(nameLabel, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            editNameTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11F),
                Height = 32,
                Margin = new Padding(0, 4, 0, 12)
            };
            layout.Controls.Add(editNameTextBox, 0, layout.RowCount - 1);

            // Grupos musculares (collapsible)
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            musclesToggleButton = CreateToggleButton("▼ Grupos musculares");
            musclesToggleButton.Click += (s, e) => ToggleSection(musclesToggleButton, musclesContentPanel);
            layout.Controls.Add(musclesToggleButton, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            musclesContentPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Visible = true,
                Padding = new Padding(0)
            };

            muscleGroupsCheckedListBox = new CheckedListBox
            {
                Dock = DockStyle.Top,
                CheckOnClick = true,
                IntegralHeight = false,
                Height = 180,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 4, 0, 12)
            };
            muscleGroupsCheckedListBox.Items.AddRange(_defaultMuscleGroups.Cast<object>().ToArray());
            musclesContentPanel.Controls.Add(muscleGroupsCheckedListBox);
            layout.Controls.Add(musclesContentPanel, 0, layout.RowCount - 1);

            // Descripción (collapsible)
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            descriptionToggleButton = CreateToggleButton("▼ Descripción");
            descriptionToggleButton.Click += (s, e) => ToggleSection(descriptionToggleButton, descriptionContentPanel);
            layout.Controls.Add(descriptionToggleButton, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            descriptionContentPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Visible = true,
                Padding = new Padding(0)
            };

            editDescriptionTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                Height = 80,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 4, 0, 12)
            };
            descriptionContentPanel.Controls.Add(editDescriptionTextBox);
            layout.Controls.Add(descriptionContentPanel, 0, layout.RowCount - 1);

            // NUEVO: Link de video (collapsible)
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            videoToggleButton = CreateToggleButton("▼ Link de video (opcional)");
            videoToggleButton.Click += (s, e) => ToggleSection(videoToggleButton, videoContentPanel);
            layout.Controls.Add(videoToggleButton, 0, layout.RowCount - 1);

            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            videoContentPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Visible = true,
                Padding = new Padding(0)
            };

            editVideoUrlTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F),
                Height = 32,
                PlaceholderText = "https://www.youtube.com/watch?v=...",
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 0, 4)
            };
            videoContentPanel.Controls.Add(editVideoUrlTextBox);

            // Botón para abrir video en navegador
            openVideoButton = CreateButton("Abrir video en navegador", Color.FromArgb(13, 110, 253), Color.White);
            openVideoButton.Dock = DockStyle.Top;
            openVideoButton.Height = 36;
            openVideoButton.Margin = new Padding(0, 4, 0, 12);
            openVideoButton.Click += (s, e) => OpenVideoButton_Click();
            videoContentPanel.Controls.Add(openVideoButton);

            layout.Controls.Add(videoContentPanel, 0, layout.RowCount - 1);

            // Botones de acción
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0)
            };

            saveButton = CreateButton("Guardar cambios", Color.FromArgb(25, 135, 84), Color.White);
            saveButton.Click += (s, e) => SaveCurrentExercise();
            saveButton.Enabled = false;
            buttonsPanel.Controls.Add(saveButton);

            cancelButton = CreateButton("Cancelar", Color.FromArgb(108, 117, 125), Color.White);
            cancelButton.Click += (s, e) => CancelEdit();
            cancelButton.Enabled = false;
            buttonsPanel.Controls.Add(cancelButton);

            layout.Controls.Add(buttonsPanel, 0, layout.RowCount - 1);

            scrollPanel.Controls.Add(layout);
            card.Controls.Add(scrollPanel, 0, 1);
            return card;
        }

        private Control BuildCollapsibleSection(string title, out Button toggleButton, out Panel contentPanel)
        {
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var button = new Button
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"{title} ▾",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(233, 236, 239),
                ForeColor = Color.FromArgb(52, 58, 64),
                Padding = new Padding(8, 4, 8, 4)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 212, 218);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(248, 249, 250),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            button.Click += (_, _) => ToggleSection(button, panel);

            container.Controls.Add(button, 0, 0);
            container.Controls.Add(panel, 0, 1);

            toggleButton = button;
            contentPanel = panel;
            return container;
        }

        private void ToggleSection(Button button, Panel panel)
        {
            panel.Visible = !panel.Visible;
            button.Text = panel.Visible
                ? button.Text.Replace("▸", "▾")
                : button.Text.Replace("▾", "▸");
        }

        #endregion

        #region UI Helpers

        private TableLayoutPanel CreateCard(string title)
        {
            // Crear container principal con 2 rows: título + contenido
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(6),
                Padding = new Padding(0),
                ColumnCount = 1,
                RowCount = 2
            };
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));  // Row 0: Título (45px fijo)
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Row 1: Contenido (resto del espacio)

            var titleBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(233, 236, 239)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            titleBar.Controls.Add(titleLabel);
            container.Controls.Add(titleBar, 0, 0);  // Agregar título a row 0

            return container;
        }

        private Button CreateButton(string text, Color backColor, Color foreColor)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Height = 40,
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 6, 0, 0),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                Padding = new Padding(0, 8, 0, 4)
            };
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                Padding = new Padding(0, 8, 0, 4)
            };
        }

        private Button CreateToggleButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(52, 58, 64),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 4, 0, 4),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 212, 218);
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
                g.DrawString(text, font, Brushes.Gray,
                    new PointF((PreviewImageSize.Width - textSize.Width) / 2,
                              (PreviewImageSize.Height - textSize.Height) / 2));
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

                if (!string.IsNullOrWhiteSpace(metadata.Description))
                    exercise.Description = metadata.Description;

                if (metadata.MuscleGroups is { Length: > 0 })
                    exercise.MuscleGroups = metadata.MuscleGroups;
            }
        }

        private void FilterExercises(string? selectExerciseName = null)
        {
            var query = searchBox.Text.Trim().ToLowerInvariant();
            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allExercises
                : _allExercises.Where(ex =>
                    ex.ExerciseName.ToLowerInvariant().Contains(query) ||
                    (ex.MuscleGroups?.Any(m => m.ToLowerInvariant().Contains(query)) ?? false));

            var sorted = filtered.OrderBy(ex => ex.ExerciseName, StringComparer.OrdinalIgnoreCase).ToList();

            exerciseListBox.BeginUpdate();
            exerciseListBox.Items.Clear();
            foreach (var exercise in sorted)
            {
                exerciseListBox.Items.Add(exercise);
            }
            exerciseListBox.EndUpdate();

            // Seleccionar ejercicio si se especifica
            if (!string.IsNullOrWhiteSpace(selectExerciseName))
            {
                for (int i = 0; i < exerciseListBox.Items.Count; i++)
                {
                    var ex = (ExerciseImageInfo)exerciseListBox.Items[i];
                    if (string.Equals(ex.ExerciseName, selectExerciseName, StringComparison.OrdinalIgnoreCase))
                    {
                        exerciseListBox.SelectedIndex = i;
                        return;
                    }
                }
            }

            UpdateStatus($"{sorted.Count} ejercicio(s) encontrado(s)");
        }

        #endregion

        #region Event Handlers

        private void ExerciseListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (exerciseListBox.SelectedItem is not ExerciseImageInfo exercise)
            {
                ShowExerciseDetails(null);
                return;
            }

            // Si hay cambios sin guardar, preguntar
            if (_isEditing)
            {
                var result = MessageBox.Show(
                    "Hay cambios sin guardar. ¿Deseas descartarlos?",
                    "Cambios sin guardar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    // Revertir selección
                    _isLoadingEditor = true;
                    exerciseListBox.SelectedItem = _currentExercise;
                    _isLoadingEditor = false;
                    return;
                }
            }

            ShowExerciseDetails(exercise);
            LoadExerciseIntoEditor(exercise);
        }

        private void ShowExerciseDetails(ExerciseImageInfo? exercise)
        {
            _currentExercise = exercise;
            _isEditing = false;

            if (exercise == null)
            {
                previewPictureBox.Image = _placeholderImage;
                exerciseNameLabel.Text = "Selecciona un ejercicio";
                muscleGroupsLabel.Text = "";
                changeImageButton.Enabled = false;
                openFolderButton.Enabled = false;
                deleteExerciseButton.Enabled = false;
                DisableEditing();

                // Ocultar video
                videoLabel.Visible = false;
                videoPanel.Visible = false;
                try { videoWebBrowser?.Navigate("about:blank"); } catch { }
                return;
            }

            LoadPreviewImage(exercise);
            exerciseNameLabel.Text = exercise.ExerciseName;
            muscleGroupsLabel.Text = exercise.MuscleGroups != null && exercise.MuscleGroups.Length > 0
                ? string.Join(", ", exercise.MuscleGroups)
                : "Sin grupos musculares asignados";

            changeImageButton.Enabled = true;
            openFolderButton.Enabled = true;
            deleteExerciseButton.Enabled = true;
            EnableEditing();

            // NUEVO: Cargar video si existe
            if (!string.IsNullOrWhiteSpace(exercise.VideoUrl))
            {
                videoLabel.Visible = true;
                videoPanel.Visible = true;
                LoadVideoInBrowser(exercise.VideoUrl);
                // Asegurar que el panel de video quede encima de otros controles y visible
                try { videoPanel.BringToFront(); videoLabel.BringToFront(); }
                catch { }
            }
            else
            {
                videoLabel.Visible = false;
                videoPanel.Visible = false;
                try { videoWebBrowser?.Navigate("about:blank"); } catch { }
            }
        }

        private void LoadPreviewImage(ExerciseImageInfo exercise)
        {
            try
            {
                Image? image = null;

                if (exercise.ImageData != null && exercise.ImageData.Length > 0)
                {
                    using var ms = new MemoryStream(exercise.ImageData);
                    image = Image.FromStream(ms);
                }
                else if (!string.IsNullOrWhiteSpace(exercise.ImagePath) && File.Exists(exercise.ImagePath))
                {
                    image = Image.FromFile(exercise.ImagePath);
                }

                previewPictureBox.Image = image ?? _placeholderImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridManager] Error loading preview: {ex.Message}");
                previewPictureBox.Image = _placeholderImage;
            }
        }

        /// <summary>
        /// NUEVO: Carga el video en el WebBrowser embebido
        /// </summary>
        private void LoadVideoInBrowser(string? videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                videoWebBrowser.DocumentText = @"<html><body style='margin:0;padding:20px;background-color:#f0f0f0;font-family:Arial;text-align:center;'><p style='color:#666;'>No hay video disponible</p></body></html>";
                return;
            }

            try
            {
                string embedUrl = ConvertToEmbedUrl(videoUrl);

                string html = $@"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body{{margin:0;padding:0;overflow:hidden;background-color:#000}}.video-container{{position:relative;width:100%;height:100%;display:flex;align-items:center;justify-content:center}}iframe{{width:100%;height:100%;border:none}}</style></head><body><div class='video-container'><iframe src='{embedUrl}' frameborder='0' allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture' allowfullscreen></iframe></div></body></html>";

                videoWebBrowser.DocumentText = html;
                Debug.WriteLine($"[HybridExerciseManagerForm] Video cargado: {embedUrl}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridExerciseManagerForm] Error cargando video: {ex.Message}");
                videoWebBrowser.DocumentText = $@"<html><body style='margin:0;padding:20px;background-color:#f0f0f0;font-family:Arial;text-align:center;'><p style='color:#c00;'>Error al cargar el video</p><p style='color:#666;font-size:12px;'>{ex.Message}</p></body></html>";
            }
        }

        /// <summary>
        /// NUEVO: Convierte URL de YouTube/Vimeo a formato embebido (sin System.Web)
        /// </summary>
        private string ConvertToEmbedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "";

            // YouTube
            if (url.Contains("youtube.com") || url.Contains("youtu.be"))
            {
                string videoId = "";

                if (url.Contains("youtube.com/watch?v="))
                {
                    var startIndex = url.IndexOf("?v=") + 3;
                    if (startIndex >= 3 && startIndex < url.Length)
                    {
                        var endIndex = url.IndexOf("&", startIndex);
                        videoId = endIndex > startIndex
                            ? url.Substring(startIndex, endIndex - startIndex)
                            : url.Substring(startIndex);
                    }
                }
                else if (url.Contains("youtu.be/"))
                {
                    var startIndex = url.LastIndexOf('/') + 1;
                    var endIndex = url.IndexOf("?", startIndex);
                    videoId = endIndex > startIndex
                        ? url.Substring(startIndex, endIndex - startIndex)
                        : url.Substring(startIndex);
                }
                else if (url.Contains("youtube.com/embed/"))
                {
                    return url; // ya embed
                }

                if (!string.IsNullOrEmpty(videoId))
                {
                    return $"https://www.youtube.com/embed/{videoId}?autoplay=0&rel=0&modestbranding=1";
                }
            }

            // Vimeo
            if (url.Contains("vimeo.com"))
            {
                var parts = url.Split('/');
                var videoId = parts[^1].Split('?')[0];
                if (!string.IsNullOrEmpty(videoId) && long.TryParse(videoId, out _))
                {
                    return $"https://player.vimeo.com/video/{videoId}";
                }
            }

            return url;
        }

        #endregion

        #region Editing Actions

        private void EnableEditing()
        {
            editNameTextBox.Enabled = true;
            editDescriptionTextBox.Enabled = true;
            muscleGroupsCheckedListBox.Enabled = true;
            editVideoUrlTextBox.Enabled = true;

            editNameTextBox.TextChanged += OnFieldChanged;
            editDescriptionTextBox.TextChanged += OnFieldChanged;
            muscleGroupsCheckedListBox.ItemCheck += OnMuscleGroupCheck;
        }

        private void DisableEditing()
        {
            editNameTextBox.Enabled = false;
            editDescriptionTextBox.Enabled = false;
            muscleGroupsCheckedListBox.Enabled = false;
            editVideoUrlTextBox.Enabled = false;
            editNameTextBox.Text = "";
            editDescriptionTextBox.Text = "";

            for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++)
            {
                muscleGroupsCheckedListBox.SetItemChecked(i, false);
            }

            saveButton.Enabled = false;
            cancelButton.Enabled = false;

            editNameTextBox.TextChanged -= OnFieldChanged;
            editDescriptionTextBox.TextChanged -= OnFieldChanged;
            muscleGroupsCheckedListBox.ItemCheck -= OnMuscleGroupCheck;
        }

        private void OnFieldChanged(object? sender, EventArgs e)
        {
            if (!_isLoadingEditor)
            {
                EnableSaveButtons();
            }
        }

        private void OnMuscleGroupCheck(object? sender, ItemCheckEventArgs e)
        {
            if (!_isLoadingEditor)
            {
                // Usar BeginInvoke para que el cambio se procese después de la actualización del checkbox
                BeginInvoke(new Action(() => EnableSaveButtons()));
            }
        }

        private void EnableSaveButtons()
        {
            _isEditing = true;
            saveButton.Enabled = true;
            cancelButton.Enabled = true;
        }

        private void SaveCurrentExercise()
        {
            if (_currentExercise == null) return;

            var newName = editNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("El nombre del ejercicio no puede estar vacío.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                editNameTextBox.Focus();
                return;
            }

            var description = editDescriptionTextBox.Text.Trim();
            var muscles = muscleGroupsCheckedListBox.CheckedItems.Cast<string>().ToArray();
            var videoUrl = editVideoUrlTextBox.Text.Trim();

            var success = _imageDatabase.UpdateExerciseDetails(
                _currentExercise.ExerciseName,
                newName,
                description,
                muscles,
                Array.Empty<string>(),
                string.Empty,
                videoUrl);

            if (!success)
            {
                MessageBox.Show("No se pudieron guardar los cambios.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _metadataStore.Upsert(new ExerciseMetadataRecord
            {
                Name = newName,
                Description = description,
                MuscleGroups = muscles,
                Keywords = Array.Empty<string>(),
                Source = string.Empty
            }, _currentExercise.ExerciseName);

            UpdateStatus("✓ Cambios guardados correctamente");
            LoadExercises(newName);
            _isEditing = false;
            saveButton.Enabled = false;
            cancelButton.Enabled = false;
        }

        private void CancelEdit()
        {
            if (_currentExercise == null) return;

            LoadExerciseIntoEditor(_currentExercise);
            _isEditing = false;
            saveButton.Enabled = false;
            cancelButton.Enabled = false;
            UpdateStatus("Cambios descartados");
        }

        private void LoadExerciseIntoEditor(ExerciseImageInfo exercise)
        {
            _isLoadingEditor = true;

            editNameTextBox.Text = exercise.ExerciseName;
            editDescriptionTextBox.Text = exercise.Description ?? "";
            editVideoUrlTextBox.Text = exercise.VideoUrl ?? "";

            // Cargar grupos musculares
            for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++)
            {
                muscleGroupsCheckedListBox.SetItemChecked(i, false);
            }

            if (exercise.MuscleGroups != null && exercise.MuscleGroups.Length > 0)
            {
                foreach (var muscle in exercise.MuscleGroups)
                {
                    var index = muscleGroupsCheckedListBox.Items.IndexOf(muscle);
                    if (index >= 0)
                    {
                        muscleGroupsCheckedListBox.SetItemChecked(index, true);
                    }
                }
            }

            _isLoadingEditor = false;
        }

        #endregion

        #region Exercise Management

        private void CreateNewExercise()
        {
            using var dialog = new AddExerciseDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            if (string.IsNullOrWhiteSpace(dialog.ExerciseName))
                return;

            var name = dialog.ExerciseName.Trim();

            if (!_imageDatabase.AddOrUpdateExercise(name, string.Empty, dialog.Keywords, dialog.MuscleGroups, dialog.Description))
            {
                MessageBox.Show("No se pudo crear el ejercicio.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _metadataStore.Upsert(new ExerciseMetadataRecord
            {
                Name = name,
                Description = dialog.Description,
                MuscleGroups = dialog.MuscleGroups,
                Keywords = Array.Empty<string>(),
                Source = string.Empty
            });

            UpdateStatus($"✓ Ejercicio '{name}' creado correctamente");
            LoadExercises(name);
        }

        private void DeleteCurrentExercise()
        {
            if (_currentExercise == null) return;

            var result = MessageBox.Show(
                $"¿Estás seguro de eliminar '{_currentExercise.ExerciseName}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
                return;

            if (_imageDatabase.RemoveExercise(_currentExercise.ExerciseName))
            {
                _metadataStore.Delete(_currentExercise.ExerciseName);
                UpdateStatus($"✓ Ejercicio '{_currentExercise.ExerciseName}' eliminado");
                LoadExercises();
            }
            else
            {
                MessageBox.Show("No se pudo eliminar el ejercicio.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportImageFromDisk()
        {
            if (_currentExercise == null) return;

            using var dialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen del ejercicio",
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|Todos los archivos|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            if (_imageDatabase.ImportImageForExercise(_currentExercise.ExerciseName, dialog.FileName))
            {
                UpdateStatus("✓ Imagen importada correctamente");
                LoadExercises(_currentExercise.ExerciseName);
            }
            else
            {
                MessageBox.Show("No se pudo importar la imagen.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenImageFolder()
        {
            if (_currentExercise == null) return;

            var path = _currentExercise.ImagePath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                if (_currentExercise.ImageData != null && _currentExercise.ImageData.Length > 0)
                {
                    _exportedTempImage = ExportImageToTemp(_currentExercise.ImageData, _currentExercise.ExerciseName);
                    path = _exportedTempImage;
                }
            }

            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                MessageBox.Show("No hay imagen disponible para este ejercicio.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenVideoButton_Click()
        {
            if (_currentExercise == null || string.IsNullOrWhiteSpace(_currentExercise.VideoUrl))
            {
                MessageBox.Show("No hay video disponible para este ejercicio.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _currentExercise.VideoUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el video.\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Drag & Drop de Imágenes

        private void PreviewPictureBox_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    var ext = Path.GetExtension(files[0]).ToLowerInvariant();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif" || ext == ".webp")
                    {
                        e.Effect = DragDropEffects.Copy;
                        return;
                    }
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void PreviewPictureBox_DragDrop(object? sender, DragEventArgs e)
        {
            if (_currentExercise == null)
            {
                MessageBox.Show("Selecciona un ejercicio primero.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (e.Data?.GetDataPresent(DataFormats.FileDrop) != true)
                return;

            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0)
                return;

            var imagePath = files[0];
            var ext = Path.GetExtension(imagePath).ToLowerInvariant();

            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".bmp" && ext != ".gif" && ext != ".webp")
            {
                MessageBox.Show("Solo se admiten archivos de imagen (JPG, PNG, BMP, GIF, WEBP).", "Formato no válido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_imageDatabase.ImportImageForExercise(_currentExercise.ExerciseName, imagePath))
            {
                UpdateStatus("✓ Imagen importada correctamente (arrastrar y soltar)");
                LoadExercises(_currentExercise.ExerciseName);
            }
            else
            {
                MessageBox.Show("No se pudo importar la imagen.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Status

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }

        private string? ExportImageToTemp(byte[] imageData, string exerciseName)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), $"{exerciseName}_{Guid.NewGuid()}.jpg");
                File.WriteAllBytes(tempPath, imageData);
                return tempPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting image to temp: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
