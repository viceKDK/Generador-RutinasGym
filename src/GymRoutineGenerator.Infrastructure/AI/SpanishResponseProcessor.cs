using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GymRoutineGenerator.Core.Services;

namespace GymRoutineGenerator.Infrastructure.AI;

public class SpanishResponseProcessor : ISpanishResponseProcessor
{
    private readonly Dictionary<string, string> _fitnessTerminology;
    private readonly HashSet<string> _commonSpanishWords;
    private readonly Dictionary<string, string> _exerciseNameNormalization;
    private readonly List<string> _requiredSections;
    private readonly Dictionary<string, List<string>> _muscleGroupTerms;

    public SpanishResponseProcessor()
    {
        _fitnessTerminology = InitializeFitnessTerminology();
        _commonSpanishWords = InitializeCommonSpanishWords();
        _exerciseNameNormalization = InitializeExerciseNameNormalization();
        _requiredSections = new List<string> { "calentamiento", "ejercicios", "enfriamiento" };
        _muscleGroupTerms = InitializeMuscleGroupTerms();
    }

    public async Task<ProcessedRoutineResponse> ProcessAIResponseAsync(string rawResponse, UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ProcessedRoutineResponse();

        try
        {
            // Step 1: Validate Spanish content
            result.Validation = await ValidateSpanishContentAsync(rawResponse, cancellationToken);

            // Step 2: Enhance formatting
            var enhancedContent = await EnhanceSpanishFormattingAsync(rawResponse, cancellationToken);
            result.ProcessedContent = enhancedContent;

            // Step 3: Parse structure
            result.Structure = await ParseRoutineStructureAsync(enhancedContent, cancellationToken);

            // Step 4: Assess quality
            result.Quality = await AssessResponseQualityAsync(enhancedContent, parameters, cancellationToken);

            // Step 5: Generate corrections and warnings
            result.Corrections = GenerateCorrections(rawResponse, enhancedContent);
            result.Warnings = GenerateProcessingWarnings(result);

            // Step 6: Determine if human review is needed
            result.RequiresHumanReview = DetermineHumanReviewRequirement(result);

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
        }
        catch (Exception ex)
        {
            result.Warnings.Add(new ProcessingWarning
            {
                WarningType = "ProcessingError",
                Message = $"Error durante el procesamiento: {ex.Message}",
                SuggestedAction = "Revisar respuesta manualmente",
                Severity = WarningSeverity.Critical,
                RequiresUserAttention = true
            });
        }

        return result;
    }

    public async Task<SpanishValidationResult> ValidateSpanishContentAsync(string content, CancellationToken cancellationToken = default)
    {
        var result = new SpanishValidationResult();

        await Task.Run(() =>
        {
            result.HasProperFitnessTerminology = ValidateFitnessTerminology(content);
            result.HasCorrectGrammar = ValidateBasicGrammar(content);
            result.HasAppropriateFormality = ValidateFormality(content);
            result.SpellingErrors = CountSpellingErrors(content);
            result.GrammarErrors = CountGrammarErrors(content);

            result.LanguageQualityScore = CalculateLanguageQualityScore(result);
            result.IsValid = result.LanguageQualityScore >= 0.7; // 70% threshold

            result.Errors = FindLanguageErrors(content);
            result.Suggestions = GenerateLanguageSuggestions(result);
            result.ImprovedSentences = ImproveSentences(content);
        }, cancellationToken);

        return result;
    }

