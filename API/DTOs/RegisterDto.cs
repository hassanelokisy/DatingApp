using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public String Username { get; set; }
        [Required]
        public string Password { get; set; }

    }
}