using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.AI;
using GymRoutineGenerator.Core.Services.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GymRoutineGenerator.Infrastructure.AI;

public class FallbackService : IFallbackService
{
    private readonly IErrorHandlingService _errorService;
    private readonly ILogger<FallbackService> _logger;
    private readonly HttpClient _httpClient;
    private bool _isAIAvailable;
    private DateTime _lastAICheck;

    private readonly Dictionary<string, List<string>> _basicExerciseTemplates = new()
    {
        ["fuerza"] = new List<string>
        {
            "Press de Banca", "Sentadillas", "Peso Muerto", "Press Militar",
            "Dominadas", "Remo con Barra", "Fondos", "Curl de Bíceps"
        },
        ["resistencia"] = new List<string>
        {
            "Burpees", "Mountain Climbers", "Jumping Jacks", "High Knees",
            "Planchas", "Abdominales", "Lunges", "Step-ups"
        },
        ["flexibilidad"] = new List<string>
        {
            "Estiramiento de Isquiotibiales", "Estiramiento de Cuádriceps",
            "Estiramiento de Hombros", "Gato-Camello", "Postura del Niño",
            "Estiramiento de Pantorrillas", "Rotación de Cadera"
        },
        ["rehabilitacion"] = new List<string>
        {
            "Ejercicios Isométricos", "Movilidad Articular", "Ejercicios de Equilibrio",
            "Fortalecimiento Suave", "Estiramientos Suaves", "Caminar", "Natación Suave"
        }
    };

    public FallbackService(IErrorHandlingService errorService, ILogger<FallbackService>? logger = null)
    {
        _errorService = errorService;
        _logger = logger ?? CreateDefaultLogger();
        _httpClient = new HttpClient();
        _isAIAvailable = false;
        _lastAICheck = DateTime.MinValue;
    }

    public async Task<bool> IsAIServiceAvailableAsync()
    {
        // Cache el resultado por 5 minutos para evitar checks constantesç
        if (DateTime.Now - _lastAICheck < TimeSpan.FromMinutes(5))
        {
            return _isAIAvailable;
        }

        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
            var response = await _httpClient.GetAsync("http://localhost:11434/api/version");
            _isAIAvailable = response.IsSuccessStatusCode;

            if (!_isAIAvailable)
            {
                _errorService.EnableGracefulDegradation("Ollama", true);
                await _errorService.LogInfoAsync("AI service unavailable, enabling fallback mode", "FallbackService");
            }
            else
            {
                _errorService.EnableGracefulDegradation("Ollama", false);
                await _errorService.LogInfoAsync("AI service available", "FallbackService");
            }
        }
        catch (Exception ex)
        {
            _isAIAvailable = false;
            _errorService.EnableGracefulDegradation("Ollama", true);
            await _errorService.HandleErrorAsync(ex, "AIServiceCheck");
        }
        finally
        {
            _lastAICheck = DateTime.Now;
        }

