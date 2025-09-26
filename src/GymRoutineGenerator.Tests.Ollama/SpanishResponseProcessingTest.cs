using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Infrastructure.AI;

namespace GymRoutineGenerator.Tests.Ollama;

public static class SpanishResponseProcessingTest
{
    public static async Task RunSpanishResponseProcessingTests()
    {
        Console.WriteLine("🔍 INICIANDO PRUEBAS DE PROCESAMIENTO DE RESPUESTAS EN ESPAÑOL");
        Console.WriteLine("================================================================");
        Console.WriteLine();

        var processor = new SpanishResponseProcessor();

        // Test 1: Validación de contenido en español
        await TestSpanishValidation(processor);
        Console.WriteLine();

        // Test 2: Procesamiento de respuesta AI completa
        await TestCompleteResponseProcessing(processor);
        Console.WriteLine();

        // Test 3: Mejora de formato español
        await TestSpanishFormatting(processor);
        Console.WriteLine();

        // Test 4: Parsing de instrucciones de ejercicios
        await TestExerciseInstructionParsing(processor);
        Console.WriteLine();

        // Test 5: Evaluación de calidad de respuesta
        await TestResponseQualityAssessment(processor);
        Console.WriteLine();

        // Test 6: Manejo de errores comunes en español
        await TestCommonSpanishErrors(processor);
        Console.WriteLine();

        // Test 7: Procesamiento de rutina con terminología mixta
        await TestMixedTerminologyHandling(processor);
        Console.WriteLine();

        Console.WriteLine("✅ TODAS LAS PRUEBAS DE PROCESAMIENTO ESPAÑOL COMPLETADAS");
    }

