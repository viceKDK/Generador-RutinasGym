using AutoMapper;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Domain.Repositories;
using MediatR;

namespace GymRoutineGenerator.Application.Queries.Exercises;

/// <summary>
/// Handler que devuelve el catálogo de ejercicios para la gestión en UI.
/// </summary>
public class GetExerciseCatalogQueryHandler : IRequestHandler<GetExerciseCatalogQuery, Result<List<ExerciseCatalogItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExerciseCatalogQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ExerciseCatalogItemDto>>> Handle(GetExerciseCatalogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exercises = await _unitOfWork.Exercises.GetAllAsync(cancellationToken);
            var dtoList = _mapper.Map<List<ExerciseCatalogItemDto>>(exercises.ToList());
            return Result.Success(dtoList);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ExerciseCatalogItemDto>>($"Error al obtener catálogo de ejercicios: {ex.Message}");
        }
    }
}
