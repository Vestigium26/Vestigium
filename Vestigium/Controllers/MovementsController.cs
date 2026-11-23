using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vestigium.Data;
using Vestigium.Models;
using Vestigium.Models.Enums;
using Vestigium.Models.ViewModels;
using Vestigium.Constants;
using QuestPDF.Fluent;
using Vestigium.ReportModels;
using Vestigium.Reports;

namespace Vestigium.Controllers
{
    [Authorize]
    public class MovementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MovementsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region Index

        /*public async Task<IActionResult> Index()
        {
            var movements = await _context.Movements
                .Include(x => x.Material)
                .OrderByDescending(x => x.MovementDate)
                .ToListAsync();

            return View(movements);
        }*/

        public async Task<IActionResult> Index(
    DateTime? date,
    DateTime? fromDate,
    DateTime? toDate)
        {
            var query = _context.Movements
                .Include(x => x.Material)
                .AsQueryable();

            // Fecha específica

            if (date.HasValue)
            {
                query = query.Where(x =>
                    x.MovementDate.Date ==
                    date.Value.Date);

                ViewBag.SelectedDate =
                    date.Value.ToString("yyyy-MM-dd");

                ViewBag.Periodo =
                    date.Value.ToString("dd/MM/yyyy");
            }

            // Rango de fechas

            else if (fromDate.HasValue &&
                     toDate.HasValue)
            {
                query = query.Where(x =>
                    x.MovementDate >= fromDate.Value.Date &&
                    x.MovementDate < toDate.Value.Date.AddDays(1));

                ViewBag.FromDate =
                    fromDate.Value.ToString("yyyy-MM-dd");

                ViewBag.ToDate =
                    toDate.Value.ToString("yyyy-MM-dd");

                ViewBag.Periodo =
                    $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";
            }

            // Default: hoy

            else
            {
                var today = DateTime.Today;

                query = query.Where(x =>
                    x.MovementDate.Date == today);

                ViewBag.SelectedDate =
                    today.ToString("yyyy-MM-dd");

                ViewBag.Periodo =
                    today.ToString("dd/MM/yyyy");
            }

            var movements = await query
                .OrderByDescending(x => x.MovementDate)
                .ToListAsync();

            return View(movements);
        }

        #endregion

        #region Create

        public async Task<IActionResult> Create()
        {
            await LoadMaterialsList();


            // ==========================================
            // UBICACIONES VÁLIDAS DE ALMACÉN
            // ==========================================

            ViewBag.WarehouseLocations =
                WarehouseLocations.GetLocations();


            return View(
                new MovementCreateVM());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MovementCreateVM model)
        {
            // ==========================================
            // RECARGAR DATOS DE LA VISTA
            // ==========================================

            await LoadMaterialsList();


            ViewBag.WarehouseLocations =
                WarehouseLocations.GetLocations();


            // ==========================================
            // VALIDACIÓN DEL MODELO
            // ==========================================

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ==========================================
            // VALIDAR TIPO DE MOVIMIENTO
            // ==========================================

            if (!Enum.IsDefined(
                typeof(MovementType),
                model.MovementType))
            {
                ModelState.AddModelError(
                    nameof(model.MovementType),
                    "Seleccione un tipo de movimiento válido.");


                return View(model);
            }


            // ==========================================
            // BUSCAR MATERIAL
            // ==========================================

            var material =
                await _context.Materials
                    .Include(x =>
                        x.Locations)
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        model.MaterialId);


            if (material == null)
            {
                ModelState.AddModelError(
                    nameof(model.MaterialId),
                    "Material no encontrado.");


                return View(model);
            }


            // ==========================================
            // NORMALIZAR UBICACIÓN
            // ==========================================

            model.OriginLocation =
                model.OriginLocation?
                    .Trim()
                    .ToUpperInvariant()
                ?? string.Empty;


            // ==========================================
            // VALIDAR UBICACIÓN DE ALMACÉN
            // ==========================================

