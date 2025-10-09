using AutoMapper;
using FluentAssertions;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Application.Queries.WorkoutPlans;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.ValueObjects;
using Moq;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Application.Queries;

public class GetWorkoutPlanByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetWorkoutPlanByIdQueryHandler _handler;

    public GetWorkoutPlanByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetWorkoutPlanByIdQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ShouldReturnWorkoutPlanDto()
    {
        // Arrange
        var planId = 1;
        var workoutPlan = WorkoutPlan.Create(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            DifficultyLevel.Intermedio,
            4,
            "Plan de prueba"
        );

        var workoutPlanDto = new WorkoutPlanDto
        {
            Id = planId,
            Name = "Plan Test",
            UserName = "Juan Pérez",
            UserAge = 25,
            Gender = "Masculino",
            TrainingDaysPerWeek = 4,
            Description = "Plan de prueba"
        };

        var workoutPlanRepoMock = new Mock<IWorkoutPlanRepository>();
        workoutPlanRepoMock
            .Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workoutPlan);

        _unitOfWorkMock
            .Setup(uow => uow.WorkoutPlans)
            .Returns(workoutPlanRepoMock.Object);

        _mapperMock
            .Setup(m => m.Map<WorkoutPlanDto>(workoutPlan))
            .Returns(workoutPlanDto);

        var query = new GetWorkoutPlanByIdQuery(planId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(planId);
        result.Value.Name.Should().Be("Plan Test");
        result.Value.UserName.Should().Be("Juan Pérez");

        workoutPlanRepoMock.Verify(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<WorkoutPlanDto>(workoutPlan), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingId_ShouldReturnFailure()
    {
        // Arrange
        var planId = 999;

        var workoutPlanRepoMock = new Mock<IWorkoutPlanRepository>();
        workoutPlanRepoMock
            .Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkoutPlan?)null);

        _unitOfWorkMock
            .Setup(uow => uow.WorkoutPlans)
            .Returns(workoutPlanRepoMock.Object);

        var query = new GetWorkoutPlanByIdQuery(planId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no encontrado");
        result.Error.Should().Contain(planId.ToString());

        workoutPlanRepoMock.Verify(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<WorkoutPlanDto>(It.IsAny<WorkoutPlan>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var planId = 1;
        var exceptionMessage = "Database connection failed";

        var workoutPlanRepoMock = new Mock<IWorkoutPlanRepository>();
        workoutPlanRepoMock
            .Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        _unitOfWorkMock
            .Setup(uow => uow.WorkoutPlans)
            .Returns(workoutPlanRepoMock.Object);

        var query = new GetWorkoutPlanByIdQuery(planId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error al obtener plan de entrenamiento");
        result.Error.Should().Contain(exceptionMessage);

        workoutPlanRepoMock.Verify(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<WorkoutPlanDto>(It.IsAny<WorkoutPlan>()), Times.Never);
    }
}
