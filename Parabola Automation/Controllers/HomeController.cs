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
using System.Text.Json.Serialization;

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
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                // Call the Meth method to download the Word file
                var fileName = await Meth();

                if (string.IsNullOrEmpty(fileName))
                {
                    ViewBag.Message = "Error: User data file not found.";
                    return View("~/Views/Login/Index.cshtml");
                }

                // Extract JSON content from the Word document
                var jsonContent = ExtractJsonFromWord(fileName);

                if (string.IsNullOrEmpty(jsonContent))
                {
                    ViewBag.Message = "Error: Unable to extract user data.";
                    return View("~/Views/Login/Index.cshtml");
                }

                // Deserialize the JSON
                var users = JsonSerializer.Deserialize<List<User>>(jsonContent);

                if (users == null)
                {
                    ViewBag.Message = "Error: Unable to parse user data.";
                    return View("~/Views/Login/Index.cshtml");
                }

                // Debugging: Print input values and stored data
                Console.WriteLine($"Input Email: {email}");
                Console.WriteLine($"Input Password: {password}");

                foreach (var u in users)
                {
                    Console.WriteLine($"Stored Email: {u.Email}");
                    Console.WriteLine($"Stored Password: {u.Password}");
                    Console.WriteLine($"Stored Names: {(u.Names != null ? string.Join(", ", u.Names) : "No Names Found")}");
                }

                // Validate user credentials
                var matchingUser = users.FirstOrDefault(u =>
                    u.Email.Trim().Equals(email.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    u.Password.Trim().Equals(password.Trim()));

                if (matchingUser == null)
                {
                    Console.WriteLine("No matching user found. Check input values and stored data.");
                    ViewBag.Message = "Invalid email or password.";
                    return View("~/Views/Login/Index.cshtml");
                }

                // Handle null Names property
                ViewBag.Names = matchingUser.Names ?? new List<string>();
                Console.WriteLine($"User found: {matchingUser.Email}, Names: {string.Join(", ", ViewBag.Names)}");

                return View("~/Views/Home/Index.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View("~/Views/Login/Index.cshtml");
            }
        }




        public class User
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("password")]
            public string Password { get; set; }

            [JsonPropertyName("names")]
            public List<string> Names { get; set; }
        }

        public async Task<string> Meth()
        {
            // Load the credentials
            var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFiles)
                .CreateScoped(DriveService.ScopeConstants.Drive);

            // Create the Drive service
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            // Search for Google Docs in the specified directory
            var request = service.Files.List();
            request.Q = $"'{DirectoryId}' in parents"; // Specify the folder ID
            var response = await request.ExecuteAsync();

            foreach (var driveFile in response.Files)
            {
                Console.WriteLine($"{driveFile.Id} {driveFile.Name} {driveFile.MimeType}");
            }

            // Export the first Google Docs file to `.docx` format if it exists
            var googleDocFile = response.Files.FirstOrDefault(file => file.MimeType.Equals("application/vnd.google-apps.document"));
            if (googleDocFile != null)
            {
                var exportRequest = service.Files.Export(googleDocFile.Id, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                var fileName = $"{googleDocFile.Name}.docx";

                await using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                await exportRequest.DownloadAsync(fileStream);

                Console.WriteLine($"Exported and saved file: {fileName}");

                // Return the file name
                return fileName;
            }

            Console.WriteLine("No Google Docs found in the specified directory.");
            return null;
        }

        public string ExtractJsonFromWord(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                // Extract the text content from the Word document
                var body = wordDoc.MainDocumentPart.Document.Body;
                return body.InnerText; // Return the plain text content
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Call the Meth method to list and download files
                await Meth();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }

            return View("~/Views/Login/Index.cshtml");
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
