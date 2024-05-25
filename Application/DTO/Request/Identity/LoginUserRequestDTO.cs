using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Request.Identity
{
    public class LoginUserRequestDTO
    {
        [EmailAddress]
        [RegularExpression("[^@ \\t\\r\\n]+@[^@ \\t\\r\\n]+\\.[^@ \\t\\r\\n]+", ErrorMessage = "Your Email Is Not Valid")]
        public string Email { get; set; }
        [Required]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$", ErrorMessage ="Your Password Is Not Strong")]
        [MinLength(8), MaxLength(15)]
        public string Password { get; set; }
    }
}
