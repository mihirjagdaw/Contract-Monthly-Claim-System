using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace ST10449392_CLDV6212_POE.Models
{
    public class Customer : ITableEntity
    {
        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        //Implementation of ITableEntity properties
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
