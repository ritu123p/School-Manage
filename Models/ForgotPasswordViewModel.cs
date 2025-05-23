using System.ComponentModel.DataAnnotations;

namespace schoool_Management.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "The email address is not valid.")]
        public string Email { get; set; }
    }
}