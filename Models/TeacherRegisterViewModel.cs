using System.ComponentModel.DataAnnotations;

namespace schoool_Management.Models.ViewModels
{
    public class TeacherRegisterViewModel
    {
        [Required(ErrorMessage ="Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage ="Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Experience is required")]
        public int Experience { get; set; }

        [Required(ErrorMessage = "Password is required"), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required"), DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
