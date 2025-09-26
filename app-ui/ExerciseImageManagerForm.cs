using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Text;

namespace GymRoutineGenerator.UI
{
    public partial class ExerciseImageManagerForm : Form
    {
        private readonly ExerciseImageDatabase _imageDatabase;
        private ListBox exerciseListBox;
        private TextBox searchTextBox;
        private PictureBox imagePreview;
        private TextBox exerciseNameLabel;
        private Label exerciseDescriptionLabel;
        private TextBox keywordsTextBox;
        private TextBox muscleGroupsTextBox;
        private ModernButton selectImageButton;
        private ModernButton saveButton;
        private ModernButton deleteButton;
        private ModernButton addNewExerciseButton;
        private ModernButton goToMainButton;
        private Label statusLabel;
        private List<ExerciseImageInfo> _allExercises = new List<ExerciseImageInfo>();

        public ExerciseImageManagerForm()
        {
            _imageDatabase = new ExerciseImageDatabase();
            InitializeComponent();
            LoadExercises();
        }

        private void InitializeComponent()
        {
            this.Text = " Gestor de Imgenes de Ejercicios";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 800);
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Panel izquierdo - Lista de ejercicios
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var exercisesLabel = new Label
            {
                Text = " Ejercicios Disponibles",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Search box for exercises
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 8)
            };

            var searchLabel = new Label
            {
                Text = " Buscar ejercicio:",
                Dock = DockStyle.Left,
                Width = 130,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            searchTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            try { searchTextBox.PlaceholderText = "Escribe para buscar..."; } catch { /* PlaceholderText not supported */ }
            searchTextBox.TextChanged += (s, e) => ApplyExerciseFilter(searchTextBox.Text);

            searchPanel.Controls.Add(searchTextBox);
            searchPanel.Controls.Add(searchLabel);

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
                Text = " Agregar Ejercicio",
                Dock = DockStyle.Bottom,
                Height = 40,
                NormalColor = Color.FromArgb(40, 167, 69),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            addNewExerciseButton.Click += AddNewExerciseButton_Click;

            goToMainButton = new ModernButton
            {
                Text = " Ir a Ventana Principal",
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

            // Panel central - Vista previa de imagen
            var centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var imageLabel = new Label
            {
                Text = " Vista Previa de Imagen",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            imagePreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var imageButtonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Transparent
            };

            selectImageButton = new ModernButton
            {
                Text = " Seleccionar Imagen",
                Dock = DockStyle.Fill,
                Height = 40,
                NormalColor = Color.FromArgb(13, 110, 253),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            selectImageButton.Click += SelectImageButton_Click;

            imageButtonsPanel.Controls.Add(selectImageButton);

            centerPanel.Controls.Add(imagePreview);
            centerPanel.Controls.Add(imageLabel);
            centerPanel.Controls.Add(imageButtonsPanel);

            // Panel derecho - Detalles del ejercicio
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var detailsLabel = new Label
            {
                Text = " Detalles del Ejercicio",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            exerciseNameLabel = new TextBox
            {
                Text = "Selecciona un ejercicio",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 135, 84),
                Dock = DockStyle.Top,
                Height = 32,
                BorderStyle = BorderStyle.FixedSingle
            };

            exerciseDescriptionLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(108, 117, 125),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = false
            };

            // Keywords
            var keywordsLabel = new Label
            {
                Text = " Palabras clave (separadas por comas):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            keywordsTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 100,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Spacer below keywords to separate from muscle groups
            var keywordsSpacer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.Transparent
            };

            // Muscle Groups
            var muscleGroupsLabel = new Label
            {
                Text = " Grupos musculares (separados por comas):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            muscleGroupsTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 80,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Botones de accin
            var actionsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.Transparent
            };

            saveButton = new ModernButton
            {
                Text = " Guardar Cambios",
                Dock = DockStyle.Top,
                Height = 40,
                NormalColor = Color.FromArgb(40, 167, 69),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 5, 0, 5)
            };
            saveButton.Click += SaveButton_Click;

            deleteButton = new ModernButton
            {
                Text = " Eliminar Ejercicio",
                Dock = DockStyle.Top,
                Height = 40,
                NormalColor = Color.FromArgb(220, 53, 69),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 5, 0, 0)
            };
            deleteButton.Click += DeleteButton_Click;

            actionsPanel.Controls.Add(deleteButton);
            actionsPanel.Controls.Add(saveButton);

