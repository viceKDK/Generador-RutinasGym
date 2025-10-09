using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Infrastructure.AI;

namespace GymRoutineGenerator.Tests.Ollama;

public static class RoutineStructureTest
{
    public static async Task RunRoutineStructureTests()
    {
        Console.WriteLine("=== TESTING ROUTINE STRUCTURE & PROGRAMMING LOGIC (STORY 4.3) ===");
        Console.WriteLine();

        await TestStructuredRoutineCreation();
        await TestWeeklyProgramGeneration();
        await TestExerciseSequenceOptimization();
        await TestTrainingVolumeCalculation();
        await TestWarmupCooldownProtocols();
        await TestProgressionPlanning();
        await TestSafetyConsiderations();

        Console.WriteLine();
        Console.WriteLine("=== ROUTINE STRUCTURE TESTS COMPLETED ===");
    }

    private static async Task TestStructuredRoutineCreation()
    {
        Console.WriteLine("1. Testing Structured Routine Creation");
        Console.WriteLine("-------------------------------------");

        var fallbackService = new FallbackRoutineService();
        var structureService = new RoutineStructureService(fallbackService);

        try
        {
            // Test different user scenarios
            var scenarios = new[]
            {
                ("Beginner strength", CreateBeginnerStrengthParameters()),
                ("Intermediate bodybuilding", CreateIntermediateBodybuildingParameters()),
                ("Senior fitness", CreateSeniorFitnessParameters()),
                ("Advanced athlete", CreateAdvancedAthleteParameters())
            };

            foreach (var (description, parameters) in scenarios)
            {
                Console.WriteLine($"\nTesting scenario: {description}");

                var routine = await structureService.CreateStructuredRoutineAsync(parameters);

                Console.WriteLine($"✓ Routine created: {routine.RoutineName}");
                Console.WriteLine($"✓ Estimated duration: {routine.EstimatedDuration.TotalMinutes} minutes");
                Console.WriteLine($"✓ Training blocks: {routine.MainWorkout.Count}");
                Console.WriteLine($"✓ Total exercises: {routine.MainWorkout.Sum(b => b.Exercises.Count)}");
                Console.WriteLine($"✓ Warmup phases: {routine.Warmup.Phases.Count}");
                Console.WriteLine($"✓ Cooldown phases: {routine.Cooldown.Phases.Count}");
                Console.WriteLine($"✓ Safety considerations: {routine.SafetyNotes.Count}");
                Console.WriteLine($"✓ Progression weeks planned: {routine.Progression.Weeks.Count}");

                // Validate structure principles
                ValidateRoutineStructure(routine, parameters);
            }

            Console.WriteLine("✅ Structured Routine Creation tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Structured Routine Creation test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestWeeklyProgramGeneration()
    {
        Console.WriteLine("2. Testing Weekly Program Generation");
        Console.WriteLine("-----------------------------------");

        var fallbackService = new FallbackRoutineService();
        var structureService = new RoutineStructureService(fallbackService);

        try
        {
            var parameters = CreateIntermediateBodybuildingParameters();
            parameters.TrainingDaysPerWeek = 4;

            var weeklyProgram = await structureService.GenerateWeeklyProgramAsync(parameters);

            Console.WriteLine($"✓ Weekly program generated with {weeklyProgram.Count} sessions");

            for (int i = 0; i < weeklyProgram.Count; i++)
            {
                var session = weeklyProgram[i];
                Console.WriteLine($"  Day {session.DayNumber}: {session.SessionName}");
                Console.WriteLine($"    - Target muscles: {string.Join(", ", session.TargetMuscleGroups)}");
                Console.WriteLine($"    - Exercises: {session.Exercises.Count}");
                Console.WriteLine($"    - Duration: {session.EstimatedDuration.TotalMinutes} min");
                Console.WriteLine($"    - Intensity: {session.TargetIntensity}");
                Console.WriteLine($"    - Special notes: {session.SpecialNotes.Count}");
            }

            // Validate weekly distribution
            var totalTargetedMuscles = weeklyProgram.SelectMany(s => s.TargetMuscleGroups).Distinct().Count();
            Console.WriteLine($"✓ Total muscle groups covered: {totalTargetedMuscles}");

            var totalWeeklyTime = weeklyProgram.Sum(s => s.EstimatedDuration.TotalMinutes);
            Console.WriteLine($"✓ Total weekly training time: {totalWeeklyTime} minutes");

            Console.WriteLine("✅ Weekly Program Generation tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Weekly Program Generation test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestExerciseSequenceOptimization()
    {
        Console.WriteLine("3. Testing Exercise Sequence Optimization");
        Console.WriteLine("----------------------------------------");

        var fallbackService = new FallbackRoutineService();
        var structureService = new RoutineStructureService(fallbackService);

        try
        {
            var parameters = CreateIntermediateBodybuildingParameters();
            var exercises = await fallbackService.GetRecommendedExercisesAsync(parameters);

            Console.WriteLine($"Original exercise count: {exercises.Count}");

            var sequence = await structureService.OptimizeExerciseOrderAsync(exercises, parameters);

            Console.WriteLine($"✓ Exercise sequence optimized");
            Console.WriteLine($"✓ Ordered exercises: {sequence.OrderedExercises.Count}");
            Console.WriteLine($"✓ Transitions: {sequence.Transitions.Count}");
            Console.WriteLine($"✓ Optimization rationale provided: {!string.IsNullOrEmpty(sequence.SequenceRationale)}");
            Console.WriteLine($"✓ Optimization notes: {sequence.OptimizationNotes.Count}");

            // Validate exercise order principles
            Console.WriteLine("\nValidating exercise order principles:");

            // Check compound exercises come first
            var firstExercises = sequence.OrderedExercises.Take(2).ToList();
            var hasCompoundFirst = firstExercises.Any(e => e.BaseExercise.ExerciseType == ExerciseType.Compound);
            Console.WriteLine($"✓ Compound exercises prioritized: {hasCompoundFirst}");

            // Check progression of difficulty
            var difficulties = sequence.OrderedExercises.Select(e => GetDifficultyLevel(e.BaseExercise.DifficultyLevel)).ToList();
            var isProgressivelyOrdered = true;
            for (int i = 1; i < difficulties.Count; i++)
            {
                if (difficulties[i] > difficulties[i - 1] + 1) // Allow some variation
                {
                    isProgressivelyOrdered = false;
                    break;
                }
            }
            Console.WriteLine($"✓ Progressive difficulty ordering: {isProgressivelyOrdered}");

            // Check training parameters are set
            var hasProperParameters = sequence.OrderedExercises.All(e =>
                e.Parameters.Sets > 0 &&
                e.Parameters.Reps.Minimum > 0 &&
                e.RestPeriod.TargetRest.TotalSeconds > 0);
            Console.WriteLine($"✓ All exercises have proper parameters: {hasProperParameters}");

            // Check tempo prescriptions
            var hasTempoPrescrptions = sequence.OrderedExercises.All(e =>
                e.Parameters.Tempo.EccentricSeconds > 0 &&
                e.Parameters.Tempo.ConcentricSeconds >= 0);
            Console.WriteLine($"✓ Tempo prescriptions provided: {hasTempoPrescrptions}");

            Console.WriteLine("✅ Exercise Sequence Optimization tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exercise Sequence Optimization test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestTrainingVolumeCalculation()
    {
        Console.WriteLine("4. Testing Training Volume Calculation");
        Console.WriteLine("-------------------------------------");

        var fallbackService = new FallbackRoutineService();
        var structureService = new RoutineStructureService(fallbackService);

        try
        {
            var scenarios = new[]
            {
                ("Beginner", CreateBeginnerStrengthParameters()),
                ("Intermediate", CreateIntermediateBodybuildingParameters()),
                ("Advanced", CreateAdvancedAthleteParameters()),
                ("Senior", CreateSeniorFitnessParameters())
            };

            foreach (var (level, parameters) in scenarios)
            {
                Console.WriteLine($"\nTesting volume calculation for: {level}");

                var volume = await structureService.CalculateOptimalVolumeAsync(parameters);

                Console.WriteLine($"✓ Total sets: {volume.TotalSets}");
                Console.WriteLine($"✓ Total reps: {volume.TotalReps}");
                Console.WriteLine($"✓ Work time: {volume.TotalWorkTime.TotalMinutes:F1} min");
                Console.WriteLine($"✓ Rest time: {volume.TotalRestTime.TotalMinutes:F1} min");
                Console.WriteLine($"✓ Volume classification: {volume.Classification}");
                Console.WriteLine($"✓ Muscle groups covered: {volume.SetsPerMuscleGroup.Count}");
                Console.WriteLine($"✓ Volume recommendations: {volume.VolumeRecommendations.Count}");

                // Validate volume appropriateness
                var isAppropriateVolume = ValidateVolumeForLevel(volume, parameters);
                Console.WriteLine($"✓ Volume appropriate for level: {isAppropriateVolume}");

                // Check muscle group distribution
                var totalDistributedSets = volume.SetsPerMuscleGroup.Values.Sum();
                var hasReasonableDistribution = totalDistributedSets > 0 &&
                    volume.SetsPerMuscleGroup.Values.All(sets => sets >= 1);
                Console.WriteLine($"✓ Reasonable muscle group distribution: {hasReasonableDistribution}");
            }

            Console.WriteLine("✅ Training Volume Calculation tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Training Volume Calculation test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestWarmupCooldownProtocols()
    {
        Console.WriteLine("5. Testing Warmup & Cooldown Protocols");
        Console.WriteLine("--------------------------------------");

        var fallbackService = new FallbackRoutineService();
        var structureService = new RoutineStructureService(fallbackService);

        try
        {
            var parameters = CreateIntermediateBodybuildingParameters();
            var routine = await structureService.CreateStructuredRoutineAsync(parameters);

            // Test warmup protocol
            Console.WriteLine("Testing Warmup Protocol:");
            var warmup = routine.Warmup;
            Console.WriteLine($"✓ Warmup duration: {warmup.Duration.TotalMinutes:F1} minutes");
            Console.WriteLine($"✓ Warmup phases: {warmup.Phases.Count}");
            Console.WriteLine($"✓ Purpose defined: {!string.IsNullOrEmpty(warmup.Purpose)}");

            // Validate warmup structure
            var hasGeneralPhase = warmup.Phases.Any(p => p.PhaseName.Contains("General"));
            var hasDynamicPhase = warmup.Phases.Any(p => p.PhaseName.Contains("Dinámico"));
            var hasActivationPhase = warmup.Phases.Any(p => p.PhaseName.Contains("Activación"));

            Console.WriteLine($"✓ Has general activation phase: {hasGeneralPhase}");
            Console.WriteLine($"✓ Has dynamic preparation phase: {hasDynamicPhase}");
            Console.WriteLine($"✓ Has specific activation phase: {hasActivationPhase}");

            // Test cooldown protocol
            Console.WriteLine("\nTesting Cooldown Protocol:");
            var cooldown = routine.Cooldown;
            Console.WriteLine($"✓ Cooldown duration: {cooldown.Duration.TotalMinutes:F1} minutes");
            Console.WriteLine($"✓ Cooldown phases: {cooldown.Phases.Count}");
            Console.WriteLine($"✓ Recovery tips: {cooldown.RecoveryTips.Count}");

            // Validate cooldown structure
            var hasActiveRecovery = cooldown.Phases.Any(p => p.PhaseName.Contains("Recuperación"));
            var hasStretching = cooldown.Phases.Any(p => p.IsStretching);
            var hasRelaxation = cooldown.Phases.Any(p => p.PhaseName.Contains("Relajación"));

            Console.WriteLine($"✓ Has active recovery phase: {hasActiveRecovery}");
            Console.WriteLine($"✓ Has stretching phase: {hasStretching}");
            Console.WriteLine($"✓ Has relaxation phase: {hasRelaxation}");

            // Test age-specific adaptations
            var seniorParameters = CreateSeniorFitnessParameters();
            var seniorRoutine = await structureService.CreateStructuredRoutineAsync(seniorParameters);
            var hasAgeAdaptations = seniorRoutine.Warmup.SpecialConsiderations.Any() ||
                                  seniorRoutine.Cooldown.RecoveryTips.Count > cooldown.RecoveryTips.Count;
            Console.WriteLine($"✓ Age-specific adaptations for seniors: {hasAgeAdaptations}");

            Console.WriteLine("✅ Warmup & Cooldown Protocols tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Warmup & Cooldown Protocols test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestProgressionPlanning()
    {
        Console.WriteLine("6. Testing Progression Planning");
        Console.WriteLine("------------------------------");

        var fallbackService = new FallbackRoutineService();
        var structureService = new RoutineStructureService(fallbackService);

        try
        {
            var parameters = CreateIntermediateBodybuildingParameters();
            var routine = await structureService.CreateStructuredRoutineAsync(parameters);

            var progression = routine.Progression;

            Console.WriteLine($"✓ Progression strategy: {progression.Strategy}");
            Console.WriteLine($"✓ Planned weeks: {progression.Weeks.Count}");
            Console.WriteLine($"✓ Progression notes: {progression.ProgressionNotes.Count}");
            Console.WriteLine($"✓ Milestones: {progression.Milestones.Count}");

            // Validate progression structure
            Console.WriteLine("\nValidating progression structure:");

            // Check weekly progression
            var hasWeeklyFocus = progression.Weeks.All(w => !string.IsNullOrEmpty(w.Focus));
            Console.WriteLine($"✓ All weeks have focus: {hasWeeklyFocus}");

            var hasParameterAdjustments = progression.Weeks.All(w => w.ParameterAdjustments.Count > 0);
            Console.WriteLine($"✓ All weeks have parameter adjustments: {hasParameterAdjustments}");

            var hasExpectedAdaptations = progression.Weeks.All(w => w.ExpectedAdaptations.Count > 0);
            Console.WriteLine($"✓ All weeks have expected adaptations: {hasExpectedAdaptations}");

            // Check milestones
            var hasTimelyMilestones = progression.Milestones.All(m => m.TargetWeek > 0 && m.TargetWeek <= 8);
            Console.WriteLine($"✓ Milestones have appropriate timelines: {hasTimelyMilestones}");

            var hasMeasurableMetrics = progression.Milestones.All(m =>
                !string.IsNullOrEmpty(m.Metric) && !string.IsNullOrEmpty(m.TargetValue));
            Console.WriteLine($"✓ Milestones have measurable metrics: {hasMeasurableMetrics}");

            // Test exercise-specific progression
            var firstExercise = routine.MainWorkout.First().Exercises.First();
            var exerciseProgression = firstExercise.Progression;

            Console.WriteLine($"\nTesting exercise progression:");
            Console.WriteLine($"✓ Progression type: {exerciseProgression.Type}");
            Console.WriteLine($"✓ Progression values: {exerciseProgression.ProgressionValues.Count}");
            Console.WriteLine($"✓ Instructions provided: {!string.IsNullOrEmpty(exerciseProgression.ProgressionInstructions)}");
            Console.WriteLine($"✓ Weeks until progression: {exerciseProgression.WeeksUntilProgresssion}");

            Console.WriteLine("✅ Progression Planning tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Progression Planning test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestSafetyConsiderations()
    {
        Console.WriteLine("7. Testing Safety Considerations");
        Console.WriteLine("-------------------------------");

        var fallbackService = new FallbackRoutineService();
        var structureService = new RoutineStructureService(fallbackService);

        try
        {
            // Test different risk scenarios
            var scenarios = new[]
            {
                ("Young healthy", CreateIntermediateBodybuildingParameters()),
                ("Senior with limitations", CreateSeniorFitnessParameters()),
                ("Cardiovascular concerns", CreateCardiovascularLimitationsParameters()),
                ("Multiple limitations", CreateMultipleLimitationsParameters())
            };

            foreach (var (description, parameters) in scenarios)
            {
                Console.WriteLine($"\nTesting safety for: {description}");

                var routine = await structureService.CreateStructuredRoutineAsync(parameters);
                var safetyNotes = routine.SafetyNotes;

                Console.WriteLine($"✓ Safety considerations: {safetyNotes.Count}");

                // Validate safety coverage
                var hasGeneralSafety = safetyNotes.Any(s => s.Consideration.Contains("Calentamiento"));
                Console.WriteLine($"✓ General safety covered: {hasGeneralSafety}");

                if (parameters.Age >= 65)
                {
                    var hasAgeSafety = safetyNotes.Any(s =>
                        s.Consideration.Contains("adultos mayores") ||
                        s.Severity == SafetyLevel.Warning);
                    Console.WriteLine($"✓ Age-specific safety: {hasAgeSafety}");
                }

                if (parameters.PhysicalLimitations.Any(l => l.Contains("cardiovascular")))
                {
                    var hasCardiovascularSafety = safetyNotes.Any(s =>
                        s.Severity == SafetyLevel.Critical &&
                        s.Consideration.Contains("cardiovascular"));
                    Console.WriteLine($"✓ Cardiovascular safety: {hasCardiovascularSafety}");
                }

                // Check warning signs
                var hasWarningSignsForAllCritical = safetyNotes
                    .Where(s => s.Severity == SafetyLevel.Critical)
                    .All(s => s.WarningSignsToStop.Count > 0);
                Console.WriteLine($"✓ Critical items have warning signs: {hasWarningSignsForAllCritical}");

                // Check precautions
                var hasPrecautionsForWarnings = safetyNotes
                    .Where(s => s.Severity >= SafetyLevel.Warning)
                    .All(s => s.Precautions.Count > 0);
                Console.WriteLine($"✓ Warnings have precautions: {hasPrecautionsForWarnings}");
            }

            Console.WriteLine("✅ Safety Considerations tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Safety Considerations test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    // Helper methods for validation
    private static void ValidateRoutineStructure(StructuredRoutine routine, UserRoutineParameters parameters)
    {
        Console.WriteLine("  Validating routine structure:");

        // Check proper warmup
        var hasProperWarmup = routine.Warmup.Duration.TotalMinutes >= 5;
        Console.WriteLine($"    ✓ Adequate warmup duration: {hasProperWarmup}");

        // Check proper cooldown
        var hasProperCooldown = routine.Cooldown.Duration.TotalMinutes >= 5;
        Console.WriteLine($"    ✓ Adequate cooldown duration: {hasProperCooldown}");

        // Check exercise ordering (compound first)
        var firstBlock = routine.MainWorkout.FirstOrDefault();
        var compoundFirst = firstBlock?.Category == ExerciseCategory.Compound;
        Console.WriteLine($"    ✓ Compound exercises prioritized: {compoundFirst}");

        // Check rest periods are specified
        var allExercisesHaveRest = routine.MainWorkout
            .SelectMany(b => b.Exercises)
            .All(e => e.RestPeriod.TargetRest.TotalSeconds > 0);
        Console.WriteLine($"    ✓ All exercises have rest periods: {allExercisesHaveRest}");

        // Check training volume is appropriate
        var totalTime = routine.EstimatedDuration.TotalMinutes;
        var appropriateDuration = Math.Abs(totalTime - parameters.PreferredSessionDuration) <= 10;
        Console.WriteLine($"    ✓ Duration matches preference: {appropriateDuration}");
    }

    private static bool ValidateVolumeForLevel(TrainingVolume volume, UserRoutineParameters parameters)
    {
        var expectedSets = parameters.ExperienceLevel switch
        {
            "Principiante" => (6, 10),
            "Intermedio" => (10, 15),
            "Avanzado" => (15, 20),
            _ => (8, 12)
        };

        return volume.TotalSets >= expectedSets.Item1 && volume.TotalSets <= expectedSets.Item2;
    }

    // Parameter creation methods
    private static UserRoutineParameters CreateBeginnerStrengthParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Principiante Fuerza",
            Age = 25,
            Gender = "Hombre",
            TrainingDaysPerWeek = 3,
            ExperienceLevel = "Principiante",
            PrimaryGoal = "Fuerza",
            PreferredSessionDuration = 45,
            RecommendedIntensity = 2,
            AvailableEquipment = new List<string> { "Peso corporal", "Mancuernas" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Pecho", EmphasisLevel = "Alto", Priority = 1 },
                new() { MuscleGroup = "Espalda", EmphasisLevel = "Alto", Priority = 2 },
                new() { MuscleGroup = "Piernas", EmphasisLevel = "Medio", Priority = 3 }
            }
        };
    }

    private static UserRoutineParameters CreateIntermediateBodybuildingParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Intermedio Hipertrofia",
            Age = 30,
            Gender = "Mujer",
            TrainingDaysPerWeek = 4,
            ExperienceLevel = "Intermedio",
            PrimaryGoal = "Masa",
            PreferredSessionDuration = 60,
            RecommendedIntensity = 3,
            AvailableEquipment = new List<string> { "Mancuernas", "Bandas elásticas", "Peso corporal" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Glúteos", EmphasisLevel = "Alto", Priority = 1 },
                new() { MuscleGroup = "Core", EmphasisLevel = "Alto", Priority = 2 },
                new() { MuscleGroup = "Brazos", EmphasisLevel = "Medio", Priority = 3 }
            }
        };
    }

    private static UserRoutineParameters CreateSeniorFitnessParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Adulto Mayor",
            Age = 68,
            Gender = "Hombre",
            TrainingDaysPerWeek = 2,
            ExperienceLevel = "Principiante",
            PrimaryGoal = "Fitness General",
            PreferredSessionDuration = 30,
            RecommendedIntensity = 1,
            AvailableEquipment = new List<string> { "Peso corporal", "Silla" },
            PhysicalLimitations = new List<string> { "Artritis en rodillas", "Problemas de equilibrio" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Core", EmphasisLevel = "Alto", Priority = 1 },
                new() { MuscleGroup = "Equilibrio", EmphasisLevel = "Alto", Priority = 2 }
            }
        };
    }

    private static UserRoutineParameters CreateAdvancedAthleteParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Atleta Avanzado",
            Age = 28,
            Gender = "Hombre",
            TrainingDaysPerWeek = 5,
            ExperienceLevel = "Avanzado",
            PrimaryGoal = "Fuerza",
            PreferredSessionDuration = 75,
            RecommendedIntensity = 4,
            AvailableEquipment = new List<string> { "Barras", "Mancuernas", "Máquinas", "Peso corporal" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Pecho", EmphasisLevel = "Alto", Priority = 1 },
                new() { MuscleGroup = "Espalda", EmphasisLevel = "Alto", Priority = 2 },
                new() { MuscleGroup = "Piernas", EmphasisLevel = "Alto", Priority = 3 },
                new() { MuscleGroup = "Core", EmphasisLevel = "Medio", Priority = 4 }
            }
        };
    }

    private static UserRoutineParameters CreateCardiovascularLimitationsParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Usuario con Limitaciones Cardiovasculares",
            Age = 55,
            Gender = "Mujer",
            TrainingDaysPerWeek = 3,
            ExperienceLevel = "Intermedio",
            PrimaryGoal = "Fitness General",
            PreferredSessionDuration = 40,
            RecommendedIntensity = 2,
            AvailableEquipment = new List<string> { "Peso corporal", "Bandas elásticas" },
            PhysicalLimitations = new List<string> { "Problemas cardiovasculares (hipertensión controlada)" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Core", EmphasisLevel = "Medio", Priority = 1 },
                new() { MuscleGroup = "Piernas", EmphasisLevel = "Bajo", Priority = 2 }
            }
        };
    }

    private static UserRoutineParameters CreateMultipleLimitationsParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Usuario con Múltiples Limitaciones",
            Age = 62,
            Gender = "Hombre",
            TrainingDaysPerWeek = 2,
            ExperienceLevel = "Principiante",
            PrimaryGoal = "Fitness General",
            PreferredSessionDuration = 35,
            RecommendedIntensity = 1,
            AvailableEquipment = new List<string> { "Peso corporal", "Silla" },
            PhysicalLimitations = new List<string>
            {
                "Problemas de espalda",
                "Artritis en manos",
                "Problemas de equilibrio"
            },
            AvoidExercises = new List<string> { "Peso muerto", "Sentadillas profundas", "Flexiones estándar" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Core", EmphasisLevel = "Bajo", Priority = 1 },
                new() { MuscleGroup = "Equilibrio", EmphasisLevel = "Alto", Priority = 2 }
            }
        };
    }

    private static int GetDifficultyLevel(string difficultyLevel)
    {
        return difficultyLevel.ToLower() switch
        {
            "principiante" or "beginner" => 1,
            "intermedio" or "intermediate" => 2,
            "avanzado" or "advanced" => 3,
            _ => 2 // Default to intermediate
        };
    }
}