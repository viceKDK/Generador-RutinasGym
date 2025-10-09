using MediatR;

namespace GymRoutineGenerator.Application.Common;

/// <summary>
/// Interfaz marcadora para Queries (CQRS)
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
