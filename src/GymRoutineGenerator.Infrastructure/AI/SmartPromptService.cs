using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Data.Repositories;
using System.Text;
using System.Text.Json;
using DataEntities = GymRoutineGenerator.Data.Entities;
using UserProfile = GymRoutineGenerator.Core.Models.UserProfile;
using UserRoutine = GymRoutineGenerator.Core.Models.UserRoutine;

namespace GymRoutineGenerator.Infrastructure.AI
{
    public class SmartPromptService : ISmartPromptService
    {
        private readonly IUserRepository _userRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILogger<SmartPromptService> _logger;
        private readonly Dictionary<string, PromptTemplate> _promptTemplates;

        public SmartPromptService(
            IUserRepository userRepository,
            IExerciseRepository exerciseRepository,
            ILogger<SmartPromptService> logger)
        {
            _userRepository = userRepository;
            _exerciseRepository = exerciseRepository;
            _logger = logger;
            _promptTemplates = InitializePromptTemplates();
        }

        public async Task<string> BuildContextualPromptAsync(UserRoutine routine, string userMessage, UserProfile profile)
        {
            try
            {
                var context = await BuildPromptContextAsync(profile, routine);

                var prompt = new StringBuilder();
                prompt.AppendLine("# ASISTENTE INTELIGENTE DE RUTINAS DE GIMNASIO");
                prompt.AppendLine();
                prompt.AppendLine("Eres un entrenador personal experto especializado en modificación de rutinas de ejercicio.");
                prompt.AppendLine("Responde en español de manera conversacional, amigable y profesional.");
                prompt.AppendLine();

                // User context
                prompt.AppendLine("## INFORMACIÓN DEL USUARIO:");
                prompt.AppendLine($"- Nombre: {profile.Name}");
                prompt.AppendLine($"- Edad: {profile.Age} años");
                prompt.AppendLine($"- Género: {profile.Gender}");
                prompt.AppendLine($"- Nivel de fitness: {profile.FitnessLevel}");
                prompt.AppendLine();

                // Physical limitations
                if (context.PhysicalLimitations.Any())
                {
                    prompt.AppendLine("## LIMITACIONES FÍSICAS:");
                    foreach (var limitation in context.PhysicalLimitations)
                    {
                        prompt.AppendLine($"- {limitation.LimitationType}: {limitation.Description} (Severidad: {limitation.Severity}/5)");
                        // Note: ExercisesToAvoid property to be added to UserPhysicalLimitation
                        // prompt.AppendLine($"  Evitar: {string.Join(", ", limitation.ExercisesToAvoid)}");
                    }
                    prompt.AppendLine();
                }

                // Equipment preferences
                if (context.EquipmentPreferences.Any())
                {
                    prompt.AppendLine("## EQUIPAMIENTO DISPONIBLE:");
                    var availableEquipment = context.EquipmentPreferences
                        .Where(e => e.IsAvailable)
                        .Select(e => e.EquipmentType);
                    prompt.AppendLine($"- {string.Join(", ", availableEquipment)}");
                    prompt.AppendLine();
                }

                // Current routine
                if (routine != null)
                {
                    prompt.AppendLine("## RUTINA ACTUAL:");
                    prompt.AppendLine($"- Nombre: {routine.Name}");
                    prompt.AppendLine($"- Creada: {routine.CreatedAt:dd/MM/yyyy}");
                    prompt.AppendLine("- Ejercicios:");

                    var routineData = await ExtractRoutineExercisesAsync(routine);
                    foreach (var exercise in routineData)
                    {
                        prompt.AppendLine($"  • {exercise}");
                    }
                    prompt.AppendLine();
                }

                // Conversation history
                if (context.PreviousInteractions.Any())
                {
                    prompt.AppendLine("## CONTEXTO DE CONVERSACIÓN RECIENTE:");
                    foreach (var interaction in context.PreviousInteractions.TakeLast(3))
                    {
                        prompt.AppendLine($"- {interaction}");
                    }
                    prompt.AppendLine();
                }

                // User message
                prompt.AppendLine("## MENSAJE DEL USUARIO:");
                prompt.AppendLine($"\"{userMessage}\"");
                prompt.AppendLine();

                // Instructions
                prompt.AppendLine("## INSTRUCCIONES:");
                prompt.AppendLine("1. Analiza el mensaje del usuario en el contexto de su rutina actual y perfil");
                prompt.AppendLine("2. Si solicita modificaciones, propón cambios específicos y seguros");
                prompt.AppendLine("3. Explica científicamente por qué recomiendas cada cambio");
                prompt.AppendLine("4. Considera siempre las limitaciones físicas del usuario");
                prompt.AppendLine("5. Mantén un tono conversacional y motivador");
                prompt.AppendLine("6. Si no estás seguro, pide aclaraciones específicas");
                prompt.AppendLine();

                _logger.LogInformation($"Built contextual prompt for user {profile.Name} with {prompt.Length} characters");
                return prompt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building contextual prompt");
                return BuildFallbackPrompt(userMessage, profile);
            }
        }

