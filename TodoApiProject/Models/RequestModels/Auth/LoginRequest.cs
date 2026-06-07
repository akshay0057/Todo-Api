using System.ComponentModel.DataAnnotations;

namespace TodoApiProject.Models.RequestModels.Auth
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
    }
}
