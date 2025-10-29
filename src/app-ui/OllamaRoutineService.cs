using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Servicio para generar rutinas usando Ollama + Mistral
    /// </summary>
    public class OllamaRoutineService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaUrl = "http://localhost:11434/api/generate";
        private readonly string _model = "mistral:7b";
        private readonly ExerciseImageSearchService _exerciseSearch;

        public OllamaRoutineService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _exerciseSearch = new ExerciseImageSearchService();
        }

        /// <summary>
        /// Verifica si Ollama esta disponible
        /// </summary>
        public async Task<bool> IsOllamaAvailable()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:11434/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genera rutina personalizada con IA
        /// </summary>
        public async Task<AIRoutineResponse> GenerateRoutineWithAI(UserProfile profile, bool alternative = false)
        {
            // 1. Determinar division de grupos musculares segun dias
            var muscleSplit = DetermineMuscleGroupSplit(profile.TrainingDays, profile.FitnessLevel, profile.Age);

            // 2. Obtener ejercicios disponibles por grupo muscular
            var availableExercises = new Dictionary<string, List<ExerciseWithImage>>();
            System.Diagnostics.Debug.WriteLine("=== BUSCANDO EJERCICIOS DISPONIBLES ===");
            foreach (var day in muscleSplit)
            {
                foreach (var muscle in day.MuscleGroups)
                {
                    if (!availableExercises.ContainsKey(muscle))
                    {
                        var exercises = _exerciseSearch.GetExercisesByMuscleGroup(muscle);
                        availableExercises[muscle] = exercises;
                        System.Diagnostics.Debug.WriteLine($"Grupo '{muscle}': {exercises.Count} ejercicio(s) encontrados");

                        // Guardar en archivo para debug
                        try
                        {
                            System.IO.File.AppendAllText("debug-ejercicios.txt",
                                $"Grupo '{muscle}': {exercises.Count} ejercicio(s) - {string.Join(", ", exercises.Select(e => e.Name))}\n");
                        }
                        catch { }
                    }
                }
            }

            // 3. Generar rutina dia por dia
            var workoutDays = new List<WorkoutDay>();
            int dayNumber = 1;

            foreach (var day in muscleSplit)
            {
                var prompt = BuildPromptForDay(profile, day, availableExercises, dayNumber, alternative);

                // Guardar prompt en archivo
                try
                {
                    System.IO.File.WriteAllText($"debug-prompt-dia{dayNumber}.txt", prompt);
                }
                catch { }

                var aiResponse = await CallOllamaAPI(prompt, alternative);

                // Guardar respuesta en archivo
                try
                {
                    System.IO.File.WriteAllText($"debug-respuesta-dia{dayNumber}.txt", aiResponse ?? "[RESPUESTA VACIA]");
                }
                catch { }

                var workoutDay = ParseAIResponseToWorkoutDay(aiResponse, day.DayName, day.MuscleGroups, availableExercises);
                workoutDays.Add(workoutDay);
                dayNumber++;
            }

            return new AIRoutineResponse
            {
                Success = true,
                WorkoutDays = workoutDays,
                Message = "Rutina generada con IA exitosamente"
            };
        }

        /// <summary>
        /// Determina division de grupos musculares segun dias disponibles
        /// </summary>
        private List<MuscleGroupDay> DetermineMuscleGroupSplit(int trainingDays, string fitnessLevel, int age)
        {
            var isBeginner = fitnessLevel?.Equals("Principiante", StringComparison.OrdinalIgnoreCase) ?? false;
            var isOlder = age >= 50;

            var split = new List<MuscleGroupDay>();

            switch (trainingDays)
            {
                case 1:
                    split.Add(new MuscleGroupDay
                    {
                        DayName = "Dia 1",
                        MuscleGroups = new[] { "Pecho", "Espalda", "Piernas", "Core" }
                    });
                    break;

                case 2:
                    split.Add(new MuscleGroupDay
                    {
                        DayName = "Dia 1",
                        MuscleGroups = new[] { "Pecho", "Espalda", "Piernas", "Core" }
                    });
                    split.Add(new MuscleGroupDay
                    {
                        DayName = "Dia 2",
                        MuscleGroups = new[] { "Hombros", "Biceps", "Triceps", "Gluteos", "Core" }
                    });
                    break;

                case 3:
                    split.Add(new MuscleGroupDay { DayName = "Dia 1", MuscleGroups = new[] { "Pecho", "Hombros", "Triceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 2", MuscleGroups = new[] { "Espalda", "Biceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 3", MuscleGroups = new[] { "Piernas", "Gluteos", "Core" } });
                    break;

                case 4:
                    split.Add(new MuscleGroupDay { DayName = "Dia 1", MuscleGroups = new[] { "Pecho", "Hombros", "Triceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 2", MuscleGroups = new[] { "Piernas", "Gluteos" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 3", MuscleGroups = new[] { "Espalda", "Biceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 4", MuscleGroups = new[] { "Piernas", "Core" } });
                    break;

                case 5:
                    split.Add(new MuscleGroupDay { DayName = "Dia 1", MuscleGroups = new[] { "Pecho", "Triceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 2", MuscleGroups = new[] { "Espalda", "Biceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 3", MuscleGroups = new[] { "Piernas" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 4", MuscleGroups = new[] { "Hombros", "Core" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 5", MuscleGroups = new[] { "Gluteos", "Core" } });
                    break;

                case 6:
                    split.Add(new MuscleGroupDay { DayName = "Dia 1", MuscleGroups = new[] { "Pecho" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 2", MuscleGroups = new[] { "Espalda" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 3", MuscleGroups = new[] { "Piernas" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 4", MuscleGroups = new[] { "Hombros" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 5", MuscleGroups = new[] { "Biceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 6", MuscleGroups = new[] { "Triceps", "Core" } });
                    break;

                case 7:
                    split.Add(new MuscleGroupDay { DayName = "Dia 1", MuscleGroups = new[] { "Pecho" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 2", MuscleGroups = new[] { "Espalda" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 3", MuscleGroups = new[] { "Piernas" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 4", MuscleGroups = new[] { "Hombros" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 5", MuscleGroups = new[] { "Biceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 6", MuscleGroups = new[] { "Triceps" } });
                    split.Add(new MuscleGroupDay { DayName = "Dia 7", MuscleGroups = new[] { "Core", "Cuerpo Completo" } });
                    break;

                default:
                    split.Add(new MuscleGroupDay { DayName = "Dia 1", MuscleGroups = new[] { "Pecho", "Espalda", "Piernas" } });
                    break;
            }

            return split;
        }

        /// <summary>
        /// Calcula distribuciÃ³n de ejercicios por grupo muscular
        /// </summary>
        private int[] CalculateExerciseDistribution(int muscleGroupCount, int totalExercises)
        {
            var distribution = new int[muscleGroupCount];

            if (muscleGroupCount == 1)
            {
                distribution[0] = totalExercises;
            }
            else if (muscleGroupCount == 2)
            {
                // Primer grupo (principal/mÃ¡s grande): 3 ejercicios
                // Segundo grupo (secundario/mÃ¡s pequeÃ±o): 2 ejercicios
                distribution[0] = 3;
                distribution[1] = 2;
            }
            else if (muscleGroupCount == 3)
            {
                // 2-2-1 distribuciÃ³n
                distribution[0] = 2;
                distribution[1] = 2;
                distribution[2] = 1;
            }
            else
            {
                // Distribuir equitativamente
                int base_count = totalExercises / muscleGroupCount;
                int remainder = totalExercises % muscleGroupCount;

                for (int i = 0; i < muscleGroupCount; i++)
                {
                    distribution[i] = base_count + (i < remainder ? 1 : 0);
                }
            }

            return distribution;
        }

        /// <summary>
        /// Construye prompt para IA
        /// </summary>
        private string BuildPromptForDay(UserProfile profile, MuscleGroupDay day, Dictionary<string, List<ExerciseWithImage>> availableExercises, int dayNumber, bool shuffled)
        {
            // Calcular distribuciÃ³n de ejercicios por grupo
            int totalExercises = 5;
            var distribution = CalculateExerciseDistribution(day.MuscleGroups.Length, totalExercises);

            var exerciseList = new StringBuilder();
            exerciseList.AppendLine("**DISTRIBUCIÃ“N EXACTA DE EJERCICIOS:**");
            for (int i = 0; i < day.MuscleGroups.Length; i++)
            {
                exerciseList.AppendLine($"- {day.MuscleGroups[i]}: {distribution[i]} ejercicios");
            }
            exerciseList.AppendLine();

            foreach (var muscle in day.MuscleGroups)
            {
                if (availableExercises.TryGetValue(muscle, out var exercises))
                {
                    exerciseList.AppendLine($"=== {muscle.ToUpper()} ===");
                    var rand = new Random();
                    var list = shuffled ? exercises.OrderBy(_ => rand.Next()).ToList() : exercises;
                    foreach (var ex in list.Take(10))
                    {
                        exerciseList.AppendLine($"- {ex.Name}");
                    }
                    exerciseList.AppendLine();
                }
            }

            // Agregar variaciÃ³n al prompt
            var variationPhrase = new[] {
                "Selecciona ejercicios variados y efectivos",
                "Elige una combinaciÃ³n diferente de ejercicios",
                "VarÃ­a la selecciÃ³n de ejercicios para mantener el entrenamiento interesante",
                "Crea una rutina Ãºnica con ejercicios diversos"
            }[new Random().Next(4)];

            // Determinar series y reps segÃºn nivel y objetivo
            string goalsStr = profile.Goals != null ? string.Join(", ", profile.Goals) : "";
            string seriesRepsGuide = GetSeriesRepsGuide(profile.FitnessLevel, goalsStr);

            var prompt = $@"Eres un entrenador personal experto. Genera ejercicios para: {day.DayName}

CLIENTE: {profile.Name}, {profile.Age} aÃ±os, {profile.Gender}, {profile.FitnessLevel}
GRUPOS MUSCULARES: {string.Join(", ", day.MuscleGroups)}

EJERCICIOS DISPONIBLES:
{exerciseList}

REGLAS DE SERIES Y REPETICIONES:
{seriesRepsGuide}

**â”â”â” REGLA CRÃTICA - ORDEN ESTRICTO â”â”â”**
Debes generar EXACTAMENTE 5 EJERCICIOS en este orden OBLIGATORIO:

{string.Join("\n", distribution.Select((count, i) => $"{string.Join("\n", Enumerable.Range(distribution.Take(i).Sum(), count).Select(j => $"Ejercicio #{j + 1}: Seleccionar de === {day.MuscleGroups[i].ToUpper()} ==="))}"))}

**â”â”â” INSTRUCCIONES OBLIGATORIAS â”â”â”**
1. Generar SOLO 5 ejercicios (ni mÃ¡s ni menos)
2. COPIAR el nombre EXACTO de la lista (NO traducir, NO inventar)
3. Seguir el orden numÃ©rico de arriba (Ejercicio #1, #2, #3, #4, #5)
4. NO agregar texto extra ni explicaciones despuÃ©s del 5Âº ejercicio
5. NO usar emojis ni cÃ³digo markdown (```)
6. Respetar series/reps segÃºn nivel del cliente

**â”â”â” FORMATO OBLIGATORIO â”â”â”**
[EJERCICIO]Nombre copiado exacto de la lista
[SERIES]3 x 12
[INSTRUCCIONES]Mantener postura correcta y respirar bien
[FIN]

**GENERAR AHORA LOS 5 EJERCICIOS (DETENER DESPUÃ‰S DEL 5Âº [FIN]):**";

            return prompt;
        }

        /// <summary>
        /// Obtiene guÃ­a de series y repeticiones segÃºn nivel y objetivo
        /// </summary>
        private string GetSeriesRepsGuide(string fitnessLevel, string goals)
        {
            bool isFuerza = goals?.ToLower().Contains("fuerza") == true || goals?.ToLower().Contains("fuerz") == true;
            bool isAvanzado = fitnessLevel?.ToLower().Contains("avanzado") == true;

            if (isAvanzado && isFuerza)
            {
                return @"AVANZADO - FUERZA:
- Ejercicios compuestos pesados (Press Banca, Sentadilla, Peso Muerto, Press Militar): 3 x 6-8 reps (primera serie calentamiento progresivo)
- Ejercicios con cables o accesorios: 3 x 8-10 reps
- IMPORTANTE: Solo 3 series totales (la primera como calentamiento no se cuenta aparte)";
            }
            else if (isAvanzado)
            {
                return @"AVANZADO - HIPERTROFIA:
- Ejercicios compuestos: 4 x 8-10 reps
- Ejercicios aislados: 3 x 10-12 reps";
            }
            else if (fitnessLevel?.ToLower().Contains("intermedio") == true)
            {
                return @"INTERMEDIO:
- Ejercicios compuestos: 3-4 x 10-12 reps
- Ejercicios aislados: 3 x 12-15 reps";
            }
            else
            {
                return @"PRINCIPIANTE:
- Todos los ejercicios: 3 x 12-15 reps
- Enfoque en tÃ©cnica correcta";
            }
        }

        /// <summary>
        /// Llama a Ollama API
        /// </summary>
        private async Task<string> CallOllamaAPI(string prompt, bool alternative)
        {
            try
            {
                var requestBody = new
                {
                    model = _model,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = (alternative ? 0.9 : 0.7),
                        top_p = (alternative ? 0.95 : 0.9),
                        top_k = (alternative ? 80 : 40),         // Agregar top_k para mÃ¡s variaciÃ³n
                        num_predict = 3000,
                        seed = new Random().Next() // Seed aleatorio para evitar repeticiones
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_ollamaUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);

                if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                {
                    return responseElement.GetString() ?? "";
                }

                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calling Ollama: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Parsea respuesta de IA a WorkoutDay
        /// </summary>
        private WorkoutDay ParseAIResponseToWorkoutDay(string aiResponse, string dayName, string[] muscleGroups, Dictionary<string, List<ExerciseWithImage>> availableExercises)
        {
            var workoutDay = new WorkoutDay
            {
                Name = dayName,
                Exercises = new List<Exercise>(),
                MuscleGroups = muscleGroups
            };

            System.Diagnostics.Debug.WriteLine($"\n========== PARSEANDO RESPUESTA PARA {dayName} ==========");

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Respuesta de IA vacÃ­a");
                return workoutDay;
            }

            // Limpiar markdown (```) que a veces agrega la IA
            aiResponse = aiResponse.Replace("```", "").Trim();

            System.Diagnostics.Debug.WriteLine($"Longitud de respuesta: {aiResponse.Length} caracteres");
            System.Diagnostics.Debug.WriteLine($"Primeros 100 caracteres: {aiResponse.Substring(0, Math.Min(100, aiResponse.Length))}");

            // Parsear formato [EJERCICIO]...[SERIES]...[INSTRUCCIONES]...[FIN]
            var exerciseBlocks = aiResponse.Split(new[] { "[FIN]" }, StringSplitOptions.RemoveEmptyEntries);

            System.Diagnostics.Debug.WriteLine($"Total de bloques [FIN] encontrados: {exerciseBlocks.Length}");

            foreach (var block in exerciseBlocks)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"\n--- Procesando bloque ---");
                    System.Diagnostics.Debug.WriteLine($"Bloque: {block.Substring(0, Math.Min(100, block.Length))}...");

                    string exerciseName = "";
                    string series = "";
                    string instructions = string.Empty;

                    // Soportar mÃºltiples formatos:
                    // Formato 1: [EJERCICIO]Name [SERIES]... [INSTRUCCIONES]... (una o mÃºltiples lÃ­neas)
                    // Formato 2: [Nombre]Series [INSTRUCCIONES]...

                    var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var trimmed = lines[i].Trim();

                        // Detectar [EJERCICIO] o [EJERCIO] (con typo de IA)
                        if (trimmed.StartsWith("[EJERCICIO]") || trimmed.StartsWith("[EJERCIO]") ||
                            (trimmed.StartsWith("[Ejercicio") && trimmed.Contains("]"))) // TambiÃ©n [Ejercicio 1], [Ejercicio 2], etc.
                        {
                            var startTag = trimmed.StartsWith("[EJERCICIO]") ? "[EJERCICIO]" :
                                          trimmed.StartsWith("[EJERCIO]") ? "[EJERCIO]" :
                                          trimmed.Substring(0, trimmed.IndexOf(']') + 1);
                            var ejercicioIdx = startTag.Length;

                            // Caso 1: [EJERCICIO]Name [SERIES]... (en la misma lÃ­nea)
                            if (trimmed.Contains("[SERIES]") || trimmed.Contains("[SERIE]"))
                            {
                                var seriesTag = trimmed.Contains("[SERIES]") ? "[SERIES]" : "[SERIE]";
                                var seriesIdx = trimmed.IndexOf(seriesTag);
                                exerciseName = trimmed.Substring(ejercicioIdx, seriesIdx - ejercicioIdx).Trim();
                                var afterSeries = seriesIdx + seriesTag.Length;
                                var instrIdx = trimmed.IndexOf("[INSTRUCCIONES]");

                                if (instrIdx > afterSeries)
                                {
                                    series = trimmed.Substring(afterSeries, instrIdx - afterSeries).Trim();
                                }
                                else
                                {
                                    series = trimmed.Substring(afterSeries).Replace("]S", "").Trim(); // Fix [SERIE]S typo
                                }
                                System.Diagnostics.Debug.WriteLine($"  Formato 1a - Nombre: '{exerciseName}', Series: '{series}'");
                            }
                            // Caso 2: [EJERCICIO]Name (lÃ­nea separada, series viene despuÃ©s)
                            else
                            {
                                exerciseName = trimmed.Substring(ejercicioIdx).Trim();
                                System.Diagnostics.Debug.WriteLine($"  Formato 1b - Nombre: '{exerciseName}' (series en siguiente lÃ­nea)");

                                // Verificar si la siguiente lÃ­nea es directamente las series (sin tag [SERIES])
                                if (i + 1 < lines.Length)
                                {
                                    var nextLine = lines[i + 1].Trim();
                                    // Si la siguiente lÃ­nea parece series (ej: "3 x 12", "4x10")
                                    if (System.Text.RegularExpressions.Regex.IsMatch(nextLine, @"^\d+\s*x\s*\d+"))
                                    {
                                        series = nextLine;
                                        i++; // Saltar esta lÃ­nea en la prÃ³xima iteraciÃ³n
                                        System.Diagnostics.Debug.WriteLine($"  Series detectadas sin tag: '{series}'");
                                    }
                                }
                            }
                        }
                        // Detectar [SERIES] o [SERIE]S (con typo)
                        else if (trimmed.StartsWith("[SERIES]") || trimmed.StartsWith("[SERIE]"))
                        {
                            var seriesTag = trimmed.StartsWith("[SERIES]") ? "[SERIES]" : "[SERIE]";
                            series = trimmed.Substring(seriesTag.Length).Replace("]S", "").Trim(); // Fix [SERIE]S
                            System.Diagnostics.Debug.WriteLine($"  Series: '{series}'");
                        }
                        // Detectar lÃ­nea que parece series directamente (sin tag)
                        else if (string.IsNullOrEmpty(series) && !string.IsNullOrEmpty(exerciseName) &&
                                System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+\s*x\s*\d+"))
                        {
                            series = trimmed;
                            System.Diagnostics.Debug.WriteLine($"  Series sin tag detectadas: '{series}'");
                        }
                        // Formato 2: [Nombre]Series (sin etiquetas EJERCICIO/SERIES)
                        else if (trimmed.StartsWith("[") && trimmed.Contains("]") && !trimmed.Contains("[INSTRUCCIONES]") && string.IsNullOrEmpty(exerciseName))
                        {
                            var endBracket = trimmed.IndexOf("]");
                            exerciseName = trimmed.Substring(1, endBracket - 1).Trim();
                            var afterBracket = trimmed.Substring(endBracket + 1);

                            // Detectar si tiene [SERIES] o es directo
                            if (afterBracket.Contains("[SERIES]") || afterBracket.Contains("[SERIE]"))
                            {
                                var seriesTag = afterBracket.Contains("[SERIES]") ? "[SERIES]" : "[SERIE]";
                                var idx = afterBracket.IndexOf(seriesTag);
                                series = afterBracket.Substring(idx + seriesTag.Length).Replace("]S", "").Trim();
                            }
                            else
                            {
                                series = afterBracket.Trim();
                            }
                            System.Diagnostics.Debug.WriteLine($"  Formato 2 - Nombre: '{exerciseName}', Series: '{series}'");
                        }
                        // Detectar [INSTRUCCIONES]
                        else if (trimmed.StartsWith("[INSTRUCCIONES]"))
                        {
                            instructions = trimmed.Substring("[INSTRUCCIONES]".Length).Trim();
                            System.Diagnostics.Debug.WriteLine($"  Instrucciones: '{instructions}'");
                        }
                    }

                    if (!string.IsNullOrEmpty(exerciseName))
                    {
                        // Formatear series: "3x10" -> "3 x 10"
                        var formattedSeries = series.Replace("x", " x ");
                        if (formattedSeries.Contains("  ")) formattedSeries = formattedSeries.Replace("  ", " ");

                        System.Diagnostics.Debug.WriteLine($"âœ“ Ejercicio parseado correctamente: {exerciseName} - {formattedSeries}");

                        // Buscar imagen del ejercicio
                        var exerciseWithImage = _exerciseSearch.FindExerciseWithImage(exerciseName);

                        if (exerciseWithImage != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Imagen encontrada: {exerciseWithImage.Source}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  Sin imagen para: {exerciseName}");
                        }

                        workoutDay.Exercises.Add(new Exercise
                        {
                            Name = exerciseName,
                            SetsAndReps = formattedSeries,
                            Instructions = instructions,
                            ImageData = exerciseWithImage?.ImageData,
                            ImagePath = exerciseWithImage?.ImagePath ?? ""
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Bloque ignorado (sin nombre): {block.Substring(0, Math.Min(50, block.Length))}...");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parseando bloque: {ex.Message}");
                    continue;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total ejercicios parseados para {dayName}: {workoutDay.Exercises.Count}");

            // FALLBACK: Si no se parseÃ³ ningÃºn ejercicio, intentar un parseo mÃ¡s simple
            if (workoutDay.Exercises.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("âš ï¸ FALLBACK: Intentando parseo simple...");
                workoutDay.Exercises = ParseSimpleFormat(aiResponse, muscleGroups, availableExercises);
                System.Diagnostics.Debug.WriteLine($"Ejercicios parseados con fallback: {workoutDay.Exercises.Count}");
            }

            // POST-PROCESAMIENTO: Ordenar ejercicios por grupo muscular
            workoutDay.Exercises = ReorderExercisesByMuscleGroup(workoutDay.Exercises, muscleGroups, availableExercises);

            // Limitar a mÃ¡ximo 5 ejercicios (por si la IA genera de mÃ¡s)
            if (workoutDay.Exercises.Count > 5)
            {
                System.Diagnostics.Debug.WriteLine($"ADVERTENCIA: IA generÃ³ {workoutDay.Exercises.Count} ejercicios, limitando a 5");
                workoutDay.Exercises = workoutDay.Exercises.Take(5).ToList();
            }

            System.Diagnostics.Debug.WriteLine($"Ejercicios despuÃ©s de reordenar:");
            foreach (var ex in workoutDay.Exercises)
            {
                System.Diagnostics.Debug.WriteLine($"  - {ex.Name}");
            }

            return workoutDay;
        }

        /// <summary>
        /// Parseo simple de respuesta de IA cuando el formato estructurado falla
        /// </summary>
        private List<Exercise> ParseSimpleFormat(string aiResponse, string[] muscleGroups, Dictionary<string, List<ExerciseWithImage>> availableExercises)
        {
            var exercises = new List<Exercise>();

            try
            {
                // Intentar encontrar nombres de ejercicios en la respuesta
                // Buscar lÃ­neas que parezcan ejercicios: "- Ejercicio" o "1. Ejercicio" o simplemente nombres que coincidan
                var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    // Saltar lÃ­neas vacÃ­as o muy cortas
                    if (trimmed.Length < 5) continue;

                    // Saltar lÃ­neas que son headers o meta-info
                    if (trimmed.StartsWith("Eres ") || trimmed.StartsWith("CLIENTE") ||
                        trimmed.StartsWith("GRUPOS") || trimmed.StartsWith("EJERCICIOS") ||
                        trimmed.StartsWith("REGLAS") || trimmed.StartsWith("INSTRUCCIONES") ||
                        trimmed.Contains("â”â”â”") || trimmed.Contains("==="))
                        continue;

                    // Limpiar prefijos comunes
                    var cleaned = trimmed.TrimStart(new[] { '-', '*', '1','2','3','4','5','6','7','8','9','0','.', ' ', '\t' });

                    // Buscar coincidencias con ejercicios disponibles
                    foreach (var muscle in muscleGroups)
                    {
                        if (availableExercises.TryGetValue(muscle, out var muscleExercises))
                        {
                            foreach (var availEx in muscleExercises)
                            {
                                // Coincidencia exacta o parcial
                                if (cleaned.Contains(availEx.Name, StringComparison.OrdinalIgnoreCase) ||
                                    availEx.Name.Contains(cleaned, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Verificar que no lo hayamos agregado ya
                                    if (!exercises.Any(e => e.Name.Equals(availEx.Name, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        exercises.Add(new Exercise
                                        {
                                            Name = availEx.Name,
                                            SetsAndReps = "3 x 10",  // Por defecto
                                            Instructions = string.Empty,
                                            ImageData = availEx.ImageData,
                                            ImagePath = availEx.ImagePath ?? ""
                                        });

                                        System.Diagnostics.Debug.WriteLine($"  âœ“ Matched (simple): {availEx.Name}");

                                        // Limitar a 5 ejercicios
                                        if (exercises.Count >= 5)
                                            return exercises;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en parseo simple: {ex.Message}");
            }

            return exercises;
        }

        /// <summary>
        /// Reordena ejercicios por grupo muscular segÃºn el orden especificado
        /// </summary>
        private List<Exercise> ReorderExercisesByMuscleGroup(
            List<Exercise> exercises,
            string[] muscleGroups,
            Dictionary<string, List<ExerciseWithImage>> availableExercises)
        {
            if (exercises.Count == 0) return exercises;

            System.Diagnostics.Debug.WriteLine($"\n=== REORDENANDO EJERCICIOS ===");
            System.Diagnostics.Debug.WriteLine($"Grupos objetivo en orden: {string.Join(" â†’ ", muscleGroups)}");

            // Clasificar cada ejercicio por grupo muscular
            var exercisesByGroup = new Dictionary<string, List<Exercise>>();
            foreach (var muscle in muscleGroups)
            {
                exercisesByGroup[muscle] = new List<Exercise>();
            }

            foreach (var exercise in exercises)
            {
                // Buscar a quÃ© grupo pertenece este ejercicio
                string? foundGroup = null;
                foreach (var muscle in muscleGroups)
                {
                    if (availableExercises.TryGetValue(muscle, out var exercisesInGroup))
                    {
                        if (exercisesInGroup.Any(e =>
                            e.Name.Equals(exercise.Name, StringComparison.OrdinalIgnoreCase) ||
                            exercise.Name.Contains(e.Name, StringComparison.OrdinalIgnoreCase) ||
                            e.Name.Contains(exercise.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            foundGroup = muscle;
                            break;
                        }
                    }
                }

                if (foundGroup != null)
                {
                    exercisesByGroup[foundGroup].Add(exercise);
                    System.Diagnostics.Debug.WriteLine($"  '{exercise.Name}' â†’ {foundGroup}");
                }
                else
                {
                    // Buscar en TODOS los grupos disponibles para ver si pertenece a otro dÃ­a
                    bool belongsToOtherDay = false;
                    foreach (var kvp in availableExercises)
                    {
                        if (!muscleGroups.Contains(kvp.Key))
                        {
                            if (kvp.Value.Any(e =>
                                e.Name.Equals(exercise.Name, StringComparison.OrdinalIgnoreCase) ||
                                exercise.Name.Contains(e.Name, StringComparison.OrdinalIgnoreCase) ||
                                e.Name.Contains(exercise.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                belongsToOtherDay = true;
                                System.Diagnostics.Debug.WriteLine($"  '{exercise.Name}' pertenece a {kvp.Key} - DESCARTADO (no es del dÃ­a)");
                                break;
                            }
                        }
                    }

                    // Solo agregar si no pertenece a otro grupo del dÃ­a
                    if (!belongsToOtherDay)
                    {
                        exercisesByGroup[muscleGroups[0]].Add(exercise);
                        System.Diagnostics.Debug.WriteLine($"  '{exercise.Name}' â†’ {muscleGroups[0]} (por defecto)");
                    }
                }
            }

            // Reconstruir lista en el orden correcto
            var reordered = new List<Exercise>();
            foreach (var muscle in muscleGroups)
            {
                reordered.AddRange(exercisesByGroup[muscle]);
            }

            System.Diagnostics.Debug.WriteLine($"Orden final: {string.Join(", ", reordered.Select(e => e.Name))}");

            return reordered;
        }
    }

    /// <summary>
    /// Dia de division muscular
    /// </summary>
    public class MuscleGroupDay
    {
        public string DayName { get; set; }
        public string[] MuscleGroups { get; set; }
    }

    /// <summary>
    /// Respuesta de generacion con IA
    /// </summary>
    public class AIRoutineResponse
    {
        public bool Success { get; set; }
        public List<WorkoutDay> WorkoutDays { get; set; }
        public string Message { get; set; }
    }
}