        public async Task<string> BuildExplanationPromptAsync(ExerciseModification modification)
        {
            try
            {
                var prompt = new StringBuilder();
                prompt.AppendLine("# EXPLICACIÓN DE MODIFICACIÓN DE EJERCICIO");
                prompt.AppendLine();
                prompt.AppendLine("Explica de manera clara y educativa la siguiente modificación de rutina:");
                prompt.AppendLine();

                prompt.AppendLine("## MODIFICACIÓN PROPUESTA:");
                prompt.AppendLine($"- Tipo: {modification.ModificationType}");
                prompt.AppendLine($"- Valor original: {modification.OriginalValue}");
                prompt.AppendLine($"- Nuevo valor: {modification.NewValue}");
                prompt.AppendLine($"- Razón: {modification.Reason}");
                prompt.AppendLine();

                if (modification.ExerciseId.HasValue)
                {
                    var exercise = await _exerciseRepository.GetByIdWithImagesAsync(modification.ExerciseId.Value);
                    if (exercise != null)
                    {
                        prompt.AppendLine("## EJERCICIO AFECTADO:");
                        prompt.AppendLine($"- Nombre: {exercise.Name}");
                        prompt.AppendLine($"- Músculo principal: {exercise.PrimaryMuscleGroup?.Name}");
                        prompt.AppendLine($"- Dificultad: {exercise.DifficultyLevel}");
                        prompt.AppendLine($"- Equipamiento: {exercise.EquipmentType?.Name}");
                        prompt.AppendLine();
                    }
                }

                prompt.AppendLine("## RESPUESTA REQUERIDA:");
                prompt.AppendLine("Proporciona una explicación que incluya:");
                prompt.AppendLine("1. **Por qué** esta modificación es beneficiosa");
                prompt.AppendLine("2. **Fundamento científico** detrás del cambio");
                prompt.AppendLine("3. **Qué esperar** como resultado de la modificación");
                prompt.AppendLine("4. **Consejos adicionales** para maximizar los beneficios");
                prompt.AppendLine();

                prompt.AppendLine("Mantén un tono educativo pero accesible, usando analogías cuando sea apropiado.");

                _logger.LogInformation($"Built explanation prompt for modification {modification.ModificationType}");
                return prompt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building explanation prompt");
                return "Explica por qué esta modificación es beneficiosa para la rutina del usuario.";
            }
        }

