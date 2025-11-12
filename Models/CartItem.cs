using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10449392_CLDV6212_POE.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public virtual User User { get; set; }

        public virtual Product Product { get; set; }
    }
}
