using FluentAssertions;
using FluentValidation.TestHelper;
using GymRoutineGenerator.Application.Commands.WorkoutPlans;
using GymRoutineGenerator.Application.Validators;

namespace GymRoutineGenerator.Tests.CleanArchitecture.Application.Validators;

public class CreateWorkoutPlanCommandValidatorTests
{
    private readonly CreateWorkoutPlanCommandValidator _validator;

    public CreateWorkoutPlanCommandValidatorTests()
    {
        _validator = new CreateWorkoutPlanCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Hipertrofia",
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            4,
            "Plan enfocado en ganar masa muscular"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldHaveValidationError(string? invalidName)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            invalidName!,
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre del plan es requerido");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            new string('A', 201), // 201 caracteres
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre no puede exceder 200 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyUserName_ShouldHaveValidationError(string? invalidUserName)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            invalidUserName!,
            25,
            "Masculino",
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("El nombre del usuario es requerido");
    }

    [Fact]
    public void Validate_UserNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            new string('A', 101), // 101 caracteres
            25,
            "Masculino",
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("El nombre del usuario no puede exceder 100 caracteres");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(121)]
    [InlineData(150)]
    public void Validate_InvalidAge_ShouldHaveValidationError(int invalidAge)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            invalidAge,
            "Masculino",
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserAge)
            .WithErrorMessage("La edad debe estar entre 1 y 120 años");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyGender_ShouldHaveValidationError(string? invalidGender)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            invalidGender!,
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorMessage("El género es requerido");
    }

    [Theory]
    [InlineData("Hombre")]
    [InlineData("Mujer")]
    [InlineData("Invalido")]
    public void Validate_InvalidGender_ShouldHaveValidationError(string invalidGender)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            invalidGender,
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorMessage("El género debe ser Masculino, Femenino u Otro");
    }

    [Theory]
    [InlineData("Masculino")]
    [InlineData("Femenino")]
    [InlineData("Otro")]
    public void Validate_ValidGender_ShouldNotHaveValidationError(string validGender)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            validGender,
            "Intermedio",
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyUserLevel_ShouldHaveValidationError(string? invalidLevel)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            invalidLevel!,
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserLevel)
            .WithErrorMessage("El nivel del usuario es requerido");
    }

    [Theory]
    [InlineData("Novato")]
    [InlineData("Pro")]
    [InlineData("Invalido")]
    public void Validate_InvalidUserLevel_ShouldHaveValidationError(string invalidLevel)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            invalidLevel,
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserLevel)
            .WithErrorMessage("Nivel de usuario inválido");
    }

    [Theory]
    [InlineData("Principiante")]
    [InlineData("Principiante Avanzado")]
    [InlineData("Intermedio")]
    [InlineData("Avanzado")]
    [InlineData("Experto")]
    public void Validate_ValidUserLevel_ShouldNotHaveValidationError(string validLevel)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            validLevel,
            4
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserLevel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(10)]
    public void Validate_InvalidTrainingDays_ShouldHaveValidationError(int invalidDays)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            invalidDays
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TrainingDaysPerWeek)
            .WithErrorMessage("Los días de entrenamiento deben estar entre 1 y 7");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void Validate_ValidTrainingDays_ShouldNotHaveValidationError(int validDays)
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            validDays
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TrainingDaysPerWeek);
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            4,
            new string('A', 1001) // 1001 caracteres
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("La descripción no puede exceder 1000 caracteres");
    }

    [Fact]
    public void Validate_ValidDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            4,
            "Esta es una descripción válida"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NullDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Plan Test",
            "Juan Pérez",
            25,
            "Masculino",
            "Intermedio",
            4,
            null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