        public async Task<string> BuildSafetyValidationPromptAsync(ExerciseModification modification, UserProfile profile)
        {
            try
            {
                var context = await BuildPromptContextAsync(profile);

                var prompt = new StringBuilder();
                prompt.AppendLine("# VALIDACIÓN DE SEGURIDAD DE MODIFICACIÓN");
                prompt.AppendLine();
                prompt.AppendLine("Analiza la seguridad de la siguiente modificación considerando el perfil del usuario:");
                prompt.AppendLine();

                // User safety profile
                prompt.AppendLine("## PERFIL DE SEGURIDAD DEL USUARIO:");
                prompt.AppendLine($"- Edad: {profile.Age} años");
                prompt.AppendLine($"- Nivel de fitness: {profile.FitnessLevel}");
                prompt.AppendLine();

                if (context.PhysicalLimitations.Any())
                {
                    prompt.AppendLine("## LIMITACIONES FÍSICAS:");
                    foreach (var limitation in context.PhysicalLimitations)
                    {
                        prompt.AppendLine($"- {limitation.LimitationType}: {limitation.Description}");
                        prompt.AppendLine($"  Severidad: {limitation.Severity}/5");
                        if (limitation.ExercisesToAvoid?.Any() == true)
                        {
                            prompt.AppendLine($"  Ejercicios contraindicados: {string.Join(", ", limitation.ExercisesToAvoid)}");
                        }
                    }
                    prompt.AppendLine();
                }

                // Modification details
                prompt.AppendLine("## MODIFICACIÓN A VALIDAR:");
                prompt.AppendLine($"- Tipo: {modification.ModificationType}");
                prompt.AppendLine($"- Descripción: {modification.Description}");
                prompt.AppendLine($"- Justificación: {modification.Justification}");
                prompt.AppendLine();

                // Safety criteria
                prompt.AppendLine("## CRITERIOS DE EVALUACIÓN:");
                prompt.AppendLine("Evalúa la modificación basándose en:");
                prompt.AppendLine("1. **Progresión segura**: ¿El cambio respeta los principios de sobrecarga progresiva?");
                prompt.AppendLine("2. **Limitaciones médicas**: ¿Podría agravar alguna limitación física?");
                prompt.AppendLine("3. **Nivel de experiencia**: ¿Es apropiado para el nivel de fitness del usuario?");
                prompt.AppendLine("4. **Riesgo de lesión**: ¿Aumenta significativamente el riesgo?");
                prompt.AppendLine();

                prompt.AppendLine("## RESPUESTA REQUERIDA:");
                prompt.AppendLine("Responde en formato JSON:");
                prompt.AppendLine("```json");
                prompt.AppendLine("{");
                prompt.AppendLine("  \"esSafe\": true/false,");
                prompt.AppendLine("  \"nivelRiesgo\": \"BAJO|MEDIO|ALTO\",");
                prompt.AppendLine("  \"advertencias\": [\"lista de advertencias específicas\"],");
                prompt.AppendLine("  \"recomendaciones\": [\"sugerencias para hacer la modificación más segura\"],");
                prompt.AppendLine("  \"requiereSupervision\": true/false,");
                prompt.AppendLine("  \"explicacion\": \"breve explicación del análisis\"");
                prompt.AppendLine("}");
                prompt.AppendLine("```");

                _logger.LogInformation($"Built safety validation prompt for modification {modification.ModificationType}");
                return prompt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building safety validation prompt");
                return "Evalúa la seguridad de esta modificación para el usuario.";
            }
        }

