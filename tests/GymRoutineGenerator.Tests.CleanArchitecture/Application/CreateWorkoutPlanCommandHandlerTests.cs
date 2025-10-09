using FluentAssertions;
using GymRoutineGenerator.Application.Commands.WorkoutPlans;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.ValueObjects;
using Moq;
using AutoMapper;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Application;

public class CreateWorkoutPlanCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateWorkoutPlanCommandHandler _handler;

    public CreateWorkoutPlanCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateWorkoutPlanCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateWorkoutPlan()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            Name: "Rutina Principiante",
            UserName: "Juan Pérez",
            UserAge: 25,
            Gender: "Masculino",
            UserLevel: "Principiante",
            TrainingDaysPerWeek: 3,
            Description: "Plan básico",
            UserLimitations: new List<string> { "rodilla" }
        );

        var workoutPlanRepositoryMock = new Mock<IWorkoutPlanRepository>();

        workoutPlanRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<WorkoutPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkoutPlan plan, CancellationToken _) => plan);

        _unitOfWorkMock
            .Setup(u => u.WorkoutPlans)
            .Returns(workoutPlanRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<GymRoutineGenerator.Application.DTOs.WorkoutPlanDto>(It.IsAny<WorkoutPlan>()))
            .Returns(new GymRoutineGenerator.Application.DTOs.WorkoutPlanDto
            {
                Id = 1,
                Name = command.Name,
                UserName = command.UserName
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(command.Name);
        result.Value.UserName.Should().Be(command.UserName);

        workoutPlanRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<WorkoutPlan>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_InvalidName_ShouldFail(string? invalidName)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            Name: invalidName!,
            UserName: "Juan Pérez",
            UserAge: 25,
            Gender: "Masculino",
            UserLevel: "Principiante",
            TrainingDaysPerWeek: 3
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // FluentValidation debería fallar antes de llegar al handler,
        // pero si llega, debería manejarse apropiadamente
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            Name: "Rutina Test",
            UserName: "Test User",
            UserAge: 30,
            Gender: "Masculino",
            UserLevel: "Intermedio",
            TrainingDaysPerWeek: 4
        );

        var workoutPlanRepositoryMock = new Mock<IWorkoutPlanRepository>();

        workoutPlanRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<WorkoutPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkoutPlan plan, CancellationToken _) => plan);

        _unitOfWorkMock
            .Setup(u => u.WorkoutPlans)
            .Returns(workoutPlanRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Database error");
    }
}
