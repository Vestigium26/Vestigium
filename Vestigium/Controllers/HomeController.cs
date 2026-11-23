using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Vestigium.Data;
using Vestigium.Models;
using Vestigium.Models.Enums;
using Vestigium.Models.ViewModels;

namespace Vestigium.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Página pública de bienvenida
        public IActionResult Index()
        {
            return View();
        }

        // Dashboard protegido
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user =
                await _userManager.GetUserAsync(User);

            var vm = new DashboardVM
            {
                FullName = user?.FullName ?? "Usuario",

                UserName = user?.UserName ?? string.Empty,

                Area = user?.Area ?? string.Empty, // Nuevo

                LowInventoryCount = await _context.Materials
                    .CountAsync(x => x.Quantity <= 10), // Nuevo

                CurrentDateTime = DateTime.Now,

                TotalMaterials = await _context.Materials
                    .CountAsync(),

                TotalInventory = await _context.Materials
                    .SumAsync(x => x.Quantity),

                ScrapQuantity = await _context.Movements
                    .Where(x =>
                        x.MovementType == MovementType.Output &&
                        x.DestinationLocation == "Scrap")
                    .SumAsync(x => (int?)x.Quantity) ?? 0,

                MrbQuantity = await _context.Movements
                    .Where(x =>
                        x.MovementType == MovementType.Output &&
                        x.DestinationLocation == "MRB")
                    .SumAsync(x => (int?)x.Quantity) ?? 0,

                LowInventoryMaterials = await _context.Materials
                    .Where(x => x.Quantity <= 10)
                    .OrderBy(x => x.Quantity)
                    .Take(10)
                    .ToListAsync(),

                LastMovement = await _context.Movements
                    .Include(x => x.Material)
                    .OrderByDescending(x => x.MovementDate)
                    .FirstOrDefaultAsync(),

                RecentMovements = await _context.Movements
                    .Include(x => x.Material)
                    .OrderByDescending(x => x.MovementDate)
                    .Take(5)
                    .ToListAsync(),

                TotalEntries = await _context.Movements
                    .Where(x =>
                        x.MovementType == MovementType.Entry)
                    .SumAsync(x => (int?)x.Quantity) ?? 0,

                TotalTransfers = await _context.Movements
                    .Where(x =>
                        x.MovementType == MovementType.Transfer)
                    .SumAsync(x => (int?)x.Quantity) ?? 0,

                TotalScrap = await _context.Movements
                    .Where(x =>
                        x.MovementType == MovementType.Output &&
                        x.DestinationLocation == "Scrap")
                    .SumAsync(x => (int?)x.Quantity) ?? 0,

                TotalMrb = await _context.Movements
                    .Where(x =>
                        x.MovementType == MovementType.Output &&
                        x.DestinationLocation == "MRB")
                    .SumAsync(x => (int?)x.Quantity) ?? 0
            };

            

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}