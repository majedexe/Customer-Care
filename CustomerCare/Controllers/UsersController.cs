using CustomerCare.Models.Identity;
using CustomerCare.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerCare.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                list.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                return RedirectToAction(nameof(Index));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction(nameof(Index));

            if (!await _roleManager.RoleExistsAsync(role))
                return RedirectToAction(nameof(Index));

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                return RedirectToAction(nameof(Index));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction(nameof(Index));

            if (!await _userManager.IsInRoleAsync(user, role))
                return RedirectToAction(nameof(Index));

            if (role == RoleNames.Admin)
            {
                var admins = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);
                if (admins.Count <= 1 && admins.Any(a => a.Id == userId))
                {
                    TempData["Error"] = "Cannot remove the last Admin user.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _userManager.RemoveFromRoleAsync(user, role);

            return RedirectToAction(nameof(Index));
        }
    }
}
