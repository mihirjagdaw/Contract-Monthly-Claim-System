using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis.Operations;
using ST10449392_CLDV6212_POE.Models;
using ST10449392_CLDV6212_POE.Services;
using System.Text;
using System.Text.Json;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        //public CustomerController (TableStorageService tableStorageService)
        //{
        //    _tableStorageService = tableStorageService;
        //}

        public CustomerController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        //public async Task<IActionResult> Index()
        //{
        //    var customers = await _tableStorageService.GetAllCustomersAsync();
        //    return View(customers);
        //}

        public async Task<IActionResult> Index()
        {
            var HttpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["FunctionApi:BaseUrl"];

            try
            {
                var httpResponseMessage = await HttpClient.GetAsync($"{apiBaseUrl}customer");
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var customer = await JsonSerializer.DeserializeAsync<IEnumerable<Customer>>(contentStream, options);
                    return View(customer);
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Could not connect to the API. Please ensure Azure Function is running.";
                return View(new List<Customer>());
            }

            ViewBag.ErrorMessage = "An error occurred while retrieving the data from the API";
            return View();
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public async Task<IActionResult> AddCustomer(Customer customer)
        //{
        //    customer.PartitionKey = "CustomerPartition"; // Set a default partition key
        //    customer.RowKey = Guid.NewGuid().ToString(); // Generate a unique row key

        //    await _tableStorageService.AddCustomerAsync(customer);
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer)
        {
            var queuePayload = new
            {
                Name = customer.Name,
                Email = customer.Email,
                Password = customer.Password
            };

            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["FunctionApi:BaseUrl"];
            var jsonContent = JsonSerializer.Serialize(queuePayload);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{apiBaseUrl}customer", httpContent);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            ViewBag.ErrorMessage = "Failed to add customer.";
            return View(customer);
        }


        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View();
        }

        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return BadRequest();
            }

            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            await _tableStorageService.UpdateCustomerAsync(customer);
            return RedirectToAction("Index");
        }

    }
}
