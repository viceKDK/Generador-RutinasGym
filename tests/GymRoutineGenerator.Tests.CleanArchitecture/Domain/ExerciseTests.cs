using FluentAssertions;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Domain;

public class ExerciseTests
{
    [Fact]
    public void Exercise_Create_ShouldCreateExerciseWithValidData()
    {
        // Arrange
        var name = "Press de Banca";
        var equipment = EquipmentType.Barra;
        var difficulty = DifficultyLevel.Intermedio;
        var description = "Ejercicio compuesto para pecho";

        // Act
        var exercise = Exercise.Create(name, equipment, difficulty, description);

        // Assert
        exercise.Should().NotBeNull();
        exercise.Name.Should().Be(name);
        exercise.Equipment.Should().Be(equipment);
        exercise.Difficulty.Should().Be(difficulty);
        exercise.Description.Should().Be(description);
        exercise.IsActive.Should().BeTrue();
        exercise.TargetMuscles.Should().BeEmpty();
    }

    [Fact]
    public void Exercise_AddTargetMuscle_ShouldAddMuscleToList()
    {
        // Arrange
        var exercise = Exercise.Create("Press de Banca", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var pecho = MuscleGroup.Pecho;

        // Act
        exercise.AddTargetMuscle(pecho);

        // Assert
        exercise.TargetMuscles.Should().Contain(pecho);
        exercise.TargetMuscles.Should().HaveCount(1);
    }

    [Fact]
    public void Exercise_IsAppropriateForLevel_ShouldReturnTrueForSameOrHigherLevel()
    {
        // Arrange
        var exercise = Exercise.Create("Flexiones", EquipmentType.PesoCorporal, DifficultyLevel.Principiante);

        // Act & Assert
        exercise.IsAppropriateForLevel(DifficultyLevel.Principiante).Should().BeTrue();
        exercise.IsAppropriateForLevel(DifficultyLevel.Intermedio).Should().BeTrue();
        exercise.IsAppropriateForLevel(DifficultyLevel.Avanzado).Should().BeTrue();
    }

    [Fact]
    public void Exercise_IsAppropriateForLevel_ShouldReturnFalseForLowerLevel()
    {
        // Arrange
        var exercise = Exercise.Create("Muscle Up", EquipmentType.Barra, DifficultyLevel.Experto);

        // Act
        var result = exercise.IsAppropriateForLevel(DifficultyLevel.Principiante);

        // Assert
        result.Should().BeFalse();
    }
}