        public async Task<string> BuildExerciseSearchPromptAsync(string userDescription, UserProfile profile)
        {
            try
            {
                var context = await BuildPromptContextAsync(profile);

                var prompt = new StringBuilder();
                prompt.AppendLine("# BÚSQUEDA INTELIGENTE DE EJERCICIOS");
                prompt.AppendLine();
                prompt.AppendLine("Encuentra ejercicios basándote en la descripción del usuario y su perfil:");
                prompt.AppendLine();

                // User profile
                prompt.AppendLine("## PERFIL DEL USUARIO:");
                prompt.AppendLine($"- Nivel: {profile.FitnessLevel}");
                prompt.AppendLine($"- Edad: {profile.Age} años");
                prompt.AppendLine();

                // Available equipment
                if (context.EquipmentPreferences.Any(e => e.IsAvailable))
                {
                    var equipment = context.EquipmentPreferences
                        .Where(e => e.IsAvailable)
                        .Select(e => e.EquipmentType);
                    prompt.AppendLine("## EQUIPAMIENTO DISPONIBLE:");
                    prompt.AppendLine($"- {string.Join(", ", equipment)}");
                    prompt.AppendLine();
                }

                // Physical limitations
                if (context.PhysicalLimitations.Any())
                {
                    prompt.AppendLine("## RESTRICCIONES:");
                    foreach (var limitation in context.PhysicalLimitations)
                    {
                        prompt.AppendLine($"- Evitar ejercicios que afecten: {limitation.LimitationType}");
                        if (limitation.ExercisesToAvoid?.Any() == true)
                        {
                            prompt.AppendLine($"  Específicamente evitar: {string.Join(", ", limitation.ExercisesToAvoid)}");
                        }
                    }
                    prompt.AppendLine();
                }

                // User search description
                prompt.AppendLine("## DESCRIPCIÓN DEL USUARIO:");
                prompt.AppendLine($"\"{userDescription}\"");
                prompt.AppendLine();

                // Instructions
                prompt.AppendLine("## INSTRUCCIONES:");
                prompt.AppendLine("1. Interpreta la descripción para identificar:");
                prompt.AppendLine("   - Grupos musculares objetivo");
                prompt.AppendLine("   - Tipo de movimiento deseado");
                prompt.AppendLine("   - Nivel de dificultad implícito");
                prompt.AppendLine("   - Equipamiento mencionado o implícito");
                prompt.AppendLine();
                prompt.AppendLine("2. Sugiere 3-5 ejercicios específicos que coincidan");
                prompt.AppendLine("3. Para cada ejercicio, explica por qué coincide con la descripción");
                prompt.AppendLine("4. Considera las limitaciones y equipamiento del usuario");
                prompt.AppendLine();

                prompt.AppendLine("Responde con ejercicios específicos y sus justificaciones.");

                _logger.LogInformation($"Built exercise search prompt for description: {userDescription}");
                return prompt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building exercise search prompt");
                return $"Encuentra ejercicios que coincidan con: {userDescription}";
            }
        }

        public async Task<string> BuildProgressionAnalysisPromptAsync(UserProfile profile, List<UserRoutine> routineHistory)
        {
            try
            {
                var prompt = new StringBuilder();
                prompt.AppendLine("# ANÁLISIS DE PROGRESIÓN DE ENTRENAMIENTO");
                prompt.AppendLine();
                prompt.AppendLine("Analiza el progreso del usuario basándote en su historial de rutinas:");
                prompt.AppendLine();

                // User profile
                prompt.AppendLine("## PERFIL DEL USUARIO:");
                prompt.AppendLine($"- Nombre: {profile.Name}");
                prompt.AppendLine($"- Edad: {profile.Age} años");
                prompt.AppendLine($"- Nivel actual: {profile.FitnessLevel}");
                prompt.AppendLine($"- Tiempo entrenando: {CalculateTrainingDuration(routineHistory)}");
                prompt.AppendLine();

                // Routine history
                if (routineHistory.Any())
                {
                    prompt.AppendLine("## HISTORIAL DE RUTINAS:");
                    prompt.AppendLine($"- Total de rutinas generadas: {routineHistory.Count}");
                    prompt.AppendLine("- Rutinas recientes:");

                    foreach (var routine in routineHistory.OrderByDescending(r => r.CreatedAt).Take(5))
                    {
                        prompt.AppendLine($"  • {routine.Name} (Creada: {routine.CreatedAt:dd/MM/yyyy})");
                        prompt.AppendLine($"    Estado: {routine.Status}, Rating: {routine.Rating}/5");
                        if (!string.IsNullOrEmpty(routine.Notes))
                        {
                            prompt.AppendLine($"    Notas: {routine.Notes}");
                        }
                    }
                    prompt.AppendLine();
                }

                // Analysis instructions
                prompt.AppendLine("## ANÁLISIS REQUERIDO:");
                prompt.AppendLine("Proporciona un análisis que incluya:");
                prompt.AppendLine("1. **Patrones de entrenamiento**: ¿Qué tendencias observas?");
                prompt.AppendLine("2. **Progresión aparente**: ¿Ha aumentado la complejidad/intensidad?");
                prompt.AppendLine("3. **Áreas de enfoque**: ¿En qué músculos/objetivos se centra más?");
                prompt.AppendLine("4. **Sugerencias de progresión**: ¿Qué debería ser el siguiente paso?");
                prompt.AppendLine("5. **Posibles mejoras**: ¿Qué aspectos podrían optimizarse?");
                prompt.AppendLine();

                prompt.AppendLine("Mantén un tono motivador y constructivo en tu análisis.");

                _logger.LogInformation($"Built progression analysis prompt for {profile.Name} with {routineHistory.Count} routines");
                return prompt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building progression analysis prompt");
                return "Analiza el progreso del usuario basándote en su historial de entrenamiento.";
            }
        }

