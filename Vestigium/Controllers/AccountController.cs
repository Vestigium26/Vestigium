using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vestigium.Data;
using Vestigium.Models.ViewModels;

namespace Vestigium.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return Redirect("/Identity/Account/Login");

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

            user.MustChangePassword = false;

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                "Contraseña actualizada correctamente.";

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}