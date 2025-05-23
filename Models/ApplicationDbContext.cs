using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using schoool_Management.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TeacherModel> Teachers { get; set; }
    public DbSet<StudentModel> Students { get; set; }
    public DbSet<CourseModel> Courses { get; set; }
    public DbSet<CourseEnroll> CourseEnrolls { get; set; }
}