using FluentAssertions;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Persistence.Repositories;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Integration;

public class DomainExerciseRepositoryTests : IDisposable
{
    private readonly GymRoutineContext _context;
    private readonly DomainExerciseRepository _repository;

    public DomainExerciseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GymRoutineContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GymRoutineContext(options);
        _repository = new DomainExerciseRepository(_context);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        // Seed Equipment Types
        _context.EquipmentTypes.AddRange(
            new GymRoutineGenerator.Data.Entities.EquipmentType { Id = 1, Name = "Peso Corporal", SpanishName = "Peso Corporal" },
            new GymRoutineGenerator.Data.Entities.EquipmentType { Id = 2, Name = "Barra", SpanishName = "Barra" },
            new GymRoutineGenerator.Data.Entities.EquipmentType { Id = 3, Name = "Mancuernas", SpanishName = "Mancuernas" }
        );

        // Seed Muscle Groups
        _context.MuscleGroups.AddRange(
            new GymRoutineGenerator.Data.Entities.MuscleGroup { Id = 1, Name = "Pecho", SpanishName = "Pecho" },
            new GymRoutineGenerator.Data.Entities.MuscleGroup { Id = 2, Name = "Espalda", SpanishName = "Espalda" },
            new GymRoutineGenerator.Data.Entities.MuscleGroup { Id = 3, Name = "Cuádriceps", SpanishName = "Cuádriceps" }
        );

        // Seed Exercises
        _context.Exercises.AddRange(
            new GymRoutineGenerator.Data.Entities.Exercise
            {
                Id = 1,
                Name = "Flexiones",
                SpanishName = "Flexiones",
                Description = "Ejercicio de pecho con peso corporal",
                PrimaryMuscleGroupId = 1,
                EquipmentTypeId = 1,
                DifficultyLevel = GymRoutineGenerator.Core.Enums.DifficultyLevel.Beginner,
                IsActive = true
            },
            new GymRoutineGenerator.Data.Entities.Exercise
            {
                Id = 2,
                Name = "Press de Banca",
                SpanishName = "Press de Banca",
                Description = "Ejercicio compuesto de pecho",
                PrimaryMuscleGroupId = 1,
                EquipmentTypeId = 2,
                DifficultyLevel = GymRoutineGenerator.Core.Enums.DifficultyLevel.Intermediate,
                IsActive = true
            },
            new GymRoutineGenerator.Data.Entities.Exercise
            {
                Id = 3,
                Name = "Remo con Barra",
                SpanishName = "Remo con Barra",
                Description = "Ejercicio de espalda",
                PrimaryMuscleGroupId = 2,
                EquipmentTypeId = 2,
                DifficultyLevel = GymRoutineGenerator.Core.Enums.DifficultyLevel.Intermediate,
                IsActive = true
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllExercises()
    {
        // Act
        var exercises = await _repository.GetAllAsync();

        // Assert
        exercises.Should().NotBeNull();
        exercises.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ShouldReturnExercise()
    {
        // Act
        var exercise = await _repository.GetByIdAsync(1);

        // Assert
        exercise.Should().NotBeNull();
        exercise!.Name.Should().Be("Flexiones");
        exercise.Difficulty.Should().Be(DifficultyLevel.Principiante);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
    {
        // Act
        var exercise = await _repository.GetByIdAsync(999);

        // Assert
        exercise.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveExercisesAsync_ShouldReturnOnlyActiveExercises()
    {
        // Arrange
        _context.Exercises.Add(new GymRoutineGenerator.Data.Entities.Exercise
        {
            Id = 4,
            Name = "Ejercicio Inactivo",
            SpanishName = "Ejercicio Inactivo",
            PrimaryMuscleGroupId = 1,
            EquipmentTypeId = 1,
            DifficultyLevel = GymRoutineGenerator.Core.Enums.DifficultyLevel.Beginner,
            IsActive = false
        });
        await _context.SaveChangesAsync();

        // Act
        var activeExercises = await _repository.GetActiveExercisesAsync();

        // Assert
        activeExercises.Should().HaveCount(3);
        activeExercises.Should().OnlyContain(e => e.IsActive);
    }

    [Fact]
    public async Task GetByMuscleGroupAsync_ShouldReturnExercisesForMuscleGroup()
    {
        // Arrange
        var pecho = MuscleGroup.Pecho;

        // Act
        var exercises = await _repository.GetByMuscleGroupAsync(pecho);

        // Assert
        exercises.Should().HaveCount(2);
        exercises.Should().Contain(e => e.Name == "Flexiones");
        exercises.Should().Contain(e => e.Name == "Press de Banca");
    }

    [Fact]
    public async Task GetByEquipmentAsync_ShouldReturnExercisesWithEquipment()
    {
        // Arrange
        var barra = EquipmentType.Barra;

        // Act
        var exercises = await _repository.GetByEquipmentAsync(barra);

        // Assert
        exercises.Should().HaveCount(2);
        exercises.Should().Contain(e => e.Name == "Press de Banca");
        exercises.Should().Contain(e => e.Name == "Remo con Barra");
    }

    [Fact]
    public async Task GetByDifficultyAsync_ShouldReturnExercisesWithDifficulty()
    {
        // Arrange
        var intermedio = DifficultyLevel.Intermedio;

        // Act
        var exercises = await _repository.GetByDifficultyAsync(intermedio);

        // Assert
        exercises.Should().HaveCount(2);
        exercises.Should().Contain(e => e.Name == "Press de Banca");
        exercises.Should().Contain(e => e.Name == "Remo con Barra");
    }

    [Fact]
    public async Task AddAsync_ShouldAddExerciseToDatabase()
    {
        // Arrange
        var newExercise = Exercise.Create(
            "Sentadillas",
            EquipmentType.PesoCorporal,
            DifficultyLevel.Principiante,
            "Ejercicio de piernas"
        );
        newExercise.AddTargetMuscle(MuscleGroup.Cuadriceps);

        // Act
        var addedExercise = await _repository.AddAsync(newExercise);

        // Assert
        addedExercise.Should().NotBeNull();
        addedExercise.Id.Should().BeGreaterThan(0);

        var exerciseFromDb = await _repository.GetByIdAsync(addedExercise.Id);
        exerciseFromDb.Should().NotBeNull();
        exerciseFromDb!.Name.Should().Be("Sentadillas");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
