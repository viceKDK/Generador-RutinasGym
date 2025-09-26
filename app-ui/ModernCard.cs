using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    public class ModernCard : Panel
    {
        private int _borderRadius = 16;
        private Color _cardColor = Color.White;
        private Color _shadowColor = Color.FromArgb(15, 0, 0, 0);
        private int _shadowOffset = 4;
        private string _title = "";
        private Font _titleFont = new Font("Segoe UI", 14F, FontStyle.Bold);
        private Color _titleColor = Color.FromArgb(33, 37, 41);

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public Color CardColor
        {
            get => _cardColor;
            set { _cardColor = value; BackColor = value; Invalidate(); }
        }

        public Color ShadowColor
        {
            get => _shadowColor;
            set { _shadowColor = value; Invalidate(); }
        }

        public int ShadowOffset
        {
            get => _shadowOffset;
            set { _shadowOffset = value; Invalidate(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public Font TitleFont
        {
            get => _titleFont;
            set { _titleFont = value; Invalidate(); }
        }

        public Color TitleColor
        {
            get => _titleColor;
            set { _titleColor = value; Invalidate(); }
        }

        public ModernCard()
        {
            BackColor = _cardColor;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Padding = new Padding(20);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Clear with transparent background
            graphics.Clear(Parent?.BackColor ?? SystemColors.Control);

            // Draw shadow
            var shadowRect = new Rectangle(_shadowOffset, _shadowOffset,
                Width - _shadowOffset, Height - _shadowOffset);
            var shadowPath = CreateRoundedRectangle(shadowRect, _borderRadius);
            using (var shadowBrush = new SolidBrush(_shadowColor))
            {
                graphics.FillPath(shadowBrush, shadowPath);
            }

            // Draw card
            var cardRect = new Rectangle(0, 0, Width - _shadowOffset, Height - _shadowOffset);
            var cardPath = CreateRoundedRectangle(cardRect, _borderRadius);

            // Create subtle gradient for the card
            using (var cardBrush = new LinearGradientBrush(
                cardRect,
                _cardColor,
                ControlPaint.Light(_cardColor, 0.02f),
                LinearGradientMode.Vertical))
            {
                graphics.FillPath(cardBrush, cardPath);
            }

            // Draw border
            using (var borderPen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
            {
                graphics.DrawPath(borderPen, cardPath);
            }

            // Draw title if provided
            if (!string.IsNullOrEmpty(_title))
            {
                using (var titleBrush = new SolidBrush(_titleColor))
                {
                    var titleRect = new Rectangle(Padding.Left, Padding.Top,
                        Width - Padding.Horizontal, 30);
                    var titleFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };
                    graphics.DrawString(_title, _titleFont, titleBrush, titleRect, titleFormat);
                }
            }

            shadowPath.Dispose();
            cardPath.Dispose();
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;

            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }
    }
}