        public async Task<string> BuildRoutineOptimizationPromptAsync(UserRoutine routine, UserProfile profile)
        {
            try
            {
                var context = await BuildPromptContextAsync(profile, routine);

                var prompt = new StringBuilder();
                prompt.AppendLine("# OPTIMIZACIÓN DE RUTINA DE ENTRENAMIENTO");
                prompt.AppendLine();
                prompt.AppendLine("Analiza y optimiza la siguiente rutina considerando el perfil del usuario:");
                prompt.AppendLine();

                // User context
                prompt.AppendLine("## PERFIL DEL USUARIO:");
                prompt.AppendLine($"- Nombre: {profile.Name}");
                prompt.AppendLine($"- Edad: {profile.Age} años");
                prompt.AppendLine($"- Nivel: {profile.FitnessLevel}");
                prompt.AppendLine();

                // Current routine
                prompt.AppendLine("## RUTINA ACTUAL:");
                prompt.AppendLine($"- Nombre: {routine.Name}");
                prompt.AppendLine($"- Creada: {routine.CreatedAt:dd/MM/yyyy}");
                prompt.AppendLine($"- Rating actual: {routine.Rating}/5");

                var exercises = await ExtractRoutineExercisesAsync(routine);
                prompt.AppendLine("- Ejercicios:");
                foreach (var exercise in exercises)
                {
                    prompt.AppendLine($"  • {exercise}");
                }
                prompt.AppendLine();

                // User preferences and limitations
                if (context.MuscleGroupPreferences.Any())
                {
                    prompt.AppendLine("## PREFERENCIAS DE GRUPOS MUSCULARES:");
                    var preferences = context.MuscleGroupPreferences
                        .OrderByDescending(p => p.Priority)
                        .Take(5);
                    foreach (var pref in preferences)
                    {
                        prompt.AppendLine($"- {pref.MuscleGroup}: Prioridad {pref.Priority}/5");
                    }
                    prompt.AppendLine();
                }

                if (context.PhysicalLimitations.Any())
                {
                    prompt.AppendLine("## LIMITACIONES A CONSIDERAR:");
                    foreach (var limitation in context.PhysicalLimitations)
                    {
                        prompt.AppendLine($"- {limitation.LimitationType}: {limitation.Description}");
                    }
                    prompt.AppendLine();
                }

                // Optimization criteria
                prompt.AppendLine("## CRITERIOS DE OPTIMIZACIÓN:");
                prompt.AppendLine("Evalúa y mejora la rutina en base a:");
                prompt.AppendLine("1. **Balance muscular**: ¿Están todos los grupos principales representados?");
                prompt.AppendLine("2. **Progresión lógica**: ¿El orden de ejercicios es óptimo?");
                prompt.AppendLine("3. **Intensidad apropiada**: ¿Es adecuada para el nivel del usuario?");
                prompt.AppendLine("4. **Variedad**: ¿Hay suficiente variedad para mantener el interés?");
                prompt.AppendLine("5. **Eficiencia**: ¿Se maximiza el tiempo de entrenamiento?");
                prompt.AppendLine();

                prompt.AppendLine("## RESPUESTA ESPERADA:");
                prompt.AppendLine("Proporciona:");
                prompt.AppendLine("1. **Análisis de la rutina actual** (fortalezas y debilidades)");
                prompt.AppendLine("2. **Sugerencias específicas de mejora**");
                prompt.AppendLine("3. **Alternativas de ejercicios** si es necesario");
                prompt.AppendLine("4. **Justificación científica** para cada sugerencia");

                _logger.LogInformation($"Built optimization prompt for routine {routine.Name}");
                return prompt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building optimization prompt");
                return "Analiza y sugiere mejoras para optimizar esta rutina de entrenamiento.";
            }
        }

