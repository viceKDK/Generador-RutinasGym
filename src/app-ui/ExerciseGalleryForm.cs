using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GymRoutineGenerator.UI.Models;
using Timer = System.Windows.Forms.Timer;

namespace GymRoutineGenerator.UI
{
    public partial class ExerciseGalleryForm : Form
    {
        private readonly ManualExerciseLibraryService _libraryService;
        private readonly Timer _searchDebounceTimer;
        private readonly List<ExerciseGalleryItem> _currentResults = new();
        private readonly ToolTip _toolTip = new();

        private TextBox _searchTextBox = null!;
        private Label _resultsLabel = null!;
        private ListView _resultsListView = null!;
        private CheckBox _secondaryDatabaseCheckBox = null!;
        private ContextMenuStrip _resultsContextMenu = null!;

        public ExerciseGalleryForm(ManualExerciseLibraryService libraryService)
        {
            _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
            _searchDebounceTimer = new Timer { Interval = 300 };
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Galería de ejercicios";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(780, 520);
            Size = new Size(880, 580);

            _searchTextBox = new TextBox
            {
                PlaceholderText = "Buscar ejercicio...",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 20),
                Width = 520
            };
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            _secondaryDatabaseCheckBox = new CheckBox
            {
                Text = "Mostrar BD secundaria",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(560, 22),
                AutoSize = true
            };
            _secondaryDatabaseCheckBox.CheckedChanged += SecondaryDatabaseCheckBox_CheckedChanged;
            _toolTip.SetToolTip(_secondaryDatabaseCheckBox, "Activa esta opción para consultar únicamente la base secundaria de ejercicios.");

            _resultsLabel = new Label
            {
                Text = "Escribe al menos 3 caracteres para buscar.",
                Location = new Point(20, 60),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _resultsListView = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 90),
                Size = new Size(820, 420),
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false
            };
            _resultsListView.Columns.Add("Ejercicio", 250);
            _resultsListView.Columns.Add("Fuente", 120);
            _resultsListView.Columns.Add("Grupos musculares", 220);
            _resultsListView.Columns.Add("Ruta de imagen", 200);
            _resultsListView.DoubleClick += ResultsListView_DoubleClick;

            BuildContextMenu();

            Controls.Add(_searchTextBox);
            Controls.Add(_secondaryDatabaseCheckBox);
            Controls.Add(_resultsLabel);
            Controls.Add(_resultsListView);

            FormClosed += ExerciseGalleryForm_FormClosed;

            ResumeLayout(false);
            PerformLayout();
        }

        private void BuildContextMenu()
        {
            _resultsContextMenu = new ContextMenuStrip();

            var copyImageItem = new ToolStripMenuItem("Copiar imagen al portapapeles", null, (_, _) => CopySelectedImage());
            var openLocationItem = new ToolStripMenuItem("Abrir ubicación en el explorador", null, (_, _) => OpenSelectedImageLocation());

            _resultsContextMenu.Items.Add(copyImageItem);
            _resultsContextMenu.Items.Add(openLocationItem);

            _resultsListView.ContextMenuStrip = _resultsContextMenu;
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void SecondaryDatabaseCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            PerformSearch(force: true);
        }

        private void SearchDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            PerformSearch();
        }

        private void PerformSearch(bool force = false)
        {
            var query = _searchTextBox.Text ?? string.Empty;
            if (!force && string.IsNullOrWhiteSpace(query))
            {
                _resultsListView.Items.Clear();
                _currentResults.Clear();
                _resultsLabel.Text = "Escribe al menos 3 caracteres para buscar.";
                return;
            }

            if (query.Length < 3)
            {
                _resultsListView.Items.Clear();
                _currentResults.Clear();
                _resultsLabel.Text = "Escribe al menos 3 caracteres para buscar.";
                return;
            }

            try
            {
                var source = _secondaryDatabaseCheckBox.Checked
                    ? ManualExerciseDataSource.Secondary
                    : ManualExerciseDataSource.Primary;

                var results = _libraryService.Search(query, source);
                _currentResults.Clear();
                _currentResults.AddRange(results);

                _resultsListView.BeginUpdate();
                _resultsListView.Items.Clear();

                foreach (var item in results)
                {
                    var listItem = new ListViewItem(item.DisplayName)
                    {
                        Tag = item
                    };

                    listItem.SubItems.Add(item.Source);
                    var groups = item.MuscleGroups.Count > 0 ? string.Join(", ", item.MuscleGroups) : "ND";
                    listItem.SubItems.Add(groups);
                    listItem.SubItems.Add(GetPathLabel(item.ImagePath));

                    _resultsListView.Items.Add(listItem);
                }

                _resultsListView.EndUpdate();

                _resultsLabel.Text = results.Count switch
                {
                    0 => "Sin resultados. Prueba con otro término.",
                    1 => "1 resultado encontrado.",
                    _ => $"{results.Count} resultados encontrados."
                };
            }
            catch (OperationCanceledException)
            {
                // Ignorar cancelaciones, son parte del debounce.
            }
            catch (Exception ex)
            {
                _resultsLabel.Text = "Ocurrió un error al buscar. Revisa los logs.";
                Debug.WriteLine($"[ExerciseGalleryForm] Error realizando búsqueda: {ex}");
            }
        }

        private static string GetPathLabel(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "Sin imagen";
            }

            try
            {
                return Path.GetFileName(path);
            }
            catch
            {
                return path;
            }
        }

        private void CopySelectedImage()
        {
            var item = GetSelectedGalleryItem();
            if (item == null)
            {
                return;
            }

            if (_libraryService.TryCopyImageToClipboard(item))
            {
                _resultsLabel.Text = $"Imagen copiada: {item.DisplayName}";
            }
            else
            {
                _resultsLabel.Text = "No se pudo copiar la imagen. Verifica la ruta.";
            }
        }

        private void OpenSelectedImageLocation()
        {
            var item = GetSelectedGalleryItem();
            if (item == null)
            {
                return;
            }

            if (_libraryService.TryOpenImageLocation(item))
            {
                _resultsLabel.Text = $"Abriendo ubicación de: {item.DisplayName}";
            }
            else
            {
                _resultsLabel.Text = "No se pudo abrir la ubicación del archivo.";
            }
        }

        private void ResultsListView_DoubleClick(object? sender, EventArgs e)
        {
            CopySelectedImage();
        }

        private ExerciseGalleryItem? GetSelectedGalleryItem()
        {
            if (_resultsListView.SelectedItems.Count == 0)
            {
                return null;
            }

            return _resultsListView.SelectedItems[0].Tag as ExerciseGalleryItem;
        }

        private void ExerciseGalleryForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Dispose();
        }
    }
}
