using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    public partial class AddExerciseDialog : Form
    {
        private TextBox exerciseNameTextBox;
        private TextBox descriptionTextBox;
        private TextBox keywordsTextBox;
        private CheckedListBox muscleGroupsCheckedListBox;
        private ModernButton okButton;
        private ModernButton cancelButton;

        public string ExerciseName => exerciseNameTextBox.Text.Trim();
        public string Description => descriptionTextBox.Text.Trim();
        public string[] Keywords => keywordsTextBox.Text.Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToArray();
        public string[] MuscleGroups => muscleGroupsCheckedListBox.CheckedItems
            .Cast<string>()
            .ToArray();

        public AddExerciseDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Agregar Nuevo Ejercicio";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Titulo
            var titleLabel = new Label
            {
                Text = "Informacion del Nuevo Ejercicio",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 135, 84),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Nombre del ejercicio
            var nameLabel = new Label
            {
                Text = "Nombre del ejercicio:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.BottomLeft
            };

            exerciseNameTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Descripcion
            var descLabel = new Label
            {
                Text = "Descripcion:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.BottomLeft
            };

            descriptionTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Palabras clave
            var keywordsLabel = new Label
            {
                Text = "Palabras clave (separadas por comas):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.BottomLeft
            };

            keywordsTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Grupos musculares
            var muscleLabel = new Label
            {
                Text = "Grupos musculares (selecciona los que apliquen):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.BottomLeft
            };

            muscleGroupsCheckedListBox = new CheckedListBox
            {
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };

            // Cargar grupos musculares disponibles
            var muscleGroups = new[] {
                "Pecho", "Espalda", "Hombros", "Biceps", "Triceps",
                "Cuadriceps", "Isquiotibiales", "Gluteos", "Pantorrillas",
                "Abdomen", "Core", "Antebrazos", "Trapecio"
            };

            foreach (var group in muscleGroups)
            {
                muscleGroupsCheckedListBox.Items.Add(group);
            }

            // Panel de botones
            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.Transparent
            };

            cancelButton = new ModernButton
            {
                Text = "Cancelar",
                Size = new Size(120, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                NormalColor = Color.FromArgb(108, 117, 125),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                DialogResult = DialogResult.Cancel
            };
            cancelButton.Location = new Point(buttonsPanel.Width - 260, 10);

            okButton = new ModernButton
            {
                Text = "Agregar",
                Size = new Size(120, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                NormalColor = Color.FromArgb(40, 167, 69),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            okButton.Location = new Point(buttonsPanel.Width - 130, 10);
            okButton.Click += OkButton_Click;

            buttonsPanel.Controls.Add(okButton);
            buttonsPanel.Controls.Add(cancelButton);

            // Agregar controles al panel principal en orden correcto (desde abajo hacia arriba para Dock.Top)
            mainPanel.Controls.Add(buttonsPanel);
            mainPanel.Controls.Add(muscleGroupsCheckedListBox);
            mainPanel.Controls.Add(muscleLabel);
            mainPanel.Controls.Add(keywordsTextBox);
            mainPanel.Controls.Add(keywordsLabel);
            mainPanel.Controls.Add(descriptionTextBox);
            mainPanel.Controls.Add(descLabel);
            mainPanel.Controls.Add(exerciseNameTextBox);
            mainPanel.Controls.Add(nameLabel);
            mainPanel.Controls.Add(titleLabel);

            this.Controls.Add(mainPanel);

            // Configurar comportamiento
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
            exerciseNameTextBox.Focus();

            // Ajustar posiciones despus de agregar controles
            this.Load += (s, e) =>
            {
                cancelButton.Location = new Point(buttonsPanel.Width - 260, 10);
                okButton.Location = new Point(buttonsPanel.Width - 130, 10);
            };
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(exerciseNameTextBox.Text))
            {
                MessageBox.Show("Por favor, ingresa el nombre del ejercicio.", "Campo requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                exerciseNameTextBox.Focus();
                return;
            }

            // Si llegamos aqu, todo est bien y el dilogo se cerrar con DialogResult.OK
        }
    }
}
