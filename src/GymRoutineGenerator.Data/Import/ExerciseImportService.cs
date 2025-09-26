using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Globalization;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Core.Enums;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using CsvHelper.Configuration;

namespace GymRoutineGenerator.Data.Import;

public class ExerciseImportService : IExerciseImportService
{
    private readonly GymRoutineContext _context;

    public ExerciseImportService(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<ImportResult> ImportFromJsonAsync(string jsonFilePath)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ImportResult();

        try
        {
            if (!File.Exists(jsonFilePath))
            {
                result.Errors.Add($"File not found: {jsonFilePath}");
                return result;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var exerciseData = JsonSerializer.Deserialize<List<ExerciseImportData>>(jsonContent);

            if (exerciseData == null)
            {
                result.Errors.Add("Failed to parse JSON file");
                return result;
            }

            result = await ImportFromDataAsync(exerciseData);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading JSON file: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<ImportResult> ImportFromCsvAsync(string csvFilePath)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ImportResult();

        try
        {
            if (!File.Exists(csvFilePath))
            {
                result.Errors.Add($"File not found: {csvFilePath}");
                return result;
            }

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var exerciseData = csv.GetRecords<ExerciseImportData>().ToList();
            result = await ImportFromDataAsync(exerciseData);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading CSV file: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<ImportResult> ImportFromDataAsync(IEnumerable<ExerciseImportData> exerciseData)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ImportResult
        {
            TotalRecords = exerciseData.Count()
        };

        try
        {
            // Validate data first
            var validationResult = await ValidateImportDataAsync(exerciseData);
            if (!validationResult.IsValid)
            {
                result.Errors.AddRange(validationResult.Errors);
                result.Warnings.AddRange(validationResult.Warnings);
                return result;
            }

            // Load lookup data
            var muscleGroups = await _context.MuscleGroups.ToDictionaryAsync(mg => mg.Name, mg => mg);
            var equipmentTypes = await _context.EquipmentTypes.ToDictionaryAsync(et => et.Name, et => et);

            // Get existing exercises to check for duplicates
            var existingExercises = (await _context.Exercises
                .Select(e => e.SpanishName.ToLower())
                .ToListAsync()).ToHashSet();

            var exercisesToAdd = new List<Exercise>();
            var exerciseImages = new List<ExerciseImage>();
            var secondaryMuscles = new List<ExerciseSecondaryMuscle>();

            foreach (var exerciseDto in exerciseData)
            {
                try
                {
                    // Skip if already exists
                    if (existingExercises.Contains(exerciseDto.SpanishName.ToLower()))
                    {
                        result.Warnings.Add($"Exercise already exists: {exerciseDto.SpanishName}");
                        continue;
                    }

                    // Parse enums
                    if (!Enum.TryParse<DifficultyLevel>(exerciseDto.DifficultyLevel, true, out var difficultyLevel))
                    {
                        result.Errors.Add($"Invalid difficulty level for {exerciseDto.SpanishName}: {exerciseDto.DifficultyLevel}");
                        result.FailedImports++;
                        continue;
                    }

                    if (!Enum.TryParse<ExerciseType>(exerciseDto.ExerciseType, true, out var exerciseType))
                    {
                        result.Errors.Add($"Invalid exercise type for {exerciseDto.SpanishName}: {exerciseDto.ExerciseType}");
                        result.FailedImports++;
                        continue;
                    }

                    // Get lookup entities
                    if (!muscleGroups.TryGetValue(exerciseDto.PrimaryMuscleGroup, out var primaryMuscleGroup))
                    {
                        result.Errors.Add($"Invalid primary muscle group for {exerciseDto.SpanishName}: {exerciseDto.PrimaryMuscleGroup}");
                        result.FailedImports++;
                        continue;
                    }

                    if (!equipmentTypes.TryGetValue(exerciseDto.EquipmentType, out var equipmentType))
                    {
                        result.Errors.Add($"Invalid equipment type for {exerciseDto.SpanishName}: {exerciseDto.EquipmentType}");
                        result.FailedImports++;
                        continue;
                    }

                    // Create exercise entity
                    var exercise = new Exercise
                    {
                        Name = exerciseDto.Name,
                        SpanishName = exerciseDto.SpanishName,
                        Description = exerciseDto.Description,
                        Instructions = exerciseDto.Instructions,
                        PrimaryMuscleGroupId = primaryMuscleGroup.Id,
                        EquipmentTypeId = equipmentType.Id,
                        DifficultyLevel = difficultyLevel,
                        ExerciseType = exerciseType,
                        DurationSeconds = exerciseDto.DurationSeconds,
                        IsActive = exerciseDto.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    exercisesToAdd.Add(exercise);

                    // Prepare secondary muscles (to be added after exercise is saved)
                    foreach (var secondaryMuscleGroupName in exerciseDto.SecondaryMuscleGroups)
                    {
                        if (muscleGroups.TryGetValue(secondaryMuscleGroupName, out var secondaryMuscleGroup))
                        {
                            secondaryMuscles.Add(new ExerciseSecondaryMuscle
                            {
                                Exercise = exercise,
                                MuscleGroupId = secondaryMuscleGroup.Id
                            });
                        }
                        else
                        {
                            result.Warnings.Add($"Invalid secondary muscle group for {exerciseDto.SpanishName}: {secondaryMuscleGroupName}");
                        }
                    }

                    // Prepare images (to be added after exercise is saved)
                    for (int i = 0; i < exerciseDto.ImagePaths.Count; i++)
                    {
                        var imagePath = exerciseDto.ImagePaths[i];
                        var position = i switch
                        {
                            0 => "start",
                            1 => "mid",
                            2 => "end",
                            _ => "demonstration"
                        };

                        exerciseImages.Add(new ExerciseImage
                        {
                            Exercise = exercise,
                            ImagePath = imagePath,
                            ImagePosition = position,
                            IsPrimary = i == 0 // First image is primary
                        });
                    }

                    result.SuccessfulImports++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing exercise {exerciseDto.SpanishName}: {ex.Message}");
                    result.FailedImports++;
                }
            }

            // Save to database
            if (exercisesToAdd.Any())
            {
                await _context.Exercises.AddRangeAsync(exercisesToAdd);
                await _context.SaveChangesAsync();

                // Add secondary muscles and images
                if (secondaryMuscles.Any())
                {
                    await _context.ExerciseSecondaryMuscles.AddRangeAsync(secondaryMuscles);
                }

                if (exerciseImages.Any())
                {
                    await _context.ExerciseImages.AddRangeAsync(exerciseImages);
                }

                await _context.SaveChangesAsync();
            }

            result.Success = result.FailedImports == 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Database error during import: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<ImportValidationResult> ValidateImportDataAsync(IEnumerable<ExerciseImportData> exerciseData)
    {
        var result = new ImportValidationResult { IsValid = true };

        // Load lookup data for validation
        var muscleGroupNames = (await _context.MuscleGroups.Select(mg => mg.Name).ToListAsync()).ToHashSet();
        var equipmentTypeNames = (await _context.EquipmentTypes.Select(et => et.Name).ToListAsync()).ToHashSet();

        var validDifficultyLevels = Enum.GetNames<DifficultyLevel>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var validExerciseTypes = Enum.GetNames<ExerciseType>().ToHashSet(StringComparer.OrdinalIgnoreCase);

        var exerciseNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var exercise in exerciseData)
        {
            // Validate required fields
            var validationContext = new ValidationContext(exercise);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(exercise, validationContext, validationResults, true))
            {
                foreach (var validationError in validationResults)
                {
                    result.Errors.Add($"{exercise.SpanishName}: {validationError.ErrorMessage}");
                }
                result.IsValid = false;
            }

            // Check for duplicate exercise names
            if (!exerciseNames.Add(exercise.SpanishName))
            {
                result.Errors.Add($"Duplicate exercise name: {exercise.SpanishName}");
                result.IsValid = false;
            }

            // Validate lookup values
            if (!muscleGroupNames.Contains(exercise.PrimaryMuscleGroup))
            {
                result.Errors.Add($"{exercise.SpanishName}: Invalid primary muscle group '{exercise.PrimaryMuscleGroup}'");
                result.IsValid = false;
            }

            if (!equipmentTypeNames.Contains(exercise.EquipmentType))
            {
                result.Errors.Add($"{exercise.SpanishName}: Invalid equipment type '{exercise.EquipmentType}'");
                result.IsValid = false;
            }

            if (!validDifficultyLevels.Contains(exercise.DifficultyLevel))
            {
                result.Errors.Add($"{exercise.SpanishName}: Invalid difficulty level '{exercise.DifficultyLevel}'");
                result.IsValid = false;
            }

            if (!validExerciseTypes.Contains(exercise.ExerciseType))
            {
                result.Errors.Add($"{exercise.SpanishName}: Invalid exercise type '{exercise.ExerciseType}'");
                result.IsValid = false;
            }

            // Validate secondary muscle groups
            foreach (var secondaryMuscle in exercise.SecondaryMuscleGroups)
            {
                if (!muscleGroupNames.Contains(secondaryMuscle))
                {
                    result.Warnings.Add($"{exercise.SpanishName}: Invalid secondary muscle group '{secondaryMuscle}'");
                }
            }

            // Validate image paths
            foreach (var imagePath in exercise.ImagePaths)
            {
                if (!string.IsNullOrEmpty(imagePath) && !File.Exists(imagePath))
                {
                    result.Warnings.Add($"{exercise.SpanishName}: Image file not found '{imagePath}'");
                }
            }

            // Validate duration for isometric exercises
            if (exercise.ExerciseType.Equals("Isometric", StringComparison.OrdinalIgnoreCase) && !exercise.DurationSeconds.HasValue)
            {
                result.Warnings.Add($"{exercise.SpanishName}: Isometric exercises should have a duration specified");
            }
        }

        return result;
    }

    public async Task<ImportResult> ImportBulkSeedDataAsync()
    {
        var exerciseData = GetBulkSeedData();
        return await ImportFromDataAsync(exerciseData);
    }

    private List<ExerciseImportData> GetBulkSeedData()
    {
        return new List<ExerciseImportData>
        {
            // CHEST EXERCISES
            new ExerciseImportData
            {
                Name = "Push-ups",
                SpanishName = "Flexiones de Pecho",
                Description = "Ejercicio básico de empuje para fortalecer el pecho, hombros y tríceps",
                Instructions = "Colócate en posición de plancha con las manos separadas al ancho de los hombros. Baja el cuerpo hasta que el pecho casi toque el suelo. Empuja hacia arriba hasta la posición inicial manteniendo el cuerpo recto.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders", "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Incline Push-ups",
                SpanishName = "Flexiones Inclinadas",
                Description = "Variante más fácil de flexiones con las manos elevadas",
                Instructions = "Coloca las manos en una superficie elevada (banco, escalón). Realiza flexiones manteniendo el cuerpo recto.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders", "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Decline Push-ups",
                SpanishName = "Flexiones Declinadas",
                Description = "Variante más difícil con los pies elevados",
                Instructions = "Coloca los pies en una superficie elevada. Realiza flexiones enfatizando la parte superior del pecho.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders", "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Diamond Push-ups",
                SpanishName = "Flexiones Diamante",
                Description = "Flexiones con las manos en forma de diamante para enfatizar tríceps",
                Instructions = "Forma un diamante con tus manos bajo el pecho. Realiza flexiones manteniendo los codos cerca del cuerpo.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Chest" }
            },
            new ExerciseImportData
            {
                Name = "Bench Press",
                SpanishName = "Press de Banca",
                Description = "Ejercicio básico de empuje con barra para el desarrollo del pecho",
                Instructions = "Acuéstate en el banco con los pies firmes en el suelo. Baja la barra hasta el pecho controladamente. Empuja hacia arriba hasta extender completamente los brazos.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders", "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Incline Bench Press",
                SpanishName = "Press de Banca Inclinado",
                Description = "Press de banca en banco inclinado para la parte superior del pecho",
                Instructions = "En banco inclinado 30-45 grados, realiza press de banca enfocándote en la parte superior del pecho.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders", "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Decline Bench Press",
                SpanishName = "Press de Banca Declinado",
                Description = "Press de banca en banco declinado para la parte inferior del pecho",
                Instructions = "En banco declinado, realiza press de banca enfocándote en la parte inferior del pecho.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders", "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Dumbbell Flyes",
                SpanishName = "Aperturas con Mancuernas",
                Description = "Ejercicio de aislamiento para el pecho con mancuernas",
                Instructions = "Acostado en banco, abre y cierra los brazos en arco amplio manteniendo ligera flexión en los codos.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders" }
            },
            new ExerciseImportData
            {
                Name = "Chest Dips",
                SpanishName = "Fondos en Paralelas",
                Description = "Ejercicio de peso corporal para pecho y tríceps",
                Instructions = "En paralelas, baja el cuerpo inclinándote ligeramente hacia adelante. Sube hasta la posición inicial.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms", "Shoulders" }
            },
            new ExerciseImportData
            {
                Name = "Cable Crossover",
                SpanishName = "Cruces en Polea",
                Description = "Ejercicio de aislamiento en máquina de poleas",
                Instructions = "Con poleas altas, cruza los cables por delante del pecho manteniendo ligera flexión en los codos.",
                PrimaryMuscleGroup = "Chest",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Shoulders" }
            },

            // BACK EXERCISES
            new ExerciseImportData
            {
                Name = "Pull-ups",
                SpanishName = "Dominadas",
                Description = "Ejercicio de tracción para desarrollar la espalda y bíceps",
                Instructions = "Cuelgate de una barra con las palmas hacia adelante, separadas al ancho de los hombros. Tira de tu cuerpo hacia arriba hasta que la barbilla pase la barra. Baja controladamente.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Pull-up Bar",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Chin-ups",
                SpanishName = "Dominadas Supinas",
                Description = "Dominadas con agarre supino enfatizando bíceps",
                Instructions = "Con las palmas hacia ti, realiza dominadas enfocándote en contraer la espalda y bíceps.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Pull-up Bar",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Assisted Pull-ups",
                SpanishName = "Dominadas Asistidas",
                Description = "Dominadas con ayuda para principiantes",
                Instructions = "Usa una banda de resistencia o máquina asistida para realizar dominadas con menos peso corporal.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Resistance Bands",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Deadlift",
                SpanishName = "Peso Muerto",
                Description = "Ejercicio compuesto fundamental para espalda, piernas y glúteos",
                Instructions = "Con la barra en el suelo, inclínate manteniendo la espalda recta y agárrala. Levanta extendiendo caderas y rodillas hasta estar completamente erguido.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Advanced",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs", "Glutes" }
            },
            new ExerciseImportData
            {
                Name = "Romanian Deadlift",
                SpanishName = "Peso Muerto Rumano",
                Description = "Variante de peso muerto enfocada en isquiotibiales y glúteos",
                Instructions = "Con la barra en las manos, baja flexionando las caderas manteniendo las piernas casi rectas. Regresa extendiendo las caderas.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs", "Back" }
            },
            new ExerciseImportData
            {
                Name = "Barbell Row",
                SpanishName = "Remo con Barra",
                Description = "Ejercicio de tracción horizontal para espalda media",
                Instructions = "Inclinado hacia adelante, rema la barra hacia el abdomen bajo contrayendo los omóplatos.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Dumbbell Row",
                SpanishName = "Remo con Mancuernas",
                Description = "Ejercicio de tracción unilateral para espalda media",
                Instructions = "Con apoyo en banco, rema la mancuerna hacia el costado del torso contrayendo el omóplato.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "T-Bar Row",
                SpanishName = "Remo en T",
                Description = "Ejercicio de remo con barra en T para grosor de espalda",
                Instructions = "Con la barra en T, rema hacia el pecho bajo manteniendo el pecho erguido.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Seated Cable Row",
                SpanishName = "Remo Sentado en Polea",
                Description = "Ejercicio de remo horizontal en máquina",
                Instructions = "Sentado, tira del cable hacia el abdomen manteniendo la espalda recta y contrayendo los omóplatos.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Lat Pulldown",
                SpanishName = "Jalón al Pecho",
                Description = "Ejercicio de tracción vertical en máquina",
                Instructions = "Sentado, tira de la barra hacia el pecho superior contrayendo los dorsales.",
                PrimaryMuscleGroup = "Back",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },

            // SHOULDER EXERCISES
            new ExerciseImportData
            {
                Name = "Military Press",
                SpanishName = "Press Militar",
                Description = "Ejercicio de empuje vertical para desarrollo completo de hombros",
                Instructions = "De pie con la barra a la altura de los hombros, levanta la barra hasta arriba de la cabeza. Baja controladamente.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Dumbbell Shoulder Press",
                SpanishName = "Press de Hombros con Mancuernas",
                Description = "Press de hombros con mancuernas para desarrollo unilateral",
                Instructions = "Sentado o de pie, levanta las mancuernas desde los hombros hasta arriba de la cabeza.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Lateral Raises",
                SpanishName = "Elevaciones Laterales",
                Description = "Ejercicio de aislamiento para deltoides medio",
                Instructions = "Con mancuernas a los lados, levanta los brazos hacia los costados hasta la altura de los hombros. Baja controladamente.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Front Raises",
                SpanishName = "Elevaciones Frontales",
                Description = "Ejercicio de aislamiento para deltoides anterior",
                Instructions = "Con mancuernas al frente, levanta alternadamente los brazos hacia adelante hasta la altura de los hombros.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Rear Delt Flyes",
                SpanishName = "Aperturas Posteriores",
                Description = "Ejercicio para deltoides posterior",
                Instructions = "Inclinado hacia adelante, abre los brazos hacia los lados apretando los omóplatos.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Back" }
            },
            new ExerciseImportData
            {
                Name = "Arnold Press",
                SpanishName = "Press Arnold",
                Description = "Variante de press de hombros con rotación",
                Instructions = "Inicia con las palmas hacia ti, rota mientras presionas hasta tener las palmas hacia adelante arriba.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Upright Row",
                SpanishName = "Remo Vertical",
                Description = "Ejercicio de tracción vertical para hombros y trapecios",
                Instructions = "Con la barra pegada al cuerpo, levanta los codos hacia arriba llevando la barra hacia el pecho.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Back" }
            },
            new ExerciseImportData
            {
                Name = "Face Pulls",
                SpanishName = "Jalones a la Cara",
                Description = "Ejercicio para deltoides posterior y estabilidad del hombro",
                Instructions = "Con cable a la altura del rostro, tira separando las manos hacia las orejas.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Back" }
            },
            new ExerciseImportData
            {
                Name = "Pike Push-ups",
                SpanishName = "Flexiones Pike",
                Description = "Ejercicio de peso corporal para hombros",
                Instructions = "En posición de V invertida, realiza flexiones enfocándote en los hombros.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms" }
            },
            new ExerciseImportData
            {
                Name = "Handstand Push-ups",
                SpanishName = "Flexiones en Pino",
                Description = "Ejercicio avanzado de peso corporal para hombros",
                Instructions = "En posición de pino contra la pared, realiza flexiones verticales.",
                PrimaryMuscleGroup = "Shoulders",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Expert",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Arms", "Core" }
            },

            // ARM EXERCISES
            new ExerciseImportData
            {
                Name = "Bicep Curls",
                SpanishName = "Curl de Bíceps",
                Description = "Ejercicio básico de aislamiento para bíceps",
                Instructions = "Con mancuernas en las manos, flexiona los codos llevando el peso hacia los hombros. Mantén los codos fijos y baja controladamente.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Hammer Curls",
                SpanishName = "Curl Martillo",
                Description = "Curl de bíceps con agarre neutro",
                Instructions = "Con mancuernas en posición de martillo, flexiona los codos manteniendo las muñecas neutrales.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Barbell Curls",
                SpanishName = "Curl con Barra",
                Description = "Curl de bíceps con barra para mayor carga",
                Instructions = "Con la barra en las manos, flexiona los codos llevando la barra hacia el pecho.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Preacher Curls",
                SpanishName = "Curl en Banco Scott",
                Description = "Curl de bíceps con apoyo para aislamiento",
                Instructions = "En banco Scott, realiza curls manteniendo los brazos apoyados en el banco.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Tricep Pushdown",
                SpanishName = "Tríceps en Polea",
                Description = "Ejercicio de aislamiento para tríceps en máquina de poleas",
                Instructions = "En la polea alta con barra recta, empuja hacia abajo extendiendo completamente los codos. Regresa controladamente.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Rope Tricep Pushdown",
                SpanishName = "Tríceps en Polea con Cuerda",
                Description = "Extensión de tríceps con cuerda para mayor rango",
                Instructions = "Con cuerda en polea alta, separa las manos al final del movimiento para mayor contracción.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Overhead Tricep Extension",
                SpanishName = "Extensión de Tríceps por Encima",
                Description = "Ejercicio de tríceps con mancuerna sobre la cabeza",
                Instructions = "Con una mancuerna sobre la cabeza, baja flexionando solo los codos y regresa a la posición inicial.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Close-Grip Bench Press",
                SpanishName = "Press de Banca Agarre Cerrado",
                Description = "Press de banca con agarre estrecho para tríceps",
                Instructions = "Con agarre más estrecho que el ancho de hombros, realiza press enfocándote en tríceps.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Chest" }
            },
            new ExerciseImportData
            {
                Name = "Tricep Dips",
                SpanishName = "Fondos de Tríceps",
                Description = "Ejercicio de peso corporal para tríceps",
                Instructions = "En banco o paralelas, baja el cuerpo flexionando los codos y sube extendiendo los tríceps.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "21s Bicep Curls",
                SpanishName = "Curl 21",
                Description = "Técnica de entrenamiento intenso para bíceps",
                Instructions = "7 medias repeticiones abajo, 7 medias arriba, 7 repeticiones completas sin descanso.",
                PrimaryMuscleGroup = "Arms",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Advanced",
                ExerciseType = "Strength"
            },

            // LEG EXERCISES
            new ExerciseImportData
            {
                Name = "Squats",
                SpanishName = "Sentadillas",
                Description = "Ejercicio fundamental para piernas y glúteos",
                Instructions = "Ponte de pie con los pies separados al ancho de los hombros. Baja como si fueras a sentarte en una silla invisible, manteniendo la espalda recta. Regresa a la posición inicial.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Jump Squats",
                SpanishName = "Sentadillas con Salto",
                Description = "Variante explosiva de sentadillas",
                Instructions = "Realiza una sentadilla y al subir, salta explosivamente. Aterriza suavemente y repite.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Plyometric",
                SecondaryMuscleGroups = new List<string> { "Glutes" }
            },
            new ExerciseImportData
            {
                Name = "Goblet Squats",
                SpanishName = "Sentadillas Goblet",
                Description = "Sentadillas con mancuerna en el pecho",
                Instructions = "Sostén una mancuerna contra el pecho y realiza sentadillas profundas.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Barbell Back Squat",
                SpanishName = "Sentadilla con Barra",
                Description = "Sentadilla con barra en la espalda",
                Instructions = "Con la barra en la espalda alta, realiza sentadillas profundas manteniendo la técnica.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Front Squats",
                SpanishName = "Sentadillas Frontales",
                Description = "Sentadillas con barra en la parte frontal",
                Instructions = "Con la barra en la parte frontal de los hombros, realiza sentadillas enfatizando cuádriceps.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Advanced",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Core" }
            },
            new ExerciseImportData
            {
                Name = "Lunges",
                SpanishName = "Zancadas",
                Description = "Ejercicio unilateral para piernas y glúteos",
                Instructions = "Da un paso largo hacia adelante, baja la rodilla trasera hacia el suelo manteniendo el torso erguido. Regresa a la posición inicial.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Reverse Lunges",
                SpanishName = "Zancadas Inversas",
                Description = "Zancadas hacia atrás para mayor estabilidad",
                Instructions = "Da un paso hacia atrás y baja en zancada. Regresa a la posición inicial.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes" }
            },
            new ExerciseImportData
            {
                Name = "Walking Lunges",
                SpanishName = "Zancadas Caminando",
                Description = "Zancadas en movimiento continuo",
                Instructions = "Realiza zancadas caminando hacia adelante alternando las piernas.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Bulgarian Split Squats",
                SpanishName = "Sentadillas Búlgaras",
                Description = "Sentadillas unilaterales con pie trasero elevado",
                Instructions = "Con el pie trasero en banco, realiza sentadillas con la pierna delantera.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes" }
            },
            new ExerciseImportData
            {
                Name = "Leg Press",
                SpanishName = "Prensa de Piernas",
                Description = "Ejercicio de piernas en máquina",
                Instructions = "En la máquina de prensa, empuja la plataforma con los pies separados al ancho de hombros.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes" }
            },
            new ExerciseImportData
            {
                Name = "Leg Curls",
                SpanishName = "Curl de Piernas",
                Description = "Ejercicio de aislamiento para isquiotibiales",
                Instructions = "En máquina de curl, flexiona las piernas llevando los talones hacia los glúteos.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Leg Extensions",
                SpanishName = "Extensiones de Piernas",
                Description = "Ejercicio de aislamiento para cuádriceps",
                Instructions = "En máquina de extensiones, extiende las piernas hasta la posición horizontal.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Machines",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Calf Raises",
                SpanishName = "Elevaciones de Pantorrillas",
                Description = "Ejercicio para músculos de la pantorrilla",
                Instructions = "Ponte de puntillas contrayendo las pantorrillas, mantén y baja lentamente.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Step-ups",
                SpanishName = "Subidas al Banco",
                Description = "Ejercicio unilateral subiendo a plataforma",
                Instructions = "Sube a un banco con una pierna, llevando la otra rodilla arriba. Baja controladamente.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Glutes" }
            },
            new ExerciseImportData
            {
                Name = "Wall Sit",
                SpanishName = "Sentadilla en Pared",
                Description = "Ejercicio isométrico para piernas",
                Instructions = "Apoyado en la pared en posición de sentadilla, mantén la posición el tiempo indicado.",
                PrimaryMuscleGroup = "Legs",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Isometric",
                DurationSeconds = 30,
                SecondaryMuscleGroups = new List<string> { "Glutes" }
            },

            // GLUTE EXERCISES
            new ExerciseImportData
            {
                Name = "Glute Bridge",
                SpanishName = "Puentes de Glúteo",
                Description = "Ejercicio específico para activar y fortalecer los glúteos",
                Instructions = "Acostado boca arriba con rodillas flexionadas, levanta las caderas contrayendo fuertemente los glúteos. Mantén la posición y baja controladamente.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs" }
            },
            new ExerciseImportData
            {
                Name = "Single-Leg Glute Bridge",
                SpanishName = "Puente de Glúteo Una Pierna",
                Description = "Variante unilateral más desafiante",
                Instructions = "Realiza puentes de glúteo con una sola pierna, manteniendo la otra extendida.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Hip Thrusts",
                SpanishName = "Empujes de Cadera",
                Description = "Variante elevada de puentes de glúteo",
                Instructions = "Con la espalda apoyada en banco, realiza empujes de cadera con mayor rango de movimiento.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs" }
            },
            new ExerciseImportData
            {
                Name = "Weighted Hip Thrusts",
                SpanishName = "Empujes de Cadera con Peso",
                Description = "Hip thrusts con carga adicional",
                Instructions = "Con peso en las caderas, realiza empujes de cadera para mayor resistencia.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Advanced",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs" }
            },
            new ExerciseImportData
            {
                Name = "Clamshells",
                SpanishName = "Almejas",
                Description = "Ejercicio de activación para glúteo medio",
                Instructions = "De lado con rodillas flexionadas, abre y cierra la rodilla superior como una almeja.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Side-Lying Leg Lifts",
                SpanishName = "Elevaciones Laterales de Pierna",
                Description = "Ejercicio para glúteo medio y menor",
                Instructions = "De lado, eleva la pierna superior manteniendo el cuerpo alineado.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Fire Hydrants",
                SpanishName = "Hidrantes",
                Description = "Ejercicio de activación en cuadrupedia",
                Instructions = "En posición de cuatro patas, levanta la rodilla hacia el lado como un perro en hidrante.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Sumo Squats",
                SpanishName = "Sentadillas Sumo",
                Description = "Sentadillas con stance amplio para glúteos",
                Instructions = "Con pies muy separados y puntas hacia afuera, realiza sentadillas profundas.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs" }
            },
            new ExerciseImportData
            {
                Name = "Curtsy Lunges",
                SpanishName = "Zancadas Reverencia",
                Description = "Zancadas cruzadas para glúteo medio",
                Instructions = "Cruza una pierna detrás de la otra en zancada, como haciendo reverencia.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs" }
            },
            new ExerciseImportData
            {
                Name = "Donkey Kicks",
                SpanishName = "Patadas de Burro",
                Description = "Ejercicio de extensión de cadera en cuadrupedia",
                Instructions = "En cuatro patas, extiende una pierna hacia atrás manteniendo la rodilla flexionada.",
                PrimaryMuscleGroup = "Glutes",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },

            // CORE EXERCISES
            new ExerciseImportData
            {
                Name = "Plank",
                SpanishName = "Plancha",
                Description = "Ejercicio isométrico fundamental para fortalecer el core",
                Instructions = "Mantén una posición de plancha con el cuerpo recto desde la cabeza hasta los talones. Contrae activamente el abdomen y mantén la respiración normal.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Isometric",
                DurationSeconds = 30
            },
            new ExerciseImportData
            {
                Name = "Side Plank",
                SpanishName = "Plancha Lateral",
                Description = "Plancha lateral para oblicuos",
                Instructions = "De lado apoyado en un antebrazo, mantén el cuerpo recto formando una línea.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Isometric",
                DurationSeconds = 30
            },
            new ExerciseImportData
            {
                Name = "Plank Up-Downs",
                SpanishName = "Plancha Subidas y Bajadas",
                Description = "Plancha dinámica alternando antebrazos y manos",
                Instructions = "Desde plancha en antebrazos, sube a plancha alta alternando los brazos.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Crunches",
                SpanishName = "Abdominales Crunch",
                Description = "Ejercicio básico para abdominales superiores",
                Instructions = "Acostado boca arriba con rodillas flexionadas, levanta los hombros del suelo contrayendo el abdomen. Baja controladamente sin relajar completamente.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Bicycle Crunches",
                SpanishName = "Abdominales Bicicleta",
                Description = "Crunch con rotación para oblicuos",
                Instructions = "Alterna llevando codo a rodilla opuesta en movimiento de pedaleo.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Russian Twists",
                SpanishName = "Giros Rusos",
                Description = "Ejercicio de rotación para oblicuos",
                Instructions = "Sentado con pies elevados, gira el torso de lado a lado tocando el suelo.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Leg Raises",
                SpanishName = "Elevaciones de Piernas",
                Description = "Ejercicio para abdominales inferiores",
                Instructions = "Acostado, eleva las piernas rectas hasta formar 90 grados con el torso.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Mountain Climbers",
                SpanishName = "Escaladores",
                Description = "Ejercicio dinámico de core y cardio",
                Instructions = "En posición de plancha, alterna llevando las rodillas al pecho rápidamente.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Cardio",
                SecondaryMuscleGroups = new List<string> { "Shoulders" }
            },
            new ExerciseImportData
            {
                Name = "Dead Bug",
                SpanishName = "Bicho Muerto",
                Description = "Ejercicio de control del core",
                Instructions = "Boca arriba con brazos y piernas a 90 grados, extiende brazo y pierna opuestos.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Bird Dog",
                SpanishName = "Perro Pájaro",
                Description = "Ejercicio de estabilidad en cuadrupedia",
                Instructions = "En cuatro patas, extiende brazo y pierna opuestos manteniendo la estabilidad.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Back" }
            },
            new ExerciseImportData
            {
                Name = "Hollow Body Hold",
                SpanishName = "Posición Hueca",
                Description = "Ejercicio isométrico de core avanzado",
                Instructions = "Boca arriba, presiona la espalda baja al suelo mientras levantas hombros y piernas.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Advanced",
                ExerciseType = "Isometric",
                DurationSeconds = 30
            },
            new ExerciseImportData
            {
                Name = "V-Ups",
                SpanishName = "Abdominales en V",
                Description = "Ejercicio completo de core",
                Instructions = "Simultaneamente levanta el torso y las piernas formando una V.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Advanced",
                ExerciseType = "Strength"
            },
            new ExerciseImportData
            {
                Name = "Hanging Knee Raises",
                SpanishName = "Elevaciones de Rodillas Colgado",
                Description = "Ejercicio de core colgado en barra",
                Instructions = "Colgado de una barra, eleva las rodillas hacia el pecho contrayendo el abdomen.",
                PrimaryMuscleGroup = "Core",
                EquipmentType = "Pull-up Bar",
                DifficultyLevel = "Advanced",
                ExerciseType = "Strength"
            },

            // FULL BODY EXERCISES
            new ExerciseImportData
            {
                Name = "Burpees",
                SpanishName = "Burpees",
                Description = "Ejercicio de cuerpo completo que combina fuerza y cardio",
                Instructions = "Desde de pie, baja a cuclillas, coloca las manos en el suelo, salta hacia posición de plancha, haz una flexión opcional, salta los pies hacia las manos y salta arriba con los brazos extendidos.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Plyometric",
                SecondaryMuscleGroups = new List<string> { "Core", "Chest", "Legs" }
            },
            new ExerciseImportData
            {
                Name = "Turkish Get-ups",
                SpanishName = "Levantamiento Turco",
                Description = "Ejercicio complejo de cuerpo completo",
                Instructions = "Con peso en una mano, pasa de acostado a de pie siguiendo la secuencia específica.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Expert",
                ExerciseType = "Functional",
                SecondaryMuscleGroups = new List<string> { "Core", "Shoulders" }
            },
            new ExerciseImportData
            {
                Name = "Thrusters",
                SpanishName = "Thrusters",
                Description = "Combinación de sentadilla y press de hombros",
                Instructions = "Realiza una sentadilla y al subir, presiona las mancuernas sobre la cabeza.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Strength",
                SecondaryMuscleGroups = new List<string> { "Legs", "Shoulders" }
            },
            new ExerciseImportData
            {
                Name = "Man Makers",
                SpanishName = "Man Makers",
                Description = "Ejercicio completo que combina múltiples movimientos",
                Instructions = "Burpee con mancuernas, remo en plancha y press de hombros al final.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Free Weights",
                DifficultyLevel = "Advanced",
                ExerciseType = "Functional",
                SecondaryMuscleGroups = new List<string> { "Core", "Back", "Shoulders" }
            },
            new ExerciseImportData
            {
                Name = "Bear Crawl",
                SpanishName = "Gateo de Oso",
                Description = "Movimiento de locomoción en cuadrupedia",
                Instructions = "En cuatro patas con rodillas ligeramente elevadas, avanza alternando manos y pies.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Functional",
                SecondaryMuscleGroups = new List<string> { "Core", "Shoulders" }
            },
            new ExerciseImportData
            {
                Name = "Crab Walk",
                SpanishName = "Gateo de Cangrejo",
                Description = "Movimiento de locomoción hacia atrás",
                Instructions = "Sentado con manos y pies apoyados, camina hacia atrás manteniendo las caderas elevadas.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Functional",
                SecondaryMuscleGroups = new List<string> { "Arms", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Jumping Jacks",
                SpanishName = "Saltos de Tijera",
                Description = "Ejercicio cardiovascular básico",
                Instructions = "Salta separando pies y brazos simultáneamente, regresa a posición inicial.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Cardio"
            },
            new ExerciseImportData
            {
                Name = "High Knees",
                SpanishName = "Rodillas Altas",
                Description = "Ejercicio de activación y cardio",
                Instructions = "Corre en el lugar llevando las rodillas tan alto como sea posible.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Cardio",
                SecondaryMuscleGroups = new List<string> { "Legs", "Core" }
            },
            new ExerciseImportData
            {
                Name = "Butt Kickers",
                SpanishName = "Talones a Glúteos",
                Description = "Ejercicio de activación para isquiotibiales",
                Instructions = "Corre en el lugar llevando los talones hacia los glúteos.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Beginner",
                ExerciseType = "Cardio",
                SecondaryMuscleGroups = new List<string> { "Legs" }
            },
            new ExerciseImportData
            {
                Name = "Sprawls",
                SpanishName = "Sprawls",
                Description = "Variante de burpees sin salto final",
                Instructions = "Desde de pie, baja a cuclillas, salta a plancha, regresa a cuclillas y ponte de pie.",
                PrimaryMuscleGroup = "FullBody",
                EquipmentType = "Bodyweight",
                DifficultyLevel = "Intermediate",
                ExerciseType = "Cardio",
                SecondaryMuscleGroups = new List<string> { "Core", "Chest" }
            }
        };
    }
}