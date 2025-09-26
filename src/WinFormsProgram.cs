using GymRoutineGenerator.WinForms.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GymRoutineGenerator.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();

        // Run the main form
        var mainForm = serviceProvider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder => builder.AddConsole());

        // Forms
        services.AddTransient<MainForm>();

        // Core services would be added here
        // services.AddScoped<IRoutineService, RoutineService>();
        // services.AddScoped<IExportService, ExportService>();
    }
}