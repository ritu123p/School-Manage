using Microsoft.AspNetCore.Identity;

namespace schoool_Management.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? JwtToken { get; set; }
        public DateTime JwtTokenExpiry { get; set; }

        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}