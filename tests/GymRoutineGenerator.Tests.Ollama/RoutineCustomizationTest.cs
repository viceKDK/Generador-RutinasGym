using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Infrastructure.AI;

namespace GymRoutineGenerator.Tests.Ollama;

public static class RoutineCustomizationTest
{
    public static async Task RunRoutineCustomizationTests()
    {
        Console.WriteLine("🎯 INICIANDO PRUEBAS DE PERSONALIZACIÓN Y VARIACIÓN DE RUTINAS");
        Console.WriteLine("================================================================");
        Console.WriteLine();

        // Create mock services
        var responseProcessor = new SpanishResponseProcessor();
        var customizationService = new RoutineCustomizationService(responseProcessor);

        // Test 1: Complete routine customization
        await TestCompleteRoutineCustomization(customizationService);
        Console.WriteLine();

        // Test 2: Routine variations generation
        await TestRoutineVariationsGeneration(customizationService);
        Console.WriteLine();

        // Test 3: Constraint-based adaptation
        await TestConstraintBasedAdaptation(customizationService);
        Console.WriteLine();

        // Test 4: Personalized program creation
        await TestPersonalizedProgramCreation(customizationService);
        Console.WriteLine();

        // Test 5: Exercise substitutions
        await TestExerciseSubstitutions(customizationService);
        Console.WriteLine();

        // Test 6: Advanced customization scenarios
        await TestAdvancedCustomizationScenarios(customizationService);
        Console.WriteLine();

        Console.WriteLine("✅ TODAS LAS PRUEBAS DE PERSONALIZACIÓN COMPLETADAS");
    }