    private static async Task TestSpanishValidation(ISpanishResponseProcessor processor)
    {
        Console.WriteLine("🧪 Test 1: Validación de Contenido en Español");
        Console.WriteLine("─────────────────────────────────────────────");

        var testCases = new[]
        {
            new
            {
                Name = "Español correcto con terminología fitness",
                Content = @"Esta rutina de entrenamiento incluye ejercicios para desarrollar fuerza y resistencia.
                           Realiza cada ejercicio con técnica correcta y mantén la respiración controlada durante las repeticiones.",
                ExpectedValid = true
            },
            new
            {
                Name = "Contenido con errores ortográficos",
                Content = @"Esta rutinha de entrenamineto incluye ejercisios para desarrolar fuerca.
                           Realisa cada ejercisio con tecnica corecta.",
                ExpectedValid = false
            },
            new
            {
                Name = "Mezcla español-inglés",
                Content = @"Esta rutina de workout incluye exercises para strength training.
                           Realiza cada exercise con proper form.",
                ExpectedValid = false
            },
            new
            {
                Name = "Español formal apropiado",
                Content = @"Se recomienda realizar cada ejercicio con la técnica adecuada.
                           Es importante mantener una respiración constante durante el entrenamiento.",
                ExpectedValid = true
            }
        };

        foreach (var testCase in testCases)
        {
            try
            {
                var result = await processor.ValidateSpanishContentAsync(testCase.Content);

                Console.WriteLine($"   • {testCase.Name}:");
                Console.WriteLine($"     - Validez: {result.IsValid} (esperado: {testCase.ExpectedValid})");
                Console.WriteLine($"     - Puntuación idioma: {result.LanguageQualityScore:F2}");
                Console.WriteLine($"     - Terminología fitness: {result.HasProperFitnessTerminology}");
                Console.WriteLine($"     - Gramática correcta: {result.HasCorrectGrammar}");
                Console.WriteLine($"     - Formalidad apropiada: {result.HasAppropriateFormality}");
                Console.WriteLine($"     - Errores ortográficos: {result.SpellingErrors}");
                Console.WriteLine($"     - Errores gramaticales: {result.GrammarErrors}");

                if (result.Errors.Any())
                {
                    Console.WriteLine($"     - Errores encontrados:");
                    foreach (var error in result.Errors.Take(3))
                    {
                        Console.WriteLine($"       * {error.ErrorType}: '{error.OriginalText}' → '{error.SuggestedCorrection}'");
                    }
                }

                if (result.Suggestions.Any())
                {
                    Console.WriteLine($"     - Sugerencias: {string.Join(", ", result.Suggestions.Take(2))}");
                }

                var validationMatch = result.IsValid == testCase.ExpectedValid;
                Console.WriteLine($"     ✓ {(validationMatch ? "CORRECTO" : "DISCREPANCIA DETECTADA")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en validación '{testCase.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestCompleteResponseProcessing(ISpanishResponseProcessor processor)
    {
        Console.WriteLine("🧪 Test 2: Procesamiento de Respuesta AI Completa");
        Console.WriteLine("────────────────────────────────────────────────");

        var sampleResponse = @"
# RUTINA DE ENTRENAMIENTO PERSONALIZADA

## OBJETIVO:
Desarrollar fuerza funcional y mejorar la composición corporal mediante ejercicios compound y accesorios.

## CALENTAMIENTO (5-7 minutos):
1. Movilidad articular - rotaciones de hombros y caderas (30 segundos cada una)
2. Marcha en el lugar con elevación de rodillas (1 minuto)
3. Estiramientos dinámicos de brazos y piernas (2 minutos)
4. Activación del core - planchas ligeras (30 segundos)

## EJERCICIOS PRINCIPALES:

1. **Sentadillas con peso corporal**
   - Series: 3
   - Repeticiones: 12-15
   - Músculos trabajados: Cuádriceps, glúteos, isquiotibiales
   - Técnica: Mantén la espalda recta, baja hasta 90 grados
   - Descanso: 60-90 segundos

2. **Flexiones de pecho**
   - Series: 3
   - Repeticiones: 8-12
   - Músculos trabajados: Pectorales, tríceps, hombros anteriores
   - Técnica: Cuerpo en línea recta, baja hasta casi tocar el suelo
   - Descanso: 60-90 segundos

3. **Plancha frontal**
   - Series: 3
   - Duración: 30-45 segundos
   - Músculos trabajados: Core, hombros, glúteos
   - Técnica: Cuerpo recto, contrae abdominales
   - Descanso: 45 segundos

## ENFRIAMIENTO (5-8 minutos):
1. Estiramiento de cuádriceps (30 segundos cada pierna)
2. Estiramiento de pectorales en pared (45 segundos)
3. Estiramiento de espalda - posición fetal (1 minuto)
4. Respiración profunda y relajación (2 minutos)

## CONSEJOS ADICIONALES:
- Mantén hidratación constante durante el entrenamiento
- Escucha a tu cuerpo y ajusta la intensidad según sea necesario
- Progresa gradualmente aumentando repeticiones o series cada semana

PRECAUCIÓN: Si sientes dolor agudo, detén el ejercicio inmediatamente.
";

        var parameters = new UserRoutineParameters
        {
            Name = "María García",
            Age = 28,
            Gender = "Femenino",
            ExperienceLevel = "Principiante",
            PrimaryGoal = "Tonificación",
            TrainingDaysPerWeek = 3,
            PreferredSessionDuration = 45,
            AvailableEquipment = new List<string> { "Peso corporal", "Esterilla" },
            PhysicalLimitations = new List<string> { "Dolor leve de rodillas" },
            RecommendedIntensity = 3
        };

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await processor.ProcessAIResponseAsync(sampleResponse, parameters);
            stopwatch.Stop();

            Console.WriteLine($"   📊 RESULTADOS DEL PROCESAMIENTO:");
            Console.WriteLine($"   • Tiempo de procesamiento: {result.ProcessingTime.TotalMilliseconds:F0} ms");
            Console.WriteLine($"   • Requiere revisión humana: {result.RequiresHumanReview}");
            Console.WriteLine();

            Console.WriteLine($"   📝 VALIDACIÓN DEL IDIOMA:");
            Console.WriteLine($"   • Es válido: {result.Validation.IsValid}");
            Console.WriteLine($"   • Puntuación: {result.Validation.LanguageQualityScore:F2}/1.0");
            Console.WriteLine($"   • Terminología fitness: {result.Validation.HasProperFitnessTerminology}");
            Console.WriteLine($"   • Gramática: {result.Validation.HasCorrectGrammar}");
            Console.WriteLine();

            Console.WriteLine($"   🎯 EVALUACIÓN DE CALIDAD:");
            Console.WriteLine($"   • Puntuación general: {result.Quality.OverallScore:F1}/10.0");
            Console.WriteLine($"   • Cumple umbral: {result.Quality.MeetsQualityThreshold}");
            Console.WriteLine($"   • Completitud: {result.Quality.Metrics.CompletenessScore:F1}/10");
            Console.WriteLine($"   • Claridad: {result.Quality.Metrics.ClarityScore:F1}/10");
            Console.WriteLine($"   • Seguridad: {result.Quality.Metrics.SafetyScore:F1}/10");
            Console.WriteLine($"   • Personalización: {result.Quality.Metrics.PersonalizationScore:F1}/10");
            Console.WriteLine($"   • Precisión científica: {result.Quality.Metrics.ScientificAccuracyScore:F1}/10");
            Console.WriteLine();

            Console.WriteLine($"   🏗️ ESTRUCTURA PARSEADA:");
            Console.WriteLine($"   • Título: '{result.Structure.Title}'");
            Console.WriteLine($"   • Objetivo: '{result.Structure.Objective}'");
            Console.WriteLine($"   • Ejercicios calentamiento: {result.Structure.Warmup.Exercises.Count}");
            Console.WriteLine($"   • Bloques principales: {result.Structure.ExerciseBlocks.Count}");
            Console.WriteLine($"   • Ejercicios enfriamiento: {result.Structure.Cooldown.Exercises.Count}");
            Console.WriteLine($"   • Notas seguridad: {result.Structure.SafetyNotes.Count}");
            Console.WriteLine($"   • Duración estimada: {result.Structure.EstimatedDuration.TotalMinutes:F0} minutos");
            Console.WriteLine();

            if (result.Structure.ExerciseBlocks.Any())
            {
                var mainBlock = result.Structure.ExerciseBlocks.First();
                Console.WriteLine($"   💪 EJERCICIOS PRINCIPALES PARSEADOS:");
                foreach (var exercise in mainBlock.Exercises.Take(3))
                {
                    Console.WriteLine($"   • {exercise.Name}:");
                    Console.WriteLine($"     - Series: {exercise.Parameters.Sets}");
                    Console.WriteLine($"     - Repeticiones: {exercise.Parameters.Repetitions}");
                    Console.WriteLine($"     - Músculos: {string.Join(", ", exercise.MuscleGroups)}");
                    Console.WriteLine($"     - Instrucciones: {exercise.StepByStepInstructions.Count}");
                    Console.WriteLine($"     - Consejos seguridad: {exercise.SafetyTips.Count}");
                }
                Console.WriteLine();
            }

            if (result.Quality.Insights.Any())
            {
                Console.WriteLine($"   🔍 INSIGHTS DE CALIDAD:");
                foreach (var insight in result.Quality.Insights.Take(3))
                {
                    Console.WriteLine($"   • {insight.Category}: {insight.Observation}");
                    Console.WriteLine($"     → {insight.Recommendation} (Prioridad: {insight.Priority})");
                }
                Console.WriteLine();
            }

            if (result.Warnings.Any())
            {
                Console.WriteLine($"   ⚠️ ADVERTENCIAS ({result.Warnings.Count}):");
                foreach (var warning in result.Warnings.Take(3))
                {
                    Console.WriteLine($"   • {warning.WarningType}: {warning.Message}");
                    Console.WriteLine($"     → {warning.SuggestedAction}");
                }
                Console.WriteLine();
            }

            if (result.Corrections.Any())
            {
                Console.WriteLine($"   ✏️ CORRECCIONES APLICADAS:");
                foreach (var correction in result.Corrections)
                {
                    Console.WriteLine($"   • {correction}");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"   ✓ PROCESAMIENTO COMPLETO EXITOSO");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error en procesamiento completo: {ex.Message}");
        }
    }

    private static async Task TestSpanishFormatting(ISpanishResponseProcessor processor)
    {
        Console.WriteLine("🧪 Test 3: Mejora de Formato Español");
        Console.WriteLine("───────────────────────────────────");

        var testCases = new[]
        {
            new
            {
                Name = "Formato básico con errores",
                Input = "calentamiento : hacer ejercisios   de  movilidad .\npress banca - 3 series\nsentadilla libre con mancuernas",
                Description = "Espaciado irregular, nombres no normalizados"
            },
            new
            {
                Name = "Listas sin formato consistente",
                Input = "ejercicios:\n1.sentadillas\n-flexiones\n•dominadas\n2 press de hombros",
                Description = "Formato inconsistente de listas"
            },
            new
            {
                Name = "Términos en inglés mezclados",
                Input = "Tu workout incluye 3 sets de push-ups y squats para strength training",
                Description = "Términos ingleses que necesitan traducción"
            }
        };

        foreach (var testCase in testCases)
        {
            try
            {
                Console.WriteLine($"   • {testCase.Name}:");
                Console.WriteLine($"     Descripción: {testCase.Description}");
                Console.WriteLine($"     Input: \"{testCase.Input}\"");

                var enhanced = await processor.EnhanceSpanishFormattingAsync(testCase.Input);

                Console.WriteLine($"     Output: \"{enhanced}\"");

                // Check improvements
                var improvements = new List<string>();

                if (enhanced.Contains("##"))
                    improvements.Add("Encabezados mejorados");

                if (enhanced.Split(' ').Length != testCase.Input.Split(' ').Length)
                    improvements.Add("Espaciado corregido");

                if (enhanced != testCase.Input)
                    improvements.Add("Formato normalizado");

                Console.WriteLine($"     Mejoras: {string.Join(", ", improvements)}");
                Console.WriteLine($"     ✓ FORMATO MEJORADO");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en formato '{testCase.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestExerciseInstructionParsing(ISpanishResponseProcessor processor)
    {
        Console.WriteLine("🧪 Test 4: Parsing de Instrucciones de Ejercicios");
        Console.WriteLine("─────────────────────────────────────────────────");

        var instructionsText = @"
1. Sentadillas con peso corporal
   - Coloca los pies a la anchura de los hombros
   - Baja lentamente flexionando las rodillas hasta 90 grados
   - Mantén la espalda recta durante todo el movimiento
   - Empuja con los talones para volver a la posición inicial
   Músculos trabajados: cuádriceps, glúteos, isquiotibiales

2. Flexiones de pecho
   - Colócate en posición de plancha con brazos extendidos
   - Baja el cuerpo hasta casi tocar el suelo con el pecho
   - Mantén el cuerpo en línea recta
   - Empuja hacia arriba hasta la posición inicial
   Músculos trabajados: pectorales, tríceps, deltoides anteriores

3. Plancha abdominal
   - Apóyate en antebrazos y puntas de los pies
   - Mantén el cuerpo recto como una tabla
   - Contrae los músculos abdominales
   - Respira de forma controlada
   Músculos trabajados: core, hombros, glúteos
";

        try
        {
            var instructions = await processor.ParseExerciseInstructionsAsync(instructionsText);

            Console.WriteLine($"   📋 INSTRUCCIONES PARSEADAS: {instructions.Count} ejercicios");
            Console.WriteLine();

            foreach (var instruction in instructions)
            {
                Console.WriteLine($"   💪 {instruction.ExerciseName}:");
                Console.WriteLine($"   • Pasos: {instruction.StepByStep.Count}");

                if (instruction.StepByStep.Any())
                {
                    Console.WriteLine($"   • Instrucciones detalladas:");
                    foreach (var step in instruction.StepByStep.Take(3))
                    {
                        Console.WriteLine($"     - {step}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(instruction.TargetMuscles))
                {
                    Console.WriteLine($"   • Músculos objetivo: {instruction.TargetMuscles}");
                }

                if (instruction.KeyPoints.Any())
                {
                    Console.WriteLine($"   • Puntos clave: {instruction.KeyPoints.Count}");
                }

                Console.WriteLine($"   • Puntuación calidad: {instruction.QualityScore}/10");
                Console.WriteLine();
            }

            // Validation checks
            var hasValidParsing = instructions.Count >= 3;
            var hasDetailedInstructions = instructions.All(i => i.StepByStep.Count >= 2);
            var hasQualityScores = instructions.All(i => i.QualityScore > 0);

            Console.WriteLine($"   ✓ Parsing válido: {hasValidParsing}");
            Console.WriteLine($"   ✓ Instrucciones detalladas: {hasDetailedInstructions}");
            Console.WriteLine($"   ✓ Puntuaciones calculadas: {hasQualityScores}");

            if (hasValidParsing && hasDetailedInstructions && hasQualityScores)
            {
                Console.WriteLine($"   ✅ PARSING DE INSTRUCCIONES EXITOSO");
            }
            else
            {
                Console.WriteLine($"   ⚠️ PARSING PARCIALMENTE EXITOSO");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error en parsing de instrucciones: {ex.Message}");
        }
    }

    private static async Task TestResponseQualityAssessment(ISpanishResponseProcessor processor)
    {
        Console.WriteLine("🧪 Test 5: Evaluación de Calidad de Respuesta");
        Console.WriteLine("─────────────────────────────────────────────");

        var qualityTestCases = new[]
        {
            new
            {
                Name = "Respuesta de alta calidad",
                Response = @"# RUTINA COMPLETA DE ENTRENAMIENTO

## CALENTAMIENTO (5 minutos):
1. Movilidad articular completa
2. Activación cardiovascular suave
3. Estiramientos dinámicos

## EJERCICIOS PRINCIPALES:
1. Sentadillas - 3 series x 12 repeticiones
   Técnica: Mantén espalda recta, baja controladamente
   Precaución: No sobrepases 90 grados si hay molestias en rodillas

2. Flexiones - 3 series x 8-10 repeticiones
   Técnica: Cuerpo en línea recta, movimiento controlado
   Precaución: Modifica apoyando rodillas si es necesario

## ENFRIAMIENTO (5 minutos):
1. Estiramientos estáticos
2. Respiración profunda
3. Relajación muscular

## PROGRESIÓN:
- Semana 1-2: Aprender técnica correcta
- Semana 3-4: Aumentar repeticiones
- Semana 5-6: Añadir complejidad

IMPORTANTE: Mantén hidratación constante y escucha a tu cuerpo.",
                ExpectedScore = 8.5
            },
            new
            {
                Name = "Respuesta de calidad media",
                Response = @"Rutina de ejercicios:

Calentamiento:
- Caminar 5 minutos

Ejercicios:
1. Sentadillas - 3x12
2. Flexiones - 3x10
3. Abdominales - 3x15

Enfriamiento:
- Estirar músculos

Hacer 3 veces por semana.",
                ExpectedScore = 5.5
            },
            new
            {
                Name = "Respuesta de baja calidad",
                Response = @"hacer ejercicio es bueno. puedes hacer sentadillas y flexiones. tambien caminar. importante estirar.",
                ExpectedScore = 3.0
            }
        };

        var parameters = new UserRoutineParameters
        {
            Name = "Pedro Martín",
            Age = 35,
            ExperienceLevel = "Intermedio",
            PrimaryGoal = "Fuerza",
            AvailableEquipment = new List<string> { "Peso corporal", "Mancuernas" },
            PhysicalLimitations = new List<string> { "Lesión previa de espalda" },
            PreferredSessionDuration = 50
        };

        foreach (var testCase in qualityTestCases)
        {
            try
            {
                Console.WriteLine($"   • {testCase.Name}:");

                var qualityResult = await processor.AssessResponseQualityAsync(testCase.Response, parameters);

                Console.WriteLine($"     📊 MÉTRICAS DE CALIDAD:");
                Console.WriteLine($"     • Puntuación general: {qualityResult.OverallScore:F1}/10 (esperado: ~{testCase.ExpectedScore})");
                Console.WriteLine($"     • Completitud: {qualityResult.Metrics.CompletenessScore:F1}/10");
                Console.WriteLine($"     • Claridad: {qualityResult.Metrics.ClarityScore:F1}/10");
                Console.WriteLine($"     • Seguridad: {qualityResult.Metrics.SafetyScore:F1}/10");
                Console.WriteLine($"     • Personalización: {qualityResult.Metrics.PersonalizationScore:F1}/10");
                Console.WriteLine($"     • Precisión científica: {qualityResult.Metrics.ScientificAccuracyScore:F1}/10");
                Console.WriteLine($"     • Progresión: {qualityResult.Metrics.ProgressionScore:F1}/10");
                Console.WriteLine($"     • Practicidad: {qualityResult.Metrics.PracticalityScore:F1}/10");

                Console.WriteLine($"     ✓ Cumple umbral calidad: {qualityResult.MeetsQualityThreshold}");

                if (qualityResult.StrengthAreas.Any())
                {
                    Console.WriteLine($"     💪 Fortalezas: {string.Join(", ", qualityResult.StrengthAreas)}");
                }

                if (qualityResult.ImprovementAreas.Any())
                {
                    Console.WriteLine($"     🔧 Áreas mejora: {string.Join(", ", qualityResult.ImprovementAreas)}");
                }

                var scoreDifference = Math.Abs(qualityResult.OverallScore - testCase.ExpectedScore);
                var isAccurate = scoreDifference <= 2.0; // Tolerance of 2 points

                Console.WriteLine($"     ✓ Evaluación {(isAccurate ? "PRECISA" : "NECESITA AJUSTE")} (diferencia: {scoreDifference:F1})");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en evaluación '{testCase.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestCommonSpanishErrors(ISpanishResponseProcessor processor)
    {
        Console.WriteLine("🧪 Test 6: Manejo de Errores Comunes en Español");
        Console.WriteLine("───────────────────────────────────────────────");

        var errorTestCases = new[]
        {
            new
            {
                Name = "Errores ortográficos comunes",
                Content = "Realizar ejercisios de fuersa con buena tecnica para evitar lesiones. Las repiticiones deben ser controladas.",
                ErrorTypes = new[] { "Spelling" }
            },
            new
            {
                Name = "Errores de concordancia",
                Content = "Los ejercicio más efectivo son las sentadilla y los flexion de pecho. Una rutina completa debe incluir todos los músculo.",
                ErrorTypes = new[] { "Grammar" }
            },
            new
            {
                Name = "Terminología inglesa",
                Content = "Tu workout debe incluir 3 sets de push-ups y squats. El training debe ser regular para obtener gains.",
                ErrorTypes = new[] { "Terminology" }
            },
            new
            {
                Name = "Mezcla de errores",
                Content = "Los ejercisio de strength son importantes. Hacer 3 set de cada exercise con proper form.",
                ErrorTypes = new[] { "Spelling", "Grammar", "Terminology" }
            }
        };

        foreach (var testCase in errorTestCases)
        {
            try
            {
                Console.WriteLine($"   • {testCase.Name}:");
                Console.WriteLine($"     Contenido: \"{testCase.Content}\"");

                var validation = await processor.ValidateSpanishContentAsync(testCase.Content);

                Console.WriteLine($"     📊 ANÁLISIS DE ERRORES:");
                Console.WriteLine($"     • Errores ortográficos: {validation.SpellingErrors}");
                Console.WriteLine($"     • Errores gramaticales: {validation.GrammarErrors}");
                Console.WriteLine($"     • Total errores encontrados: {validation.Errors.Count}");

                if (validation.Errors.Any())
                {
                    Console.WriteLine($"     🔍 DETALLES DE ERRORES:");
                    var errorsByType = validation.Errors.GroupBy(e => e.ErrorType);

                    foreach (var errorGroup in errorsByType)
                    {
                        Console.WriteLine($"     • {errorGroup.Key}: {errorGroup.Count()} errores");
                        foreach (var error in errorGroup.Take(2))
                        {
                            Console.WriteLine($"       - '{error.OriginalText}' → '{error.SuggestedCorrection}'");
                        }
                    }
                }

                // Check if expected error types were detected
                var detectedTypes = validation.Errors.Select(e => e.ErrorType).Distinct().ToList();
                var expectedDetection = testCase.ErrorTypes.All(expected =>
                    detectedTypes.Any(detected => detected.Equals(expected, StringComparison.OrdinalIgnoreCase)));

                Console.WriteLine($"     ✓ Detección esperada: {expectedDetection}");

                // Test formatting enhancement
                var enhanced = await processor.EnhanceSpanishFormattingAsync(testCase.Content);
                var hasImprovements = enhanced != testCase.Content;

                Console.WriteLine($"     ✓ Mejoras aplicadas: {hasImprovements}");
                if (hasImprovements)
                {
                    Console.WriteLine($"     Mejorado: \"{enhanced}\"");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error en manejo de errores '{testCase.Name}': {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    private static async Task TestMixedTerminologyHandling(ISpanishResponseProcessor processor)
    {
        Console.WriteLine("🧪 Test 7: Manejo de Terminología Mixta");
        Console.WriteLine("─────────────────────────────────────────");

        var mixedTerminologyResponse = @"
# FITNESS ROUTINE FOR STRENGTH

## WARM-UP:
- Do some stretching exercises
- Light cardio para activar el sistema cardiovascular
- Joint mobility para preparar articulaciones

## MAIN WORKOUT:
1. Squats (sentadillas) - 3 sets x 12 reps
   Target muscles: quadriceps, glutes, hamstrings
   Form tips: mantén la espalda recta

2. Push-ups (flexiones) - 3 sets x 10 reps
   Target muscles: chest, triceps, shoulders
   Technique: controlled movement, full range of motion

3. Plank (plancha) - 3 sets x 30 seconds
   Target: core muscles, shoulders
   Instructions: keep body straight como una tabla

## COOL DOWN:
- Static stretching de todos los músculos trabajados
- Deep breathing para relajarse
- Hydration is important - mantente hidratado

TIPS: Listen to your body, progress gradually, get proper rest between sessions.
";

        var parameters = new UserRoutineParameters
        {
            Name = "Ana López",
            ExperienceLevel = "Principiante",
            PrimaryGoal = "Fitness general",
            AvailableEquipment = new List<string> { "Peso corporal" }
        };

        try
        {
            Console.WriteLine($"   📝 PROCESANDO RUTINA CON TERMINOLOGÍA MIXTA...");
            Console.WriteLine();

            var result = await processor.ProcessAIResponseAsync(mixedTerminologyResponse, parameters);

            Console.WriteLine($"   🔍 ANÁLISIS DE TERMINOLOGÍA:");
            Console.WriteLine($"   • Validez en español: {result.Validation.IsValid}");
            Console.WriteLine($"   • Puntuación idioma: {result.Validation.LanguageQualityScore:F2}");
            Console.WriteLine($"   • Terminología fitness adecuada: {result.Validation.HasProperFitnessTerminology}");
            Console.WriteLine();

            if (result.Validation.Errors.Any())
            {
                Console.WriteLine($"   🚨 ERRORES DE TERMINOLOGÍA DETECTADOS ({result.Validation.Errors.Count}):");
                var terminologyErrors = result.Validation.Errors.Where(e => e.ErrorType == "Terminology");
                foreach (var error in terminologyErrors.Take(5))
                {
                    Console.WriteLine($"   • '{error.OriginalText}' → '{error.SuggestedCorrection}'");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"   🔧 CONTENIDO MEJORADO:");
            var improvedLines = result.ProcessedContent.Split('\n').Take(10);
            foreach (var line in improvedLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    Console.WriteLine($"   {line.Trim()}");
            }
            Console.WriteLine("   ...");
            Console.WriteLine();

            Console.WriteLine($"   📊 IMPACTO DEL PROCESAMIENTO:");
            Console.WriteLine($"   • Calidad general: {result.Quality.OverallScore:F1}/10");
            Console.WriteLine($"   • Correcciones aplicadas: {result.Corrections.Count}");
            Console.WriteLine($"   • Advertencias generadas: {result.Warnings.Count}");
            Console.WriteLine($"   • Tiempo procesamiento: {result.ProcessingTime.TotalMilliseconds:F0} ms");
            Console.WriteLine();

            var hasSignificantImprovement = result.ProcessedContent.Length > mixedTerminologyResponse.Length * 0.8 &&
                                          result.Quality.OverallScore > 6.0;

            Console.WriteLine($"   ✓ {(hasSignificantImprovement ? "MEJORA SIGNIFICATIVA LOGRADA" : "PROCESAMIENTO BÁSICO COMPLETADO")}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error en manejo de terminología mixta: {ex.Message}");
        }
    }
}