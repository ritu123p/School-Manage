using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace schoool_Management.Migrations
{
    /// <inheritdoc />
    public partial class CourseIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Courses_CourseModelCourseId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "CourseModelCourseId",
                table: "Courses",
                newName: "CourseModelId");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Courses",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_CourseModelCourseId",
                table: "Courses",
                newName: "IX_Courses_CourseModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Courses_CourseModelId",
                table: "Courses",
                column: "CourseModelId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Courses_CourseModelId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "CourseModelId",
                table: "Courses",
                newName: "CourseModelCourseId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Courses",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_CourseModelId",
                table: "Courses",
                newName: "IX_Courses_CourseModelCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Courses_CourseModelCourseId",
                table: "Courses",
                column: "CourseModelCourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }
    }
}
