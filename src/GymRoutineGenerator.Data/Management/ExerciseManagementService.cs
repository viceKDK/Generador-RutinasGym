using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Infrastructure.Images;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Data.Management;

public class ExerciseManagementService : IExerciseManagementService
{
    private readonly GymRoutineContext _context;
    private readonly IImageService _imageService;

    public ExerciseManagementService(GymRoutineContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<ExerciseManagementResult> CreateExerciseAsync(ExerciseCreateRequest request)
    {
        var result = new ExerciseManagementResult();

        try
        {
            // Validate the request
            var validation = await ValidateExerciseAsync(request);
            if (!validation.IsValid)
            {
                result.Errors.AddRange(validation.Errors.Select(e => e.Message));
                return result;
            }

            // Create the exercise entity
            var exercise = new Exercise
            {
                Name = request.Name.Trim(),
                SpanishName = request.SpanishName.Trim(),
                Description = request.Description.Trim(),
                Instructions = request.Instructions.Trim(),
                PrimaryMuscleGroupId = request.PrimaryMuscleGroupId,
                EquipmentTypeId = request.EquipmentTypeId,
                ParentExerciseId = request.ParentExerciseId,
                DifficultyLevel = request.DifficultyLevel,
                ExerciseType = request.ExerciseType,
                DurationSeconds = request.DurationSeconds,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add to context
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Add secondary muscles
            if (request.SecondaryMuscleGroupIds.Any())
            {
                var secondaryMuscles = request.SecondaryMuscleGroupIds.Select(id => new ExerciseSecondaryMuscle
                {
                    ExerciseId = exercise.Id,
                    MuscleGroupId = id
                }).ToList();

                _context.ExerciseSecondaryMuscles.AddRange(secondaryMuscles);
                await _context.SaveChangesAsync();
            }

            // Process images
            if (request.Images.Any())
            {
                var imageResults = await ProcessExerciseImagesAsync(exercise.Id, request.Images);
                result.Warnings.AddRange(imageResults.Where(r => !r.Success).Select(r => r.Message ?? "Error procesando imagen"));
            }

            result.Success = true;
            result.ExerciseId = exercise.Id;
            result.Message = $"Ejercicio '{exercise.SpanishName}' creado exitosamente";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error creating exercise: {ex.Message}");
        }

        return result;
    }

    public async Task<ExerciseManagementResult> UpdateExerciseAsync(ExerciseUpdateRequest request)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var exercise = await _context.Exercises
                .Include(e => e.SecondaryMuscles)
                .Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.Id == request.Id);

            if (exercise == null)
            {
                result.Errors.Add($"Exercise with ID {request.Id} not found");
                return result;
            }

            // Validate the request
            var validation = await ValidateExerciseAsync(request, request.Id);
            if (!validation.IsValid)
            {
                result.Errors.AddRange(validation.Errors.Select(e => e.Message));
                return result;
            }

            // Update exercise properties
            exercise.Name = request.Name.Trim();
            exercise.SpanishName = request.SpanishName.Trim();
            exercise.Description = request.Description.Trim();
            exercise.Instructions = request.Instructions.Trim();
            exercise.PrimaryMuscleGroupId = request.PrimaryMuscleGroupId;
            exercise.EquipmentTypeId = request.EquipmentTypeId;
            exercise.ParentExerciseId = request.ParentExerciseId;
            exercise.DifficultyLevel = request.DifficultyLevel;
            exercise.ExerciseType = request.ExerciseType;
            exercise.DurationSeconds = request.DurationSeconds;
            exercise.IsActive = request.IsActive;
            exercise.UpdatedAt = DateTime.UtcNow;

            // Update secondary muscles
            var currentSecondaryMuscles = exercise.SecondaryMuscles.ToList();
            var newSecondaryMuscleIds = request.SecondaryMuscleGroupIds;

            // Remove muscles that are no longer needed
            var musclesToRemove = currentSecondaryMuscles
                .Where(sm => !newSecondaryMuscleIds.Contains(sm.MuscleGroupId))
                .ToList();
            _context.ExerciseSecondaryMuscles.RemoveRange(musclesToRemove);

            // Add new muscles
            var currentMuscleIds = currentSecondaryMuscles.Select(sm => sm.MuscleGroupId).ToList();
            var musclesToAdd = newSecondaryMuscleIds
                .Where(id => !currentMuscleIds.Contains(id))
                .Select(id => new ExerciseSecondaryMuscle
                {
                    ExerciseId = exercise.Id,
                    MuscleGroupId = id
                })
                .ToList();
            _context.ExerciseSecondaryMuscles.AddRange(musclesToAdd);

            await _context.SaveChangesAsync();

            // Process new images
            if (request.Images.Any())
            {
                var imageResults = await ProcessExerciseImagesAsync(exercise.Id, request.Images);
                result.Warnings.AddRange(imageResults.Where(r => !r.Success).Select(r => r.Message ?? "Error procesando imagen"));
            }

            result.Success = true;
            result.ExerciseId = exercise.Id;
            result.Message = $"Ejercicio '{exercise.SpanishName}' actualizado exitosamente";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error updating exercise: {ex.Message}");
        }

