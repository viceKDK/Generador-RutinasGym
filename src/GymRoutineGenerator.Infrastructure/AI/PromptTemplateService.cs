using System.Text;
using GymRoutineGenerator.Core.Services;

namespace GymRoutineGenerator.Infrastructure.AI;

public class PromptTemplateService : IPromptTemplateService
{
    public async Task<string> BuildIntelligentRoutinePromptAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Placeholder for async operations

        var prompt = new StringBuilder();

        // System Context - Enhanced AI Persona
        prompt.AppendLine("Eres un entrenador personal certificado y especialista en ciencias del ejercicio con más de 15 años de experiencia.");
        prompt.AppendLine("Tu especialidad es crear rutinas de ejercicio científicamente fundamentadas, seguras y personalizadas.");
        prompt.AppendLine("Tienes experiencia trabajando con personas de todas las edades y niveles de condición física.");
        prompt.AppendLine();

        // Critical Instructions
        prompt.AppendLine("INSTRUCCIONES CRÍTICAS:");
        prompt.AppendLine("- Responde EXCLUSIVAMENTE en español argentino/mexicano");
        prompt.AppendLine("- Sigue ESTRICTAMENTE el formato especificado");
        prompt.AppendLine("- Considera TODAS las limitaciones físicas mencionadas");
        prompt.AppendLine("- Usa SOLO el equipamiento disponible especificado");
        prompt.AppendLine("- Aplica principios de periodización y progresión");
        prompt.AppendLine("- Incluye variaciones para diferentes niveles");
        prompt.AppendLine();

        // User Analysis
        BuildUserAnalysisSection(prompt, parameters);

        // Exercise Selection Strategy
        BuildExerciseSelectionStrategy(prompt, parameters);

        // Routine Structure Requirements
        BuildRoutineStructureRequirements(prompt, parameters);

        // Safety and Adaptation Guidelines
        BuildSafetyGuidelines(prompt, parameters);

        // Output Format Specification
        BuildOutputFormatSpecification(prompt, parameters);

        // Final Request
        prompt.AppendLine("SOLICITUD ESPECÍFICA:");
        prompt.AppendLine($"Crea una rutina de entrenamiento personalizada para {parameters.Name}, considerando todos los parámetros anteriores.");
        prompt.AppendLine("La rutina debe ser progresiva, segura y motivante.");
        prompt.AppendLine();
        prompt.AppendLine("GENERA LA RUTINA AHORA:");

