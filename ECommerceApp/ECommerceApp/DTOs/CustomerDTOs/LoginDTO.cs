using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.CustomerDTOs
{
    // DTO for customer login
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; }
    }

    public class LoginAuthenticationDTO
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public List<string> Roles { get; set; }
    }



}