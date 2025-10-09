using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Data.Context;
using Entities = GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Infrastructure.Images;

namespace GymRoutineGenerator.Infrastructure.Services
{
    /// <summary>
    /// Servicio principal para generación de rutinas personalizadas
    /// Combina generación con IA y métodos clásicos como fallback
    /// </summary>
    public class RoutineGenerationService : IRoutineGenerationService
    {
        private readonly Random _random;
        private readonly IImageService _imageService;
        private readonly GymRoutineContext _context;
        private readonly IIntelligentRoutineService _aiRoutineService;
        private readonly IOllamaService _ollamaService;

        public RoutineGenerationService(
            GymRoutineContext context,
            IIntelligentRoutineService aiRoutineService,
            IOllamaService ollamaService,
            IImageService imageService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _aiRoutineService = aiRoutineService ?? throw new ArgumentNullException(nameof(aiRoutineService));
            _ollamaService = ollamaService ?? throw new ArgumentNullException(nameof(ollamaService));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _random = new Random();
        }

        public async Task<RoutineGenerationResult> GeneratePersonalizedRoutineAsync(UserRoutineParameters parameters)
        {
            var startTime = DateTime.Now;

            try
            {
                // Intentar generación con IA primero
                var aiResult = await GenerateWithAIAsync(parameters);
                if (aiResult.Success)
                {
                    aiResult.GenerationTime = DateTime.Now - startTime;
                    aiResult.GeneratedWithAI = true;
                    return aiResult;
                }

                Console.WriteLine($"⚠️ IA no disponible, usando generador clásico: {aiResult.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error en generación con IA: {ex.Message}");
            }

            // Fallback a generación clásica
            var classicResult = await GenerateClassicRoutineAsync(parameters);
            classicResult.GenerationTime = DateTime.Now - startTime;
            classicResult.GeneratedWithAI = false;
            return classicResult;
        }

