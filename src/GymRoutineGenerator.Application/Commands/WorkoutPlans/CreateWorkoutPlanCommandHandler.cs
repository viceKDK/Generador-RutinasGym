using AutoMapper;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.ValueObjects;
using MediatR;

namespace GymRoutineGenerator.Application.Commands.WorkoutPlans;

/// <summary>
/// Handler para CreateWorkoutPlanCommand
/// </summary>
public class CreateWorkoutPlanCommandHandler : IRequestHandler<CreateWorkoutPlanCommand, Result<WorkoutPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateWorkoutPlanCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<WorkoutPlanDto>> Handle(CreateWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Mapear nivel de usuario
            var userLevel = MapUserLevel(request.UserLevel);
            if (userLevel == null)
            {
                return Result.Failure<WorkoutPlanDto>($"Nivel de usuario inválido: {request.UserLevel}");
            }

            // Crear el WorkoutPlan usando el agregado de dominio
            var workoutPlan = WorkoutPlan.Create(
                request.Name,
                request.UserName,
                request.UserAge,
                request.Gender,
                userLevel,
                request.TrainingDaysPerWeek,
                request.Description
            );

            // Agregar limitaciones del usuario
            if (request.UserLimitations != null)
            {
                foreach (var limitation in request.UserLimitations)
                {
                    workoutPlan.AddUserLimitation(limitation);
                }
            }

            // Persistir usando Unit of Work
            await _unitOfWork.WorkoutPlans.AddAsync(workoutPlan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Mapear a DTO y retornar
            var dto = _mapper.Map<WorkoutPlanDto>(workoutPlan);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<WorkoutPlanDto>($"Error al crear plan de entrenamiento: {ex.Message}");
        }
    }

    private static DifficultyLevel? MapUserLevel(string levelName)
    {
        return levelName.ToLowerInvariant() switch
        {
            "principiante" => DifficultyLevel.Principiante,
            "principiante avanzado" => DifficultyLevel.PrincipianteAvanzado,
            "intermedio" => DifficultyLevel.Intermedio,
            "avanzado" => DifficultyLevel.Avanzado,
            "experto" => DifficultyLevel.Experto,
            _ => null
        };
    }
}
