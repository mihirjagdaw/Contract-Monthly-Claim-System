using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using ST10449392_CLDV6212_POE.Models;
using ST10449392_CLDV6212_POE.Services;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;

        public ProductController (BlobService blobService, TableStorageService tableStorageService)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            if (file != null)
            {
                // Upload image to Blob Storage
                using var stream = file.OpenReadStream();
                var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
                product.ImageUrl = imageUrl;
            }

            if (ModelState.IsValid)
            {
                product.PartitionKey = "ProductPartition"; // Set a default partition key
                product.RowKey = Guid.NewGuid().ToString(); // Generate a unique row key
                await _tableStorageService.AddProductAsync(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
        {
            // Delete image from Blob Storage
            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                await _blobService.DeleteBlobAsync(product.ImageUrl);
            }
            // Delete product from Table Storage
            await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        } 

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        // GET: Product/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return BadRequest();

            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Product/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                // Upload new image to Blob Storage
                using var stream = file.OpenReadStream();
                var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
                product.ImageUrl = imageUrl;
            }
            else
            {
                // Keep existing image if no new file is uploaded
                var existingProduct = await _tableStorageService.GetProductAsync(product.PartitionKey, product.RowKey);
                if (existingProduct != null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
            }

            if (!ModelState.IsValid)
                return View(product);

            await _tableStorageService.UpdateProductAsync(product);

            return RedirectToAction("Index");
        }


    }
}
