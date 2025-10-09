using AutoMapper;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Application.Mappings;

/// <summary>
/// Perfil de mapeo de AutoMapper
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Exercise mappings
        CreateMap<Exercise, ExerciseCatalogItemDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.SpanishName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Equipment.SpanishName))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty.Name))
            .ForMember(dest => dest.HasImage, opt => opt.MapFrom(src => src.ImagePaths.Any()))
            .ForMember(dest => dest.TargetMuscles, opt => opt.MapFrom(src => src.TargetMuscles.Select(m => m.SpanishName).ToList()))
            .ForMember(dest => dest.SecondaryMuscles, opt => opt.MapFrom(src => src.SecondaryMuscles.Select(m => m.SpanishName).ToList()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<Exercise, ExerciseDto>()
            .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Equipment.SpanishName))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty.Name))
            .ForMember(dest => dest.DifficultyLevel, opt => opt.MapFrom(src => src.Difficulty.Level))
            .ForMember(dest => dest.TargetMuscles, opt => opt.MapFrom(src => src.TargetMuscles.Select(m => m.SpanishName).ToList()))
            .ForMember(dest => dest.SecondaryMuscles, opt => opt.MapFrom(src => src.SecondaryMuscles.Select(m => m.SpanishName).ToList()))
            .ForMember(dest => dest.ImagePaths, opt => opt.MapFrom(src => src.ImagePaths.ToList()));

        // ExerciseSet mappings
        CreateMap<ExerciseSet, ExerciseSetDto>();

        // Routine mappings
        CreateMap<Routine, RoutineDto>()
            .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises));

        CreateMap<RoutineExercise, RoutineExerciseDto>()
            .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.Exercise.Id))
            .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
            .ForMember(dest => dest.Sets, opt => opt.MapFrom(src => src.Sets));

        // WorkoutPlan mappings
        CreateMap<WorkoutPlan, WorkoutPlanDto>()
            .ForMember(dest => dest.UserLevel, opt => opt.MapFrom(src => src.UserLevel.Name))
            .ForMember(dest => dest.UserLevelNumeric, opt => opt.MapFrom(src => src.UserLevel.Level))
            .ForMember(dest => dest.Routines, opt => opt.MapFrom(src => src.Routines))
            .ForMember(dest => dest.UserLimitations, opt => opt.MapFrom(src => src.UserLimitations.ToList()))
            .ForMember(dest => dest.TotalExercises, opt => opt.MapFrom(src => src.GetTotalExercises()))
            .ForMember(dest => dest.TotalSets, opt => opt.MapFrom(src => src.GetTotalSets()))
            .ForMember(dest => dest.IsComplete, opt => opt.MapFrom(src => src.IsComplete()));
    }
}