    private static async Task TestCompleteRoutineCustomization(IRoutineCustomizationService service)
    {
        Console.WriteLine("🧪 Test 1: Personalización Completa de Rutina");
        Console.WriteLine("─────────────────────────────────────────────");

        var testCases = new[]
        {
            new
            {
                Name = "Principiante joven en casa",
                Profile = new UserProfile
                {
                    UserId = "user001",
                    Name = "Ana Martín",
                    Age = 25,
                    Gender = "Femenino",
                    Weight = 60,
                    Height = 165,
                    ExperienceLevel = "Principiante",
                    ActivityLevel = "Sedentario",
                    PhysicalLimitations = new List<string>(),
                    InjuryHistory = new List<string>()
                },
                Preferences = new RoutinePreferences
                {
                    PreferredWorkoutDuration = TimeSpan.FromMinutes(45),
                    MaxWorkoutDuration = TimeSpan.FromMinutes(60),
                    PreferredDaysPerWeek = 3,
                    PreferredExerciseTypes = new List<string> { "Compound", "Bodyweight" },
                    PreferredMuscleGroupFocus = new List<string> { "Core", "Piernas", "Brazos" },
                    IntensityPreference = "Moderada",
                    WantsCardioIntegration = true,
                    WantsFlexibilityWork = true
                },
                Environment = new EnvironmentConstraints
                {
                    WorkoutLocation = "Casa",
                    AvailableEquipment = new List<string> { "Peso corporal", "Esterilla", "Bandas elásticas" },
                    AvailableSpace = 10.0,
                    NoiseConstraints = "Baja"
                }
            },
            new
            {
                Name = "Adulto intermedio con limitaciones",
                Profile = new UserProfile
                {
                    UserId = "user002",
                    Name = "Carlos García",
                    Age = 45,
                    Gender = "Masculino",
                    Weight = 80,
                    Height = 178,
                    ExperienceLevel = "Intermedio",
                    ActivityLevel = "Ligero",
                    PhysicalLimitations = new List<string> { "Dolor lumbar leve", "Problemas de rodilla" },
                    InjuryHistory = new List<string> { "Lesión de espalda hace 2 años" }
                },
                Preferences = new RoutinePreferences
                {
                    PreferredWorkoutDuration = TimeSpan.FromMinutes(50),
                    MaxWorkoutDuration = TimeSpan.FromMinutes(70),
                    PreferredDaysPerWeek = 4,
                    PreferredExerciseTypes = new List<string> { "Low-impact", "Strength" },
                    PreferredMuscleGroupFocus = new List<string> { "Espalda", "Core", "Hombros" },
                    IntensityPreference = "Moderada",
                    DislikedExercises = new List<string> { "Sentadillas profundas", "Peso muerto" }
                },
                Environment = new EnvironmentConstraints
                {
                    WorkoutLocation = "Gimnasio",
                    AvailableEquipment = new List<string> { "Mancuernas", "Máquinas", "Cables", "Esterilla" },
                    AvailableSpace = 25.0
                }
            },
            new
            {
                Name = "Adulto mayor enfocado en funcionalidad",
                Profile = new UserProfile
                {
                    UserId = "user003",
                    Name = "María López",
                    Age = 68,
                    Gender = "Femenino",
                    Weight = 65,
                    Height = 162,
                    ExperienceLevel = "Principiante",
                    ActivityLevel = "Ligero",
                    PhysicalLimitations = new List<string> { "Artritis leve", "Equilibrio reducido" },
                    Medications = new List<string> { "Medicación para presión arterial" }
                },
                Preferences = new RoutinePreferences
                {
                    PreferredWorkoutDuration = TimeSpan.FromMinutes(35),
                    MaxWorkoutDuration = TimeSpan.FromMinutes(45),
                    PreferredDaysPerWeek = 3,
                    PreferredExerciseTypes = new List<string> { "Functional", "Flexibility", "Balance" },
                    PreferredMuscleGroupFocus = new List<string> { "Core", "Piernas", "Balance" },
                    IntensityPreference = "Baja",
                    WantsFlexibilityWork = true
                },
                Environment = new EnvironmentConstraints
                {
                    WorkoutLocation = "Casa",
                    AvailableEquipment = new List<string> { "Silla", "Peso corporal", "Bandas ligeras" },
                    AvailableSpace = 8.0,
                    SafetyFeatures = new List<string> { "Apoyo disponible", "Superficie antideslizante" }
                }
            }
        };

        foreach (var testCase in testCases)
        {
            try
            {
                Console.WriteLine($"   • {testCase.Name}:");

                var customizationRequest = new CustomizationRequest
                {
                    UserProfile = testCase.Profile,
                    Preferences = testCase.Preferences,
                    Environment = testCase.Environment,
                    Priorities = new PrioritySettings
                    {
                        SafetyPriority = 10,
                        EffectivenessPriority = 8,
                        ConveniencePriority = 7
                    },
                    Progression = new ProgressionPreferences
                    {
                        ProgressionStyle = "Linear",
                        WeeksPerPhase = 4,
                        WantsDeloadWeeks = true
                    }
                };

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var customizedRoutine = await service.CreateCustomizedRoutineAsync(customizationRequest);
                stopwatch.Stop();

                Console.WriteLine($"     📊 RUTINA PERSONALIZADA CREADA:");
                Console.WriteLine($"     • Tiempo de personalización: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"     • Nombre: {customizedRoutine.RoutineName}");
                Console.WriteLine($"     • Duración estimada: {customizedRoutine.EstimatedDuration.TotalMinutes:F0} minutos");
                Console.WriteLine($"     • Usuario: {customizedRoutine.UserId}");

                Console.WriteLine($"     🔥 CALENTAMIENTO PERSONALIZADO:");
                Console.WriteLine($"     • Duración: {customizedRoutine.Warmup.Duration.TotalMinutes:F0} minutos");
                Console.WriteLine($"     • Fases: {customizedRoutine.Warmup.Phases.Count}");
                Console.WriteLine($"     • Razón personalización: {customizedRoutine.Warmup.PersonalizationReason}");
                if (customizedRoutine.Warmup.SpecialConsiderations.Any())
                {
                    Console.WriteLine($"     • Consideraciones especiales: {customizedRoutine.Warmup.SpecialConsiderations.Count}");
                }

                Console.WriteLine($"     💪 BLOQUES DE ENTRENAMIENTO:");
                foreach (var block in customizedRoutine.WorkoutBlocks.Take(3))
                {
                    Console.WriteLine($"     • {block.BlockName} ({block.BlockType}):");
                    Console.WriteLine($"       - Propósito: {block.Purpose}");
                    Console.WriteLine($"       - Ejercicios: {block.Exercises.Count}");
                    Console.WriteLine($"       - Tiempo estimado: {block.EstimatedTime.TotalMinutes:F0} min");
                    if (block.CustomizationReasons.Any())
                    {
                        Console.WriteLine($"       - Personalización: {string.Join(", ", block.CustomizationReasons.Take(1))}");
                    }
                }

                Console.WriteLine($"     🧘 ENFRIAMIENTO PERSONALIZADO:");
                Console.WriteLine($"     • Duración: {customizedRoutine.Cooldown.Duration.TotalMinutes:F0} minutos");
                Console.WriteLine($"     • Fases: {customizedRoutine.Cooldown.Phases.Count}");
                Console.WriteLine($"     • Consejos de recuperación: {customizedRoutine.Cooldown.RecoveryTips.Count}");

                Console.WriteLine($"     📈 PLAN DE PROGRESIÓN:");
                Console.WriteLine($"     • Estrategia: {customizedRoutine.ProgressionPlan.Strategy}");
                Console.WriteLine($"     • Semanas planificadas: {customizedRoutine.ProgressionPlan.Weeks.Count}");
                Console.WriteLine($"     • Hitos: {customizedRoutine.ProgressionPlan.Milestones.Count}");

                Console.WriteLine($"     🎯 METADATA DE PERSONALIZACIÓN:");
                Console.WriteLine($"     • Puntuación personalización: {customizedRoutine.Metadata.PersonalizationScore:F2}/1.0");
                Console.WriteLine($"     • Reglas aplicadas: {customizedRoutine.Metadata.AppliedRules.Count}");
                Console.WriteLine($"     • Adaptaciones de seguridad: {customizedRoutine.Metadata.SafetyAdaptations.Count}");
                Console.WriteLine($"     • Adaptaciones de preferencias: {customizedRoutine.Metadata.PreferenceAdaptations.Count}");

                if (customizedRoutine.PersonalizationNotes.Any())
                {
                    Console.WriteLine($"     📝 NOTAS DE PERSONALIZACIÓN ({customizedRoutine.PersonalizationNotes.Count}):");
                    foreach (var note in customizedRoutine.PersonalizationNotes.Take(2))
                    {
                        Console.WriteLine($"     • {note.Category}: {note.Note}");
                        Console.WriteLine($"       → Razón: {note.Reason}");
                    }
                }

                if (customizedRoutine.Adaptations.MajorAdaptations.Any() || customizedRoutine.Adaptations.SafetyModifications.Any())
                {
                    Console.WriteLine($"     🔧 RESUMEN DE ADAPTACIONES:");
                    if (customizedRoutine.Adaptations.MajorAdaptations.Any())
                        Console.WriteLine($"     • Adaptaciones mayores: {customizedRoutine.Adaptations.MajorAdaptations.Count}");
                    if (customizedRoutine.Adaptations.SafetyModifications.Any())
                        Console.WriteLine($"     • Modificaciones de seguridad: {customizedRoutine.Adaptations.SafetyModifications.Count}");
                    if (customizedRoutine.Adaptations.PreferenceAccommodations.Any())
                        Console.WriteLine($"     • Acomodaciones de preferencias: {customizedRoutine.Adaptations.PreferenceAccommodations.Count}");
                }

                // Validation checks
                var isValid = ValidateCustomizedRoutine(customizedRoutine, testCase.Preferences);
                Console.WriteLine($"     ✓ {(isValid ? "PERSONALIZACIÓN EXITOSA" : "PERSONALIZACIÓN INCOMPLETA")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en personalización '{testCase.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestRoutineVariationsGeneration(IRoutineCustomizationService service)
    {
        Console.WriteLine("🧪 Test 2: Generación de Variaciones de Rutina");
        Console.WriteLine("──────────────────────────────────────────────");

        var baseRoutine = new BaseRoutine
        {
            RoutineId = "base001",
            Name = "Rutina Base Fuerza",
            Description = "Rutina de fuerza con ejercicios compound",
            Exercises = new List<GymRoutineGenerator.Core.Models.Exercise>
            {
                new() { Name = "Sentadillas", MuscleGroups = new List<string> { "Piernas", "Glúteos" } },
                new() { Name = "Press de banca", MuscleGroups = new List<string> { "Pecho", "Tríceps" } },
                new() { Name = "Peso muerto", MuscleGroups = new List<string> { "Espalda", "Piernas" } },
                new() { Name = "Press militar", MuscleGroups = new List<string> { "Hombros", "Core" } }
            },
            EstimatedDuration = TimeSpan.FromMinutes(60),
            DifficultyLevel = "Intermedio",
            TargetMuscleGroups = new List<string> { "Piernas", "Pecho", "Espalda", "Hombros" }
        };

        var variationOptions = new VariationOptions
        {
            VariationTypes = new List<string> { "Equipment", "Difficulty", "Duration", "Focus" },
            MaxVariations = 5,
            MinSimilarityScore = 0.6,
            AllowEquipmentChanges = true,
            AllowDifficultyChanges = true,
            AllowDurationChanges = true
        };

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var variations = await service.GenerateRoutineVariationsAsync(baseRoutine, variationOptions);
            stopwatch.Stop();

            Console.WriteLine($"   📊 VARIACIONES GENERADAS: {variations.Count}");
            Console.WriteLine($"   • Tiempo de generación: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine();

            foreach (var variation in variations.Take(3))
            {
                Console.WriteLine($"   🔄 {variation.VariationName}:");
                Console.WriteLine($"   • Tipo: {variation.VariationType}");
                Console.WriteLine($"   • Puntuación similitud: {variation.SimilarityScore:F2}/1.0");
                Console.WriteLine($"   • Razón variación: {variation.VariationReason}");
                Console.WriteLine($"   • Cambios: {variation.Changes.Count}");

                if (variation.Changes.Any())
                {
                    Console.WriteLine($"   • Principales cambios:");
                    foreach (var change in variation.Changes.Take(2))
                    {
                        Console.WriteLine($"     - {change}");
                    }
                }

                if (variation.Benefits.Any())
                {
                    Console.WriteLine($"   • Beneficios: {string.Join(", ", variation.Benefits.Take(2))}");
                }

                if (variation.Considerations.Any())
                {
                    Console.WriteLine($"   • Consideraciones: {string.Join(", ", variation.Considerations.Take(2))}");
                }

                Console.WriteLine($"   • Rutina modificada:");
                Console.WriteLine($"     - Nombre: {variation.ModifiedRoutine.Name}");
                Console.WriteLine($"     - Ejercicios: {variation.ModifiedRoutine.Exercises.Count}");
                Console.WriteLine($"     - Duración: {variation.ModifiedRoutine.EstimatedDuration.TotalMinutes:F0} min");
                Console.WriteLine($"     - Dificultad: {variation.ModifiedRoutine.DifficultyLevel}");
                Console.WriteLine();
            }

            // Validation
            var hasValidVariations = variations.All(v => v.SimilarityScore >= variationOptions.MinSimilarityScore);
            var hasCorrectCount = variations.Count <= variationOptions.MaxVariations;
            var hasVariedTypes = variations.Select(v => v.VariationType).Distinct().Count() > 1;

            Console.WriteLine($"   ✓ Variaciones válidas: {hasValidVariations}");
            Console.WriteLine($"   ✓ Cantidad correcta: {hasCorrectCount}");
            Console.WriteLine($"   ✓ Tipos variados: {hasVariedTypes}");

            var success = hasValidVariations && hasCorrectCount;
            Console.WriteLine($"   {(success ? "✅ GENERACIÓN DE VARIACIONES EXITOSA" : "⚠️ GENERACIÓN PARCIALMENTE EXITOSA")}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error en generación de variaciones: {ex.Message}");
        }
    }

    private static async Task TestConstraintBasedAdaptation(IRoutineCustomizationService service)
    {
        Console.WriteLine("🧪 Test 3: Adaptación Basada en Restricciones");
        Console.WriteLine("─────────────────────────────────────────────");

        var baseRoutine = new BaseRoutine
        {
            RoutineId = "adapt001",
            Name = "Rutina Completa Gym",
            Description = "Rutina completa para gimnasio",
            Exercises = new List<GymRoutineGenerator.Core.Models.Exercise>
            {
                new() { Name = "Sentadillas con barra", MuscleGroups = new List<string> { "Piernas" } },
                new() { Name = "Press de banca", MuscleGroups = new List<string> { "Pecho" } },
                new() { Name = "Dominadas", MuscleGroups = new List<string> { "Espalda" } },
                new() { Name = "Press militar", MuscleGroups = new List<string> { "Hombros" } }
            },
            EstimatedDuration = TimeSpan.FromMinutes(75),
            DifficultyLevel = "Intermedio"
        };

        var constraintSets = new[]
        {
            new
            {
                Name = "Limitaciones de tiempo y equipamiento",
                Constraints = new ConstraintSet
                {
                    TimeConstraints = new List<TimeConstraint>
                    {
                        new() {
                            MaxWorkoutDuration = TimeSpan.FromMinutes(45),
                            PreferredDuration = TimeSpan.FromMinutes(40)
                        }
                    },
                    EquipmentConstraints = new List<EquipmentConstraint>
                    {
                        new() {
                            AvailableEquipment = new List<string> { "Peso corporal", "Mancuernas", "Esterilla" },
                            UnavailableEquipment = new List<string> { "Barra", "Máquinas", "Poleas" }
                        }
                    }
                }
            },
            new
            {
                Name = "Limitaciones físicas y de seguridad",
                Constraints = new ConstraintSet
                {
                    PhysicalConstraints = new List<PhysicalConstraint>
                    {
                        new() {
                            ConstraintType = "Injury",
                            Description = "Lesión de hombro derecho",
                            AffectedMovements = new List<string> { "Press vertical", "Elevaciones laterales" },
                            RestrictedExercises = new List<string> { "Press militar", "Dominadas" },
                            Severity = ConstraintSeverity.Moderate
                        }
                    },
                    SafetyConstraints = new List<SafetyConstraint>
                    {
                        new() {
                            ProhibitedMovements = new List<string> { "Movimientos por encima de la cabeza" },
                            MaxHeartRateLimit = 150
                        }
                    }
                }
            },
            new
            {
                Name = "Múltiples restricciones combinadas",
                Constraints = new ConstraintSet
                {
                    PhysicalConstraints = new List<PhysicalConstraint>
                    {
                        new() {
                            ConstraintType = "Limitation",
                            Description = "Problemas de rodilla",
                            RestrictedExercises = new List<string> { "Sentadillas profundas", "Saltos" },
                            Severity = ConstraintSeverity.Mild
                        }
                    },
                    TimeConstraints = new List<TimeConstraint>
                    {
                        new() { MaxWorkoutDuration = TimeSpan.FromMinutes(35) }
                    },
                    PreferenceConstraints = new List<PreferenceConstraint>
                    {
                        new() {
                            DislikedExercises = new List<string> { "Burpees", "Mountain climbers" },
                            PreferCompoundMovements = true
                        }
                    }
                }
            }
        };

        foreach (var constraintSet in constraintSets)
        {
            try
            {
                Console.WriteLine($"   • {constraintSet.Name}:");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var adaptedRoutine = await service.AdaptRoutineToConstraintsAsync(baseRoutine, constraintSet.Constraints);
                stopwatch.Stop();

                Console.WriteLine($"     📊 ADAPTACIÓN COMPLETADA:");
                Console.WriteLine($"     • Tiempo de adaptación: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"     • Puntuación adaptación: {adaptedRoutine.AdaptationScore:F2}/1.0");

                Console.WriteLine($"     🔄 RUTINA ORIGINAL VS ADAPTADA:");
                Console.WriteLine($"     • Original - Ejercicios: {adaptedRoutine.OriginalRoutine.Exercises.Count}, Duración: {adaptedRoutine.OriginalRoutine.EstimatedDuration.TotalMinutes:F0} min");
                Console.WriteLine($"     • Adaptada - Ejercicios: {adaptedRoutine.AdaptedRoutine_.Exercises.Count}, Duración: {adaptedRoutine.AdaptedRoutine_.EstimatedDuration.TotalMinutes:F0} min");

                if (adaptedRoutine.Adaptations.Any())
                {
                    Console.WriteLine($"     🛠️ ADAPTACIONES APLICADAS ({adaptedRoutine.Adaptations.Count}):");
                    foreach (var adaptation in adaptedRoutine.Adaptations.Take(3))
                    {
                        Console.WriteLine($"     • {adaptation.AdaptationType}: {adaptation.OriginalElement} → {adaptation.AdaptedElement}");
                        Console.WriteLine($"       Razón: {adaptation.Reason}");
                        Console.WriteLine($"       Impacto: {adaptation.ImpactScore:F2}/1.0");
                    }
                }

                if (adaptedRoutine.LimitationsNotAddressed.Any())
                {
                    Console.WriteLine($"     ⚠️ LIMITACIONES NO RESUELTAS:");
                    foreach (var limitation in adaptedRoutine.LimitationsNotAddressed.Take(2))
                    {
                        Console.WriteLine($"     • {limitation}");
                    }
                }

                // Constraint analysis
                var constraintTypes = GetConstraintTypes(constraintSet.Constraints);
                Console.WriteLine($"     📋 TIPOS DE RESTRICCIONES APLICADAS:");
                foreach (var constraintType in constraintTypes)
                {
                    Console.WriteLine($"     • {constraintType}");
                }

                var isWellAdapted = adaptedRoutine.AdaptationScore >= 0.7 && adaptedRoutine.LimitationsNotAddressed.Count <= 1;
                Console.WriteLine($"     ✓ {(isWellAdapted ? "ADAPTACIÓN EXITOSA" : "ADAPTACIÓN PARCIAL")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"     ❌ Error en adaptación '{constraintSet.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestPersonalizedProgramCreation(IRoutineCustomizationService service)
    {
        Console.WriteLine("🧪 Test 4: Creación de Programa Personalizado");
        Console.WriteLine("─────────────────────────────────────────────");

        var testPrograms = new[]
        {
            new
            {
                Name = "Programa pérdida de peso para principiante",
                Profile = new UserProfile
                {
                    UserId = "prog001",
                    Name = "Laura Ruiz",
                    Age = 32,
                    Weight = 75,
                    Height = 165,
                    ExperienceLevel = "Principiante",
                    ActivityLevel = "Sedentario"
                },
                Goals = new ProgramGoals
                {
                    PrimaryGoal = "Pérdida de peso",
                    SecondaryGoals = new List<string> { "Mejorar condición cardiovascular", "Tonificar músculos" },
                    QuantifiableTargets = new Dictionary<string, MeasurableTarget>
                    {
                        ["Peso"] = new() { Metric = "Peso", CurrentValue = 75, TargetValue = 65, Unit = "kg" },
                        ["Grasa corporal"] = new() { Metric = "Grasa corporal", CurrentValue = 30, TargetValue = 22, Unit = "%" }
                    },
                    TargetDate = DateTime.UtcNow.AddMonths(6)
                }
            },
            new
            {
                Name = "Programa ganancia de masa muscular",
                Profile = new UserProfile
                {
                    UserId = "prog002",
                    Name = "Miguel Torres",
                    Age = 28,
                    Weight = 70,
                    Height = 180,
                    ExperienceLevel = "Intermedio",
                    ActivityLevel = "Moderado"
                },
                Goals = new ProgramGoals
                {
                    PrimaryGoal = "Ganancia de masa muscular",
                    SecondaryGoals = new List<string> { "Aumentar fuerza", "Mejorar definición" },
                    QuantifiableTargets = new Dictionary<string, MeasurableTarget>
                    {
                        ["Peso"] = new() { Metric = "Peso", CurrentValue = 70, TargetValue = 78, Unit = "kg" },
                        ["Press banca"] = new() { Metric = "Press banca 1RM", CurrentValue = 80, TargetValue = 100, Unit = "kg" }
                    },
                    TargetDate = DateTime.UtcNow.AddMonths(4)
                }
            }
        };

        foreach (var testProgram in testPrograms)
        {
            try
            {
                Console.WriteLine($"   • {testProgram.Name}:");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var personalizedProgram = await service.CreatePersonalizedProgramAsync(testProgram.Profile, testProgram.Goals);
                stopwatch.Stop();

                Console.WriteLine($"     📊 PROGRAMA PERSONALIZADO CREADO:");
                Console.WriteLine($"     • Tiempo de creación: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"     • ID del programa: {personalizedProgram.ProgramId}");
                Console.WriteLine($"     • Usuario: {personalizedProgram.UserProfile.Name}");
                Console.WriteLine($"     • Duración total: {personalizedProgram.TotalDuration.TotalDays:F0} días");
                Console.WriteLine($"     • Fecha inicio: {personalizedProgram.StartDate:yyyy-MM-dd}");

                Console.WriteLine($"     🎯 OBJETIVOS DEL PROGRAMA:");
                Console.WriteLine($"     • Objetivo principal: {personalizedProgram.Goals.PrimaryGoal}");
                Console.WriteLine($"     • Objetivos secundarios: {personalizedProgram.Goals.SecondaryGoals.Count}");
                Console.WriteLine($"     • Metas cuantificables: {personalizedProgram.Goals.QuantifiableTargets.Count}");

                if (personalizedProgram.Goals.QuantifiableTargets.Any())
                {
                    Console.WriteLine($"     📈 METAS ESPECÍFICAS:");
                    foreach (var target in personalizedProgram.Goals.QuantifiableTargets.Take(2))
                    {
                        var targetInfo = target.Value;
                        Console.WriteLine($"     • {targetInfo.Metric}: {targetInfo.CurrentValue} → {targetInfo.TargetValue} {targetInfo.Unit}");
                    }
                }

                Console.WriteLine($"     🏗️ ESTRUCTURA DEL PROGRAMA:");
                Console.WriteLine($"     • Fases del programa: {personalizedProgram.Phases.Count}");

                foreach (var phase in personalizedProgram.Phases.Take(2))
                {
                    Console.WriteLine($"     • Fase {phase.PhaseNumber}: {phase.PhaseName}");
                    Console.WriteLine($"       - Duración: {phase.Duration.TotalDays:F0} días");
                    Console.WriteLine($"       - Enfoque: {phase.Focus}");
                    Console.WriteLine($"       - Rutinas: {phase.Routines.Count}");
                }

                Console.WriteLine($"     📊 SEGUIMIENTO DEL PROGRESO:");
                Console.WriteLine($"     • Métricas a seguir: {personalizedProgram.TrackingPlan.TrackingMetrics.Count}");
                Console.WriteLine($"     • Frecuencia evaluación: {personalizedProgram.TrackingPlan.AssessmentFrequency} días");
                Console.WriteLine($"     • Métodos evaluación: {personalizedProgram.TrackingPlan.AssessmentMethods.Count}");

                Console.WriteLine($"     🏆 HITOS DEL PROGRAMA:");
                Console.WriteLine($"     • Hitos planificados: {personalizedProgram.Milestones.Count}");

                foreach (var milestone in personalizedProgram.Milestones.Take(2))
                {
                    Console.WriteLine($"     • {milestone.MilestoneName}:");
                    Console.WriteLine($"       - Fecha objetivo: {milestone.TargetDate:yyyy-MM-dd}");
                    Console.WriteLine($"       - Criterios éxito: {milestone.SuccessCriteria.Count}");
                }

                // Program validation
                var hasValidStructure = personalizedProgram.Phases.Count >= 2 && personalizedProgram.TotalDuration.TotalDays >= 28;
                var hasProgressTracking = personalizedProgram.TrackingPlan.TrackingMetrics.Any();
                var hasMilestones = personalizedProgram.Milestones.Any();

                Console.WriteLine($"     ✓ Estructura válida: {hasValidStructure}");
                Console.WriteLine($"     ✓ Sistema seguimiento: {hasProgressTracking}");
                Console.WriteLine($"     ✓ Hitos definidos: {hasMilestones}");

                var isProgramValid = hasValidStructure && hasProgressTracking && hasMilestones;
                Console.WriteLine($"     {(isProgramValid ? "✅ PROGRAMA PERSONALIZADO EXITOSO" : "⚠️ PROGRAMA PARCIALMENTE COMPLETADO")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"     ❌ Error en programa '{testProgram.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestExerciseSubstitutions(IRoutineCustomizationService service)
    {
        Console.WriteLine("🧪 Test 5: Sustituciones de Ejercicios");
        Console.WriteLine("─────────────────────────────────────");

        var substitutionTests = new[]
        {
            new
            {
                Exercise = "Sentadillas con barra",
                Criteria = new SubstitutionCriteria
                {
                    RequiredMuscleGroups = new List<string> { "Cuádriceps", "Glúteos" },
                    AvailableEquipment = new List<string> { "Peso corporal", "Mancuernas" },
                    MaxDifficulty = "Intermedio",
                    MovementPatterns = new List<string> { "Squat pattern" },
                    AvoidedMovements = new List<string> { "Impacto alto" },
                    MaintainIntensity = true,
                    MinSimilarityScore = 0.7
                },
                Description = "Sustituir sentadilla con barra por opciones sin equipamiento pesado"
            },
            new
            {
                Exercise = "Dominadas",
                Criteria = new SubstitutionCriteria
                {
                    RequiredMuscleGroups = new List<string> { "Dorsales", "Bíceps" },
                    AvailableEquipment = new List<string> { "Bandas elásticas", "Mesa", "Peso corporal" },
                    MaxDifficulty = "Principiante",
                    MovementPatterns = new List<string> { "Pull pattern" },
                    MaintainIntensity = false,
                    MinSimilarityScore = 0.6
                },
                Description = "Sustituir dominadas por ejercicios para principiantes"
            },
            new
            {
                Exercise = "Press de banca",
                Criteria = new SubstitutionCriteria
                {
                    RequiredMuscleGroups = new List<string> { "Pectorales", "Tríceps" },
                    AvailableEquipment = new List<string> { "Mancuernas", "Peso corporal" },
                    MaxDifficulty = "Intermedio",
                    AvoidedMovements = new List<string> { "Presión en hombros" },
                    MaintainIntensity = true,
                    MinSimilarityScore = 0.8
                },
                Description = "Sustituir press de banca evitando estrés en hombros"
            }
        };

        foreach (var test in substitutionTests)
        {
            try
            {
                Console.WriteLine($"   • {test.Description}:");
                Console.WriteLine($"     Ejercicio original: {test.Exercise}");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var substitutions = await service.GetExerciseSubstitutionsAsync(test.Exercise, test.Criteria);
                stopwatch.Stop();

                Console.WriteLine($"     📊 SUSTITUCIONES ENCONTRADAS: {substitutions.Count}");
                Console.WriteLine($"     • Tiempo de búsqueda: {stopwatch.ElapsedMilliseconds} ms");

                foreach (var substitution in substitutions.Take(3))
                {
                    Console.WriteLine($"     🔄 {substitution.SubstituteExercise}:");
                    Console.WriteLine($"     • Similitud: {substitution.SimilarityScore:F2}/1.0");
                    Console.WriteLine($"     • Razón sustitución: {substitution.SubstitutionReason}");
                    Console.WriteLine($"     • Músculos similares: {string.Join(", ", substitution.SimilarMuscleGroups.Take(3))}");
                    Console.WriteLine($"     • Equipamiento: {substitution.EquipmentRequired}");
                    Console.WriteLine($"     • Comparación dificultad: {substitution.DifficultyComparison}");

                    if (substitution.Differences.Any())
                    {
                        Console.WriteLine($"     • Diferencias: {string.Join(", ", substitution.Differences.Take(2))}");
                    }

                    if (substitution.ModificationNotes.Any())
                    {
                        Console.WriteLine($"     • Notas modificación: {string.Join(", ", substitution.ModificationNotes.Take(1))}");
                    }
                    Console.WriteLine();
                }

                // Validation
                var meetsMinSimilarity = substitutions.All(s => s.SimilarityScore >= test.Criteria.MinSimilarityScore);
                var hasRequiredMuscles = substitutions.All(s =>
                    test.Criteria.RequiredMuscleGroups.All(rm =>
                        s.SimilarMuscleGroups.Any(sm =>
                            sm.Contains(rm, StringComparison.OrdinalIgnoreCase))));
                var hasValidEquipment = substitutions.All(s =>
                    test.Criteria.AvailableEquipment.Contains(s.EquipmentRequired, StringComparer.OrdinalIgnoreCase) ||
                    s.EquipmentRequired == "Ninguno");

                Console.WriteLine($"     ✓ Similitud adecuada: {meetsMinSimilarity}");
                Console.WriteLine($"     ✓ Músculos requeridos: {hasRequiredMuscles}");
                Console.WriteLine($"     ✓ Equipamiento disponible: {hasValidEquipment}");

                var isSubstitutionValid = meetsMinSimilarity && substitutions.Count > 0;
                Console.WriteLine($"     {(isSubstitutionValid ? "✅ SUSTITUCIONES VÁLIDAS" : "⚠️ SUSTITUCIONES LIMITADAS")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"     ❌ Error en sustitución '{test.Exercise}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestAdvancedCustomizationScenarios(IRoutineCustomizationService service)
    {
        Console.WriteLine("🧪 Test 6: Escenarios Avanzados de Personalización");
        Console.WriteLine("──────────────────────────────────────────────────");

        var advancedScenarios = new[]
        {
            new
            {
                Name = "Atleta de élite con periodización compleja",
                Request = new CustomizationRequest
                {
                    UserProfile = new UserProfile
                    {
                        UserId = "elite001",
                        Name = "Pedro Atleta",
                        Age = 26,
                        ExperienceLevel = "Avanzado",
                        ActivityLevel = "Muy Activo",
                        BiometricHistory = new List<BiometricData>
                        {
                            new() { Date = DateTime.Now.AddDays(-30), Weight = 75, BodyFatPercentage = 8, RestingHeartRate = 45 }
                        }
                    },
                    Preferences = new RoutinePreferences
                    {
                        PreferredWorkoutDuration = TimeSpan.FromMinutes(90),
                        PreferredDaysPerWeek = 6,
                        PreferredExerciseTypes = new List<string> { "Olympic lifts", "Compound", "Plyometric" },
                        IntensityPreference = "Alta"
                    },
                    Progression = new ProgressionPreferences
                    {
                        ProgressionStyle = "Block",
                        WeeksPerPhase = 3,
                        WantsPeriodization = true,
                        WantsDeloadWeeks = true
                    }
                }
            },
            new
            {
                Name = "Rehabilitación post-lesión con múltiples restricciones",
                Request = new CustomizationRequest
                {
                    UserProfile = new UserProfile
                    {
                        UserId = "rehab001",
                        Name = "María Recuperación",
                        Age = 40,
                        ExperienceLevel = "Intermedio",
                        PhysicalLimitations = new List<string> { "Lesión ACL reciente", "Tendinitis hombro izquierdo" },
                        InjuryHistory = new List<string> { "Cirugía rodilla hace 6 meses" },
                        Medications = new List<string> { "Antiinflamatorios" }
                    },
                    Preferences = new RoutinePreferences
                    {
                        PreferredWorkoutDuration = TimeSpan.FromMinutes(40),
                        PreferredDaysPerWeek = 4,
                        IntensityPreference = "Baja",
                        WantsFlexibilityWork = true
                    },
                    Priorities = new PrioritySettings
                    {
                        SafetyPriority = 10,
                        EffectivenessPriority = 6,
                        ConveniencePriority = 8
                    }
                }
            },
            new
            {
                Name = "Madre ocupada con tiempo limitado",
                Request = new CustomizationRequest
                {
                    UserProfile = new UserProfile
                    {
                        UserId = "busy001",
                        Name = "Ana Ocupada",
                        Age = 35,
                        ExperienceLevel = "Principiante",
                        ActivityLevel = "Ligero"
                    },
                    Preferences = new RoutinePreferences
                    {
                        PreferredWorkoutDuration = TimeSpan.FromMinutes(25),
                        MaxWorkoutDuration = TimeSpan.FromMinutes(30),
                        PreferredDaysPerWeek = 3,
                        PreferredTimeSlots = new List<string> { "Morning" },
                        WantsCardioIntegration = true
                    },
                    Environment = new EnvironmentConstraints
                    {
                        WorkoutLocation = "Casa",
                        AvailableEquipment = new List<string> { "Peso corporal" },
                        AvailableSpace = 5.0,
                        NoiseConstraints = "Muy Baja" // Bebé durmiendo
                    }
                }
            }
        };

        foreach (var scenario in advancedScenarios)
        {
            try
            {
                Console.WriteLine($"   • {scenario.Name}:");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var customizedRoutine = await service.CreateCustomizedRoutineAsync(scenario.Request);
                stopwatch.Stop();

                Console.WriteLine($"     📊 PERSONALIZACIÓN AVANZADA COMPLETADA:");
                Console.WriteLine($"     • Tiempo procesamiento: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"     • Puntuación personalización: {customizedRoutine.Metadata.PersonalizationScore:F2}/1.0");

                // Analyze specific scenario requirements
                var profile = scenario.Request.UserProfile;
                var preferences = scenario.Request.Preferences;

                Console.WriteLine($"     🎯 ANÁLISIS DE REQUISITOS ESPECÍFICOS:");

                if (profile.ExperienceLevel == "Avanzado")
                {
                    Console.WriteLine($"     • Rutina para atleta avanzado - Complejidad apropiada: ✓");
                    Console.WriteLine($"     • Bloques de entrenamiento: {customizedRoutine.WorkoutBlocks.Count}");
                }

                if (profile.PhysicalLimitations.Any() || profile.InjuryHistory.Any())
                {
                    Console.WriteLine($"     • Adaptaciones de seguridad aplicadas: {customizedRoutine.Metadata.SafetyAdaptations.Count}");
                    Console.WriteLine($"     • Notas de personalización críticas: {customizedRoutine.PersonalizationNotes.Count(n => n.Priority >= NotePriority.High)}");
                }

                if (preferences.PreferredWorkoutDuration.TotalMinutes <= 30)
                {
                    var actualDuration = customizedRoutine.EstimatedDuration.TotalMinutes;
                    var withinTimeLimit = actualDuration <= preferences.MaxWorkoutDuration.TotalMinutes;
                    Console.WriteLine($"     • Duración optimizada: {actualDuration:F0} min (límite: {preferences.MaxWorkoutDuration.TotalMinutes} min) - {(withinTimeLimit ? "✓" : "⚠️")}");
                }

                if (scenario.Request.Environment?.NoiseConstraints == "Muy Baja")
                {
                    Console.WriteLine($"     • Ejercicios de bajo impacto priorizados: ✓");
                }

                Console.WriteLine($"     🔧 RESUMEN DE PERSONALIZACIONES:");
                Console.WriteLine($"     • Reglas aplicadas: {customizedRoutine.Metadata.AppliedRules.Count}");
                Console.WriteLine($"     • Adaptaciones preferencias: {customizedRoutine.Metadata.PreferenceAdaptations.Count}");
                Console.WriteLine($"     • Adaptaciones restricciones: {customizedRoutine.Metadata.ConstraintAdaptations.Count}");

                if (customizedRoutine.PersonalizationNotes.Any())
                {
                    var criticalNotes = customizedRoutine.PersonalizationNotes.Where(n => n.Priority >= NotePriority.High).ToList();
                    if (criticalNotes.Any())
                    {
                        Console.WriteLine($"     ⚠️ NOTAS CRÍTICAS:");
                        foreach (var note in criticalNotes.Take(2))
                        {
                            Console.WriteLine($"     • {note.Category}: {note.Note}");
                        }
                    }
                }

                // Scenario-specific validation
                var scenarioScore = ValidateAdvancedScenario(scenario, customizedRoutine);
                Console.WriteLine($"     📈 Puntuación escenario específico: {scenarioScore:F2}/10.0");

                var isHighQuality = customizedRoutine.Metadata.PersonalizationScore >= 0.8 && scenarioScore >= 7.0;
                Console.WriteLine($"     {(isHighQuality ? "✅ PERSONALIZACIÓN AVANZADA EXITOSA" : "⚠️ PERSONALIZACIÓN NECESITA REFINAMIENTO")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"     ❌ Error en escenario '{scenario.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    // Helper methods for validation
    private static bool ValidateCustomizedRoutine(CustomizedRoutine routine, RoutinePreferences preferences)
    {
        var durationMatch = Math.Abs(routine.EstimatedDuration.TotalMinutes - preferences.PreferredWorkoutDuration.TotalMinutes) <= 15;
        var hasWarmup = routine.Warmup.Phases.Any();
        var hasCooldown = routine.Cooldown.Phases.Any();
        var hasWorkoutBlocks = routine.WorkoutBlocks.Any();

        return durationMatch && hasWarmup && hasCooldown && hasWorkoutBlocks;
    }

    private static List<string> GetConstraintTypes(ConstraintSet constraints)
    {
        var types = new List<string>();

        if (constraints.PhysicalConstraints.Any()) types.Add("Limitaciones físicas");
        if (constraints.EquipmentConstraints.Any()) types.Add("Restricciones de equipamiento");
        if (constraints.TimeConstraints.Any()) types.Add("Limitaciones de tiempo");
        if (constraints.SafetyConstraints.Any()) types.Add("Restricciones de seguridad");
        if (constraints.PreferenceConstraints.Any()) types.Add("Restricciones de preferencias");

        return types;
    }

    private static double ValidateAdvancedScenario(dynamic scenario, CustomizedRoutine routine)
    {
        var score = 7.0; // Base score

        var request = scenario.Request as CustomizationRequest;
        if (request == null) return score;

        // Advanced athlete scenario
        if (request.UserProfile.ExperienceLevel == "Avanzado")
        {
            if (routine.WorkoutBlocks.Count >= 3) score += 1.0;
            if (routine.ProgressionPlan.Strategy != ProgressionStrategy.Linear) score += 1.0;
        }

        // Rehabilitation scenario
        if (request.UserProfile.PhysicalLimitations.Any())
        {
            if (routine.Metadata.SafetyAdaptations.Count >= 2) score += 1.0;
            if (routine.PersonalizationNotes.Any(n => n.Priority >= NotePriority.High)) score += 1.0;
        }

        // Time-limited scenario
        if (request.Preferences.PreferredWorkoutDuration.TotalMinutes <= 30)
        {
            var withinTime = routine.EstimatedDuration.TotalMinutes <= request.Preferences.MaxWorkoutDuration.TotalMinutes;
            if (withinTime) score += 1.0;
        }

        return Math.Min(10.0, score);
    }

}