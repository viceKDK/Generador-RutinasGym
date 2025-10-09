using AutoMapper;
using AutoMapper.Configuration;
using FluentAssertions;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Application.Mappings;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Application.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MappingProfile_Configuration_ShouldBeValid()
    {
        // Arrange
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

        // Act & Assert
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_Exercise_To_ExerciseDto_ShouldMapCorrectly()
    {
        // Arrange
        var exercise = Exercise.Create(
            "Press de Banca",
            EquipmentType.Barra,
            DifficultyLevel.Intermedio,
            "Ejercicio básico de pecho"
        );

        exercise.AddTargetMuscle(MuscleGroup.Pecho);
        exercise.AddTargetMuscle(MuscleGroup.Triceps);
        exercise.AddSecondaryMuscle(MuscleGroup.Hombros);

        // Usar reflexión para establecer el Id
        var idProperty = typeof(Exercise).GetProperty("Id");
        idProperty?.SetValue(exercise, 1);

        // Act
        var dto = _mapper.Map<ExerciseDto>(exercise);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Press de Banca");
        dto.Description.Should().Be("Ejercicio básico de pecho");
        dto.Equipment.Should().Be("Barra");
        dto.Difficulty.Should().Be("Intermedio");
        dto.DifficultyLevel.Should().Be(3); // Intermedio = Level 3
        dto.TargetMuscles.Should().Contain("Pecho");
        dto.TargetMuscles.Should().Contain("Tríceps");
        dto.SecondaryMuscles.Should().Contain("Hombros");
    }

    [Fact]
    public void Map_ExerciseSet_To_ExerciseSetDto_ShouldMapCorrectly()
    {
        // Arrange
        var exerciseSet = ExerciseSet.Create(
            repetitions: 10,
            weight: 100,
            restSeconds: 90,
            notes: "Serie de calentamiento"
        );

        // Act
        var dto = _mapper.Map<ExerciseSetDto>(exerciseSet);

        // Assert
        dto.Should().NotBeNull();
        dto.Repetitions.Should().Be(10);
        dto.Weight.Should().Be(100);
        dto.RestSeconds.Should().Be(90);
        dto.Notes.Should().Be("Serie de calentamiento");
    }

    [Fact]
    public void Map_Routine_To_RoutineDto_ShouldMapCorrectly()
    {
        // Arrange
        var routine = Routine.Create(
            "Día 1 - Pecho y Tríceps",
            1,
            "Entrenamiento de pecho"
        );

        var exercise = Exercise.Create("Press de Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var idProperty = typeof(Exercise).GetProperty("Id");
        idProperty?.SetValue(exercise, 1);

        var sets = new List<ExerciseSet>
        {
            ExerciseSet.Create(10, 100),
            ExerciseSet.Create(8, 110)
        };

        routine.AddExercise(exercise, 1, sets, "Ejercicio principal");

        // Establecer Id en routine
        var routineIdProperty = typeof(Routine).GetProperty("Id");
        routineIdProperty?.SetValue(routine, 10);

        // Act
        var dto = _mapper.Map<RoutineDto>(routine);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(10);
        dto.Name.Should().Be("Día 1 - Pecho y Tríceps");
        dto.DayNumber.Should().Be(1);
        dto.Description.Should().Be("Entrenamiento de pecho");
        dto.Exercises.Should().HaveCount(1);
        dto.Exercises.First().ExerciseId.Should().Be(1);
        dto.Exercises.First().ExerciseName.Should().Be("Press de Banca");
        dto.Exercises.First().Sets.Should().HaveCount(2);
    }

    [Fact]
    public void Map_WorkoutPlan_To_WorkoutPlanDto_ShouldMapCorrectly()
    {
        // Arrange
        var workoutPlan = WorkoutPlan.Create(
            "Plan Hipertrofia",
            "Juan Pérez",
            25,
            "Masculino",
            DifficultyLevel.Intermedio,
            4,
            "Plan enfocado en masa muscular"
        );

        workoutPlan.AddUserLimitation("rodilla");
        workoutPlan.AddUserLimitation("hombro");

        var routine1 = Routine.Create("Día 1", 1);
        var routine2 = Routine.Create("Día 2", 2);

        var exercise = Exercise.Create("Press de Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var sets = new List<ExerciseSet> { ExerciseSet.Create(10, 100) };

        routine1.AddExercise(exercise, 1, sets);
        routine2.AddExercise(exercise, 1, sets);

        workoutPlan.AddRoutine(routine1);
        workoutPlan.AddRoutine(routine2);

        // Establecer Id
        var idProperty = typeof(WorkoutPlan).GetProperty("Id");
        idProperty?.SetValue(workoutPlan, 100);

        // Act
        var dto = _mapper.Map<WorkoutPlanDto>(workoutPlan);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(100);
        dto.Name.Should().Be("Plan Hipertrofia");
        dto.UserName.Should().Be("Juan Pérez");
        dto.UserAge.Should().Be(25);
        dto.Gender.Should().Be("Masculino");
        dto.UserLevel.Should().Be("Intermedio");
        dto.UserLevelNumeric.Should().Be(3); // Intermedio = Level 3
        dto.TrainingDaysPerWeek.Should().Be(4);
        dto.Description.Should().Be("Plan enfocado en masa muscular");
        dto.UserLimitations.Should().Contain("rodilla");
        dto.UserLimitations.Should().Contain("hombro");
        dto.Routines.Should().HaveCount(2);
        dto.TotalExercises.Should().Be(2);
        dto.TotalSets.Should().Be(2);
        dto.IsComplete.Should().BeFalse(); // 2 rutinas pero necesita 4
    }

    [Fact]
    public void Map_WorkoutPlan_WithCompleteRoutines_ShouldShowIsComplete()
    {
        // Arrange
        var workoutPlan = WorkoutPlan.Create(
            "Plan Completo",
            "Usuario Test",
            30,
            "Masculino",
            DifficultyLevel.Intermedio,
            2 // Solo requiere 2 días
        );

        var exercise = Exercise.Create("Sentadillas", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var sets = new List<ExerciseSet> { ExerciseSet.Create(10, 100) };

        var routine1 = Routine.Create("Día 1", 1);
        var routine2 = Routine.Create("Día 2", 2);

        routine1.AddExercise(exercise, 1, sets);
        routine2.AddExercise(exercise, 1, sets);

        workoutPlan.AddRoutine(routine1);
        workoutPlan.AddRoutine(routine2);

        // Act
        var dto = _mapper.Map<WorkoutPlanDto>(workoutPlan);

        // Assert
        dto.Should().NotBeNull();
        dto.TrainingDaysPerWeek.Should().Be(2);
        dto.Routines.Should().HaveCount(2);
        dto.IsComplete.Should().BeTrue(); // 2 rutinas = 2 días requeridos
    }

    [Fact]
    public void Map_RoutineExercise_To_RoutineExerciseDto_ShouldMapCorrectly()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);
        var exercise = Exercise.Create("Dominadas", EquipmentType.PesoCorporal, DifficultyLevel.Avanzado);

        // Establecer Id
        var exerciseIdProperty = typeof(Exercise).GetProperty("Id");
        exerciseIdProperty?.SetValue(exercise, 5);

        var sets = new List<ExerciseSet>
        {
            ExerciseSet.Create(8, null, 60, "Serie al fallo"),
            ExerciseSet.Create(6, null, 90, "Serie final")
        };

        routine.AddExercise(exercise, 1, sets, "Ejercicio compuesto");

        // Act
        var routineDto = _mapper.Map<RoutineDto>(routine);
        var routineExerciseDto = routineDto.Exercises.First();

        // Assert
        routineExerciseDto.Should().NotBeNull();
        routineExerciseDto.ExerciseId.Should().Be(5);
        routineExerciseDto.ExerciseName.Should().Be("Dominadas");
        routineExerciseDto.Order.Should().Be(1);
        routineExerciseDto.Notes.Should().Be("Ejercicio compuesto");
        routineExerciseDto.Sets.Should().HaveCount(2);
        routineExerciseDto.Sets.First().Repetitions.Should().Be(8);
        routineExerciseDto.Sets.First().Weight.Should().BeNull();
        routineExerciseDto.Sets.First().Notes.Should().Be("Serie al fallo");
    }

    [Fact]
    public void Map_EmptyCollections_ShouldHandleGracefully()
    {
        // Arrange
        var exercise = Exercise.Create("Test Exercise", EquipmentType.Mancuernas, DifficultyLevel.Principiante);
        var routine = Routine.Create("Empty Routine", 1);
        var workoutPlan = WorkoutPlan.Create("Empty Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        // Act
        var exerciseDto = _mapper.Map<ExerciseDto>(exercise);
        var routineDto = _mapper.Map<RoutineDto>(routine);
        var workoutPlanDto = _mapper.Map<WorkoutPlanDto>(workoutPlan);

        // Assert
        exerciseDto.TargetMuscles.Should().BeEmpty();
        exerciseDto.SecondaryMuscles.Should().BeEmpty();
        exerciseDto.ImagePaths.Should().BeEmpty();

        routineDto.Exercises.Should().BeEmpty();

        workoutPlanDto.Routines.Should().BeEmpty();
        workoutPlanDto.UserLimitations.Should().BeEmpty();
        workoutPlanDto.TotalExercises.Should().Be(0);
        workoutPlanDto.TotalSets.Should().Be(0);
        workoutPlanDto.IsComplete.Should().BeFalse();
    }
}
