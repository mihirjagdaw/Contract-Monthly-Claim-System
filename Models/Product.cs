using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ST10449392_CLDV6212_POE.Models
{
    public class Product : ITableEntity
    {
        [Key]
        public int Product_Id { get; set; }

        public string? Product_Name { get; set; }

        public string? Price { get; set; }

        public string? ImageUrl { get; set; }

        // Implementation of ITableEntity properties
        public string? PartitionKey { get; set; }

        public string? RowKey { get; set; }

        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