        public async Task<string> BuildAlternativeExercisePromptAsync(Exercise currentExercise, UserProfile profile, string reason)
        {
            try
            {
                var context = await BuildPromptContextAsync(profile);

                var prompt = new StringBuilder();
                prompt.AppendLine("# BÚSQUEDA DE EJERCICIOS ALTERNATIVOS");
                prompt.AppendLine();
                prompt.AppendLine("Encuentra ejercicios alternativos para el ejercicio actual:");
                prompt.AppendLine();

                // Current exercise details
                prompt.AppendLine("## EJERCICIO ACTUAL:");
                prompt.AppendLine($"- Nombre: {currentExercise.Name}");
                prompt.AppendLine($"- Músculo principal: {string.Join(", ", currentExercise.MuscleGroups)}");
                prompt.AppendLine($"- Dificultad: {currentExercise.DifficultyLevel}");
                prompt.AppendLine($"- Equipamiento: {currentExercise.Equipment}");
                prompt.AppendLine($"- Tipo de ejercicio: {currentExercise.ExerciseType}");
                if (!string.IsNullOrEmpty(currentExercise.Description))
                {
                    prompt.AppendLine($"- Descripción: {currentExercise.Description}");
                }
                prompt.AppendLine();

                // Reason for seeking alternatives
                prompt.AppendLine("## RAZÓN PARA BUSCAR ALTERNATIVAS:");
                prompt.AppendLine($"{reason}");
                prompt.AppendLine();

                // User constraints
                prompt.AppendLine("## PERFIL DEL USUARIO:");
                prompt.AppendLine($"- Nivel: {profile.FitnessLevel}");
                prompt.AppendLine($"- Edad: {profile.Age} años");
                prompt.AppendLine();

                if (context.EquipmentPreferences.Any(e => e.IsAvailable))
                {
                    var equipment = context.EquipmentPreferences
                        .Where(e => e.IsAvailable)
                        .Select(e => e.EquipmentType);
                    prompt.AppendLine("## EQUIPAMIENTO DISPONIBLE:");
                    prompt.AppendLine($"- {string.Join(", ", equipment)}");
                    prompt.AppendLine();
                }

                if (context.PhysicalLimitations.Any())
                {
                    prompt.AppendLine("## LIMITACIONES FÍSICAS:");
                    foreach (var limitation in context.PhysicalLimitations)
                    {
                        prompt.AppendLine($"- {limitation.LimitationType}: {limitation.Description}");
                        if (limitation.ExercisesToAvoid?.Any() == true)
                        {
                            prompt.AppendLine($"  Evitar: {string.Join(", ", limitation.ExercisesToAvoid)}");
                        }
                    }
                    prompt.AppendLine();
                }

                // Instructions
                prompt.AppendLine("## INSTRUCCIONES:");
                prompt.AppendLine("Sugiere 3-5 ejercicios alternativos que:");
                prompt.AppendLine("1. **Trabajen el mismo músculo principal** que el ejercicio actual");
                prompt.AppendLine("2. **Respeten las limitaciones** del usuario");
                prompt.AppendLine("3. **Usen equipamiento disponible** del usuario");
                prompt.AppendLine("4. **Sean apropiados para su nivel** de fitness");
                prompt.AppendLine("5. **Aborden la razón específica** mencionada");
                prompt.AppendLine();

                prompt.AppendLine("Para cada alternativa proporciona:");
                prompt.AppendLine("- Nombre del ejercicio");
                prompt.AppendLine("- Por qué es una buena alternativa");
                prompt.AppendLine("- Ventajas sobre el ejercicio original");
                prompt.AppendLine("- Consideraciones especiales");

                _logger.LogInformation($"Built alternative exercise prompt for {currentExercise.Name}");
                return prompt.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building alternative exercise prompt");
                return $"Encuentra ejercicios alternativos para {currentExercise.Name} porque: {reason}";
            }
        }