        return _isAIAvailable;
    }

    public async Task<Routine> GenerateBasicRoutineAsync(string clientName, string goal, int durationWeeks)
    {
        await _errorService.LogInfoAsync($"Generating basic routine for {clientName} with goal: {goal}", "FallbackService");

        var routine = new Routine
        {
            Id = new Random().Next(1000, 9999),
            Name = $"Rutina Básica - {goal.ToTitleCase()}",
            ClientName = clientName,
            Description = GenerateBasicDescription(goal),
            Goal = goal,
            DurationWeeks = durationWeeks,
            CreatedDate = DateTime.Now
        };

        // Generar días basados en el objetivo
        var exerciseList = GetExercisesForGoal(goal);
        var daysToGenerate = goal.ToLower().Contains("fuerza") ? 4 : 3;

        for (int i = 1; i <= daysToGenerate; i++)
        {
            var day = GenerateBasicDay(i, exerciseList, goal);
            routine.Days.Add(day);
        }

        // Calcular métricas
        routine.Metrics = CalculateRoutineMetrics(routine);

        await _errorService.LogInfoAsync($"Generated basic routine with {routine.Days.Count} days, {routine.Metrics.TotalExercises} exercises", "FallbackService");

        return routine;
    }

    public async Task<List<string>> GetBasicRecommendationsAsync(string goal)
    {
        var recommendations = goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => new List<string>
            {
                "💪 Enfócate en ejercicios compuestos (sentadillas, peso muerto, press)",
                "⚖️ Incrementa el peso progresivamente cada semana",
                "😴 Descansa 48-72 horas entre entrenamientos del mismo grupo muscular",
                "🥩 Consume suficiente proteína (1.6-2.2g por kg de peso corporal)",
                "💧 Mantente bien hidratado durante todo el entrenamiento"
            },
            var g when g.Contains("resistencia") => new List<string>
            {
                "⏰ Mantén intervalos de descanso cortos (30-60 segundos)",
                "🔥 Incrementa la intensidad gradualmente cada semana",
                "💓 Monitorea tu frecuencia cardíaca durante el ejercicio",
                "🥤 Bebe líquidos antes, durante y después del ejercicio",
                "🧘 Incluye días de recuperación activa con ejercicios suaves"
            },
            var g when g.Contains("flexibilidad") => new List<string>
            {
                "🌅 Realiza estiramientos cuando los músculos estén calientes",
                "⏳ Mantén cada estiramiento por al menos 30 segundos",
                "🧘 Respira profundamente durante los estiramientos",
                "🔄 Realiza los ejercicios de forma consistente, preferiblemente diario",
                "⚠️ No fuerces los estiramientos hasta sentir dolor"
            },
            var g when g.Contains("rehabilitacion") => new List<string>
            {
                "👩‍⚕️ Sigue siempre las indicaciones de tu profesional de la salud",
                "🐌 Progresa lentamente, escucha a tu cuerpo",
                "🧊 Aplica hielo si hay inflamación, calor si hay rigidez",
                "📝 Registra tu progreso y cualquier molestia",
                "⏸️ Detente si sientes dolor agudo o empeoramiento"
            },
            _ => new List<string>
            {
                "🎯 Establece objetivos realistas y específicos",
                "📅 Mantén consistencia en tu rutina de ejercicios",
                "🥗 Combina ejercicio con una alimentación balanceada",
                "😴 Asegúrate de dormir 7-9 horas por noche",
                "💪 Escucha a tu cuerpo y descansa cuando sea necesario"
            }
        };

        await _errorService.LogInfoAsync($"Generated {recommendations.Count} basic recommendations for goal: {goal}", "FallbackService");
        return recommendations;
    }

    public async Task<Routine> ModifyRoutineBasicAsync(Routine routine, string modification)
    {
        await _errorService.LogInfoAsync($"Modifying routine {routine.Name} with: {modification}", "FallbackService");

        var modifiedRoutine = routine.Clone();

        // Modificaciones básicas basadas en palabras clave
        if (modification.ToLower().Contains("intensidad") || modification.ToLower().Contains("dificil"))
        {
            IncreaseIntensity(modifiedRoutine);
        }
        else if (modification.ToLower().Contains("facil") || modification.ToLower().Contains("reducir"))
        {
            DecreaseIntensity(modifiedRoutine);
        }
        else if (modification.ToLower().Contains("tiempo") || modification.ToLower().Contains("rapido"))
        {
            ReduceRestTimes(modifiedRoutine);
        }
        else if (modification.ToLower().Contains("dia") || modification.ToLower().Contains("añadir"))
        {
            if (modifiedRoutine.Days.Count < 6)
            {
                AddBasicDay(modifiedRoutine);
            }
        }

        modifiedRoutine.Metrics = CalculateRoutineMetrics(modifiedRoutine);
        modifiedRoutine.Description += $" | Modificado: {modification}";

        return modifiedRoutine;
    }

    #region Private Methods

    private string GenerateBasicDescription(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("fuerza") =>
                "Rutina básica de fuerza diseñada para desarrollar masa muscular y potencia. " +
                "Incluye ejercicios compuestos fundamentales con progresión gradual.",
            var g when g.Contains("resistencia") =>
                "Rutina básica de resistencia para mejorar la capacidad cardiovascular y la resistencia muscular. " +
                "Combina ejercicios aeróbicos con intervalos de alta intensidad.",
            var g when g.Contains("flexibilidad") =>
                "Rutina básica de flexibilidad y movilidad para mejorar el rango de movimiento articular. " +
                "Incluye estiramientos estáticos y dinámicos para todo el cuerpo.",
            var g when g.Contains("rehabilitacion") =>
                "Rutina básica de rehabilitación con ejercicios suaves y progresivos. " +
                "Diseñada para recuperación segura y fortalecimiento gradual.",
            _ =>
                "Rutina básica de acondicionamiento físico general. " +
                "Combina fuerza, resistencia y flexibilidad para un entrenamiento completo."
        };
    }

    private List<string> GetExercisesForGoal(string goal)
    {
        var goalKey = goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => "fuerza",
            var g when g.Contains("resistencia") => "resistencia",
            var g when g.Contains("flexibilidad") => "flexibilidad",
            var g when g.Contains("rehabilitacion") => "rehabilitacion",
            _ => "fuerza" // Default
        };

        return _basicExerciseTemplates.ContainsKey(goalKey)
            ? _basicExerciseTemplates[goalKey]
            : _basicExerciseTemplates["fuerza"];
    }

    private RoutineDay GenerateBasicDay(int dayNumber, List<string> exercises, string goal)
    {
        var day = new RoutineDay
        {
            Id = dayNumber,
            DayNumber = dayNumber,
            Name = $"Día {dayNumber} - Entrenamiento {goal.ToTitleCase()}",
            Description = $"Sesión {dayNumber} enfocada en {goal.ToLower()}",
            FocusArea = DetermineFocusArea(dayNumber, goal),
            TargetIntensity = DetermineIntensity(goal),
            EstimatedDurationMinutes = DetermineDuration(goal)
        };

        // Seleccionar ejercicios para este día
        var exercisesForDay = SelectExercisesForDay(exercises, dayNumber, goal);

        for (int i = 0; i < exercisesForDay.Count; i++)
        {
            var exercise = CreateBasicExercise(i + 1, exercisesForDay[i], goal);
            day.Exercises.Add(exercise);
        }

        return day;
    }

    private List<string> SelectExercisesForDay(List<string> exercises, int dayNumber, string goal)
    {
        var exerciseCount = goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => 6,
            var g when g.Contains("resistencia") => 8,
            var g when g.Contains("flexibilidad") => 10,
            var g when g.Contains("rehabilitacion") => 5,
            _ => 6
        };

        // Distribuir ejercicios de forma cíclica
        var selected = new List<string>();
        var startIndex = (dayNumber - 1) * (exerciseCount / 2);

        for (int i = 0; i < exerciseCount && selected.Count < exercises.Count; i++)
        {
            var index = (startIndex + i) % exercises.Count;
            if (!selected.Contains(exercises[index]))
            {
                selected.Add(exercises[index]);
            }
        }

        return selected;
    }

    private RoutineExercise CreateBasicExercise(int order, string exerciseName, string goal)
    {
        var exercise = new RoutineExercise
        {
            Id = order,
            Order = order,
            Name = exerciseName,
            Category = DetermineCategory(goal),
            MuscleGroups = GetMuscleGroups(exerciseName),
            Equipment = GetBasicEquipment(exerciseName),
            Instructions = GetBasicInstructions(exerciseName),
            SafetyTips = GetBasicSafetyTips(exerciseName),
            RestTimeSeconds = DetermineRestTime(goal),
            Difficulty = DetermineDifficulty(goal)
        };

        // Generar sets básicos
        var setCount = goal.ToLower().Contains("resistencia") ? 3 : 4;
        var baseReps = goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => 8,
            var g when g.Contains("resistencia") => 15,
            var g when g.Contains("flexibilidad") => 1, // Tiempo en lugar de reps
            var g when g.Contains("rehabilitacion") => 10,
            _ => 12
        };

        for (int i = 1; i <= setCount; i++)
        {
            exercise.Sets.Add(new ExerciseSet
            {
                Id = (order * 10) + i,
                SetNumber = i,
                Reps = baseReps + (i - 1) * -2, // Pirámide descendente
                Weight = CalculateBasicWeight(exerciseName, i),
                RestSeconds = exercise.RestTimeSeconds
            });
        }

        return exercise;
    }

    private string DetermineFocusArea(int dayNumber, string goal)
    {
        if (goal.ToLower().Contains("fuerza"))
        {
            return dayNumber switch
            {
                1 => "Pecho y Tríceps",
                2 => "Espalda y Bíceps",
                3 => "Piernas y Glúteos",
                4 => "Hombros y Core",
                _ => "Cuerpo Completo"
            };
        }

        return "Cuerpo Completo";
    }

    private string DetermineIntensity(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => "Alta",
            var g when g.Contains("resistencia") => "Moderada-Alta",
            var g when g.Contains("flexibilidad") => "Baja",
            var g when g.Contains("rehabilitacion") => "Muy Baja",
            _ => "Moderada"
        };
    }

    private int DetermineDuration(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => 75,
            var g when g.Contains("resistencia") => 60,
            var g when g.Contains("flexibilidad") => 45,
            var g when g.Contains("rehabilitacion") => 30,
            _ => 60
        };
    }

    private string DetermineCategory(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => "Fuerza",
            var g when g.Contains("resistencia") => "Cardio",
            var g when g.Contains("flexibilidad") => "Flexibilidad",
            var g when g.Contains("rehabilitacion") => "Rehabilitación",
            _ => "General"
        };
    }

    private int DetermineRestTime(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => 90,
            var g when g.Contains("resistencia") => 45,
            var g when g.Contains("flexibilidad") => 30,
            var g when g.Contains("rehabilitacion") => 60,
            _ => 60
        };
    }

    private string DetermineDifficulty(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => "Intermedio",
            var g when g.Contains("resistencia") => "Principiante",
            var g when g.Contains("flexibilidad") => "Principiante",
            var g when g.Contains("rehabilitacion") => "Principiante",
            _ => "Principiante"
        };
    }

    private List<string> GetMuscleGroups(string exerciseName)
    {
        // Mapeo básico basado en nombre del ejercicio
        return exerciseName.ToLower() switch
        {
            var e when e.Contains("press") && e.Contains("banca") => new List<string> { "Pectorales", "Tríceps", "Deltoides" },
            var e when e.Contains("sentadilla") => new List<string> { "Cuádriceps", "Glúteos", "Core" },
            var e when e.Contains("peso muerto") => new List<string> { "Isquiotibiales", "Glúteos", "Espalda Baja" },
            var e when e.Contains("dominada") => new List<string> { "Dorsales", "Bíceps", "Antebrazos" },
            var e when e.Contains("plancha") => new List<string> { "Core", "Hombros", "Pecho" },
            _ => new List<string> { "Múltiples grupos musculares" }
        };
    }

    private string GetBasicEquipment(string exerciseName)
    {
        return exerciseName.ToLower() switch
        {
            var e when e.Contains("press banca") => "Barra y banco",
            var e when e.Contains("mancuerna") => "Mancuernas",
            var e when e.Contains("barra") => "Barra olímpica",
            var e when e.Contains("dominada") => "Barra de dominadas",
            var e when e.Contains("peso corporal") || e.Contains("plancha") => "Peso corporal",
            _ => "Equipamiento básico de gimnasio"
        };
    }

    private string GetBasicInstructions(string exerciseName)
    {
        return $"Realiza {exerciseName.ToLower()} manteniendo una técnica correcta. " +
               "Controla el movimiento tanto en la fase concéntrica como excéntrica. " +
               "Respira adecuadamente durante cada repetición.";
    }

    private string GetBasicSafetyTips(string exerciseName)
    {
        return "Calienta adecuadamente antes de comenzar. " +
               "Mantén la postura correcta en todo momento. " +
               "Detente si sientes dolor agudo o molestia inusual.";
    }

    private decimal CalculateBasicWeight(string exerciseName, int setNumber)
    {
        // Pesos básicos estimados (esto sería personalizable en una implementación real)
        var baseWeight = exerciseName.ToLower() switch
        {
            var e when e.Contains("press banca") => 60m,
            var e when e.Contains("sentadilla") => 70m,
            var e when e.Contains("peso muerto") => 80m,
            var e when e.Contains("mancuerna") => 15m,
            var e when e.Contains("dominada") || e.Contains("peso corporal") => 0m,
            _ => 20m
        };

        // Incrementar peso ligeramente en sets posteriores
        return baseWeight + ((setNumber - 1) * 2.5m);
    }

    private RoutineMetrics CalculateRoutineMetrics(Routine routine)
    {
        return new RoutineMetrics
        {
            TotalExercises = routine.Days.SelectMany(d => d.Exercises).Count(),
            TotalSets = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.Sets).Count(),
            EstimatedDurationMinutes = routine.Days.Sum(d => d.EstimatedDurationMinutes),
            MuscleGroupsCovered = routine.Days.SelectMany(d => d.Exercises)
                                           .SelectMany(e => e.MuscleGroups)
                                           .Distinct()
                                           .ToList(),
            EquipmentRequired = routine.Days.SelectMany(d => d.Exercises)
                                          .Select(e => e.Equipment)
                                          .Distinct()
                                          .ToList(),
            DifficultyLevel = routine.Days.FirstOrDefault()?.Exercises.FirstOrDefault()?.Difficulty ?? "Principiante",
            CaloriesBurnedEstimate = EstimateCaloriesBurned(routine)
        };
    }

    private int EstimateCaloriesBurned(Routine routine)
    {
        var totalMinutes = routine.Days.Sum(d => d.EstimatedDurationMinutes);
        var caloriesPerMinute = routine.Goal.ToLower() switch
        {
            var g when g.Contains("fuerza") => 8,
            var g when g.Contains("resistencia") => 12,
            var g when g.Contains("flexibilidad") => 3,
            var g when g.Contains("rehabilitacion") => 4,
            _ => 6
        };

        return totalMinutes * caloriesPerMinute;
    }

    private void IncreaseIntensity(Routine routine)
    {
        foreach (var day in routine.Days)
        {
            foreach (var exercise in day.Exercises)
            {
                foreach (var set in exercise.Sets)
                {
                    if (set.Weight > 0)
                        set.Weight += 5; // Aumentar peso
                    if (set.Reps < 20)
                        set.Reps += 2; // Aumentar repeticiones
                }
                exercise.RestTimeSeconds = Math.Max(30, exercise.RestTimeSeconds - 15); // Reducir descanso
            }
            day.TargetIntensity = "Alta";
        }
    }

    private void DecreaseIntensity(Routine routine)
    {
        foreach (var day in routine.Days)
        {
            foreach (var exercise in day.Exercises)
            {
                foreach (var set in exercise.Sets)
                {
                    if (set.Weight > 5)
                        set.Weight -= 5; // Reducir peso
                    if (set.Reps > 5)
                        set.Reps -= 2; // Reducir repeticiones
                }
                exercise.RestTimeSeconds += 30; // Aumentar descanso
            }
            day.TargetIntensity = "Baja-Moderada";
        }
    }

    private void ReduceRestTimes(Routine routine)
    {
        foreach (var day in routine.Days)
        {
            foreach (var exercise in day.Exercises)
            {
                exercise.RestTimeSeconds = Math.Max(30, exercise.RestTimeSeconds - 30);
                foreach (var set in exercise.Sets)
                {
                    set.RestSeconds = Math.Max(30, set.RestSeconds - 30);
                }
            }
            day.EstimatedDurationMinutes = Math.Max(30, day.EstimatedDurationMinutes - 15);
        }
    }

    private void AddBasicDay(Routine routine)
    {
        var newDayNumber = routine.Days.Count + 1;
        var exercises = GetExercisesForGoal(routine.Goal);
        var newDay = GenerateBasicDay(newDayNumber, exercises, routine.Goal);

        routine.Days.Add(newDay);
    }

    private ILogger<FallbackService> CreateDefaultLogger()
    {
        return new NullLogger<FallbackService>();
    }

    private class NullLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        private class NullDisposable : IDisposable
        {
            public static NullDisposable Instance = new();
            public void Dispose() { }
        }
    }

    #endregion

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Extension methods para ayudar con el texto
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }

    public static Routine Clone(this Routine original)
    {
        // En una implementación real, usaríamos un mapeo más sofisticado
        var clone = new Routine
        {
            Id = original.Id,
            Name = original.Name,
            ClientName = original.ClientName,
            Description = original.Description,
            Goal = original.Goal,
            DurationWeeks = original.DurationWeeks,
            CreatedDate = original.CreatedDate,
            Days = new List<RoutineDay>()
        };

        // Clonar días (implementación básica)
        foreach (var day in original.Days)
        {
            clone.Days.Add(day); // En implementación real, clonaríamos profundamente
        }

        return clone;
    }
}