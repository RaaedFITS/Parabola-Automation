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
            var flow = await _context.Flows
                .Include(f => f.UserFlows) // Include UserFlows to check assignments
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flow == null)
            {
                TempData["Message"] = "Flow not found.";
                return RedirectToAction("Index");
            }

            // Check if this flow is assigned to any user
            if (flow.UserFlows != null && flow.UserFlows.Any())
            {
                // On error
                TempData["Message"] = "Cannot delete this flow as it is currently assigned to one or more users. Please unassign it first.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            _context.Flows.Remove(flow);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Flow deleted successfully.";
            return RedirectToAction("Index");
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

        [HttpPost]
        public async Task<IActionResult> EditUser(int Id, string Email, string Password, string Role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
            {
                TempData["Message"] = "User not found.";
                return RedirectToAction("Index");
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Role))
            {
                TempData["Message"] = "Email and role are required.";
                return RedirectToAction("Index");
            }

            // Update Email and Role
            user.Email = Email.Trim();
            user.Role = Role.Trim().ToLower();
            user.UpdatedAt = DateTime.UtcNow;

            // Update password only if a new password is provided
            if (!string.IsNullOrWhiteSpace(Password))
            {
                user.PasswordHash = HashPassword(Password);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var loggedInEmail = HttpContext.Session.GetString("LoggedInUser");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["Message"] = "User not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Prevent deleting the currently logged-in user
            if (user.Email.Equals(loggedInEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Message"] = "You cannot delete your own account while logged in.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "User deleted successfully.";
            return RedirectToAction("Index");
        }


    }

}
