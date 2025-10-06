using Microsoft.AspNetCore.Mvc;
using ST10449392_CLDV6212_POE.Models;
using ST10449392_CLDV6212_POE.Services;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Text.Json;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        //public ProductController (BlobService blobService, TableStorageService tableStorageService)
        //{
        //    _blobService = blobService;
        //    _tableStorageService = tableStorageService;
        //}

        public ProductController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        //public async Task<IActionResult> Index()
        //{
        //    try
        //    {
        //        var baseUrl = _configuration["FunctionApi:BaseUrl"]?.TrimEnd('/');
        //        if (string.IsNullOrEmpty(baseUrl))
        //        {
        //            throw new Exception("Base URL is missing or invalid in configuration.");
        //        }

        //        var client = _httpClientFactory.CreateClient();
        //        client.BaseAddress = new Uri(baseUrl + "/");

        //        // Call your Azure Function endpoint (GET /api/product)
        //        var response = await client.GetAsync("product");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            using var contentStream = await response.Content.ReadAsStreamAsync();
        //            var options = new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            };

        //            var products = await JsonSerializer.DeserializeAsync<IEnumerable<Product>>(contentStream, options);
        //            return View(products);
        //        }

        //        // Non-success response (like 404 or 500)
        //        ViewBag.ErrorMessage = $"Error retrieving data: {response.StatusCode}";
        //        return View(new List<Product>());
        //    }
        //    catch (HttpRequestException)
        //    {
        //        ViewBag.ErrorMessage = "Could not connect to the API. Please ensure Azure Function is running.";
        //        return View(new List<Product>());
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrorMessage = $"Unexpected error: {ex.Message}";
        //        return View(new List<Product>());
        //    }
        //}

        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                var baseUrl = _configuration["FunctionApi:BaseUrl"]?.TrimEnd('/');
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new Exception("Base URL is missing or invalid in configuration.");
                }

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl + "/");

                var response = await client.GetAsync("product");

                if (response.IsSuccessStatusCode)
                {
                    using var contentStream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var products = await JsonSerializer.DeserializeAsync<IEnumerable<Product>>(contentStream, options);

                    // Filter in memory if search string is provided
                    if (!string.IsNullOrEmpty(searchString) && products != null)
                    {
                        products = products.Where(p => p.Product_Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
                    }

                    ViewData["CurrentFilter"] = searchString;

                    return View(products);
                }

                ViewBag.ErrorMessage = $"Error retrieving data: {response.StatusCode}";
                return View(new List<Product>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Could not connect to the API. Please ensure Azure Function is running.";
                return View(new List<Product>());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Unexpected error: {ex.Message}";
                return View(new List<Product>());
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please upload a product image.");
                return View(product);
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(product.Product_Name ?? ""), "Product_Name");
            content.Add(new StringContent(product.Price ?? ""), "Price");

            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "ImageUrl", file.FileName);

            try
            {
                var baseUrl = _configuration["FunctionApi:BaseUrl"]?.TrimEnd('/');
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new Exception("Base URL is missing or invalid in configuration.");
                }

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl + "/");

                var response = await client.PostAsync("product-with-image", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, $"Error: {responseContent}");
                return View(product);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
                return View(product);
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        //{
        //    if (file != null)
        //    {
        //        // Upload image to Blob Storage
        //        using var stream = file.OpenReadStream();
        //        var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
        //        product.ImageUrl = imageUrl;
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        product.PartitionKey = "ProductPartition"; // Set a default partition key
        //        product.RowKey = Guid.NewGuid().ToString(); // Generate a unique row key
        //        await _tableStorageService.AddProductAsync(product);
        //        return RedirectToAction("Index");
        //    }
        //    return View(product);
        //}

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
