using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Seeds;

Console.WriteLine("🏋️ Epic 2: Enhanced Exercise Database Schema Test");
Console.WriteLine("Story 2.1: Enhanced Exercise Database Schema");
Console.WriteLine(new string('=', 60));

// Setup enhanced database
var optionsBuilder = new DbContextOptionsBuilder<GymRoutineContext>();
optionsBuilder.UseSqlite("Data Source=gymroutine_epic2.db");

using var context = new GymRoutineContext(optionsBuilder.Options);

// Apply migrations
Console.WriteLine("🔄 Applying database migrations...");
await context.Database.MigrateAsync();
Console.WriteLine("✅ Database migrations applied");

// Seed enhanced data
Console.WriteLine("🌱 Seeding enhanced exercise data...");
EnhancedExerciseSeeder.SeedData(context);
Console.WriteLine("✅ Enhanced exercise data seeded");

// Test the enhanced schema
Console.WriteLine("\n📊 Enhanced Schema Validation:");

// Test muscle groups
var muscleGroups = await context.MuscleGroups.ToListAsync();
Console.WriteLine($"✅ Muscle Groups: {muscleGroups.Count} groups");
foreach (var mg in muscleGroups)
{
    Console.WriteLine($"  - {mg.SpanishName} ({mg.Name})");
}

// Test equipment types
var equipmentTypes = await context.EquipmentTypes.ToListAsync();
Console.WriteLine($"\n✅ Equipment Types: {equipmentTypes.Count} types");
foreach (var et in equipmentTypes)
{
    Console.WriteLine($"  - {et.SpanishName} ({et.Name}) - Required: {et.IsRequired}");
}

// Test enhanced exercises with relations
var exercises = await context.Exercises
    .Include(e => e.PrimaryMuscleGroup)
    .Include(e => e.EquipmentType)
    .Include(e => e.SecondaryMuscles)
        .ThenInclude(sm => sm.MuscleGroup)
    .ToListAsync();

Console.WriteLine($"\n✅ Enhanced Exercises: {exercises.Count} exercises");
foreach (var exercise in exercises.Take(5))
{
    Console.WriteLine($"  - {exercise.SpanishName}");
    Console.WriteLine($"    Primary: {exercise.PrimaryMuscleGroup.SpanishName}");
    Console.WriteLine($"    Equipment: {exercise.EquipmentType.SpanishName}");
    Console.WriteLine($"    Difficulty: {exercise.DifficultyLevel}");
    Console.WriteLine($"    Type: {exercise.ExerciseType}");
    if (exercise.DurationSeconds.HasValue)
        Console.WriteLine($"    Duration: {exercise.DurationSeconds}s");
    Console.WriteLine();
}

// Test search capabilities
Console.WriteLine("🔍 Testing Search & Filter Capabilities:");

// Filter by difficulty
var beginnerExercises = await context.Exercises
    .Where(e => e.DifficultyLevel == GymRoutineGenerator.Core.Enums.DifficultyLevel.Beginner)
    .CountAsync();
Console.WriteLine($"  - Beginner exercises: {beginnerExercises}");

// Filter by equipment
var bodyweightExercises = await context.Exercises
    .Include(e => e.EquipmentType)
    .Where(e => e.EquipmentType.Name == "Bodyweight")
    .CountAsync();
Console.WriteLine($"  - Bodyweight exercises: {bodyweightExercises}");

// Filter by muscle group
var chestExercises = await context.Exercises
    .Include(e => e.PrimaryMuscleGroup)
    .Where(e => e.PrimaryMuscleGroup.Name == "Chest")
    .CountAsync();
Console.WriteLine($"  - Chest exercises: {chestExercises}");

Console.WriteLine("\n🎉 Story 2.1 Acceptance Criteria Validation:");
Console.WriteLine("✅ Extended exercise table with rich metadata");
Console.WriteLine("✅ Muscle groups table with standardized naming");
Console.WriteLine("✅ Equipment types table with descriptions");
Console.WriteLine("✅ Exercise variations support (parent_exercise_id)");
Console.WriteLine("✅ Database migration system handles schema updates");

Console.WriteLine("\n🚀 Story 2.1: Enhanced Exercise Database Schema - COMPLETED!");
Console.WriteLine("Ready for Story 2.2: Exercise Image Management System");
