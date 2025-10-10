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
        private readonly List<ExerciseSelectionEntry> _selectionEntries = new();
        private readonly ToolTip _toolTip = new();

        private TextBox _searchTextBox = null!;
        private Label _resultsLabel = null!;
        private ListView _resultsListView = null!;
        private CheckBox _secondaryDatabaseCheckBox = null!;
        private ContextMenuStrip _resultsContextMenu = null!;
        private Button _addToSelectionButton = null!;
        private Panel _selectionPanel = null!;
        private ListView _selectionListView = null!;
        private Label _selectionHeaderLabel = null!;
        private Button _copySelectionNamesButton = null!;
        private Button _copySelectionNamesWithPathButton = null!;
        private Button _removeSelectionButton = null!;
        private Button _clearSelectionButton = null!;
        private ContextMenuStrip _selectionContextMenu = null!;

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
            MinimumSize = new Size(940, 560);
            Size = new Size(1040, 600);

            _searchTextBox = new TextBox
            {
                PlaceholderText = "Buscar ejercicio...",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 20),
                Width = 580
            };
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            _secondaryDatabaseCheckBox = new CheckBox
            {
                Text = "Mostrar BD secundaria",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(620, 22),
                AutoSize = true
            };
            _secondaryDatabaseCheckBox.CheckedChanged += SecondaryDatabaseCheckBox_CheckedChanged;
            _toolTip.SetToolTip(_secondaryDatabaseCheckBox, "Activa esta opción para consultar únicamente la base secundaria de ejercicios.");

            _addToSelectionButton = new Button
            {
                Text = "Agregar a selección",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(780, 18),
                Size = new Size(150, 32)
            };
            _addToSelectionButton.Click += (_, _) => AddSelectedToSelection();
            _toolTip.SetToolTip(_addToSelectionButton, "Agrega el ejercicio seleccionado a la lista temporal.");

            _resultsLabel = new Label
            {
                Text = "Escribe al menos 3 caracteres para buscar.",
                Location = new Point(20, 60),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _resultsListView = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(20, 90),
                Size = new Size(660, 420),
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
            BuildSelectionPanel();

            Controls.Add(_searchTextBox);
            Controls.Add(_secondaryDatabaseCheckBox);
            Controls.Add(_addToSelectionButton);
            Controls.Add(_resultsLabel);
            Controls.Add(_resultsListView);
            Controls.Add(_selectionPanel);

            FormClosed += ExerciseGalleryForm_FormClosed;

            ResumeLayout(false);
            PerformLayout();
        }

        private void BuildContextMenu()
        {
            _resultsContextMenu = new ContextMenuStrip();

            var addToSelectionItem = new ToolStripMenuItem("Agregar a selección", null, (_, _) => AddSelectedToSelection());
            var copyImageItem = new ToolStripMenuItem("Copiar imagen al portapapeles", null, (_, _) => CopySelectedImage());
            var openLocationItem = new ToolStripMenuItem("Abrir ubicación en el explorador", null, (_, _) => OpenSelectedImageLocation());

            _resultsContextMenu.Items.Add(addToSelectionItem);
            _resultsContextMenu.Items.Add(copyImageItem);
            _resultsContextMenu.Items.Add(openLocationItem);

            _resultsListView.ContextMenuStrip = _resultsContextMenu;
        }

        private void BuildSelectionPanel()
        {
            _selectionPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(700, 20),
                Size = new Size(300, 490)
            };

            _selectionHeaderLabel = new Label
            {
                Text = "Seleccionados (0)",
                AutoSize = true,
                Location = new Point(0, 0)
            };

            _selectionListView = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(0, 25),
                Size = new Size(300, 320),
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false
            };
            _selectionListView.Columns.Add("Ejercicio", 160);
            _selectionListView.Columns.Add("Fuente", 120);
            _selectionListView.DoubleClick += (_, _) => OpenSelectionLocation();

            BuildSelectionContextMenu();
            _selectionListView.ContextMenuStrip = _selectionContextMenu;

            _copySelectionNamesButton = new Button
            {
                Text = "Copiar nombres",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(0, 360),
                Size = new Size(140, 32)
            };
            _copySelectionNamesButton.Click += (_, _) => CopySelection(includePaths: false);

            _copySelectionNamesWithPathButton = new Button
            {
                Text = "Copiar nombres + rutas",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(150, 360),
                Size = new Size(150, 32)
            };
            _copySelectionNamesWithPathButton.Click += (_, _) => CopySelection(includePaths: true);

            _removeSelectionButton = new Button
            {
                Text = "Quitar seleccionado",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(0, 404),
                Size = new Size(140, 32)
            };
            _removeSelectionButton.Click += (_, _) => RemoveSelectedSelection();

            _clearSelectionButton = new Button
            {
                Text = "Limpiar lista",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(150, 404),
                Size = new Size(150, 32)
            };
            _clearSelectionButton.Click += (_, _) => ClearSelection();

            _selectionPanel.Controls.Add(_selectionHeaderLabel);
            _selectionPanel.Controls.Add(_selectionListView);
            _selectionPanel.Controls.Add(_copySelectionNamesButton);
            _selectionPanel.Controls.Add(_copySelectionNamesWithPathButton);
            _selectionPanel.Controls.Add(_removeSelectionButton);
            _selectionPanel.Controls.Add(_clearSelectionButton);

            UpdateSelectionSummary();
        }

        private void BuildSelectionContextMenu()
        {
            _selectionContextMenu = new ContextMenuStrip();

            var copyNameItem = new ToolStripMenuItem("Copiar nombre", null, (_, _) => CopySelection(includePaths: false));
            var copyNameWithPathItem = new ToolStripMenuItem("Copiar nombre + ruta", null, (_, _) => CopySelection(includePaths: true));
            var openLocationItem = new ToolStripMenuItem("Abrir ubicación", null, (_, _) => OpenSelectionLocation());
            var removeItem = new ToolStripMenuItem("Quitar", null, (_, _) => RemoveSelectedSelection());

            _selectionContextMenu.Items.Add(copyNameItem);
            _selectionContextMenu.Items.Add(copyNameWithPathItem);
            _selectionContextMenu.Items.Add(openLocationItem);
            _selectionContextMenu.Items.Add(new ToolStripSeparator());
            _selectionContextMenu.Items.Add(removeItem);
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
                    listItem.SubItems.Add(GetPathLabel(_libraryService.GetImagePath(item) ?? item.ImagePath));

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
            AddSelectedToSelection();
        }

        private void AddSelectedToSelection()
        {
            var item = GetSelectedGalleryItem();
            if (item == null)
            {
                return;
            }

            var existing = _selectionEntries.FirstOrDefault(entry => string.Equals(entry.ExerciseId, item.Id, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _resultsLabel.Text = "Este ejercicio ya está en la selección.";
                return;
            }

            var resolvedPath = _libraryService.GetImagePath(item) ?? item.ImagePath;
            var entry = new ExerciseSelectionEntry(
                item.Id,
                item.DisplayName,
                resolvedPath ?? string.Empty,
                item.MuscleGroups,
                DateTime.UtcNow,
                item.Source);

            _selectionEntries.Add(entry);
            AppendSelectionEntry(entry);
            UpdateSelectionSummary();
            _resultsLabel.Text = $"Agregado a la selección: {item.DisplayName}";
        }

        private void AppendSelectionEntry(ExerciseSelectionEntry entry)
        {
            var listItem = new ListViewItem(entry.DisplayName)
            {
                Tag = entry
            };
            listItem.SubItems.Add(string.IsNullOrWhiteSpace(entry.Source) ? "ND" : entry.Source);

            _selectionListView.Items.Add(listItem);
        }

        private void CopySelection(bool includePaths)
        {
            if (_selectionEntries.Count == 0)
            {
                _resultsLabel.Text = "La selección está vacía.";
                return;
            }

            var builder = new System.Text.StringBuilder();
            foreach (var entry in _selectionEntries)
            {
                var line = includePaths
                    ? $"{entry.DisplayName} - {entry.ImagePath}"
                    : entry.DisplayName;
                builder.AppendLine(line);
            }

            Clipboard.SetText(builder.ToString().Trim());
            _resultsLabel.Text = includePaths ? "Lista copiada (nombres + rutas)." : "Lista copiada (solo nombres).";
        }

        private void RemoveSelectedSelection()
        {
            if (_selectionListView.SelectedItems.Count == 0)
            {
                _resultsLabel.Text = "Selecciona un elemento para quitar.";
                return;
            }

            var selected = _selectionListView.SelectedItems[0];
            if (selected.Tag is ExerciseSelectionEntry entry)
            {
                _selectionEntries.Remove(entry);
                _selectionListView.Items.Remove(selected);
                UpdateSelectionSummary();
                _resultsLabel.Text = $"Quitado: {entry.DisplayName}";
            }
        }

        private void ClearSelection()
        {
            if (_selectionEntries.Count == 0)
            {
                return;
            }

            _selectionEntries.Clear();
            _selectionListView.Items.Clear();
            UpdateSelectionSummary();
            _resultsLabel.Text = "Selección vaciada.";
        }

        private void UpdateSelectionSummary()
        {
            _selectionHeaderLabel.Text = $"Seleccionados ({_selectionEntries.Count})";
            _removeSelectionButton.Enabled = _selectionEntries.Count > 0;
            _clearSelectionButton.Enabled = _selectionEntries.Count > 0;
            _copySelectionNamesButton.Enabled = _selectionEntries.Count > 0;
            _copySelectionNamesWithPathButton.Enabled = _selectionEntries.Count > 0;
        }

        private void OpenSelectionLocation()
        {
            if (_selectionListView.SelectedItems.Count == 0)
            {
                return;
            }

            if (_selectionListView.SelectedItems[0].Tag is ExerciseSelectionEntry entry)
            {
                if (!string.IsNullOrWhiteSpace(entry.ImagePath) && File.Exists(entry.ImagePath))
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"/select,\"{entry.ImagePath}\"",
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        _resultsLabel.Text = "No se pudo abrir la ubicación del archivo.";
                        Debug.WriteLine($"[ExerciseGalleryForm] Error abriendo selección: {ex}");
                    }
                }
                else
                {
                    _resultsLabel.Text = "Ruta no disponible para este ejercicio.";
                }
            }
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
