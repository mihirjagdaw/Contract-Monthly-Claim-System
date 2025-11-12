using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10449392_CLDV6212_POE.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";

        public virtual User User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }

    }
}
