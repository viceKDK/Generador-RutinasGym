using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymRoutineGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageMetadataColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "ExerciseImages",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "ExerciseImages",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ImageMetadata",
                table: "ExerciseImages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "ExerciseImages");

            migrationBuilder.DropColumn(
                name: "ImageMetadata",
                table: "ExerciseImages");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "ExerciseImages",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
