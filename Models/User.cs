using System.ComponentModel.DataAnnotations;

namespace ST10449392_CLDV6212_POE.Models
{
    public class User 
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

    }
}
