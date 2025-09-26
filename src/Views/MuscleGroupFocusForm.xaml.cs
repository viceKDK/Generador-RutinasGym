using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class MuscleGroupFocusForm : UserControl
{
    public event EventHandler? FocusSaved;
    public MuscleGroupFocusForm()
    {
        this.InitializeComponent();

        // Hook buttons if present
        if (SaveFocusButton != null) SaveFocusButton.Click += SaveFocusButton_Click;
        if (ResetFocusButton != null) ResetFocusButton.Click += ResetFocusButton_Click;

        // Hook template buttons
        if (WeightLossButton != null) WeightLossButton.Click += WeightLossButton_Click;
        if (MuscleGainButton != null) MuscleGainButton.Click += MuscleGainButton_Click;
        if (GeneralFitnessButton != null) GeneralFitnessButton.Click += GeneralFitnessButton_Click;
        if (ClearTemplateButton != null) ClearTemplateButton.Click += ClearTemplateButton_Click;

        // Hook sliders and checkboxes to update visualization
        HookChangeEvents();
        UpdateVisualization();
    }

    private void SaveFocusButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateSelection(out var error))
        {
            ShowError(error);
            return;
        }

        ShowSuccess("Preferencias de enfoque muscular guardadas");
        FocusSaved?.Invoke(this, EventArgs.Empty);
    }

    private void ResetFocusButton_Click(object sender, RoutedEventArgs e)
    {
        // Uncheck all and reset sliders to 1
        foreach (var cb in new[] { ChestCheckBox, BackCheckBox, ShouldersCheckBox, ArmsCheckBox, CoreCheckBox, LegsCheckBox, GlutesCheckBox, FullBodyCheckBox })
        {
            if (cb != null) cb.IsChecked = false;
        }
        foreach (var slider in new[] { ChestSlider, BackSlider, ShouldersSlider, ArmsSlider, CoreSlider, LegsSlider, GlutesSlider, FullBodySlider })
        {
            if (slider != null) slider.Value = 1;
        }
        HideError();
        UpdateVisualization();
    }

    private void HookChangeEvents()
    {
        var sliders = new[] { ChestSlider, BackSlider, ShouldersSlider, ArmsSlider, CoreSlider, LegsSlider, GlutesSlider, FullBodySlider };
        foreach (var s in sliders)
        {
            if (s != null) s.ValueChanged += (s2, e2) => UpdateVisualization();
        }
        var checks = new[] { ChestCheckBox, BackCheckBox, ShouldersCheckBox, ArmsCheckBox, CoreCheckBox, LegsCheckBox, GlutesCheckBox, FullBodyCheckBox };
        foreach (var c in checks)
        {
            if (c != null) c.Checked += (s2, e2) => UpdateVisualization();
            if (c != null) c.Unchecked += (s2, e2) => UpdateVisualization();
        }
    }

    private void UpdateVisualization()
    {
        // Color intensity: 1=light,2=medium,3=dark
        SetRegionColor(BodyChest, ChestCheckBox?.IsChecked == true, (int)(ChestSlider?.Value ?? 1));
        SetRegionColor(BodyBack, BackCheckBox?.IsChecked == true, (int)(BackSlider?.Value ?? 1));
        SetRegionColor(BodyShoulders, ShouldersCheckBox?.IsChecked == true, (int)(ShouldersSlider?.Value ?? 1));
        SetRegionColor(BodyArms, ArmsCheckBox?.IsChecked == true, (int)(ArmsSlider?.Value ?? 1));
        SetRegionColor(BodyCore, CoreCheckBox?.IsChecked == true, (int)(CoreSlider?.Value ?? 1));
        SetRegionColor(BodyLegs, LegsCheckBox?.IsChecked == true, (int)(LegsSlider?.Value ?? 1));
        SetRegionColor(BodyGlutes, GlutesCheckBox?.IsChecked == true, (int)(GlutesSlider?.Value ?? 1));
    }

    private void SetRegionColor(Microsoft.UI.Xaml.Shapes.Rectangle? rect, bool active, int level)
    {
        if (rect == null) return;
        var baseColor = active ? Microsoft.UI.Colors.SteelBlue : Microsoft.UI.Colors.LightGray;
        byte factor = (byte)(active ? (80 + level * 50) : 200);
        var color = Windows.UI.Color.FromArgb(255, (byte)Math.Min(255, baseColor.R + (3 - level) * 10), (byte)Math.Min(255, baseColor.G + (3 - level) * 10), (byte)Math.Min(255, baseColor.B + (3 - level) * 10));
        rect.Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
    }

    private bool ValidateSelection(out string error)
    {
        error = string.Empty;
        var anyChecked = new[] { ChestCheckBox, BackCheckBox, ShouldersCheckBox, ArmsCheckBox, CoreCheckBox, LegsCheckBox, GlutesCheckBox, FullBodyCheckBox }
            .Any(cb => cb != null && cb.IsChecked == true);
        if (!anyChecked)
        {
            error = "Seleccione al menos un grupo muscular";
            return false;
        }
        return true;
    }

    private void ShowError(string message)
    {
        if (ErrorTextBlock != null)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
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

    private void WeightLossButton_Click(object sender, RoutedEventArgs e)
    {
        ResetAll();
        // Weight loss focus: Full body with emphasis on cardio muscle groups
        SetMuscleGroup(CoreCheckBox, CoreSlider, 3);
        SetMuscleGroup(LegsCheckBox, LegsSlider, 3);
        SetMuscleGroup(FullBodyCheckBox, FullBodySlider, 2);
        UpdateVisualization();
    }

    private void MuscleGainButton_Click(object sender, RoutedEventArgs e)
    {
        ResetAll();
        // Muscle gain focus: Major muscle groups
        SetMuscleGroup(ChestCheckBox, ChestSlider, 3);
        SetMuscleGroup(BackCheckBox, BackSlider, 3);
        SetMuscleGroup(LegsCheckBox, LegsSlider, 3);
        SetMuscleGroup(ArmsCheckBox, ArmsSlider, 2);
        SetMuscleGroup(ShouldersCheckBox, ShouldersSlider, 2);
        UpdateVisualization();
    }

    private void GeneralFitnessButton_Click(object sender, RoutedEventArgs e)
    {
        ResetAll();
        // General fitness: Balanced approach
        SetMuscleGroup(FullBodyCheckBox, FullBodySlider, 3);
        SetMuscleGroup(CoreCheckBox, CoreSlider, 2);
        UpdateVisualization();
    }

    private void ClearTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        ResetFocusButton_Click(sender, e);
    }

    private void ResetAll()
    {
        foreach (var cb in new[] { ChestCheckBox, BackCheckBox, ShouldersCheckBox, ArmsCheckBox, CoreCheckBox, LegsCheckBox, GlutesCheckBox, FullBodyCheckBox })
        {
            if (cb != null) cb.IsChecked = false;
        }
        foreach (var slider in new[] { ChestSlider, BackSlider, ShouldersSlider, ArmsSlider, CoreSlider, LegsSlider, GlutesSlider, FullBodySlider })
        {
            if (slider != null) slider.Value = 1;
        }
    }

    private void SetMuscleGroup(CheckBox? checkBox, Slider? slider, int intensity)
    {
        if (checkBox != null) checkBox.IsChecked = true;
        if (slider != null) slider.Value = intensity;
    }
}

