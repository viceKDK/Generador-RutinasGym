using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Infrastructure.AI;

public class RoutineStructureService : IRoutineStructureService
{
    private readonly IFallbackRoutineService _fallbackRoutineService;

    public RoutineStructureService(IFallbackRoutineService fallbackRoutineService)
    {
        _fallbackRoutineService = fallbackRoutineService;
    }

    public async Task<StructuredRoutine> CreateStructuredRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        var routine = new StructuredRoutine
        {
            RoutineName = GenerateRoutineName(parameters),
            UserParameters = parameters,
            EstimatedDuration = TimeSpan.FromMinutes(parameters.PreferredSessionDuration)
        };

        // Create warmup protocol
        routine.Warmup = await CreateWarmupProtocolAsync(parameters, cancellationToken);

        // Get recommended exercises
        var exercises = await _fallbackRoutineService.GetRecommendedExercisesAsync(parameters, cancellationToken);

        // Optimize exercise sequence
        var sequence = await OptimizeExerciseOrderAsync(exercises, parameters, cancellationToken);

        // Create structured training blocks
        routine.MainWorkout = CreateTrainingBlocks(sequence.OrderedExercises, parameters);

        // Create cooldown protocol
        routine.Cooldown = await CreateCooldownProtocolAsync(parameters, cancellationToken);

        // Calculate training volume
        routine.Volume = await CalculateOptimalVolumeAsync(parameters, cancellationToken);

        // Create progression plan
        routine.Progression = CreateProgressionPlan(parameters);

        // Add safety considerations
        routine.SafetyNotes = CreateSafetyConsiderations(parameters);

