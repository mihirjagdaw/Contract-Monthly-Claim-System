using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10449392_CLDV6212_POE.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductRowKey { get; set; }  

        public int Quantity { get; set; }

        [NotMapped]
        public Product Product { get; set; }

        public virtual Order Order { get; set; }
    }

}
