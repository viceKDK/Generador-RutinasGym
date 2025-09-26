using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymRoutineUI
{
    public partial class AboutForm : Form
    {
        private PictureBox logoBox;
        private Label appNameLabel;
        private Label versionLabel;
        private Label descriptionLabel;
        private Label copyrightLabel;
        private LinkLabel websiteLink;
        private LinkLabel supportLink;
        private Button okButton;
        private TextBox creditsTextBox;

        public AboutForm()
        {
            InitializeComponent();
            InitializeControls();
            LayoutControls();
        }

        private void InitializeComponent()
        {
            this.Text = "Acerca de Generador de Rutinas";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = true;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.BackColor = Color.White;
        }

        private void InitializeControls()
        {
            // Logo placeholder
            logoBox = new PictureBox
            {
                Size = new Size(128, 128),
                Location = new Point((this.Width - 128) / 2, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            // Create a simple logo with text
            var logoBitmap = new Bitmap(128, 128);
            using (var g = Graphics.FromImage(logoBitmap))
            {
                g.Clear(Color.FromArgb(0, 123, 255));
                using (var brush = new SolidBrush(Color.White))
                using (var font = new Font("Segoe UI", 16, FontStyle.Bold))
                {
                    var text = "\nGYM";
                    var textSize = g.MeasureString(text, font);
                    var x = (128 - textSize.Width) / 2;
                    var y = (128 - textSize.Height) / 2;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            logoBox.Image = logoBitmap;

            // App name
            appNameLabel = new Label
            {
                Text = "Generador de Rutinas de Gimnasio",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(50, 180),
                Size = new Size(400, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Version
            versionLabel = new Label
            {
                Text = "Versin 1.0.0",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(50, 220),
                Size = new Size(400, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Description
            descriptionLabel = new Label
            {
                Text = "Una aplicacin completa para crear rutinas de ejercicio personalizadas\n" +
                       "y exportarlas a documentos de Word profesionales.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(73, 80, 87),
                Location = new Point(30, 260),
                Size = new Size(440, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Copyright
            copyrightLabel = new Label
            {
                Text = " 2024 Gym Routine Generator. Todos los derechos reservados.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(30, 320),
                Size = new Size(440, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Website link
            websiteLink = new LinkLabel
            {
                Text = " Sitio web del proyecto",
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 350),
                Size = new Size(200, 25),
                LinkColor = Color.FromArgb(0, 123, 255),
                VisitedLinkColor = Color.FromArgb(0, 123, 255)
            };
            websiteLink.LinkClicked += WebsiteLink_LinkClicked;

            // Support link
            supportLink = new LinkLabel
            {
                Text = " Soporte tcnico",
                Font = new Font("Segoe UI", 10),
                Location = new Point(270, 350),
                Size = new Size(200, 25),
                LinkColor = Color.FromArgb(0, 123, 255),
                VisitedLinkColor = Color.FromArgb(0, 123, 255)
            };
            supportLink.LinkClicked += SupportLink_LinkClicked;

            // Credits
            creditsTextBox = new TextBox
            {
                Text = "CRDITOS Y TECNOLOGAS:\n\n" +
                       " .NET 8.0 - Framework de desarrollo\n" +
                       " Windows Forms - Interfaz de usuario\n" +
                       " Microsoft Word Interop - Exportacin de documentos\n" +
                       " Entity Framework Core - Gestin de datos\n" +
                       " Microsoft Extensions - Inyeccin de dependencias\n\n" +
                       "CARACTERSTICAS:\n\n" +
                       " Generacin inteligente de rutinas personalizadas\n" +
                       " Exportacin profesional a documentos Word\n" +
                       " Interfaz moderna con animaciones\n" +
                       " Diseo responsive y accesible\n" +
                       " Sistema de configuracin avanzado",
                Font = new Font("Segoe UI", 9),
                Location = new Point(30, 390),
                Size = new Size(440, 140),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // OK button
            okButton = new Button
            {
                Text = " Cerrar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(200, 550),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            okButton.FlatAppearance.BorderSize = 0;
        }

        private void LayoutControls()
        {
            this.Controls.AddRange(new Control[] {
                logoBox, appNameLabel, versionLabel, descriptionLabel,
                copyrightLabel, websiteLink, supportLink, creditsTextBox, okButton
            });
        }

        private void WebsiteLink_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://github.com/gym-routine-generator",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el enlace: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SupportLink_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "mailto:support@gym-routine-generator.com?subject=Soporte%20Tcnico",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el cliente de correo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Center the logo after form is loaded
            logoBox.Location = new Point((this.ClientSize.Width - logoBox.Width) / 2, logoBox.Location.Y);
        }
    }
}
