using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    public partial class RoutinePreviewForm : Form
    {
        private RichTextBox previewTextBox = null!;
        private Button printButton = null!;
        private Button exportButton = null!;
        private Button closeButton = null!;
        private Button editButton = null!;
        private ToolStrip toolStrip = null!;
        private Label statusLabel = null!;
        private Panel controlPanel = null!;
        private StatusStrip statusStrip = null!;

        private string routineContent;
        private string clientName;

        public event EventHandler? RoutineEdited;
        public event EventHandler<string>? ExportRequested;

        public RoutinePreviewForm(string content, string name = "Cliente")
        {
            routineContent = content;
            clientName = name;
            InitializeComponent();
            LoadPreview();
        }

        private void InitializeComponent()
        {
            // Form properties
            this.Text = $" Vista previa - {clientName}";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(700, 500);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.BackColor = Color.White;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Toolbar
            toolStrip = new ToolStrip
            {
                BackColor = Color.FromArgb(248, 249, 250),
                GripStyle = ToolStripGripStyle.Hidden,
                Renderer = new ToolStripProfessionalRenderer(new CustomColorTable())
            };

            var zoomInButton = new ToolStripButton("+", null, ZoomIn_Click) { ToolTipText = "Acercar" };
            var zoomOutButton = new ToolStripButton("-", null, ZoomOut_Click) { ToolTipText = "Alejar" };
            var printPreviewButton = new ToolStripButton("", null, PrintPreview_Click) { ToolTipText = "Vista previa de impresin" };
            var separator1 = new ToolStripSeparator();
            var fontButton = new ToolStripButton("", null, ChangeFont_Click) { ToolTipText = "Cambiar fuente" };
            var separator2 = new ToolStripSeparator();
            var fullScreenButton = new ToolStripButton("", null, ToggleFullScreen_Click) { ToolTipText = "Pantalla completa" };

            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                zoomInButton, zoomOutButton, separator1,
                printPreviewButton, separator2, fontButton,
                separator2, fullScreenButton
            });

            // Preview text box
            previewTextBox = new RichTextBox
            {
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 11),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(20)
            };

            // Control panel
            controlPanel = new Panel
            {
                Height = 80,
                BackColor = Color.FromArgb(248, 249, 250),
                Dock = DockStyle.Bottom
            };

            // Buttons
            printButton = new Button
            {
                Text = " Imprimir",
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            printButton.FlatAppearance.BorderSize = 0;

            exportButton = new Button
            {
                Text = " Exportar",
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            exportButton.FlatAppearance.BorderSize = 0;

            editButton = new Button
            {
                Text = " Editar",
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            editButton.FlatAppearance.BorderSize = 0;

            closeButton = new Button
            {
                Text = " Cerrar",
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;

            // Status strip
            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(248, 249, 250)
            };

            statusLabel = new Label
            {
                Text = $" Rutina lista para {clientName} | Generada: {DateTime.Now:dd/MM/yyyy HH:mm}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true
            };

            var statusLabelItem = new ToolStripControlHost(statusLabel);
            statusStrip.Items.Add(statusLabelItem);
        }

        private void LayoutControls()
        {
            this.Controls.Add(toolStrip);
            this.Controls.Add(previewTextBox);
            this.Controls.Add(controlPanel);
            this.Controls.Add(statusStrip);

            // Layout toolbar
            toolStrip.Dock = DockStyle.Top;

            // Layout preview text box
            previewTextBox.Dock = DockStyle.Fill;

            // Layout buttons in control panel
            int buttonSpacing = 20;
            int startX = (controlPanel.Width - (4 * 120 + 3 * buttonSpacing)) / 2;
            int buttonY = 20;

            printButton.Location = new Point(startX, buttonY);
            exportButton.Location = new Point(startX + 120 + buttonSpacing, buttonY);
            editButton.Location = new Point(startX + 2 * (120 + buttonSpacing), buttonY);
            closeButton.Location = new Point(startX + 3 * (120 + buttonSpacing), buttonY);

            controlPanel.Controls.AddRange(new Control[] { printButton, exportButton, editButton, closeButton });

            // Re-center buttons when form is resized
            controlPanel.Resize += (s, e) =>
            {
                startX = Math.Max(20, (controlPanel.Width - (4 * 120 + 3 * buttonSpacing)) / 2);
                printButton.Location = new Point(startX, buttonY);
                exportButton.Location = new Point(startX + 120 + buttonSpacing, buttonY);
                editButton.Location = new Point(startX + 2 * (120 + buttonSpacing), buttonY);
                closeButton.Location = new Point(startX + 3 * (120 + buttonSpacing), buttonY);
            };
        }

        private void SetupEventHandlers()
        {
            printButton.Click += PrintButton_Click;
            exportButton.Click += ExportButton_Click;
            editButton.Click += EditButton_Click;
            closeButton.Click += CloseButton_Click;

            // Add hover effects
            AddHoverEffects(printButton, Color.FromArgb(0, 123, 255), Color.FromArgb(0, 86, 179));
            AddHoverEffects(exportButton, Color.FromArgb(40, 167, 69), Color.FromArgb(33, 136, 56));
            AddHoverEffects(editButton, Color.FromArgb(255, 193, 7), Color.FromArgb(218, 165, 32));
            AddHoverEffects(closeButton, Color.FromArgb(108, 117, 125), Color.FromArgb(90, 98, 104));
        }

        private void AddHoverEffects(Button button, Color normalColor, Color hoverColor)
        {
            button.MouseEnter += (s, e) => { button.BackColor = hoverColor; };
            button.MouseLeave += (s, e) => { button.BackColor = normalColor; };
        }

        private void LoadPreview()
        {
            previewTextBox.Text = routineContent;

            // Apply formatting
            ApplyFormattingToPreview();

            // Scroll to top
            previewTextBox.SelectionStart = 0;
            previewTextBox.ScrollToCaret();
        }

        private void ApplyFormattingToPreview()
        {
            previewTextBox.SelectAll();
            previewTextBox.SelectionFont = new Font("Segoe UI", 11);
            previewTextBox.SelectionColor = Color.Black;
            previewTextBox.Select(0, 0);

            // Format headers
            FormatText("", FontStyle.Bold, 14, Color.FromArgb(0, 123, 255));
            FormatText("", FontStyle.Bold, 14, Color.FromArgb(40, 167, 69));
            FormatText("", FontStyle.Bold, 14, Color.FromArgb(220, 53, 69));
            FormatText("", FontStyle.Bold, 14, Color.FromArgb(255, 193, 7));
            FormatText("", FontStyle.Bold, 14, Color.FromArgb(40, 167, 69));

            // Format day headers
            FormatText("DA", FontStyle.Bold, 12, Color.FromArgb(108, 117, 125));
        }

        private void FormatText(string searchText, FontStyle style, int size, Color color)
        {
            string text = previewTextBox.Text;
            int index = 0;

            while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                int lineStart = text.LastIndexOf('\n', index) + 1;
                int lineEnd = text.IndexOf('\n', index);
                if (lineEnd == -1) lineEnd = text.Length;

                previewTextBox.Select(lineStart, lineEnd - lineStart);
                previewTextBox.SelectionFont = new Font("Segoe UI", size, style);
                previewTextBox.SelectionColor = color;

                index = lineEnd;
            }
        }

        private void ZoomIn_Click(object? sender, EventArgs e)
        {
            if (previewTextBox.Font.Size < 20)
            {
                var newFont = new Font(previewTextBox.Font.FontFamily, previewTextBox.Font.Size + 1);
                previewTextBox.Font = newFont;
                ApplyFormattingToPreview();
            }
        }

        private void ZoomOut_Click(object? sender, EventArgs e)
        {
            if (previewTextBox.Font.Size > 8)
            {
                var newFont = new Font(previewTextBox.Font.FontFamily, previewTextBox.Font.Size - 1);
                previewTextBox.Font = newFont;
                ApplyFormattingToPreview();
            }
        }

        private void ChangeFont_Click(object? sender, EventArgs e)
        {
            using var fontDialog = new FontDialog
            {
                Font = previewTextBox.Font,
                ShowEffects = false
            };

            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                previewTextBox.Font = fontDialog.Font;
                ApplyFormattingToPreview();
            }
        }

        private void PrintPreview_Click(object? sender, EventArgs e)
        {
            using var printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            using var printPreviewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
                WindowState = FormWindowState.Maximized
            };

            printPreviewDialog.ShowDialog();
        }

        private void ToggleFullScreen_Click(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void PrintButton_Click(object? sender, EventArgs e)
        {
            using var printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            using var printDialog = new PrintDialog
            {
                Document = printDocument
            };

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }

        private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null) return;

            var font = new Font("Segoe UI", 10);
            var brush = new SolidBrush(Color.Black);
            var rect = e.MarginBounds;

            // Print the routine content
            e.Graphics.DrawString(previewTextBox.Text, font, brush, rect);
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            ExportRequested?.Invoke(this, routineContent);
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            RoutineEdited?.Invoke(this, EventArgs.Empty);
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        public void UpdateContent(string newContent)
        {
            routineContent = newContent;
            LoadPreview();
        }

        public string GetContent()
        {
            return routineContent;
        }
    }

    // Custom color table for toolbar
    public class CustomColorTable : ProfessionalColorTable
    {
        public override Color ToolStripBorder => Color.FromArgb(228, 230, 235);
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
        public override Color MenuBorder => Color.FromArgb(228, 230, 235);
        public override Color MenuItemBorder => Color.FromArgb(0, 123, 255);
        public override Color MenuItemSelected => Color.FromArgb(225, 243, 255);
        public override Color MenuStripGradientBegin => Color.FromArgb(248, 249, 250);
        public override Color MenuStripGradientEnd => Color.FromArgb(248, 249, 250);
        public override Color ToolStripGradientBegin => Color.FromArgb(248, 249, 250);
        public override Color ToolStripGradientEnd => Color.FromArgb(248, 249, 250);
        public override Color ToolStripGradientMiddle => Color.FromArgb(248, 249, 250);
    }
}
