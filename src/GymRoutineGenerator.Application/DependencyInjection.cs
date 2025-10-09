using FluentValidation;
using GymRoutineGenerator.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GymRoutineGenerator.Application;

/// <summary>
/// Configuración de Dependency Injection para Application Layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Registrar MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Registrar AutoMapper
        services.AddAutoMapper(assembly);

        // Registrar FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
