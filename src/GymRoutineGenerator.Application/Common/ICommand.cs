using MediatR;

namespace GymRoutineGenerator.Application.Common;

/// <summary>
/// Interfaz marcadora para Commands (CQRS)
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Interfaz marcadora para Commands con respuesta (CQRS)
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
