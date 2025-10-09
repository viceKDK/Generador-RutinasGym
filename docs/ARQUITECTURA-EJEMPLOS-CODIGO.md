# Ejemplos de Código - Refactoring hacia Clean Architecture

Este documento complementa `ARQUITECTURA-MEJORAS-PROPUESTAS.md` con ejemplos de código concretos.

---

## 📋 Tabla de Contenidos
1. [Domain Layer](#1-domain-layer)
2. [Application Layer](#2-application-layer)
3. [Infrastructure Layer](#3-infrastructure-layer)
4. [Presentation Layer](#4-presentation-layer)
5. [Casos de Uso Completos](#5-casos-de-uso-completos)

---

## 1. Domain Layer

### 1.1 Aggregate Root - WorkoutPlan

```csharp
// Domain/Aggregates/WorkoutPlan/WorkoutPlan.cs
namespace GymRoutineGenerator.Domain.Aggregates.WorkoutPlan
{
    public class WorkoutPlan : AggregateRoot<WorkoutPlanId>
    {
        private readonly List<Routine> _routines = new();
        private readonly List<DomainEvent> _domainEvents = new();

        public UserId UserId { get; private set; }
        public string Name { get; private set; }
        public PlanStatus Status { get; private set; }
        public DateRange ActivePeriod { get; private set; }
        public IReadOnlyList<Routine> Routines => _routines.AsReadOnly();

        // Constructor privado - solo accesible via factory methods
        private WorkoutPlan(WorkoutPlanId id, UserId userId, string name, DateRange period)
        {
            Id = id;
            UserId = userId;
            Name = name;
            ActivePeriod = period;
            Status = PlanStatus.Draft;
        }

        // Factory Method - asegura invariantes
        public static WorkoutPlan Create(UserId userId, string name, DateRange period)
        {
            // Validación de reglas de negocio
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Workout plan name cannot be empty");

            if (!period.IsValid())
                throw new DomainException("Invalid date range");

            var plan = new WorkoutPlan(WorkoutPlanId.Generate(), userId, name, period);

            // Domain Event
            plan.AddDomainEvent(new WorkoutPlanCreatedEvent(plan.Id, plan.UserId, plan.Name));

            return plan;
        }

        // Métodos de negocio
        public void AddRoutine(Routine routine)
        {
            if (Status == PlanStatus.Archived)
                throw new DomainException("Cannot add routines to archived plan");

            if (_routines.Count >= 7) // Max 7 días
                throw new DomainException("Cannot exceed 7 routines per week");

            if (_routines.Any(r => r.DayOfWeek == routine.DayOfWeek))
                throw new DomainException($"Routine already exists for {routine.DayOfWeek}");

            _routines.Add(routine);
            AddDomainEvent(new RoutineAddedToWorkoutPlanEvent(Id, routine.Id));
        }

        public void RemoveRoutine(RoutineId routineId)
        {
            var routine = _routines.FirstOrDefault(r => r.Id == routineId);
            if (routine == null)
                throw new DomainException("Routine not found in plan");

            _routines.Remove(routine);
            AddDomainEvent(new RoutineRemovedFromWorkoutPlanEvent(Id, routineId));
        }

        public void Activate()
        {
            if (Status == PlanStatus.Active)
                throw new DomainException("Plan is already active");

            if (!_routines.Any())
                throw new DomainException("Cannot activate plan without routines");

            Status = PlanStatus.Active;
            AddDomainEvent(new WorkoutPlanActivatedEvent(Id));
        }

        public void Archive()
        {
            if (Status == PlanStatus.Archived)
                throw new DomainException("Plan is already archived");

            Status = PlanStatus.Archived;
            AddDomainEvent(new WorkoutPlanArchivedEvent(Id));
        }

        // Domain Events management
        private void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IReadOnlyList<DomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
```

### 1.2 Entity - Routine

```csharp
// Domain/Aggregates/WorkoutPlan/Routine.cs
namespace GymRoutineGenerator.Domain.Aggregates.WorkoutPlan
{
    public class Routine : Entity<RoutineId>
    {
        private readonly List<ExerciseSet> _exerciseSets = new();

        public string Name { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public DifficultyLevel Difficulty { get; private set; }
        public Duration EstimatedDuration { get; private set; }
        public IReadOnlyList<ExerciseSet> ExerciseSets => _exerciseSets.AsReadOnly();

        private Routine(RoutineId id, string name, DayOfWeek dayOfWeek, DifficultyLevel difficulty)
        {
            Id = id;
            Name = name;
            DayOfWeek = dayOfWeek;
            Difficulty = difficulty;
        }

        public static Routine Create(string name, DayOfWeek dayOfWeek, DifficultyLevel difficulty)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Routine name is required");

            return new Routine(RoutineId.Generate(), name, dayOfWeek, difficulty);
        }

        public void AddExercise(Exercise exercise, int sets, int reps, RestPeriod rest)
        {
            if (_exerciseSets.Count >= 15)
                throw new DomainException("Cannot exceed 15 exercises per routine");

            if (_exerciseSets.Any(es => es.Exercise.Id == exercise.Id))
                throw new DomainException("Exercise already in routine");

            var exerciseSet = ExerciseSet.Create(exercise, sets, reps, rest);
            _exerciseSets.Add(exerciseSet);

            RecalculateDuration();
        }

        public void RemoveExercise(ExerciseId exerciseId)
        {
            var exerciseSet = _exerciseSets.FirstOrDefault(es => es.Exercise.Id == exerciseId);
            if (exerciseSet == null)
                throw new DomainException("Exercise not found in routine");

            _exerciseSets.Remove(exerciseSet);
            RecalculateDuration();
        }

        private void RecalculateDuration()
        {
            var totalMinutes = _exerciseSets.Sum(es => es.CalculateTotalTime().TotalMinutes);
            EstimatedDuration = Duration.FromMinutes((int)totalMinutes);
        }
    }
}
```

### 1.3 Value Objects

```csharp
// Domain/ValueObjects/MuscleGroup.cs
namespace GymRoutineGenerator.Domain.ValueObjects
{
    public class MuscleGroup : ValueObject
    {
        public string Name { get; private set; }
        public MuscleGroupCategory Category { get; private set; }

        private MuscleGroup(string name, MuscleGroupCategory category)
        {
            Name = name;
            Category = category;
        }

        public static MuscleGroup Create(string name, MuscleGroupCategory category)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Muscle group name is required");

            return new MuscleGroup(name.Trim(), category);
        }

        // Predefined muscle groups
        public static MuscleGroup Chest => new("Pecho", MuscleGroupCategory.Upper);
        public static MuscleGroup Back => new("Espalda", MuscleGroupCategory.Upper);
        public static MuscleGroup Legs => new("Piernas", MuscleGroupCategory.Lower);
        public static MuscleGroup Shoulders => new("Hombros", MuscleGroupCategory.Upper);
        public static MuscleGroup Arms => new("Brazos", MuscleGroupCategory.Upper);
        public static MuscleGroup Core => new("Core", MuscleGroupCategory.Core);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name.ToLowerInvariant();
        }
    }

    public enum MuscleGroupCategory
    {
        Upper,
        Lower,
        Core
    }
}

// Domain/ValueObjects/ExerciseSet.cs
namespace GymRoutineGenerator.Domain.ValueObjects
{
    public class ExerciseSet : ValueObject
    {
        public Exercise Exercise { get; private set; }
        public int Sets { get; private set; }
        public int Reps { get; private set; }
        public RestPeriod RestBetweenSets { get; private set; }

        private ExerciseSet(Exercise exercise, int sets, int reps, RestPeriod rest)
        {
            Exercise = exercise;
            Sets = sets;
            Reps = reps;
            RestBetweenSets = rest;
        }

        public static ExerciseSet Create(Exercise exercise, int sets, int reps, RestPeriod rest)
        {
            if (exercise == null)
                throw new DomainException("Exercise is required");
            if (sets <= 0 || sets > 10)
                throw new DomainException("Sets must be between 1 and 10");
            if (reps <= 0 || reps > 50)
                throw new DomainException("Reps must be between 1 and 50");

            return new ExerciseSet(exercise, sets, reps, rest);
        }

        public Duration CalculateTotalTime()
        {
            // Estimación: 3 segundos por rep + descanso entre sets
            var exerciseTime = Sets * Reps * 3; // segundos
            var restTime = (Sets - 1) * RestBetweenSets.Seconds;
            return Duration.FromSeconds(exerciseTime + restTime);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Exercise.Id;
            yield return Sets;
            yield return Reps;
        }
    }
}
```

### 1.4 Domain Services

```csharp
// Domain/DomainServices/IRoutineSafetyValidator.cs
namespace GymRoutineGenerator.Domain.DomainServices
{
    public interface IRoutineSafetyValidator
    {
        Task<SafetyValidationResult> ValidateAsync(
            Routine routine,
            UserProfile userProfile);
    }

    public class SafetyValidationResult
    {
        public bool IsSafe { get; private set; }
        public List<SafetyWarning> Warnings { get; private set; }
        public List<SafetyViolation> Violations { get; private set; }

        public static SafetyValidationResult Safe() =>
            new() { IsSafe = true, Warnings = new(), Violations = new() };

        public static SafetyValidationResult Unsafe(List<SafetyViolation> violations) =>
            new() { IsSafe = false, Warnings = new(), Violations = violations };
    }
}

// Domain/DomainServices/RoutineSafetyValidator.cs
namespace GymRoutineGenerator.Domain.DomainServices
{
    public class RoutineSafetyValidator : IRoutineSafetyValidator
    {
        public async Task<SafetyValidationResult> ValidateAsync(
            Routine routine,
            UserProfile userProfile)
        {
            var warnings = new List<SafetyWarning>();
            var violations = new List<SafetyViolation>();

            // Validar contra limitaciones físicas
            foreach (var exerciseSet in routine.ExerciseSets)
            {
                var exercise = exerciseSet.Exercise;

                // Verificar si el ejercicio afecta una zona con limitación
                foreach (var limitation in userProfile.PhysicalLimitations)
                {
                    if (IsExerciseConflicting(exercise, limitation))
                    {
                        violations.Add(new SafetyViolation(
                            exercise.Name,
                            $"Conflicto con limitación: {limitation.Description}",
                            SafetyLevel.High));
                    }
                }

                // Validar volumen para principiantes
                if (userProfile.FitnessLevel == FitnessLevel.Beginner)
                {
                    if (exerciseSet.Sets > 4)
                    {
                        warnings.Add(new SafetyWarning(
                            exercise.Name,
                            "Alto volumen para principiante",
                            "Considerar reducir a 3 sets"));
                    }
                }
            }

            return violations.Any()
                ? SafetyValidationResult.Unsafe(violations)
                : SafetyValidationResult.Safe();
        }

        private bool IsExerciseConflicting(Exercise exercise, PhysicalLimitation limitation)
        {
            // Lógica para determinar conflictos
            // Por ejemplo: Si limitación es "rodilla" y ejercicio es "sentadillas"
            return exercise.AffectedBodyParts.Any(bp =>
                limitation.AffectedAreas.Contains(bp));
        }
    }
}
```

### 1.5 Repository Interfaces (en Domain)

```csharp
// Domain/Repositories/IWorkoutPlanRepository.cs
namespace GymRoutineGenerator.Domain.Repositories
{
    public interface IWorkoutPlanRepository
    {
        Task<WorkoutPlan?> GetByIdAsync(WorkoutPlanId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkoutPlan>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
        Task<WorkoutPlan?> GetActiveByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
        Task AddAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default);
        void Update(WorkoutPlan workoutPlan);
        void Remove(WorkoutPlan workoutPlan);
    }
}

// Domain/Repositories/IExerciseRepository.cs
namespace GymRoutineGenerator.Domain.Repositories
{
    public interface IExerciseRepository
    {
        Task<Exercise?> GetByIdAsync(ExerciseId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(
            MuscleGroup muscleGroup,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Exercise>> GetByDifficultyAsync(
            DifficultyLevel difficulty,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Exercise>> FindAsync(
            ISpecification<Exercise> specification,
            CancellationToken cancellationToken = default);
    }
}
```

---

## 2. Application Layer

### 2.1 Commands con CQRS

```csharp
// Application/UseCases/Routines/Commands/CreateRoutine/CreateRoutineCommand.cs
namespace GymRoutineGenerator.Application.UseCases.Routines.Commands.CreateRoutine
{
    public record CreateRoutineCommand : IRequest<Result<RoutineDto>>
    {
        public string UserId { get; init; }
        public string RoutineName { get; init; }
        public DayOfWeek DayOfWeek { get; init; }
        public string DifficultyLevel { get; init; }
        public List<ExerciseRequestDto> Exercises { get; init; }

        public class ExerciseRequestDto
        {
            public int ExerciseId { get; init; }
            public int Sets { get; init; }
            public int Reps { get; init; }
            public int RestSeconds { get; init; }
        }
    }
}

// Application/UseCases/Routines/Commands/CreateRoutine/CreateRoutineCommandValidator.cs
namespace GymRoutineGenerator.Application.UseCases.Routines.Commands.CreateRoutine
{
    public class CreateRoutineCommandValidator : AbstractValidator<CreateRoutineCommand>
    {
        public CreateRoutineCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.RoutineName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Routine name must be between 1 and 100 characters");

            RuleFor(x => x.DifficultyLevel)
                .Must(BeValidDifficultyLevel)
                .WithMessage("Invalid difficulty level");

            RuleFor(x => x.Exercises)
                .NotEmpty()
                .WithMessage("At least one exercise is required")
                .Must(x => x.Count <= 15)
                .WithMessage("Cannot exceed 15 exercises per routine");

            RuleForEach(x => x.Exercises).ChildRules(exercise =>
            {
                exercise.RuleFor(e => e.Sets)
                    .InclusiveBetween(1, 10)
                    .WithMessage("Sets must be between 1 and 10");

                exercise.RuleFor(e => e.Reps)
                    .InclusiveBetween(1, 50)
                    .WithMessage("Reps must be between 1 and 50");
            });
        }

        private bool BeValidDifficultyLevel(string level)
        {
            return Enum.TryParse<DifficultyLevel>(level, out _);
        }
    }
}

// Application/UseCases/Routines/Commands/CreateRoutine/CreateRoutineCommandHandler.cs
namespace GymRoutineGenerator.Application.UseCases.Routines.Commands.CreateRoutine
{
    public class CreateRoutineCommandHandler : IRequestHandler<CreateRoutineCommand, Result<RoutineDto>>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepo;
        private readonly IExerciseRepository _exerciseRepo;
        private readonly IUserProfileRepository _userProfileRepo;
        private readonly IRoutineSafetyValidator _safetyValidator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRoutineCommandHandler> _logger;

        public CreateRoutineCommandHandler(
            IWorkoutPlanRepository workoutPlanRepo,
            IExerciseRepository exerciseRepo,
            IUserProfileRepository userProfileRepo,
            IRoutineSafetyValidator safetyValidator,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateRoutineCommandHandler> logger)
        {
            _workoutPlanRepo = workoutPlanRepo;
            _exerciseRepo = exerciseRepo;
            _userProfileRepo = userProfileRepo;
            _safetyValidator = safetyValidator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<RoutineDto>> Handle(
            CreateRoutineCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Obtener o crear WorkoutPlan activo
                var userId = UserId.Create(request.UserId);
                var workoutPlan = await _workoutPlanRepo.GetActiveByUserIdAsync(userId, cancellationToken);

                if (workoutPlan == null)
                {
                    // Crear un nuevo plan si no existe uno activo
                    workoutPlan = WorkoutPlan.Create(
                        userId,
                        "Mi Plan de Entrenamiento",
                        DateRange.FromNow(TimeSpan.FromDays(90)));

                    await _workoutPlanRepo.AddAsync(workoutPlan, cancellationToken);
                }

                // 2. Crear la rutina
                var difficulty = Enum.Parse<DifficultyLevel>(request.DifficultyLevel);
                var routine = Routine.Create(request.RoutineName, request.DayOfWeek, difficulty);

                // 3. Agregar ejercicios
                foreach (var exerciseRequest in request.Exercises)
                {
                    var exerciseId = ExerciseId.Create(exerciseRequest.ExerciseId);
                    var exercise = await _exerciseRepo.GetByIdAsync(exerciseId, cancellationToken);

                    if (exercise == null)
                    {
                        _logger.LogWarning("Exercise {ExerciseId} not found", exerciseRequest.ExerciseId);
                        return Result<RoutineDto>.Failure(
                            Error.NotFound($"Exercise with ID {exerciseRequest.ExerciseId} not found"));
                    }

                    var restPeriod = RestPeriod.FromSeconds(exerciseRequest.RestSeconds);
                    routine.AddExercise(exercise, exerciseRequest.Sets, exerciseRequest.Reps, restPeriod);
                }

                // 4. Validar seguridad
                var userProfile = await _userProfileRepo.GetByUserIdAsync(userId, cancellationToken);
                if (userProfile != null)
                {
                    var safetyResult = await _safetyValidator.ValidateAsync(routine, userProfile);
                    if (!safetyResult.IsSafe)
                    {
                        _logger.LogWarning("Safety validation failed for routine {RoutineName}", request.RoutineName);
                        return Result<RoutineDto>.Failure(
                            Error.Validation("Routine has safety violations",
                            safetyResult.Violations.Select(v => v.Message).ToList()));
                    }
                }

                // 5. Agregar rutina al plan
                workoutPlan.AddRoutine(routine);

                // 6. Persistir cambios
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 7. Mapear a DTO y retornar
                var routineDto = _mapper.Map<RoutineDto>(routine);
                return Result<RoutineDto>.Success(routineDto);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Domain exception while creating routine");
                return Result<RoutineDto>.Failure(Error.Validation(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating routine");
                return Result<RoutineDto>.Failure(Error.Unexpected("An unexpected error occurred"));
            }
        }
    }
}
```

### 2.2 Queries con CQRS

```csharp
// Application/UseCases/Routines/Queries/GetRoutineById/GetRoutineByIdQuery.cs
namespace GymRoutineGenerator.Application.UseCases.Routines.Queries.GetRoutineById
{
    public record GetRoutineByIdQuery(int RoutineId) : IRequest<Result<RoutineDetailDto>>;
}

// Application/UseCases/Routines/Queries/GetRoutineById/GetRoutineByIdQueryHandler.cs
namespace GymRoutineGenerator.Application.UseCases.Routines.Queries.GetRoutineById
{
    public class GetRoutineByIdQueryHandler : IRequestHandler<GetRoutineByIdQuery, Result<RoutineDetailDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetRoutineByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<RoutineDetailDto>> Handle(
            GetRoutineByIdQuery request,
            CancellationToken cancellationToken)
        {
            var routine = await _context.Routines
                .Include(r => r.ExerciseSets)
                    .ThenInclude(es => es.Exercise)
                        .ThenInclude(e => e.PrimaryMuscleGroup)
                .Include(r => r.ExerciseSets)
                    .ThenInclude(es => es.Exercise)
                        .ThenInclude(e => e.EquipmentType)
                .FirstOrDefaultAsync(r => r.Id == request.RoutineId, cancellationToken);

            if (routine == null)
                return Result<RoutineDetailDto>.Failure(Error.NotFound("Routine not found"));

            var dto = _mapper.Map<RoutineDetailDto>(routine);
            return Result<RoutineDetailDto>.Success(dto);
        }
    }
}
```

### 2.3 DTOs y Mapping

```csharp
// Application/DTOs/RoutineDto.cs
namespace GymRoutineGenerator.Application.DTOs
{
    public class RoutineDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DayOfWeek { get; set; }
        public string Difficulty { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public List<ExerciseSetDto> Exercises { get; set; }
    }

    public class RoutineDetailDto : RoutineDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public string CreatedBy { get; set; }
        public List<string> TargetMuscleGroups { get; set; }
    }

    public class ExerciseSetDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public string PrimaryMuscleGroup { get; set; }
        public string EquipmentType { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int RestSeconds { get; set; }
        public string ImageUrl { get; set; }
    }
}

// Application/Mappings/AutoMapperProfile.cs
namespace GymRoutineGenerator.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Domain to DTO
            CreateMap<Routine, RoutineDto>()
                .ForMember(d => d.DayOfWeek, opt => opt.MapFrom(s => s.DayOfWeek.ToString()))
                .ForMember(d => d.Difficulty, opt => opt.MapFrom(s => s.Difficulty.ToString()))
                .ForMember(d => d.EstimatedDurationMinutes,
                    opt => opt.MapFrom(s => s.EstimatedDuration.TotalMinutes))
                .ForMember(d => d.Exercises, opt => opt.MapFrom(s => s.ExerciseSets));

            CreateMap<ExerciseSet, ExerciseSetDto>()
                .ForMember(d => d.ExerciseId, opt => opt.MapFrom(s => s.Exercise.Id.Value))
                .ForMember(d => d.ExerciseName, opt => opt.MapFrom(s => s.Exercise.Name))
                .ForMember(d => d.PrimaryMuscleGroup,
                    opt => opt.MapFrom(s => s.Exercise.PrimaryMuscleGroup.Name))
                .ForMember(d => d.EquipmentType,
                    opt => opt.MapFrom(s => s.Exercise.EquipmentType.Name))
                .ForMember(d => d.RestSeconds,
                    opt => opt.MapFrom(s => s.RestBetweenSets.Seconds));
        }
    }
}
```

### 2.4 Result Pattern

```csharp
// Application/Common/Result.cs
namespace GymRoutineGenerator.Application.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Value { get; private set; }
        public Error? Error { get; private set; }

        private Result(bool isSuccess, T? value, Error? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) =>
            new(true, value, null);

        public static Result<T> Failure(Error error) =>
            new(false, default, error);

        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<Error, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
        }
    }

    public class Error
    {
        public string Code { get; private set; }
        public string Message { get; private set; }
        public ErrorType Type { get; private set; }
        public List<string>? ValidationErrors { get; private set; }

        private Error(ErrorType type, string code, string message, List<string>? validationErrors = null)
        {
            Type = type;
            Code = code;
            Message = message;
            ValidationErrors = validationErrors;
        }

        public static Error NotFound(string message) =>
            new(ErrorType.NotFound, "NOT_FOUND", message);

        public static Error Validation(string message, List<string>? errors = null) =>
            new(ErrorType.Validation, "VALIDATION_ERROR", message, errors);

        public static Error Unexpected(string message) =>
            new(ErrorType.Unexpected, "UNEXPECTED_ERROR", message);

        public static Error Conflict(string message) =>
            new(ErrorType.Conflict, "CONFLICT", message);
    }

    public enum ErrorType
    {
        NotFound,
        Validation,
        Conflict,
        Unexpected
    }
}
```

---

## 3. Infrastructure Layer

### 3.1 Repository Implementation

```csharp
// Infrastructure.Persistence/Repositories/WorkoutPlanRepository.cs
namespace GymRoutineGenerator.Infrastructure.Persistence.Repositories
{
    public class WorkoutPlanRepository : IWorkoutPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkoutPlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WorkoutPlan?> GetByIdAsync(
            WorkoutPlanId id,
            CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.Routines)
                    .ThenInclude(r => r.ExerciseSets)
                        .ThenInclude(es => es.Exercise)
                .FirstOrDefaultAsync(wp => wp.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<WorkoutPlan>> GetByUserIdAsync(
            UserId userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.Routines)
                .Where(wp => wp.UserId == userId)
                .OrderByDescending(wp => wp.ActivePeriod.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkoutPlan?> GetActiveByUserIdAsync(
            UserId userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.Routines)
                    .ThenInclude(r => r.ExerciseSets)
                .FirstOrDefaultAsync(
                    wp => wp.UserId == userId && wp.Status == PlanStatus.Active,
                    cancellationToken);
        }

        public async Task AddAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default)
        {
            await _context.WorkoutPlans.AddAsync(workoutPlan, cancellationToken);
        }

        public void Update(WorkoutPlan workoutPlan)
        {
            _context.WorkoutPlans.Update(workoutPlan);
        }

        public void Remove(WorkoutPlan workoutPlan)
        {
            _context.WorkoutPlans.Remove(workoutPlan);
        }
    }
}
```

### 3.2 Unit of Work

```csharp
// Infrastructure.Persistence/UnitOfWork.cs
namespace GymRoutineGenerator.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;

        public IExerciseRepository Exercises { get; }
        public IRoutineRepository Routines { get; }
        public IWorkoutPlanRepository WorkoutPlans { get; }
        public IUserProfileRepository UserProfiles { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IMediator mediator,
            IExerciseRepository exercises,
            IRoutineRepository routines,
            IWorkoutPlanRepository workoutPlans,
            IUserProfileRepository userProfiles)
        {
            _context = context;
            _mediator = mediator;
            Exercises = exercises;
            Routines = routines;
            WorkoutPlans = workoutPlans;
            UserProfiles = userProfiles;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch domain events BEFORE saving
            await DispatchDomainEventsAsync(cancellationToken);

            // Save changes
            return await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            var domainEntities = _context.ChangeTracker
                .Entries<IAggregateRoot>()
                .Where(x => x.Entity.GetDomainEvents().Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.GetDomainEvents())
                .ToList();

            domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
```

### 3.3 EF Core Configuration

```csharp
// Infrastructure.Persistence/Configurations/WorkoutPlanConfiguration.cs
namespace GymRoutineGenerator.Infrastructure.Persistence.Configurations
{
    public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
    {
        public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
        {
            builder.ToTable("WorkoutPlans");

            builder.HasKey(wp => wp.Id);

            builder.Property(wp => wp.Id)
                .HasConversion(
                    id => id.Value,
                    value => WorkoutPlanId.Create(value))
                .ValueGeneratedNever();

            builder.Property(wp => wp.UserId)
                .HasConversion(
                    id => id.Value,
                    value => UserId.Create(value))
                .IsRequired();

            builder.Property(wp => wp.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(wp => wp.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Value Object - DateRange
            builder.OwnsOne(wp => wp.ActivePeriod, period =>
            {
                period.Property(p => p.StartDate)
                    .HasColumnName("StartDate")
                    .IsRequired();

                period.Property(p => p.EndDate)
                    .HasColumnName("EndDate")
                    .IsRequired();
            });

            // Relationships
            builder.HasMany(wp => wp.Routines)
                .WithOne()
                .HasForeignKey("WorkoutPlanId")
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore domain events
            builder.Ignore(wp => wp.DomainEvents);

            // Indexes
            builder.HasIndex(wp => new { wp.UserId, wp.Status });
        }
    }
}
```

---

## 4. Presentation Layer

### 4.1 UI usando Application Layer

```csharp
// UI.WinForms/Features/Routines/CreateRoutineForm.cs
namespace GymRoutineGenerator.UI.WinForms.Features.Routines
{
    public partial class CreateRoutineForm : Form
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateRoutineForm> _logger;
        private readonly string _currentUserId;

        public CreateRoutineForm(
            IMediator mediator,
            ILogger<CreateRoutineForm> logger,
            string currentUserId)
        {
            _mediator = mediator;
            _logger = logger;
            _currentUserId = currentUserId;
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Construir command desde UI
                var command = new CreateRoutineCommand
                {
                    UserId = _currentUserId,
                    RoutineName = routineNameTextBox.Text,
                    DayOfWeek = (DayOfWeek)dayOfWeekComboBox.SelectedItem,
                    DifficultyLevel = difficultyComboBox.SelectedItem.ToString(),
                    Exercises = exercisesDataGrid.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow)
                        .Select(r => new CreateRoutineCommand.ExerciseRequestDto
                        {
                            ExerciseId = (int)r.Cells["ExerciseId"].Value,
                            Sets = (int)r.Cells["Sets"].Value,
                            Reps = (int)r.Cells["Reps"].Value,
                            RestSeconds = (int)r.Cells["Rest"].Value
                        })
                        .ToList()
                };

                // Enviar command via Mediator
                var result = await _mediator.Send(command);

                // Manejar resultado
                result.Match(
                    onSuccess: routine =>
                    {
                        MessageBox.Show(
                            $"Rutina '{routine.Name}' creada exitosamente",
                            "Éxito",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        DialogResult = DialogResult.OK;
                        Close();
                        return routine;
                    },
                    onFailure: error =>
                    {
                        ShowError(error);
                        return null;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating routine");
                MessageBox.Show(
                    "Error inesperado al crear rutina",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ShowError(Error error)
        {
            var message = error.Type switch
            {
                ErrorType.Validation => $"Error de validación:\n{error.Message}",
                ErrorType.NotFound => $"No encontrado:\n{error.Message}",
                ErrorType.Conflict => $"Conflicto:\n{error.Message}",
                _ => $"Error:\n{error.Message}"
            };

            if (error.ValidationErrors?.Any() == true)
            {
                message += "\n\nDetalles:\n" + string.Join("\n", error.ValidationErrors);
            }

            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
```

### 4.2 Dependency Injection en UI

```csharp
// UI.WinForms/Program.cs
namespace GymRoutineGenerator.UI.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var host = CreateHostBuilder().Build();

            ServiceProvider = host.Services;

            Application.Run(host.Services.GetRequiredService<MainForm>());
        }

        public static IServiceProvider ServiceProvider { get; private set; }

        static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Application Layer
                    services.AddApplication();

                    // Infrastructure Layer
                    services.AddInfrastructure(context.Configuration);

                    // UI Forms
                    services.AddTransient<MainForm>();
                    services.AddTransient<CreateRoutineForm>();
                    services.AddTransient<ExerciseManagerForm>();

                    // Singleton for current user (mock for now)
                    services.AddSingleton<ICurrentUserService, CurrentUserService>();
                });
    }
}

// Application/DependencyInjection.cs
namespace GymRoutineGenerator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            return services;
        }
    }
}

// Infrastructure.Persistence/DependencyInjection.cs
namespace GymRoutineGenerator.Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            var dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "gymroutine.db");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Repositories
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
            services.AddScoped<IRoutineRepository, RoutineRepository>();
            services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Domain Services
            services.AddScoped<IRoutineSafetyValidator, RoutineSafetyValidator>();

            // External Services
            services.AddScoped<IAIService, OllamaAIService>();
            services.AddScoped<IExportService, DocumentExportService>();

            return services;
        }
    }
}
```

---

## 5. Casos de Uso Completos

### Ejemplo completo: Generar rutina con IA

```csharp
// Application/UseCases/Routines/Commands/GenerateAIRoutine/GenerateAIRoutineCommand.cs
public record GenerateAIRoutineCommand : IRequest<Result<RoutineDto>>
{
    public string UserId { get; init; }
    public List<string> MuscleGroups { get; init; }
    public string FitnessLevel { get; init; }
    public int TrainingDaysPerWeek { get; init; }
    public List<string> AvailableEquipment { get; init; }
    public List<string> PhysicalLimitations { get; init; }
}

// Handler
public class GenerateAIRoutineHandler : IRequestHandler<GenerateAIRoutineCommand, Result<RoutineDto>>
{
    private readonly IAIService _aiService;
    private readonly IExerciseRepository _exerciseRepo;
    private readonly IUserProfileRepository _userProfileRepo;
    private readonly IWorkoutPlanRepository _workoutPlanRepo;
    private readonly IRoutineSafetyValidator _safetyValidator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateAIRoutineHandler> _logger;

    public async Task<Result<RoutineDto>> Handle(
        GenerateAIRoutineCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener perfil de usuario
        var userId = UserId.Create(request.UserId);
        var userProfile = await _userProfileRepo.GetByUserIdAsync(userId, cancellationToken);

        if (userProfile == null)
        {
            return Result<RoutineDto>.Failure(Error.NotFound("User profile not found"));
        }

        // 2. Obtener ejercicios disponibles
        var muscleGroups = request.MuscleGroups
            .Select(mg => MuscleGroup.Create(mg, MuscleGroupCategory.Upper))
            .ToList();

        var availableExercises = new List<Exercise>();
        foreach (var muscleGroup in muscleGroups)
        {
            var exercises = await _exerciseRepo.GetByMuscleGroupAsync(muscleGroup, cancellationToken);
            availableExercises.AddRange(exercises);
        }

        // 3. Llamar a IA para generar rutina
        var aiRequest = new AIRoutineRequest
        {
            UserProfile = userProfile,
            AvailableExercises = availableExercises,
            MuscleGroupsToTarget = muscleGroups,
            TrainingDaysPerWeek = request.TrainingDaysPerWeek
        };

        var aiResponse = await _aiService.GenerateRoutineAsync(aiRequest, cancellationToken);

        if (!aiResponse.IsSuccess)
        {
            return Result<RoutineDto>.Failure(
                Error.Unexpected($"AI service failed: {aiResponse.ErrorMessage}"));
        }

        // 4. Construir rutina desde respuesta de IA
        var routine = Routine.Create(
            aiResponse.RoutineName,
            aiResponse.DayOfWeek,
            Enum.Parse<DifficultyLevel>(request.FitnessLevel));

        foreach (var exerciseSelection in aiResponse.SelectedExercises)
        {
            var exercise = availableExercises.First(e => e.Id == exerciseSelection.ExerciseId);
            var restPeriod = RestPeriod.FromSeconds(exerciseSelection.RestSeconds);

            routine.AddExercise(
                exercise,
                exerciseSelection.Sets,
                exerciseSelection.Reps,
                restPeriod);
        }

        // 5. Validar seguridad
        var safetyResult = await _safetyValidator.ValidateAsync(routine, userProfile);
        if (!safetyResult.IsSafe)
        {
            _logger.LogWarning("AI-generated routine failed safety validation");
            return Result<RoutineDto>.Failure(
                Error.Validation("Generated routine has safety concerns",
                    safetyResult.Violations.Select(v => v.Message).ToList()));
        }

        // 6. Agregar a WorkoutPlan
        var workoutPlan = await _workoutPlanRepo.GetActiveByUserIdAsync(userId, cancellationToken)
            ?? WorkoutPlan.Create(userId, "Mi Plan de Entrenamiento",
                DateRange.FromNow(TimeSpan.FromDays(90)));

        workoutPlan.AddRoutine(routine);

        if (workoutPlan.Id == null)
        {
            await _workoutPlanRepo.AddAsync(workoutPlan, cancellationToken);
        }

        // 7. Guardar
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Retornar DTO
        var routineDto = _mapper.Map<RoutineDto>(routine);
        return Result<RoutineDto>.Success(routineDto);
    }
}
```

---

## Conclusión

Este documento proporciona ejemplos concretos de cómo implementar Clean Architecture con SOLID/GRASP en el proyecto GymRoutineGenerator.

**Ventajas de esta arquitectura**:
- ✅ Testabilidad máxima
- ✅ Bajo acoplamiento
- ✅ Alta cohesión
- ✅ Fácil mantenimiento
- ✅ Fácil evolución

**Próximos pasos**: Ver `ARQUITECTURA-MEJORAS-PROPUESTAS.md` para el plan de implementación.
