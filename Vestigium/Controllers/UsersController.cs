using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Vestigium.Constants;
using Vestigium.Data;
using Vestigium.Models.ViewModels;
using ClosedXML.Excel;

namespace Vestigium.Controllers
{
    [Authorize(Roles = "Admin")]
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

        #region Index

        public IActionResult Index()
        {
            var users = _userManager.Users
                .OrderBy(x => x.UserName)
                .ToList();

            return View(users);
        }

        #endregion

        #region Create
        /*
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewBag.Areas = UserAreas.Areas;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            UserCreateVM model)
        {
            ViewBag.Roles = _roleManager.Roles.ToList();

            if (!ModelState.IsValid)
                return View(model);

            var exists = await _userManager
                .FindByNameAsync(model.UserName);

            if (exists != null)
            {
                ModelState.AddModelError(
                    "",
                    "El usuario ya existe.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Area = model.Area,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager
                .CreateAsync(user, model.Password);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(
                user,
                model.Role);

            TempData["Success"] =
                "Usuario creado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        */
        #endregion

        #region Edit

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new UserEditVM
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Area = user.Area,
                Role = roles.FirstOrDefault() ?? "User"
            };

            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewBag.Areas = UserAreas.Areas;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UserEditVM model)
        {
            ViewBag.Roles = _roleManager.Roles.ToList();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager
                .FindByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Area = model.Area;

            await _userManager.UpdateAsync(user);

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(
                user,
                currentRoles);

            await _userManager.AddToRoleAsync(
                user,
                model.Role);

            TempData["Success"] =
                "Usuario actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        #endregion

        public async Task<IActionResult> ResetAdmin()
        {
            var user =
                await _userManager.FindByEmailAsync(
                    "admin@inventory.com");

            var token =
                await _userManager.GeneratePasswordResetTokenAsync(
                    user);

            await _userManager.ResetPasswordAsync(
                user,
                token,
                "Admin#2026");

            return Content("Contraseña restablecida.");
        }

        #region
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(new ResetPasswordVM
            {
                UserId = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? string.Empty,
                ForceChange = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
    ResetPasswordVM model)
        {
            var user =
                await _userManager.FindByIdAsync(
                    model.UserId);

            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.FullName =
                    user.FullName;

                model.UserName =
                    user.UserName ?? string.Empty;

                return View(model);
            }

            var token =
                await _userManager
                    .GeneratePasswordResetTokenAsync(user);

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    model.TemporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                model.FullName =
                    user.FullName;

                model.UserName =
                    user.UserName ?? string.Empty;

                return View(model);
            }

            user.MustChangePassword =
                model.ForceChange;

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                $"La contraseña de {user.FullName} fue restablecida correctamente.";

            return RedirectToAction(nameof(Index));
        }

        #endregion


        /*#region Disable

        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.Email == "admin@inventory.com")
            {
                TempData["Error"] =
                    "No se puede desactivar el administrador principal.";

                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                user.IsActive
                    ? "Usuario activado correctamente."
                    : "Usuario desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        #endregion*/

        #region Import Users

        public IActionResult ImportUsers()
        {
            return View(new ImportUsersVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportUsers(
            ImportUsersVM model)
        {
            if (model.ExcelFile == null ||
                model.ExcelFile.Length == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Seleccione un archivo Excel.");

                return View(model);
            }

            using var stream = new MemoryStream();

            await model.ExcelFile.CopyToAsync(stream);

            stream.Position = 0;

            ExcelPackage.License
                .SetNonCommercialOrganization("Vestigium");

            using var package = new ExcelPackage(stream);

            var worksheet =
                package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El archivo no contiene hojas.");

                return View(model);
            }

            if (worksheet.Dimension == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El archivo no contiene datos.");

                return View(model);
            }

            int rows = worksheet.Dimension.Rows;

            int createdUsers = 0;
            int skippedUsers = 0;

            for (int row = 2; row <= rows; row++)
            {
                string employeeNumber =
                    worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty;

                string fullName =
                    worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty;

                string email =
                    worksheet.Cells[row, 3].Text?.Trim() ?? string.Empty;

                string area =
                    worksheet.Cells[row, 4].Text?.Trim() ?? string.Empty;

                string role =
                    worksheet.Cells[row, 5].Text?.Trim() ?? "User";

                if (string.IsNullOrWhiteSpace(employeeNumber))
                {
                    skippedUsers++;
                    continue;
                }

                var existingUser =
                    await _userManager.FindByNameAsync(
                        employeeNumber);

                if (existingUser != null)
                {
                    skippedUsers++;
                    continue;
                }

                var user = new ApplicationUser
                {
                    UserName = employeeNumber,
                    Email = email,
                    FullName = fullName,
                    Area = area,
                    MustChangePassword = true,
                    EmailConfirmed = true
                };

                var result =
                    await _userManager.CreateAsync(
                        user,
                        "Welcome#2026");

                if (result.Succeeded)
                {
                    if (!string.IsNullOrWhiteSpace(role) &&
                        await _roleManager.RoleExistsAsync(role))
                    {
                        await _userManager.AddToRoleAsync(
                            user,
                            role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(
                            user,
                            "User");
                    }

                    createdUsers++;
                }
                else
                {
                    skippedUsers++;
                }
            }

            TempData["Success"] =
                $"Importación completada. " +
                $"Usuarios creados: {createdUsers}. " +
                $"Omitidos: {skippedUsers}.";

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region

        [HttpGet]
        public async Task<IActionResult> ExportUsers()
        {
            var users = _userManager.Users.ToList();

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Usuarios");

            worksheet.Cell(1, 1).Value = "Número Empleado";
            worksheet.Cell(1, 2).Value = "Nombre";
            worksheet.Cell(1, 3).Value = "Correo";
            worksheet.Cell(1, 4).Value = "Área";
            worksheet.Cell(1, 5).Value = "Rol";

            int row = 2;

            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);

                worksheet.Cell(row, 1).Value =
                    user.UserName;

                worksheet.Cell(row, 2).Value =
                    user.FullName;

                worksheet.Cell(row, 3).Value =
                    user.Email;

                worksheet.Cell(row, 4).Value =
                    user.Area;

                worksheet.Cell(row, 5).Value =
                    roles.FirstOrDefault();

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Usuarios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        #endregion

    }
}