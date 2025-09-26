using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymRoutineGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedExerciseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "MuscleGroup",
                table: "Exercises",
                newName: "PrimaryMuscleGroupId");

            migrationBuilder.RenameColumn(
                name: "Equipment",
                table: "Exercises",
                newName: "IsActive");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Exercises",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Exercises",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DifficultyLevel",
                table: "Exercises",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Exercises",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EquipmentTypeId",
                table: "Exercises",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExerciseType",
                table: "Exercises",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Exercises",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParentExerciseId",
                table: "Exercises",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpanishName",
                table: "Exercises",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Exercises",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EquipmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SpanishName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ImagePosition = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseImages_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuscleGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SpanishName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSecondaryMuscles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    MuscleGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSecondaryMuscles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseSecondaryMuscles_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseSecondaryMuscles_MuscleGroups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "MuscleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_DifficultyLevel",
                table: "Exercises",
                column: "DifficultyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_EquipmentTypeId",
                table: "Exercises",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ParentExerciseId",
                table: "Exercises",
                column: "ParentExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_PrimaryMuscleGroupId",
                table: "Exercises",
                column: "PrimaryMuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_SpanishName",
                table: "Exercises",
                column: "SpanishName");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseImages_ExerciseId",
                table: "ExerciseImages",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSecondaryMuscles_ExerciseId_MuscleGroupId",
                table: "ExerciseSecondaryMuscles",
                columns: new[] { "ExerciseId", "MuscleGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSecondaryMuscles_MuscleGroupId",
                table: "ExerciseSecondaryMuscles",
                column: "MuscleGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_EquipmentTypes_EquipmentTypeId",
                table: "Exercises",
                column: "EquipmentTypeId",
                principalTable: "EquipmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Exercises_ParentExerciseId",
                table: "Exercises",
                column: "ParentExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_MuscleGroups_PrimaryMuscleGroupId",
                table: "Exercises",
                column: "PrimaryMuscleGroupId",
                principalTable: "MuscleGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_EquipmentTypes_EquipmentTypeId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Exercises_ParentExerciseId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_MuscleGroups_PrimaryMuscleGroupId",
                table: "Exercises");

            migrationBuilder.DropTable(
                name: "EquipmentTypes");

            migrationBuilder.DropTable(
                name: "ExerciseImages");

            migrationBuilder.DropTable(
                name: "ExerciseSecondaryMuscles");

            migrationBuilder.DropTable(
                name: "MuscleGroups");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_DifficultyLevel",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_EquipmentTypeId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Name",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_ParentExerciseId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_PrimaryMuscleGroupId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_SpanishName",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "EquipmentTypeId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ExerciseType",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ParentExerciseId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SpanishName",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "PrimaryMuscleGroupId",
                table: "Exercises",
                newName: "MuscleGroup");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Exercises",
                newName: "Equipment");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Exercises",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
