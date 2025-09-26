using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using GymRoutineGenerator.Data.Services;
using System;
using System.Collections.Generic;

namespace GymRoutineGenerator.UI.Views
{
    public sealed partial class UserInputWizard : UserControl
    {
        private readonly IUserProfileService? _userProfileService;

        public UserInputWizard()
        {
            this.InitializeComponent();
            _userProfileService = App.ServiceProvider?.GetService<IUserProfileService>();
        }

        private void TrainingDaysSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (TrainingDaysText != null)
            {
                var days = (int)e.NewValue;
                TrainingDaysText.Text = $"{days} día{(days != 1 ? "s" : "")} por semana";
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowError("Por favor, ingresa tu nombre");
                return;
            }

            if (!int.TryParse(AgeTextBox.Text, out int age) || age < 16 || age > 80)
            {
                ShowError("La edad debe estar entre 16 y 80 años");
                return;
            }

            // Collect user data
            var userData = new UserInputData
            {
                Name = NameTextBox.Text,
                Age = age,
                Gender = GetSelectedGender(),
                TrainingDays = (int)TrainingDaysSlider.Value,
                FitnessLevel = GetSelectedFitnessLevel(),
                PreferredDuration = GetSelectedDuration(),
                Goals = GetSelectedGoals()
            };

            // Navigate to next step or process data
            // This would typically navigate to equipment preferences
            ShowSuccess("Informacion guardada correctamente. Continuando.....");
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset all controls to default values
            NameTextBox.Text = "";
            AgeTextBox.Text = "25";
            MaleRadioButton.IsChecked = true;
            FemaleRadioButton.IsChecked = false;
            OtherRadioButton.IsChecked = false;
            TrainingDaysSlider.Value = 3;
            FitnessLevelComboBox.SelectedIndex = 1;
            DurationComboBox.SelectedIndex = 1;

            // Reset checkboxes
            StrengthGoalCheckBox.IsChecked = true;
            MuscleGoalCheckBox.IsChecked = false;
            WeightLossGoalCheckBox.IsChecked = false;
            EnduranceGoalCheckBox.IsChecked = false;
            ToneGoalCheckBox.IsChecked = false;

            ShowSuccess("Formulario reiniciado");
        }

        private string GetSelectedGender()
        {
            if (MaleRadioButton.IsChecked == true) return "Male";
            if (FemaleRadioButton.IsChecked == true) return "Female";
            return "Other";
        }

        private string GetSelectedFitnessLevel()
        {
            return FitnessLevelComboBox.SelectedIndex switch
            {
                0 => "Beginner",
                1 => "Intermediate",
                2 => "Advanced",
                _ => "Intermediate"
            };
        }

        private int GetSelectedDuration()
        {
            return DurationComboBox.SelectedIndex switch
            {
                0 => 45,  // 30-45 minutes
                1 => 60,  // 45-60 minutes
                2 => 75,  // 60-90 minutes
                _ => 60
            };
        }

        private string[] GetSelectedGoals()
        {
            var goals = new List<string>();

            if (StrengthGoalCheckBox.IsChecked == true) goals.Add("Strength");
            if (MuscleGoalCheckBox.IsChecked == true) goals.Add("Muscle");
            if (WeightLossGoalCheckBox.IsChecked == true) goals.Add("Weight Loss");
            if (EnduranceGoalCheckBox.IsChecked == true) goals.Add("Endurance");
            if (ToneGoalCheckBox.IsChecked == true) goals.Add("Tone");

            return goals.ToArray();
        }

        private void ShowError(string message)
        {
            try
            {
                if (this.FindName("WizardInfo") is Microsoft.UI.Xaml.Controls.InfoBar info)
                {
                    info.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                    info.Title = "Error";
                    info.Message = message;
                    info.IsOpen = true;
                }
            }
            catch { }
        }

        private void ShowSuccess(string message)
        {
            try
            {
                if (this.FindName("WizardInfo") is Microsoft.UI.Xaml.Controls.InfoBar info)
                {
                    info.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    info.Title = "OK";
                    info.Message = message;
                    info.IsOpen = true;
                }
            }
            catch { }
        }
    }

    // Data model for user input
    public class UserInputData
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Gender { get; set; } = "";
        public int TrainingDays { get; set; }
        public string FitnessLevel { get; set; } = "";
        public int PreferredDuration { get; set; }
        public string[] Goals { get; set; } = Array.Empty<string>();
    }
}

