using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ST10449392_CLDV6212_POE.Models;
using ST10449392_CLDV6212_POE.Services;
using System.Net.Http;
using System.Text.Json;

namespace ST10449392_CLDV6212_POE.Controllers
{
    public class FilesController : Controller
    {
        private readonly AzureFileShareService _fileShareService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        //public FilesController(AzureFileShareService fileShareService)
        //{
        //    _fileShareService = fileShareService;
        //}

        public FilesController(AzureFileShareService fileShareService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            string connectionString = _configuration.GetConnectionString("AzureStorage");
            string fileShareName = "productshare"; 

            _fileShareService = new AzureFileShareService(connectionString, fileShareName);
        }

        //public async Task<IActionResult> Index()
        //{
        //    List<FileModel> files;
        //    try
        //    {
        //        files = await _fileShareService.ListFilesAsync("uploads");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = $"Failed to load files : {ex.Message}";
        //        files = new List<FileModel>();
        //    }
            
        //    return View(files);
        //}

        public async Task<IActionResult> Index()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["FunctionApi:BaseUrl"];

            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{apiBaseUrl}files");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    var files = await JsonSerializer.DeserializeAsync<IEnumerable<FileModel>>(contentStream, options);
                    return View(files);
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Could not connect to the API. Please ensure the Azure Function is running.";
                return View(new List<FileModel>());
            }

            ViewBag.ErrorMessage = "An error occurred while retrieving data from the API.";
            return View(new List<FileModel>());
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var baseUrl = _configuration["FunctionApi:BaseUrl"]?.TrimEnd('/');
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{baseUrl}/files", content);

            TempData["Message"] = response.IsSuccessStatusCode
                ? "File uploaded successfully."
                : $"Error: {await response.Content.ReadAsStringAsync()}";

            return RedirectToAction("Index");
        }



        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be null or empty");
            }
            try
            {
                var fileStream = await _fileShareService.DownloadFileAsync("uploads", fileName);
                if (fileStream == null)
                {
                    return NotFound($"File '{fileName}' not found.");
                }
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception e)
            {
                return BadRequest($"Error downloading file: {e.Message}");
            }
        }
    }
}
