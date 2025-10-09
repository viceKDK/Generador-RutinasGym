using AutoMapper;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Domain.Repositories;
using MediatR;

namespace GymRoutineGenerator.Application.Queries.WorkoutPlans;

/// <summary>
/// Handler para GetWorkoutPlanByIdQuery
/// </summary>
public class GetWorkoutPlanByIdQueryHandler : IRequestHandler<GetWorkoutPlanByIdQuery, Result<WorkoutPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetWorkoutPlanByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<WorkoutPlanDto>> Handle(GetWorkoutPlanByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var workoutPlan = await _unitOfWork.WorkoutPlans.GetByIdAsync(request.Id, cancellationToken);

            if (workoutPlan == null)
            {
                return Result.Failure<WorkoutPlanDto>($"Plan de entrenamiento con ID {request.Id} no encontrado");
            }

            var dto = _mapper.Map<WorkoutPlanDto>(workoutPlan);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<WorkoutPlanDto>($"Error al obtener plan de entrenamiento: {ex.Message}");
        }
    }
}
