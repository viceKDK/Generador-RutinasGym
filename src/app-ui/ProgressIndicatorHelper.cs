using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    public class ProgressIndicatorHelper
    {
        private readonly ProgressBar _progressBar;
        private readonly Label _statusLabel;
        private readonly Control _parentControl;

        public ProgressIndicatorHelper(ProgressBar progressBar, Label statusLabel, Control parentControl)
        {
            _progressBar = progressBar;
            _statusLabel = statusLabel;
            _parentControl = parentControl;
        }

        public async Task ShowProgressWithSteps(string[] steps, Func<int, Task> stepAction)
        {
            await ShowProgress();

            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 0;
            _progressBar.Maximum = steps.Length;

            for (int i = 0; i < steps.Length; i++)
            {
                UpdateStatus($" {steps[i]}", PremiumColors.GoldLight);

                // Execute the step action
                if (stepAction != null)
                {
                    await stepAction(i);
                }

                _progressBar.Value = i + 1;

                // Small delay between steps for better UX
                await Task.Delay(300);
            }
        }

        public async Task ShowProgress()
        {
            if (_progressBar.Visible) return;

            _progressBar.Visible = true;
            _progressBar.Height = 0;

            // Animate height expansion with smooth curve
            var targetHeight = 25;
            var steps = 10;
            var increment = targetHeight / steps;

            for (int i = 0; i <= steps; i++)
            {
                var progress = (double)i / steps;
                // Ease-out animation curve
                var easedProgress = 1 - Math.Pow(1 - progress, 3);
                _progressBar.Height = (int)(targetHeight * easedProgress);
                await Task.Delay(30);
                _parentControl.Refresh();
            }
        }

        public async Task HideProgress()
        {
            if (!_progressBar.Visible) return;

            var currentHeight = _progressBar.Height;
            var steps = 8;
            var decrement = currentHeight / steps;

            // Animate height reduction with smooth curve
            for (int i = steps; i >= 0; i--)
            {
                var progress = (double)i / steps;
                // Ease-in animation curve
                var easedProgress = Math.Pow(progress, 2);
                _progressBar.Height = (int)(currentHeight * easedProgress);
                await Task.Delay(25);
                _parentControl.Refresh();
            }

            _progressBar.Visible = false;
            _progressBar.Height = 25; // Reset to original height
            _progressBar.Value = 0;
        }

        public async Task UpdateStatus(string text, Color color)
        {
            // Smooth color transition
            var currentColor = _statusLabel.ForeColor;
            var steps = 8;

            // Calculate color step differences
            int rDiff = (color.R - currentColor.R) / steps;
            int gDiff = (color.G - currentColor.G) / steps;
            int bDiff = (color.B - currentColor.B) / steps;

            // Animate color transition
            for (int i = 0; i < steps; i++)
            {
                var newR = Math.Max(0, Math.Min(255, currentColor.R + (rDiff * i)));
                var newG = Math.Max(0, Math.Min(255, currentColor.G + (gDiff * i)));
                var newB = Math.Max(0, Math.Min(255, currentColor.B + (bDiff * i)));

                _statusLabel.ForeColor = Color.FromArgb(newR, newG, newB);
                await Task.Delay(20);
                _parentControl.Refresh();
            }

            _statusLabel.Text = text;
            _statusLabel.ForeColor = color;

            // Add a subtle glow effect by briefly making the text slightly larger
            var originalFont = _statusLabel.Font;
            var glowFont = new Font(originalFont.FontFamily, originalFont.Size + 1, originalFont.Style);

            _statusLabel.Font = glowFont;
            await Task.Delay(100);
            _statusLabel.Font = originalFont;
        }

        public void SetMarqueeMode()
        {
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressBar.MarqueeAnimationSpeed = 50;
        }

        public void SetContinuousMode()
        {
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.MarqueeAnimationSpeed = 0;
        }

        public async Task SimulateProgress(int durationMs, string statusText, Color statusColor)
        {
            UpdateStatus(statusText, statusColor).ConfigureAwait(false);

            SetContinuousMode();
            _progressBar.Value = 0;

            var steps = 100;
            var stepDelay = durationMs / steps;

            for (int i = 0; i <= steps; i++)
            {
                _progressBar.Value = i;
                await Task.Delay(stepDelay);
            }
        }

        public async Task PulseProgress(int pulseCount = 3)
        {
            var originalHeight = _progressBar.Height;
            var pulseHeight = originalHeight + 5;

            for (int pulse = 0; pulse < pulseCount; pulse++)
            {
                // Expand
                for (int h = originalHeight; h <= pulseHeight; h++)
                {
                    _progressBar.Height = h;
                    await Task.Delay(15);
                    _parentControl.Refresh();
                }

                // Contract
                for (int h = pulseHeight; h >= originalHeight; h--)
                {
                    _progressBar.Height = h;
                    await Task.Delay(15);
                    _parentControl.Refresh();
                }

                await Task.Delay(200); // Pause between pulses
            }
        }
    }

    public static class ProgressSteps
    {
        public static readonly string[] RoutineGeneration = {
            "Analizando informacin del usuario",
            "Evaluando limitaciones fsicas",
            "Seleccionando ejercicios apropiados",
            "Calculando series y repeticiones",
            "Organizando das de entrenamiento",
            "Aplicando recomendaciones de IA",
            "Generando plan personalizado",
            "Finalizando rutina"
        };

        public static readonly string[] DocumentExport = {
            "Preparando contenido para exportacin",
            "Aplicando formato profesional",
            "Generando estructura del documento",
            "Aadiendo informacin del cliente",
            "Insertando ejercicios y series",
            "Aplicando estilos y diseo",
            "Guardando archivo final"
        };

        public static readonly string[] DataValidation = {
            "Validando datos del usuario",
            "Verificando objetivos de entrenamiento",
            "Comprobando limitaciones fsicas",
            "Validando das de entrenamiento",
            "Confirmando nivel de experiencia"
        };

        public static readonly string[] AIProcessing = {
            "Conectando con servicio de IA",
            "Enviando datos del usuario",
            "Procesando recomendaciones",
            "Recibiendo sugerencias personalizadas",
            "Integrando resultados"
        };
    }
}