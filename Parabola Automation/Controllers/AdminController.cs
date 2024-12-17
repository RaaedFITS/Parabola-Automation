using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parabola_Automation.Data;
using Parabola_Automation.Models;

namespace Parabola_Automation.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserFlows)
                .ToListAsync();
            var flows = await _context.Flows.ToListAsync();

            ViewBag.Flows = flows;

            return View(users);
        }

        // Remove or comment out this action since we don't have a Flows.cshtml and don't want a separate page.
        // public async Task<IActionResult> Flows()
        // {
        //     var flows = await _context.Flows.ToListAsync();
        //     return View(flows); // This line is causing the error because there's no Flows view.
        // }

        [HttpPost]
        public async Task<IActionResult> AddFlow(string Name, string Description)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                TempData["Message"] = "Flow name is required.";
                return RedirectToAction("Index"); // Changed from "Flows" to "Index"
            }

            var newFlow = new Flow
            {
                Name = Name.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Flows.Add(newFlow);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Flow added successfully.";
            return RedirectToAction("Index"); // Changed from "Flows" to "Index"
        }

        [HttpPost]
        public async Task<IActionResult> EditFlow(int Id, string Name, string Description)
        {
            var flow = await _context.Flows.FirstOrDefaultAsync(f => f.Id == Id);
            if (flow == null)
            {
                TempData["Message"] = "Flow not found.";
                return RedirectToAction("Index"); // Changed from "Flows" to "Index"
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                TempData["Message"] = "Flow name is required.";
                return RedirectToAction("Index"); // Changed from "Flows" to "Index"
            }

            flow.Name = Name.Trim();
            flow.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            flow.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Flow updated successfully.";
            return RedirectToAction("Index"); // Changed from "Flows" to "Index"
        }

        public async Task<IActionResult> DeleteFlow(int id)
        {
            var flow = await _context.Flows.FirstOrDefaultAsync(f => f.Id == id);
            if (flow == null)
            {
                TempData["Message"] = "Flow not found.";
                return RedirectToAction("Index"); // Changed from "Flows" to "Index"
            }

            _context.Flows.Remove(flow);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Flow deleted successfully.";
            return RedirectToAction("Index"); // Changed from "Flows" to "Index"
        }

        [HttpPost]
        public async Task<IActionResult> AssignFlows(int userId, int[] flowIds)
        {
            var user = await _context.Users
                .Include(u => u.UserFlows)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["Message"] = "User not found.";
                return RedirectToAction("Index");
            }

            _context.UserFlows.RemoveRange(user.UserFlows);

            foreach (var flowId in flowIds)
            {
                _context.UserFlows.Add(new UserFlow
                {
                    UserId = userId,
                    FlowId = flowId,
                    AddedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Flows updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(string Email, string Password, string Role)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Role))
            {
                TempData["Message"] = "Please fill in all fields.";
                return RedirectToAction("Index");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == Email.Trim().ToLower());
            if (existingUser != null)
            {
                TempData["Message"] = "A user with this email already exists.";
                return RedirectToAction("Index");
            }

            var hashedPassword = HashPassword(Password);

            var newUser = new User
            {
                Email = Email.Trim(),
                PasswordHash = hashedPassword,
                Role = Role.Trim().ToLower(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["Message"] = "User added successfully.";
            return RedirectToAction("Index");
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
    }

}
