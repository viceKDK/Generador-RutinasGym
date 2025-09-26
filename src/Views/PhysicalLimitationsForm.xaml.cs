using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class PhysicalLimitationsForm : UserControl
{
    public event EventHandler? LimitationsSaved;

    public PhysicalLimitationsForm()
    {
        this.InitializeComponent();
        InitializeEventHandlers();
        InitializeRecommendations();
        UpdateIntensityText();
    }

    private void InitializeEventHandlers()
    {
        if (SaveLimitationsButton != null) SaveLimitationsButton.Click += SaveLimitationsButton_Click;
        if (ResetLimitationsButton != null) ResetLimitationsButton.Click += ResetLimitationsButton_Click;
        if (IntensitySlider != null) IntensitySlider.ValueChanged += IntensitySlider_ValueChanged;
        if (NoLimitationsButton != null) NoLimitationsButton.Click += NoLimitationsButton_Click;
        if (ClearAllLimitationsButton != null) ClearAllLimitationsButton.Click += ClearAllLimitationsButton_Click;

        // Hook up limitation checkboxes to update recommendations
        foreach (var checkBox in GetLimitationCheckBoxes())
        {
            if (checkBox != null)
            {
                checkBox.Checked += (s, e) => UpdateRecommendations();
                checkBox.Unchecked += (s, e) => UpdateRecommendations();
            }
        }

        if (OtherLimitationsTextBox != null)
        {
            OtherLimitationsTextBox.TextChanged += (s, e) => UpdateRecommendations();
        }
    }

    private IEnumerable<CheckBox> GetLimitationCheckBoxes()
    {
        return new[]
        {
            BackProblemsCheckBox, KneeProblemsCheckBox, ShoulderProblemsCheckBox, NeckProblemsCheckBox,
            CardiovascularIssuesCheckBox, RecentInjuryCheckBox, ArthritisCheckBox, PregnancyCheckBox
        }.Where(cb => cb != null);
    }

    private void NoLimitationsButton_Click(object sender, RoutedEventArgs e)
    {
        ClearAllLimitations();
        if (IntensitySlider != null) IntensitySlider.Value = 3;
        ShowSuccess("Configuración: Sin limitaciones físicas");
        UpdateRecommendations();
    }

    private void ClearAllLimitationsButton_Click(object sender, RoutedEventArgs e)
    {
        ClearAllLimitations();
        UpdateRecommendations();
    }

    private void ClearAllLimitations()
    {
        foreach (var checkBox in GetLimitationCheckBoxes())
        {
            checkBox.IsChecked = false;
        }
        if (OtherLimitationsTextBox != null) OtherLimitationsTextBox.Text = string.Empty;
    }

    private void IntensitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateIntensityText();
    }

    private void UpdateIntensityText()
    {
        if (IntensityText != null && IntensitySlider != null)
        {
            var level = (int)IntensitySlider.Value;
            var label = level switch
            {
                1 => "Baja intensidad",
                2 => "Intensidad moderada",
                _ => "Alta intensidad"
            };
            IntensityText.Text = label;
        }
        UpdateRecommendations();
    }

    private void InitializeRecommendations()
    {
        UpdateRecommendations();
    }

    private void SaveLimitationsButton_Click(object sender, RoutedEventArgs e)
    {
        var limitations = GetSelectedLimitations();
        var hasLimitations = limitations.Count > 0 || !string.IsNullOrWhiteSpace(OtherLimitationsTextBox?.Text);

        if (hasLimitations)
        {
            ShowSuccess($"Limitaciones guardadas correctamente ({limitations.Count} seleccionadas)");
        }
        else
        {
            ShowSuccess("Sin limitaciones físicas registradas");
        }

        LimitationsSaved?.Invoke(this, EventArgs.Empty);
    }

    private void ResetLimitationsButton_Click(object sender, RoutedEventArgs e)
    {
        ClearAllLimitations();
        if (IntensitySlider != null) IntensitySlider.Value = 3;
        HideError();
        UpdateIntensityText();
        UpdateRecommendations();
    }

    private void UpdateRecommendations()
    {
        if (RecommendationsTextBlock == null) return;

        var recommendations = new List<string>();
        var hasLimitations = false;

        if (BackProblemsCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Evitar ejercicios con carga axial excesiva (peso muerto pesado, sentadillas muy cargadas)");
            recommendations.Add("• Priorizar ejercicios de fortalecimiento del core");
            hasLimitations = true;
        }

        if (KneeProblemsCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Evitar sentadillas profundas y saltos de alto impacto");
            recommendations.Add("• Incluir ejercicios de fortalecimiento del cuádriceps con ROM controlado");
            hasLimitations = true;
        }

        if (ShoulderProblemsCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Evitar press militar por detrás de la cabeza");
            recommendations.Add("• Priorizar ejercicios de movilidad y estabilización escapular");
            hasLimitations = true;
        }

        if (NeckProblemsCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Evitar ejercicios que requieran flexión/extensión cervical excesiva");
            recommendations.Add("• Mantener posición neutra del cuello durante todos los ejercicios");
            hasLimitations = true;
        }

        if (CardiovascularIssuesCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Evitar ejercicios de muy alta intensidad (HIIT extremo)");
            recommendations.Add("• Monitorear frecuencia cardíaca durante el entrenamiento");
            hasLimitations = true;
        }

        if (RecentInjuryCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Evitar movimientos balísticos y entrenamiento al fallo");
            recommendations.Add("• Priorizar ejercicios de rehabilitación y movilidad");
            hasLimitations = true;
        }

        if (PregnancyCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Evitar ejercicios en posición supina después del primer trimestre");
            recommendations.Add("• Mantener hidratación adecuada y evitar sobrecalentamiento");
            hasLimitations = true;
        }

        if (ArthritisCheckBox?.IsChecked == true)
        {
            recommendations.Add("• Priorizar ejercicios de bajo impacto y en agua si es posible");
            recommendations.Add("• Incluir ejercicios de movilidad articular diarios");
            hasLimitations = true;
        }

        // Recomendaciones por nivel de intensidad
        var level = (int)(IntensitySlider?.Value ?? 3);
        if (level == 1)
        {
            recommendations.Add("• Usar cargas ligeras (40-60% 1RM) con mayor volumen");
        }
        else if (level == 2)
        {
            recommendations.Add("• Usar cargas moderadas (60-75% 1RM) con técnica perfecta");
        }

        // Otras limitaciones
        var otherLimitations = OtherLimitationsTextBox?.Text?.Trim();
        if (!string.IsNullOrEmpty(otherLimitations))
        {
            recommendations.Add($"• Consideración especial: {otherLimitations}");
            hasLimitations = true;
        }

        if (!hasLimitations)
        {
            RecommendationsTextBlock.Text = "✅ Sin limitaciones detectadas. Se puede aplicar un programa de entrenamiento estándar con progresión normal.";
        }
        else
        {
            RecommendationsTextBlock.Text = string.Join("\n", recommendations);
        }
    }

    public List<string> GetSelectedLimitations()
    {
        var limitations = new List<string>();

        if (BackProblemsCheckBox?.IsChecked == true) limitations.Add("Problemas de espalda");
        if (KneeProblemsCheckBox?.IsChecked == true) limitations.Add("Problemas de rodilla");
        if (ShoulderProblemsCheckBox?.IsChecked == true) limitations.Add("Problemas de hombro");
        if (NeckProblemsCheckBox?.IsChecked == true) limitations.Add("Problemas cervicales");
        if (CardiovascularIssuesCheckBox?.IsChecked == true) limitations.Add("Problemas cardiovasculares");
        if (RecentInjuryCheckBox?.IsChecked == true) limitations.Add("Lesión reciente");
        if (ArthritisCheckBox?.IsChecked == true) limitations.Add("Artritis");
        if (PregnancyCheckBox?.IsChecked == true) limitations.Add("Embarazo");

        return limitations;
    }

    public int GetIntensityLevel()
    {
        return (int)(IntensitySlider?.Value ?? 3);
    }

    public string GetOtherLimitations()
    {
        return OtherLimitationsTextBox?.Text?.Trim() ?? string.Empty;
    }

    private void ShowSuccess(string message)
    {
        if (ErrorTextBlock != null)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }

    private void HideError()
    {
        if (ErrorTextBlock != null)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }
    }
}
