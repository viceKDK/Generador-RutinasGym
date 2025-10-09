using AutoMapper;
using FluentAssertions;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Application.Queries.Exercises;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.ValueObjects;
using Moq;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Application.Queries;

public class GetAllExercisesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllExercisesQueryHandler _handler;

    public GetAllExercisesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllExercisesQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            Exercise.Create("Press de Banca", EquipmentType.Barra, DifficultyLevel.Intermedio),
            Exercise.Create("Sentadillas", EquipmentType.Barra, DifficultyLevel.Intermedio),
            Exercise.Create("Dominadas", EquipmentType.PesoCorporal, DifficultyLevel.Avanzado)
        };

        var exerciseDtos = new List<ExerciseDto>
        {
            new ExerciseDto { Id = 1, Name = "Press de Banca" },
            new ExerciseDto { Id = 2, Name = "Sentadillas" },
            new ExerciseDto { Id = 3, Name = "Dominadas" }
        };

        var exerciseRepoMock = new Mock<IExerciseRepository>();
        exerciseRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(exercises);

        _unitOfWorkMock
            .Setup(uow => uow.Exercises)
            .Returns(exerciseRepoMock.Object);

        _mapperMock
            .Setup(m => m.Map<List<ExerciseDto>>(It.IsAny<List<Exercise>>()))
            .Returns(exerciseDtos);

        var query = new GetAllExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(e => e.Name == "Press de Banca");
        result.Value.Should().Contain(e => e.Name == "Sentadillas");
        result.Value.Should().Contain(e => e.Name == "Dominadas");

        exerciseRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<ExerciseDto>>(It.IsAny<List<Exercise>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoExercises_ShouldReturnEmptyList()
    {
        // Arrange
        var exercises = new List<Exercise>();
        var exerciseDtos = new List<ExerciseDto>();

        var exerciseRepoMock = new Mock<IExerciseRepository>();
        exerciseRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(exercises);

        _unitOfWorkMock
            .Setup(uow => uow.Exercises)
            .Returns(exerciseRepoMock.Object);

        _mapperMock
            .Setup(m => m.Map<List<ExerciseDto>>(It.IsAny<List<Exercise>>()))
            .Returns(exerciseDtos);

        var query = new GetAllExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        exerciseRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var exceptionMessage = "Database connection failed";

        var exerciseRepoMock = new Mock<IExerciseRepository>();
        exerciseRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        _unitOfWorkMock
            .Setup(uow => uow.Exercises)
            .Returns(exerciseRepoMock.Object);

        var query = new GetAllExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error al obtener ejercicios");
        result.Error.Should().Contain(exceptionMessage);

        exerciseRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<ExerciseDto>>(It.IsAny<List<Exercise>>()), Times.Never);
    }
}
