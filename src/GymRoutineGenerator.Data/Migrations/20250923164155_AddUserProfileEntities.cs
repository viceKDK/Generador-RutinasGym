using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymRoutineGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    TrainingDaysPerWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserEquipmentPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipmentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEquipmentPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserEquipmentPreferences_EquipmentTypes_EquipmentTypeId",
                        column: x => x.EquipmentTypeId,
                        principalTable: "EquipmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserEquipmentPreferences_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMuscleGroupPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    MuscleGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmphasisLevel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMuscleGroupPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMuscleGroupPreferences_MuscleGroups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "MuscleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMuscleGroupPreferences_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPhysicalLimitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    LimitationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CustomRestrictions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPhysicalLimitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPhysicalLimitations_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipmentPreferences_EquipmentTypeId",
                table: "UserEquipmentPreferences",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEquipmentPreferences_UserProfileId_EquipmentTypeId",
                table: "UserEquipmentPreferences",
                columns: new[] { "UserProfileId", "EquipmentTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMuscleGroupPreferences_MuscleGroupId",
                table: "UserMuscleGroupPreferences",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMuscleGroupPreferences_UserProfileId_MuscleGroupId",
                table: "UserMuscleGroupPreferences",
                columns: new[] { "UserProfileId", "MuscleGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPhysicalLimitations_UserProfileId",
                table: "UserPhysicalLimitations",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Name",
                table: "UserProfiles",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEquipmentPreferences");

            migrationBuilder.DropTable(
                name: "UserMuscleGroupPreferences");

            migrationBuilder.DropTable(
                name: "UserPhysicalLimitations");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
