using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Context;

public class GymRoutineContext : DbContext
{
    public GymRoutineContext(DbContextOptions<GymRoutineContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<MuscleGroup> MuscleGroups { get; set; }
    public DbSet<EquipmentType> EquipmentTypes { get; set; }
    public DbSet<ExerciseImage> ExerciseImages { get; set; }
    public DbSet<ExerciseSecondaryMuscle> ExerciseSecondaryMuscles { get; set; }

    // User Profile entities
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserEquipmentPreference> UserEquipmentPreferences { get; set; }
    public DbSet<UserMuscleGroupPreference> UserMuscleGroupPreferences { get; set; }
    public DbSet<UserPhysicalLimitation> UserPhysicalLimitations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MuscleGroup entity
        modelBuilder.Entity<MuscleGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SpanishName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // EquipmentType entity
        modelBuilder.Entity<EquipmentType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SpanishName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Exercise entity
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SpanishName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.Property(e => e.DifficultyLevel).IsRequired();
            entity.Property(e => e.ExerciseType).IsRequired();

            // Foreign key relationships
            entity.HasOne(e => e.PrimaryMuscleGroup)
                  .WithMany(m => m.PrimaryMuscleExercises)
                  .HasForeignKey(e => e.PrimaryMuscleGroupId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EquipmentType)
                  .WithMany(et => et.Exercises)
                  .HasForeignKey(e => e.EquipmentTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship for exercise variations
            entity.HasOne(e => e.ParentExercise)
                  .WithMany(e => e.ChildExercises)
                  .HasForeignKey(e => e.ParentExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.SpanishName);
            entity.HasIndex(e => e.PrimaryMuscleGroupId);
            entity.HasIndex(e => e.EquipmentTypeId);
            entity.HasIndex(e => e.DifficultyLevel);
        });

        // ExerciseImage entity
        modelBuilder.Entity<ExerciseImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ImagePosition).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(ei => ei.Exercise)
                  .WithMany(e => e.Images)
                  .HasForeignKey(ei => ei.ExerciseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ExerciseSecondaryMuscle entity (many-to-many)
        modelBuilder.Entity<ExerciseSecondaryMuscle>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(esm => esm.Exercise)
                  .WithMany(e => e.SecondaryMuscles)
                  .HasForeignKey(esm => esm.ExerciseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(esm => esm.MuscleGroup)
                  .WithMany(m => m.SecondaryMuscleExercises)
                  .HasForeignKey(esm => esm.MuscleGroupId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint to prevent duplicate secondary muscle assignments
            entity.HasIndex(e => new { e.ExerciseId, e.MuscleGroupId }).IsUnique();
        });

        // UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Gender).IsRequired();
            entity.Property(e => e.Age).IsRequired();
            entity.Property(e => e.TrainingDaysPerWeek).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Name);
        });

        // UserEquipmentPreference entity
        modelBuilder.Entity<UserEquipmentPreference>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(uep => uep.UserProfile)
                  .WithMany(up => up.EquipmentPreferences)
                  .HasForeignKey(uep => uep.UserProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uep => uep.EquipmentType)
                  .WithMany()
                  .HasForeignKey(uep => uep.EquipmentTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserProfileId, e.EquipmentTypeId }).IsUnique();
        });

        // UserMuscleGroupPreference entity
        modelBuilder.Entity<UserMuscleGroupPreference>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(umgp => umgp.UserProfile)
                  .WithMany(up => up.MuscleGroupPreferences)
                  .HasForeignKey(umgp => umgp.UserProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(umgp => umgp.MuscleGroup)
                  .WithMany()
                  .HasForeignKey(umgp => umgp.MuscleGroupId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserProfileId, e.MuscleGroupId }).IsUnique();
        });

        // UserPhysicalLimitation entity
        modelBuilder.Entity<UserPhysicalLimitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LimitationType).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CustomRestrictions).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(upl => upl.UserProfile)
                  .WithMany(up => up.PhysicalLimitations)
                  .HasForeignKey(upl => upl.UserProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}