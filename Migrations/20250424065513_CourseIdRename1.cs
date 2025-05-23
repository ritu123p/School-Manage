using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace schoool_Management.Migrations
{
    /// <inheritdoc />
    public partial class CourseIdRename1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Courses_CourseModelId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CourseModelId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CourseModelId",
                table: "Courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseModelId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CourseModelId",
                table: "Courses",
                column: "CourseModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Courses_CourseModelId",
                table: "Courses",
                column: "CourseModelId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
