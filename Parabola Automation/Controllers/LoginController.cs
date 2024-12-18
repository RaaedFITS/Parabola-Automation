using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parabola_Automation.Data;

namespace Parabola_Automation.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            System.Diagnostics.Debug.WriteLine("Starting");
            return View("~/Views/Login/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Email and password are required.";
                return View("Index");
            }

            var normalizedEmail = email.Trim().ToLower();

            // Fetch the user by email (case-insensitive)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                ViewBag.Message = "Invalid email or password.";
                return View("Index");
            }

            // Hash the input password and compare with the stored hash
            var hashedInputPassword = HashPassword(password);
            if (user.PasswordHash != hashedInputPassword)
            {
                ViewBag.Message = "Invalid email or password.";
                return View("Index");
            }

            HttpContext.Session.SetString("LoggedInUser", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clears all session data
            return RedirectToAction("Index", "Login"); // Redirect to the login page
          
        }

    }

}
