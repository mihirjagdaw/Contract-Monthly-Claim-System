using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10449392_CLDV6212_POE.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int UserId { get; set; }

        public string ProductRowKey { get; set; } 

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        [NotMapped]
        public Product Product { get; set; }
    }

}
