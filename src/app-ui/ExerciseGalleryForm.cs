using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly ReadOnlyCollection<ExerciseSelectionEntry> _selectionView;
        private readonly ManualExerciseSelectionStore? _selectionStore;
        private readonly ToolTip _toolTip = new();
        private readonly Dictionary<string, ExerciseGalleryCard> _cardIndex = new(StringComparer.OrdinalIgnoreCase);

        private TextBox _searchTextBox = null!;
        private ComboBox _dataSourceComboBox = null!;
        private Label _resultsLabel = null!;
        private Label _titleLabel = null!;
        private Button _openImageLocationButton = null!;
        private Button _returnToGeneratorButton = null!;
        private FlowLayoutPanel _galleryPanel = null!;
        private ContextMenuStrip _resultsContextMenu = null!;
        private ExerciseGalleryCard? _activeCard;

        public event EventHandler<ManualExerciseSelectionChangedEventArgs>? SelectionChanged;

        public IReadOnlyList<ExerciseSelectionEntry> Selection => _selectionView;

        public ExerciseGalleryForm(
            ManualExerciseLibraryService libraryService,
            ManualExerciseSelectionStore? selectionStore = null)
        {
            _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
            _selectionStore = selectionStore;
            _selectionView = _selectionEntries.AsReadOnly();
            _searchDebounceTimer = new Timer { Interval = 300 };
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            InitializeComponent();
            RestoreSelectionFromStore();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Galeria de ejercicios";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 800);
            BackColor = Color.White;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 128,
                Padding = new Padding(24, 24, 24, 16),
                BackColor = Color.White
            };

            var topFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoSize = false,
                AutoScroll = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            _titleLabel = new Label
            {
                Text = "Generador de Rutinas · Galeria de ejercicios",
                AutoSize = true,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 38, 43),
                Margin = new Padding(0, 0, 0, 16)
            };
            topFlow.Controls.Add(_titleLabel);
            topFlow.SetFlowBreak(_titleLabel, true);

            _searchTextBox = new TextBox
            {
                PlaceholderText = "Buscar ejercicio...",
                Width = 340,
                Margin = new Padding(0, 0, 20, 0),
                Height = 40
            };
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            _dataSourceComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200,
                Margin = new Padding(0, 0, 20, 0),
                Height = 40
            };
            _dataSourceComboBox.Items.AddRange(new object[]
            {
                "BD principal",
                "BD secundaria",
                "Ambas"
            });
            _dataSourceComboBox.SelectedIndex = 0;
            _dataSourceComboBox.SelectedIndexChanged += DataSourceComboBox_SelectedIndexChanged;
            _toolTip.SetToolTip(_dataSourceComboBox, "Selecciona la fuente de datos para la busqueda.");

            _openImageLocationButton = new Button
            {
                Text = "Abrir ubicacion de la imagen",
                Enabled = false,
                Width = 260,
                Height = 48,
                Margin = new Padding(0, 0, 20, 0)
            };
            _openImageLocationButton.Click += (_, _) => OpenSelectedImageLocation();
            _toolTip.SetToolTip(_openImageLocationButton, "Abre la carpeta que contiene la imagen del ejercicio.");

            _returnToGeneratorButton = new Button
            {
                Text = "Volver al generador",
                Width = 220,
                Height = 48,
                Margin = new Padding(0),
                BackColor = Color.FromArgb(54, 79, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            _returnToGeneratorButton.FlatAppearance.BorderSize = 0;
            _returnToGeneratorButton.Click += (_, _) => Close();
            _toolTip.SetToolTip(_returnToGeneratorButton, "Cerrar la galeria y regresar al generador.");

            topFlow.Controls.Add(_searchTextBox);
            topFlow.Controls.Add(_dataSourceComboBox);
            topFlow.Controls.Add(_openImageLocationButton);
            topFlow.Controls.Add(_returnToGeneratorButton);
            topPanel.Controls.Add(topFlow);

            _resultsLabel = new Label
            {
                Text = "Escribe al menos 3 caracteres para buscar.",
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(20, 4, 20, 4),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(60, 60, 68),
                BackColor = Color.White
            };

            _galleryPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(28, 16, 28, 28),
                BackColor = Color.White
            };

            BuildContextMenu();

            Controls.Add(_galleryPanel);
            Controls.Add(_resultsLabel);
            Controls.Add(topPanel);

            FormClosed += ExerciseGalleryForm_FormClosed;

            ResumeLayout(false);
        }

        private void BuildContextMenu()
        {
            _resultsContextMenu = new ContextMenuStrip();
            _resultsContextMenu.Opening += ResultsContextMenu_Opening;

            var toggleSelectionItem = new ToolStripMenuItem("Agregar a seleccion manual", null, (_, _) => ToggleSelectedInSelection());
            var copyImageItem = new ToolStripMenuItem("Copiar imagen al portapapeles", null, (_, _) => CopySelectedImage());
            var openLocationItem = new ToolStripMenuItem("Abrir ubicacion en el explorador", null, (_, _) => OpenSelectedImageLocation());

            _resultsContextMenu.Items.Add(toggleSelectionItem);
            _resultsContextMenu.Items.Add(copyImageItem);
            _resultsContextMenu.Items.Add(openLocationItem);
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void DataSourceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
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
                ClearResults("Escribe al menos 3 caracteres para buscar.");
                return;
            }

            if (query.Length < 3)
            {
                ClearResults("Escribe al menos 3 caracteres para buscar.");
                return;
            }

            try
            {
                var source = GetSelectedDataSource();
                var results = _libraryService.Search(query, source);
                _currentResults.Clear();
                _currentResults.AddRange(results);

                _galleryPanel.SuspendLayout();
                DisposeResultCards();
                _galleryPanel.Controls.Clear();
                _cardIndex.Clear();
                SetActiveCard(null);

                foreach (var item in results)
                {
                    var card = new ExerciseGalleryCard();
                    var resolvedPath = _libraryService.GetImagePath(item) ?? item.ImagePath;
                    var thumbnail = _libraryService.LoadThumbnail(item, card.ImageDisplaySize);
                    var detailText = BuildDetailText(item);

                    card.SetData(item, resolvedPath, thumbnail, detailText);
                    card.ContextMenuStrip = _resultsContextMenu;
                    card.CardClicked += CardOnCardClicked;
                    card.CardDoubleClicked += CardOnCardDoubleClicked;
                    card.CardMouseDown += CardOnCardMouseDown;

                    if (IsItemInManualSelection(item))
                    {
                        card.SetManualSelection(true);
                    }

                    _cardIndex[item.Id] = card;
                    _galleryPanel.Controls.Add(card);
                }

                _galleryPanel.ResumeLayout();

                UpdateResultsLabel(results.Count);

                if (results.Count > 0 && _galleryPanel.Controls[0] is ExerciseGalleryCard firstCard)
                {
                    SetActiveCard(firstCard);
                }
                else
                {
                    SetActiveCard(null);
                }

                UpdateActionButtons();
            }
            catch (OperationCanceledException)
            {
                // Ignorado: parte del debounce.
            }
            catch (Exception ex)
            {
                ClearResults("Ocurrio un error al buscar. Revisa los logs.");
                Debug.WriteLine($"[ExerciseGalleryForm] Error realizando busqueda: {ex}");
            }
        }

        private string BuildDetailText(ExerciseGalleryItem item)
        {
            var segments = new List<string>();

            if (!string.IsNullOrWhiteSpace(item.Source))
            {
                segments.Add(item.Source);
            }

            if (item.MuscleGroups.Count > 0)
            {
                var muscles = item.MuscleGroups.Take(3).ToArray();
                var label = string.Join(", ", muscles);
                segments.Add(label);
            }

            return segments.Count == 0 ? "Sin metadatos" : string.Join(" • ", segments);
        }

        private void ClearResults(string message)
        {
            DisposeResultCards();
            _galleryPanel.Controls.Clear();
            SetActiveCard(null);
            _currentResults.Clear();
            _resultsLabel.Text = message;
            UpdateActionButtons();
        }

        private void DisposeResultCards()
        {
            foreach (var card in _cardIndex.Values)
            {
                card.CardClicked -= CardOnCardClicked;
                card.CardDoubleClicked -= CardOnCardDoubleClicked;
                card.CardMouseDown -= CardOnCardMouseDown;
                card.Dispose();
            }

            _cardIndex.Clear();
            _activeCard = null;
        }

        private void CardOnCardMouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is ExerciseGalleryCard card && e.Button == MouseButtons.Right)
            {
                SetActiveCard(card);
            }
        }

        private void CardOnCardDoubleClicked(object? sender, EventArgs e)
        {
            if (sender is ExerciseGalleryCard card)
            {
                SetActiveCard(card);
                ToggleSelectedInSelection();
            }
        }

        private void CardOnCardClicked(object? sender, EventArgs e)
        {
            if (sender is ExerciseGalleryCard card)
            {
                SetActiveCard(card);
            }
        }

        private void SetActiveCard(ExerciseGalleryCard? card)
        {
            if (_activeCard == card)
            {
                UpdateActionButtons();
                return;
            }

            if (_activeCard is { IsDisposed: false })
            {
                _activeCard.SetActive(false);
            }

            _activeCard = card;

            if (_activeCard is { IsDisposed: false })
            {
                _activeCard.SetActive(true);
            }

            UpdateActionButtons();
        }

        private void UpdateResultsLabel(int count)
        {
            _resultsLabel.Text = count switch
            {
                0 => "Sin resultados. Proba con otro termino.",
                1 => "1 resultado encontrado.",
                _ => $"{count} resultados encontrados."
            };
        }

        private void UpdateActionButtons()
        {
            var item = GetSelectedGalleryItem();
            var hasSelection = item != null;

            _openImageLocationButton.Enabled = hasSelection;

            if (!hasSelection)
            {
                return;
            }
        }

        private bool IsItemInManualSelection(ExerciseGalleryItem item)
        {
            return _selectionEntries.Any(entry =>
                string.Equals(entry.ExerciseId, item.Id, StringComparison.OrdinalIgnoreCase));
        }

        private ManualExerciseDataSource GetSelectedDataSource()
        {
            return _dataSourceComboBox.SelectedIndex switch
            {
                0 => ManualExerciseDataSource.Primary,
                1 => ManualExerciseDataSource.Secondary,
                2 => ManualExerciseDataSource.Combined,
                _ => ManualExerciseDataSource.Primary
            };
        }

        private void ToggleSelectedInSelection()
        {
            var item = GetSelectedGalleryItem();
            if (item == null)
            {
                return;
            }

            if (IsItemInManualSelection(item))
            {
                RemoveItemFromSelection(item);
                _resultsLabel.Text = $"Quitado de la seleccion manual: {item.DisplayName}";
            }
            else
            {
                AddItemToSelection(item);
                _resultsLabel.Text = $"Agregado a la seleccion manual: {item.DisplayName}";
            }

            UpdateManualSelectionIndicators();
            UpdateActionButtons();
        }

        private void UpdateManualSelectionIndicators()
        {
            foreach (var card in _cardIndex.Values)
            {
                var manual = card.Item != null && IsItemInManualSelection(card.Item);
                card.SetManualSelection(manual);
            }
        }

        private void AddItemToSelection(ExerciseGalleryItem item)
        {
            if (IsItemInManualSelection(item))
            {
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
            NotifySelectionChanged();
        }

        private void RemoveItemFromSelection(ExerciseGalleryItem item)
        {
            var existing = _selectionEntries.FirstOrDefault(entry =>
                string.Equals(entry.ExerciseId, item.Id, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                return;
            }

            _selectionEntries.Remove(existing);
            NotifySelectionChanged();
        }

        private ExerciseGalleryItem? GetSelectedGalleryItem()
        {
            return _activeCard?.Item;
        }

        private void CopySelectedImage()
        {
            var item = GetSelectedGalleryItem();
            if (item == null)
            {
                _resultsLabel.Text = "Selecciona un ejercicio para copiar su imagen.";
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
                _resultsLabel.Text = "Selecciona un ejercicio para abrir la imagen.";
                return;
            }

            if (_libraryService.TryOpenImageLocation(item))
            {
                _resultsLabel.Text = $"Abriendo ubicacion de: {item.DisplayName}";
            }
            else
            {
                _resultsLabel.Text = "No se pudo abrir la ubicacion del archivo.";
            }
        }

        private void ResultsContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            var card = GetOwningCard(_resultsContextMenu.SourceControl);
            if (card != null)
            {
                SetActiveCard(card);
            }

            var item = GetSelectedGalleryItem();
            if (item == null)
            {
                e.Cancel = true;
                return;
            }

            if (_resultsContextMenu.Items.Count >= 1 && _resultsContextMenu.Items[0] is ToolStripMenuItem toggleItem)
            {
                toggleItem.Text = IsItemInManualSelection(item)
                    ? "Quitar de seleccion manual"
                    : "Agregar a seleccion manual";
            }
        }

        private static ExerciseGalleryCard? GetOwningCard(Control? control)
        {
            while (control != null && control is not ExerciseGalleryCard)
            {
                control = control.Parent;
            }

            return control as ExerciseGalleryCard;
        }

        private void RestoreSelectionFromStore()
        {
            _selectionEntries.Clear();

            if (_selectionStore == null)
            {
                return;
            }

            var existing = _selectionStore.CurrentSelection;
            if (existing.Count == 0)
            {
                return;
            }

            _selectionEntries.AddRange(existing);
        }

        private void NotifySelectionChanged()
        {
            var snapshot = _selectionEntries.Select(entry => entry).ToArray();
            var readOnly = new ReadOnlyCollection<ExerciseSelectionEntry>(snapshot);

            _selectionStore?.UpdateSelectionSnapshot(snapshot);
            SelectionChanged?.Invoke(this, new ManualExerciseSelectionChangedEventArgs(readOnly));
        }

        private void ExerciseGalleryForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Dispose();
            DisposeResultCards();
        }
    }

    internal sealed class ExerciseGalleryCard : Panel
    {
        private static readonly Color BaseColor = Color.White;
        private static readonly Color HoverColor = Color.FromArgb(247, 247, 249);
        private static readonly Color ActiveColor = Color.FromArgb(235, 240, 255);
        private static readonly Color BorderColor = Color.FromArgb(218, 220, 224);
        private static readonly Color SelectedBorderColor = Color.FromArgb(82, 133, 246);

        private readonly Panel _imagePanel;
        private readonly Label _placeholderLabel;
        private readonly Label _titleLabel;
        private readonly Label _detailLabel;
        private readonly Panel _textContainer;

        private Image? _thumbnail;
        private bool _isActive;
        private bool _isManualSelected;
        private int _hoverDepth;

        public ExerciseGalleryCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = BaseColor;
            Margin = new Padding(20);
            Size = new Size(280, 300);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;

            _imagePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 170,
                BackColor = Color.White
            };
            _imagePanel.Paint += ImagePanel_Paint;

            _textContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 14, 16, 16),
                BackColor = BaseColor
            };

            _titleLabel = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 68,
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 48, 52),
                TextAlign = ContentAlignment.TopLeft,
                AutoEllipsis = false,
                UseMnemonic = false,
                Padding = new Padding(0, 0, 0, 6)
            };

            _detailLabel = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 44,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(120, 124, 130),
                TextAlign = ContentAlignment.TopLeft
            };

            _placeholderLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Sin imagen",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 156),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(248, 248, 250)
            };
            _imagePanel.Controls.Add(_placeholderLabel);

            _textContainer.Controls.Add(_detailLabel);
            _textContainer.Controls.Add(_titleLabel);

            Controls.Add(_textContainer);
            Controls.Add(_imagePanel);

            AttachInteractionHandlers(this);
        }

        public ExerciseGalleryItem? Item { get; private set; }

        public string? ResolvedImagePath { get; private set; }

        public Size ImageDisplaySize => new Size(220, 160);

        public event EventHandler? CardClicked;
        public event EventHandler? CardDoubleClicked;
        public event MouseEventHandler? CardMouseDown;

        public void SetData(ExerciseGalleryItem item, string? resolvedImagePath, Image? thumbnail, string? detailText)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            ResolvedImagePath = resolvedImagePath;

            _titleLabel.Text = item.DisplayName;
            _detailLabel.Text = detailText ?? string.Empty;

            SetThumbnail(thumbnail);
            if (_thumbnail == null && !string.IsNullOrWhiteSpace(ResolvedImagePath) && File.Exists(ResolvedImagePath))
            {
                try
                {
                    SetThumbnail(Image.FromFile(ResolvedImagePath));
                }
                catch
                {
                    // ignore: placeholder remains visible
                }
            }
            UpdateVisualState();
        }

        public void SetActive(bool isActive)
        {
            if (_isActive == isActive)
            {
                return;
            }

            _isActive = isActive;
            UpdateVisualState();
        }

        public void SetManualSelection(bool selected)
        {
            if (_isManualSelected == selected)
            {
                return;
            }

            _isManualSelected = selected;
            UpdateVisualState();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateRoundedRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var rect = ClientRectangle;
            rect.Inflate(-1, -1);

            using var borderPen = new Pen(_isManualSelected ? SelectedBorderColor : BorderColor, _isActive ? 2.5f : 1.5f);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawRectangle(borderPen, rect);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SetThumbnail(null);
            }

            base.Dispose(disposing);
        }

        private void UpdateVisualState()
        {
            var background = _isActive
                ? ActiveColor
                : _hoverDepth > 0 ? HoverColor : BaseColor;

            BackColor = background;
            _textContainer.BackColor = background;
            Invalidate();
        }

        private void SetThumbnail(Image? thumbnail)
        {
            if (ReferenceEquals(_thumbnail, thumbnail))
            {
                return;
            }

            _thumbnail?.Dispose();
            _thumbnail = thumbnail != null ? new Bitmap(thumbnail) : null;

            var hasImage = _thumbnail != null;
            _placeholderLabel.Visible = !hasImage;
            _imagePanel.Invalidate();
        }

        private void ImagePanel_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (_thumbnail == null)
            {
                return;
            }

            var bounds = new Rectangle(0, 0, _imagePanel.Width, _imagePanel.Height);
            var target = CalculateImageRect(_thumbnail.Size, bounds);

            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            e.Graphics.DrawImage(_thumbnail, target);
        }

        private static Rectangle CalculateImageRect(Size imageSize, Rectangle bounds)
        {
            if (imageSize.Width <= 0 || imageSize.Height <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return bounds;
            }

            var ratio = Math.Min((double)bounds.Width / imageSize.Width, (double)bounds.Height / imageSize.Height);
            var width = Math.Max(1, (int)Math.Round(imageSize.Width * ratio));
            var height = Math.Max(1, (int)Math.Round(imageSize.Height * ratio));
            var x = bounds.X + (bounds.Width - width) / 2;
            var y = bounds.Y + (bounds.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }


        private void AttachInteractionHandlers(Control control)
        {
            control.MouseEnter += (_, _) => AdjustHover(1);
            control.MouseLeave += (_, _) => AdjustHover(-1);
            control.Click += (_, _) => CardClicked?.Invoke(this, EventArgs.Empty);
            control.DoubleClick += (_, _) => CardDoubleClicked?.Invoke(this, EventArgs.Empty);
            control.MouseDown += (s, e) => CardMouseDown?.Invoke(this, e);

            foreach (Control child in control.Controls)
            {
                AttachInteractionHandlers(child);
            }
        }

        private void AdjustHover(int delta)
        {
            _hoverDepth = Math.Max(0, _hoverDepth + delta);
            UpdateVisualState();
        }

        private void UpdateRoundedRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            var handle = NativeMethods.CreateRoundRectRgn(0, 0, Width, Height, 18, 18);
            var region = Region.FromHrgn(handle);
            NativeMethods.DeleteObject(handle);
            Region = region;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}

