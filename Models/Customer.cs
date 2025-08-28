using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ST10449392_CLDV6212_POE.Models
{
    public class Customer : ITableEntity
    {
        [Key]   
        public int Customer_Id { get; set; }

        public string? Customer_Name { get; set; }

        public string? email { get; set; }

        public string? password { get; set; }

        //Implementation of ITableEntity properties
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
