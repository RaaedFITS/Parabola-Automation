using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Parabola_Automation.Models;
using System.Text.Json;

namespace Parabola_Automation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
