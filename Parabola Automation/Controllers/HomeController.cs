using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Parabola_Automation.Models;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Google.Apis.Upload;
using Google.Apis.Drive.v3.Data;

namespace Parabola_Automation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string PathToServiceAccountKeyFiles = "credentials.json";
        private const string ServiceAccountEmail = "";
        private const string UploadFileName = "docdoc.docx";
        private const string DirectoryId = "1KuBOVBlb0iNA6gZvFHk6r0R_n8pXgrjY";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Meth()
        {
            // Load the credentials
            var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFiles)
                .CreateScoped(DriveService.ScopeConstants.Drive);

            // Create the Drive service
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            // Search for text files in the specified directory
            var request = service.Files.List();
            request.Q = $"'{DirectoryId}' in parents"; // Specify the folder ID
            var response = await request.ExecuteAsync();

            foreach (var driveFile in response.Files)
            {
                Console.WriteLine($"{driveFile.Id} {driveFile.Name} {driveFile.MimeType}");
            }

            // Download the first text file if it exists
            var textFile = response.Files.FirstOrDefault(file => file.MimeType.Equals("application/vnd.google-apps.document"));
            if (textFile != null)
            {
                var getRequest = service.Files.Get(textFile.Id);
                await using var fileStream = new FileStream(textFile.Name, FileMode.Create, FileAccess.Write);
                await getRequest.DownloadAsync(fileStream);

                Console.WriteLine($"Downloaded file: {textFile.Name}");
            }
            else
            {
                Console.WriteLine("No text files found in the specified directory.");
            }

            return View("~/Views/Login/Index.cshtml");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Call the Meth method to list and download files
                await Meth();

                string fileId = "1vX1OH1waD4ZLre-EbsWKU4RM-rHPPmwJN0H59invdLw"; // Replace with your `.docx` file ID
                string docContent = FetchDocxFileFromGoogleDrive(fileId);

                if (string.IsNullOrEmpty(docContent))
                {
                    ViewBag.Message = "No content found in the Google Drive file.";
                }
                else
                {
                    ViewBag.Message = docContent;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }

            return View("~/Views/Login/Index.cshtml");
        }
        private string FetchDocxFileFromGoogleDrive(string fileId)
        {
            string credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "credentials.json");

            // Authenticate using Service Account credentials
            GoogleCredential credential = GoogleCredential.FromFile(credentialsPath)
                .CreateScoped(DriveService.Scope.DriveReadonly);

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ParabolaProject"
            });

            // Get the file content
            var request = service.Files.Get(fileId);
            using (var stream = new MemoryStream())
            {
                request.Download(stream);

                // Log the size of the downloaded file
                if (stream.Length == 0)
                {
                    throw new Exception("The downloaded file is empty. Check the file ID and permissions.");
                }

                // Parse `.docx` content
                using (var wordDoc = WordprocessingDocument.Open(stream, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    return body.InnerText;
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult GetFlows()
        {
            try
            {
                var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "flows.json");

                if (!System.IO.File.Exists(jsonFilePath))
                {
                    return NotFound(new { error = "Flows file not found." });
                }

                var jsonData = System.IO.File.ReadAllText(jsonFilePath);
                var flowNames = JsonSerializer.Deserialize<List<string>>(jsonData);

                return Json(flowNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

      

        [HttpPost]
        public IActionResult TriggerPython([FromForm] IFormFile file, [FromForm] string flowName)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(flowName))
            {
                return BadRequest(new { error = "File or flow name is missing." });
            }

            // Save the uploaded file
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            try
            {
                // Trigger the Python script with the flowName and filePath
                var startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"parabola.py \"{flowName}\" \"{filePath}\"", // Pass both flowName and filePath
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    return BadRequest(new { error });
                }

                return Ok(new { message = "Python script triggered successfully.", output });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

     



    }
}
