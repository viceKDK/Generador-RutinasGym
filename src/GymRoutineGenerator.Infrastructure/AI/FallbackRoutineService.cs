using System.Text;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Infrastructure.AI;

public class FallbackRoutineService : IFallbackRoutineService
{
    private readonly Dictionary<string, List<Exercise>> _exerciseDatabase;

    public FallbackRoutineService()
    {
        _exerciseDatabase = InitializeExerciseDatabase();
    }

    public async Task<string> GenerateRuleBasedRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var routine = new StringBuilder();

        // Header
        routine.AppendLine("📋 **RUTINA DE ENTRENAMIENTO PERSONALIZADA**");
        routine.AppendLine("*(Generada por algoritmo de respaldo)*");
        routine.AppendLine();

        // User Summary
        routine.AppendLine("👤 **RESUMEN DEL CLIENTE**");
        routine.AppendLine($"- Nombre: {parameters.Name}");
        routine.AppendLine($"- Perfil: {parameters.Age} años, {parameters.ExperienceLevel}, {parameters.PrimaryGoal}");
        routine.AppendLine($"- Frecuencia: {parameters.TrainingDaysPerWeek} días/semana, {parameters.PreferredSessionDuration} min/sesión");
        routine.AppendLine();

        // Goals
        routine.AppendLine("🎯 **OBJETIVOS DE LA RUTINA**");
        routine.AppendLine($"- Objetivo principal: {parameters.PrimaryGoal}");
        routine.AppendLine($"- Enfoque muscular: {string.Join(", ", parameters.MuscleGroupPreferences.Take(3).Select(mg => mg.MuscleGroup))}");
        if (parameters.PhysicalLimitations.Any())
        {
            routine.AppendLine($"- Adaptaciones especiales: {string.Join(", ", parameters.PhysicalLimitations)}");
        }
        routine.AppendLine();

        // Warmup
        var warmupExercises = GetWarmupExercises(parameters);
        routine.AppendLine("🔥 **CALENTAMIENTO** (5-8 min)");
        foreach (var exercise in warmupExercises)
        {
            routine.AppendLine($"• {exercise.Name} - {exercise.Description}");
        }
        routine.AppendLine();

        // Main exercises
        var mainExercises = await GetRecommendedExercisesAsync(parameters, cancellationToken);
        routine.AppendLine("💪 **EJERCICIOS PRINCIPALES**");

        for (int i = 0; i < mainExercises.Count; i++)
        {
            var exercise = mainExercises[i];
            routine.AppendLine($"**{i + 1}. {exercise.Name}**");
            routine.AppendLine($"   - Músculos: {string.Join(", ", exercise.MuscleGroups)}");
            routine.AppendLine($"   - Series: {exercise.RecommendedSets} x Reps: {exercise.RecommendedReps}");
            routine.AppendLine($"   - Descanso: {exercise.RestPeriod}");
            routine.AppendLine($"   - Técnica: {exercise.Description}");

            if (exercise.Modifications.Any())
            {
                routine.AppendLine($"   - Modificaciones: {string.Join(", ", exercise.Modifications)}");
            }

            if (exercise.SafetyNotes.Any())
            {
                routine.AppendLine($"   - Seguridad: {string.Join(", ", exercise.SafetyNotes)}");
            }
            routine.AppendLine();
        }

        // Cooldown
        var cooldownExercises = GetCooldownExercises(parameters);
        routine.AppendLine("🧘 **ENFRIAMIENTO** (5-8 min)");
        foreach (var exercise in cooldownExercises)
        {
            routine.AppendLine($"• {exercise.Name} - {exercise.Description}");
        }
        routine.AppendLine();

        // Progression
        routine.AppendLine("📊 **PROGRESIÓN SEMANAL**");
        routine.AppendLine(GetProgressionPlan(parameters));
        routine.AppendLine();

        // Safety tips
        routine.AppendLine("⚠️ **CONSEJOS DE SEGURIDAD**");
        routine.AppendLine(GetSafetyAdvice(parameters));
        routine.AppendLine();

