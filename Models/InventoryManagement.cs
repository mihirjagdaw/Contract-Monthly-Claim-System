using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ST10449392_CLDV6212_POE.Models
{
    public class InventoryManagement : ITableEntity
    {
        
        public string PartitionKey { get; set; } = "Purchase";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Required]
        public string Customer_Id { get; set; } = string.Empty;

        [Required]
        public string Product_Id { get; set; } = string.Empty;

        [Required]
        public DateTime Purchase_Date { get; set; }

    }
}