            // Status label
            statusLabel = new Label
            {
                Text = "Listo para gestionar imgenes",
                Dock = DockStyle.Bottom,
                Height = 25,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(108, 117, 125),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(10, 5, 10, 5)
            };

            // Agregar controles en orden correcto
            rightPanel.Controls.Add(muscleGroupsTextBox);
            rightPanel.Controls.Add(muscleGroupsLabel);
            rightPanel.Controls.Add(keywordsSpacer);
            rightPanel.Controls.Add(keywordsTextBox);
            rightPanel.Controls.Add(keywordsLabel);
            rightPanel.Controls.Add(exerciseDescriptionLabel);
            rightPanel.Controls.Add(exerciseNameLabel);
            rightPanel.Controls.Add(detailsLabel);
            rightPanel.Controls.Add(actionsPanel);

            // Agregar todos los paneles al formulario
            this.Controls.Add(centerPanel);
            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);
            this.Controls.Add(statusLabel);

            // Configurar controles iniciales
            EnableControls(false);
        }

        private void LoadExercises()
        {
            exerciseListBox.Items.Clear();
            _allExercises = _imageDatabase.GetAllExercises().OrderBy(e => e.ExerciseName).ToList();
            ApplyExerciseFilter(searchTextBox?.Text ?? string.Empty);
            UpdateStatus($"Cargados {_allExercises.Count} ejercicios");
        }

        private void ApplyExerciseFilter(string filter)
        {
            exerciseListBox.BeginUpdate();
            exerciseListBox.Items.Clear();
            var term = SanitizeLettersSpaces(filter ?? string.Empty);
            IEnumerable<ExerciseImageInfo> source = _allExercises;
            if (!string.IsNullOrEmpty(term))
            {
                source = source.Where(e =>
                {
                    var nameSan = SanitizeLettersSpaces(e.ExerciseName ?? string.Empty);
                    var descSan = SanitizeLettersSpaces(e.Description ?? string.Empty);
                    var kwSan = (e.Keywords ?? Array.Empty<string>()).Select(k => SanitizeLettersSpaces(k ?? string.Empty));
                    return (!string.IsNullOrEmpty(nameSan) && nameSan.Contains(term))
                           || (!string.IsNullOrEmpty(descSan) && descSan.Contains(term))
                           || kwSan.Any(k => !string.IsNullOrEmpty(k) && k.Contains(term));
                });
            }

            foreach (var exercise in source)
            {
                var displayText = SanitizeLettersSpaces(exercise.ExerciseName ?? string.Empty);
                // optional indicator if image exists
                // if desired, add icons or marks here

                exerciseListBox.Items.Add(new ExerciseListItem
                {
                    DisplayText = displayText,
                    ExerciseInfo = exercise
                });
            }
            exerciseListBox.EndUpdate();
        }

        private void ExerciseListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (exerciseListBox.SelectedItem is ExerciseListItem selectedItem)
            {
                LoadExerciseDetails(selectedItem.ExerciseInfo);
                EnableControls(true);
            }
            else
            {
                ClearExerciseDetails();
                EnableControls(false);
            }
        }

        private void LoadExerciseDetails(ExerciseImageInfo exerciseInfo)
        {
            exerciseNameLabel.Text = SanitizeLettersSpaces(exerciseInfo.ExerciseName);
            exerciseDescriptionLabel.Text = SanitizeLettersSpaces(exerciseInfo.Description);
            keywordsTextBox.Text = string.Join(", ", exerciseInfo.Keywords.Select(k => SanitizeLettersSpaces(k)));
            muscleGroupsTextBox.Text = string.Join(", ", exerciseInfo.MuscleGroups.Select(m => SanitizeLettersSpaces(m)));

            // Cargar imagen si existe
            if (!string.IsNullOrEmpty(exerciseInfo.ImagePath) && File.Exists(exerciseInfo.ImagePath))
            {
                try
                {
                    using (var fileStream = new FileStream(exerciseInfo.ImagePath, FileMode.Open, FileAccess.Read))
                    {
                        imagePreview.Image?.Dispose();
                        imagePreview.Image = Image.FromStream(fileStream);
                    }
                    UpdateStatus($"Imagen cargada: {Path.GetFileName(exerciseInfo.ImagePath)}");
                }
                catch (Exception ex)
                {
                    imagePreview.Image = null;
                    UpdateStatus($"Error cargando imagen: {ex.Message}");
                }
            }
            else
            {
                imagePreview.Image = null;
                UpdateStatus("Sin imagen asignada");
            }
        }

        private void ClearExerciseDetails()
        {
            exerciseNameLabel.Text = "Selecciona un ejercicio";
            exerciseDescriptionLabel.Text = "";
            keywordsTextBox.Text = "";
            muscleGroupsTextBox.Text = "";
            imagePreview.Image?.Dispose();
            imagePreview.Image = null;
        }

        private void EnableControls(bool enabled)
        {
            selectImageButton.Enabled = enabled;
            saveButton.Enabled = enabled;
            deleteButton.Enabled = enabled;
            keywordsTextBox.Enabled = enabled;
            muscleGroupsTextBox.Enabled = enabled;
        }

        private void SelectImageButton_Click(object sender, EventArgs e)
        {
            if (!(exerciseListBox.SelectedItem is ExerciseListItem selectedItem))
                return;

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Seleccionar imagen del ejercicio";
                openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos los archivos|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _imageDatabase.ImportImageForExercise(selectedItem.ExerciseInfo.ExerciseName, openFileDialog.FileName);
                        LoadExerciseDetails(selectedItem.ExerciseInfo);
                        LoadExercises(); // Refresh list to show updated status
                        UpdateStatus($"Imagen importada exitosamente");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importando imagen: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!(exerciseListBox.SelectedItem is ExerciseListItem selectedItem))
                return;

            try
            {
                var newName = SanitizeLettersSpaces(exerciseNameLabel.Text);
                var keywords = keywordsTextBox.Text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => SanitizeLettersSpaces(k))
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .ToArray();

                var muscleGroups = muscleGroupsTextBox.Text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => SanitizeLettersSpaces(m))
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToArray();

                var desc = SanitizeLettersSpaces(exerciseDescriptionLabel.Text);

                var oldName = selectedItem.ExerciseInfo.ExerciseName;
                if (!string.Equals(newName, oldName, StringComparison.Ordinal))
                {
                    _imageDatabase.AddOrUpdateExercise(
                        newName,
                        selectedItem.ExerciseInfo.ImagePath,
                        keywords,
                        muscleGroups,
                        desc);
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
                        desc);
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
                $"Ests seguro de que deseas eliminar el ejercicio '{selectedItem.ExerciseInfo.ExerciseName}'?",
                "Confirmar eliminacin",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _imageDatabase.RemoveExercise(selectedItem.ExerciseInfo.ExerciseName);
                    LoadExercises();
                    ClearExerciseDetails();
                    EnableControls(false);
                    UpdateStatus("Ejercicio eliminado");
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
                    var name = SanitizeLettersSpaces(dialog.ExerciseName);
                    var keywords = (dialog.Keywords ?? Array.Empty<string>())
                        .Select(k => SanitizeLettersSpaces(k))
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .ToArray();
                    var muscles = (dialog.MuscleGroups ?? Array.Empty<string>())
                        .Select(m => SanitizeLettersSpaces(m))
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .ToArray();
                    var desc = SanitizeLettersSpaces(dialog.Description);

                    _imageDatabase.AddOrUpdateExercise(
                        name,
                        "",
                        keywords,
                        muscles,
                        desc);

                    LoadExercises();
                    UpdateStatus($"Ejercicio '{name}' agregado");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error agregando ejercicio: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        private void GoToMainButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Buscar la ventana principal existente
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm is MainForm mainForm)
                    {
                        mainForm.WindowState = FormWindowState.Normal;
                        mainForm.BringToFront();
                        mainForm.Activate();
                        this.Close();
                        return;
                    }
                }

                // Si no se encuentra, crear una nueva instancia
                var newMainForm = new MainForm();
                newMainForm.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error abriendo ventana principal: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                imagePreview?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }

        private class ExerciseListItem
        {
            public string DisplayText { get; set; } = "";
            public ExerciseImageInfo ExerciseInfo { get; set; } = new ExerciseImageInfo();

            public override string ToString()
            {
                return DisplayText;
            }
        }

        // Helpers to sanitize strings: keep only letters and spaces, remove diacritics
        private static string SanitizeLettersSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var text = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category == UnicodeCategory.NonSpacingMark) continue;
                var c = ch == 'ñ' ? 'n' : ch;
                if (char.IsLetter(c) || char.IsWhiteSpace(c)) sb.Append(c);
            }
            var result = sb.ToString();
            result = System.Text.RegularExpressions.Regex.Replace(result, "\u0020+", " ").Trim();
            return result;
        }
    }
}