        return prompt.ToString();
    }

    public async Task<string> BuildExerciseSelectionPromptAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var prompt = new StringBuilder();

        prompt.AppendLine("ANÁLISIS DE SELECCIÓN DE EJERCICIOS");
        prompt.AppendLine("=====================================");
        prompt.AppendLine();

        // Equipment Analysis
        prompt.AppendLine("EQUIPAMIENTO DISPONIBLE:");
        foreach (var equipment in parameters.AvailableEquipment)
        {
            prompt.AppendLine($"✓ {equipment}");
        }
        prompt.AppendLine();

        // Muscle Group Priority Analysis
        prompt.AppendLine("PRIORIDADES MUSCULARES:");
        foreach (var muscle in parameters.MuscleGroupPreferences.OrderBy(m => m.Priority))
        {
            prompt.AppendLine($"{muscle.Priority}. {muscle.MuscleGroup} - Énfasis: {muscle.EmphasisLevel}");
        }
        prompt.AppendLine();

        // Limitation Considerations
        if (parameters.PhysicalLimitations.Any())
        {
            prompt.AppendLine("LIMITACIONES A CONSIDERAR:");
            foreach (var limitation in parameters.PhysicalLimitations)
            {
                prompt.AppendLine($"⚠️ {limitation}");
            }
            prompt.AppendLine();
        }

        // Exercise Selection Request
        prompt.AppendLine("Basándote en este análisis, selecciona ejercicios que:");
        prompt.AppendLine("1. Usen SOLO el equipamiento disponible");
        prompt.AppendLine("2. Prioricen los grupos musculares según importancia");
        prompt.AppendLine("3. Respeten todas las limitaciones físicas");
        prompt.AppendLine("4. Sean apropiados para el nivel de experiencia");
        prompt.AppendLine("5. Permitan progresión a lo largo del tiempo");

        return prompt.ToString();
    }

    public async Task<string> BuildFallbackPromptAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var prompt = new StringBuilder();

        prompt.AppendLine("RUTINA BÁSICA PERSONALIZADA");
        prompt.AppendLine("===========================");
        prompt.AppendLine();

        prompt.AppendLine($"Cliente: {parameters.Name}");
        prompt.AppendLine($"Edad: {parameters.Age} años");
        prompt.AppendLine($"Días de entrenamiento: {parameters.TrainingDaysPerWeek} por semana");
        prompt.AppendLine($"Duración por sesión: {parameters.PreferredSessionDuration} minutos");
        prompt.AppendLine();

        prompt.AppendLine("Crea una rutina simple y efectiva con ejercicios básicos.");
        prompt.AppendLine("Incluye calentamiento, ejercicios principales y enfriamiento.");
        prompt.AppendLine("Especifica series, repeticiones y tiempo de descanso.");

        return prompt.ToString();
    }

    private void BuildUserAnalysisSection(StringBuilder prompt, UserRoutineParameters parameters)
    {
        prompt.AppendLine("ANÁLISIS DEL CLIENTE:");
        prompt.AppendLine("====================");
        prompt.AppendLine($"👤 Nombre: {parameters.Name}");
        prompt.AppendLine($"📅 Edad: {parameters.Age} años");
        prompt.AppendLine($"⚧ Género: {parameters.Gender}");
        prompt.AppendLine($"📊 Nivel: {parameters.ExperienceLevel}");
        prompt.AppendLine($"📆 Frecuencia: {parameters.TrainingDaysPerWeek} días/semana");
        prompt.AppendLine($"⏰ Duración: {parameters.PreferredSessionDuration} minutos/sesión");
        prompt.AppendLine($"🎯 Objetivo Principal: {parameters.PrimaryGoal}");
        prompt.AppendLine($"🏋️ Lugar: {parameters.GymType}");
        prompt.AppendLine($"💪 Intensidad Recomendada: {parameters.RecommendedIntensity}/5");
        prompt.AppendLine();

        // Physical Assessment
        if (parameters.PhysicalLimitations.Any())
        {
            prompt.AppendLine("⚠️ LIMITACIONES FÍSICAS IMPORTANTES:");
            foreach (var limitation in parameters.PhysicalLimitations)
            {
                prompt.AppendLine($"   • {limitation}");
            }
            prompt.AppendLine();
        }

        if (parameters.AvoidExercises.Any())
        {
            prompt.AppendLine("🚫 EJERCICIOS A EVITAR:");
            foreach (var exercise in parameters.AvoidExercises)
            {
                prompt.AppendLine($"   • {exercise}");
            }
            prompt.AppendLine();
        }
    }

    private void BuildExerciseSelectionStrategy(StringBuilder prompt, UserRoutineParameters parameters)
    {
        prompt.AppendLine("ESTRATEGIA DE SELECCIÓN DE EJERCICIOS:");
        prompt.AppendLine("====================================");

        // Equipment Available
        prompt.AppendLine("🔧 EQUIPAMIENTO DISPONIBLE:");
        if (parameters.AvailableEquipment.Any())
        {
            foreach (var equipment in parameters.AvailableEquipment)
            {
                prompt.AppendLine($"   ✓ {equipment}");
            }
        }
        else
        {
            prompt.AppendLine("   • Solo ejercicios corporales (sin equipamiento)");
        }
        prompt.AppendLine();

        // Muscle Group Priorities
        prompt.AppendLine("🎯 PRIORIDADES MUSCULARES:");
        var sortedMuscleGroups = parameters.MuscleGroupPreferences
            .OrderBy(mg => mg.Priority)
            .ThenByDescending(mg => GetEmphasisWeight(mg.EmphasisLevel));

        foreach (var muscleGroup in sortedMuscleGroups)
        {
            var emphasis = GetEmphasisIcon(muscleGroup.EmphasisLevel);
            prompt.AppendLine($"   {emphasis} {muscleGroup.MuscleGroup} (Prioridad {muscleGroup.Priority})");
        }
        prompt.AppendLine();

        // Selection Rules
        prompt.AppendLine("📋 REGLAS DE SELECCIÓN:");
        prompt.AppendLine("   1. Ejercicios compuestos antes que aislados");
        prompt.AppendLine("   2. Balancear músculos agonistas y antagonistas");
        prompt.AppendLine("   3. Progresión de dificultad apropiada");
        prompt.AppendLine("   4. Considerar fatiga acumulativa entre ejercicios");
        prompt.AppendLine("   5. Incluir variaciones para mantener interés");
        prompt.AppendLine();
    }

    private void BuildRoutineStructureRequirements(StringBuilder prompt, UserRoutineParameters parameters)
    {
        prompt.AppendLine("ESTRUCTURA REQUERIDA DE LA RUTINA:");
        prompt.AppendLine("=================================");

        // Calculate time allocation
        int warmupTime = Math.Max(5, parameters.PreferredSessionDuration / 10);
        int cooldownTime = Math.Max(5, parameters.PreferredSessionDuration / 12);
        int mainWorkoutTime = parameters.PreferredSessionDuration - warmupTime - cooldownTime;

        prompt.AppendLine($"⏱️ DISTRIBUCIÓN DE TIEMPO ({parameters.PreferredSessionDuration} min total):");
        prompt.AppendLine($"   🔥 Calentamiento: {warmupTime} minutos");
        prompt.AppendLine($"   💪 Ejercicios principales: {mainWorkoutTime} minutos");
        prompt.AppendLine($"   🧘 Enfriamiento: {cooldownTime} minutos");
        prompt.AppendLine();

        prompt.AppendLine("📊 PARÁMETROS DE ENTRENAMIENTO:");
        prompt.AppendLine($"   • Series: {GetRecommendedSets(parameters)}");
        prompt.AppendLine($"   • Repeticiones: {GetRecommendedReps(parameters)}");
        prompt.AppendLine($"   • Descanso: {GetRecommendedRest(parameters)}");
        prompt.AppendLine($"   • RPE objetivo: {GetRecommendedRPE(parameters)}/10");
        prompt.AppendLine();

        // Training Split Strategy
        prompt.AppendLine("🗓️ DISTRIBUCIÓN SEMANAL:");
        prompt.AppendLine(GetTrainingSplitStrategy(parameters));
        prompt.AppendLine();
    }

    private void BuildSafetyGuidelines(StringBuilder prompt, UserRoutineParameters parameters)
    {
        prompt.AppendLine("⚠️ PAUTAS DE SEGURIDAD OBLIGATORIAS:");
        prompt.AppendLine("===================================");

        // Age-specific considerations
        if (parameters.Age >= 65)
        {
            prompt.AppendLine("👥 CONSIDERACIONES PARA ADULTOS MAYORES:");
            prompt.AppendLine("   • Enfoque en equilibrio y estabilidad");
            prompt.AppendLine("   • Movimientos controlados y deliberados");
            prompt.AppendLine("   • Evitar cambios bruscos de posición");
            prompt.AppendLine("   • Incluir ejercicios funcionales");
        }
        else if (parameters.Age <= 18)
        {
            prompt.AppendLine("👥 CONSIDERACIONES PARA JÓVENES:");
            prompt.AppendLine("   • Enfoque en técnica antes que intensidad");
            prompt.AppendLine("   • Desarrollo atlético general");
            prompt.AppendLine("   • Evitar sobrecarga excesiva");
        }

        // Intensity guidelines
        prompt.AppendLine($"🎚️ INTENSIDAD ADAPTADA (Nivel {parameters.RecommendedIntensity}/5):");
        prompt.AppendLine(GetIntensityGuidelines(parameters.RecommendedIntensity));

        // General safety
        prompt.AppendLine("🛡️ SEGURIDAD GENERAL:");
        prompt.AppendLine("   • Siempre calentar antes de ejercicios intensos");
        prompt.AppendLine("   • Mantener hidratación constante");
        prompt.AppendLine("   • Parar si hay dolor agudo o mareos");
        prompt.AppendLine("   • Progresar gradualmente");
        prompt.AppendLine();
    }

    private void BuildOutputFormatSpecification(StringBuilder prompt, UserRoutineParameters parameters)
    {
        prompt.AppendLine("FORMATO DE RESPUESTA OBLIGATORIO:");
        prompt.AppendLine("================================");
        prompt.AppendLine();
        prompt.AppendLine("📋 **RUTINA DE ENTRENAMIENTO PERSONALIZADA**");
        prompt.AppendLine();
        prompt.AppendLine("👤 **RESUMEN DEL CLIENTE**");
        prompt.AppendLine("- Nombre: [nombre]");
        prompt.AppendLine("- Perfil: [edad] años, [nivel], [objetivo]");
        prompt.AppendLine("- Frecuencia: [días] días/semana, [duración] min/sesión");
        prompt.AppendLine();
        prompt.AppendLine("🎯 **OBJETIVOS DE LA RUTINA**");
        prompt.AppendLine("- Objetivo principal: [objetivo específico]");
        prompt.AppendLine("- Enfoque muscular: [grupos prioritarios]");
        prompt.AppendLine("- Adaptaciones especiales: [si las hay]");
        prompt.AppendLine();
        prompt.AppendLine("🔥 **CALENTAMIENTO** (5-10 min)");
        prompt.AppendLine("[Lista de ejercicios de calentamiento con duración]");
        prompt.AppendLine();
        prompt.AppendLine("💪 **EJERCICIOS PRINCIPALES**");
        prompt.AppendLine("[Para cada ejercicio especificar:]");
        prompt.AppendLine("**1. [Nombre del ejercicio]**");
        prompt.AppendLine("   - Músculos: [grupos musculares trabajados]");
        prompt.AppendLine("   - Series: [número] x Reps: [número/rango]");
        prompt.AppendLine("   - Descanso: [tiempo]");
        prompt.AppendLine("   - Técnica: [puntos clave de ejecución]");
        prompt.AppendLine("   - Progresión: [cómo aumentar dificultad]");
        prompt.AppendLine();
        prompt.AppendLine("🧘 **ENFRIAMIENTO** (5-10 min)");
        prompt.AppendLine("[Estiramientos específicos y relajación]");
        prompt.AppendLine();
        prompt.AppendLine("📊 **PROGRESIÓN SEMANAL**");
        prompt.AppendLine("- Semana 1-2: [parámetros iniciales]");
        prompt.AppendLine("- Semana 3-4: [primera progresión]");
        prompt.AppendLine("- Semana 5+: [progresión continua]");
        prompt.AppendLine();
        prompt.AppendLine("⚠️ **CONSEJOS DE SEGURIDAD**");
        prompt.AppendLine("[Recomendaciones específicas basadas en limitaciones]");
        prompt.AppendLine();
        prompt.AppendLine("💡 **CONSEJOS ADICIONALES**");
        prompt.AppendLine("[Nutrición, descanso, motivación]");
        prompt.AppendLine();
    }

    // Helper methods for parameter calculations
    private string GetRecommendedSets(UserRoutineParameters parameters)
    {
        return parameters.ExperienceLevel switch
        {
            "Principiante" => "2-3 series",
            "Intermedio" => "3-4 series",
            "Avanzado" => "4-5 series",
            _ => "3 series"
        };
    }

    private string GetRecommendedReps(UserRoutineParameters parameters)
    {
        return parameters.PrimaryGoal switch
        {
            "Fuerza" => "3-6 repeticiones",
            "Masa" => "6-12 repeticiones",
            "Resistencia" => "12-20 repeticiones",
            "Pérdida de peso" => "8-15 repeticiones",
            _ => "8-12 repeticiones"
        };
    }

    private string GetRecommendedRest(UserRoutineParameters parameters)
    {
        return parameters.PrimaryGoal switch
        {
            "Fuerza" => "2-3 minutos",
            "Masa" => "1-2 minutos",
            "Resistencia" => "30-60 segundos",
            "Pérdida de peso" => "30-90 segundos",
            _ => "60-90 segundos"
        };
    }

    private string GetRecommendedRPE(UserRoutineParameters parameters)
    {
        return parameters.RecommendedIntensity switch
        {
            1 => "4-5",
            2 => "5-6",
            3 => "6-7",
            4 => "7-8",
            5 => "8-9",
            _ => "6-7"
        };
    }

    private string GetTrainingSplitStrategy(UserRoutineParameters parameters)
    {
        return parameters.TrainingDaysPerWeek switch
        {
            2 => "   • Día 1: Cuerpo completo (enfoque superior)\n   • Día 2: Cuerpo completo (enfoque inferior)",
            3 => "   • Día 1: Tren superior\n   • Día 2: Tren inferior\n   • Día 3: Cuerpo completo",
            4 => "   • Día 1: Pecho/Tríceps\n   • Día 2: Espalda/Bíceps\n   • Día 3: Piernas/Glúteos\n   • Día 4: Hombros/Core",
            5 => "   • División por grupos musculares específicos\n   • Incluir día de cardio/acondicionamiento",
            6 => "   • Push/Pull/Legs repetido 2 veces\n   • O división por grupos musculares",
            _ => "   • Rutina de cuerpo completo adaptada"
        };
    }

    private string GetIntensityGuidelines(int intensity)
    {
        return intensity switch
        {
            1 => "   • Muy suave, enfoque en movilidad y técnica\n   • Sin fatiga significativa",
            2 => "   • Suave, puede conversar durante ejercicio\n   • Fatiga mínima",
            3 => "   • Moderada, conversación con cierto esfuerzo\n   • Fatiga ligera",
            4 => "   • Vigorosa, conversación difícil\n   • Fatiga moderada",
            5 => "   • Muy vigorosa, sin conversación\n   • Fatiga significativa",
            _ => "   • Intensidad moderada y progresiva"
        };
    }

    private int GetEmphasisWeight(string emphasis)
    {
        return emphasis switch
        {
            "Alto" => 3,
            "Medio" => 2,
            "Bajo" => 1,
            _ => 2
        };
    }

    private string GetEmphasisIcon(string emphasis)
    {
        return emphasis switch
        {
            "Alto" => "🔥🔥🔥",
            "Medio" => "🔥🔥",
            "Bajo" => "🔥",
            _ => "🔥🔥"
        };
    }
}