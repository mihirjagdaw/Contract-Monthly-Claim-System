using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ST10449392_CLDV6212_POE.Models
{
    public class InventoryManagement : ITableEntity
    {
        [Key]
        public int Inventory_Id { get; set; }

        public string? PartitionKey { get; set; }

        public string? RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        // Introduce validation sample

        [Required(ErrorMessage = "Please select a customer")]
        public int Customer_Id { get; set; } //FK to Customer who purchased the product

        [Required(ErrorMessage = "Please select a product")]
        public int Product_Id { get; set; } //FK to Product that was purchased

        [Required(ErrorMessage = "Please select the date of purchase")]
        public DateTime Purchase_Date { get; set; }

    }
}
