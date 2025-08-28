using Microsoft.AspNetCore.Mvc;
using ST10449392_CLDV6212_POE.Models;
using ST10449392_CLDV6212_POE.Services;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class InventoryManagementController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public InventoryManagementController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }

        public async Task<IActionResult> Index()
        {
            var inventoryItems = await _tableStorageService.GetAllInventoryManagementsAsync();
            return View(inventoryItems);
        }

        public async Task<IActionResult> Purchase()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();

            // Check if customers or products are empty and handle accordingly
            if (customers == null || customers.Count == 0)
            {
                // Handle the case where there are no customers
                ModelState.AddModelError("", "No Customers found. Please add customers first");
                return View();
            }

            if (products == null || products.Count == 0)
            {
                // Handle the case where there are no products
                ModelState.AddModelError("", "No Products found. Please add products first");
                return View();
            }

            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(InventoryManagement inventoryManagement, string CustomerName, string ProductName)
        {
            if (ModelState.IsValid)
            {
                inventoryManagement.Purchase_Date =
                    DateTime.SpecifyKind(inventoryManagement.Purchase_Date, DateTimeKind.Utc);
                inventoryManagement.PartitionKey = "InventoryPartition";
                inventoryManagement.RowKey = Guid.NewGuid().ToString();

                await _tableStorageService.AddInventoryManagementAsync(inventoryManagement);

                // Use names directly from the form
                string message = $"New purchase by customer {CustomerName} of product {ProductName} on {inventoryManagement.Purchase_Date}";
                await _queueService.SendMessageAsync(message);

                return RedirectToAction("Index");
            }

            ViewData["Customer"] = await _tableStorageService.GetAllCustomersAsync();
            ViewData["Product"] = await _tableStorageService.GetAllProductsAsync();
            return View(inventoryManagement);
        }

    }
}