            if (!WarehouseLocations.IsValid(
                model.OriginLocation))
            {
                ModelState.AddModelError(
                    nameof(model.OriginLocation),
                    "Seleccione una ubicación válida de almacén.");


                return View(model);
            }


            // ==========================================
            // BUSCAR UBICACIÓN ACTUAL
            // ==========================================

            var location =
                material.Locations
                    .FirstOrDefault(x =>
                        x.LocationCode ==
                        model.OriginLocation);


            // ==========================================
            // TRANSFERENCIA / SALIDA
            // REQUIEREN INVENTARIO EXISTENTE
            // ==========================================

            if (
                model.MovementType !=
                MovementType.Entry
            )
            {
                if (location == null)
                {
                    ModelState.AddModelError(
                        nameof(model.OriginLocation),
                        "La ubicación seleccionada no contiene inventario de este material.");


                    return View(model);
                }


                if (
                    location.Quantity <
                    model.Quantity
                )
                {
                    ModelState.AddModelError(
                        nameof(model.Quantity),
                        "La cantidad excede el inventario disponible en la ubicación seleccionada.");


                    return View(model);
                }
            }


            // ==========================================
            // VALIDAR DESTINO DE TRANSFERENCIA
            // ==========================================

            if (
                model.MovementType ==
                MovementType.Transfer
            )
            {
                if (
                    string.IsNullOrWhiteSpace(
                        model.DestinationLocation) ||
                    !ProductionAreas.Areas.Contains(
                        model.DestinationLocation)
                )
                {
                    ModelState.AddModelError(
                        nameof(model.DestinationLocation),
                        "Seleccione un destino válido de producción.");


                    return View(model);
                }
            }


            // ==========================================
            // VALIDAR DESTINO DE SALIDA
            // ==========================================

            if (
                model.MovementType ==
                MovementType.Output
            )
            {
                var validOutputs =
                    new[]
                    {
                "MRB",
                "Scrap"
                    };


                if (
                    string.IsNullOrWhiteSpace(
                        model.DestinationLocation) ||
                    !validOutputs.Contains(
                        model.DestinationLocation)
                )
                {
                    ModelState.AddModelError(
                        nameof(model.DestinationLocation),
                        "Seleccione MRB o Scrap como destino.");


                    return View(model);
                }
            }


            // ==========================================
            // USUARIO QUE REGISTRA
            // ==========================================

            var currentUser =
                await _userManager.GetUserAsync(
                    User);


            if (currentUser == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible identificar al usuario actual.");


                return View(model);
            }


            // ==========================================
            // VALIDAR RECEPTOR
            // IMPORTANTE:
            // Esto ocurre ANTES de modificar inventario.
            // ==========================================

            if (string.IsNullOrWhiteSpace(
                model.ReceivedByUserId))
            {
                ModelState.AddModelError(
                    nameof(model.ReceivedByUserId),
                    "Seleccione la persona que recibe el material.");


                return View(model);
            }


            var receiver =
                await _userManager.FindByIdAsync(
                    model.ReceivedByUserId);


            if (receiver == null)
            {
                ModelState.AddModelError(
                    nameof(model.ReceivedByUserId),
                    "Seleccione un receptor válido.");


                return View(model);
            }


            // ==========================================
            // VALIDAR ÁREA DEL RECEPTOR
            // ==========================================

            bool validReceiver =
                model.MovementType switch
                {
                    MovementType.Entry =>
                        receiver.Area ==
                        "Warehouse",

                    MovementType.Transfer =>
                        receiver.Area ==
                        "Production Line",

                    MovementType.Output =>
                        receiver.Area ==
                        "Engineering" ||
                        receiver.Area ==
                        "Quality",

                    _ =>
                        false
                };


            if (!validReceiver)
            {
                ModelState.AddModelError(
                    nameof(model.ReceivedByUserId),
                    "El receptor seleccionado no corresponde al área requerida para este movimiento.");


                return View(model);
            }


            // ==========================================
            // ORIGEN / DESTINO PARA TRAZABILIDAD
            // ==========================================

            string originLocation =
                string.Empty;

