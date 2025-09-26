using System;
using System.Drawing;
using System.Windows.Forms;

namespace GymRoutineUI
{
    public partial class HelpForm : Form
    {
        private TreeView helpTreeView;
        private Button closeButton;
        private Button homeButton;
        private TextBox searchTextBox;
        private Button searchButton;

        public HelpForm()
        {
            InitializeComponent();
            InitializeControls();
            LayoutControls();
            LoadHelpContent();
        }

        private void InitializeComponent()
        {
            this.Text = " Sistema de Ayuda";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 500);
            this.ShowIcon = true;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.BackColor = Color.FromArgb(248, 249, 250);
        }

        private void InitializeControls()
        {
            // Search controls
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            searchTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 15),
                Size = new Size(300, 25),
                PlaceholderText = "Buscar en la ayuda..."
            };

            searchButton = new Button
            {
                Text = " Buscar",
                Font = new Font("Segoe UI", 9),
                Location = new Point(320, 13),
                Size = new Size(80, 29),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White
            };
            searchButton.FlatAppearance.BorderSize = 0;
            searchButton.Click += SearchButton_Click;

            homeButton = new Button
            {
                Text = " Inicio",
                Font = new Font("Segoe UI", 9),
                Location = new Point(410, 13),
                Size = new Size(80, 29),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White
            };
            homeButton.FlatAppearance.BorderSize = 0;
            homeButton.Click += HomeButton_Click;

            searchPanel.Controls.AddRange(new Control[] { searchTextBox, searchButton, homeButton });

            // Help tree view
            helpTreeView = new TreeView
            {
                Dock = DockStyle.Left,
                Width = 250,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = false
            };
            helpTreeView.AfterSelect += HelpTreeView_AfterSelect;

            // Help content browser (using RichTextBox as alternative to WebBrowser)
            var helpPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var helpContentTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };
            helpContentTextBox.Name = "helpContentTextBox";

            helpPanel.Controls.Add(helpContentTextBox);

            // Splitter
            var splitter = new Splitter
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = Color.FromArgb(108, 117, 125)
            };

            // Close button
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            closeButton = new Button
            {
                Text = " Cerrar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(780, 15),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            closeButton.FlatAppearance.BorderSize = 0;

            buttonPanel.Controls.Add(closeButton);

            // Add all controls to form
            this.Controls.AddRange(new Control[] { helpPanel, splitter, helpTreeView, buttonPanel, searchPanel });
        }

        private void LayoutControls()
        {
            // Layout is handled by docking
        }

        private void LoadHelpContent()
        {
            helpTreeView.Nodes.Clear();

            // Getting Started
            var gettingStarted = new TreeNode(" Primeros Pasos");
            gettingStarted.Nodes.Add("Instalacin y configuracin");
            gettingStarted.Nodes.Add("Primer uso de la aplicacin");
            gettingStarted.Nodes.Add("Configuracin inicial");

            // Creating Routines
            var creatingRoutines = new TreeNode(" Creando Rutinas");
            creatingRoutines.Nodes.Add("Informacin personal");
            creatingRoutines.Nodes.Add("Seleccin de objetivos");
            creatingRoutines.Nodes.Add("Configuracin de preferencias");
            creatingRoutines.Nodes.Add("Generacin de rutina");

            // Export and Sharing
            var exportSharing = new TreeNode(" Exportacin y Compartir");
            exportSharing.Nodes.Add("Exportar a Word");
            exportSharing.Nodes.Add("Configurar plantillas");
            exportSharing.Nodes.Add("Compartir rutinas");

            // Settings and Configuration
            var settings = new TreeNode(" Configuracin");
            settings.Nodes.Add("Configuracin general");
            settings.Nodes.Add("Preferencias de exportacin");
            settings.Nodes.Add("Personalizacin de interfaz");

            // Troubleshooting
            var troubleshooting = new TreeNode(" Solucin de Problemas");
            troubleshooting.Nodes.Add("Problemas comunes");
            troubleshooting.Nodes.Add("Error al exportar");
            troubleshooting.Nodes.Add("Problemas de rendimiento");

            // FAQ
            var faq = new TreeNode(" Preguntas Frecuentes");
            faq.Nodes.Add("Cmo personalizar rutinas?");
            faq.Nodes.Add("Puedo crear plantillas propias?");
            faq.Nodes.Add("Qu formatos de exportacin soporta?");

            helpTreeView.Nodes.AddRange(new TreeNode[] {
                gettingStarted, creatingRoutines, exportSharing, settings, troubleshooting, faq
            });

            // Expand first node
            helpTreeView.Nodes[0].Expand();

            // Load home content
            LoadHomeContent();
        }

        private void LoadHomeContent()
        {
            var helpContent = this.Controls.Find("helpContentTextBox", true)[0] as RichTextBox;
            if (helpContent != null)
            {
                helpContent.Clear();
                helpContent.SelectionFont = new Font("Segoe UI", 16, FontStyle.Bold);
                helpContent.SelectionColor = Color.FromArgb(0, 123, 255);
                helpContent.AppendText("Bienvenido al Sistema de Ayuda\n\n");

                helpContent.SelectionFont = new Font("Segoe UI", 11);
                helpContent.SelectionColor = Color.Black;
                helpContent.AppendText("Esta aplicacin te ayuda a crear rutinas de gimnasio personalizadas y exportarlas a documentos de Word profesionales.\n\n");

                helpContent.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
                helpContent.SelectionColor = Color.FromArgb(40, 167, 69);
                helpContent.AppendText(" Para empezar:\n\n");

                helpContent.SelectionFont = new Font("Segoe UI", 10);
                helpContent.SelectionColor = Color.Black;
                helpContent.AppendText("1. Completa tu informacin personal\n");
                helpContent.AppendText("2. Selecciona tus objetivos de entrenamiento\n");
                helpContent.AppendText("3. Configura tus preferencias\n");
                helpContent.AppendText("4. Genera tu rutina personalizada\n");
                helpContent.AppendText("5. Exporta a Word y comienza a entrenar!\n\n");

                helpContent.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
                helpContent.SelectionColor = Color.FromArgb(255, 193, 7);
                helpContent.AppendText(" Consejos rpidos:\n\n");

                helpContent.SelectionFont = new Font("Segoe UI", 10);
                helpContent.SelectionColor = Color.Black;
                helpContent.AppendText(" Usa la barra de bsqueda para encontrar temas especficos\n");
                helpContent.AppendText(" Haz clic en los elementos del men de la izquierda para navegar\n");
                helpContent.AppendText(" Todas las configuraciones se guardan automticamente\n");
                helpContent.AppendText(" Puedes personalizar la apariencia en Configuracin > Interfaz\n\n");

                helpContent.SelectionFont = new Font("Segoe UI", 10, FontStyle.Italic);
                helpContent.SelectionColor = Color.FromArgb(108, 117, 125);
                helpContent.AppendText("Necesitas ayuda adicional? Contacta nuestro soporte tcnico desde el men Acerca de.");
            }
        }

        private void HelpTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;

            var helpContent = this.Controls.Find("helpContentTextBox", true)[0] as RichTextBox;
            if (helpContent == null) return;

            helpContent.Clear();

            // Load content based on selected node
            switch (e.Node.Text)
            {
                case "Instalacin y configuracin":
                    LoadInstallationContent(helpContent);
                    break;
                case "Primer uso de la aplicacin":
                    LoadFirstUseContent(helpContent);
                    break;
                case "Informacin personal":
                    LoadPersonalInfoContent(helpContent);
                    break;
                case "Exportar a Word":
                    LoadExportContent(helpContent);
                    break;
                case "Problemas comunes":
                    LoadTroubleshootingContent(helpContent);
                    break;
                default:
                    LoadDefaultContent(helpContent, e.Node.Text);
                    break;
            }
        }

        private void LoadInstallationContent(RichTextBox content)
        {
            content.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(0, 123, 255);
            content.AppendText("Instalacin y Configuracin\n\n");

            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("La aplicacin Generador de Rutinas de Gimnasio requiere los siguientes componentes:\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.AppendText("Requisitos del sistema:\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.AppendText(" Windows 10 o superior\n");
            content.AppendText(" .NET 8.0 Runtime\n");
            content.AppendText(" Microsoft Word (para exportacin completa)\n");
            content.AppendText(" 512 MB de RAM disponible\n");
            content.AppendText(" 100 MB de espacio en disco\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.AppendText("Primera configuracin:\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.AppendText("1. Ejecuta la aplicacin como administrador la primera vez\n");
            content.AppendText("2. Configura la ruta de exportacin predeterminada\n");
            content.AppendText("3. Selecciona tus preferencias de idioma\n");
            content.AppendText("4. Configura las plantillas de Word\n");
        }

        private void LoadFirstUseContent(RichTextBox content)
        {
            content.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(0, 123, 255);
            content.AppendText("Primer Uso de la Aplicacin\n\n");

            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("Sigue estos pasos para crear tu primera rutina:\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(40, 167, 69);
            content.AppendText("Paso 1: Informacin Personal\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("Completa los campos bsicos como nombre, edad y gnero.\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(40, 167, 69);
            content.AppendText("Paso 2: Configurar Objetivos\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("Selecciona tus metas: perder peso, ganar msculo, mejorar resistencia, etc.\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(40, 167, 69);
            content.AppendText("Paso 3: Generar Rutina\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("Haz clic en 'Generar Rutina' y espera mientras se crea tu plan personalizado.\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(40, 167, 69);
            content.AppendText("Paso 4: Exportar\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("Exporta tu rutina a Word para imprimirla o compartirla.");
        }

        private void LoadPersonalInfoContent(RichTextBox content)
        {
            content.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(0, 123, 255);
            content.AppendText("Informacin Personal\n\n");

            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("La informacin personal ayuda a personalizar tu rutina:\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.AppendText(" Nombre: ");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.AppendText("Aparecer en el documento exportado\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.AppendText(" Edad: ");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.AppendText("Influye en la intensidad y tipo de ejercicios recomendados\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.AppendText(" Gnero: ");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.AppendText("Afecta las recomendaciones de entrenamiento y objetivos\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.AppendText(" Nivel de condicin fsica: ");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.AppendText("Determina la complejidad de los ejercicios\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.AppendText(" Das de entrenamiento: ");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.AppendText("Define la frecuencia y distribucin de ejercicios");
        }

        private void LoadExportContent(RichTextBox content)
        {
            content.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(0, 123, 255);
            content.AppendText("Exportar a Word\n\n");

            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText("La funcin de exportacin crea documentos profesionales de Word:\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(40, 167, 69);
            content.AppendText("Caractersticas del documento exportado:\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText(" Formato profesional y fcil de leer\n");
            content.AppendText(" Informacin personal y objetivos\n");
            content.AppendText(" Rutina detallada con ejercicios especficos\n");
            content.AppendText(" Tablas organizadas por das y grupos musculares\n");
            content.AppendText(" Recomendaciones de seguridad\n\n");

            content.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(255, 193, 7);
            content.AppendText("Consejos para exportar:\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText(" Asegrate de tener Microsoft Word instalado\n");
            content.AppendText(" Elige una ubicacin fcil de recordar\n");
            content.AppendText(" El archivo se abrir automticamente despus de la exportacin\n");
            content.AppendText(" Puedes modificar el documento en Word si lo necesitas");
        }

        private void LoadTroubleshootingContent(RichTextBox content)
        {
            content.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(220, 53, 69);
            content.AppendText("Problemas Comunes y Soluciones\n\n");

            content.SelectionFont = new Font("Segoe UI", 11, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(220, 53, 69);
            content.AppendText(" Error al exportar a Word:\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText(" Verifica que Microsoft Word est instalado\n");
            content.AppendText(" Ejecuta la aplicacin como administrador\n");
            content.AppendText(" Asegrate de tener permisos de escritura en la carpeta destino\n\n");

            content.SelectionFont = new Font("Segoe UI", 11, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(220, 53, 69);
            content.AppendText(" La aplicacin se cierra inesperadamente:\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText(" Verifica que tengas .NET 8.0 Runtime instalado\n");
            content.AppendText(" Cierra otras aplicaciones que consuman mucha memoria\n");
            content.AppendText(" Reinicia la aplicacin\n\n");

            content.SelectionFont = new Font("Segoe UI", 11, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(220, 53, 69);
            content.AppendText(" Las rutinas generadas no son adecuadas:\n");
            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText(" Revisa la informacin personal ingresada\n");
            content.AppendText(" Ajusta los objetivos de entrenamiento\n");
            content.AppendText(" Considera cambiar el nivel de condicin fsica\n");
            content.AppendText(" Consulta con un profesional del fitness si es necesario");
        }

        private void LoadDefaultContent(RichTextBox content, string nodeName)
        {
            content.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            content.SelectionColor = Color.FromArgb(0, 123, 255);
            content.AppendText($"{nodeName}\n\n");

            content.SelectionFont = new Font("Segoe UI", 10);
            content.SelectionColor = Color.Black;
            content.AppendText($"Contenido de ayuda para '{nodeName}' estar disponible prximamente.\n\n");
            content.AppendText("Mientras tanto, puedes:\n");
            content.AppendText(" Explorar otras secciones de ayuda\n");
            content.AppendText(" Usar la funcin de bsqueda\n");
            content.AppendText(" Contactar soporte tcnico desde 'Acerca de'");
        }

        private void SearchButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                MessageBox.Show("Por favor, ingresa un trmino de bsqueda.", "Bsqueda",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Simple search functionality
            var searchTerm = searchTextBox.Text.ToLowerInvariant();
            var helpContent = this.Controls.Find("helpContentTextBox", true)[0] as RichTextBox;

            if (helpContent != null)
            {
                helpContent.Clear();
                helpContent.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
                helpContent.SelectionColor = Color.FromArgb(0, 123, 255);
                helpContent.AppendText($"Resultados de bsqueda para: '{searchTextBox.Text}'\n\n");

                helpContent.SelectionFont = new Font("Segoe UI", 10);
                helpContent.SelectionColor = Color.Black;

                // Simple search logic
                if (searchTerm.Contains("export") || searchTerm.Contains("word"))
                {
                    helpContent.AppendText(" Exportar a Word - Crear documentos profesionales\n");
                    helpContent.AppendText(" Configurar plantillas - Personalizar el formato de exportacin\n");
                }
                else if (searchTerm.Contains("rutin") || searchTerm.Contains("ejerc"))
                {
                    helpContent.AppendText(" Creando Rutinas - Gua completa para generar rutinas\n");
                    helpContent.AppendText(" Informacin personal - Personalizar segn tus datos\n");
                }
                else if (searchTerm.Contains("error") || searchTerm.Contains("problem"))
                {
                    helpContent.AppendText(" Problemas comunes - Soluciones a errores frecuentes\n");
                    helpContent.AppendText(" Error al exportar - Solucionar problemas de exportacin\n");
                }
                else
                {
                    helpContent.AppendText("No se encontraron resultados especficos.\n\n");
                    helpContent.AppendText("Intenta buscar trminos como:\n");
                    helpContent.AppendText(" exportar, word, rutina, ejercicio, error, configuracin");
                }
            }
        }

        private void HomeButton_Click(object? sender, EventArgs e)
        {
            helpTreeView.SelectedNode = null;
            LoadHomeContent();
        }
    }
}
