using FluentValidation;
using GymRoutineGenerator.Application.Commands.WorkoutPlans;

namespace GymRoutineGenerator.Application.Validators;

/// <summary>
/// Validador para CreateWorkoutPlanCommand
/// </summary>
public class CreateWorkoutPlanCommandValidator : AbstractValidator<CreateWorkoutPlanCommand>
{
    public CreateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del plan es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre del usuario es requerido")
            .MaximumLength(100).WithMessage("El nombre del usuario no puede exceder 100 caracteres");

        RuleFor(x => x.UserAge)
            .InclusiveBetween(1, 120).WithMessage("La edad debe estar entre 1 y 120 años");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("El género es requerido")
            .Must(g => new[] { "Masculino", "Femenino", "Otro" }.Contains(g))
            .WithMessage("El género debe ser Masculino, Femenino u Otro");

        RuleFor(x => x.UserLevel)
            .NotEmpty().WithMessage("El nivel del usuario es requerido")
            .Must(l => new[] { "Principiante", "Principiante Avanzado", "Intermedio", "Avanzado", "Experto" }.Contains(l))
            .WithMessage("Nivel de usuario inválido");

        RuleFor(x => x.TrainingDaysPerWeek)
            .InclusiveBetween(1, 7).WithMessage("Los días de entrenamiento deben estar entre 1 y 7");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
