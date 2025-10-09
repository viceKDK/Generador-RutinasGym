using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.Services;
using GymRoutineGenerator.Infrastructure.DomainServices;
using Microsoft.Extensions.DependencyInjection;

namespace GymRoutineGenerator.Infrastructure;

/// <summary>
/// Configuración de inyección de dependencias para la capa Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Registrar Domain Services
        services.AddScoped<IRoutineSafetyValidator, RoutineSafetyValidator>();
        services.AddScoped<IExerciseSelector, ExerciseSelector>();

        // Aquí se pueden agregar otros servicios de Infrastructure:
        // - OllamaService
        // - ExportService
        // - etc.

        return services;
    }
}
