using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Vestigium.Data;
using Vestigium.Models;
using Vestigium.Models.Enums;
using Vestigium.Models.ViewModels;
using Vestigium.Constants;

namespace Vestigium.Controllers
{
    [Authorize]
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public MaterialsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        #region QR

        public async Task<IActionResult> DownloadQr(int id)
        {
            var material = await _context.Materials
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


            if (material == null)
            {
                return NotFound();
            }


            string qrContent =
    $"PN={material.PartNumber};" +
    $"DESC={material.Description}";


            using var qrGenerator =
                new QRCodeGenerator();


            var qrData =
                qrGenerator.CreateQrCode(
                    qrContent,
                    QRCodeGenerator.ECCLevel.Q);


            var pngQr =
                new PngByteQRCode(qrData);


            byte[] bytes =
                pngQr.GetGraphic(20);


            return File(
                bytes,
                "image/png",
                $"QR_{material.PartNumber}.png");
        }

        #endregion


        #region Index

        public async Task<IActionResult> Index(
            string? searchText,
            string? rack,
            string? partNumber,
            int? productId,
            string? quantityOrder,
            int page = 1)
        {
            // ==========================================
            // CONSULTA BASE
            // ==========================================

            var query =
                _context.Materials
                    .AsNoTracking()
                    .AsQueryable();


            // ==========================================
            // BÚSQUEDA GENERAL
            // ==========================================

            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                query = query.Where(x =>
                    x.PartNumber.Contains(searchText) ||
                    x.Description.Contains(searchText));
            }


            // ==========================================
            // NÚMERO DE PARTE
            // ==========================================

            if (!string.IsNullOrWhiteSpace(
                partNumber))
            {
                query = query.Where(x =>
                    x.PartNumber.Contains(partNumber));
            }


            // ==========================================
            // PRODUCTO BOM
            // ==========================================

            if (productId.HasValue)
            {
                query = query.Where(material =>
                    _context.ProductBoms.Any(bom =>
                        bom.MaterialId == material.Id &&
                        bom.ProductId == productId.Value));
            }


            // ==========================================
            // UBICACIÓN PRINCIPAL
            // ==========================================

            if (!string.IsNullOrWhiteSpace(
                rack))
            {
                query = query.Where(x =>
                    x.Rack.StartsWith(rack));
            }


            // ==========================================
            // ORDEN
            // ==========================================

            query = quantityOrder switch
            {
                "desc" =>
                    query.OrderByDescending(x =>
                        x.Quantity),

                "asc" =>
                    query.OrderBy(x =>
                        x.Quantity),

                _ =>
                    query.OrderBy(x =>
                        x.PartNumber)
            };


            // ==========================================
            // PART NUMBERS PARA DATALIST
            // ==========================================

            ViewBag.PartNumbers =
                await _context.Materials
                    .AsNoTracking()
                    .Select(x =>
                        x.PartNumber)
                    .Distinct()
                    .OrderBy(x =>
                        x)
                    .ToListAsync();


            // ==========================================
            // PRODUCTOS QUE TIENEN BOM
            // ==========================================

            ViewBag.BomProducts =
                await _context.ProductBoms
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.ProductId,

