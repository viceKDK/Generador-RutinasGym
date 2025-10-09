using FluentAssertions;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Domain;

public class RoutineTests
{
    [Fact]
    public void Routine_Create_ShouldCreateRoutineWithValidData()
    {
        // Arrange
        var name = "Lunes - Pecho y Tríceps";
        var dayNumber = 1; // Lunes
        var description = "Entrenamiento enfocado en pecho y tríceps";

        // Act
        var routine = Routine.Create(name, dayNumber, description);

        // Assert
        routine.Should().NotBeNull();
        routine.Name.Should().Be(name);
        routine.DayNumber.Should().Be(dayNumber);
        routine.Description.Should().Be(description);
        routine.Exercises.Should().BeEmpty();
        routine.GetTotalExercises().Should().Be(0);
        routine.GetTotalSets().Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Routine_Create_InvalidName_ShouldThrowException(string? invalidName)
    {
        // Arrange & Act
        Action act = () => Routine.Create(invalidName!, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*nombre*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(10)]
    public void Routine_Create_InvalidDayNumber_ShouldThrowException(int invalidDay)
    {
        // Arrange & Act
        Action act = () => Routine.Create("Test Routine", invalidDay);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*día*");
    }

    [Fact]
    public void Routine_AddExercise_ShouldAddExerciseToRoutine()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);
        var exercise = Exercise.Create("Press de Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        exercise.AddTargetMuscle(MuscleGroup.Pecho);

        var sets = new List<ExerciseSet>
        {
            ExerciseSet.Create(repetitions: 10, weight: 100)
        };

        // Act
        routine.AddExercise(exercise, order: 1, sets, "Ejercicio principal");

        // Assert
        routine.Exercises.Should().HaveCount(1);
        routine.GetTotalExercises().Should().Be(1);
        routine.GetTotalSets().Should().Be(1);

        var addedExercise = routine.Exercises.First();
        addedExercise.Exercise.Should().Be(exercise);
        addedExercise.Order.Should().Be(1);
        addedExercise.Sets.Should().HaveCount(1);
        addedExercise.Notes.Should().Be("Ejercicio principal");
    }

    [Fact]
    public void Routine_AddExercise_NullExercise_ShouldThrowException()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);
        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        // Act
        Action act = () => routine.AddExercise(null!, order: 1, sets);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Routine_AddExercise_EmptySets_ShouldThrowException()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);
        var exercise = Exercise.Create("Press de Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var emptySets = new List<ExerciseSet>();

        // Act
        Action act = () => routine.AddExercise(exercise, order: 1, emptySets);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*serie*");
    }

    [Fact]
    public void Routine_AddMultipleExercises_ShouldMaintainOrder()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);

        var exercise1 = Exercise.Create("Press Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var exercise2 = Exercise.Create("Aperturas", EquipmentType.Mancuernas, DifficultyLevel.Intermedio);
        var exercise3 = Exercise.Create("Fondos", EquipmentType.PesoCorporal, DifficultyLevel.Intermedio);

        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        // Act - Agregar en orden no secuencial
        routine.AddExercise(exercise2, order: 2, sets);
        routine.AddExercise(exercise3, order: 3, sets);
        routine.AddExercise(exercise1, order: 1, sets);

        // Assert - Deberían estar ordenados por Order
        routine.Exercises.Should().HaveCount(3);
        routine.Exercises.ElementAt(0).Exercise.Name.Should().Be("Press Banca");
        routine.Exercises.ElementAt(1).Exercise.Name.Should().Be("Aperturas");
        routine.Exercises.ElementAt(2).Exercise.Name.Should().Be("Fondos");
    }

    [Fact]
    public void Routine_RemoveExercise_ShouldRemoveExerciseFromRoutine()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);
        var exercise1 = Exercise.Create("Press Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var exercise2 = Exercise.Create("Aperturas", EquipmentType.Mancuernas, DifficultyLevel.Intermedio);

        exercise1.GetType().GetProperty("Id")!.SetValue(exercise1, 1);
        exercise2.GetType().GetProperty("Id")!.SetValue(exercise2, 2);

        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        routine.AddExercise(exercise1, 1, sets);
        routine.AddExercise(exercise2, 2, sets);

        // Act
        routine.RemoveExercise(1);

        // Assert
        routine.Exercises.Should().HaveCount(1);
        routine.Exercises.First().Exercise.Name.Should().Be("Aperturas");
    }

    [Fact]
    public void Routine_UpdateName_ShouldUpdateRoutineName()
    {
        // Arrange
        var routine = Routine.Create("Old Name", 1);

        // Act
        routine.UpdateName("New Name");

        // Assert
        routine.Name.Should().Be("New Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Routine_UpdateName_InvalidName_ShouldThrowException(string? invalidName)
    {
        // Arrange
        var routine = Routine.Create("Valid Name", 1);

        // Act
        Action act = () => routine.UpdateName(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*nombre*");
    }

    [Fact]
    public void Routine_GetTargetedMuscleGroups_ShouldReturnDistinctMuscleGroups()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);

        var exercise1 = Exercise.Create("Press Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        exercise1.AddTargetMuscle(MuscleGroup.Pecho);
        exercise1.AddTargetMuscle(MuscleGroup.Triceps);

        var exercise2 = Exercise.Create("Aperturas", EquipmentType.Mancuernas, DifficultyLevel.Intermedio);
        exercise2.AddTargetMuscle(MuscleGroup.Pecho);

        var exercise3 = Exercise.Create("Press Militar", EquipmentType.Barra, DifficultyLevel.Intermedio);
        exercise3.AddTargetMuscle(MuscleGroup.Hombros);

        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        routine.AddExercise(exercise1, 1, sets);
        routine.AddExercise(exercise2, 2, sets);
        routine.AddExercise(exercise3, 3, sets);

        // Act
        var targetedMuscles = routine.GetTargetedMuscleGroups().ToList();

        // Assert
        targetedMuscles.Should().Contain(m => m.SpanishName == "Pecho");
        targetedMuscles.Should().Contain(m => m.SpanishName == "Tríceps");
        targetedMuscles.Should().Contain(m => m.SpanishName == "Hombros");
        targetedMuscles.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Routine_GetTotalSets_ShouldSumAllSets()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);

        var exercise1 = Exercise.Create("Press Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var exercise2 = Exercise.Create("Aperturas", EquipmentType.Mancuernas, DifficultyLevel.Intermedio);

        var sets1 = new List<ExerciseSet>
        {
            ExerciseSet.Create(repetitions: 10, weight: 100),
            ExerciseSet.Create(repetitions: 8, weight: 110),
            ExerciseSet.Create(repetitions: 6, weight: 120)
        };

        var sets2 = new List<ExerciseSet>
        {
            ExerciseSet.Create(repetitions: 12, weight: 20),
            ExerciseSet.Create(repetitions: 12, weight: 20)
        };

        routine.AddExercise(exercise1, 1, sets1);
        routine.AddExercise(exercise2, 2, sets2);

        // Act
        var totalSets = routine.GetTotalSets();

        // Assert
        totalSets.Should().Be(5); // 3 sets del primer ejercicio + 2 sets del segundo
    }

    [Fact]
    public void Routine_ReorderExercise_ShouldUpdateExerciseOrder()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1);

        var exercise1 = Exercise.Create("Exercise 1", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var exercise2 = Exercise.Create("Exercise 2", EquipmentType.Mancuernas, DifficultyLevel.Intermedio);
        var exercise3 = Exercise.Create("Exercise 3", EquipmentType.PesoCorporal, DifficultyLevel.Intermedio);

        exercise1.GetType().GetProperty("Id")!.SetValue(exercise1, 1);
        exercise2.GetType().GetProperty("Id")!.SetValue(exercise2, 2);
        exercise3.GetType().GetProperty("Id")!.SetValue(exercise3, 3);

        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        routine.AddExercise(exercise1, 1, sets);
        routine.AddExercise(exercise2, 2, sets);
        routine.AddExercise(exercise3, 3, sets);

        // Act - Mover exercise3 a la posición 0
        routine.ReorderExercise(3, 0);

        // Assert
        routine.Exercises.ElementAt(0).Exercise.Name.Should().Be("Exercise 3");
        routine.Exercises.ElementAt(1).Exercise.Name.Should().Be("Exercise 1");
        routine.Exercises.ElementAt(2).Exercise.Name.Should().Be("Exercise 2");
    }

    [Fact]
    public void Routine_UpdateDescription_ShouldUpdateDescription()
    {
        // Arrange
        var routine = Routine.Create("Test Routine", 1, "Old description");

        // Act
        routine.UpdateDescription("New description");

        // Assert
        routine.Description.Should().Be("New description");
    }
}