        public async Task<RoutineGenerationResult> GenerateAlternativeRoutineAsync(UserRoutineParameters parameters)
        {
            var startTime = DateTime.Now;

            try
            {
                if (_aiRoutineService != null && await IsAIAvailableAsync())
                {
                    var result = await _aiRoutineService.GenerateAlternativeRoutineAsync(parameters);

                    if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.GeneratedRoutine))
                    {
                        var workoutDays = await ParseAIRoutineToStructureAsync(result.GeneratedRoutine, parameters);

                        return new RoutineGenerationResult
                        {
                            Success = true,
                            RoutineText = result.GeneratedRoutine,
                            WorkoutDays = workoutDays,
                            GenerationTime = DateTime.Now - startTime,
                            GeneratedWithAI = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando alternativa con IA: {ex.Message}");
            }

            // Fallback: generar rutina clásica con variación
            var alternativeResult = await GenerateClassicVariationAsync(parameters);
            alternativeResult.GenerationTime = DateTime.Now - startTime;
            alternativeResult.GeneratedWithAI = false;
            return alternativeResult;
        }

        public async Task<bool> IsAIAvailableAsync()
        {
            if (_ollamaService == null) return false;

            try
            {
                return await _ollamaService.IsAvailableAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetAIStatusAsync()
        {
            if (_ollamaService == null)
            {
                return @"❌ Servicio de IA no inicializado

📋 Para usar IA necesitas:

1. Instalar Ollama desde: https://ollama.ai
2. Una vez instalado, ejecutar en terminal:
   ollama pull mistral:7b
3. Verificar que Ollama esté corriendo:
   ollama list

💡 Sin IA, la aplicación usa el generador clásico (funciona perfectamente).";
            }

            try
            {
                var health = await _ollamaService.GetHealthStatusAsync();
                var models = await _ollamaService.GetAvailableModelsAsync();

                var status = new StringBuilder();
                status.AppendLine($"Estado: {health.Status}");
                status.AppendLine($"Versión: {health.Version}");
                status.AppendLine($"Tiempo de respuesta: {health.ResponseTime.TotalMilliseconds:F0}ms");
                status.AppendLine($"Modelos disponibles: {models.Count}");

                if (models.Contains("mistral:7b"))
                {
                    status.AppendLine("✅ Mistral 7B disponible");
                }
                else
                {
                    status.AppendLine("❌ Mistral 7B no encontrado");
                    status.AppendLine("\n💡 Para instalar: ollama pull mistral:7b");
                }

                return status.ToString();
            }
            catch (HttpRequestException ex)
            {
                return @$"❌ No se puede conectar con Ollama

Detalle del error: {ex.Message}

📋 Para usar IA necesitas:

1. Instalar Ollama desde: https://ollama.ai
2. Ejecutar Ollama (debería iniciarse automáticamente)
3. Verificar que esté corriendo en http://localhost:11434
4. Instalar el modelo: ollama pull mistral:7b

💡 Sin IA, la aplicación usa el generador clásico (funciona perfectamente).";
            }
            catch (Exception ex)
            {
                return $@"❌ Error verificando IA: {ex.Message}

💡 Sin IA, la aplicación usa el generador clásico (funciona perfectamente).";
            }
        }

        #region Private Methods - AI Generation

        private async Task<RoutineGenerationResult> GenerateWithAIAsync(UserRoutineParameters parameters)
        {
            if (_aiRoutineService == null || _ollamaService == null)
            {
                return new RoutineGenerationResult
                {
                    Success = false,
                    ErrorMessage = "Servicios de IA no disponibles"
                };
            }

            // Verificar disponibilidad
            var isAvailable = await _ollamaService.IsAvailableAsync();
            if (!isAvailable)
            {
                return new RoutineGenerationResult
                {
                    Success = false,
                    ErrorMessage = "Ollama no está disponible"
                };
            }

            // Generar con IA
            var result = await _aiRoutineService.GenerateIntelligentRoutineAsync(parameters);

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.GeneratedRoutine))
            {
                Console.WriteLine($"🤖 IA generó rutina en {result.GenerationTime.TotalSeconds:F1}s");

                if (result.Warnings.Any())
                {
                    Console.WriteLine($"⚠️ Advertencias: {string.Join(", ", result.Warnings)}");
                }

                // Parsear respuesta de IA a estructura con imágenes
                var workoutDays = await ParseAIRoutineToStructureAsync(result.GeneratedRoutine, parameters);

                return new RoutineGenerationResult
                {
                    Success = true,
                    RoutineText = result.GeneratedRoutine,
                    WorkoutDays = workoutDays
                };
            }

            return new RoutineGenerationResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "Error desconocido generando con IA"
            };
        }

        private async Task<List<WorkoutDay>> ParseAIRoutineToStructureAsync(string aiRoutine, UserRoutineParameters parameters)
        {
            var workoutDays = new List<WorkoutDay>();

            try
            {
                // Obtener ejercicios de BD para hacer matching
                var allExercises = await _context.Exercises
                    .Include(e => e.PrimaryMuscleGroup)
                    .Include(e => e.EquipmentType)
                    .Where(e => e.IsActive)
                    .ToListAsync();

                if (!allExercises.Any())
                {
                    Console.WriteLine("⚠️ No hay ejercicios en BD para parsear rutina IA");
                    return workoutDays;
                }

                var lines = aiRoutine.Split('\n');
                WorkoutDay? currentDay = null;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // Detectar inicio de día
                    if (trimmedLine.StartsWith("DÍA", StringComparison.OrdinalIgnoreCase) ||
                        trimmedLine.StartsWith("DIA", StringComparison.OrdinalIgnoreCase) ||
                        trimmedLine.Contains("**DÍA") ||
                        trimmedLine.Contains("**DIA"))
                    {
                        if (currentDay != null && currentDay.Exercises.Any())
                        {
                            workoutDays.Add(currentDay);
                        }

                        currentDay = new WorkoutDay
                        {
                            Name = ExtractDayName(trimmedLine),
                            Exercises = new List<Exercise>()
                        };
                        continue;
                    }

                    // Detectar ejercicios
                    if (IsExerciseLine(trimmedLine))
                    {
                        if (currentDay == null)
                        {
                            currentDay = new WorkoutDay
                            {
                                Name = "Día 1",
                                Exercises = new List<Exercise>()
                            };
                        }

                        var exerciseName = ExtractExerciseName(trimmedLine);
                        if (!string.IsNullOrWhiteSpace(exerciseName))
                        {
                            var dbExercise = FindBestMatchingExercise(allExercises, exerciseName);

                            if (dbExercise != null)
                            {
                                var exercise = ConvertToRoutineExercise(dbExercise, parameters);
                                currentDay.Exercises.Add(exercise);
                                Console.WriteLine($"✅ Matched '{exerciseName}' -> '{dbExercise.SpanishName}'");
                            }
                            else
                            {
                                // Buscar en carpeta de imágenes
                                var imagePath = SearchImageInFolder(exerciseName);

                                currentDay.Exercises.Add(new Exercise
                                {
                                    Name = exerciseName,
                                    Sets = 3,
                                    Reps = "10-12",
                                    Instructions = "Sigue las indicaciones del entrenador",
                                    ImagePath = imagePath
                                });

                                Console.WriteLine(string.IsNullOrEmpty(imagePath)
                                    ? $"⚠️ No match para '{exerciseName}'"
                                    : $"✅ Imagen encontrada para '{exerciseName}'");
                            }
                        }
                    }
                }

                // Agregar último día
                if (currentDay != null && currentDay.Exercises.Any())
                {
                    workoutDays.Add(currentDay);
                }

                Console.WriteLine($"✅ Parseados {workoutDays.Count} días con {workoutDays.Sum(d => d.Exercises.Count)} ejercicios");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error parseando rutina IA: {ex.Message}");
            }