                        ProductNumber =
                            x.Product.ProductNumber
                    })
                    .Distinct()
                    .OrderBy(x =>
                        x.ProductNumber)
                    .ToListAsync();


            // ==========================================
            // PAGINACIÓN
            // ==========================================

            const int pageSize =
                50;


            var totalRecords =
                await query.CountAsync();


            var totalPages =
                (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);


            page =
                Math.Max(
                    1,
                    page);


            if (
                totalPages > 0 &&
                page > totalPages
            )
            {
                page =
                    totalPages;
            }


            // ==========================================
            // INDICADORES GLOBALES
            // ==========================================

            var globalMaterials =
                await _context.Materials
                    .CountAsync();


            var totalInventory =
                await _context.Materials
                    .SumAsync(x =>
                        x.Quantity);


            var criticalMaterials =
                await _context.Materials
                    .CountAsync(x =>
                        x.Quantity <= 10);


            // ==========================================
            // MATERIALES DE LA PÁGINA
            // ==========================================

            var materials =
                await query
                    .Skip(
                        (page - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .Select(x =>
                        new MaterialInventoryItemVM
                        {
                            Id =
                                x.Id,

                            PartNumber =
                                x.PartNumber,

                            Description =
                                x.Description,

                            Quantity =
                                x.Quantity,

                            Rack =
                                x.Rack
                        })
                    .ToListAsync();


            // ==========================================
            // PRODUCTOS DONDE SE UTILIZA
            // CADA MATERIAL DE LA PÁGINA
            // ==========================================

            var materialIds =
                materials
                    .Select(x =>
                        x.Id)
                    .ToList();


            if (materialIds.Count > 0)
            {
                var productRelations =
                    await _context.ProductBoms
                        .AsNoTracking()
                        .Where(x =>
                            materialIds.Contains(
                                x.MaterialId))
                        .Select(x => new
                        {
                            x.MaterialId,

                            ProductNumber =
                                x.Product.ProductNumber
                        })
                        .Distinct()
                        .OrderBy(x =>
                            x.ProductNumber)
                        .ToListAsync();


                var productsByMaterial =
                    productRelations
                        .GroupBy(x =>
                            x.MaterialId)
                        .ToDictionary(
                            group =>
                                group.Key,

                            group =>
                                group
                                    .Select(x =>
                                        x.ProductNumber)
                                    .ToList()
                        );


                foreach (var material in materials)
                {
                    if (productsByMaterial.TryGetValue(
                        material.Id,
                        out var products))
                    {
                        material.UsedInProducts =
                            products;
                    }
                }
            }


            // ==========================================
            // INVENTARIO DE LA PÁGINA
            // ==========================================

            var currentPageInventory =
                materials.Sum(x =>
                    x.Quantity);


            // ==========================================
            // VIEWMODEL
            // ==========================================

            var viewModel =
                new MaterialSearchVM
                {
                    SearchText =
                        searchText,

                    Rack =
                        rack,

                    PartNumber =
                        partNumber,

                    ProductId =
                        productId,

                    QuantityOrder =
                        quantityOrder,

                    Materials =
                        materials,

                    GlobalMaterials =
                        globalMaterials,

                    TotalRecords =
                        totalRecords,

                    TotalInventory =
                        totalInventory,

                    CriticalMaterials =
                        criticalMaterials,

                    CurrentPageInventory =
                        currentPageInventory,

                    CurrentPage =
                        page,

                    PageSize =
                        pageSize,

                    TotalPages =
                        totalPages
                };


            return View(
                viewModel);
        }

        #endregion


        #region Create

        public IActionResult Create()
        {
            ViewBag.RackLocations =
                WarehouseLocations.GetLocations();


            var model =
                new MaterialCreateVM();


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MaterialCreateVM model)
        {
            // ==========================================
            // VALIDACIÓN DEL MODELO
            // ==========================================

            if (!ModelState.IsValid)
            {
                ViewBag.RackLocations =
                    WarehouseLocations.GetLocations();


                return View(model);
            }


            // ==========================================
            // VALIDAR UBICACIÓN
            // ==========================================

            if (!WarehouseLocations.IsValid(
                model.Rack))
            {
                ModelState.AddModelError(
                    nameof(model.Rack),
                    "Debe seleccionar una ubicación válida de almacén.");


                ViewBag.RackLocations =
                    WarehouseLocations.GetLocations();


                return View(model);
            }


            // ==========================================
            // VALIDAR PART NUMBER DUPLICADO
            // ==========================================

            var exists =
                await _context.Materials
                    .AnyAsync(x =>
                        x.PartNumber ==
                        model.PartNumber);


            if (exists)
            {
                ViewBag.RackLocations =
                    WarehouseLocations.GetLocations();


                ModelState.AddModelError(
                    nameof(model.PartNumber),
                    "El número de parte ya existe.");


                return View(model);
            }


            // ==========================================
            // NORMALIZAR UBICACIÓN
            // ==========================================

            var normalizedRack =
                model.Rack
                    .Trim()
                    .ToUpperInvariant();


            // ==========================================
            // CREAR MATERIAL
            // ==========================================

            var material =
                new Material
                {
                    PartNumber =
                        model.PartNumber,

                    Description =
                        model.Description,

                    Quantity =
                        model.Quantity,

                    Rack =
                        normalizedRack,

                    Barcode =
                        $"PN={model.PartNumber};" +
                        $"DESC={model.Description}"
                };


            _context.Materials.Add(
                material);


            await _context.SaveChangesAsync();


            // ==========================================
            // UBICACIÓN INICIAL
            // ==========================================

            var location =
                new MaterialLocation
                {
                    MaterialId =
                        material.Id,

                    LocationCode =
                        normalizedRack,

                    Quantity =
                        model.Quantity
                };


            _context.MaterialLocations.Add(
                location);


            await _context.SaveChangesAsync();


            // ==========================================
            // SINCRONIZAR MATERIAL
            // ==========================================

            await SyncMaterial(
                material);


            await _context.SaveChangesAsync();


            // ==========================================
            // RESULTADO
            // ==========================================

            TempData["Success"] =
                "Material creado correctamente.";


            return RedirectToAction(
                nameof(Index));
        }

        #endregion


        #region Edit

        public async Task<IActionResult> Edit(int id)
        {
            var material =
                await _context.Materials
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            var vm =
                new MaterialEditVM
                {
                    Id = material.Id,
                    PartNumber = material.PartNumber,
                    Description = material.Description,
                    Quantity = material.Quantity,
                    Rack = material.Rack,
                    Barcode = material.Barcode
                };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    MaterialEditVM model)
        {
            var material =
                await _context.Materials
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.Id);

            if (material == null)
            {
                return NotFound();
            }


            // Información controlada por el servidor

            model.Quantity =
                material.Quantity;

            model.Rack =
                material.Rack;

            model.Barcode =
                material.Barcode;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Validar Part Number duplicado

            var duplicatePartNumber =
                await _context.Materials
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id != model.Id &&
                        x.PartNumber == model.PartNumber);


            if (duplicatePartNumber)
            {
                ModelState.AddModelError(
                    nameof(model.PartNumber),
                    "El número de parte ya pertenece a otro material.");

                return View(model);
            }


            // Solamente estos datos son editables

            material.PartNumber =
                model.PartNumber;

            material.Description =
                model.Description;


            material.Barcode =
                $"PN={model.PartNumber};" +
                $"DESC={model.Description}";


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Material actualizado correctamente.";


            return RedirectToAction(
                nameof(Index));
        }

        #endregion


        #region Scrap / MRB

        public async Task<IActionResult> ScrapMrb()
        {
            var result =
                await _context.Materials
                    .Select(material =>
                        new ScrapMrbInventoryVM
                        {
                            PartNumber =
                                material.PartNumber,

                            Description =
                                material.Description,

                            ScrapQuantity =
                                _context.Movements
                                    .Where(x =>
                                        x.MaterialId == material.Id &&
                                        x.MovementType ==
                                            MovementType.Output &&
                                        x.DestinationLocation ==
                                            "Scrap")
                                    .Sum(x =>
                                        (int?)x.Quantity)
                                ?? 0,

                            MrbQuantity =
                                _context.Movements
                                    .Where(x =>
                                        x.MaterialId == material.Id &&
                                        x.MovementType ==
                                            MovementType.Output &&
                                        x.DestinationLocation ==
                                            "MRB")
                                    .Sum(x =>
                                        (int?)x.Quantity)
                                ?? 0,

                            LastMovementDate =
                                _context.Movements
                                    .Where(x =>
                                        x.MaterialId == material.Id &&
                                        x.MovementType ==
                                            MovementType.Output &&
                                        (
                                            x.DestinationLocation == "Scrap" ||
                                            x.DestinationLocation == "MRB"
                                        ))
                                    .OrderByDescending(x =>
                                        x.MovementDate)
                                    .Select(x =>
                                        (DateTime?)
                                        x.MovementDate)
                                    .FirstOrDefault(),

                            LastMovementBy =
                                _context.Movements
                                    .Where(x =>
                                        x.MaterialId == material.Id &&
                                        x.MovementType ==
                                            MovementType.Output &&
                                        (
                                            x.DestinationLocation == "Scrap" ||
                                            x.DestinationLocation == "MRB"
                                        ))
                                    .OrderByDescending(x =>
                                        x.MovementDate)
                                    .Select(x =>
                                        x.RegisteredByName)
                                    .FirstOrDefault()
                        })
                    .Where(x =>
                        x.ScrapQuantity > 0 ||
                        x.MrbQuantity > 0)
                    .OrderBy(x =>
                        x.PartNumber)
                    .ToListAsync();


            return View(result);
        }

        #endregion


        #region Traceability

        public async Task<IActionResult> Traceability(
            int id)
        {
            var material =
                await _context.Materials
                    .Include(x =>
                        x.Locations)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);


            if (material == null)
            {
                return NotFound();
            }


            var movements =
                await _context.Movements
                    .AsNoTracking()
                    .Where(x =>
                        x.MaterialId == id)
                    .OrderByDescending(x =>
                        x.MovementDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .ToListAsync();


            var vm =
                new TraceabilityVM
                {
                    MaterialId =
                        material.Id,

                    PartNumber =
                        material.PartNumber,

                    Description =
                        material.Description,

                    CurrentInventory =
                        material.Quantity,

                    Rack =
                        material.Rack,

                    Movements =
                        movements,

                    Locations =
                        material.Locations
                            .Where(x =>
                                x.Quantity > 0)
                            .OrderByDescending(x =>
                                x.Quantity)
                            .ThenBy(x =>
                                x.LocationCode)
                            .ToList()
                };


            return View(vm);
        }

        #endregion


        #region Material Info

        [HttpGet]
        public async Task<IActionResult> GetMaterialInfo(
            string partNumber)
        {
            if (string.IsNullOrWhiteSpace(
                partNumber))
            {
                return Json(null);
            }


            var material =
                await _context.Materials
                    .AsNoTracking()
                    .Where(x =>
                        x.PartNumber == partNumber)
                    .Select(x =>
                        new
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


            return Json(
                material);
        }

        #endregion


        #region Locations

        public async Task<IActionResult> Locations(
            int id)
        {
            var material =
                await _context.Materials
                    .Include(x =>
                        x.Locations)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);


            if (material == null)
            {
                return NotFound();
            }


            var vm =
                new MaterialLocationVM
                {
                    MaterialId =
                        material.Id,

                    PartNumber =
                        material.PartNumber,

                    Description =
                        material.Description,

                    Locations =
                        material.Locations
                            .OrderBy(x =>
                                x.LocationCode)
                            .ToList()
                };


            return View(
                vm);
        }

        #endregion


        
        #region Transfer Location

        public async Task<IActionResult> TransferLocation(
            int id)
        {
            var material =
                await _context.Materials
                    .Include(x =>
                        x.Locations)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);


            if (material == null)
            {
                return NotFound();
            }


            ViewBag.Locations =
                material.Locations
                    .Where(x =>
                        x.Quantity > 0)
                    .OrderBy(x =>
                        x.LocationCode)
                    .ToList();


            var vm =
                new TransferLocationVM
                {
                    MaterialId =
                        material.Id,

                    PartNumber =
                        material.PartNumber
                };


            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferLocation(
            TransferLocationVM model)
        {
            var material =
                await _context.Materials
                    .Include(x =>
                        x.Locations)
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        model.MaterialId);


            if (material == null)
            {
                return NotFound();
            }


            // ==========================================
            // INFORMACIÓN CONTROLADA POR EL SERVIDOR
            // ==========================================

            model.PartNumber =
                material.PartNumber;


            // ==========================================
            // UBICACIONES DISPONIBLES
            // ==========================================

            ViewBag.Locations =
                material.Locations
                    .Where(x =>
                        x.Quantity > 0)
                    .OrderBy(x =>
                        x.LocationCode)
                    .ToList();


            // ==========================================
            // VALIDACIÓN DEL MODELO
            // ==========================================

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ==========================================
            // VALIDAR UBICACIÓN DESTINO
            // ==========================================

            if (!WarehouseLocations.IsValid(
                model.DestinationRack,
                model.DestinationLevel))
            {
                ModelState.AddModelError(
                    nameof(model.DestinationRack),
                    "Seleccione una ubicación destino válida.");


                return View(model);
            }


            // ==========================================
            // CONSTRUIR UBICACIÓN DESTINO
            // ==========================================

            model.DestinationLocation =
                WarehouseLocations.BuildLocationCode(
                    model.DestinationRack,
                    model.DestinationLevel)
                ?? string.Empty;


            // ==========================================
            // ORIGEN Y DESTINO
            // ==========================================

            if (
                model.OriginLocation ==
                model.DestinationLocation
            )
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Origen y destino no pueden ser iguales.");


                return View(model);
            }


            // ==========================================
            // UBICACIÓN ORIGEN
            // ==========================================

            var origin =
                material.Locations
                    .FirstOrDefault(x =>
                        x.LocationCode ==
                        model.OriginLocation);


            if (origin == null)
            {
                ModelState.AddModelError(
                    nameof(model.OriginLocation),
                    "Ubicación origen no encontrada.");


                return View(model);
            }


            // ==========================================
            // VALIDAR INVENTARIO
            // ==========================================

            if (
                origin.Quantity <
                model.Quantity
            )
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "La cantidad excede el inventario disponible en la ubicación origen.");


                return View(model);
            }


            // ==========================================
            // USUARIO ACTUAL
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
            // UBICACIÓN DESTINO
            // ==========================================

            var destination =
                material.Locations
                    .FirstOrDefault(x =>
                        x.LocationCode ==
                        model.DestinationLocation);


            if (destination == null)
            {
                destination =
                    new MaterialLocation
                    {
                        MaterialId =
                            material.Id,

                        LocationCode =
                            model.DestinationLocation,

                        Quantity =
                            0
                    };


                _context.MaterialLocations.Add(
                    destination);
            }


            // ==========================================
            // TRANSFERENCIA FÍSICA
            // ==========================================

            origin.Quantity -=
                model.Quantity;


            destination.Quantity +=
                model.Quantity;


            // ==========================================
            // ELIMINAR ORIGEN VACÍO
            // ==========================================

            if (origin.Quantity == 0)
            {
                _context.MaterialLocations.Remove(
                    origin);
            }


            await _context.SaveChangesAsync();


            // ==========================================
            // SINCRONIZAR MATERIAL
            // ==========================================

            await SyncMaterial(
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
                        MovementType.Transfer,

                    Quantity =
                        model.Quantity,

                    OriginLocation =
                        model.OriginLocation,

                    DestinationLocation =
                        model.DestinationLocation,

                    Comments =
                        "Transferencia interna entre ubicaciones."
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
                $"Transferencia realizada correctamente. " +
                $"Folio: {movement.ReportNumber}";


            return RedirectToAction(
                nameof(Locations),
                new
                {
                    id = material.Id
                });
        }

        #endregion

        
        #region Sync Inventory

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncInventory()
        {
            var materials =
                await _context.Materials
                    .ToListAsync();


            int updated =
                0;


            foreach (var material in materials)
            {
                await SyncMaterial(
                    material);


                updated++;
            }


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"{updated} materiales sincronizados.";


            return RedirectToAction(
                nameof(Index));
        }

        #endregion


        #region Sync Material

        private async Task SyncMaterial(
            Material material)
        {
            var locations =
                await _context.MaterialLocations
                    .Where(x =>
                        x.MaterialId ==
                        material.Id)
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
    }
}