            string destinationLocation =
                string.Empty;


            // ==========================================
            // APLICAR MOVIMIENTO
            // ==========================================

            switch (model.MovementType)
            {
                // ======================================
                // ENTRADA
                // Receiving → Rack
                // ======================================

                case MovementType.Entry:

                    originLocation =
                        "Receiving";


                    destinationLocation =
                        model.OriginLocation;


                    // La ubicación puede no existir todavía
                    // para este material.

                    if (location == null)
                    {
                        location =
                            new MaterialLocation
                            {
                                MaterialId =
                                    material.Id,

                                LocationCode =
                                    model.OriginLocation,

                                Quantity =
                                    0
                            };


                        _context.MaterialLocations.Add(
                            location);
                    }


                    location.Quantity +=
                        model.Quantity;


                    break;


                // ======================================
                // TRANSFERENCIA
                // Rack → Producción
                // ======================================

                case MovementType.Transfer:

                    originLocation =
                        model.OriginLocation;


                    destinationLocation =
                        model.DestinationLocation;


                    location!.Quantity -=
                        model.Quantity;


                    break;


                // ======================================
                // SALIDA
                // Rack → MRB / Scrap
                // ======================================

                case MovementType.Output:

                    originLocation =
                        model.OriginLocation;


                    destinationLocation =
                        model.DestinationLocation;


                    location!.Quantity -=
                        model.Quantity;


                    break;
            }


            // ==========================================
            // ELIMINAR UBICACIÓN VACÍA
            // ==========================================

            if (
                location != null &&
                location.Quantity == 0
            )
            {
                _context.MaterialLocations.Remove(
                    location);
            }


            // ==========================================
            // GUARDAR CAMBIOS DE UBICACIÓN
            // ==========================================

            await _context.SaveChangesAsync();


            // ==========================================
            // SINCRONIZAR MATERIAL
            // ==========================================

            await SyncMaterialInventory(
                material);


            await _context.SaveChangesAsync();


            // ==========================================
            // REGISTRAR MOVIMIENTO
            // ==========================================

            var movement =
                new Movement
                {
                    ReportNumber =
                        string.Empty,

                    MovementDate =
                        DateTime.Now,

                    UserId =
                        currentUser.Id,

                    UserName =
                        currentUser.UserName
                        ?? string.Empty,

                    RegisteredByName =
                        currentUser.FullName,

                    MaterialId =
                        material.Id,

                    MovementType =
                        model.MovementType,

                    Quantity =
                        model.Quantity,

                    OriginLocation =
                        originLocation,

                    DestinationLocation =
                        destinationLocation,

                    Comments =
                        model.Comments,

                    ReceivedByUserId =
                        receiver.Id,

                    ReceivedByName =
                        receiver.FullName,

                    // UserName = Número de Empleado
                    ReceivedByEmployeeNumber =
                        receiver.UserName
                };


            _context.Movements.Add(
                movement);


            await _context.SaveChangesAsync();


            // ==========================================
            // GENERAR FOLIO
            // ==========================================

            movement.ReportNumber =
                $"MOV-{DateTime.Now:yyyyMMdd}-" +
                $"{movement.Id:D6}";


            await _context.SaveChangesAsync();


            // ==========================================
            // RESULTADO
            // ==========================================

            TempData["Success"] =
                $"Movimiento registrado correctamente. " +
                $"Folio: {movement.ReportNumber}";