        // Additional tips
        routine.AppendLine("💡 **CONSEJOS ADICIONALES**");
        routine.AppendLine("- Mantén una hidratación constante durante el entrenamiento");
        routine.AppendLine("- Escucha a tu cuerpo y ajusta la intensidad según sea necesario");
        routine.AppendLine("- Asegúrate de dormir 7-8 horas para una recuperación óptima");
        routine.AppendLine("- Considera incorporar una alimentación balanceada");
        routine.AppendLine();

        return routine.ToString();
    }

    public async Task<List<Exercise>> GetRecommendedExercisesAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var recommendedExercises = new List<Exercise>();
        var availableExercises = FilterExercisesByEquipment(parameters.AvailableEquipment);
        availableExercises = FilterExercisesByLimitations(availableExercises, parameters.PhysicalLimitations);

        // Calculate number of exercises based on session duration and training days
        int targetExercises = CalculateTargetExerciseCount(parameters);

        // Priority-based exercise selection
        var muscleGroupPriorities = parameters.MuscleGroupPreferences
            .OrderBy(mg => mg.Priority)
            .ToList();

        // Always include compound movements first
        var compoundExercises = availableExercises
            .Where(e => e.ExerciseType == ExerciseType.Compound)
            .Where(e => IsExerciseAppropriate(e, parameters))
            .Take(Math.Max(1, targetExercises / 3))
            .ToList();

        recommendedExercises.AddRange(compoundExercises);

        // Add exercises for priority muscle groups
        foreach (var priorityMuscle in muscleGroupPriorities.Take(targetExercises - compoundExercises.Count))
        {
            var muscleExercises = availableExercises
                .Where(e => e.MuscleGroups.Any(mg => mg.Contains(priorityMuscle.MuscleGroup, StringComparison.OrdinalIgnoreCase)))
                .Where(e => !recommendedExercises.Contains(e))
                .Where(e => IsExerciseAppropriate(e, parameters))
                .OrderBy(e => e.DifficultyLevel)
                .Take(1);

            recommendedExercises.AddRange(muscleExercises);
        }

        // Fill remaining slots with balanced exercises
        while (recommendedExercises.Count < targetExercises)
        {
            var remainingExercises = availableExercises
                .Where(e => !recommendedExercises.Contains(e))
                .Where(e => IsExerciseAppropriate(e, parameters))
                .OrderBy(e => e.DifficultyLevel);

            var nextExercise = remainingExercises.FirstOrDefault();
            if (nextExercise != null)
            {
                recommendedExercises.Add(nextExercise);
            }
            else
            {
                break;
            }
        }

        // Set parameters for each exercise
        foreach (var exercise in recommendedExercises)
        {
            SetExerciseParameters(exercise, parameters);
        }

        return recommendedExercises;
    }

    public async Task<string> GenerateBasicRoutineTemplateAsync(string templateType, UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return templateType.ToLower() switch
        {
            "beginner" => GenerateBeginnerTemplate(parameters),
            "intermediate" => GenerateIntermediateTemplate(parameters),
            "senior" => GenerateSeniorTemplate(parameters),
            "home" => GenerateHomeWorkoutTemplate(parameters),
            _ => await GenerateRuleBasedRoutineAsync(parameters, cancellationToken)
        };
    }

    private Dictionary<string, List<Exercise>> InitializeExerciseDatabase()
    {
        return new Dictionary<string, List<Exercise>>
        {
            ["bodyweight"] = new List<Exercise>
            {
                new Exercise
                {
                    Name = "Flexiones de pecho",
                    Description = "Mantén el cuerpo recto, baja controladamente hasta que el pecho casi toque el suelo",
                    MuscleGroups = new List<string> { "Pecho", "Tríceps", "Hombros" },
                    Equipment = "Peso corporal",
                    DifficultyLevel = "Avanzado",
                    Type = "Compound",
                    ExerciseType = ExerciseType.Compound,
                    Modifications = new List<string> { "Flexiones en rodillas", "Flexiones inclinadas" },
                    SafetyNotes = new List<string> { "Mantener core activado", "No arquear la espalda" }
                },
                new Exercise
                {
                    Name = "Sentadillas",
                    Description = "Pies al ancho de hombros, baja como si te sentaras en una silla",
                    MuscleGroups = new List<string> { "Cuádriceps", "Glúteos", "Core" },
                    Equipment = "Peso corporal",
                    DifficultyLevel = "Intermedio",
                    Type = "Compound",
                    ExerciseType = ExerciseType.Compound,
                    Modifications = new List<string> { "Sentadillas en silla", "Sentadillas poco profundas" },
                    SafetyNotes = new List<string> { "Rodillas alineadas con pies", "No sobrepasar los dedos de los pies" }
                },
                new Exercise
                {
                    Name = "Plancha",
                    Description = "Mantén el cuerpo recto desde cabeza hasta talones",
                    MuscleGroups = new List<string> { "Core", "Hombros" },
                    Equipment = "Peso corporal",
                    DifficultyLevel = "Avanzado",
                    Type = "Core",
                    ExerciseType = ExerciseType.Core,
                    Modifications = new List<string> { "Plancha en rodillas", "Plancha en antebrazos" },
                    SafetyNotes = new List<string> { "No hundir caderas", "Respiración constante" }
                }
            },
            ["resistance_bands"] = new List<Exercise>
            {
                new Exercise
                {
                    Name = "Remo con banda",
                    Description = "Tira de la banda hacia el pecho manteniendo los codos cerca del cuerpo",
                    MuscleGroups = new List<string> { "Espalda", "Bíceps" },
                    Equipment = "Bandas elásticas",
                    DifficultyLevel = "Intermedio",
                    Type = "Compound",
                    ExerciseType = ExerciseType.Compound,
                    Modifications = new List<string> { "Ajustar tensión de banda", "Cambiar agarre" },
                    SafetyNotes = new List<string> { "Controlar el retorno", "Mantener postura erguida" }
                }
            },
            ["weights"] = new List<Exercise>
            {
                new Exercise
                {
                    Name = "Press de pecho con mancuernas",
                    Description = "Acostado, baja las mancuernas controladamente hasta el pecho",
                    MuscleGroups = new List<string> { "Pecho", "Tríceps", "Hombros" },
                    Equipment = "Mancuernas",
                    DifficultyLevel = "Avanzado",
                    Type = "Compound",
                    ExerciseType = ExerciseType.Compound,
                    Modifications = new List<string> { "Reducir peso", "Rango parcial" },
                    SafetyNotes = new List<string> { "Control total del peso", "Usar spotteador si es necesario" }
                }
            }
        };
    }

    private List<Exercise> FilterExercisesByEquipment(List<string> availableEquipment)
    {
        var allExercises = new List<Exercise>();

        // Always include bodyweight exercises
        allExercises.AddRange(_exerciseDatabase["bodyweight"]);

        // Add equipment-specific exercises
        foreach (var equipment in availableEquipment)
        {
            var equipmentKey = equipment.ToLower() switch
            {
                var x when x.Contains("banda") => "resistance_bands",
                var x when x.Contains("mancuerna") || x.Contains("pesa") => "weights",
                _ => null
            };

            if (equipmentKey != null && _exerciseDatabase.ContainsKey(equipmentKey))
            {
                allExercises.AddRange(_exerciseDatabase[equipmentKey]);
            }
        }

        return allExercises.Distinct().ToList();
    }

    private List<Exercise> FilterExercisesByLimitations(List<Exercise> exercises, List<string> limitations)
    {
        foreach (var limitation in limitations)
        {
            var limitationLower = limitation.ToLower();

            exercises = exercises.Where(e =>
            {
                if (limitationLower.Contains("espalda") && e.Name.ToLower().Contains("peso muerto"))
                    return false;
                if (limitationLower.Contains("rodilla") && e.Name.ToLower().Contains("sentadilla"))
                    return false;
                if (limitationLower.Contains("hombro") && e.Name.ToLower().Contains("press"))
                    return false;
                return true;
            }).ToList();
        }

        return exercises;
    }

    private bool IsExerciseAppropriate(Exercise exercise, UserRoutineParameters parameters)
    {
        // Check difficulty level - convert string to numeric for comparison
        var exerciseDifficulty = GetDifficultyLevel(exercise.DifficultyLevel);
        if (exerciseDifficulty > parameters.RecommendedIntensity + 1)
            return false;

        // Check if exercise should be avoided
        if (parameters.AvoidExercises.Any(avoid =>
            exercise.Name.Contains(avoid, StringComparison.OrdinalIgnoreCase)))
            return false;

        return true;
    }

    private void SetExerciseParameters(Exercise exercise, UserRoutineParameters parameters)
    {
        // Set sets based on experience level
        exercise.RecommendedSets = parameters.ExperienceLevel switch
        {
            "Principiante" => 2,
            "Intermedio" => 3,
            "Avanzado" => 4,
            _ => 3
        };

        // Set reps based on goal
        exercise.RecommendedReps = parameters.PrimaryGoal switch
        {
            "Fuerza" => "4-6",
            "Masa" => "8-12",
            "Resistencia" => "15-20",
            "Pérdida de peso" => "10-15",
            _ => "8-12"
        };

        // Set rest period
        exercise.RestPeriod = parameters.PrimaryGoal switch
        {
            "Fuerza" => "2-3 min",
            "Masa" => "60-90 seg",
            "Resistencia" => "30-45 seg",
            "Pérdida de peso" => "45-60 seg",
            _ => "60-90 seg"
        };
    }

    private int CalculateTargetExerciseCount(UserRoutineParameters parameters)
    {
        var baseCount = parameters.PreferredSessionDuration switch
        {
            <= 30 => 4,
            <= 45 => 6,
            <= 60 => 8,
            _ => 10
        };

        // Adjust for experience level
        var adjustment = parameters.ExperienceLevel switch
        {
            "Principiante" => -1,
            "Avanzado" => +1,
            _ => 0
        };

        return Math.Max(3, Math.Min(12, baseCount + adjustment));
    }

    private List<Exercise> GetWarmupExercises(UserRoutineParameters parameters)
    {
        var warmupExercises = new List<Exercise>
        {
            new Exercise { Name = "Marcha en el lugar", Description = "2-3 minutos, aumentar gradualmente el ritmo" },
            new Exercise { Name = "Círculos de brazos", Description = "10 hacia adelante, 10 hacia atrás" },
            new Exercise { Name = "Rotaciones de cadera", Description = "10 en cada dirección" },
            new Exercise { Name = "Estiramientos dinámicos", Description = "Piernas y brazos, movimientos controlados" }
        };

        // Add age-specific warmup
        if (parameters.Age >= 50)
        {
            warmupExercises.Add(new Exercise
            {
                Name = "Equilibrio en un pie",
                Description = "30 segundos cada pie, usar apoyo si es necesario"
            });
        }

        return warmupExercises.Take(4).ToList();
    }

    private List<Exercise> GetCooldownExercises(UserRoutineParameters parameters)
    {
        return new List<Exercise>
        {
            new Exercise { Name = "Caminata suave", Description = "2-3 minutos para reducir frecuencia cardíaca" },
            new Exercise { Name = "Estiramiento de pecho", Description = "30 segundos, brazos contra la pared" },
            new Exercise { Name = "Estiramiento de piernas", Description = "30 segundos cada grupo muscular" },
            new Exercise { Name = "Respiración profunda", Description = "5 respiraciones lentas y profundas" }
        };
    }

    private string GetProgressionPlan(UserRoutineParameters parameters)
    {
        return parameters.ExperienceLevel switch
        {
            "Principiante" => @"- Semana 1-2: Enfócate en la técnica correcta, 2 series por ejercicio
- Semana 3-4: Aumenta a 3 series, mantén las repeticiones
- Semana 5+: Aumenta repeticiones gradualmente o añade resistencia",

            "Intermedio" => @"- Semana 1-2: Establece base con pesos/resistencia actual
- Semana 3-4: Aumenta peso/resistencia en 5-10%
- Semana 5+: Continúa progresión o añade variaciones de ejercicios",

            _ => @"- Semana 1-2: Parámetros iniciales establecidos
- Semana 3-4: Aumenta intensidad gradualmente
- Semana 5+: Progresión basada en capacidad individual"
        };
    }

    private string GetSafetyAdvice(UserRoutineParameters parameters)
    {
        var advice = new List<string>
        {
            "Siempre calentar antes de ejercicios intensos",
            "Parar inmediatamente si sientes dolor agudo",
            "Mantener técnica correcta antes que intensidad",
            "Hidratarse antes, durante y después del ejercicio"
        };

        // Add age-specific advice
        if (parameters.Age >= 65)
        {
            advice.Add("Realizar cambios de posición lentamente");
            advice.Add("Tener un punto de apoyo cerca durante ejercicios de equilibrio");
        }

        // Add limitation-specific advice
        foreach (var limitation in parameters.PhysicalLimitations)
        {
            if (limitation.Contains("cardiovascular"))
            {
                advice.Add("Monitorear frecuencia cardíaca durante el ejercicio");
            }
            if (limitation.Contains("espalda"))
            {
                advice.Add("Evitar flexiones extremas de columna");
            }
        }

        return string.Join("\n", advice.Select(a => $"- {a}"));
    }

    private string GenerateBeginnerTemplate(UserRoutineParameters parameters)
    {
        return $@"📋 **RUTINA PARA PRINCIPIANTES**

🎯 **OBJETIVO**: Establecer base fitness y aprender técnica correcta

💪 **RUTINA COMPLETA** (3 días/semana)

**CALENTAMIENTO** (5 min)
• Marcha en el lugar - 2 min
• Círculos de brazos - 1 min
• Movimientos suaves - 2 min

**EJERCICIOS PRINCIPALES**
1. **Sentadillas** - 2 series x 8-10 reps
2. **Flexiones** (modificadas si es necesario) - 2 series x 5-8 reps
3. **Plancha** - 2 series x 15-30 seg
4. **Marcha con rodillas al pecho** - 2 series x 10 cada pierna

**ENFRIAMIENTO** (5 min)
• Estiramientos suaves
• Respiración profunda

⚠️ **ENFOQUE**: Técnica perfecta antes que intensidad";
    }

    private string GenerateIntermediateTemplate(UserRoutineParameters parameters)
    {
        return $@"📋 **RUTINA INTERMEDIA**

🎯 **OBJETIVO**: Desarrollo muscular equilibrado y progresión

💪 **RUTINA DIVIDIDA** ({parameters.TrainingDaysPerWeek} días/semana)

**DÍA A - TREN SUPERIOR**
1. Flexiones - 3x8-12
2. Remo (banda/peso) - 3x10-12
3. Press vertical - 3x8-10
4. Plancha lateral - 3x20-30 seg

**DÍA B - TREN INFERIOR**
1. Sentadillas - 3x10-12
2. Lunges - 3x8 cada pierna
3. Puente de glúteos - 3x12-15
4. Pantorrillas - 3x15-20

📊 **PROGRESIÓN**: Aumentar intensidad semanalmente";
    }

    private string GenerateSeniorTemplate(UserRoutineParameters parameters)
    {
        return $@"📋 **RUTINA PARA ADULTOS MAYORES**

🎯 **OBJETIVO**: Mantener movilidad, fuerza funcional y equilibrio

💪 **RUTINA SUAVE** (2-3 días/semana)

**CALENTAMIENTO EXTENDIDO** (8 min)
• Movimientos articulares suaves
• Activación gradual

**EJERCICIOS FUNCIONALES**
1. **Sentarse y levantarse** - 2 series x 5-8 reps
2. **Flexiones de pared** - 2 series x 8-10 reps
3. **Equilibrio en un pie** - 2 series x 20-30 seg
4. **Marcha en el lugar** - 2 series x 30 seg
5. **Estiramientos dinámicos** - 5 min

**ENFRIAMIENTO** (10 min)
• Estiramientos prolongados
• Ejercicios de respiración

⚠️ **PRIORIDAD**: Seguridad y mantenimiento de independencia";
    }

    private string GenerateHomeWorkoutTemplate(UserRoutineParameters parameters)
    {
        return $@"📋 **RUTINA EN CASA**

🎯 **OBJETIVO**: Entrenamiento efectivo sin equipamiento especial

💪 **RUTINA CORPORAL** (Sin equipamiento)

**EJERCICIOS PRINCIPALES**
1. **Sentadillas** - 3x10-15
2. **Flexiones** - 3x8-12
3. **Lunges** - 3x8 cada pierna
4. **Plancha** - 3x20-45 seg
5. **Burpees** (opcional) - 2x5-8
6. **Mountain climbers** - 3x15 cada pierna

**VARIACIONES DISPONIBLES**
• Usar silla para apoyo
• Modificar intensidad según nivel
• Añadir bandas elásticas si disponibles

🏠 **VENTAJA**: Flexibilidad total de horarios";
    }

    private int GetDifficultyLevel(string difficultyLevel)
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