using System.ComponentModel.DataAnnotations;

namespace ST10449392_CLDV6212_POE.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
