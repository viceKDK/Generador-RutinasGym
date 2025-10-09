using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Persistence.UnitOfWork;
using GymRoutineGenerator.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymRoutineGenerator.Data;

/// <summary>
/// Configuración de inyección de dependencias para la capa Data
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        // Registrar DbContext
        services.AddDbContext<GymRoutineContext>(options =>
            options.UseSqlite(connectionString));

        // Registrar Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
