using System;
using System.Drawing;
using System.Windows.Forms;

namespace GymRoutineUI
{
    public partial class SettingsForm : Form
    {
        private TabControl tabControl;
        private TabPage generalTab;
        private TabPage exportTab;
        private TabPage uiTab;

        // General settings controls
        private CheckBox autoSaveCheckBox;
        private CheckBox showTipsCheckBox;
        private ComboBox languageComboBox;

        // Export settings controls
        private TextBox defaultPathTextBox;
        private Button browseButton;
        private CheckBox openAfterExportCheckBox;
        private ComboBox templateComboBox;

        // UI settings controls
        private CheckBox animationsCheckBox;
        private ComboBox themeComboBox;
        private TrackBar fontSizeTrackBar;
        private Label fontSizeLabel;

        // Action buttons
        private Button saveButton;
        private Button cancelButton;
        private Button resetButton;

        public SettingsForm()
        {
            InitializeComponent();
            InitializeControls();
            LayoutControls();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = " Configuracin";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = true;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.BackColor = Color.FromArgb(248, 249, 250);
        }

        private void InitializeControls()
        {
            // Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 380,
                Font = new Font("Segoe UI", 10)
            };

            // General Tab
            generalTab = new TabPage("General");
            autoSaveCheckBox = new CheckBox
            {
                Text = "Guardar automticamente al generar rutinas",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 30),
                Size = new Size(300, 23),
                Checked = true
            };

            showTipsCheckBox = new CheckBox
            {
                Text = "Mostrar consejos de entrenamiento",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 65),
                Size = new Size(250, 23),
                Checked = true
            };

            var languageLabel = new Label
            {
                Text = "Idioma:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 105),
                Size = new Size(60, 23)
            };

            languageComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(90, 103),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            languageComboBox.Items.AddRange(new[] { "Espaol", "English" });
            languageComboBox.SelectedIndex = 0;

            generalTab.Controls.AddRange(new Control[] { autoSaveCheckBox, showTipsCheckBox, languageLabel, languageComboBox });

            // Export Tab
            exportTab = new TabPage("Exportacin");
            var pathLabel = new Label
            {
                Text = "Ruta predeterminada para exportar:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 30),
                Size = new Size(250, 23)
            };

            defaultPathTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 55),
                Size = new Size(350, 25),
                Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            browseButton = new Button
            {
                Text = "Examinar...",
                Font = new Font("Segoe UI", 9),
                Location = new Point(380, 55),
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White
            };
            browseButton.FlatAppearance.BorderSize = 0;
            browseButton.Click += BrowseButton_Click;

            openAfterExportCheckBox = new CheckBox
            {
                Text = "Abrir archivo automticamente despus de exportar",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 100),
                Size = new Size(350, 23),
                Checked = true
            };

            var templateLabel = new Label
            {
                Text = "Plantilla predeterminada:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 135),
                Size = new Size(180, 23)
            };

            templateComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(210, 133),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            templateComboBox.Items.AddRange(new[] { "Plantilla Bsica", "Plantilla Profesional", "Plantilla Detallada" });
            templateComboBox.SelectedIndex = 0;

            exportTab.Controls.AddRange(new Control[] { pathLabel, defaultPathTextBox, browseButton, openAfterExportCheckBox, templateLabel, templateComboBox });

            // UI Tab
            uiTab = new TabPage("Interfaz");
            animationsCheckBox = new CheckBox
            {
                Text = "Activar animaciones",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 30),
                Size = new Size(150, 23),
                Checked = true
            };

            var themeLabel = new Label
            {
                Text = "Tema:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 70),
                Size = new Size(50, 23)
            };

            themeComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 9),
                Location = new Point(80, 68),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            themeComboBox.Items.AddRange(new[] { "Claro", "Oscuro", "Automtico" });
            themeComboBox.SelectedIndex = 0;

            var fontSizeMainLabel = new Label
            {
                Text = "Tamao de fuente:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 110),
                Size = new Size(130, 23)
            };

            fontSizeTrackBar = new TrackBar
            {
                Location = new Point(20, 135),
                Size = new Size(200, 45),
                Minimum = 8,
                Maximum = 16,
                Value = 10,
                TickFrequency = 1
            };
            fontSizeTrackBar.ValueChanged += FontSizeTrackBar_ValueChanged;

            fontSizeLabel = new Label
            {
                Text = "10 pt",
                Font = new Font("Segoe UI", 9),
                Location = new Point(230, 145),
                Size = new Size(40, 23)
            };

            uiTab.Controls.AddRange(new Control[] { animationsCheckBox, themeLabel, themeComboBox, fontSizeMainLabel, fontSizeTrackBar, fontSizeLabel });

            tabControl.TabPages.AddRange(new[] { generalTab, exportTab, uiTab });

            // Action buttons
            saveButton = new Button
            {
                Text = " Guardar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(360, 420),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = " Cancelar",
                Font = new Font("Segoe UI", 10),
                Location = new Point(470, 420),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += CancelButton_Click;

            resetButton = new Button
            {
                Text = " Restablecer",
                Font = new Font("Segoe UI", 10),
                Location = new Point(230, 420),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black
            };
            resetButton.FlatAppearance.BorderSize = 0;
            resetButton.Click += ResetButton_Click;
        }

        private void LayoutControls()
        {
            this.Controls.AddRange(new Control[] { tabControl, saveButton, cancelButton, resetButton });
        }

        private void LoadCurrentSettings()
        {
            // Load current settings from configuration
            // This would typically read from a config file or registry
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Selecciona la carpeta predeterminada para exportar rutinas";
            folderDialog.SelectedPath = defaultPathTextBox.Text;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                defaultPathTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void FontSizeTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            fontSizeLabel.Text = $"{fontSizeTrackBar.Value} pt";
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // Save settings logic here
            var message = "Configuracin guardada exitosamente.\n\n" +
                         "Algunos cambios pueden requerir reiniciar la aplicacin.";

            MessageBox.Show(message, "Configuracin Guardada",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ResetButton_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Ests seguro de que deseas restablecer toda la configuracin a los valores predeterminados?",
                "Restablecer Configuracin",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Reset all controls to default values
                autoSaveCheckBox.Checked = true;
                showTipsCheckBox.Checked = true;
                languageComboBox.SelectedIndex = 0;
                defaultPathTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                openAfterExportCheckBox.Checked = true;
                templateComboBox.SelectedIndex = 0;
                animationsCheckBox.Checked = true;
                themeComboBox.SelectedIndex = 0;
                fontSizeTrackBar.Value = 10;
            }
        }
    }
}