            return RedirectToAction(
                nameof(Index));
        }

        #endregion

        #region History

        public async Task<IActionResult> History(
            string? partNumber,
            string? employeeSearch,
            MovementType? movementType,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            var query = _context.Movements
                .Include(x => x.Material)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(partNumber))
            {
                query = query.Where(x =>
                    x.Material.PartNumber.Contains(partNumber));
            }

            if (!string.IsNullOrWhiteSpace(employeeSearch))
            {
                query = query.Where(x =>

                    (x.UserName ?? "")
                        .Contains(employeeSearch) ||

                    (x.ReceivedByEmployeeNumber ?? "")
                        .Contains(employeeSearch) ||

                    (x.RegisteredByName ?? "")
                        .Contains(employeeSearch) ||

                    (x.ReceivedByName ?? "")
                        .Contains(employeeSearch)
                );
            }

            if (movementType.HasValue)
            {
                query = query.Where(x =>
                    x.MovementType == movementType);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x =>
                    x.MovementDate >= fromDate);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);

                query = query.Where(x =>
                    x.MovementDate < endDate);
            }

            const int pageSize = 50;

            var totalRecords = await query.CountAsync();

            var totalEntries = await query
                .CountAsync(x =>
                    x.MovementType == MovementType.Entry);

            var totalTransfers = await query
                .CountAsync(x =>
                    x.MovementType == MovementType.Transfer);

            var totalOutputs = await query
                .CountAsync(x =>
                    x.MovementType == MovementType.Output);

            var totalMovedPieces = await query
                .SumAsync(x => x.Quantity);

            var movements = await query
                .OrderByDescending(x => x.MovementDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(new MovementHistoryVM
            {
                PartNumber = partNumber,

                EmployeeSearch = employeeSearch,

                MovementType = movementType,

                FromDate = fromDate,

                ToDate = toDate,

                Movements = movements,

                CurrentPage = page,

                PageSize = pageSize,

                TotalRecords = totalRecords,

                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)pageSize),

                TotalMovements = totalRecords,

                TotalEntries = totalEntries,

                TotalTransfers = totalTransfers,

                TotalOutputs = totalOutputs,

                TotalMovedPieces = totalMovedPieces
            });
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> GetMaterialInfo(int id)
        {
            var material = await _context.Materials
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Description,
                    x.Quantity,
                    x.Rack
                })
                .FirstOrDefaultAsync();

            if (material == null)
            {
                return Json(null);
            }

            return Json(material);
        }

        #region Helpers

        private async Task LoadMaterialsList()
        {
            ViewBag.MaterialsList = await _context.Materials
                .AsNoTracking()
                .OrderBy(x => x.PartNumber)
                .Select(x => new
                {
                    x.Id,
                    x.PartNumber,
                    x.Description,
                    x.Quantity,
                    x.Rack
                })
                .ToListAsync();
        }
        #endregion


        #region Usuarios por movimiento

        [HttpGet]
        public IActionResult GetReceivers(int movementType)
        {
            IQueryable<ApplicationUser> query =
                _userManager.Users;

            switch (movementType)
            {
                // Entrada
                case 1:

                    query = query.Where(x =>
                        x.Area == "Warehouse");

                    break;

                // Transferencia
                case 2:

                    query = query.Where(x =>
                        x.Area == "Production Line");

                    break;

                // Salida
                case 3:

                    query = query.Where(x =>
                        x.Area == "Engineering"
                        || x.Area == "Quality");

                    break;
            }

            return Json(
                query.Select(x => new
                {
                    id = x.Id,

                    name = x.FullName,

                    employeeNumber = x.UserName
                }).ToList());
        }

        #endregion

        #region

        [HttpGet]
        public async Task<IActionResult> GetMaterialLocations(int materialId)
        {
            var locations = await _context.MaterialLocations
                .Where(x =>
                    x.MaterialId == materialId &&
                    x.Quantity > 0)
                .OrderBy(x => x.LocationCode)
                .Select(x => new
                {
                    code = x.LocationCode,
                    quantity = x.Quantity
                })
                .ToListAsync();

            return Json(locations);
        }

        #endregion

        #region Sync Material Inventory

        private async Task SyncMaterialInventory(
            Material material)
        {
            var locations =
                await _context.MaterialLocations
                    .Where(x =>
                        x.MaterialId == material.Id)
                    .ToListAsync();


            material.Quantity =
                locations.Sum(x =>
                    x.Quantity);


            var primaryLocation =
                locations
                    .Where(x =>
                        x.Quantity > 0)
                    .OrderByDescending(x =>
                        x.Quantity)
                    .ThenBy(x =>
                        x.LocationCode)
                    .FirstOrDefault();


            material.Rack =
                primaryLocation?.LocationCode
                ?? string.Empty;
        }

        #endregion

        #region Qr
        [HttpGet]
        public async Task<IActionResult> GetMaterialByQr(
    string qrContent)
        {
            if (string.IsNullOrWhiteSpace(qrContent))
            {
                return BadRequest();
            }

            string? partNumber = null;

            var pairs = qrContent.Split(';');

            foreach (var pair in pairs)
            {
                var values = pair.Split('=');

                if (values.Length != 2)
                    continue;

                if (values[0].Trim().Equals("PN",
                    StringComparison.OrdinalIgnoreCase))
                {
                    partNumber = values[1].Trim();
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(partNumber))
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Where(x => x.PartNumber == partNumber)
                .Select(x => new
                {
                    id = x.Id,
                    partNumber = x.PartNumber
                })
                .FirstOrDefaultAsync();

            if (material == null)
            {
                return NotFound();
            }

            return Json(material);
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterialByPartNumber(
    string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return NotFound();

            var value = partNumber.Trim();

            if (value.Contains("PN=",
                StringComparison.OrdinalIgnoreCase))
            {
                string? pn = null;

                var pairs = value.Split(';');

                foreach (var pair in pairs)
                {
                    var items = pair.Split('=');

                    if (items.Length != 2)
                        continue;

                    if (items[0].Trim().Equals(
                        "PN",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        pn = items[1].Trim();
                        break;
                    }
                }

                value = pn ?? value;
            }

            var material = await _context.Materials
                .Where(x => x.PartNumber == value)
                .Select(x => new
                {
                    id = x.Id,
                    partNumber = x.PartNumber
                })
                .FirstOrDefaultAsync();

            if (material == null)
                return NotFound();

            return Json(material);
        }


        #endregion

        [HttpGet]
        public async Task<IActionResult> ExportPdf(
    DateTime? date,
    DateTime? fromDate,
    DateTime? toDate)
        {
            var query = _context.Movements
                .Include(x => x.Material)
                .AsQueryable();

            DateTime startDate;
            DateTime endDate;

            // Fecha específica

            if (date.HasValue)
            {
                startDate = date.Value.Date;

                endDate = startDate.AddDays(1);

                query = query.Where(x =>
                    x.MovementDate >= startDate &&
                    x.MovementDate < endDate);
            }

            // Periodo

            else if (fromDate.HasValue &&
                     toDate.HasValue)
            {
                startDate = fromDate.Value.Date;

                endDate = toDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.MovementDate >= startDate &&
                    x.MovementDate < endDate);
            }

            // Hoy

            else
            {
                startDate = DateTime.Today;

                endDate = startDate.AddDays(1);

                query = query.Where(x =>
                    x.MovementDate >= startDate &&
                    x.MovementDate < endDate);
            }

            var movements = await query
                .OrderByDescending(x => x.MovementDate)
                .ToListAsync();

            var currentUser =
                await _userManager.GetUserAsync(User);

            var model = new MovementReportVM
            {
                Date = startDate,

                EndDate =
    fromDate.HasValue && toDate.HasValue
        ? toDate.Value.Date
        : null,

                GeneratedBy =
                    currentUser?.FullName ??
                    User.Identity?.Name ??
                    "Usuario",

                GeneratedAt = DateTime.Now,

                TotalMovements = movements.Count,

                TotalEntries =
                    movements.Count(x =>
                        x.MovementType ==
                        MovementType.Entry),

                TotalTransfers =
                    movements.Count(x =>
                        x.MovementType ==
                        MovementType.Transfer),

                TotalOutputs =
                    movements.Count(x =>
                        x.MovementType ==
                        MovementType.Output),

                TotalMovedPieces =
                    movements.Sum(x => x.Quantity),

                Movements = movements
            };

            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "reichert-ai-logo-650.png");

            var document =
                new MovementsPdfDocument(
                    model,
                    logoPath);

            var pdf =
                document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Movimientos_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }
    }
}