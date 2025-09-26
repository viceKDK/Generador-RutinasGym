using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class AboutPage : UserControl
{
    public event EventHandler? AboutClosed;

    public AboutPage()
    {
        this.InitializeComponent();
        InitializeEventHandlers();
        LoadVersionInfo();
    }

    private void InitializeEventHandlers()
    {
        if (CloseButton != null) CloseButton.Click += CloseButton_Click;
        if (CheckUpdatesButton != null) CheckUpdatesButton.Click += CheckUpdatesButton_Click;
        if (SystemInfoButton != null) SystemInfoButton.Click += SystemInfoButton_Click;

        if (WebsiteLink != null) WebsiteLink.Click += WebsiteLink_Click;
        if (DocumentationLink != null) DocumentationLink.Click += DocumentationLink_Click;
        if (UpdatesLink != null) UpdatesLink.Click += UpdatesLink_Click;
        if (SupportLink != null) SupportLink.Click += SupportLink_Click;
        if (FeedbackLink != null) FeedbackLink.Click += FeedbackLink_Click;
        if (GitHubLink != null) GitHubLink.Click += GitHubLink_Click;
    }

    private void LoadVersionInfo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

            if (VersionLabel != null)
            {
                VersionLabel.Text = $"Versión {version?.ToString(3) ?? "1.0.0"} Beta";
            }

            // Update app name with current info
            if (AppNameLabel != null)
            {
                AppNameLabel.Text = fileVersion.ProductName ?? "Gym Routine Generator";
            }
        }
        catch (Exception)
        {
            // Fallback to default values if version info can't be retrieved
            if (VersionLabel != null)
            {
                VersionLabel.Text = "Versión 1.0.0 Beta";
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        AboutClosed?.Invoke(this, EventArgs.Empty);
    }

    private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        CheckUpdatesButton.IsEnabled = false;
        CheckUpdatesButton.Content = "🔄 Verificando...";

        try
        {
            // Simulate update check (in a real app, this would check a server)
            await Task.Delay(2000);

            // For now, show that the app is up to date
            var dialog = new ContentDialog()
            {
                Title = "Actualizaciones",
                Content = "✅ La aplicación está actualizada.\n\nVersión actual: 1.0.0 Beta\nÚltima verificación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog()
            {
                Title = "Error",
                Content = $"❌ No se pudo verificar las actualizaciones:\n{ex.Message}",
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            await errorDialog.ShowAsync();
        }
        finally
        {
            CheckUpdatesButton.IsEnabled = true;
            CheckUpdatesButton.Content = "🔄 Buscar Actualizaciones";
        }
    }

    private async void SystemInfoButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var systemInfo = GetSystemInfo();

            var dialog = new ContentDialog()
            {
                Title = "Información del Sistema",
                Content = systemInfo,
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog()
            {
                Title = "Error",
                Content = $"❌ No se pudo obtener la información del sistema:\n{ex.Message}",
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            await errorDialog.ShowAsync();
        }
    }

    private string GetSystemInfo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var osVersion = Environment.OSVersion;
            var runtimeVersion = Environment.Version;

            return $"🖥️ Sistema Operativo: {osVersion.VersionString}\n" +
                   $"🏗️ .NET Runtime: {runtimeVersion}\n" +
                   $"💾 Memoria Total: {GC.GetTotalMemory(false) / (1024 * 1024)} MB\n" +
                   $"🔧 Procesador: {Environment.ProcessorCount} cores\n" +
                   $"👤 Usuario: {Environment.UserName}\n" +
                   $"📁 Directorio de trabajo: {Environment.CurrentDirectory}\n" +
                   $"⏰ Tiempo activo: {Environment.TickCount / 1000} segundos";
        }
        catch
        {
            return "❌ No se pudo obtener la información del sistema";
        }
    }

    private async void WebsiteLink_Click(object sender, RoutedEventArgs e)
    {
        await OpenUrlSafely("https://github.com/vicentek/gym-routine-generator");
    }

    private async void DocumentationLink_Click(object sender, RoutedEventArgs e)
    {
        await OpenUrlSafely("https://github.com/vicentek/gym-routine-generator/wiki");
    }

    private async void UpdatesLink_Click(object sender, RoutedEventArgs e)
    {
        await OpenUrlSafely("https://github.com/vicentek/gym-routine-generator/releases");
    }

    private async void SupportLink_Click(object sender, RoutedEventArgs e)
    {
        await OpenUrlSafely("https://github.com/vicentek/gym-routine-generator/issues");
    }

    private async void FeedbackLink_Click(object sender, RoutedEventArgs e)
    {
        await OpenUrlSafely("https://github.com/vicentek/gym-routine-generator/discussions");
    }

    private async void GitHubLink_Click(object sender, RoutedEventArgs e)
    {
        await OpenUrlSafely("https://github.com/vicentek/gym-routine-generator");
    }

    private async Task OpenUrlSafely(string url)
    {
        try
        {
            await Launcher.LaunchUriAsync(new Uri(url));
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog()
            {
                Title = "Error",
                Content = $"❌ No se pudo abrir el enlace:\n{ex.Message}\n\nURL: {url}",
                CloseButtonText = "Cerrar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }

    // Public methods to get application information
    public string GetApplicationVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version?.ToString(3) ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }

    public string GetApplicationName()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.ProductName ?? "Gym Routine Generator";
        }
        catch
        {
            return "Gym Routine Generator";
        }
    }
}