using System.ComponentModel.DataAnnotations;

namespace schoool_Management.Models
{
    public class StudentRegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Roll no is required")]
        public int RollNo { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Password is required"), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required"), DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}