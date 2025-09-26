using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using GymRoutineGenerator.Data.Services;
using GymRoutineGenerator.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class UserDemographicsForm : UserControl
{
    public event EventHandler? DemographicsSaved;
    private readonly IUserProfileService? _userProfileService;

    public UserDemographicsForm()
    {
        this.InitializeComponent();
        _userProfileService = UI.App.ServiceProvider?.GetService<IUserProfileService>();
        InitializeEventHandlers();
        SetDefaultValues();
    }

    private void InitializeEventHandlers()
    {
        if (SaveButton != null) SaveButton.Click += SaveButton_Click;
        if (ClearButton != null) ClearButton.Click += ClearButton_Click;
        if (TrainingDaysSlider != null) TrainingDaysSlider.ValueChanged += TrainingDaysSlider_ValueChanged;
    }

    private void SetDefaultValues()
    {
        if (MaleRadioButton != null) MaleRadioButton.IsChecked = true;
        if (FitnessLevelComboBox != null) FitnessLevelComboBox.SelectedIndex = 0;
        if (GeneralFitnessGoalCheckBox != null) GeneralFitnessGoalCheckBox.IsChecked = true;
        UpdateTrainingDaysLabel();
    }

    private void TrainingDaysSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        UpdateTrainingDaysLabel();
    }

    private void UpdateTrainingDaysLabel()
    {
        if (TrainingDaysLabel != null && TrainingDaysSlider != null)
        {
            int days = (int)TrainingDaysSlider.Value;
            TrainingDaysLabel.Text = days == 1 ? "1 día" : $"{days} días";
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate(out var error))
        {
            ShowError(error);
            return;
        }
        try
        {
            if (_userProfileService != null)
            {
                var request = new UserProfileCreateRequest
                {
                    Name = NameTextBox.Text.Trim(),
                    Age = (int)AgeNumberBox.Value,
                    Gender = MaleRadioButton.IsChecked == true ? Gender.Hombre : (FemaleRadioButton.IsChecked == true ? Gender.Mujer : Gender.Otro),
                    TrainingDaysPerWeek = (int)TrainingDaysSlider.Value
                };

                var validation = await _userProfileService.ValidateUserProfileAsync(request);
                if (!validation.IsValid)
                {
                    ShowError(string.Join("; ", validation.Errors));
                    return;
                }

                var profile = await _userProfileService.CreateUserProfileAsync(request);
                ShowSuccess($"Datos personales guardados (ID: {profile.Id})");
            }
            else
            {
                ShowSuccess("Datos validados (modo sin persistencia: servicio no disponible)");
            }
            DemographicsSaved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ShowError($"No se pudo guardar el perfil: {ex.Message}");
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        NameTextBox.Text = string.Empty;
        AgeNumberBox.Value = 25;
        MaleRadioButton.IsChecked = true;
        FemaleRadioButton.IsChecked = false;
        OtherRadioButton.IsChecked = false;
        FitnessLevelComboBox.SelectedIndex = 0;
        TrainingDaysSlider.Value = 3;

        // Clear all goal checkboxes
        WeightLossGoalCheckBox.IsChecked = false;
        MuscleGainGoalCheckBox.IsChecked = false;
        EnduranceGoalCheckBox.IsChecked = false;
        StrengthGoalCheckBox.IsChecked = false;
        GeneralFitnessGoalCheckBox.IsChecked = true; // Default

        HideError();
        UpdateTrainingDaysLabel();
    }

    private bool Validate(out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            error = "Ingrese el nombre del cliente";
            return false;
        }

        if (NameTextBox.Text.Trim().Length < 2)
        {
            error = "El nombre debe tener al menos 2 caracteres";
            return false;
        }

        if (AgeNumberBox.Value < 12 || AgeNumberBox.Value > 99)
        {
            error = "La edad debe estar entre 12 y 99 años";
            return false;
        }

        if (!MaleRadioButton.IsChecked.HasValue ||
            (!MaleRadioButton.IsChecked.Value && !FemaleRadioButton.IsChecked.Value && !OtherRadioButton.IsChecked.Value))
        {
            error = "Seleccione el género del cliente";
            return false;
        }

        if (FitnessLevelComboBox.SelectedIndex < 0)
        {
            error = "Seleccione el nivel de condición física";
            return false;
        }

        var hasGoals = WeightLossGoalCheckBox.IsChecked == true ||
                      MuscleGainGoalCheckBox.IsChecked == true ||
                      EnduranceGoalCheckBox.IsChecked == true ||
                      StrengthGoalCheckBox.IsChecked == true ||
                      GeneralFitnessGoalCheckBox.IsChecked == true;

        if (!hasGoals)
        {
            error = "Seleccione al menos un objetivo principal";
            return false;
        }

        return true;
    }

    public UserProfileCreateRequest GetUserProfile()
    {
        return new UserProfileCreateRequest
        {
            Name = NameTextBox.Text.Trim(),
            Age = (int)AgeNumberBox.Value,
            Gender = GetSelectedGender(),
            TrainingDaysPerWeek = (int)TrainingDaysSlider.Value
        };
    }

    public List<string> GetSelectedGoals()
    {
        var goals = new List<string>();

        if (WeightLossGoalCheckBox.IsChecked == true) goals.Add("Pérdida de peso");
        if (MuscleGainGoalCheckBox.IsChecked == true) goals.Add("Ganancia muscular");
        if (EnduranceGoalCheckBox.IsChecked == true) goals.Add("Mejora de resistencia");
        if (StrengthGoalCheckBox.IsChecked == true) goals.Add("Aumento de fuerza");
        if (GeneralFitnessGoalCheckBox.IsChecked == true) goals.Add("Estado físico general");

        return goals;
    }

    public string GetFitnessLevel()
    {
        return FitnessLevelComboBox.SelectedIndex switch
        {
            0 => "Principiante",
            1 => "Intermedio",
            2 => "Avanzado",
            _ => "Principiante"
        };
    }

    private Gender GetSelectedGender()
    {
        if (MaleRadioButton.IsChecked == true) return Gender.Hombre;
        if (FemaleRadioButton.IsChecked == true) return Gender.Mujer;
        return Gender.Otro;
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
        ErrorTextBlock.Visibility = Visibility.Visible;
    }

    private void ShowSuccess(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
        ErrorTextBlock.Visibility = Visibility.Visible;
    }

    private void HideError()
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;
    }
}
