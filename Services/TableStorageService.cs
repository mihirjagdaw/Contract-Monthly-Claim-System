using Azure;
using Azure.Data.Tables;
using ST10449392_CLDV6212_POE.Models;

namespace ST10449392_CLDV6212_POE.Services
{
    public class TableStorageService
    {
        public readonly TableClient _CustomerTableClient; // Table client for customer operations

        public readonly TableClient _ProductTableClient; // Table client for product operations

        public readonly TableClient _inventoryManagementTableClient;

        public TableStorageService(string connectionString)
        {
            _CustomerTableClient =  new TableClient(connectionString, "Customer");
            _ProductTableClient = new TableClient(connectionString, "Products");
            _inventoryManagementTableClient = new TableClient(connectionString, "purchases");
        }
        public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _CustomerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
                return response.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                // Return null if the entity does not exist
                return null;
            }
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();

            await foreach (var customer in _CustomerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }

            return customers;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _CustomerTableClient.AddEntityAsync(customer);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _CustomerTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
           
            await _CustomerTableClient.UpdateEntityAsync(customer, Azure.ETag.All, TableUpdateMode.Replace);
        }

        // Get a single product by PartitionKey + RowKey
        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _ProductTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Update a product
        public async Task UpdateProductAsync(Product product)
        {
            await _ProductTableClient.UpdateEntityAsync(product, Azure.ETag.All, TableUpdateMode.Replace);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _ProductTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _ProductTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _ProductTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task AddInventoryManagementAsync(InventoryManagement inventoryManagement)
        {
            if (string.IsNullOrEmpty(inventoryManagement.PartitionKey) || string.IsNullOrEmpty(inventoryManagement.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _inventoryManagementTableClient.AddEntityAsync(inventoryManagement);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding inventory management to " + "table storage", ex);
            }
        }

        public async Task<List<InventoryManagement>> GetAllInventoryManagementsAsync()
        {
            var inventoryManagements = new List<InventoryManagement>();
            await foreach (var inventoryManagement in _inventoryManagementTableClient.QueryAsync<InventoryManagement>())
            {
                inventoryManagements.Add(inventoryManagement);
            }
            return inventoryManagements;
        }

    }
}
