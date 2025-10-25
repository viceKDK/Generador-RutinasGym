using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    public partial class ImprovedExerciseImageManagerForm : Form
    {
        private static readonly Size ThumbnailSize = new Size(36, 36);

        private readonly SQLiteExerciseImageDatabase _imageDatabase;
        private readonly ExerciseMetadataStore _metadataStore;
        private readonly string[] _defaultMuscleGroups =
        {
            "Pecho", "Espalda", "Hombros", "Biceps", "Triceps",
            "Antebrazos", "Abdominales", "Core", "Cuadriceps", "Isquiotibiales",
            "Gluteos", "Gemelos", "Aductores", "Abductores", "Trapecio",
            "Dorsales", "Lumbar", "Cuello", "Cardio"
        };

        private readonly Image _placeholderImage;

        private ListView exerciseListView = null!;
        private ImageList exerciseImageList = null!;
        private TextBox searchTextBox = null!;
        private ComboBox muscleFilterComboBox = null!;
        private TextBox exerciseNameTextBox = null!;
        private CheckedListBox muscleGroupsCheckedListBox = null!;
        private Panel musclesContentPanel = null!;
        private Panel descriptionContentPanel = null!;
        private Button musclesToggleButton = null!;
        private Button descriptionToggleButton = null!;
        private TextBox editDescriptionTextBox = null!;
        private Button importImageButton = null!;
        private Button openImageButton = null!;
        private Button openFolderButton = null!;
        private Button addNewExerciseButton = null!;
        private Button saveButton = null!;
        private Button deleteButton = null!;
        private ToolStripStatusLabel statusMessageLabel = null!;

        private List<ExerciseImageInfo> _allExercises = new();
        private ExerciseImageInfo? _currentExercise;
        private bool _isLoadingEditor;
        private bool _isDirty;
        private string? _exportedTempImage;

        public ImprovedExerciseImageManagerForm()
        {
            _imageDatabase = new SQLiteExerciseImageDatabase();
            _metadataStore = new ExerciseMetadataStore(AppDomain.CurrentDomain.BaseDirectory);
            _placeholderImage = CreatePlaceholderThumbnail();
            InitializeComponent();
            LoadExercises();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_exportedTempImage != null && File.Exists(_exportedTempImage))
                {
                    try
                    {
                        File.Delete(_exportedTempImage);
                    }
                    catch
                    {
                        // ignore cleanup errors
                    }
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Text = "Gestor de Ejercicios e Imágenes";
            Size = new Size(1400, 860);
            MinimumSize = new Size(1200, 760);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 246, 250);
            Font = new Font("Segoe UI", 10F);

            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6
            };

            mainSplit.Panel1.Controls.Add(BuildLeftPanel());
            mainSplit.Panel2.Controls.Add(BuildRightPanel());

            var statusStrip = new StatusStrip { SizingGrip = false };
            statusMessageLabel = new ToolStripStatusLabel
            {
                Spring = true,
                Text = "Listo para agregar ejercicios",
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusStrip.Items.Add(statusMessageLabel);

            Controls.Add(mainSplit);
            Controls.Add(statusStrip);

            Load += (_, _) =>
            {
                if (mainSplit.Width > 0)
                {
                    mainSplit.SplitterDistance = mainSplit.Width / 2;
                }
            };

            mainSplit.SizeChanged += (_, _) =>
            {
                if (mainSplit.Width > 0)
                {
                    mainSplit.SplitterDistance = Math.Max(mainSplit.Width / 2, 320);
                }
            };
        }

        private Control BuildLeftPanel()
        {
            exerciseImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = ThumbnailSize
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var titleLabel = new Label
            {
                Text = "Ejercicios guardados",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var filtersPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 12, 0, 12)
            };

            searchTextBox = new TextBox
            {
                Width = 240,
                PlaceholderText = "Buscar..."
            };
            searchTextBox.TextChanged += (_, _) => ApplyFilters();

            muscleFilterComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180
            };
            muscleFilterComboBox.SelectedIndexChanged += (_, _) => ApplyFilters();

            filtersPanel.Controls.Add(searchTextBox);
            filtersPanel.Controls.Add(new Label
            {
                Text = "Grupo:",
                AutoSize = true,
                Margin = new Padding(12, 5, 6, 0)
            });
            filtersPanel.Controls.Add(muscleFilterComboBox);

            exerciseListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false,
                BorderStyle = BorderStyle.FixedSingle,
                HeaderStyle = ColumnHeaderStyle.None,
                SmallImageList = exerciseImageList
            };
            exerciseListView.Columns.Add(string.Empty, 260);
            exerciseListView.SelectedIndexChanged += ExerciseListView_SelectedIndexChanged;

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            addNewExerciseButton = CreatePrimaryButton("Agregar ejercicio", (_, _) => CreateNewExercise());
            importImageButton = CreateSecondaryButton("Importar imagen…", (_, _) => ImportImageFromDisk());
            openImageButton = CreateSecondaryButton("Abrir imagen", (_, _) => OpenCurrentImage());
            openFolderButton = CreateSecondaryButton("Abrir carpeta", (_, _) => OpenImageFolder());

            importImageButton.Enabled = false;
            openImageButton.Enabled = false;
            openFolderButton.Enabled = false;

            buttonsPanel.Controls.Add(addNewExerciseButton);
            buttonsPanel.Controls.Add(importImageButton);
            buttonsPanel.Controls.Add(openImageButton);
            buttonsPanel.Controls.Add(openFolderButton);

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(filtersPanel, 0, 1);
            layout.Controls.Add(exerciseListView, 0, 2);
            layout.Controls.Add(buttonsPanel, 0, 3);

            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildRightPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var titleLabel = new Label
            {
                Text = "Editar ejercicio",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true
            };

            layout.Controls.Add(titleLabel, 0, 0);

            layout.Controls.Add(CreateFieldLabel("Nombre"), 0, 1);
            exerciseNameTextBox = CreateEditorTextBox();
            exerciseNameTextBox.TextChanged += (_, _) => MarkDirty();
            layout.Controls.Add(exerciseNameTextBox, 0, 2);

            var quickActions = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 12, 0, 12)
            };

            saveButton = CreatePrimaryButton("Guardar cambios", (_, _) => SaveCurrentExercise());
            deleteButton = CreateSecondaryButton("Eliminar", (_, _) => DeleteCurrentExercise());
            var discardButton = CreateSecondaryButton("Descartar", (_, _) => ResetEditor());

            quickActions.Controls.Add(saveButton);
            quickActions.Controls.Add(discardButton);
            quickActions.Controls.Add(deleteButton);

            layout.Controls.Add(quickActions, 0, 3);

            var musclesSection = BuildCollapsibleSection("Grupos musculares", out musclesToggleButton, out musclesContentPanel);
            muscleGroupsCheckedListBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 280
            };
            PopulateMuscleChecklist(muscleGroupsCheckedListBox);
            muscleGroupsCheckedListBox.ItemCheck += (_, _) =>
            {
                if (_isLoadingEditor)
                {
                    return;
                }

                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(MarkDirty));
                }
                else
                {
                    MarkDirty();
                }
            };
            musclesContentPanel.Controls.Add(muscleGroupsCheckedListBox);
            layout.Controls.Add(musclesSection, 0, 4);

            var descriptionSection = BuildCollapsibleSection("Descripción", out descriptionToggleButton, out descriptionContentPanel);
            editDescriptionTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 100,
                ScrollBars = ScrollBars.Vertical
            };
            editDescriptionTextBox.TextChanged += (_, _) => MarkDirty();
            descriptionContentPanel.Controls.Add(editDescriptionTextBox);
            layout.Controls.Add(descriptionSection, 0, 5);

            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildCollapsibleSection(string title, out Button toggleButton, out Panel contentPanel)
        {
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
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
                BackColor = Color.FromArgb(248, 249, 250)
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

        private TextBox CreateEditorTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Button CreatePrimaryButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(12, 6, 12, 6),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 6, 0)
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += handler;
            return button;
        }

        private Button CreateSecondaryButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(12, 6, 12, 6),
                BackColor = Color.FromArgb(233, 236, 239),
                ForeColor = Color.FromArgb(52, 58, 64),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 0, 0)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(206, 212, 218);
            button.Click += handler;
            return button;
        }

        private Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 4),
                ForeColor = Color.FromArgb(73, 80, 87)
            };
        }

        private void LoadExercises(string? preserveSelection = null)
        {
            var remembered = preserveSelection ?? _currentExercise?.ExerciseName;
            _allExercises = _imageDatabase.GetAllExercises();
            MergeMetadata();
            RefreshMuscleFilter();
            ApplyFilters(remembered);
        }

        private void MergeMetadata()
        {
            foreach (var exercise in _allExercises)
            {
                var metadata = _metadataStore.Get(exercise.ExerciseName);
                if (metadata == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(metadata.Description))
                {
                    exercise.Description = metadata.Description;
                }

                if (metadata.MuscleGroups is { Length: > 0 })
                {
                    exercise.MuscleGroups = metadata.MuscleGroups;
                }
            }
        }

        private void RefreshMuscleFilter()
        {
            var previous = muscleFilterComboBox.SelectedItem as string;

            var groups = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in _defaultMuscleGroups)
            {
                groups.Add(group);
            }

            foreach (var exercise in _allExercises)
            {
                if (exercise.MuscleGroups == null)
                {
                    continue;
                }

                foreach (var muscle in exercise.MuscleGroups)
                {
                    if (!string.IsNullOrWhiteSpace(muscle))
                    {
                        groups.Add(muscle.Trim());
                    }
                }
            }

            muscleFilterComboBox.BeginUpdate();
            muscleFilterComboBox.Items.Clear();
            muscleFilterComboBox.Items.Add("Todos");
            foreach (var group in groups)
            {
                muscleFilterComboBox.Items.Add(group);
            }
            muscleFilterComboBox.EndUpdate();

            if (!string.IsNullOrWhiteSpace(previous) && muscleFilterComboBox.Items.Contains(previous))
            {
                muscleFilterComboBox.SelectedItem = previous;
            }
            else
            {
                muscleFilterComboBox.SelectedIndex = 0;
            }
        }

        private void ApplyFilters(string? preserveSelection = null)
        {
            var selectedGroup = muscleFilterComboBox.SelectedItem as string;
            var queryText = searchTextBox.Text.Trim();

            IEnumerable<ExerciseImageInfo> query = _allExercises;

            if (!string.IsNullOrWhiteSpace(selectedGroup) && !string.Equals(selectedGroup, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(ex =>
                    ex.MuscleGroups != null &&
                    ex.MuscleGroups.Any(m => m.Equals(selectedGroup, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(queryText))
            {
                var lowered = queryText.ToLowerInvariant();
                query = query.Where(ex =>
                    (!string.IsNullOrWhiteSpace(ex.ExerciseName) && ex.ExerciseName.ToLowerInvariant().Contains(lowered)) ||
                    (ex.MuscleGroups != null && ex.MuscleGroups.Any(m => m.ToLowerInvariant().Contains(lowered))));
            }

            var filtered = query
                .OrderBy(ex => ex.ExerciseName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            UpdateExerciseList(filtered, preserveSelection);
            UpdateStatus(filtered.Count == 1 ? "1 ejercicio encontrado" : $"{filtered.Count} ejercicios encontrados");
        }

        private void UpdateExerciseList(IReadOnlyList<ExerciseImageInfo> exercises, string? preserveSelection)
        {
            exerciseListView.BeginUpdate();
            exerciseListView.Items.Clear();
            exerciseImageList.Images.Clear();

            for (int i = 0; i < exercises.Count; i++)
            {
                var exercise = exercises[i];
                var thumbnail = LoadListThumbnail(exercise) ?? (Image)_placeholderImage.Clone();
                exerciseImageList.Images.Add(thumbnail);

                var item = new ListViewItem(exercise.ExerciseName, i)
                {
                    Tag = exercise
                };
                exerciseListView.Items.Add(item);
            }

            exerciseListView.EndUpdate();

            if (exerciseListView.Items.Count == 0)
            {
                ShowExerciseDetails(null);
                return;
            }

            if (!string.IsNullOrWhiteSpace(preserveSelection))
            {
                foreach (ListViewItem item in exerciseListView.Items)
                {
                    if (string.Equals(((ExerciseImageInfo)item.Tag).ExerciseName, preserveSelection, StringComparison.OrdinalIgnoreCase))
                    {
                        item.Selected = true;
                        item.EnsureVisible();
                        return;
                    }
                }
            }

            exerciseListView.Items[0].Selected = true;
        }

        private void ExerciseListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isLoadingEditor)
            {
                return;
            }

            var selectedExercise = exerciseListView.SelectedItems.Cast<ListViewItem>()
                .Select(item => item.Tag as ExerciseImageInfo)
                .FirstOrDefault();

            if (selectedExercise == null)
            {
                ShowExerciseDetails(null);
                UpdateDirtyState(false);
                return;
            }

            if (_isDirty && _currentExercise != null &&
                !string.Equals(_currentExercise.ExerciseName, selectedExercise.ExerciseName, StringComparison.OrdinalIgnoreCase))
            {
                var confirm = MessageBox.Show(
                    "Hay cambios sin guardar. ¿Deseas descartarlos?",
                    "Cambios sin guardar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm == DialogResult.No)
                {
                    _isLoadingEditor = true;
                    foreach (ListViewItem item in exerciseListView.Items)
                    {
                        if (string.Equals(((ExerciseImageInfo)item.Tag).ExerciseName,
                            _currentExercise.ExerciseName, StringComparison.OrdinalIgnoreCase))
                        {
                            item.Selected = true;
                            item.EnsureVisible();
                            break;
                        }
                    }
                    _isLoadingEditor = false;
                    return;
                }
            }

            ShowExerciseDetails(selectedExercise);
            LoadExerciseIntoEditor(selectedExercise);
            UpdateDirtyState(false);
        }

        private void ShowExerciseDetails(ExerciseImageInfo? exercise)
        {
            _currentExercise = exercise;

            if (exercise == null)
            {
                exerciseNameTextBox.Text = string.Empty;
                editDescriptionTextBox.Text = string.Empty;
                for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++)
                {
                    muscleGroupsCheckedListBox.SetItemChecked(i, false);
                }
                ToggleActionButtons(false);
                return;
            }

            exerciseNameTextBox.Text = exercise.ExerciseName;
            editDescriptionTextBox.Text = exercise.Description ?? string.Empty;

            _isLoadingEditor = true;
            for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++)
            {
                var muscle = muscleGroupsCheckedListBox.Items[i]?.ToString() ?? string.Empty;
                var isChecked = exercise.MuscleGroups != null &&
                                exercise.MuscleGroups.Any(m => m.Equals(muscle, StringComparison.OrdinalIgnoreCase));
                muscleGroupsCheckedListBox.SetItemChecked(i, isChecked);
            }
            _isLoadingEditor = false;

            ToggleActionButtons(true);
        }

        private void LoadExerciseIntoEditor(ExerciseImageInfo exercise)
        {
            _isLoadingEditor = true;

            exerciseNameTextBox.Text = exercise.ExerciseName;
            editDescriptionTextBox.Text = exercise.Description ?? string.Empty;

            for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++)
            {
                var muscle = muscleGroupsCheckedListBox.Items[i]?.ToString() ?? string.Empty;
                var isChecked = exercise.MuscleGroups != null &&
                                exercise.MuscleGroups.Any(m => m.Equals(muscle, StringComparison.OrdinalIgnoreCase));
                muscleGroupsCheckedListBox.SetItemChecked(i, isChecked);
            }

            _isLoadingEditor = false;
        }

        private void ToggleActionButtons(bool enabled)
        {
            importImageButton.Enabled = enabled;
            openImageButton.Enabled = enabled;
            openFolderButton.Enabled = enabled;
            saveButton.Enabled = enabled && _currentExercise != null;
            deleteButton.Enabled = enabled && _currentExercise != null;
        }

        private void MarkDirty()
        {
            if (_isLoadingEditor)
            {
                return;
            }

            UpdateDirtyState(true);
        }

        private void UpdateDirtyState(bool dirty)
        {
            _isDirty = dirty;
            saveButton.Enabled = dirty && _currentExercise != null;
        }

        private void SaveCurrentExercise()
        {
            if (_currentExercise == null)
            {
                return;
            }

            var newName = exerciseNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("El nombre del ejercicio es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                exerciseNameTextBox.Focus();
                return;
            }

            var description = editDescriptionTextBox.Text.Trim();

            var muscles = muscleGroupsCheckedListBox.CheckedItems
                .Cast<string>()
                .Select(m => m.Trim())
                .Where(m => m.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (muscles.Length == 0)
            {
                var confirm = MessageBox.Show(
                    "No se seleccionó ningún grupo muscular. ¿Deseas continuar?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.No)
                {
                    return;
                }
            }

            var success = _imageDatabase.UpdateExerciseDetails(
                _currentExercise.ExerciseName,
                newName,
                description,
                muscles,
                Array.Empty<string>(),
                string.Empty);

            if (!success)
            {
                MessageBox.Show("No se pudieron guardar los cambios. Revisa el log para más detalles.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            UpdateStatus("Cambios guardados correctamente.");
            LoadExercises(newName);
            UpdateDirtyState(false);
        }

        private void ResetEditor()
        {
            if (_currentExercise == null)
            {
                return;
            }

            LoadExerciseIntoEditor(_currentExercise);
            UpdateDirtyState(false);
            UpdateStatus("Cambios descartados.");
        }

        private void DeleteCurrentExercise()
        {
            if (_currentExercise == null)
            {
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Seguro que deseas eliminar '{_currentExercise.ExerciseName}'?",
                "Eliminar ejercicio",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            if (_imageDatabase.RemoveExercise(_currentExercise.ExerciseName))
            {
                _metadataStore.Delete(_currentExercise.ExerciseName);
                UpdateStatus($"Ejercicio '{_currentExercise.ExerciseName}' eliminado.");
                LoadExercises();
            }
            else
            {
                MessageBox.Show("No se pudo eliminar el ejercicio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateNewExercise()
        {
            using var dialog = new AddExerciseDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(dialog.ExerciseName))
            {
                return;
            }

            var name = dialog.ExerciseName.Trim();

            if (!_imageDatabase.AddOrUpdateExercise(name, string.Empty, dialog.Keywords, dialog.MuscleGroups, dialog.Description))
            {
                MessageBox.Show("No se pudo crear el ejercicio en la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            UpdateStatus($"Ejercicio '{name}' creado. Ahora importa una imagen.");
            LoadExercises(name);
        }

        private void ImportImageFromDisk()
        {
            if (_currentExercise == null)
            {
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen del ejercicio",
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|Todos los archivos|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (_imageDatabase.ImportImageForExercise(_currentExercise.ExerciseName, dialog.FileName))
            {
                UpdateStatus("Imagen importada correctamente.");
                LoadExercises(_currentExercise.ExerciseName);
            }
            else
            {
                MessageBox.Show("No se pudo importar la imagen. Verifica el archivo e inténtalo nuevamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenCurrentImage()
        {
            if (_currentExercise == null)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_currentExercise.ImagePath) && File.Exists(_currentExercise.ImagePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _currentExercise.ImagePath,
                        UseShellExecute = true
                    });
                    return;
                }

                if (_currentExercise.ImageData != null && _currentExercise.ImageData.Length > 0)
                {
                    _exportedTempImage = ExportImageToTemp(_currentExercise.ImageData, _currentExercise.ExerciseName);
                    if (_exportedTempImage != null)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = _exportedTempImage,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    MessageBox.Show("No hay imagen disponible para este ejercicio.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir la imagen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenImageFolder()
        {
            if (_currentExercise == null)
            {
                return;
            }

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
                MessageBox.Show("No se encontró el archivo de imagen o no está disponible.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string? ExportImageToTemp(byte[] imageData, string exerciseName)
        {
            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "GymRoutineImages");
                Directory.CreateDirectory(tempDir);

                var safeName = string.Join("_", exerciseName.Split(Path.GetInvalidFileNameChars()));
                var filePath = Path.Combine(tempDir, $"{safeName}_{Guid.NewGuid():N}.png");
                File.WriteAllBytes(filePath, imageData);
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExerciseManager] Export temp error: {ex.Message}");
                return null;
            }
        }

        private Image? LoadListThumbnail(ExerciseImageInfo exercise)
        {
            try
            {
                if (exercise.ImageData != null && exercise.ImageData.Length > 0)
                {
                    using var ms = new MemoryStream(exercise.ImageData);
                    using var original = Image.FromStream(ms);
                    return ResizeThumbnail(original);
                }

                if (!string.IsNullOrWhiteSpace(exercise.ImagePath) && File.Exists(exercise.ImagePath))
                {
                    using var original = Image.FromFile(exercise.ImagePath);
                    return ResizeThumbnail(original);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExerciseManager] Thumbnail error: {ex.Message}");
            }

            return null;
        }

        private static Image ResizeThumbnail(Image original)
        {
            var bitmap = new Bitmap(ThumbnailSize.Width, ThumbnailSize.Height);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.Clear(Color.White);

                var scale = Math.Min((double)ThumbnailSize.Width / original.Width, (double)ThumbnailSize.Height / original.Height);
                var width = (int)(original.Width * scale);
                var height = (int)(original.Height * scale);
                var x = (ThumbnailSize.Width - width) / 2;
                var y = (ThumbnailSize.Height - height) / 2;

                g.DrawImage(original, new Rectangle(x, y, width, height));
            }

            return bitmap;
        }

        private void PopulateMuscleChecklist(CheckedListBox list)
        {
            list.BeginUpdate();
            list.Items.Clear();
            foreach (var muscle in _defaultMuscleGroups)
            {
                list.Items.Add(muscle);
            }
            list.EndUpdate();
        }

        private void UpdateStatus(string message)
        {
            statusMessageLabel.Text = message;
        }

        private Image CreatePlaceholderThumbnail()
        {
            var bitmap = new Bitmap(ThumbnailSize.Width, ThumbnailSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.WhiteSmoke);
                using var pen = new Pen(Color.LightGray, 2);
                g.DrawRectangle(pen, 4, 4, ThumbnailSize.Width - 8, ThumbnailSize.Height - 8);
                using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
                var text = "IMG";
                var textSize = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.Gray,
                    new PointF((ThumbnailSize.Width - textSize.Width) / 2, (ThumbnailSize.Height - textSize.Height) / 2));
            }
            return bitmap;
        }
    }
}