            return workoutDays;
        }

        private string ExtractDayName(string line)
        {
            var cleaned = line.Replace("*", "").Replace("#", "").Trim();

            if (cleaned.Contains("-"))
                return cleaned.Split('-', 2).LastOrDefault()?.Trim() ?? "Entrenamiento";
            if (cleaned.Contains(":"))
                return cleaned.Split(':', 2).LastOrDefault()?.Trim() ?? "Entrenamiento";

            return cleaned;
        }

        private bool IsExerciseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;

            return Regex.IsMatch(line, @"^\d+\.") ||
                   line.StartsWith("-") || line.StartsWith("•") || line.StartsWith("*");
        }

        private string ExtractExerciseName(string line)
        {
            var cleaned = Regex.Replace(line, @"^\d+\.\s*", "");
            cleaned = cleaned.TrimStart('-', '•', '*', ' ').Replace("**", "");

            if (cleaned.Contains("("))
                cleaned = cleaned.Split('(')[0];
            if (cleaned.Contains("-") && cleaned.IndexOf("-") > 10)
                cleaned = cleaned.Split('-')[0];

            return cleaned.Trim();
        }

        private Data.Entities.Exercise? FindBestMatchingExercise(
            List<Data.Entities.Exercise> exercises, string searchName)
        {
            searchName = searchName.ToLower().Trim();

            // 1. Exact match
            var exactMatch = exercises.FirstOrDefault(e =>
                e.SpanishName.ToLower().Trim() == searchName ||
                e.Name.ToLower().Trim() == searchName);
            if (exactMatch != null) return exactMatch;

            // 2. Contains match
            var containsMatch = exercises.FirstOrDefault(e =>
                e.SpanishName.ToLower().Contains(searchName) ||
                searchName.Contains(e.SpanishName.ToLower()) ||
                e.Name.ToLower().Contains(searchName) ||
                searchName.Contains(e.Name.ToLower()));
            if (containsMatch != null) return containsMatch;

            // 3. Fuzzy match por palabras clave
            var keywords = new Dictionary<string, string[]>
            {
                ["press"] = new[] { "press", "prensa", "bench" },
                ["squat"] = new[] { "squat", "sentadilla", "cuclilla" },
                ["curl"] = new[] { "curl", "flexion" },
                ["row"] = new[] { "row", "remo" },
                ["pull"] = new[] { "pull", "jalón", "dominada" },
                ["push"] = new[] { "push", "empuje", "fondo" }
            };

            foreach (var kvp in keywords)
            {
                if (kvp.Value.Any(k => searchName.Contains(k)))
                {
                    var fuzzyMatch = exercises.FirstOrDefault(e =>
                        kvp.Value.Any(k => e.SpanishName.ToLower().Contains(k) ||
                                          e.Name.ToLower().Contains(k)));
                    if (fuzzyMatch != null) return fuzzyMatch;
                }
            }

            return null;
        }

        #endregion

        #region Private Methods - Classic Generation

        private async Task<RoutineGenerationResult> GenerateClassicRoutineAsync(UserRoutineParameters parameters)
        {
            try
            {
                var workoutDays = await CreateWorkoutFromDatabaseAsync(parameters);
                var text = FormatWorkoutDaysToString(workoutDays, parameters, false);

                return new RoutineGenerationResult
                {
                    Success = true,
                    RoutineText = text,
                    WorkoutDays = workoutDays
                };
            }
            catch (Exception ex)
            {
                return new RoutineGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error generando rutina clásica: {ex.Message}"
                };
            }
        }

        private async Task<RoutineGenerationResult> GenerateClassicVariationAsync(UserRoutineParameters parameters)
        {
            try
            {
                var workoutDays = CreateAlternativeWorkoutPlan(parameters);
                var text = FormatWorkoutDaysToString(workoutDays, parameters, true);

                return new RoutineGenerationResult
                {
                    Success = true,
                    RoutineText = text,
                    WorkoutDays = workoutDays
                };
            }
            catch (Exception ex)
            {
                return new RoutineGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error generando variación: {ex.Message}"
                };
            }
        }

        private async Task<List<WorkoutDay>> CreateWorkoutFromDatabaseAsync(UserRoutineParameters parameters)
        {
            var workoutDays = new List<WorkoutDay>();

            var allExercises = await _context.Exercises
                .Include(e => e.PrimaryMuscleGroup)
                .Include(e => e.EquipmentType)
                .Where(e => e.IsActive)
                .ToListAsync();

            Console.WriteLine($"✅ Ejercicios encontrados en BD: {allExercises.Count}");

            if (!allExercises.Any())
            {
                Console.WriteLine("⚠️ No hay ejercicios activos en BD, usando fallback");
                return CreateFallbackWorkout(parameters);
            }

            var suitableExercises = FilterExercisesByProfile(allExercises, parameters);
            Console.WriteLine($"✅ Ejercicios adecuados: {suitableExercises.Count}");

            for (int day = 1; day <= parameters.TrainingDaysPerWeek; day++)
            {
                var workoutDay = CreateWorkoutDay(day, suitableExercises, parameters);
                workoutDays.Add(workoutDay);
            }

            return workoutDays;
        }

        private List<Data.Entities.Exercise> FilterExercisesByProfile(
            List<Data.Entities.Exercise> exercises, UserRoutineParameters parameters)
        {
            var filtered = exercises.AsQueryable();

            // Filtrar por nivel
            if (parameters.ExperienceLevel.Contains("Principiante"))
            {
                filtered = filtered.Where(e => (int)e.DifficultyLevel <= 1);
            }
            else if (parameters.ExperienceLevel.Contains("Intermedio"))
            {
                filtered = filtered.Where(e => (int)e.DifficultyLevel <= 2);
            }

            // Filtrar por edad
            if (parameters.Age >= 50)
            {
                filtered = filtered.Where(e =>
                    e.EquipmentType.Name.Contains("Bodyweight") ||
                    e.EquipmentType.Name.Contains("Dumbbells") ||
                    e.EquipmentType.Name.Contains("Resistance") ||
                    e.EquipmentType.Name.Contains("Machine"));
            }

            return filtered.ToList();
        }

        private WorkoutDay CreateWorkoutDay(int dayNumber,
            List<Data.Entities.Exercise> exercises, UserRoutineParameters parameters)
        {
            var dayName = GetWorkoutDayName(dayNumber, parameters);
            var selectedExercises = new List<Exercise>();

            var muscleGroups = GetTargetMuscleGroups(dayNumber, parameters);

            foreach (var muscleGroup in muscleGroups)
            {
                var exercisesForMuscle = exercises
                    .Where(e => MuscleGroupMatches(muscleGroup, e.PrimaryMuscleGroup.Name, e.PrimaryMuscleGroup.SpanishName))
                    .ToList();

                if (exercisesForMuscle.Any())
                {
                    var exercisesForThisGroup = new List<Data.Entities.Exercise>();
                    var availableExercises = exercisesForMuscle.ToList();

                    for (int i = 0; i < 3 && availableExercises.Any(); i++)
                    {
                        var selectedExercise = availableExercises[_random.Next(availableExercises.Count)];
                        exercisesForThisGroup.Add(selectedExercise);
                        availableExercises.Remove(selectedExercise);
                    }

                    foreach (var exercise in exercisesForThisGroup)
                    {
                        selectedExercises.Add(ConvertToRoutineExercise(exercise, parameters));
                    }
                }
            }

            return new WorkoutDay
            {
                Name = dayName,
                DayNumber = dayNumber,
                Exercises = selectedExercises,
                FocusAreas = string.Join(", ", muscleGroups)
            };
        }

        private Exercise ConvertToRoutineExercise(Data.Entities.Exercise dbExercise, UserRoutineParameters parameters)
        {
            var (sets, reps) = GetSetsAndReps(dbExercise, parameters);
            var imageInfo = GetExerciseImageInfo(dbExercise.Id);

            return new Exercise
            {
                Id = dbExercise.Id,
                Name = dbExercise.SpanishName,
                Sets = sets,
                Reps = reps,
                Instructions = dbExercise.Instructions,
                Description = dbExercise.Description,
                ImageUrl = imageInfo,
                DifficultyLevel = dbExercise.DifficultyLevel.ToString(),
                EquipmentRequired = dbExercise.EquipmentType?.Name ?? "Sin equipo"
            };
        }

        private string GetExerciseImageInfo(int exerciseId)
        {
            try
            {
                var images = _context.ExerciseImages
                    .Where(img => img.ExerciseId == exerciseId)
                    .OrderByDescending(img => img.IsPrimary)
                    .ToList();

                if (images.Any())
                {
                    var primaryImage = images.First();
                    if (primaryImage.ImageData != null && primaryImage.ImageData.Length > 0)
                    {
                        return $"Imagen disponible ({primaryImage.ImageData.Length:N0} bytes)";
                    }
                }
                return "Sin imagen - Agregar en Gestor de Imágenes";
            }
            catch
            {
                return "Error verificando imagen";
            }
        }

        private (int sets, string reps) GetSetsAndReps(Data.Entities.Exercise exercise, UserRoutineParameters parameters)
        {
            var baseReps = parameters.ExperienceLevel.Contains("Principiante") ? "8-10" :
                          parameters.ExperienceLevel.Contains("Intermedio") ? "10-12" : "12-15";

            var sets = parameters.Age >= 50 ? 2 : parameters.ExperienceLevel.Contains("Principiante") ? 3 : 4;

            if (exercise.ExerciseType == Core.Enums.ExerciseType.Cardio)
            {
                return (1, parameters.Age >= 50 ? "10-15 minutos" : "15-20 minutos");
            }

            return (sets, baseReps);
        }

        private string GetWorkoutDayName(int dayNumber, UserRoutineParameters parameters)
        {
            if (parameters.TrainingDaysPerWeek <= 2)
            {
                return dayNumber == 1 ? "Cuerpo Completo A" : "Cuerpo Completo B";
            }

            return dayNumber switch
            {
                1 => "Tren Superior",
                2 => "Tren Inferior",
                3 => "Cuerpo Completo",
                4 => "Tren Superior + Core",
                5 => "Tren Inferior + Cardio",
                _ => $"Día {dayNumber}"
            };
        }

        private List<string> GetTargetMuscleGroups(int dayNumber, UserRoutineParameters parameters)
        {
            if (parameters.TrainingDaysPerWeek <= 2)
            {
                return new List<string> { "Chest", "Back", "Legs", "Shoulders" };
            }

            if (parameters.TrainingDaysPerWeek == 3)
            {
                return dayNumber switch
                {
                    1 => new List<string> { "Chest", "Triceps", "Shoulders" },
                    2 => new List<string> { "Back", "Biceps", "Abs" },
                    3 => new List<string> { "Legs", "Glutes", "Calves" },
                    _ => new List<string> { "Chest", "Back" }
                };
            }

            return dayNumber switch
            {
                1 => new List<string> { "Chest", "Triceps" },
                2 => new List<string> { "Back", "Biceps" },
                3 => new List<string> { "Legs", "Glutes" },
                4 => new List<string> { "Shoulders", "Abs" },
                5 => new List<string> { "Arms", "Forearms" },
                _ => new List<string> { "Chest", "Back" }
            };
        }

        private bool MuscleGroupMatches(string targetGroup, string englishName, string spanishName)
        {
            var target = targetGroup.ToLower();
            var english = englishName.ToLower();
            var spanish = spanishName.ToLower();

            return target switch
            {
                "chest" => english.Contains("chest") || spanish.Contains("pecho"),
                "back" => english.Contains("back") || spanish.Contains("espalda"),
                "legs" => english.Contains("leg") || english.Contains("quad") || spanish.Contains("pierna"),
                "shoulders" => english.Contains("shoulder") || spanish.Contains("hombro"),
                "biceps" => english.Contains("bicep") || spanish.Contains("bicep"),
                "triceps" => english.Contains("tricep") || spanish.Contains("tricep"),
                "abs" => english.Contains("abs") || spanish.Contains("abdomen"),
                "glutes" => english.Contains("glute") || spanish.Contains("gluteo"),
                "calves" => english.Contains("calv") || spanish.Contains("pantorrilla"),
                _ => false
            };
        }

        private List<WorkoutDay> CreateAlternativeWorkoutPlan(UserRoutineParameters parameters)
        {
            // Implementación simplificada - crear rutina alternativa sin BD
            var workouts = new List<WorkoutDay>();

            if (parameters.TrainingDaysPerWeek >= 3)
            {
                workouts.Add(new WorkoutDay
                {
                    Name = "Pecho y Tríceps - Variante",
                    DayNumber = 1,
                    Exercises = new List<Exercise>
                    {
                        new() { Name = "Press de banca inclinado", Sets = 4, Reps = "8-10", Instructions = "Usa inclinación de 30-45 grados" },
                        new() { Name = "Aperturas con mancuernas", Sets = 3, Reps = "12-15", Instructions = "Mantén codos ligeramente flexionados" },
                        new() { Name = "Fondos en paralelas", Sets = 3, Reps = "8-12", Instructions = "Inclínate hacia adelante" }
                    }
                });
            }

            return workouts;
        }

        private List<WorkoutDay> CreateFallbackWorkout(UserRoutineParameters parameters)
        {
            var workouts = new List<WorkoutDay>();

            workouts.Add(new WorkoutDay
            {
                Name = "Cuerpo Completo",
                DayNumber = 1,
                Exercises = new List<Exercise>
                {
                    new() { Name = "Flexiones", Sets = 3, Reps = "8-12", Instructions = "Ejercicio completo de pecho" },
                    new() { Name = "Sentadillas", Sets = 3, Reps = "12-15", Instructions = "Ejercicio fundamental de piernas" },
                    new() { Name = "Plancha", Sets = 3, Reps = "30-60 seg", Instructions = "Mantén el cuerpo recto" }
                }
            });

            return workouts;
        }

        private string FormatWorkoutDaysToString(List<WorkoutDay> workouts, UserRoutineParameters parameters, bool isAlternative)
        {
            var sb = new StringBuilder();

            sb.AppendLine(isAlternative ? "RUTINA ALTERNATIVA" : "RUTINA PERSONALIZADA");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();

            sb.AppendLine("INFORMACIÓN PERSONAL");
            sb.AppendLine($"Nombre: {parameters.Name}");
            sb.AppendLine($"Edad: {parameters.Age} años");
            sb.AppendLine($"Nivel: {parameters.ExperienceLevel}");
            sb.AppendLine($"Días de entrenamiento: {parameters.TrainingDaysPerWeek} días/semana");
            sb.AppendLine();

            if (parameters.Goals.Any())
            {
                sb.AppendLine("OBJETIVOS SELECCIONADOS");
                foreach (var goal in parameters.Goals)
                {
                    sb.AppendLine($"  - {goal}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("RUTINA DE ENTRENAMIENTO");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine();

            int dayNumber = 1;
            foreach (var workout in workouts)
            {
                sb.AppendLine($"DÍA {dayNumber}: {workout.Name.ToUpper()}");
                sb.AppendLine();

                int exerciseNumber = 1;
                foreach (var exercise in workout.Exercises)
                {
                    sb.AppendLine($"{exerciseNumber}. {exercise.Name} - {exercise.Sets} series x {exercise.Reps} reps");
                    if (!string.IsNullOrEmpty(exercise.Instructions))
                    {
                        sb.AppendLine($"   Nota: {exercise.Instructions}");
                    }
                    sb.AppendLine();
                    exerciseNumber++;
                }

                sb.AppendLine();
                dayNumber++;
            }

            sb.AppendLine("IMPORTANTE:");
            sb.AppendLine("- Calienta 5-10 minutos antes de cada sesión");
            sb.AppendLine("- Descansa 60-90 segundos entre series");
            sb.AppendLine("- Mantén buena forma en cada ejercicio");

            return sb.ToString();
        }

        private string SearchImageInFolder(string exerciseName)
        {
            try
            {
                var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "docs", "ejercicios");
                basePath = Path.GetFullPath(basePath);

                if (!Directory.Exists(basePath))
                {
                    return string.Empty;
                }

                exerciseName = exerciseName.ToLower().Trim();
                var exerciseFolders = Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories);

                // Búsqueda exacta
                foreach (var folder in exerciseFolders)
                {
                    var folderName = Path.GetFileName(folder).ToLower();
                    if (folderName == exerciseName)
                    {
                        var image = GetFirstImageInFolder(folder);
                        if (!string.IsNullOrEmpty(image))
                        {
                            return image;
                        }
                    }
                }

                // Búsqueda por contención
                foreach (var folder in exerciseFolders)
                {
                    var folderName = Path.GetFileName(folder).ToLower();
                    if (folderName.Contains(exerciseName) || exerciseName.Contains(folderName))
                    {
                        var image = GetFirstImageInFolder(folder);
                        if (!string.IsNullOrEmpty(image))
                        {
                            return image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error buscando imagen para '{exerciseName}': {ex.Message}");
            }

            return string.Empty;
        }

        private string GetFirstImageInFolder(string folderPath)
        {
            var imageExtensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.webp", "*.gif" };
            foreach (var ext in imageExtensions)
            {
                var files = Directory.GetFiles(folderPath, ext, SearchOption.TopDirectoryOnly);
                if (files.Any())
                    return files.First();
            }
            return string.Empty;
        }

        // Métodos adicionales para compatibilidad con UI antigua
        public async Task<(string text, List<WorkoutDay>? workouts)> GeneratePersonalizedRoutineWithStructureAsync(Entities.UserProfile profile)
        {
            var parameters = ConvertUserProfileToParameters(profile);
            var result = await GeneratePersonalizedRoutineAsync(parameters);

            return (result.RoutineText, result.WorkoutDays);
        }

        public async Task<string> GenerateAlternativeRoutineAsync(Entities.UserProfile profile)
        {
            var parameters = ConvertUserProfileToParameters(profile);
            var result = await GenerateAlternativeRoutineAsync(parameters);

            return result.RoutineText;
        }

        private UserRoutineParameters ConvertUserProfileToParameters(Entities.UserProfile profile)
        {
            // Convert fitness level to intensity (1-5)
            int intensity = profile.FitnessLevel?.ToLower() switch
            {
                "principiante" => 2,
                "intermedio" => 3,
                "avanzado" => 4,
                _ => 3
            };

            return new UserRoutineParameters
            {
                Name = profile.Name,
                Age = profile.Age,
                Gender = profile.Gender,
                TrainingDaysPerWeek = profile.TrainingDays,
                ExperienceLevel = profile.FitnessLevel,
                Goals = profile.Goals ?? new List<string>(),
                AvailableEquipment = new List<string>(),
                PhysicalLimitations = new List<string>(),
                GymType = "Gimnasio completo",
                PrimaryGoal = profile.Goals?.FirstOrDefault() ?? "Mejorar condición física",
                RecommendedIntensity = intensity,
                PreferredSessionDuration = 60,
                IncludeCardio = false,
                IncludeFlexibility = true,
                MuscleGroupPreferences = new List<MuscleGroupFocus>(),
                AvoidExercises = new List<string>(),
                PreferredExerciseTypes = new List<string>()
            };
        }

        #endregion
    }
}
