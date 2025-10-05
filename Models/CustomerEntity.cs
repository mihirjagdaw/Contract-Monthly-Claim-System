using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace ST10449392_CLDV6212_POE.Models
{
    public class CustomerEntity : ITableEntity
    {
        // properties to be populated from JSON
        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string PartitionKey { get; set; } = "Customer";

        public string RowKey { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}