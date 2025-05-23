using System.ComponentModel.DataAnnotations;

namespace schoool_Management.Models
{
    public class StudentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public int RollNo { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string IdentityUserId { get; set; }
    }
}
