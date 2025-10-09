using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    public partial class ImprovedExerciseImageManagerForm : Form
    {
        private readonly SQLiteExerciseImageDatabase _imageDatabase;
        private ListBox exerciseListBox;
        private TextBox searchTextBox;
        private PictureBox imagePreview;
        private TextBox exerciseNameTextBox;
        private Label exerciseDescriptionLabel;

        // Controles avanzados (ocultos por defecto)
        private Panel advancedPanel;
        private CheckedListBox muscleGroupsCheckedListBox;
        private TextBox muscleGroupSearchBox;
        private TextBox keywordsTextBox;
        private ModernButton toggleAdvancedButton;

        private ModernButton selectImageButton;
        private ModernButton saveButton;
        private ModernButton deleteButton;
        private ModernButton addNewExerciseButton;
        private ModernButton goToMainButton;
        private Label statusLabel;
        private Label dropZoneLabel;

        private List<ExerciseImageInfo> _allExercises = new List<ExerciseImageInfo>();
        private bool _advancedPanelExpanded = false;

        // Lista completa de grupos musculares
        private readonly string[] _allMuscleGroups = new[]
        {
            "Pecho", "Espalda", "Hombros", "Bíceps", "Tríceps", "Antebrazos",
            "Abdominales", "Oblicuos", "Core", "Cuádriceps", "Isquiotibiales",
            "Glúteos", "Gemelos", "Pantorrillas", "Aductores", "Abductores",
            "Trapecio", "Dorsales", "Lumbares", "Cuello", "Cardio"
        };

        public ImprovedExerciseImageManagerForm()
        {
            _imageDatabase = new SQLiteExerciseImageDatabase();
            InitializeComponent();
            LoadExercises();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Liberar imagen antes de cerrar
                if (imagePreview?.Image != null)
                {
                    imagePreview.Image.Dispose();
                    imagePreview.Image = null;
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Gestor de Imagenes de Ejercicios";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 800);
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Panel izquierdo - Lista de ejercicios
            var leftPanel = CreateLeftPanel();

            // Panel central - Vista previa con Drag & Drop
            var centerPanel = CreateCenterPanelWithDragDrop();

            // Panel derecho - Detalles (simple por defecto)
            var rightPanel = CreateRightPanel();

            // Status bar
            var statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(233, 236, 239)
            };

            statusLabel = new Label
            {
                Text = "Listo para agregar ejercicios",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(73, 80, 87),
                Padding = new Padding(10, 0, 0, 0)
            };
            statusBar.Controls.Add(statusLabel);

            this.Controls.Add(centerPanel);
            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);
            this.Controls.Add(statusBar);
        }

        private Panel CreateLeftPanel()
        {
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var exercisesLabel = new Label
            {
                Text = "Ejercicios Disponibles",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Search box
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 8)
            };

            searchTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            searchTextBox.PlaceholderText = "Buscar ejercicio...";
            searchTextBox.TextChanged += (s, e) => ApplyExerciseFilter(searchTextBox.Text);
            searchPanel.Controls.Add(searchTextBox);

            exerciseListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 249, 250),
                SelectionMode = SelectionMode.One
            };
            exerciseListBox.SelectedIndexChanged += ExerciseListBox_SelectedIndexChanged;

            addNewExerciseButton = new ModernButton
            {
                Text = "Agregar Ejercicio",
                Dock = DockStyle.Bottom,
                Height = 40,
                NormalColor = Color.FromArgb(40, 167, 69),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            addNewExerciseButton.Click += AddNewExerciseButton_Click;

            goToMainButton = new ModernButton
            {
                Text = "Ir a Ventana Principal",
                Dock = DockStyle.Bottom,
                Height = 40,
                NormalColor = Color.FromArgb(108, 117, 125),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            goToMainButton.Click += GoToMainButton_Click;

            leftPanel.Controls.Add(exerciseListBox);
            leftPanel.Controls.Add(searchPanel);
            leftPanel.Controls.Add(exercisesLabel);
            leftPanel.Controls.Add(addNewExerciseButton);
            leftPanel.Controls.Add(goToMainButton);

            return leftPanel;
        }

        private Panel CreateCenterPanelWithDragDrop()
        {
            var centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 50, 40, 50)
            };

            var imageLabel = new Label
            {
                Text = "Vista Previa de Imagen",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Drop zone label (aparece cuando no hay imagen)
            dropZoneLabel = new Label
            {
                Text = "Arrastra una imagen aqui\n\no\n\nClick en 'Seleccionar Imagen'",
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                ForeColor = Color.FromArgb(108, 117, 125),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            imagePreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250),
                Visible = false // Oculto por defecto, mostrar dropZoneLabel
            };

            // Habilitar Drag & Drop en imagePreview
            imagePreview.AllowDrop = true;
            imagePreview.DragEnter += ImagePreview_DragEnter;
            imagePreview.DragDrop += ImagePreview_DragDrop;

            // También en dropZoneLabel
            dropZoneLabel.AllowDrop = true;
            dropZoneLabel.DragEnter += ImagePreview_DragEnter;
            dropZoneLabel.DragDrop += ImagePreview_DragDrop;

            var imageButtonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Transparent
            };

            selectImageButton = new ModernButton
            {
                Text = "Seleccionar Imagen",
                Dock = DockStyle.Fill,
                Height = 40,
                NormalColor = Color.FromArgb(13, 110, 253),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            selectImageButton.Click += SelectImageButton_Click;

            imageButtonsPanel.Controls.Add(selectImageButton);

            centerPanel.Controls.Add(imagePreview);
            centerPanel.Controls.Add(dropZoneLabel);
            centerPanel.Controls.Add(imageLabel);
            centerPanel.Controls.Add(imageButtonsPanel);

            return centerPanel;
        }

        private Panel CreateRightPanel()
        {
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350,
                BackColor = Color.White,
                Padding = new Padding(10),
                AutoScroll = true
            };

            var detailsLabel = new Label
            {
                Text = "Detalles del Ejercicio",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var nameLabel = new Label
            {
                Text = "Nombre del ejercicio:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft
            };

            exerciseNameTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Dock = DockStyle.Top,
                Height = 32,
                BorderStyle = BorderStyle.FixedSingle
            };

            exerciseDescriptionLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(108, 117, 125),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = false
            };

            var spacer1 = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent };

            // Botón para mostrar/ocultar info avanzada
            toggleAdvancedButton = new ModernButton
            {
                Text = "Mostrar Info Avanzada (Grupos Musculares, Keywords)",
                Dock = DockStyle.Top,
                Height = 40,
                NormalColor = Color.FromArgb(108, 117, 125),
                HoverColor = Color.FromArgb(90, 100, 110),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            toggleAdvancedButton.Click += ToggleAdvancedButton_Click;

            var spacer2 = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent };

            // Panel avanzado (oculto por defecto)
            advancedPanel = CreateAdvancedPanel();
            advancedPanel.Visible = false;

            // Botones de acción
            var actionsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.Transparent
            };

            saveButton = new ModernButton
            {
                Text = "Guardar Cambios",
                Dock = DockStyle.Top,
                Height = 40,
                NormalColor = Color.FromArgb(40, 167, 69),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 5, 0, 5)
            };
            saveButton.Click += SaveButton_Click;

            deleteButton = new ModernButton
            {
                Text = "Eliminar Ejercicio",
                Dock = DockStyle.Top,
                Height = 40,
                NormalColor = Color.FromArgb(220, 53, 69),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 5, 0, 0)
            };
            deleteButton.Click += DeleteButton_Click;

            actionsPanel.Controls.Add(deleteButton);
            actionsPanel.Controls.Add(saveButton);

            rightPanel.Controls.Add(advancedPanel);
            rightPanel.Controls.Add(spacer2);
            rightPanel.Controls.Add(toggleAdvancedButton);
            rightPanel.Controls.Add(spacer1);
            rightPanel.Controls.Add(exerciseDescriptionLabel);
            rightPanel.Controls.Add(exerciseNameTextBox);
            rightPanel.Controls.Add(nameLabel);
            rightPanel.Controls.Add(detailsLabel);
            rightPanel.Controls.Add(actionsPanel);

            return rightPanel;
        }

        private Panel CreateAdvancedPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 400,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Grupos musculares con multiselect
            var muscleGroupsLabel = new Label
            {
                Text = "Grupos Musculares:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Búsqueda de grupos musculares
            muscleGroupSearchBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            muscleGroupSearchBox.PlaceholderText = "Buscar grupo muscular...";
            muscleGroupSearchBox.TextChanged += MuscleGroupSearchBox_TextChanged;

            muscleGroupsCheckedListBox = new CheckedListBox
            {
                Dock = DockStyle.Top,
                Height = 180,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };

            // Agregar todos los grupos musculares
            foreach (var group in _allMuscleGroups)
            {
                muscleGroupsCheckedListBox.Items.Add(group);
            }

            var spacer = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent };

            // Keywords
            var keywordsLabel = new Label
            {
                Text = "Palabras Clave (separadas por comas):",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            keywordsTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 80,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            panel.Controls.Add(keywordsTextBox);
            panel.Controls.Add(keywordsLabel);
            panel.Controls.Add(spacer);
            panel.Controls.Add(muscleGroupsCheckedListBox);
            panel.Controls.Add(muscleGroupSearchBox);
            panel.Controls.Add(muscleGroupsLabel);

            return panel;
        }

        #region Event Handlers

        private void ToggleAdvancedButton_Click(object sender, EventArgs e)
        {
            _advancedPanelExpanded = !_advancedPanelExpanded;
            advancedPanel.Visible = _advancedPanelExpanded;

            if (_advancedPanelExpanded)
            {
                toggleAdvancedButton.Text = "Ocultar Info Avanzada";
                toggleAdvancedButton.NormalColor = Color.FromArgb(13, 110, 253);
            }
            else
            {
                toggleAdvancedButton.Text = "Mostrar Info Avanzada (Grupos Musculares, Keywords)";
                toggleAdvancedButton.NormalColor = Color.FromArgb(108, 117, 125);
            }
        }

        private void MuscleGroupSearchBox_TextChanged(object sender, EventArgs e)
        {
            var searchText = muscleGroupSearchBox.Text.ToLower();

            muscleGroupsCheckedListBox.Items.Clear();

            foreach (var group in _allMuscleGroups)
            {
                if (string.IsNullOrWhiteSpace(searchText) || group.ToLower().Contains(searchText))
                {
                    muscleGroupsCheckedListBox.Items.Add(group);
                }
            }
        }

        private void ImagePreview_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];
                var ext = Path.GetExtension(file).ToLower();

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif" || ext == ".webp")
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void ImagePreview_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];

                ImportImage(file);
            }
        }

        private void SelectImageButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Seleccionar imagen del ejercicio";
                openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|Todos los archivos|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ImportImage(openFileDialog.FileName);
                }
            }
        }

        private void ImportImage(string imagePath)
        {
            if (!(exerciseListBox.SelectedItem is ExerciseListItem selectedItem))
            {
                MessageBox.Show("Por favor selecciona un ejercicio primero", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var exerciseName = selectedItem.ExerciseInfo.ExerciseName;

                _imageDatabase.ImportImageForExercise(exerciseName, imagePath);

                // Recargar ejercicio actualizado de BD
                var updatedExercise = _imageDatabase.FindExerciseImage(exerciseName);

                if (updatedExercise != null)
                {
                    if (updatedExercise.ImageData != null && updatedExercise.ImageData.Length > 0)
                    {
                        // Cargar imagen de forma segura desde bytes
                        LoadImageSafely(updatedExercise.ImageData);

                        // Actualizar el item en memoria
                        selectedItem.ExerciseInfo.ImageData = updatedExercise.ImageData;
                    }
                }

                // Recargar lista manteniendo seleccion
                var selectedIndex = exerciseListBox.SelectedIndex;
                LoadExercises();

                // Restaurar seleccion si es posible
                if (selectedIndex >= 0 && selectedIndex < exerciseListBox.Items.Count)
                {
                    exerciseListBox.SelectedIndex = selectedIndex;
                }

                UpdateStatus($"Imagen importada exitosamente para '{exerciseName}'");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importando imagen: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Error: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!(exerciseListBox.SelectedItem is ExerciseListItem selectedItem))
                return;

            try
            {
                var newName = SanitizeLettersSpaces(exerciseNameTextBox.Text);

                // Obtener keywords
                var keywords = keywordsTextBox.Text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => SanitizeLettersSpaces(k))
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .ToArray();

                // Obtener grupos musculares seleccionados
                var muscleGroups = muscleGroupsCheckedListBox.CheckedItems
                    .Cast<string>()
                    .ToArray();

                var oldName = selectedItem.ExerciseInfo.ExerciseName;

                if (!string.Equals(newName, oldName, StringComparison.Ordinal))
                {
                    _imageDatabase.AddOrUpdateExercise(
                        newName,
                        selectedItem.ExerciseInfo.ImagePath,
                        keywords,
                        muscleGroups,
                        "");
                    _imageDatabase.RemoveExercise(oldName);
                    selectedItem.ExerciseInfo.ExerciseName = newName;
                }
                else
                {
                    _imageDatabase.AddOrUpdateExercise(
                        newName,
                        selectedItem.ExerciseInfo.ImagePath,
                        keywords,
                        muscleGroups,
                        "");
                }

                UpdateStatus("Cambios guardados exitosamente");
                LoadExercises();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error guardando cambios: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Error: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (!(exerciseListBox.SelectedItem is ExerciseListItem selectedItem))
                return;

            var result = MessageBox.Show(
                $"Estas seguro que deseas eliminar '{selectedItem.ExerciseInfo.ExerciseName}'?",
                "Confirmar eliminacion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _imageDatabase.RemoveExercise(selectedItem.ExerciseInfo.ExerciseName);
                    UpdateStatus($"Ejercicio '{selectedItem.ExerciseInfo.ExerciseName}' eliminado");
                    LoadExercises();
                    ClearExerciseDetails();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error eliminando ejercicio: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddNewExerciseButton_Click(object sender, EventArgs e)
        {
            var dialog = new AddExerciseDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _imageDatabase.AddOrUpdateExercise(dialog.ExerciseName, "", null, null, dialog.Description);
                    UpdateStatus($"Ejercicio '{dialog.ExerciseName}' agregado");
                    LoadExercises();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error agregando ejercicio: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GoToMainButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExerciseListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (exerciseListBox.SelectedItem is ExerciseListItem selectedItem)
            {
                LoadExerciseDetails(selectedItem.ExerciseInfo);
            }
        }

        #endregion

        #region Helper Methods

        private void LoadExercises()
        {
            _allExercises = _imageDatabase.GetAllExercises();
            ApplyExerciseFilter(searchTextBox.Text);
            exerciseListBox.Refresh();
        }

        private void ApplyExerciseFilter(string filter)
        {
            exerciseListBox.Items.Clear();

            var filtered = string.IsNullOrWhiteSpace(filter)
                ? _allExercises
                : _allExercises.Where(e => e.ExerciseName.ToLower().Contains(filter.ToLower())).ToList();

            foreach (var exercise in filtered.OrderBy(e => e.ExerciseName))
            {
                var hasImage = exercise.ImageData != null && exercise.ImageData.Length > 0;
                var icon = hasImage ? "[OK]" : "";
                var displayText = string.IsNullOrEmpty(icon) ? exercise.ExerciseName : $"{icon} {exercise.ExerciseName}";

                exerciseListBox.Items.Add(new ExerciseListItem
                {
                    DisplayText = displayText,
                    ExerciseInfo = exercise
                });
            }
        }

        private void LoadExerciseDetails(ExerciseImageInfo exercise)
        {
            exerciseNameTextBox.Text = exercise.ExerciseName;
            exerciseDescriptionLabel.Text = string.IsNullOrWhiteSpace(exercise.Description)
                ? "Sin descripcion"
                : exercise.Description;

            // Cargar imagen de forma segura desde bytes
            if (exercise.ImageData != null && exercise.ImageData.Length > 0)
            {
                LoadImageSafely(exercise.ImageData);
            }
            else
            {
                dropZoneLabel.Visible = true;
                imagePreview.Visible = false;
                if (imagePreview.Image != null)
                {
                    imagePreview.Image.Dispose();
                    imagePreview.Image = null;
                }
            }

            // Cargar keywords
            keywordsTextBox.Text = exercise.Keywords != null && exercise.Keywords.Length > 0
                ? string.Join(", ", exercise.Keywords)
                : "";

            // Cargar grupos musculares (marcar los checkboxes correspondientes)
            for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++)
            {
                var group = muscleGroupsCheckedListBox.Items[i].ToString();
                var isChecked = exercise.MuscleGroups != null && exercise.MuscleGroups.Contains(group);
                muscleGroupsCheckedListBox.SetItemChecked(i, isChecked);
            }
        }

        private void ClearExerciseDetails()
        {
            exerciseNameTextBox.Text = "";
            exerciseDescriptionLabel.Text = "";
            keywordsTextBox.Text = "";
            dropZoneLabel.Visible = true;
            imagePreview.Visible = false;

            // Liberar imagen anterior
            if (imagePreview.Image != null)
            {
                imagePreview.Image.Dispose();
                imagePreview.Image = null;
            }

            for (int i = 0; i < muscleGroupsCheckedListBox.Items.Count; i++)
            {
                muscleGroupsCheckedListBox.SetItemChecked(i, false);
            }
        }

        private void LoadImageSafely(byte[]? imageData)
        {
            try
            {
                // Liberar imagen anterior primero
                if (imagePreview.Image != null)
                {
                    imagePreview.Image.Dispose();
                    imagePreview.Image = null;
                }

                if (imageData != null && imageData.Length > 0)
                {
                    // Cargar desde bytes - crear copia para que Image no dependa del stream
                    using (var ms = new MemoryStream(imageData))
                    {
                        var tempImage = Image.FromStream(ms);
                        imagePreview.Image = new Bitmap(tempImage);
                        tempImage.Dispose();
                    }

                    dropZoneLabel.Visible = false;
                    imagePreview.Visible = true;
                    imagePreview.Refresh();
                    imagePreview.Invalidate();
                }
                else
                {
                    dropZoneLabel.Visible = true;
                    imagePreview.Visible = false;
                }
            }
            catch (OutOfMemoryException)
            {
                dropZoneLabel.Visible = true;
                imagePreview.Visible = false;
                MessageBox.Show("La imagen es demasiado grande o esta corrupta. Intenta con una imagen mas pequeña.",
                    "Error de memoria", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                dropZoneLabel.Visible = true;
                imagePreview.Visible = false;
                UpdateStatus($"Error cargando imagen: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }

        private string SanitizeLettersSpaces(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return new string(input.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-').ToArray()).Trim();
        }

        #endregion

        private class ExerciseListItem
        {
            public string DisplayText { get; set; }
            public ExerciseImageInfo ExerciseInfo { get; set; }

            public override string ToString() => DisplayText;
        }
    }
}
