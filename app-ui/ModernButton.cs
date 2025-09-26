using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    public class ModernButton : Button
    {
        private Color _normalColor = Color.FromArgb(25, 135, 84);
        private Color _hoverColor = Color.FromArgb(20, 108, 67);
        private Color _pressedColor = Color.FromArgb(15, 81, 50);
        private int _borderRadius = 12;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private System.Windows.Forms.Timer _animationTimer;
        private float _animationProgress = 0f;

        public Color NormalColor
        {
            get => _normalColor;
            set { _normalColor = value; Invalidate(); }
        }

        public Color HoverColor
        {
            get => _hoverColor;
            set { _hoverColor = value; Invalidate(); }
        }

        public Color PressedColor
        {
            get => _pressedColor;
            set { _pressedColor = value; Invalidate(); }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;

            _animationTimer = new System.Windows.Forms.Timer { Interval = 16 }; // 60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            StartAnimation();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            StartAnimation();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _isPressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        private void StartAnimation()
        {
            _animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_isHovered && _animationProgress < 1f)
            {
                _animationProgress += 0.1f;
            }
            else if (!_isHovered && _animationProgress > 0f)
            {
                _animationProgress -= 0.1f;
            }
            else
            {
                _animationTimer.Stop();
                return;
            }

            _animationProgress = Math.Max(0f, Math.Min(1f, _animationProgress));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Create rounded rectangle path
            var rect = new Rectangle(0, 0, Width, Height);
            var path = CreateRoundedRectangle(rect, _borderRadius);

            // Determine button color based on state
            Color currentColor = _normalColor;
            if (!Enabled)
            {
                currentColor = Color.FromArgb(173, 181, 189);
            }
            else if (_isPressed)
            {
                currentColor = _pressedColor;
            }
            else if (_isHovered || _animationProgress > 0)
            {
                // Interpolate between normal and hover colors
                var r = (int)(_normalColor.R + (_hoverColor.R - _normalColor.R) * _animationProgress);
                var g = (int)(_normalColor.G + (_hoverColor.G - _normalColor.G) * _animationProgress);
                var b = (int)(_normalColor.B + (_hoverColor.B - _normalColor.B) * _animationProgress);
                currentColor = Color.FromArgb(r, g, b);
            }

            // Create gradient brush
            var gradientRect = new Rectangle(0, 0, Width, Height);
            var lightColor = ControlPaint.Light(currentColor, 0.2f);
            var darkColor = ControlPaint.Dark(currentColor, 0.1f);

            using (var brush = new LinearGradientBrush(gradientRect, lightColor, darkColor, LinearGradientMode.Vertical))
            {
                // Add subtle shadow effect first
                if (Enabled && (_isHovered || _animationProgress > 0))
                {
                    var shadowRect = new Rectangle(2, 2, Width, Height);
                    var shadowPath = CreateRoundedRectangle(shadowRect, _borderRadius);
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                    {
                        graphics.FillPath(shadowBrush, shadowPath);
                    }
                    shadowPath.Dispose();
                }

                graphics.FillPath(brush, path);
            }

            // Draw border
            using (var borderPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1))
            {
                graphics.DrawPath(borderPen, path);
            }

            // Draw text
            var textColor = Enabled ? ForeColor : Color.FromArgb(108, 117, 125);
            using (var textBrush = new SolidBrush(textColor))
            {
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                graphics.DrawString(Text, Font, textBrush, rect, textFormat);
            }

            path.Dispose();
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}