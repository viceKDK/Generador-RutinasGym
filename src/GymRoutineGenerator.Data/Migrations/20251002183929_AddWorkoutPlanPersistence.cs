using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymRoutineGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutPlanPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEquipmentPreferences_EquipmentTypes_EquipmentTypeId",
                table: "UserEquipmentPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEquipmentPreferences_UserProfiles_UserProfileId",
                table: "UserEquipmentPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMuscleGroupPreferences_UserProfiles_UserProfileId",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhysicalLimitations_UserProfiles_UserProfileId",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropIndex(
                name: "IX_UserPhysicalLimitations_UserProfileId",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropIndex(
                name: "IX_UserMuscleGroupPreferences_UserProfileId_MuscleGroupId",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserEquipmentPreferences_UserProfileId_EquipmentTypeId",
                table: "UserEquipmentPreferences");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "UserProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExperienceLevel",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FitnessLevel",
                table: "UserProfiles",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Goals",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "InjuryHistory",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PhysicalLimitationsList",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "TrainingDays",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "LimitationType",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomRestrictions",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AffectedBodyParts",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateReported",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExercisesToAvoid",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExercisesToAvoidList",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserPhysicalLimitations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RestrictedMovements",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "Severity",
                table: "UserPhysicalLimitations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SeverityLevel",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserPhysicalLimitations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRestricted",
                table: "UserMuscleGroupPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "UserMuscleGroupPreferences",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "UserMuscleGroupPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserMuscleGroupPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentType",
                table: "UserEquipmentPreferences",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "UserEquipmentPreferences",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserEquipmentPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserRoutines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RoutineData = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    RoutineType = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Goals = table.Column<string>(type: "TEXT", nullable: false),
                    UserIdString = table.Column<string>(type: "TEXT", nullable: false),
                    RoutineContent = table.Column<string>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", nullable: false),
                    FitnessLevel = table.Column<string>(type: "TEXT", nullable: false),
                    TrainingDays = table.Column<int>(type: "INTEGER", nullable: false),
                    TrainingDaysPerWeek = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoutines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoutines_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    UserAge = table.Column<int>(type: "INTEGER", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UserLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    UserLevelName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TrainingDaysPerWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserLimitationsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoutineExercise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserRoutineId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExerciseName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DayNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrderInDay = table.Column<int>(type: "INTEGER", nullable: false),
                    SetsAndReps = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ImageInfo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsCustomExercise = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineExercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutineExercise_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoutineExercise_UserRoutines_UserRoutineId",
                        column: x => x.UserRoutineId,
                        principalTable: "UserRoutines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutineModifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserRoutineId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificationType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OriginalValue = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UserRequest = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    WasApplied = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineModifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutineModifications_UserRoutines_UserRoutineId",
                        column: x => x.UserRoutineId,
                        principalTable: "UserRoutines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlanRoutines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DayNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlanRoutines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutPlanRoutines_WorkoutPlans_WorkoutPlanId",
                        column: x => x.WorkoutPlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlanRoutineExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutPlanRoutineId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExerciseName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SetsJson = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlanRoutineExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutPlanRoutineExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutPlanRoutineExercises_WorkoutPlanRoutines_WorkoutPlanRoutineId",
                        column: x => x.WorkoutPlanRoutineId,
                        principalTable: "WorkoutPlanRoutines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPhysicalLimitations_UserId",
                table: "UserPhysicalLimitations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMuscleGroupPreferences_UserId_MuscleGroupId",
                table: "UserMuscleGroupPreferences",
                columns: new[] { "UserId", "MuscleGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipmentPreferences_UserId",
                table: "UserEquipmentPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineExercise_ExerciseId",
                table: "RoutineExercise",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineExercise_UserRoutineId",
                table: "RoutineExercise",
                column: "UserRoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineModifications_ModifiedAt",
                table: "RoutineModifications",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineModifications_UserRoutineId",
                table: "RoutineModifications",
                column: "UserRoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutines_CreatedAt",
                table: "UserRoutines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutines_UserId",
                table: "UserRoutines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanRoutineExercises_ExerciseId",
                table: "WorkoutPlanRoutineExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanRoutineExercises_WorkoutPlanRoutineId_Order",
                table: "WorkoutPlanRoutineExercises",
                columns: new[] { "WorkoutPlanRoutineId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanRoutines_WorkoutPlanId_DayNumber",
                table: "WorkoutPlanRoutines",
                columns: new[] { "WorkoutPlanId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_CreatedAt",
                table: "WorkoutPlans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_UserName",
                table: "WorkoutPlans",
                column: "UserName");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEquipmentPreferences_EquipmentTypes_EquipmentTypeId",
                table: "UserEquipmentPreferences",
                column: "EquipmentTypeId",
                principalTable: "EquipmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEquipmentPreferences_UserProfiles_UserId",
                table: "UserEquipmentPreferences",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMuscleGroupPreferences_UserProfiles_UserId",
                table: "UserMuscleGroupPreferences",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhysicalLimitations_UserProfiles_UserId",
                table: "UserPhysicalLimitations",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEquipmentPreferences_EquipmentTypes_EquipmentTypeId",
                table: "UserEquipmentPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEquipmentPreferences_UserProfiles_UserId",
                table: "UserEquipmentPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMuscleGroupPreferences_UserProfiles_UserId",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhysicalLimitations_UserProfiles_UserId",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropTable(
                name: "RoutineExercise");

            migrationBuilder.DropTable(
                name: "RoutineModifications");

            migrationBuilder.DropTable(
                name: "WorkoutPlanRoutineExercises");

            migrationBuilder.DropTable(
                name: "UserRoutines");

            migrationBuilder.DropTable(
                name: "WorkoutPlanRoutines");

            migrationBuilder.DropTable(
                name: "WorkoutPlans");

            migrationBuilder.DropIndex(
                name: "IX_UserPhysicalLimitations_UserId",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropIndex(
                name: "IX_UserMuscleGroupPreferences_UserId_MuscleGroupId",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserEquipmentPreferences_UserId",
                table: "UserEquipmentPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "FitnessLevel",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Goals",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InjuryHistory",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PhysicalLimitationsList",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "TrainingDays",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "AffectedBodyParts",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "DateReported",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "ExercisesToAvoid",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "ExercisesToAvoidList",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "RestrictedMovements",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "SeverityLevel",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserPhysicalLimitations");

            migrationBuilder.DropColumn(
                name: "IsRestricted",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserMuscleGroupPreferences");

            migrationBuilder.DropColumn(
                name: "EquipmentType",
                table: "UserEquipmentPreferences");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "UserEquipmentPreferences");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserEquipmentPreferences");

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "LimitationType",
                table: "UserPhysicalLimitations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "CustomRestrictions",
                table: "UserPhysicalLimitations",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_UserPhysicalLimitations_UserProfileId",
                table: "UserPhysicalLimitations",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMuscleGroupPreferences_UserProfileId_MuscleGroupId",
                table: "UserMuscleGroupPreferences",
                columns: new[] { "UserProfileId", "MuscleGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipmentPreferences_UserProfileId_EquipmentTypeId",
                table: "UserEquipmentPreferences",
                columns: new[] { "UserProfileId", "EquipmentTypeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEquipmentPreferences_EquipmentTypes_EquipmentTypeId",
                table: "UserEquipmentPreferences",
                column: "EquipmentTypeId",
                principalTable: "EquipmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEquipmentPreferences_UserProfiles_UserProfileId",
                table: "UserEquipmentPreferences",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMuscleGroupPreferences_UserProfiles_UserProfileId",
                table: "UserMuscleGroupPreferences",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhysicalLimitations_UserProfiles_UserProfileId",
                table: "UserPhysicalLimitations",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
