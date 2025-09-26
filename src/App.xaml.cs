using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GymRoutineGenerator.UI.Views;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Core.Services.Diagnostics;
using GymRoutineGenerator.Infrastructure.Documents;
using GymRoutineGenerator.Infrastructure.Diagnostics;
using GymRoutineGenerator.Data.Services;
using System.Runtime.InteropServices;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window MainWindow { get; private set; } = null!;
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        // Constructor definido más abajo para centralizar el manejo de errores

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<IExportService, SimpleExportService>();
            services.AddSingleton<IWordDocumentService, WordDocumentService>();
            services.AddSingleton<ITemplateManagerService, TemplateManagerService>();
            services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
            services.AddSingleton<IUserProfileService, UserProfileService>();

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Configure services first
            ConfigureServices();

            MainWindow = new Window();
            MainWindow.Title = "Generador de Rutinas";

            var frame = new Frame();
            frame.NavigationFailed += OnNavigationFailed;

            // Placeholder visible immediately
            frame.Content = new TextBlock { Text = "Cargando interfaz...", Margin = new Thickness(20), FontSize = 18 };
            MainWindow.Content = frame;
            MainWindow.Activate();
            EnsureWindowVisible();

            try
            {
                // Navigate to main page after window is visible
                frame.Navigate(typeof(MainPage), e.Arguments);
            }
            catch (Exception ex)
            {
                frame.Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = $"No se pudo abrir la pantalla principal.\n\nDetalle: {ex.Message}\n\n{ex.StackTrace}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20)
                    }
                };
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
            ConfigureServices();
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SW_SHOW = 5;
        private const int SW_MAXIMIZE = 3;
        private static readonly nint HWND_TOPMOST = new nint(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private static void EnsureWindowVisible()
        {
            try
            {
                if (MainWindow is null) return;
                var hwnd = WindowNative.GetWindowHandle(MainWindow);

                // Center using system metrics and Win32 calls
                int width = 1200, height = 800;
                int screenW = GetSystemMetrics(0); // SM_CXSCREEN
                int screenH = GetSystemMetrics(1); // SM_CYSCREEN
                int x = Math.Max(0, (screenW - width) / 2);
                int y = Math.Max(0, (screenH - height) / 2);

                SetWindowPos(hwnd, 0, x, y, width, height, SWP_SHOWWINDOW);

                // Bring to front and set topmost momentarily
                // Momentáneamente topmost para forzar visibilidad, luego quitar
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                ShowWindow(hwnd, SW_SHOW);
                MainWindow.Activate();
                SetForegroundWindow(hwnd);
                // Quitar topmost
                SetWindowPos(hwnd, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            catch
            {
                // Fallback
                try
                {
                    var hwnd = WindowNative.GetWindowHandle(MainWindow);
                    ShowWindow(hwnd, SW_MAXIMIZE);
                    SetForegroundWindow(hwnd);
                    MainWindow?.Activate();
                }
                catch { MainWindow?.Activate(); }
            }
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                // Log full exception details to local app data for troubleshooting
                try
                {
                    if (e.Exception is Exception ex)
                    {
                        ErrorLogger.LogException(ex, "UnhandledException");
                    }
                }
                catch { }

                var contentText = e.Exception is Exception exObj
                    ? ($"{exObj.Message}\n\n{exObj.StackTrace}")
                    : e.Message;

                var dialog = new ContentDialog
                {
                    Title = "Error no controlado",
                    Content = contentText,
                    CloseButtonText = "Cerrar",
                    XamlRoot = (MainWindow.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch
            {
                // Último recurso: mostrar texto en la ventana
                if (MainWindow?.Content is Frame fr)
                {
                    var details = e.Exception is Exception ex ? ex.ToString() : e.Message;
                    fr.Content = new TextBlock { Text = $"Error: {details}", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(20) };
                }
            }
        }
    }
}

internal static class ErrorLogger
{
    public static void LogException(Exception ex, string context)
    {
        try
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var app = System.IO.Path.Combine(folder, "GymRoutineGenerator");
            Directory.CreateDirectory(app);
            var logFile = System.IO.Path.Combine(app, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            var lines = new[]
            {
                $"[{DateTime.Now:O}] Context: {context}",
                ex.ToString(),
                new string('-', 80)
            };
            System.IO.File.AppendAllLines(logFile, lines);
        }
        catch { }
    }
}