        return routine;
    }

    public async Task<List<WorkoutSession>> GenerateWeeklyProgramAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        var sessions = new List<WorkoutSession>();

        // Determine training split based on frequency
        var split = DetermineTrainingSplit(parameters);

        for (int day = 1; day <= parameters.TrainingDaysPerWeek; day++)
        {
            var session = await CreateWorkoutSessionAsync(day, split[day - 1], parameters, cancellationToken);
            sessions.Add(session);
        }

        return sessions;
    }

    public async Task<ExerciseSequence> OptimizeExerciseOrderAsync(List<Exercise> exercises, UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var sequence = new ExerciseSequence();
        var orderedExercises = new List<StructuredExercise>();

        // Sort exercises by optimal order
        var sortedExercises = exercises
            .OrderBy(e => GetExercisePriority(e, parameters))
            .ThenBy(e => e.DifficultyLevel)
            .ToList();

        // Convert to structured exercises with proper parameters
        for (int i = 0; i < sortedExercises.Count; i++)
        {
            var structuredExercise = new StructuredExercise
            {
                BaseExercise = sortedExercises[i],
                OrderInWorkout = i + 1,
                Parameters = CreateTrainingParameters(sortedExercises[i], parameters),
                RestPeriod = CreateRestPeriod(sortedExercises[i], parameters, i),
                TechniqueNotes = CreateTechniqueNotes(sortedExercises[i], parameters),
                Modifications = CreateModifications(sortedExercises[i], parameters),
                Progression = CreateProgressionScheme(sortedExercises[i], parameters)
            };

            orderedExercises.Add(structuredExercise);
        }

        sequence.OrderedExercises = orderedExercises;
        sequence.SequenceRationale = GenerateSequenceRationale(orderedExercises, parameters);
        sequence.OptimizationNotes = GenerateOptimizationNotes(orderedExercises, parameters);
        sequence.Transitions = CreateExerciseTransitions(orderedExercises);

        return sequence;
    }

    public async Task<TrainingVolume> CalculateOptimalVolumeAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var volume = new TrainingVolume();

        // Base volume calculations
        var baseVolume = CalculateBaseVolume(parameters);
        volume.TotalSets = baseVolume.TotalSets;
        volume.TotalReps = baseVolume.EstimatedReps;

        // Calculate time components
        volume.TotalWorkTime = CalculateWorkTime(parameters);
        volume.TotalRestTime = CalculateRestTime(parameters);

        // Sets per muscle group distribution
        volume.SetsPerMuscleGroup = DistributeSetsPerMuscleGroup(parameters);

        // Volume classification
        volume.Classification = ClassifyVolume(volume.TotalSets, parameters);

        // Volume recommendations
        volume.VolumeRecommendations = GenerateVolumeRecommendations(volume, parameters);

        return volume;
    }

    // Private helper methods
    private string GenerateRoutineName(UserRoutineParameters parameters)
    {
        var goalPrefix = parameters.PrimaryGoal switch
        {
            "Fuerza" => "Rutina de Fuerza",
            "Masa" => "Rutina de Hipertrofia",
            "Resistencia" => "Rutina de Resistencia",
            "Pérdida de peso" => "Rutina de Pérdida de Peso",
            _ => "Rutina de Fitness"
        };

        var frequencySuffix = parameters.TrainingDaysPerWeek switch
        {
            1 => "Básica",
            2 => "Básica",
            3 => "Intermedia",
            4 => "Avanzada",
            5 => "Intensiva",
            6 => "Intensiva",
            _ => "Personalizada"
        };

        return $"{goalPrefix} {frequencySuffix} - {parameters.Name}";
    }

    private async Task<WarmupProtocol> CreateWarmupProtocolAsync(UserRoutineParameters parameters, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var warmupDuration = CalculateWarmupDuration(parameters);

        var protocol = new WarmupProtocol
        {
            Duration = warmupDuration,
            Purpose = "Preparar el cuerpo para el ejercicio, activar músculos y aumentar temperatura corporal"
        };

        // Phase 1: General Activation
        var generalPhase = new WarmupPhase
        {
            PhaseName = "Activación General",
            Duration = TimeSpan.FromMinutes(warmupDuration.TotalMinutes * 0.4),
            Instructions = "Movimientos suaves para aumentar la frecuencia cardíaca gradualmente",
            TargetIntensity = IntensityLevel.Light,
            Exercises = CreateGeneralWarmupExercises(parameters)
        };

        // Phase 2: Dynamic Preparation
        var dynamicPhase = new WarmupPhase
        {
            PhaseName = "Preparación Dinámica",
            Duration = TimeSpan.FromMinutes(warmupDuration.TotalMinutes * 0.4),
            Instructions = "Movimientos dinámicos específicos para los músculos que se trabajarán",
            TargetIntensity = IntensityLevel.Moderate,
            Exercises = CreateDynamicWarmupExercises(parameters)
        };

        // Phase 3: Activation
        var activationPhase = new WarmupPhase
        {
            PhaseName = "Activación Específica",
            Duration = TimeSpan.FromMinutes(warmupDuration.TotalMinutes * 0.2),
            Instructions = "Ejercicios ligeros similares a los del entrenamiento principal",
            TargetIntensity = IntensityLevel.Moderate,
            Exercises = CreateActivationExercises(parameters)
        };

        protocol.Phases = new List<WarmupPhase> { generalPhase, dynamicPhase, activationPhase };

        // Special considerations based on user parameters
        if (parameters.Age >= 50)
        {
            protocol.SpecialConsiderations.Add("Calentamiento extendido debido a la edad - tómate más tiempo");
        }

        if (parameters.PhysicalLimitations.Any(l => l.Contains("artritis", StringComparison.OrdinalIgnoreCase)))
        {
            protocol.SpecialConsiderations.Add("Movimientos suaves y controlados para las articulaciones afectadas");
        }

        if (parameters.RecommendedIntensity <= 2)
        {
            protocol.SpecialConsiderations.Add("Calentamiento muy gradual - escucha a tu cuerpo");
        }

        return protocol;
    }

    private async Task<CooldownProtocol> CreateCooldownProtocolAsync(UserRoutineParameters parameters, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var cooldownDuration = CalculateCooldownDuration(parameters);

        var protocol = new CooldownProtocol
        {
            Duration = cooldownDuration,
            Purpose = "Reducir gradualmente la frecuencia cardíaca y mejorar la recuperación"
        };

        // Phase 1: Active Recovery
        var activePhase = new CooldownPhase
        {
            PhaseName = "Recuperación Activa",
            Duration = TimeSpan.FromMinutes(cooldownDuration.TotalMinutes * 0.3),
            Instructions = "Movimientos suaves para reducir la frecuencia cardíaca gradualmente",
            Exercises = CreateActiveRecoveryExercises(parameters)
        };

        // Phase 2: Static Stretching
        var stretchingPhase = new CooldownPhase
        {
            PhaseName = "Estiramientos Estáticos",
            Duration = TimeSpan.FromMinutes(cooldownDuration.TotalMinutes * 0.5),
            Instructions = "Mantén cada estiramiento por 20-30 segundos",
            IsStretching = true,
            Exercises = CreateStretchingExercises(parameters)
        };

        // Phase 3: Relaxation
        var relaxationPhase = new CooldownPhase
        {
            PhaseName = "Relajación",
            Duration = TimeSpan.FromMinutes(cooldownDuration.TotalMinutes * 0.2),
            Instructions = "Respiración profunda y relajación mental",
            Exercises = CreateRelaxationExercises(parameters)
        };

        protocol.Phases = new List<CooldownPhase> { activePhase, stretchingPhase, relaxationPhase };

        // Recovery tips
        protocol.RecoveryTips = new List<string>
        {
            "Hidrátate adecuadamente después del entrenamiento",
            "Considera una ducha con agua tibia para relajar los músculos",
            "Mantén una alimentación balanceada para la recuperación",
            "Asegúrate de dormir 7-8 horas para una recuperación óptima"
        };

        if (parameters.Age >= 50)
        {
            protocol.RecoveryTips.Add("Considera técnicas adicionales de recuperación como masajes suaves");
        }

        return protocol;
    }

    private List<TrainingBlock> CreateTrainingBlocks(List<StructuredExercise> exercises, UserRoutineParameters parameters)
    {
        var blocks = new List<TrainingBlock>();

        // Group exercises by category
        var compoundExercises = exercises.Where(e => e.BaseExercise.ExerciseType == ExerciseType.Compound).ToList();
        var isolationExercises = exercises.Where(e => e.BaseExercise.ExerciseType == ExerciseType.Isolation).ToList();
        var coreExercises = exercises.Where(e => e.BaseExercise.ExerciseType == ExerciseType.Core).ToList();

        int blockOrder = 1;

        // Compound Block (highest priority)
        if (compoundExercises.Any())
        {
            blocks.Add(new TrainingBlock
            {
                BlockName = "Ejercicios Compuestos",
                Category = ExerciseCategory.Compound,
                Exercises = compoundExercises,
                Purpose = "Trabajar múltiples grupos musculares simultáneamente con ejercicios fundamentales",
                EstimatedDuration = CalculateBlockDuration(compoundExercises),
                OrderInWorkout = blockOrder++
            });
        }

        // Isolation Block
        if (isolationExercises.Any())
        {
            blocks.Add(new TrainingBlock
            {
                BlockName = "Ejercicios de Aislamiento",
                Category = ExerciseCategory.Isolation,
                Exercises = isolationExercises,
                Purpose = "Enfocar músculos específicos para desarrollo detallado",
                EstimatedDuration = CalculateBlockDuration(isolationExercises),
                OrderInWorkout = blockOrder++
            });
        }

        // Core Block
        if (coreExercises.Any())
        {
            blocks.Add(new TrainingBlock
            {
                BlockName = "Fortalecimiento del Core",
                Category = ExerciseCategory.Core,
                Exercises = coreExercises,
                Purpose = "Desarrollar estabilidad y fuerza del núcleo corporal",
                EstimatedDuration = CalculateBlockDuration(coreExercises),
                OrderInWorkout = blockOrder++
            });
        }

        return blocks;
    }

    private TrainingParameters CreateTrainingParameters(Exercise exercise, UserRoutineParameters parameters)
    {
        var trainingParams = new TrainingParameters
        {
            Sets = exercise.RecommendedSets,
            Reps = CreateRepRange(exercise, parameters),
            Intensity = CreateIntensityRange(exercise, parameters),
            Tempo = CreateTempoPrescription(exercise, parameters),
            LoadPercentage = CalculateLoadPercentage(exercise, parameters)
        };

        // Advanced techniques based on experience level
        if (parameters.ExperienceLevel == "Avanzado" && exercise.ExerciseType == ExerciseType.Isolation)
        {
            trainingParams.IsDropSet = Random.Shared.Next(0, 4) == 0; // 25% chance
        }

        return trainingParams;
    }

    private RepRange CreateRepRange(Exercise exercise, UserRoutineParameters parameters)
    {
        var baseRange = GetBaseRepRange(parameters.PrimaryGoal, exercise.ExerciseType);

        var repRange = new RepRange
        {
            Minimum = baseRange.min,
            Maximum = baseRange.max,
            Target = (baseRange.min + baseRange.max) / 2
        };

        // Adjust for age and limitations
        if (parameters.Age >= 65 || parameters.RecommendedIntensity <= 2)
        {
            repRange.Minimum = Math.Max(repRange.Minimum + 2, 8);
            repRange.Maximum = Math.Max(repRange.Maximum + 4, 15);
            repRange.Target = (repRange.Minimum + repRange.Maximum) / 2;
        }

        // Time-based exercises (planks, holds)
        if (exercise.Name.Contains("plancha", StringComparison.OrdinalIgnoreCase) ||
            exercise.Name.Contains("hold", StringComparison.OrdinalIgnoreCase))
        {
            repRange.IsTimeBasedReps = true;
            repRange.TimeInSeconds = parameters.ExperienceLevel switch
            {
                "Principiante" => 15,
                "Intermedio" => 30,
                "Avanzado" => 45,
                _ => 30
            };
        }

        return repRange;
    }

    private IntensityRange CreateIntensityRange(Exercise exercise, UserRoutineParameters parameters)
    {
        var baseRPE = parameters.RecommendedIntensity * 2; // Convert 1-5 scale to RPE

        return new IntensityRange
        {
            RPEMinimum = Math.Max(baseRPE - 1, 4),
            RPEMaximum = Math.Min(baseRPE + 1, 9),
            RPETarget = baseRPE,
            PercentageOfMax = ConvertRPEToPercentage(baseRPE),
            IntensityDescription = GetIntensityDescription(baseRPE)
        };
    }

    private TempoPrescription CreateTempoPrescription(Exercise exercise, UserRoutineParameters parameters)
    {
        // Default controlled tempo
        var tempo = new TempoPrescription
        {
            EccentricSeconds = 2,
            PauseSeconds = 0,
            ConcentricSeconds = 1,
            TopPauseSeconds = 0,
            IsControlledTempo = true
        };

        // Adjust for beginners (slower, more controlled)
        if (parameters.ExperienceLevel == "Principiante")
        {
            tempo.EccentricSeconds = 3;
            tempo.PauseSeconds = 1;
        }

        // Adjust for power/strength goals
        if (parameters.PrimaryGoal == "Fuerza")
        {
            tempo.ConcentricSeconds = 0; // Explosive
            tempo.EccentricSeconds = 3; // Controlled lowering
        }

        return tempo;
    }

    private RestPeriod CreateRestPeriod(Exercise exercise, UserRoutineParameters parameters, int exerciseIndex)
    {
        var baseRest = GetBaseRestTime(parameters.PrimaryGoal, exercise.ExerciseType);

        var restPeriod = new RestPeriod
        {
            TargetRest = baseRest,
            MinimumRest = TimeSpan.FromSeconds(baseRest.TotalSeconds * 0.8),
            MaximumRest = TimeSpan.FromSeconds(baseRest.TotalSeconds * 1.5),
            Type = RestType.Complete
        };

        // Adjust for age and fitness level
        if (parameters.Age >= 50 || parameters.RecommendedIntensity <= 2)
        {
            restPeriod.TargetRest = TimeSpan.FromSeconds(restPeriod.TargetRest.TotalSeconds * 1.3);
            restPeriod.ActivityDuringRest = "Caminar lentamente o estiramientos suaves";
            restPeriod.IsActiveRest = true;
        }

        // First exercise needs less rest (just finished warmup)
        if (exerciseIndex == 0)
        {
            restPeriod.TargetRest = TimeSpan.FromSeconds(Math.Max(30, restPeriod.TargetRest.TotalSeconds * 0.5));
        }

        return restPeriod;
    }

    private List<string> CreateTechniqueNotes(Exercise exercise, UserRoutineParameters parameters)
    {
        var notes = new List<string>(exercise.SafetyNotes);

        // Add experience-level specific notes
        if (parameters.ExperienceLevel == "Principiante")
        {
            notes.Insert(0, "Enfócate en la técnica correcta antes que en el peso o velocidad");
        }

        // Add age-specific notes
        if (parameters.Age >= 60)
        {
            notes.Add("Realiza movimientos lentos y controlados");
            notes.Add("Escucha a tu cuerpo y detente si sientes molestias");
        }

        // Add limitation-specific notes
        foreach (var limitation in parameters.PhysicalLimitations)
        {
            if (limitation.Contains("espalda", StringComparison.OrdinalIgnoreCase))
            {
                notes.Add("Mantén la espalda neutral en todo momento");
            }
            if (limitation.Contains("rodilla", StringComparison.OrdinalIgnoreCase))
            {
                notes.Add("Evita flexiones profundas de rodilla");
            }
        }

        return notes.Distinct().ToList();
    }

    private List<ExerciseModification> CreateModifications(Exercise exercise, UserRoutineParameters parameters)
    {
        var modifications = new List<ExerciseModification>();

        // Add existing modifications
        foreach (var modification in exercise.Modifications)
        {
            modifications.Add(new ExerciseModification
            {
                ModificationName = modification,
                Description = modification,
                Reason = ModificationReason.SkillLevel,
                Instructions = $"Usa {modification.ToLower()} si la versión estándar es muy difícil"
            });
        }

        // Add parameter-specific modifications
        if (parameters.RecommendedIntensity <= 2)
        {
            modifications.Add(new ExerciseModification
            {
                ModificationName = "Versión asistida",
                Description = "Reduce la dificultad usando apoyo o asistencia",
                Reason = ModificationReason.PhysicalLimitation,
                Instructions = "Usa una silla, pared o banda elástica para asistencia"
            });
        }

        if (parameters.Age >= 65)
        {
            modifications.Add(new ExerciseModification
            {
                ModificationName = "Versión sentada",
                Description = "Realiza el ejercicio desde una posición sentada cuando sea posible",
                Reason = ModificationReason.Safety,
                Instructions = "Usa una silla estable con respaldo para mayor seguridad"
            });
        }

        return modifications;
    }

    private ProgressionScheme CreateProgressionScheme(Exercise exercise, UserRoutineParameters parameters)
    {
        var progression = new ProgressionScheme
        {
            Type = ProgressionType.VolumeIncrease,
            WeeksUntilProgresssion = parameters.ExperienceLevel switch
            {
                "Principiante" => 2,
                "Intermedio" => 1,
                "Avanzado" => 1,
                _ => 1
            }
        };

        progression.ProgressionValues = new Dictionary<string, object>
        {
            ["RepsIncrease"] = parameters.ExperienceLevel == "Principiante" ? 1 : 2,
            ["SetsIncrease"] = 0, // Increase reps first, then sets
            ["IntensityIncrease"] = 0.1 // 10% intensity increase when ready
        };

        progression.ProgressionInstructions = parameters.ExperienceLevel switch
        {
            "Principiante" => "Aumenta 1 repetición cada 2 semanas. Enfócate en la técnica correcta.",
            "Intermedio" => "Aumenta 2 repeticiones cada semana. Cuando llegues al rango máximo, añade peso o dificultad.",
            "Avanzado" => "Progresión automática basada en RPE. Cuando puedas hacer 2 reps extra con buena técnica, aumenta la carga.",
            _ => "Progresión gradual basada en capacidad individual"
        };

        return progression;
    }

    private string GenerateSequenceRationale(List<StructuredExercise> exercises, UserRoutineParameters parameters)
    {
        var rationale = "Secuencia optimizada siguiendo principios científicos:\n";
        rationale += "1. Ejercicios compuestos primero para máxima energía\n";
        rationale += "2. Ejercicios de aislamiento después para trabajo específico\n";
        rationale += "3. Core al final para no comprometer estabilidad\n";
        rationale += "4. Orden de dificultad decreciente para mantener calidad técnica";

        return rationale;
    }

    private List<string> GenerateOptimizationNotes(List<StructuredExercise> exercises, UserRoutineParameters parameters)
    {
        var notes = new List<string>();

        notes.Add($"Rutina optimizada para {parameters.PrimaryGoal.ToLower()} en {parameters.PreferredSessionDuration} minutos");
        notes.Add($"Intensidad ajustada al nivel {parameters.RecommendedIntensity}/5 según perfil del usuario");

        if (parameters.PhysicalLimitations.Any())
        {
            notes.Add("Ejercicios modificados para respetar limitaciones físicas mencionadas");
        }

        if (parameters.Age >= 60)
        {
            notes.Add("Secuencia adaptada para adultos mayores con énfasis en seguridad");
        }

        return notes;
    }

    private List<ExerciseTransition> CreateExerciseTransitions(List<StructuredExercise> exercises)
    {
        var transitions = new List<ExerciseTransition>();

        for (int i = 0; i < exercises.Count - 1; i++)
        {
            var currentExercise = exercises[i];
            var nextExercise = exercises[i + 1];

            var transition = new ExerciseTransition
            {
                FromExerciseIndex = i,
                ToExerciseIndex = i + 1,
                TransitionTime = TimeSpan.FromSeconds(30), // Default transition time
                RequiresEquipmentChange = !currentExercise.BaseExercise.Equipment.Equals(nextExercise.BaseExercise.Equipment),
                TransitionActivity = "Preparar posición para el siguiente ejercicio"
            };

            // Adjust transition time based on equipment change
            if (transition.RequiresEquipmentChange)
            {
                transition.TransitionTime = TimeSpan.FromSeconds(60);
                transition.TransitionActivity = "Cambiar equipamiento y preparar posición";
            }

            // Add active rest if needed
            if (currentExercise.BaseExercise.ExerciseType == ExerciseType.Compound &&
                nextExercise.BaseExercise.ExerciseType == ExerciseType.Compound)
            {
                transition.TransitionTime = TimeSpan.FromSeconds(90);
                transition.TransitionActivity = "Descanso activo - caminar o estiramientos ligeros";
            }

            transitions.Add(transition);
        }

        return transitions;
    }

    private ProgressionPlan CreateProgressionPlan(UserRoutineParameters parameters)
    {
        var plan = new ProgressionPlan
        {
            Strategy = ProgressionStrategy.Linear // Start with linear progression
        };

        // Create weekly progression
        for (int week = 1; week <= 8; week++)
        {
            var progressionWeek = new ProgressionWeek
            {
                WeekNumber = week,
                Focus = GetWeeklyFocus(week, parameters),
                ParameterAdjustments = GetWeeklyAdjustments(week, parameters),
                ExpectedAdaptations = GetExpectedAdaptations(week, parameters)
            };

            plan.Weeks.Add(progressionWeek);
        }

        // Add progression milestones
        plan.Milestones = CreateProgressionMilestones(parameters);

        // Add progression notes
        plan.ProgressionNotes = new List<string>
        {
            "La progresión debe ser gradual y constante",
            "Escucha a tu cuerpo y ajusta según sea necesario",
            "Registra tus entrenamientos para seguir el progreso",
            "Consulta con un profesional si experimentas dolor"
        };

        return plan;
    }

    private List<SafetyConsideration> CreateSafetyConsiderations(UserRoutineParameters parameters)
    {
        var considerations = new List<SafetyConsideration>();

        // General safety
        considerations.Add(new SafetyConsideration
        {
            Consideration = "Calentamiento obligatorio antes de cada sesión",
            Severity = SafetyLevel.Warning,
            Precautions = new List<string> { "Nunca saltes el calentamiento", "Aumenta gradualmente la intensidad" },
            WarningSignsToStop = new List<string> { "Dolor agudo", "Mareos", "Dificultad para respirar" }
        });

        // Age-specific considerations
        if (parameters.Age >= 65)
        {
            considerations.Add(new SafetyConsideration
            {
                Consideration = "Precauciones especiales para adultos mayores",
                Severity = SafetyLevel.Warning,
                Precautions = new List<string>
                {
                    "Movimientos lentos y controlados",
                    "Evitar cambios bruscos de posición",
                    "Tener apoyo cerca durante ejercicios de equilibrio"
                },
                WarningSignsToStop = new List<string>
                {
                    "Mareos al cambiar de posición",
                    "Dolor en articulaciones",
                    "Fatiga excesiva"
                }
            });
        }

        // Limitation-specific considerations
        foreach (var limitation in parameters.PhysicalLimitations)
        {
            if (limitation.Contains("cardiovascular", StringComparison.OrdinalIgnoreCase))
            {
                considerations.Add(new SafetyConsideration
                {
                    Consideration = "Monitoreo cardiovascular durante el ejercicio",
                    Severity = SafetyLevel.Critical,
                    Precautions = new List<string>
                    {
                        "Monitorear frecuencia cardíaca",
                        "Comenzar con intensidad muy baja",
                        "Tener medicamentos a mano si es necesario"
                    },
                    WarningSignsToStop = new List<string>
                    {
                        "Dolor en el pecho",
                        "Dificultad para respirar",
                        "Mareos severos",
                        "Frecuencia cardíaca anormalmente alta"
                    }
                });
            }
        }

        return considerations;
    }

    // Helper methods for calculations
    private int GetExercisePriority(Exercise exercise, UserRoutineParameters parameters)
    {
        // Priority order: Compound -> Isolation -> Core -> Cardio
        return exercise.ExerciseType switch
        {
            ExerciseType.Compound => 1,
            ExerciseType.Isolation => 2,
            ExerciseType.Core => 3,
            ExerciseType.Cardio => 4,
            _ => 5
        };
    }

    private (int min, int max) GetBaseRepRange(string goal, ExerciseType type)
    {
        return goal switch
        {
            "Fuerza" => type == ExerciseType.Compound ? (3, 6) : (6, 8),
            "Masa" => type == ExerciseType.Compound ? (6, 10) : (8, 12),
            "Resistencia" => (12, 20),
            "Pérdida de peso" => (10, 15),
            _ => (8, 12)
        };
    }

    private TimeSpan GetBaseRestTime(string goal, ExerciseType type)
    {
        var baseSeconds = goal switch
        {
            "Fuerza" => type == ExerciseType.Compound ? 180 : 120,
            "Masa" => type == ExerciseType.Compound ? 90 : 60,
            "Resistencia" => 45,
            "Pérdida de peso" => 60,
            _ => 75
        };

        return TimeSpan.FromSeconds(baseSeconds);
    }

    private double ConvertRPEToPercentage(int rpe)
    {
        return rpe switch
        {
            1 => 0.5,
            2 => 0.5,
            3 => 0.5,
            4 => 0.5,
            5 => 0.6,
            6 => 0.7,
            7 => 0.8,
            8 => 0.9,
            9 => 0.95,
            10 => 0.95,
            _ => 0.7
        };
    }

    private string GetIntensityDescription(int rpe)
    {
        return rpe switch
        {
            1 => "Muy ligero - puedes conversar fácilmente",
            2 => "Muy ligero - puedes conversar fácilmente",
            3 => "Muy ligero - puedes conversar fácilmente",
            4 => "Muy ligero - puedes conversar fácilmente",
            5 => "Ligero - conversación cómoda",
            6 => "Moderado - conversación posible con esfuerzo",
            7 => "Vigoroso - conversación difícil",
            8 => "Muy vigoroso - muy poca conversación",
            9 => "Máximo esfuerzo - sin conversación",
            10 => "Máximo esfuerzo - sin conversación",
            _ => "Moderado"
        };
    }

    private TimeSpan CalculateWarmupDuration(UserRoutineParameters parameters)
    {
        var baseDuration = parameters.PreferredSessionDuration * 0.15; // 15% of session
        var minDuration = Math.Max(baseDuration, 5); // Minimum 5 minutes
        var maxDuration = Math.Min(baseDuration, 15); // Maximum 15 minutes

        // Adjust for age
        if (parameters.Age >= 50)
        {
            maxDuration += 3; // Extra 3 minutes for older adults
        }

        return TimeSpan.FromMinutes(maxDuration);
    }

    private TimeSpan CalculateCooldownDuration(UserRoutineParameters parameters)
    {
        var baseDuration = parameters.PreferredSessionDuration * 0.12; // 12% of session
        var duration = Math.Max(baseDuration, 5); // Minimum 5 minutes

        // Adjust for age and limitations
        if (parameters.Age >= 50 || parameters.PhysicalLimitations.Any())
        {
            duration += 2; // Extra time for recovery
        }

        return TimeSpan.FromMinutes(duration);
    }

    private List<Exercise> CreateGeneralWarmupExercises(UserRoutineParameters parameters)
    {
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Marcha en el lugar", Description = "Caminar en el lugar aumentando gradualmente el ritmo" },
            new Exercise { Name = "Círculos de brazos", Description = "Movimientos circulares con los brazos hacia adelante y atrás" }
        };

        if (parameters.Age < 60)
        {
            exercises.Add(new Exercise { Name = "Jumping jacks suaves", Description = "Movimientos de apertura y cierre de brazos y piernas" });
        }

        return exercises;
    }

    private List<Exercise> CreateDynamicWarmupExercises(UserRoutineParameters parameters)
    {
        return new List<Exercise>
        {
            new Exercise { Name = "Rotaciones de cadera", Description = "Movimientos circulares de cadera en ambas direcciones" },
            new Exercise { Name = "Elevaciones de rodillas", Description = "Elevar rodillas alternadamente hacia el pecho" },
            new Exercise { Name = "Estiramientos dinámicos de piernas", Description = "Balanceo suave de piernas adelante y atrás" }
        };
    }

    private List<Exercise> CreateActivationExercises(UserRoutineParameters parameters)
    {
        return new List<Exercise>
        {
            new Exercise { Name = "Sentadillas lentas", Description = "Sentadillas con peso corporal a ritmo controlado" },
            new Exercise { Name = "Flexiones de pared", Description = "Flexiones ligeras contra la pared" }
        };
    }

    private List<Exercise> CreateActiveRecoveryExercises(UserRoutineParameters parameters)
    {
        return new List<Exercise>
        {
            new Exercise { Name = "Caminata lenta", Description = "Caminar lentamente por 2-3 minutos" },
            new Exercise { Name = "Movimientos suaves de brazos", Description = "Balanceo ligero de brazos para relajar" }
        };
    }

    private List<Exercise> CreateStretchingExercises(UserRoutineParameters parameters)
    {
        return new List<Exercise>
        {
            new Exercise { Name = "Estiramiento de cuádriceps", Description = "Mantener 20-30 segundos cada pierna" },
            new Exercise { Name = "Estiramiento de isquiotibiales", Description = "Estiramiento sentado o de pie, 20-30 segundos" },
            new Exercise { Name = "Estiramiento de pecho", Description = "Brazos contra la pared, mantener 30 segundos" },
            new Exercise { Name = "Estiramiento de espalda", Description = "Flexión suave hacia adelante, mantener 30 segundos" }
        };
    }

    private List<Exercise> CreateRelaxationExercises(UserRoutineParameters parameters)
    {
        return new List<Exercise>
        {
            new Exercise { Name = "Respiración profunda", Description = "5 respiraciones lentas y profundas" },
            new Exercise { Name = "Relajación progresiva", Description = "Tensar y relajar grupos musculares progresivamente" }
        };
    }

    private List<string> DetermineTrainingSplit(UserRoutineParameters parameters)
    {
        return parameters.TrainingDaysPerWeek switch
        {
            2 => new List<string> { "Cuerpo completo A", "Cuerpo completo B" },
            3 => new List<string> { "Tren superior", "Tren inferior", "Cuerpo completo" },
            4 => new List<string> { "Pecho y tríceps", "Espalda y bíceps", "Piernas y glúteos", "Hombros y core" },
            5 => new List<string> { "Pecho", "Espalda", "Piernas", "Hombros", "Brazos y core" },
            _ => new List<string> { "Cuerpo completo" }
        };
    }

    private async Task<WorkoutSession> CreateWorkoutSessionAsync(int dayNumber, string sessionFocus, UserRoutineParameters parameters, CancellationToken cancellationToken)
    {
        var session = new WorkoutSession
        {
            DayNumber = dayNumber,
            SessionName = $"Día {dayNumber}: {sessionFocus}",
            TargetMuscleGroups = GetTargetMuscleGroups(sessionFocus),
            EstimatedDuration = TimeSpan.FromMinutes(parameters.PreferredSessionDuration),
            TargetIntensity = (IntensityLevel)parameters.RecommendedIntensity
        };

        // Create session-specific warmup and cooldown
        session.Warmup = await CreateWarmupProtocolAsync(parameters, cancellationToken);
        session.Cooldown = await CreateCooldownProtocolAsync(parameters, cancellationToken);

        // Get exercises for this session focus
        var sessionExercises = await GetSessionSpecificExercisesAsync(sessionFocus, parameters, cancellationToken);
        session.Exercises = sessionExercises;

        // Add session-specific notes
        session.SpecialNotes = GetSessionNotes(sessionFocus, parameters);

        return session;
    }

    private List<string> GetTargetMuscleGroups(string sessionFocus)
    {
        return sessionFocus.ToLower() switch
        {
            var s when s.Contains("pecho") => new List<string> { "Pecho", "Tríceps", "Hombros anterior" },
            var s when s.Contains("espalda") => new List<string> { "Espalda", "Bíceps", "Hombros posterior" },
            var s when s.Contains("piernas") => new List<string> { "Cuádriceps", "Isquiotibiales", "Glúteos", "Pantorrillas" },
            var s when s.Contains("hombros") => new List<string> { "Hombros", "Trapecios", "Core" },
            var s when s.Contains("superior") => new List<string> { "Pecho", "Espalda", "Brazos", "Hombros" },
            var s when s.Contains("inferior") => new List<string> { "Cuádriceps", "Isquiotibiales", "Glúteos" },
            _ => new List<string> { "Cuerpo completo" }
        };
    }

    private async Task<List<StructuredExercise>> GetSessionSpecificExercisesAsync(string sessionFocus, UserRoutineParameters parameters, CancellationToken cancellationToken)
    {
        // This would typically filter exercises based on session focus
        // For now, return a basic structure
        var allExercises = await _fallbackRoutineService.GetRecommendedExercisesAsync(parameters, cancellationToken);
        var structuredExercises = new List<StructuredExercise>();

        for (int i = 0; i < Math.Min(allExercises.Count, 4); i++)
        {
            structuredExercises.Add(new StructuredExercise
            {
                BaseExercise = allExercises[i],
                OrderInWorkout = i + 1,
                Parameters = CreateTrainingParameters(allExercises[i], parameters),
                RestPeriod = CreateRestPeriod(allExercises[i], parameters, i)
            });
        }

        return structuredExercises;
    }

    private List<string> GetSessionNotes(string sessionFocus, UserRoutineParameters parameters)
    {
        var notes = new List<string>();

        if (sessionFocus.Contains("superior"))
        {
            notes.Add("Enfócate en mantener buena postura durante ejercicios de tren superior");
        }
        else if (sessionFocus.Contains("inferior"))
        {
            notes.Add("Asegúrate de activar bien los glúteos en ejercicios de piernas");
        }

        if (parameters.Age >= 50)
        {
            notes.Add("Tómate tiempo extra entre ejercicios si lo necesitas");
        }

        return notes;
    }

    private (int TotalSets, int EstimatedReps) CalculateBaseVolume(UserRoutineParameters parameters)
    {
        var setsPerSession = parameters.ExperienceLevel switch
        {
            "Principiante" => 8,
            "Intermedio" => 12,
            "Avanzado" => 16,
            _ => 10
        };

        var avgRepsPerSet = parameters.PrimaryGoal switch
        {
            "Fuerza" => 5,
            "Masa" => 10,
            "Resistencia" => 15,
            _ => 10
        };

        return (setsPerSession, setsPerSession * avgRepsPerSet);
    }

    private TimeSpan CalculateWorkTime(UserRoutineParameters parameters)
    {
        var sessionDuration = parameters.PreferredSessionDuration;
        var workPercentage = 0.6; // 60% work, 40% rest/transitions
        return TimeSpan.FromMinutes(sessionDuration * workPercentage);
    }

    private TimeSpan CalculateRestTime(UserRoutineParameters parameters)
    {
        var sessionDuration = parameters.PreferredSessionDuration;
        var restPercentage = 0.4; // 40% rest/transitions
        return TimeSpan.FromMinutes(sessionDuration * restPercentage);
    }

    private Dictionary<string, int> DistributeSetsPerMuscleGroup(UserRoutineParameters parameters)
    {
        var distribution = new Dictionary<string, int>();
        var totalSets = CalculateBaseVolume(parameters).TotalSets;

        // Distribute based on priorities
        var priorities = parameters.MuscleGroupPreferences.OrderBy(mg => mg.Priority).ToList();

        foreach (var priority in priorities)
        {
            var sets = priority.EmphasisLevel switch
            {
                "Alto" => (int)(totalSets * 0.3),
                "Medio" => (int)(totalSets * 0.2),
                "Bajo" => (int)(totalSets * 0.1),
                _ => (int)(totalSets * 0.15)
            };

            distribution[priority.MuscleGroup] = Math.Max(sets, 2);
        }

        return distribution;
    }

    private VolumeClassification ClassifyVolume(int totalSets, UserRoutineParameters parameters)
    {
        var baselineVolume = parameters.ExperienceLevel switch
        {
            "Principiante" => 8,
            "Intermedio" => 12,
            "Avanzado" => 16,
            _ => 10
        };

        return totalSets switch
        {
            var v when v < baselineVolume * 0.7 => VolumeClassification.Low,
            var v when v <= baselineVolume * 1.2 => VolumeClassification.Moderate,
            var v when v <= baselineVolume * 1.5 => VolumeClassification.High,
            _ => VolumeClassification.VeryHigh
        };
    }

    private List<string> GenerateVolumeRecommendations(TrainingVolume volume, UserRoutineParameters parameters)
    {
        var recommendations = new List<string>();

        recommendations.Add($"Volumen total: {volume.Classification}");

        if (volume.Classification == VolumeClassification.Low)
        {
            recommendations.Add("Considera añadir 1-2 ejercicios más si te sientes capaz");
        }
        else if (volume.Classification == VolumeClassification.VeryHigh)
        {
            recommendations.Add("Volumen alto - asegúrate de recuperarte bien entre sesiones");
        }

        if (parameters.Age >= 50)
        {
            recommendations.Add("Prioriza calidad sobre cantidad a tu edad");
        }

        return recommendations;
    }

    private TimeSpan CalculateBlockDuration(List<StructuredExercise> exercises)
    {
        var totalTime = TimeSpan.Zero;

        foreach (var exercise in exercises)
        {
            // Estimate time per set (including reps and rest)
            var setsTime = TimeSpan.FromSeconds(exercise.Parameters.Sets * 30); // 30 seconds per set average
            var restTime = TimeSpan.FromSeconds(exercise.RestPeriod.TargetRest.TotalSeconds * (exercise.Parameters.Sets - 1));
            totalTime = totalTime.Add(setsTime).Add(restTime);
        }

        return totalTime;
    }

    private double CalculateLoadPercentage(Exercise exercise, UserRoutineParameters parameters)
    {
        // Estimate load percentage based on goal and experience
        return parameters.PrimaryGoal switch
        {
            "Fuerza" => 0.85,
            "Masa" => 0.75,
            "Resistencia" => 0.60,
            _ => 0.70
        };
    }

    private string GetWeeklyFocus(int week, UserRoutineParameters parameters)
    {
        return week switch
        {
            1 => "Adaptación y técnica",
            2 => "Consolidación de patrones",
            3 => "Aumento gradual de intensidad",
            4 => "Primera progresión",
            5 => "Refinamiento técnico",
            6 => "Aumento de volumen",
            7 => "Progresión avanzada",
            8 => "Evaluación y siguiente fase",
            _ => "Progresión continua"
        };
    }

    private Dictionary<string, object> GetWeeklyAdjustments(int week, UserRoutineParameters parameters)
    {
        var adjustments = new Dictionary<string, object>();

        if (week <= 2)
        {
            adjustments["focus"] = "técnica";
            adjustments["intensity"] = "mantener";
        }
        else if (week <= 4)
        {
            adjustments["focus"] = "progresión gradual";
            adjustments["reps"] = "+1";
        }
        else if (week <= 6)
        {
            adjustments["focus"] = "consolidación";
            adjustments["sets"] = "+1 para ejercicios clave";
        }
        else
        {
            adjustments["focus"] = "refinamiento";
            adjustments["intensity"] = "+5%";
        }

        return adjustments;
    }

    private List<string> GetExpectedAdaptations(int week, UserRoutineParameters parameters)
    {
        return week switch
        {
            1 => new List<string> { "Familiarización con ejercicios", "Activación neuromuscular" },
            2 => new List<string> { "Mejora en coordinación", "Reducción de agujetas" },
            3 => new List<string> { "Aumento de resistencia muscular", "Mejor técnica" },
            4 => new List<string> { "Primeras ganancias de fuerza", "Mayor confianza" },
            5 => new List<string> { "Consolidación de ganancias", "Refinamiento de movimientos" },
            6 => new List<string> { "Aumento notable de capacidad", "Mejor recuperación" },
            7 => new List<string> { "Adaptaciones avanzadas", "Preparación para progresión" },
            8 => new List<string> { "Evaluación de progreso", "Readiness para siguiente fase" },
            _ => new List<string> { "Adaptaciones continuas" }
        };
    }

    private List<ProgressionMarker> CreateProgressionMilestones(UserRoutineParameters parameters)
    {
        return new List<ProgressionMarker>
        {
            new ProgressionMarker
            {
                Metric = "Técnica ejercicios básicos",
                TargetValue = "Ejecución correcta consistente",
                TargetWeek = 2,
                AssessmentMethod = "Autoevaluación y video si es posible"
            },
            new ProgressionMarker
            {
                Metric = "Resistencia cardiovascular",
                TargetValue = "Completar sesión sin fatiga excesiva",
                TargetWeek = 4,
                AssessmentMethod = "Escala de percepción del esfuerzo"
            },
            new ProgressionMarker
            {
                Metric = "Fuerza funcional",
                TargetValue = "Aumento 20% en repeticiones o resistencia",
                TargetWeek = 6,
                AssessmentMethod = "Registro de entrenamientos"
            },
            new ProgressionMarker
            {
                Metric = "Capacidad general",
                TargetValue = "Listo para progresión avanzada",
                TargetWeek = 8,
                AssessmentMethod = "Evaluación integral"
            }
        };
    }
}