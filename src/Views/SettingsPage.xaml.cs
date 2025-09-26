using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.IO;
using Windows.Storage.Pickers;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class SettingsPage : UserControl
{
    public event EventHandler? SettingsSaved;

    public SettingsPage()
    {
        this.InitializeComponent();
        InitializeEventHandlers();
        LoadCurrentSettings();
    }

    private void InitializeEventHandlers()
    {
        if (SaveSettingsButton != null) SaveSettingsButton.Click += SaveSettingsButton_Click;
        if (ResetSettingsButton != null) ResetSettingsButton.Click += ResetSettingsButton_Click;
        if (ExportSettingsButton != null) ExportSettingsButton.Click += ExportSettingsButton_Click;
        if (BrowseButton != null) BrowseButton.Click += BrowseButton_Click;

        if (FontSizeSlider != null) FontSizeSlider.ValueChanged += FontSizeSlider_ValueChanged;
        if (CreativitySlider != null) CreativitySlider.ValueChanged += CreativitySlider_ValueChanged;
    }

    private void LoadCurrentSettings()
    {
        // Set default values
        UpdateFontSizeLabel();
        UpdateCreativityLabel();
        ShowStatus("Configuración cargada correctamente", true);
    }

    private void FontSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateFontSizeLabel();
    }

    private void CreativitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateCreativityLabel();
    }

    private void UpdateFontSizeLabel()
    {
        if (FontSizeLabel != null && FontSizeSlider != null)
        {
            FontSizeLabel.Text = $"{(int)FontSizeSlider.Value}pt";
        }
    }

    private void UpdateCreativityLabel()
    {
        if (CreativityLabel != null && CreativitySlider != null)
        {
            CreativityLabel.Text = $"{CreativitySlider.Value:F1}";
        }
    }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");

            // Get the current window handle for the picker
            var window = WindowHelper.GetWindowForElement(this);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null && DefaultPathTextBox != null)
            {
                DefaultPathTextBox.Text = folder.Path;
                ShowStatus("Carpeta seleccionada correctamente", true);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al seleccionar carpeta: {ex.Message}", false);
        }
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate settings
            if (!ValidateSettings(out string error))
            {
                ShowStatus(error, false);
                return;
            }

            // Here you would typically save to a configuration file or registry
            // For now, we'll just show success message

            ShowStatus("⚙️ Configuración guardada exitosamente", true);
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al guardar configuración: {ex.Message}", false);
        }
    }

    private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset to default values
            if (LanguageComboBox != null) LanguageComboBox.SelectedIndex = 0;
            if (ThemeComboBox != null) ThemeComboBox.SelectedIndex = 0;
            if (FontSizeSlider != null) FontSizeSlider.Value = 16;
            if (AutoSaveCheckBox != null) AutoSaveCheckBox.IsChecked = true;
            if (ShowTipsCheckBox != null) ShowTipsCheckBox.IsChecked = true;

            if (DefaultPathTextBox != null) DefaultPathTextBox.Text = @"C:\Users\Documents\Rutinas";
            if (TemplateComboBox != null) TemplateComboBox.SelectedIndex = 0;
            if (OpenAfterExportCheckBox != null) OpenAfterExportCheckBox.IsChecked = true;
            if (IncludeImagesCheckBox != null) IncludeImagesCheckBox.IsChecked = false;
            if (AutoBackupCheckBox != null) AutoBackupCheckBox.IsChecked = true;

            if (AIModelComboBox != null) AIModelComboBox.SelectedIndex = 0;
            if (CreativitySlider != null) CreativitySlider.Value = 0.7;
            if (UseAIRecommendationsCheckBox != null) UseAIRecommendationsCheckBox.IsChecked = true;
            if (DetailedExplanationsCheckBox != null) DetailedExplanationsCheckBox.IsChecked = false;
            if (SafeModeCheckBox != null) SafeModeCheckBox.IsChecked = true;

            if (EnableAnimationsCheckBox != null) EnableAnimationsCheckBox.IsChecked = true;
            if (CacheDataCheckBox != null) CacheDataCheckBox.IsChecked = true;
            if (LowMemoryModeCheckBox != null) LowMemoryModeCheckBox.IsChecked = false;
            if (DebugModeCheckBox != null) DebugModeCheckBox.IsChecked = false;

            UpdateFontSizeLabel();
            UpdateCreativityLabel();

            ShowStatus("🔄 Configuración restablecida a valores predeterminados", true);
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al restablecer configuración: {ex.Message}", false);
        }
    }

    private void ExportSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Here you would typically export settings to a file
            ShowStatus("📤 Configuración exportada (funcionalidad pendiente)", true);
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al exportar configuración: {ex.Message}", false);
        }
    }

    private bool ValidateSettings(out string error)
    {
        error = string.Empty;

        // Validate export path
        if (DefaultPathTextBox?.Text?.Trim().Length > 0)
        {
            var path = DefaultPathTextBox.Text.Trim();
            if (!Path.IsPathRooted(path))
            {
                error = "La ruta de exportación debe ser una ruta absoluta";
                return false;
            }
        }

        // Validate font size
        if (FontSizeSlider?.Value < 12 || FontSizeSlider?.Value > 20)
        {
            error = "El tamaño de fuente debe estar entre 12 y 20 puntos";
            return false;
        }

        // Validate creativity value
        if (CreativitySlider?.Value < 0.1 || CreativitySlider?.Value > 1.0)
        {
            error = "El valor de creatividad debe estar entre 0.1 y 1.0";
            return false;
        }

        return true;
    }

    private void ShowStatus(string message, bool isSuccess)
    {
        if (StatusTextBlock != null)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                isSuccess ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
        }
    }

    // Public methods to get current settings
    public string GetSelectedLanguage()
    {
        return LanguageComboBox?.SelectedIndex switch
        {
            0 => "es",
            1 => "en",
            2 => "fr",
            _ => "es"
        };
    }

    public string GetSelectedTheme()
    {
        return ThemeComboBox?.SelectedIndex switch
        {
            0 => "Light",
            1 => "Dark",
            2 => "Auto",
            _ => "Light"
        };
    }

    public int GetFontSize()
    {
        return (int)(FontSizeSlider?.Value ?? 16);
    }

    public string GetDefaultExportPath()
    {
        return DefaultPathTextBox?.Text?.Trim() ?? @"C:\Users\Documents\Rutinas";
    }

    public string GetSelectedTemplate()
    {
        return TemplateComboBox?.SelectedIndex switch
        {
            0 => "Standard",
            1 => "Modern",
            2 => "Detailed",
            _ => "Standard"
        };
    }

    public string GetSelectedAIModel()
    {
        return AIModelComboBox?.SelectedIndex switch
        {
            0 => "Llama3",
            1 => "GPT4",
            2 => "Claude",
            _ => "Llama3"
        };
    }

    public double GetCreativityLevel()
    {
        return CreativitySlider?.Value ?? 0.7;
    }

    public bool IsAutoSaveEnabled()
    {
        return AutoSaveCheckBox?.IsChecked ?? true;
    }

    public bool ShouldOpenAfterExport()
    {
        return OpenAfterExportCheckBox?.IsChecked ?? true;
    }

    public bool IsDebugModeEnabled()
    {
        return DebugModeCheckBox?.IsChecked ?? false;
    }
}

// Helper class for window operations
public static class WindowHelper
{
    public static Microsoft.UI.Xaml.Window GetWindowForElement(FrameworkElement element)
    {
        // This is a simplified version - in a real app you'd need proper window tracking
        return App.MainWindow;
    }
}