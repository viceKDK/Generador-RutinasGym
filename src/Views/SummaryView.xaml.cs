using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class SummaryView : UserControl
{
    public SummaryView()
    {
        this.InitializeComponent();
    }

    public void RefreshSummary(UserInputWizard wizard,
                               EquipmentPreferencesForm equipment,
                               MuscleGroupFocusForm muscles,
                               PhysicalLimitationsForm limits,
                               UserDemographicsForm demo)
    {
        // Personal
        string name = (demo.FindName("NameTextBox") as TextBox)?.Text ?? "";
        int age = (int)((demo.FindName("AgeNumberBox") as NumberBox)?.Value ?? 0);
        string gender = ((demo.FindName("MaleRadioButton") as RadioButton)?.IsChecked == true) ? "Hombre" : (((demo.FindName("FemaleRadioButton") as RadioButton)?.IsChecked == true) ? "Mujer" : "Otro");
        int days = (int)((demo.FindName("TrainingDaysSlider") as Slider)?.Value ?? 0);
        TxtPersonal.Text = $"Nombre: {name} | Edad: {age} | Genero: {gender}";
        TxtTraining.Text = $"Dias/semana: {days}";

        // Equipment
        TxtEquipment.Text = equipment.GetSelectionSummary();

        // Muscles (simple)
        var parts = new[]
        {
            ("Pecho", (muscles.FindName("ChestCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("ChestSlider") as Slider)?.Value ?? 0)),
            ("Espalda", (muscles.FindName("BackCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("BackSlider") as Slider)?.Value ?? 0)),
            ("Hombros", (muscles.FindName("ShouldersCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("ShouldersSlider") as Slider)?.Value ?? 0)),
            ("Brazos", (muscles.FindName("ArmsCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("ArmsSlider") as Slider)?.Value ?? 0)),
            ("Core", (muscles.FindName("CoreCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("CoreSlider") as Slider)?.Value ?? 0)),
            ("Piernas", (muscles.FindName("LegsCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("LegsSlider") as Slider)?.Value ?? 0)),
            ("Gluteos", (muscles.FindName("GlutesCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("GlutesSlider") as Slider)?.Value ?? 0)),
            ("Cuerpo Completo", (muscles.FindName("FullBodyCheckBox") as CheckBox)?.IsChecked == true, (int)((muscles.FindName("FullBodySlider") as Slider)?.Value ?? 0))
        };
        var selected = parts.Where(p => p.Item2).Select(p => $"{p.Item1} (prio {p.Item3})");
        TxtMuscles.Text = selected.Any() ? string.Join(", ", selected) : "Sin seleccion";

        // Limits (simple)
        var lvl = (int)((limits.FindName("IntensitySlider") as Slider)?.Value ?? 0);
        TxtLimits.Text = $"Intensidad: {lvl}";
    }
}

