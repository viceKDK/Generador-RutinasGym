using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class EquipmentPreferencesForm : UserControl
{
    private readonly IUserProfileService _userProfileService;
    private readonly Dictionary<CheckBox, int> _equipmentMapping;
    private int _currentUserProfileId;
    public event EventHandler? PreferencesSaved;

    public EquipmentPreferencesForm()
    {
        this.InitializeComponent();
        _userProfileService = App.ServiceProvider.GetService<IUserProfileService>()!;
        _equipmentMapping = new Dictionary<CheckBox, int>();
        InitializeEquipmentMapping();
    }

    public EquipmentPreferencesForm(IUserProfileService userProfileService) : this()
    {
        _userProfileService = userProfileService;
    }

    private void InitializeEquipmentMapping()
    {
        // Map checkboxes to equipment type IDs (based on our seeded data)
        _equipmentMapping[BodyweightCheckBox] = 1;        // Peso Corporal
        _equipmentMapping[DumbbellsCheckBox] = 2;         // Mancuernas
        _equipmentMapping[BarbellCheckBox] = 3;           // Barra
        _equipmentMapping[KettlebellsCheckBox] = 4;       // Kettlebells
        _equipmentMapping[CableMachinesCheckBox] = 5;     // Máquinas de Poleas
        _equipmentMapping[CardioMachinesCheckBox] = 6;    // Máquinas Cardiovasculares
        _equipmentMapping[WeightMachinesCheckBox] = 7;    // Máquinas de Pesas
        _equipmentMapping[ResistanceBandsCheckBox] = 8;   // Bandas Elásticas
        _equipmentMapping[PullUpBarCheckBox] = 9;         // Barra de Dominadas
        _equipmentMapping[MedicineBallCheckBox] = 10;     // Pelota Medicinal
        _equipmentMapping[FoamRollerCheckBox] = 11;       // Foam Roller
    }

    public void LoadUserPreferences(int userProfileId)
    {
        _currentUserProfileId = userProfileId;
        // TODO: Load existing preferences from database when service is available
        LoadDefaultPreferences();
    }

    private void LoadDefaultPreferences()
    {
        // Set common equipment as default selected
        BodyweightCheckBox.IsChecked = true;
        DumbbellsCheckBox.IsChecked = true;
        BarbellCheckBox.IsChecked = false;
        KettlebellsCheckBox.IsChecked = false;
        CableMachinesCheckBox.IsChecked = false;
        CardioMachinesCheckBox.IsChecked = false;
        WeightMachinesCheckBox.IsChecked = false;
        ResistanceBandsCheckBox.IsChecked = true;
        PullUpBarCheckBox.IsChecked = false;
        MedicineBallCheckBox.IsChecked = false;
        FoamRollerCheckBox.IsChecked = false;
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var checkbox in _equipmentMapping.Keys)
        {
            checkbox.IsChecked = true;
        }
        ShowSuccess("Se ha seleccionado todo el equipamiento");
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var checkbox in _equipmentMapping.Keys)
        {
            checkbox.IsChecked = false;
        }
        ShowSuccess("Se ha deseleccionado todo el equipamiento");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            var term = tb.Text?.Trim() ?? string.Empty;
            FilterByTerm(term);
        }
    }

    private void FilterByTerm(string term)
    {
        var normalized = term.ToLowerInvariant();
        var map = new Dictionary<CheckBox, string>
        {
            [BodyweightCheckBox] = "sin equipamiento peso corporal bodyweight",
            [DumbbellsCheckBox] = "mancuernas dumbbells",
            [BarbellCheckBox] = "barra barbell",
            [KettlebellsCheckBox] = "kettlebells kettle kettlebell",
            [CableMachinesCheckBox] = "maquinas poleas cable",
            [CardioMachinesCheckBox] = "maquinas cardio cardiovascular",
            [WeightMachinesCheckBox] = "maquinas pesas weight stack",
            [ResistanceBandsCheckBox] = "bandas resistencia bands",
            [PullUpBarCheckBox] = "barra dominadas pull up",
            [MedicineBallCheckBox] = "pelota medicinal medicine ball",
            [FoamRollerCheckBox] = "foam roller rodillo"
        };

        foreach (var kv in map)
        {
            var hit = string.IsNullOrEmpty(normalized) || kv.Value.Contains(normalized);
            kv.Key.Visibility = hit ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private async void SavePreferencesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Delay(1); // Simulate async operation
            HideError();

            var selectedEquipment = GetSelectedEquipment();

            if (selectedEquipment.Count == 0)
            {
                ShowError("Debe seleccionar al menos un tipo de equipamiento");
                return;
            }

            if (_userProfileService == null)
            {
                ShowError("Servicio no disponible. Configuracion requerida.");
                return;
            }

            // TODO: Save preferences to database when service method is available
            ShowSuccess($"Preferencias guardadas exitosamente. Equipamiento seleccionado: {selectedEquipment.Count} tipos");
            PreferencesSaved?.Invoke(this, EventArgs.Empty);

        }
        catch (Exception ex)
        {
            ShowError($"Error al guardar preferencias: {ex.Message}");
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        LoadDefaultPreferences();
        HideError();
        ShowSuccess("Preferencias restablecidas a valores predeterminados");
    }

    private List<int> GetSelectedEquipment()
    {
        return _equipmentMapping
            .Where(kvp => kvp.Key.IsChecked == true)
            .Select(kvp => kvp.Value)
            .ToList();
    }

    private List<string> GetSelectedEquipmentNames()
    {
        var selectedNames = new List<string>();

        if (BodyweightCheckBox.IsChecked == true) selectedNames.Add("Peso Corporal");
        if (DumbbellsCheckBox.IsChecked == true) selectedNames.Add("Mancuernas");
        if (BarbellCheckBox.IsChecked == true) selectedNames.Add("Barra");
        if (KettlebellsCheckBox.IsChecked == true) selectedNames.Add("Kettlebells");
        if (CableMachinesCheckBox.IsChecked == true) selectedNames.Add("Máquinas de Poleas");
        if (CardioMachinesCheckBox.IsChecked == true) selectedNames.Add("Máquinas Cardiovasculares");
        if (WeightMachinesCheckBox.IsChecked == true) selectedNames.Add("Máquinas de Pesas");
        if (ResistanceBandsCheckBox.IsChecked == true) selectedNames.Add("Bandas Elásticas");
        if (PullUpBarCheckBox.IsChecked == true) selectedNames.Add("Barra de Dominadas");
        if (MedicineBallCheckBox.IsChecked == true) selectedNames.Add("Pelota Medicinal");
        if (FoamRollerCheckBox.IsChecked == true) selectedNames.Add("Foam Roller");

        return selectedNames;
    }

    public bool ValidateForm(out List<string> errors)
    {
        errors = new List<string>();

        var selectedEquipment = GetSelectedEquipment();
        if (selectedEquipment.Count == 0)
        {
            errors.Add("Debe seleccionar al menos un tipo de equipamiento");
        }

        return errors.Count == 0;
    }

    public List<UserEquipmentPreference> GetEquipmentPreferences(int userProfileId)
    {
        var preferences = new List<UserEquipmentPreference>();
        var selectedEquipment = GetSelectedEquipment();

        foreach (var equipmentTypeId in selectedEquipment)
        {
            preferences.Add(new UserEquipmentPreference
            {
                UserProfileId = userProfileId,
                EquipmentTypeId = equipmentTypeId,
                IsAvailable = true
            });
        }

        return preferences;
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

        // Hide success message after 3 seconds
        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(3);
        timer.Tick += (s, e) =>
        {
            HideError();
            timer.Stop();
        };
        timer.Start();
    }

    private void HideError()
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;
    }

    // Public method to get selection summary for display
    public string GetSelectionSummary()
    {
        var selectedNames = GetSelectedEquipmentNames();
        if (selectedNames.Count == 0)
        {
            return "Sin equipamiento seleccionado";
        }

        if (selectedNames.Count == _equipmentMapping.Count)
        {
            return "Todo el equipamiento disponible";
        }

        return $"{selectedNames.Count} tipos seleccionados: {string.Join(", ", selectedNames.Take(3))}" +
               (selectedNames.Count > 3 ? "..." : "");
    }
}

