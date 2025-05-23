using System.ComponentModel.DataAnnotations;

namespace schoool_Management.Models
{
    public class TeacherModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public int Experience { get; set; }

        [Required]
        public string IdentityUserId { get; set; }

        public ICollection<CourseModel>? Courses { get; set; }
    }
}
