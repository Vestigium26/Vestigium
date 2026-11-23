using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vestigium.Data;
using Vestigium.Models.ViewModels;
using Vestigium.Models;

namespace Vestigium.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
        UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var roles =
                await _userManager.GetRolesAsync(user);

            var vm = new ProfileVM
            {
                FullName = user.FullName,
                EmployeeNumber = user.UserName,
                Email = user.Email ?? "",
                Area = user.Area,
                Role = roles.FirstOrDefault() ?? "Usuario"
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordVM());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var result =
                await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            TempData["Success"] =
                "La contraseña fue actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}