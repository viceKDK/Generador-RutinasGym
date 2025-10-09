using FluentAssertions;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Domain;

public class WorkoutPlanTests
{
    [Fact]
    public void WorkoutPlan_Create_ShouldCreatePlanWithValidData()
    {
        // Arrange
        var name = "Plan Hipertrofia";
        var userName = "Juan Pérez";
        var userAge = 25;
        var gender = "Masculino";
        var userLevel = DifficultyLevel.Intermedio;
        var trainingDays = 4;
        var description = "Plan enfocado en ganar masa muscular";

        // Act
        var plan = WorkoutPlan.Create(name, userName, userAge, gender, userLevel, trainingDays, description);

        // Assert
        plan.Should().NotBeNull();
        plan.Name.Should().Be(name);
        plan.UserName.Should().Be(userName);
        plan.UserAge.Should().Be(userAge);
        plan.Gender.Should().Be(gender);
        plan.UserLevel.Should().Be(userLevel);
        plan.TrainingDaysPerWeek.Should().Be(trainingDays);
        plan.Description.Should().Be(description);
        plan.Routines.Should().BeEmpty();
        plan.UserLimitations.Should().BeEmpty();
        plan.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void WorkoutPlan_Create_InvalidName_ShouldThrowException(string? invalidName)
    {
        // Arrange & Act
        Action act = () => WorkoutPlan.Create(
            invalidName!,
            "Juan Pérez",
            25,
            "Masculino",
            DifficultyLevel.Intermedio,
            4
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*nombre*plan*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(121)]
    [InlineData(150)]
    public void WorkoutPlan_Create_InvalidAge_ShouldThrowException(int invalidAge)
    {
        // Arrange & Act
        Action act = () => WorkoutPlan.Create(
            "Test Plan",
            "Juan Pérez",
            invalidAge,
            "Masculino",
            DifficultyLevel.Intermedio,
            4
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*edad*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(10)]
    public void WorkoutPlan_Create_InvalidTrainingDays_ShouldThrowException(int invalidDays)
    {
        // Arrange & Act
        Action act = () => WorkoutPlan.Create(
            "Test Plan",
            "Juan Pérez",
            25,
            "Masculino",
            DifficultyLevel.Intermedio,
            invalidDays
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*días*");
    }

    [Fact]
    public void WorkoutPlan_AddRoutine_ShouldAddRoutineToPlan()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);
        var routine = Routine.Create("Día 1", 1);

        // Act
        plan.AddRoutine(routine);

        // Assert
        plan.Routines.Should().HaveCount(1);
        plan.Routines.First().Should().Be(routine);
        plan.ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void WorkoutPlan_AddRoutine_ExceedingMaxRoutines_ShouldThrowException()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 2);
        var routine1 = Routine.Create("Día 1", 1);
        var routine2 = Routine.Create("Día 2", 2);
        var routine3 = Routine.Create("Día 3", 3);

        plan.AddRoutine(routine1);
        plan.AddRoutine(routine2);

        // Act
        Action act = () => plan.AddRoutine(routine3);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No se pueden agregar más*");
    }

    [Fact]
    public void WorkoutPlan_AddRoutine_NullRoutine_ShouldThrowException()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        // Act
        Action act = () => plan.AddRoutine(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WorkoutPlan_RemoveRoutine_ShouldRemoveRoutineFromPlan()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);
        var routine1 = Routine.Create("Día 1", 1);
        var routine2 = Routine.Create("Día 2", 2);

        routine1.GetType().GetProperty("Id")!.SetValue(routine1, 1);
        routine2.GetType().GetProperty("Id")!.SetValue(routine2, 2);

        plan.AddRoutine(routine1);
        plan.AddRoutine(routine2);

        // Act
        plan.RemoveRoutine(1);

        // Assert
        plan.Routines.Should().HaveCount(1);
        plan.Routines.First().DayNumber.Should().Be(2);
    }

    [Fact]
    public void WorkoutPlan_AddUserLimitation_ShouldAddLimitationToList()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        // Act
        plan.AddUserLimitation("rodilla");
        plan.AddUserLimitation("hombro");

