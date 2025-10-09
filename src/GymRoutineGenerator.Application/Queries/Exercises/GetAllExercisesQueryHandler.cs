using AutoMapper;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Domain.Repositories;
using MediatR;

namespace GymRoutineGenerator.Application.Queries.Exercises;

/// <summary>
/// Handler para GetAllExercisesQuery
/// </summary>
public class GetAllExercisesQueryHandler : IRequestHandler<GetAllExercisesQuery, Result<List<ExerciseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllExercisesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ExerciseDto>>> Handle(GetAllExercisesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exercises = await _unitOfWork.Exercises.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<List<ExerciseDto>>(exercises.ToList());
            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ExerciseDto>>($"Error al obtener ejercicios: {ex.Message}");
        }
    }
}