    public async Task<string> EnhanceSpanishFormattingAsync(string content, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var enhanced = content;

            // Normalize exercise names
            enhanced = NormalizeExerciseNames(enhanced);

            // Fix common Spanish formatting issues
            enhanced = FixSpanishFormatting(enhanced);

            // Improve section headers
            enhanced = ImproveSeccionHeaders(enhanced);

            // Standardize terminology
            enhanced = StandardizeTerminology(enhanced);

            // Format lists and bullet points
            enhanced = FormatLists(enhanced);

            return enhanced;
        }, cancellationToken);
    }

    public async Task<List<ExerciseInstruction>> ParseExerciseInstructionsAsync(string instructions, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var exercises = new List<ExerciseInstruction>();
            var sections = SplitIntoExerciseSections(instructions);

            foreach (var section in sections)
            {
                var exercise = ParseSingleExercise(section);
                if (exercise != null)
                {
                    exercise.QualityScore = AssessInstructionQuality(exercise);
                    exercises.Add(exercise);
                }
            }

            return exercises;
        }, cancellationToken);
    }

    public async Task<RoutineQualityScore> AssessResponseQualityAsync(string response, UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var quality = new RoutineQualityScore();
            var metrics = new QualityMetrics();

            // Assess different quality dimensions
            metrics.CompletenessScore = AssessCompleteness(response);
            metrics.ClarityScore = AssessClarity(response);
            metrics.SafetyScore = AssessSafety(response);
            metrics.PersonalizationScore = AssessPersonalization(response, parameters);
            metrics.ScientificAccuracyScore = AssessScientificAccuracy(response);
            metrics.ProgressionScore = AssessProgression(response);
            metrics.PracticalityScore = AssessPracticality(response, parameters);

            quality.Metrics = metrics;
            quality.OverallScore = CalculateOverallScore(metrics);
            quality.MeetsQualityThreshold = quality.OverallScore >= 7.0;

            quality.Insights = GenerateQualityInsights(metrics, response);
            quality.StrengthAreas = IdentifyStrengths(metrics);
            quality.ImprovementAreas = IdentifyImprovements(metrics);

            return quality;
        }, cancellationToken);
    }

    private Dictionary<string, string> InitializeFitnessTerminology()
    {
        return new Dictionary<string, string>
        {
            { "repeticion", "repetición" },
            { "ejercicio", "ejercicio" },
            { "calentamiento", "calentamiento" },
            { "enfriamiento", "enfriamiento" },
            { "musculo", "músculo" },
            { "entrenamiento", "entrenamiento" },
            { "rutina", "rutina" },
            { "series", "series" },
            { "descanso", "descanso" },
            { "intensidad", "intensidad" },
            { "resistencia", "resistencia" },
            { "flexibilidad", "flexibilidad" },
            { "cardio", "cardio" },
            { "fuerza", "fuerza" },
            { "hipertrofia", "hipertrofia" },
            { "definicion", "definición" },
            { "volumen", "volumen" },
            { "progresion", "progresión" },
            { "periodizacion", "periodización" },
            { "sobrecarga", "sobrecarga" },
            { "recuperacion", "recuperación" }
        };
    }

    private HashSet<string> InitializeCommonSpanishWords()
    {
        return new HashSet<string>
        {
            "el", "la", "de", "que", "y", "en", "un", "es", "se", "no", "te", "lo", "le", "da", "su", "por", "son", "con", "para", "al", "del", "los", "las", "una", "como", "más", "pero", "sus", "me", "ya", "muy", "sin", "sobre", "ser", "tiene", "todo", "esta", "entre", "cuando", "hasta", "desde", "están", "mis", "otro", "donde", "quien", "cual", "cada", "ellos", "hay", "fue", "puede", "tiempo", "bien", "durante", "hacer", "será", "dos", "tres", "años", "estado", "gran", "contra", "si", "también"
        };
    }

    private Dictionary<string, string> InitializeExerciseNameNormalization()
    {
        return new Dictionary<string, string>
        {
            { "press banca", "press de banca" },
            { "sentadilla libre", "sentadilla" },
            { "peso muerto", "peso muerto" },
            { "dominadas", "dominadas" },
            { "flexiones", "flexiones de pecho" },
            { "remo", "remo con barra" },
            { "curl biceps", "curl de bíceps" },
            { "extension triceps", "extensión de tríceps" },
            { "press hombros", "press de hombros" },
            { "elevaciones laterales", "elevaciones laterales" }
        };
    }

    private Dictionary<string, List<string>> InitializeMuscleGroupTerms()
    {
        return new Dictionary<string, List<string>>
        {
            { "pecho", new List<string> { "pectoral", "pectorales", "chest", "pecho" } },
            { "espalda", new List<string> { "dorsal", "dorsales", "trapecio", "romboides", "back", "espalda" } },
            { "piernas", new List<string> { "cuádriceps", "isquiotibiales", "glúteos", "pantorrillas", "legs", "piernas" } },
            { "hombros", new List<string> { "deltoides", "shoulders", "hombros" } },
            { "brazos", new List<string> { "bíceps", "tríceps", "antebrazos", "arms", "brazos" } },
            { "core", new List<string> { "abdominales", "core", "abs", "abdomen" } }
        };
    }

    private bool ValidateFitnessTerminology(string content)
    {
        var fitnessTermCount = 0;
        var totalWords = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        foreach (var term in _fitnessTerminology.Keys)
        {
            if (content.ToLower().Contains(term))
            {
                fitnessTermCount++;
            }
        }

        return fitnessTermCount >= Math.Min(5, totalWords / 50); // At least 5 fitness terms or 1 per 50 words
    }

    private bool ValidateBasicGrammar(string content)
    {
        // Check for basic grammar patterns
        var sentences = content.Split('.', '!', '?').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var validSentences = 0;

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (trimmed.Length > 3 && HasValidSpanishStructure(trimmed))
            {
                validSentences++;
            }
        }

        return sentences.Count > 0 && (validSentences / (double)sentences.Count) >= 0.8;
    }

    private bool HasValidSpanishStructure(string sentence)
    {
        // Basic Spanish sentence structure validation
        var words = sentence.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Check for common Spanish patterns
        var hasArticle = words.Any(w => new[] { "el", "la", "los", "las", "un", "una", "unos", "unas" }.Contains(w));
        var hasVerb = words.Any(w => w.EndsWith("ar") || w.EndsWith("er") || w.EndsWith("ir") ||
                                     new[] { "es", "son", "está", "están", "tiene", "tienen", "hace", "hacen" }.Contains(w));

        return words.Length >= 3 && (hasArticle || hasVerb);
    }

    private bool ValidateFormality(string content)
    {
        // Check for appropriate level of formality for fitness instruction
        var informalMarkers = new[] { "tío", "colega", "chaval", "guay", "flipar", "mola" };
        var formalMarkers = new[] { "usted", "señor", "señora", "por favor", "le recomiendo", "es aconsejable" };
        var neutralMarkers = new[] { "debes", "puedes", "realiza", "ejecuta", "mantén", "controla" };

        var informalCount = informalMarkers.Sum(marker => CountOccurrences(content.ToLower(), marker));
        var neutralCount = neutralMarkers.Sum(marker => CountOccurrences(content.ToLower(), marker));

        // Fitness instructions should be neutral to slightly formal, not overly informal
        return informalCount <= 2 && neutralCount >= 3;
    }

    private int CountSpellingErrors(string content)
    {
        // Simple spelling check based on common errors
        var commonErrors = new Dictionary<string, string>
        {
            { "ejercisio", "ejercicio" },
            { "rutinha", "rutina" },
            { "muscolos", "músculos" },
            { "repiticiones", "repeticiones" },
            { "desscanso", "descanso" },
            { "entrenamineto", "entrenamiento" },
            { "calentaminto", "calentamiento" }
        };

        return commonErrors.Keys.Sum(error => CountOccurrences(content.ToLower(), error));
    }

    private int CountGrammarErrors(string content)
    {
        var errorPatterns = new[]
        {
            @"los ejercicio(?!s)", // Agreement errors
            @"las músculo(?!s)",
            @"un rutina",
            @"una ejercicio",
            @"hacer flexion(?!es)"
        };

        return errorPatterns.Sum(pattern => Regex.Matches(content, pattern, RegexOptions.IgnoreCase).Count);
    }

    private double CalculateLanguageQualityScore(SpanishValidationResult result)
    {
        var score = 1.0;

        if (!result.HasProperFitnessTerminology) score -= 0.2;
        if (!result.HasCorrectGrammar) score -= 0.3;
        if (!result.HasAppropriateFormality) score -= 0.1;

        score -= (result.SpellingErrors * 0.05);
        score -= (result.GrammarErrors * 0.1);

        return Math.Max(0.0, score);
    }

    private List<LanguageError> FindLanguageErrors(string content)
    {
        var errors = new List<LanguageError>();

        // Find spelling errors
        var spellingErrors = FindSpellingErrors(content);
        errors.AddRange(spellingErrors);

        // Find grammar errors
        var grammarErrors = FindGrammarErrors(content);
        errors.AddRange(grammarErrors);

        // Find terminology issues
        var terminologyErrors = FindTerminologyErrors(content);
        errors.AddRange(terminologyErrors);

        return errors;
    }

    private List<LanguageError> FindSpellingErrors(string content)
    {
        var errors = new List<LanguageError>();
        var commonErrors = new Dictionary<string, string>
        {
            { "ejercisio", "ejercicio" },
            { "muscolos", "músculos" },
            { "repiticiones", "repeticiones" }
        };

        foreach (var error in commonErrors)
        {
            var matches = Regex.Matches(content, @"\b" + error.Key + @"\b", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                errors.Add(new LanguageError
                {
                    ErrorType = "Spelling",
                    OriginalText = match.Value,
                    SuggestedCorrection = error.Value,
                    Explanation = "Error ortográfico común",
                    Position = match.Index,
                    Severity = ErrorSeverity.Minor
                });
            }
        }

        return errors;
    }

    private List<LanguageError> FindGrammarErrors(string content)
    {
        var errors = new List<LanguageError>();

        // Check for agreement errors
        var matches = Regex.Matches(content, @"los ejercicio(?!s)", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            errors.Add(new LanguageError
            {
                ErrorType = "Grammar",
                OriginalText = match.Value,
                SuggestedCorrection = "los ejercicios",
                Explanation = "Error de concordancia de género y número",
                Position = match.Index,
                Severity = ErrorSeverity.Moderate
            });
        }

        return errors;
    }

    private List<LanguageError> FindTerminologyErrors(string content)
    {
        var errors = new List<LanguageError>();

        // Check for English terms that should be in Spanish
        var englishTerms = new Dictionary<string, string>
        {
            { "workout", "entrenamiento" },
            { "training", "entrenamiento" },
            { "fitness", "acondicionamiento físico" },
            { "set", "serie" },
            { "rep", "repetición" }
        };

        foreach (var term in englishTerms)
        {
            var matches = Regex.Matches(content, @"\b" + term.Key + @"\b", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                errors.Add(new LanguageError
                {
                    ErrorType = "Terminology",
                    OriginalText = match.Value,
                    SuggestedCorrection = term.Value,
                    Explanation = "Usar terminología en español",
                    Position = match.Index,
                    Severity = ErrorSeverity.Minor
                });
            }
        }

        return errors;
    }

    private List<string> GenerateLanguageSuggestions(SpanishValidationResult result)
    {
        var suggestions = new List<string>();

        if (!result.HasProperFitnessTerminology)
            suggestions.Add("Incluir más terminología específica de fitness en español");

        if (result.SpellingErrors > 0)
            suggestions.Add("Revisar ortografía, especialmente en términos técnicos");

        if (result.GrammarErrors > 0)
            suggestions.Add("Verificar concordancia de género y número");

        if (!result.HasAppropriateFormality)
            suggestions.Add("Mantener un tono profesional pero accesible");

        return suggestions;
    }

    private List<string> ImproveSentences(string content)
    {
        // Implement sentence improvement logic
        return new List<string>();
    }

    private string NormalizeExerciseNames(string content)
    {
        var normalized = content;

        foreach (var normalization in _exerciseNameNormalization)
        {
            normalized = Regex.Replace(normalized, @"\b" + normalization.Key + @"\b",
                                     normalization.Value, RegexOptions.IgnoreCase);
        }

        return normalized;
    }

    private string FixSpanishFormatting(string content)
    {
        var formatted = content;

        // Fix common Spanish formatting issues
        formatted = Regex.Replace(formatted, @"\s+", " "); // Multiple spaces
        formatted = Regex.Replace(formatted, @"\n\s*\n\s*\n", "\n\n"); // Multiple line breaks
        formatted = formatted.Replace(" ,", ",").Replace(" .", ".");
        formatted = formatted.Replace("( ", "(").Replace(" )", ")");

        return formatted.Trim();
    }

    private string ImproveSeccionHeaders(string content)
    {
        var improved = content;

        // Standardize section headers
        var headers = new Dictionary<string, string>
        {
            { @"calentamiento:?", "## CALENTAMIENTO:" },
            { @"ejercicios principales:?", "## EJERCICIOS PRINCIPALES:" },
            { @"enfriamiento:?", "## ENFRIAMIENTO:" },
            { @"consejos:?", "## CONSEJOS ADICIONALES:" }
        };

        foreach (var header in headers)
        {
            improved = Regex.Replace(improved, header.Key, header.Value, RegexOptions.IgnoreCase);
        }

        return improved;
    }

    private string StandardizeTerminology(string content)
    {
        var standardized = content;

        foreach (var term in _fitnessTerminology)
        {
            standardized = Regex.Replace(standardized, @"\b" + term.Key + @"\b",
                                       term.Value, RegexOptions.IgnoreCase);
        }

        return standardized;
    }

    private string FormatLists(string content)
    {
        var lines = content.Split('\n');
        var formatted = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Format exercise lists
            if (Regex.IsMatch(trimmed, @"^\d+\.?\s*[A-Z]"))
            {
                formatted.AppendLine($"**{trimmed}**");
            }
            else if (trimmed.StartsWith("-") || trimmed.StartsWith("•"))
            {
                formatted.AppendLine($"   {trimmed}");
            }
            else
            {
                formatted.AppendLine(line);
            }
        }

        return formatted.ToString();
    }

    private async Task<RoutineStructure> ParseRoutineStructureAsync(string content, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var structure = new RoutineStructure();

            structure.Title = ExtractTitle(content);
            structure.Objective = ExtractObjective(content);
            structure.Warmup = ParseWarmupSection(content);
            structure.ExerciseBlocks = ParseExerciseBlocks(content);
            structure.Cooldown = ParseCooldownSection(content);
            structure.SafetyNotes = ParseSafetyNotes(content);
            structure.EstimatedDuration = CalculateEstimatedDuration(structure);

            return structure;
        }, cancellationToken);
    }

    private string ExtractTitle(string content)
    {
        var lines = content.Split('\n');
        var titleLine = lines.FirstOrDefault(line =>
            line.ToLower().Contains("rutina") &&
            (line.Contains("#") || line.Length < 100));

        return titleLine?.Trim('#', ' ') ?? "Rutina de Entrenamiento";
    }

    private string ExtractObjective(string content)
    {
        var objectivePatterns = new[]
        {
            @"objetivo:?\s*(.+?)(?:\n|$)",
            @"meta:?\s*(.+?)(?:\n|$)",
            @"propósito:?\s*(.+?)(?:\n|$)"
        };

        foreach (var pattern in objectivePatterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return "Mejorar la condición física general";
    }

    private WarmupSection ParseWarmupSection(string content)
    {
        var warmupSection = new WarmupSection();

        // Extract warmup content
        var warmupMatch = Regex.Match(content, @"calentamiento:?\s*(.*?)(?=ejercicios|enfriamiento|$)",
                                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (warmupMatch.Success)
        {
            var warmupContent = warmupMatch.Groups[1].Value;
            warmupSection.Exercises = ParseWarmupExercises(warmupContent);
            warmupSection.Duration = TimeSpan.FromMinutes(5); // Default 5 minutes
            warmupSection.Purpose = "Preparar el cuerpo para el entrenamiento";
        }

        return warmupSection;
    }

    private List<WarmupExercise> ParseWarmupExercises(string content)
    {
        var exercises = new List<WarmupExercise>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("-") || trimmed.StartsWith("•") || Regex.IsMatch(trimmed, @"^\d+\."))
            {
                var exerciseName = Regex.Replace(trimmed, @"^[-•\d.]\s*", "");
                if (!string.IsNullOrWhiteSpace(exerciseName))
                {
                    exercises.Add(new WarmupExercise
                    {
                        Name = exerciseName,
                        Duration = "30-60 segundos",
                        Intensity = "Baja",
                        Instructions = "Realizar de forma controlada"
                    });
                }
            }
        }

        return exercises;
    }

    private List<ExerciseBlock> ParseExerciseBlocks(string content)
    {
        var blocks = new List<ExerciseBlock>();

        // Find main exercise section
        var exerciseMatch = Regex.Match(content, @"ejercicios:?\s*(.*?)(?=enfriamiento|consejos|$)",
                                      RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (exerciseMatch.Success)
        {
            var mainBlock = new ExerciseBlock
            {
                BlockName = "Ejercicios Principales",
                BlockType = "Principal",
                Exercises = ParseDetailedExercises(exerciseMatch.Groups[1].Value),
                OrderInRoutine = 1
            };

            blocks.Add(mainBlock);
        }

        return blocks;
    }

    private List<DetailedExercise> ParseDetailedExercises(string content)
    {
        var exercises = new List<DetailedExercise>();
        var sections = SplitIntoExerciseSections(content);

        int order = 1;
        foreach (var section in sections)
        {
            var exercise = ParseDetailedExercise(section);
            if (exercise != null)
            {
                exercises.Add(exercise);
                order++;
            }
        }

        return exercises;
    }

    private List<string> SplitIntoExerciseSections(string content)
    {
        var sections = new List<string>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var currentSection = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // New exercise starts with number, dash, or bullet
            if (Regex.IsMatch(trimmed, @"^\d+\.") ||
                (trimmed.StartsWith("-") && currentSection.Length > 0) ||
                (trimmed.StartsWith("•") && currentSection.Length > 0))
            {
                if (currentSection.Length > 0)
                {
                    sections.Add(currentSection.ToString());
                    currentSection.Clear();
                }
            }

            currentSection.AppendLine(line);
        }

        if (currentSection.Length > 0)
        {
            sections.Add(currentSection.ToString());
        }

        return sections;
    }

    private DetailedExercise ParseDetailedExercise(string section)
    {
        var lines = section.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return null;

        var exercise = new DetailedExercise();

        // Extract exercise name from first line
        var firstLine = lines[0].Trim();
        exercise.Name = Regex.Replace(firstLine, @"^[-•\d.]\s*", "").Trim();

        // Parse parameters and instructions from remaining lines
        foreach (var line in lines.Skip(1))
        {
            var trimmed = line.Trim().ToLower();

            if (trimmed.Contains("series") || trimmed.Contains("repeticiones"))
            {
                exercise.Parameters = ParseExerciseParameters(line);
            }
            else if (trimmed.Contains("músculos") || trimmed.Contains("trabaja"))
            {
                exercise.MuscleGroups = ExtractMuscleGroups(line);
            }
            else if (trimmed.Contains("instrucciones") || trimmed.Contains("técnica"))
            {
                exercise.StepByStepInstructions.Add(line.Trim());
            }
            else if (trimmed.Contains("precaución") || trimmed.Contains("cuidado"))
            {
                exercise.SafetyTips.Add(line.Trim());
            }
        }

        return exercise;
    }

    private ExerciseInstruction ParseSingleExercise(string section)
    {
        var exercise = new ExerciseInstruction();
        var lines = section.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length > 0)
        {
            exercise.ExerciseName = Regex.Replace(lines[0].Trim(), @"^[-•\d.]\s*", "");

            foreach (var line in lines.Skip(1))
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    exercise.StepByStep.Add(trimmed);
                }
            }
        }

        return exercise;
    }

    private ExerciseParameters ParseExerciseParameters(string paramLine)
    {
        var parameters = new ExerciseParameters();

        var setsMatch = Regex.Match(paramLine, @"(\d+)\s*series?", RegexOptions.IgnoreCase);
        if (setsMatch.Success)
        {
            parameters.Sets = setsMatch.Groups[1].Value;
        }

        var repsMatch = Regex.Match(paramLine, @"(\d+(?:-\d+)?)\s*repeticiones?", RegexOptions.IgnoreCase);
        if (repsMatch.Success)
        {
            parameters.Repetitions = repsMatch.Groups[1].Value;
        }

        return parameters;
    }

    private List<string> ExtractMuscleGroups(string line)
    {
        var muscles = new List<string>();
        var lineLower = line.ToLower();

        foreach (var muscleGroup in _muscleGroupTerms)
        {
            if (muscleGroup.Value.Any(term => lineLower.Contains(term)))
            {
                muscles.Add(muscleGroup.Key);
            }
        }

        return muscles.Distinct().ToList();
    }

    private CooldownSection ParseCooldownSection(string content)
    {
        var cooldownSection = new CooldownSection();

        var cooldownMatch = Regex.Match(content, @"enfriamiento:?\s*(.*?)(?=consejos|$)",
                                      RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (cooldownMatch.Success)
        {
            var cooldownContent = cooldownMatch.Groups[1].Value;
            cooldownSection.Exercises = ParseCooldownExercises(cooldownContent);
            cooldownSection.Duration = TimeSpan.FromMinutes(10);
            cooldownSection.Purpose = "Relajar músculos y reducir tensión";
        }

        return cooldownSection;
    }

    private List<CooldownExercise> ParseCooldownExercises(string content)
    {
        var exercises = new List<CooldownExercise>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("-") || trimmed.StartsWith("•") || Regex.IsMatch(trimmed, @"^\d+\."))
            {
                var exerciseName = Regex.Replace(trimmed, @"^[-•\d.]\s*", "");
                if (!string.IsNullOrWhiteSpace(exerciseName))
                {
                    exercises.Add(new CooldownExercise
                    {
                        Name = exerciseName,
                        Duration = "30 segundos",
                        Instructions = "Mantener estiramiento suave",
                        IsStretching = true
                    });
                }
            }
        }

        return exercises;
    }

    private List<SafetyNote> ParseSafetyNotes(string content)
    {
        var notes = new List<SafetyNote>();
        var safetyPatterns = new[]
        {
            @"precaución:?\s*(.+?)(?:\n|$)",
            @"cuidado:?\s*(.+?)(?:\n|$)",
            @"importante:?\s*(.+?)(?:\n|$)",
            @"advertencia:?\s*(.+?)(?:\n|$)"
        };

        foreach (var pattern in safetyPatterns)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                notes.Add(new SafetyNote
                {
                    Note = match.Groups[1].Value.Trim(),
                    Level = SafetyLevel.Caution,
                    Action = "Seguir las indicaciones cuidadosamente"
                });
            }
        }

        return notes;
    }

    private TimeSpan CalculateEstimatedDuration(RoutineStructure structure)
    {
        var totalMinutes = 0;

        totalMinutes += (int)structure.Warmup.Duration.TotalMinutes;
        totalMinutes += structure.ExerciseBlocks.Sum(block => (int)block.EstimatedTime.TotalMinutes);
        totalMinutes += (int)structure.Cooldown.Duration.TotalMinutes;

        // If no specific times were parsed, use defaults
        if (totalMinutes == 0)
        {
            totalMinutes = 45; // Default 45 minutes
        }

        return TimeSpan.FromMinutes(totalMinutes);
    }

    private int AssessInstructionQuality(ExerciseInstruction instruction)
    {
        var score = 5; // Base score

        if (instruction.StepByStep.Count >= 3) score += 2;
        if (instruction.StepByStep.Any(step => step.Length > 20)) score += 1;
        if (!string.IsNullOrWhiteSpace(instruction.TargetMuscles)) score += 1;
        if (instruction.KeyPoints.Any()) score += 1;

        return Math.Min(10, score);
    }

    private double AssessCompleteness(string response)
    {
        var requiredSections = new[] { "calentamiento", "ejercicios", "enfriamiento" };
        var foundSections = requiredSections.Count(section =>
            response.ToLower().Contains(section));

        return (foundSections / (double)requiredSections.Length) * 10.0;
    }

    private double AssessClarity(string response)
    {
        var sentences = response.Split('.', '!', '?').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var avgSentenceLength = sentences.Count > 0 ? sentences.Average(s => s.Length) : 0;

        // Penalize very long or very short sentences
        var clarityScore = 10.0;
        if (avgSentenceLength > 150) clarityScore -= 2.0;
        if (avgSentenceLength < 20) clarityScore -= 1.0;

        // Check for clear exercise formatting
        var exerciseCount = Regex.Matches(response, @"^\d+\.", RegexOptions.Multiline).Count;
        if (exerciseCount >= 3) clarityScore += 1.0;

        return Math.Max(0, clarityScore);
    }

    private double AssessSafety(string response)
    {
        var safetyKeywords = new[] { "precaución", "cuidado", "postura", "técnica", "respiración", "calentamiento", "enfriamiento" };
        var safetyScore = safetyKeywords.Count(keyword =>
            response.ToLower().Contains(keyword)) * 1.5;

        return Math.Min(10.0, safetyScore);
    }

    private double AssessPersonalization(string response, UserRoutineParameters parameters)
    {
        var personalizationScore = 5.0; // Base score

        // Check if routine mentions user's equipment
        if (parameters.AvailableEquipment.Any(eq => response.ToLower().Contains(eq.ToLower())))
            personalizationScore += 2.0;

        // Check if routine avoids mentioned limitations
        if (parameters.PhysicalLimitations.All(limit => !response.ToLower().Contains(limit.ToLower())))
            personalizationScore += 2.0;

        // Check if routine matches experience level
        var levelKeywords = GetExperienceLevelKeywords(parameters.ExperienceLevel);
        if (levelKeywords.Any(keyword => response.ToLower().Contains(keyword)))
            personalizationScore += 1.0;

        return Math.Min(10.0, personalizationScore);
    }

    private double AssessScientificAccuracy(string response)
    {
        var scientificTerms = new[] { "series", "repeticiones", "descanso", "intensidad", "progresión", "volumen", "frecuencia" };
        var termCount = scientificTerms.Count(term => response.ToLower().Contains(term));

        return Math.Min(10.0, termCount * 1.2);
    }

    private double AssessProgression(string response)
    {
        var progressionKeywords = new[] { "progresión", "aumentar", "incrementar", "avanzar", "semana", "nivel" };
        var progressionScore = progressionKeywords.Count(keyword =>
            response.ToLower().Contains(keyword)) * 2.0;

        return Math.Min(10.0, progressionScore);
    }

    private double AssessPracticality(string response, UserRoutineParameters parameters)
    {
        var practicalityScore = 7.0; // Start high, deduct for issues

        // Check for unrealistic time requirements
        var timeMatches = Regex.Matches(response, @"(\d+)\s*horas?", RegexOptions.IgnoreCase);
        if (timeMatches.Cast<Match>().Any(m => int.Parse(m.Groups[1].Value) > 2))
            practicalityScore -= 2.0;

        // Check for excessive exercise count
        var exerciseCount = Regex.Matches(response, @"^\d+\.", RegexOptions.Multiline).Count;
        if (exerciseCount > 12) practicalityScore -= 1.0;

        return Math.Max(0, practicalityScore);
    }

    private double CalculateOverallScore(QualityMetrics metrics)
    {
        var weights = new Dictionary<string, double>
        {
            { nameof(metrics.CompletenessScore), 0.2 },
            { nameof(metrics.ClarityScore), 0.15 },
            { nameof(metrics.SafetyScore), 0.2 },
            { nameof(metrics.PersonalizationScore), 0.15 },
            { nameof(metrics.ScientificAccuracyScore), 0.15 },
            { nameof(metrics.ProgressionScore), 0.1 },
            { nameof(metrics.PracticalityScore), 0.05 }
        };

        var weightedSum =
            metrics.CompletenessScore * weights[nameof(metrics.CompletenessScore)] +
            metrics.ClarityScore * weights[nameof(metrics.ClarityScore)] +
            metrics.SafetyScore * weights[nameof(metrics.SafetyScore)] +
            metrics.PersonalizationScore * weights[nameof(metrics.PersonalizationScore)] +
            metrics.ScientificAccuracyScore * weights[nameof(metrics.ScientificAccuracyScore)] +
            metrics.ProgressionScore * weights[nameof(metrics.ProgressionScore)] +
            metrics.PracticalityScore * weights[nameof(metrics.PracticalityScore)];

        return Math.Round(weightedSum, 1);
    }

    private List<QualityInsight> GenerateQualityInsights(QualityMetrics metrics, string response)
    {
        var insights = new List<QualityInsight>();

        if (metrics.CompletenessScore < 8.0)
        {
            insights.Add(new QualityInsight
            {
                Category = "Completeness",
                Observation = "La rutina no incluye todas las secciones requeridas",
                Recommendation = "Asegurar que incluya calentamiento, ejercicios y enfriamiento",
                Priority = InsightPriority.High
            });
        }

        if (metrics.SafetyScore < 6.0)
        {
            insights.Add(new QualityInsight
            {
                Category = "Safety",
                Observation = "Faltan consideraciones de seguridad",
                Recommendation = "Incluir más notas sobre técnica correcta y precauciones",
                Priority = InsightPriority.Critical
            });
        }

        if (metrics.PersonalizationScore < 6.0)
        {
            insights.Add(new QualityInsight
            {
                Category = "Personalization",
                Observation = "La rutina no parece personalizada para el usuario",
                Recommendation = "Incorporar más elementos específicos del perfil del usuario",
                Priority = InsightPriority.Medium
            });
        }

        return insights;
    }

    private List<string> IdentifyStrengths(QualityMetrics metrics)
    {
        var strengths = new List<string>();

        if (metrics.CompletenessScore >= 8.0) strengths.Add("Estructura completa");
        if (metrics.ClarityScore >= 8.0) strengths.Add("Instrucciones claras");
        if (metrics.SafetyScore >= 8.0) strengths.Add("Enfoque en seguridad");
        if (metrics.ScientificAccuracyScore >= 8.0) strengths.Add("Base científica sólida");

        return strengths;
    }

    private List<string> IdentifyImprovements(QualityMetrics metrics)
    {
        var improvements = new List<string>();

        if (metrics.CompletenessScore < 7.0) improvements.Add("Mejorar completitud de secciones");
        if (metrics.ClarityScore < 7.0) improvements.Add("Clarificar instrucciones");
        if (metrics.SafetyScore < 7.0) improvements.Add("Incluir más consideraciones de seguridad");
        if (metrics.PersonalizationScore < 7.0) improvements.Add("Aumentar personalización");

        return improvements;
    }

    private List<string> GetExperienceLevelKeywords(string experienceLevel)
    {
        return experienceLevel.ToLower() switch
        {
            "principiante" => new List<string> { "básico", "fundamental", "iniciación", "simple" },
            "intermedio" => new List<string> { "moderado", "intermedio", "progresivo" },
            "avanzado" => new List<string> { "avanzado", "intenso", "complejo", "alta intensidad" },
            _ => new List<string>()
        };
    }

    private List<string> GenerateCorrections(string original, string enhanced)
    {
        var corrections = new List<string>();

        if (original != enhanced)
        {
            corrections.Add("Se mejoró el formato y estructura del contenido");

            if (enhanced.Length > original.Length)
                corrections.Add("Se añadió contenido para mayor claridad");

            if (CountOccurrences(enhanced, "##") > CountOccurrences(original, "##"))
                corrections.Add("Se mejoraron los encabezados de sección");
        }

        return corrections;
    }

    private List<ProcessingWarning> GenerateProcessingWarnings(ProcessedRoutineResponse result)
    {
        var warnings = new List<ProcessingWarning>();

        if (!result.Validation.IsValid)
        {
            warnings.Add(new ProcessingWarning
            {
                WarningType = "ValidationFailed",
                Message = "La respuesta no pasó la validación de calidad en español",
                SuggestedAction = "Revisar errores de idioma y formato",
                Severity = WarningSeverity.Warning,
                RequiresUserAttention = true
            });
        }

        if (result.Quality.OverallScore < 7.0)
        {
            warnings.Add(new ProcessingWarning
            {
                WarningType = "LowQuality",
                Message = $"Puntuación de calidad baja: {result.Quality.OverallScore:F1}/10",
                SuggestedAction = "Considerar regenerar la rutina",
                Severity = WarningSeverity.Warning,
                RequiresUserAttention = false
            });
        }

        return warnings;
    }

    private bool DetermineHumanReviewRequirement(ProcessedRoutineResponse result)
    {
        return result.Quality.OverallScore < 6.0 ||
               result.Warnings.Any(w => w.Severity >= WarningSeverity.Error) ||
               !result.Validation.IsValid;
    }

    private int CountOccurrences(string text, string pattern)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
            return 0;

        return (text.Length - text.Replace(pattern, "").Length) / pattern.Length;
    }
}