        // Assert
        plan.UserLimitations.Should().HaveCount(2);
        plan.UserLimitations.Should().Contain("rodilla");
        plan.UserLimitations.Should().Contain("hombro");
    }

    [Fact]
    public void WorkoutPlan_AddUserLimitation_Duplicate_ShouldNotAddTwice()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        // Act
        plan.AddUserLimitation("rodilla");
        plan.AddUserLimitation("rodilla");

        // Assert
        plan.UserLimitations.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void WorkoutPlan_AddUserLimitation_InvalidLimitation_ShouldThrowException(string? invalidLimitation)
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        // Act
        Action act = () => plan.AddUserLimitation(invalidLimitation!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*limitación*");
    }

    [Fact]
    public void WorkoutPlan_RemoveUserLimitation_ShouldRemoveLimitationFromList()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);
        plan.AddUserLimitation("rodilla");
        plan.AddUserLimitation("hombro");

        // Act
        plan.RemoveUserLimitation("rodilla");

        // Assert
        plan.UserLimitations.Should().HaveCount(1);
        plan.UserLimitations.Should().Contain("hombro");
        plan.UserLimitations.Should().NotContain("rodilla");
    }

    [Fact]
    public void WorkoutPlan_GetTotalExercises_ShouldSumAllRoutineExercises()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        var routine1 = Routine.Create("Día 1", 1);
        var routine2 = Routine.Create("Día 2", 2);

        var exercise1 = Exercise.Create("Exercise 1", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var exercise2 = Exercise.Create("Exercise 2", EquipmentType.Mancuernas, DifficultyLevel.Intermedio);
        var exercise3 = Exercise.Create("Exercise 3", EquipmentType.PesoCorporal, DifficultyLevel.Intermedio);

        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        routine1.AddExercise(exercise1, 1, sets);
        routine1.AddExercise(exercise2, 2, sets);
        routine2.AddExercise(exercise3, 1, sets);

        plan.AddRoutine(routine1);
        plan.AddRoutine(routine2);

        // Act
        var totalExercises = plan.GetTotalExercises();

        // Assert
        totalExercises.Should().Be(3);
    }

    [Fact]
    public void WorkoutPlan_GetTotalSets_ShouldSumAllSetsFromAllRoutines()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        var routine1 = Routine.Create("Día 1", 1);
        var routine2 = Routine.Create("Día 2", 2);

        var exercise1 = Exercise.Create("Exercise 1", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var exercise2 = Exercise.Create("Exercise 2", EquipmentType.Mancuernas, DifficultyLevel.Intermedio);

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

        routine1.AddExercise(exercise1, 1, sets1);
        routine2.AddExercise(exercise2, 1, sets2);

        plan.AddRoutine(routine1);
        plan.AddRoutine(routine2);

        // Act
        var totalSets = plan.GetTotalSets();

        // Assert
        totalSets.Should().Be(5); // 3 sets de routine1 + 2 sets de routine2
    }

    [Fact]
    public void WorkoutPlan_IsComplete_WhenAllRoutinesHaveExercises_ShouldReturnTrue()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 2);

        var routine1 = Routine.Create("Día 1", 1);
        var routine2 = Routine.Create("Día 2", 2);

        var exercise = Exercise.Create("Exercise", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        routine1.AddExercise(exercise, 1, sets);
        routine2.AddExercise(exercise, 1, sets);

        plan.AddRoutine(routine1);
        plan.AddRoutine(routine2);

        // Act
        var isComplete = plan.IsComplete();

        // Assert
        isComplete.Should().BeTrue();
    }

    [Fact]
    public void WorkoutPlan_IsComplete_WhenMissingRoutines_ShouldReturnFalse()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test Plan", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        var routine1 = Routine.Create("Día 1", 1);
        var exercise = Exercise.Create("Exercise", EquipmentType.Barra, DifficultyLevel.Intermedio);
        var sets = new List<ExerciseSet> { ExerciseSet.Create(repetitions: 10, weight: 100) };

        routine1.AddExercise(exercise, 1, sets);
        plan.AddRoutine(routine1);

        // Act
        var isComplete = plan.IsComplete();

        // Assert
        isComplete.Should().BeFalse(); // Plan requiere 3 routines, solo tiene 1
    }

    [Fact]
    public void WorkoutPlan_UpdateName_ShouldUpdateNameAndModifiedAt()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Old Name", "User", 25, "M", DifficultyLevel.Intermedio, 3);

        // Act
        plan.UpdateName("New Name");

        // Assert
        plan.Name.Should().Be("New Name");
        plan.ModifiedAt.Should().NotBeNull();
        plan.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WorkoutPlan_UpdateUserLevel_ShouldUpdateLevel()
    {
        // Arrange
        var plan = WorkoutPlan.Create("Test", "User", 25, "M", DifficultyLevel.Principiante, 3);

        // Act
        plan.UpdateUserLevel(DifficultyLevel.Avanzado);

        // Assert
        plan.UserLevel.Should().Be(DifficultyLevel.Avanzado);
        plan.ModifiedAt.Should().NotBeNull();
    }
}