        #region Private Helper Methods

        private async Task<PromptContext> BuildPromptContextAsync(UserProfile profile, UserRoutine? currentRoutine = null)
        {
            var context = new PromptContext
            {
                UserProfile = profile,
                CurrentRoutine = currentRoutine
            };

            try
            {
                // For now, initialize with empty lists since we need to convert between Core.Models and Data.Entities
                context.PhysicalLimitations = new List<UserPhysicalLimitation>();
                context.EquipmentPreferences = new List<UserEquipmentPreference>();
                context.MuscleGroupPreferences = new List<UserMuscleGroupPreference>();
                context.RoutineHistory = new List<UserRoutine>();

                _logger.LogInformation($"Built prompt context for user {profile.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building prompt context for user {profile.Id}");
            }

            return context;
        }

        private async Task<List<string>> ExtractRoutineExercisesAsync(UserRoutine routine)
        {
            try
            {
                // Access exercises directly instead of RoutineData
                var exercises = routine.Exercises;
                if (exercises == null || !exercises.Any())
                    return new List<string> { "No hay ejercicios en la rutina" };

                // Extract exercise names
                return exercises.Select(e => e.Name).Where(name => !string.IsNullOrEmpty(name)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting exercises from routine {routine.Id}");
                return new List<string> { "Error: No se pudieron extraer los ejercicios" };
            }
        }

        private string CalculateTrainingDuration(List<UserRoutine> routineHistory)
        {
            if (!routineHistory.Any())
                return "Sin historial";

            var firstRoutine = routineHistory.OrderBy(r => r.CreatedDate).First();
            var timeSpan = DateTime.UtcNow - firstRoutine.CreatedDate;

            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} días";
            else if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} meses";
            else
                return $"{(int)(timeSpan.TotalDays / 365)} años";
        }

        private string BuildFallbackPrompt(string userMessage, UserProfile profile)
        {
            return $@"
Eres un entrenador personal experto. El usuario {profile.Name} (edad: {profile.Age}, nivel: {profile.FitnessLevel})
te pregunta: ""{userMessage}""

Responde de manera profesional y útil, considerando su perfil.";
        }

        private Dictionary<string, PromptTemplate> InitializePromptTemplates()
        {
            return new Dictionary<string, PromptTemplate>
            {
                ["conversational"] = new PromptTemplate
                {
                    Name = "Conversational",
                    Type = PromptType.Conversational,
                    Description = "Template for natural conversation about fitness",
                    RequiredVariables = new List<string> { "userProfile", "userMessage", "context" }
                },
                ["safety"] = new PromptTemplate
                {
                    Name = "Safety Validation",
                    Type = PromptType.Safety,
                    Description = "Template for validating exercise safety",
                    RequiredVariables = new List<string> { "userProfile", "modification", "limitations" }
                },
                ["search"] = new PromptTemplate
                {
                    Name = "Exercise Search",
                    Type = PromptType.Search,
                    Description = "Template for intelligent exercise search",
                    RequiredVariables = new List<string> { "userProfile", "searchQuery", "constraints" }
                }
            };
        }

        #endregion
    }
}