        return result;
    }

    public async Task<ExerciseManagementResult> DeleteExerciseAsync(int exerciseId, bool forceDelete = false)
    {
        var result = new ExerciseManagementResult();

        try
        {
            // Check if deletion is allowed
            var deletionCheck = await CheckDeletionAsync(exerciseId);
            if (!deletionCheck.CanDelete && !forceDelete)
            {
                result.Errors.AddRange(deletionCheck.Dependencies);
                result.Message = "Cannot delete exercise due to dependencies. Use force delete if necessary.";
                return result;
            }

            var exercise = await _context.Exercises
                .Include(e => e.SecondaryMuscles)
                .Include(e => e.Images)
                .Include(e => e.ChildExercises)
                .FirstOrDefaultAsync(e => e.Id == exerciseId);

            if (exercise == null)
            {
                result.Errors.Add($"Exercise with ID {exerciseId} not found");
                return result;
            }

            // Remove dependencies if force delete
            if (forceDelete)
            {
                // Remove secondary muscles
                _context.ExerciseSecondaryMuscles.RemoveRange(exercise.SecondaryMuscles);

                // Remove images
                foreach (var image in exercise.Images)
                {
                    if (File.Exists(image.ImagePath))
                    {
                        try
                        {
                            File.Delete(image.ImagePath);
                        }
                        catch
                        {
                            result.Warnings.Add($"Could not delete image file: {image.ImagePath}");
                        }
                    }
                }
                _context.ExerciseImages.RemoveRange(exercise.Images);

                // Update child exercises to remove parent reference
                foreach (var child in exercise.ChildExercises)
                {
                    child.ParentExerciseId = null;
                }
            }

            // Remove the exercise
            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = $"Ejercicio '{exercise.SpanishName}' eliminado exitosamente";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error deleting exercise: {ex.Message}");
        }

        return result;
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int exerciseId)
    {
        return await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.ParentExercise)
            .Include(e => e.ChildExercises)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
    }

    public async Task<List<Exercise>> GetAllExercisesAsync(bool includeInactive = false)
    {
        var query = _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query
            .OrderBy(e => e.SpanishName)
            .ToListAsync();
    }

    public async Task<ExerciseValidationResult> ValidateExerciseAsync(ExerciseCreateRequest request, int? excludeExerciseId = null)
    {
        var result = new ExerciseValidationResult { IsValid = true };

        // Validate using data annotations
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            result.IsValid = false;
            result.Errors.AddRange(validationResults.Select(vr => new ValidationError
            {
                Field = vr.MemberNames.FirstOrDefault() ?? "Unknown",
                Message = vr.ErrorMessage ?? "Validation error",
                Code = "VALIDATION"
            }));
        }

        // Check for duplicate names
        var duplicateQuery = _context.Exercises
            .Where(e => e.SpanishName.ToLower() == request.SpanishName.ToLower().Trim());

        if (excludeExerciseId.HasValue)
        {
            duplicateQuery = duplicateQuery.Where(e => e.Id != excludeExerciseId.Value);
        }

        var duplicateExists = await duplicateQuery.AnyAsync();
        if (duplicateExists)
        {
            result.IsValid = false;
            result.HasDuplicateName = true;
            result.Errors.Add(new ValidationError
            {
                Field = nameof(request.SpanishName),
                Message = $"Ya existe un ejercicio con el nombre '{request.SpanishName}'",
                Code = "DUPLICATE_NAME"
            });
        }

        // Validate foreign key references
        var muscleGroupExists = await _context.MuscleGroups.AnyAsync(mg => mg.Id == request.PrimaryMuscleGroupId);
        if (!muscleGroupExists)
        {
            result.IsValid = false;
            result.HasInvalidReferences = true;
            result.Errors.Add(new ValidationError
            {
                Field = nameof(request.PrimaryMuscleGroupId),
                Message = "El grupo muscular principal especificado no existe",
                Code = "INVALID_MUSCLE_GROUP"
            });
        }

        var equipmentExists = await _context.EquipmentTypes.AnyAsync(et => et.Id == request.EquipmentTypeId);
        if (!equipmentExists)
        {
            result.IsValid = false;
            result.HasInvalidReferences = true;
            result.Errors.Add(new ValidationError
            {
                Field = nameof(request.EquipmentTypeId),
                Message = "El tipo de equipo especificado no existe",
                Code = "INVALID_EQUIPMENT"
            });
        }

        // Validate parent exercise reference
        if (request.ParentExerciseId.HasValue)
        {
            var parentExists = await _context.Exercises.AnyAsync(e => e.Id == request.ParentExerciseId.Value);
            if (!parentExists)
            {
                result.IsValid = false;
                result.HasInvalidReferences = true;
                result.Errors.Add(new ValidationError
                {
                    Field = nameof(request.ParentExerciseId),
                    Message = "El ejercicio padre especificado no existe",
                    Code = "INVALID_PARENT"
                });
            }
        }

        // Validate secondary muscle groups
        if (request.SecondaryMuscleGroupIds.Any())
        {
            var validMuscleGroups = await _context.MuscleGroups
                .Where(mg => request.SecondaryMuscleGroupIds.Contains(mg.Id))
                .CountAsync();

            if (validMuscleGroups != request.SecondaryMuscleGroupIds.Count)
            {
                result.IsValid = false;
                result.HasInvalidReferences = true;
                result.Errors.Add(new ValidationError
                {
                    Field = nameof(request.SecondaryMuscleGroupIds),
                    Message = "Uno o más grupos musculares secundarios no son válidos",
                    Code = "INVALID_SECONDARY_MUSCLES"
                });
            }

            // Check if primary muscle is also in secondary muscles
            if (request.SecondaryMuscleGroupIds.Contains(request.PrimaryMuscleGroupId))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = nameof(request.SecondaryMuscleGroupIds),
                    Message = "El grupo muscular principal también está en los músculos secundarios",
                    Suggestion = "Remover el grupo muscular principal de los músculos secundarios"
                });
            }
        }

        // Validate duration for isometric exercises
        if (request.ExerciseType == Core.Enums.ExerciseType.Isometric && !request.DurationSeconds.HasValue)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = nameof(request.DurationSeconds),
                Message = "Los ejercicios isométricos deberían tener una duración especificada",
                Suggestion = "Agregar duración en segundos para ejercicios isométricos"
            });
        }

        return result;
    }

    public async Task<ExerciseDeletionCheck> CheckDeletionAsync(int exerciseId)
    {
        var result = new ExerciseDeletionCheck { CanDelete = true };

        var exercise = await _context.Exercises
            .Include(e => e.ChildExercises)
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        if (exercise == null)
        {
            result.CanDelete = false;
            result.Dependencies.Add("Exercise not found");
            return result;
        }

        // Check for child exercises
        result.ChildExercisesCount = exercise.ChildExercises.Count;
        if (result.ChildExercisesCount > 0)
        {
            result.CanDelete = false;
            result.Dependencies.Add($"Exercise has {result.ChildExercisesCount} child exercise(s)");
        }

        // Check for routine usage (placeholder - would need routine implementation)
        // result.RoutineUsageCount = await _context.RoutineExercises.CountAsync(re => re.ExerciseId == exerciseId);

        // Check for images
        result.HasImages = exercise.Images.Any();
        if (result.HasImages)
        {
            result.Warnings.Add($"Exercise has {exercise.Images.Count} image(s) that will be deleted");
        }

        return result;
    }

    public async Task<BulkOperationResult> ExecuteBulkOperationAsync(BulkExerciseOperation operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new BulkOperationResult
        {
            TotalItems = operation.ExerciseIds.Count
        };

        try
        {
            switch (operation.Operation)
            {
                case BulkOperationType.Activate:
                    await BulkActivateAsync(operation.ExerciseIds, result);
                    break;
                case BulkOperationType.Deactivate:
                    await BulkDeactivateAsync(operation.ExerciseIds, result);
                    break;
                case BulkOperationType.Delete:
                    await BulkDeleteAsync(operation.ExerciseIds, result);
                    break;
                case BulkOperationType.ChangeDifficulty:
                    if (operation.Parameters.TryGetValue("difficulty", out var difficultyObj) &&
                        difficultyObj is Core.Enums.DifficultyLevel difficulty)
                    {
                        await BulkChangeDifficultyAsync(operation.ExerciseIds, difficulty, result);
                    }
                    else
                    {
                        result.Errors.Add("Difficulty parameter is required for this operation");
                    }
                    break;
                default:
                    result.Errors.Add($"Operation {operation.Operation} not implemented");
                    break;
            }

            result.Success = result.FailedItems == 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Bulk operation failed: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<ExerciseManagementResult> DuplicateExerciseAsync(int exerciseId, string newName, string newSpanishName)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var originalExercise = await GetExerciseByIdAsync(exerciseId);
            if (originalExercise == null)
            {
                result.Errors.Add($"Exercise with ID {exerciseId} not found");
                return result;
            }

            var duplicateRequest = new ExerciseCreateRequest
            {
                Name = newName,
                SpanishName = newSpanishName,
                Description = $"{originalExercise.Description} (Copia)",
                Instructions = originalExercise.Instructions,
                PrimaryMuscleGroupId = originalExercise.PrimaryMuscleGroupId,
                EquipmentTypeId = originalExercise.EquipmentTypeId,
                DifficultyLevel = originalExercise.DifficultyLevel,
                ExerciseType = originalExercise.ExerciseType,
                DurationSeconds = originalExercise.DurationSeconds,
                IsActive = false, // Start as inactive
                SecondaryMuscleGroupIds = originalExercise.SecondaryMuscles.Select(sm => sm.MuscleGroupId).ToList()
            };

            result = await CreateExerciseAsync(duplicateRequest);
            if (result.Success)
            {
                result.Message = $"Ejercicio duplicado exitosamente como '{newSpanishName}'";
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error duplicating exercise: {ex.Message}");
        }

        return result;
    }

    public async Task<BulkOperationResult> ImportExercisesAsync(List<ExerciseCreateRequest> exercises)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new BulkOperationResult
        {
            TotalItems = exercises.Count
        };

        foreach (var exercise in exercises)
        {
            try
            {
                var createResult = await CreateExerciseAsync(exercise);
                if (createResult.Success)
                {
                    result.SuccessfulItems++;
                    result.ItemResults[createResult.ExerciseId ?? 0] = "Created successfully";
                }
                else
                {
                    result.FailedItems++;
                    result.Errors.AddRange(createResult.Errors);
                    result.ItemResults[0] = $"Failed: {string.Join(", ", createResult.Errors)}";
                }
            }
            catch (Exception ex)
            {
                result.FailedItems++;
                result.Errors.Add($"Error importing exercise '{exercise.SpanishName}': {ex.Message}");
            }
        }

        result.Success = result.FailedItems == 0;
        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;

        return result;
    }

    // Image Management Methods
    public async Task<ExerciseManagementResult> AddExerciseImageAsync(int exerciseId, ExerciseImageUpload image)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId);
            if (exercise == null)
            {
                result.Errors.Add($"Exercise with ID {exerciseId} not found");
                return result;
            }

            // Save image using image service
            var imagePath = await _imageService.SaveImageFromBytesAsync(
                image.ImageData,
                exercise.SpanishName,
                image.Position,
                Path.GetExtension(image.FileName).TrimStart('.'));

            // Create image entity
            var exerciseImage = new ExerciseImage
            {
                ExerciseId = exerciseId,
                ImagePath = imagePath,
                ImagePosition = image.Position,
                IsPrimary = image.IsPrimary,
                Description = image.Description
            };

            _context.ExerciseImages.Add(exerciseImage);
            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = $"Imagen agregada exitosamente al ejercicio '{exercise.SpanishName}'";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error adding image: {ex.Message}");
        }

        return result;
    }

    public async Task<ExerciseManagementResult> UpdateExerciseImageAsync(int imageId, ExerciseImageUpload image)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var exerciseImage = await _context.ExerciseImages
                .Include(ei => ei.Exercise)
                .FirstOrDefaultAsync(ei => ei.Id == imageId);

            if (exerciseImage == null)
            {
                result.Errors.Add($"Image with ID {imageId} not found");
                return result;
            }

            // Update image if new data provided
            if (image.ImageData.Length > 0)
            {
                // Delete old image file
                if (File.Exists(exerciseImage.ImagePath))
                {
                    File.Delete(exerciseImage.ImagePath);
                }

                // Save new image
                var newImagePath = await _imageService.SaveImageFromBytesAsync(
                    image.ImageData,
                    exerciseImage.Exercise.SpanishName,
                    image.Position,
                    Path.GetExtension(image.FileName).TrimStart('.'));

                exerciseImage.ImagePath = newImagePath;
            }

            // Update metadata
            exerciseImage.ImagePosition = image.Position;
            exerciseImage.IsPrimary = image.IsPrimary;
            exerciseImage.Description = image.Description;

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = "Imagen actualizada exitosamente";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error updating image: {ex.Message}");
        }

        return result;
    }

    public async Task<ExerciseManagementResult> DeleteExerciseImageAsync(int imageId)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var exerciseImage = await _context.ExerciseImages.FirstOrDefaultAsync(ei => ei.Id == imageId);
            if (exerciseImage == null)
            {
                result.Errors.Add($"Image with ID {imageId} not found");
                return result;
            }

            // Delete image file
            if (File.Exists(exerciseImage.ImagePath))
            {
                File.Delete(exerciseImage.ImagePath);
            }

            _context.ExerciseImages.Remove(exerciseImage);
            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = "Imagen eliminada exitosamente";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error deleting image: {ex.Message}");
        }

        return result;
    }

    public async Task<List<ExerciseImage>> GetExerciseImagesAsync(int exerciseId)
    {
        return await _context.ExerciseImages
            .Where(ei => ei.ExerciseId == exerciseId)
            .OrderBy(ei => ei.ImagePosition)
            .ThenBy(ei => ei.Id)
            .ToListAsync();
    }

    // Relationship Management
    public async Task<ExerciseManagementResult> SetParentExerciseAsync(int exerciseId, int? parentExerciseId)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId);
            if (exercise == null)
            {
                result.Errors.Add($"Exercise with ID {exerciseId} not found");
                return result;
            }

            if (parentExerciseId.HasValue)
            {
                var parentExists = await _context.Exercises.AnyAsync(e => e.Id == parentExerciseId.Value);
                if (!parentExists)
                {
                    result.Errors.Add($"Parent exercise with ID {parentExerciseId.Value} not found");
                    return result;
                }

                // Check for circular reference
                if (await HasCircularReferenceAsync(exerciseId, parentExerciseId.Value))
                {
                    result.Errors.Add("Setting this parent would create a circular reference");
                    return result;
                }
            }

            exercise.ParentExerciseId = parentExerciseId;
            exercise.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = parentExerciseId.HasValue ? "Parent exercise set successfully" : "Parent exercise removed successfully";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error setting parent exercise: {ex.Message}");
        }

        return result;
    }

    public async Task<ExerciseManagementResult> AddSecondaryMuscleAsync(int exerciseId, int muscleGroupId)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId);
            if (exercise == null)
            {
                result.Errors.Add($"Exercise with ID {exerciseId} not found");
                return result;
            }

            var muscleGroupExists = await _context.MuscleGroups.AnyAsync(mg => mg.Id == muscleGroupId);
            if (!muscleGroupExists)
            {
                result.Errors.Add($"Muscle group with ID {muscleGroupId} not found");
                return result;
            }

            // Check if already exists
            var alreadyExists = await _context.ExerciseSecondaryMuscles
                .AnyAsync(esm => esm.ExerciseId == exerciseId && esm.MuscleGroupId == muscleGroupId);

            if (alreadyExists)
            {
                result.Warnings.Add("Secondary muscle already exists for this exercise");
                result.Success = true;
                return result;
            }

            var secondaryMuscle = new ExerciseSecondaryMuscle
            {
                ExerciseId = exerciseId,
                MuscleGroupId = muscleGroupId
            };

            _context.ExerciseSecondaryMuscles.Add(secondaryMuscle);
            exercise.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = "Secondary muscle added successfully";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error adding secondary muscle: {ex.Message}");
        }

        return result;
    }

    public async Task<ExerciseManagementResult> RemoveSecondaryMuscleAsync(int exerciseId, int muscleGroupId)
    {
        var result = new ExerciseManagementResult();

        try
        {
            var secondaryMuscle = await _context.ExerciseSecondaryMuscles
                .FirstOrDefaultAsync(esm => esm.ExerciseId == exerciseId && esm.MuscleGroupId == muscleGroupId);

            if (secondaryMuscle == null)
            {
                result.Warnings.Add("Secondary muscle not found for this exercise");
                result.Success = true;
                return result;
            }

            _context.ExerciseSecondaryMuscles.Remove(secondaryMuscle);

            var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId);
            if (exercise != null)
            {
                exercise.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Message = "Secondary muscle removed successfully";
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error removing secondary muscle: {ex.Message}");
        }

        return result;
    }

    // Statistics and Reporting
    public async Task<ExerciseManagementSummary> GetManagementSummaryAsync()
    {
        var summary = new ExerciseManagementSummary();

        summary.TotalExercises = await _context.Exercises.CountAsync();
        summary.ActiveExercises = await _context.Exercises.CountAsync(e => e.IsActive);
        summary.InactiveExercises = summary.TotalExercises - summary.ActiveExercises;

        summary.ByMuscleGroup = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .GroupBy(e => e.PrimaryMuscleGroup.SpanishName)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        summary.ByEquipment = await _context.Exercises
            .Include(e => e.EquipmentType)
            .GroupBy(e => e.EquipmentType.SpanishName)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        summary.ByDifficulty = await _context.Exercises
            .GroupBy(e => e.DifficultyLevel)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        summary.ByType = await _context.Exercises
            .GroupBy(e => e.ExerciseType)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        summary.WithImages = await _context.Exercises
            .Where(e => e.Images.Any())
            .CountAsync();

        summary.WithoutImages = summary.TotalExercises - summary.WithImages;

        summary.WithParentExercises = await _context.Exercises
            .CountAsync(e => e.ParentExerciseId.HasValue);

        summary.WithChildExercises = await _context.Exercises
            .CountAsync(e => e.ChildExercises.Any());

        summary.LastUpdated = DateTime.UtcNow;

        return summary;
    }

    public async Task<List<Exercise>> GetExercisesNeedingImagesAsync()
    {
        return await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Where(e => e.IsActive && !e.Images.Any())
            .OrderBy(e => e.SpanishName)
            .ToListAsync();
    }

    public async Task<List<Exercise>> GetExercisesWithIssuesAsync()
    {
        return await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Where(e => e.Description.Length < 50 ||
                       e.Instructions.Length < 100 ||
                       (e.ExerciseType == Core.Enums.ExerciseType.Isometric && !e.DurationSeconds.HasValue))
            .OrderBy(e => e.SpanishName)
            .ToListAsync();
    }

    public async Task<List<Exercise>> GetRecentlyModifiedExercisesAsync(int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        return await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Where(e => e.UpdatedAt >= cutoffDate || e.CreatedAt >= cutoffDate)
            .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
            .ToListAsync();
    }

    // Lookup Data
    public async Task<List<MuscleGroup>> GetMuscleGroupsAsync()
    {
        return await _context.MuscleGroups
            .OrderBy(mg => mg.SpanishName)
            .ToListAsync();
    }

    public async Task<List<EquipmentType>> GetEquipmentTypesAsync()
    {
        return await _context.EquipmentTypes
            .OrderBy(et => et.SpanishName)
            .ToListAsync();
    }

    // Private helper methods
    private async Task<List<ExerciseManagementResult>> ProcessExerciseImagesAsync(int exerciseId, List<ExerciseImageUpload> images)
    {
        var results = new List<ExerciseManagementResult>();

        foreach (var image in images)
        {
            var result = await AddExerciseImageAsync(exerciseId, image);
            results.Add(result);
        }

        return results;
    }

    private async Task BulkActivateAsync(List<int> exerciseIds, BulkOperationResult result)
    {
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync();

        foreach (var exercise in exercises)
        {
            exercise.IsActive = true;
            exercise.UpdatedAt = DateTime.UtcNow;
            result.SuccessfulItems++;
            result.ItemResults[exercise.Id] = "Activated";
        }

        await _context.SaveChangesAsync();
    }

    private async Task BulkDeactivateAsync(List<int> exerciseIds, BulkOperationResult result)
    {
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync();

        foreach (var exercise in exercises)
        {
            exercise.IsActive = false;
            exercise.UpdatedAt = DateTime.UtcNow;
            result.SuccessfulItems++;
            result.ItemResults[exercise.Id] = "Deactivated";
        }

        await _context.SaveChangesAsync();
    }

    private async Task BulkDeleteAsync(List<int> exerciseIds, BulkOperationResult result)
    {
        foreach (var exerciseId in exerciseIds)
        {
            try
            {
                var deleteResult = await DeleteExerciseAsync(exerciseId, true);
                if (deleteResult.Success)
                {
                    result.SuccessfulItems++;
                    result.ItemResults[exerciseId] = "Deleted";
                }
                else
                {
                    result.FailedItems++;
                    result.ItemResults[exerciseId] = $"Failed: {string.Join(", ", deleteResult.Errors)}";
                    result.Errors.AddRange(deleteResult.Errors);
                }
            }
            catch (Exception ex)
            {
                result.FailedItems++;
                result.ItemResults[exerciseId] = $"Error: {ex.Message}";
                result.Errors.Add($"Error deleting exercise {exerciseId}: {ex.Message}");
            }
        }
    }

    private async Task BulkChangeDifficultyAsync(List<int> exerciseIds, Core.Enums.DifficultyLevel difficulty, BulkOperationResult result)
    {
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync();

        foreach (var exercise in exercises)
        {
            exercise.DifficultyLevel = difficulty;
            exercise.UpdatedAt = DateTime.UtcNow;
            result.SuccessfulItems++;
            result.ItemResults[exercise.Id] = $"Difficulty changed to {difficulty}";
        }

        await _context.SaveChangesAsync();
    }

    private async Task<bool> HasCircularReferenceAsync(int exerciseId, int potentialParentId)
    {
        var currentId = potentialParentId;
        var visited = new HashSet<int>();

        while (currentId != 0)
        {
            if (currentId == exerciseId)
                return true;

            if (visited.Contains(currentId))
                return true; // Infinite loop detected

            visited.Add(currentId);

            var parent = await _context.Exercises
                .Where(e => e.Id == currentId)
                .Select(e => e.ParentExerciseId)
                .FirstOrDefaultAsync();

            currentId = parent ?? 0;
        }

        return false;
    }
}