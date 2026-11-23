using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vestigium.Data;
using Vestigium.Models;
using Vestigium.Models.ViewModels;
using Vestigium.ViewModels;

namespace Vestigium.Controllers
{
    [Authorize]
    public class ProductBomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ProductBomController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        #region Index

        public async Task<IActionResult> Index(int productId)
        {
            var product = await _context.Products
                .Include(x => x.BomItems)
                .ThenInclude(x => x.Material)
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            var vm = new ProductBomViewModel
            {
                ProductId = product.Id,
                ProductNumber = product.ProductNumber,
                ProductDescription = product.ProductDescription,

                Items = product.BomItems
                    .Select(x => new ProductBomItemViewModel
                    {
                        ProductBomId = x.Id,
                        MaterialId = x.MaterialId,
                        MaterialNumber = x.Material.PartNumber,
                        Description = x.Material.Description,
                        Quantity = x.Quantity
                    })
                    .ToList()
            };

            return View(vm);
        }

        #endregion


        #region Create

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == productId);


            if (product == null)
            {
                return NotFound();
            }


            var requiresPassword =
                await ProductHasBomAsync(productId);


            var vm = new AddBomItemViewModel
            {
                ProductId = productId,
                RequiresPassword = requiresPassword
            };


            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
    AddBomItemViewModel vm)
        {
            // ==========================================
            // VALIDAR PRODUCTO
            // ==========================================

            var productExists =
                await _context.Products
                    .AnyAsync(x =>
                        x.Id == vm.ProductId);


            if (!productExists)
            {
                return NotFound();
            }


            // ==========================================
            // DETERMINAR SEGURIDAD DESDE EL SERVIDOR
            // ==========================================

            var requiresPassword =
                await ProductHasBomAsync(
                    vm.ProductId);


            /*
             * Nunca confiamos en RequiresPassword recibido
             * desde el navegador.
             *
             * El servidor vuelve a determinar si el BOM
             * ya existe.
             */

            vm.RequiresPassword =
                requiresPassword;


            // ==========================================
            // VALIDAR MATERIAL
            // ==========================================

            var materialExists =
                await _context.Materials
                    .AnyAsync(x =>
                        x.Id == vm.MaterialId);


            if (!materialExists)
            {
                ModelState.AddModelError(
                    nameof(vm.MaterialId),
                    "Seleccione un material válido.");
            }


            // ==========================================
            // VALIDAR DUPLICADO
            // ==========================================

            var existing =
                await _context.ProductBoms
                    .AnyAsync(x =>
                        x.ProductId == vm.ProductId &&
                        x.MaterialId == vm.MaterialId);


            if (existing)
            {
                ModelState.AddModelError(
                    "",
                    "El material ya existe en el BOM.");
            }


            // ==========================================
            // VALIDAR CANTIDAD
            // ==========================================

            if (!vm.Quantity.HasValue || vm.Quantity.Value <= 0)
            {
                ModelState.AddModelError(
                    nameof(vm.Quantity),
                    "Ingrese una cantidad mayor que cero.");
            }


            // ==========================================
            // VALIDAR CONTRASEÑA SI EL BOM YA EXISTE
            // ==========================================

            if (requiresPassword)
            {
                if (string.IsNullOrWhiteSpace(
                    vm.Password))
                {
                    ModelState.AddModelError(
                        nameof(vm.Password),
                        "Ingrese su contraseña para autorizar la modificación del BOM.");
                }
                else
                {
                    var validPassword =
                        await ValidateCurrentUserPasswordAsync(
                            vm.Password);


                    if (!validPassword)
                    {
                        ModelState.AddModelError(
                            nameof(vm.Password),
                            "La contraseña ingresada no es correcta.");
                    }
                }
            }


            // ==========================================
            // VALIDACIÓN
            // ==========================================

            if (!ModelState.IsValid)
            {
                if (vm.MaterialId > 0)
                {
                    var selectedMaterial = await _context.Materials
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == vm.MaterialId);


                    if (selectedMaterial != null)
                    {
                        vm.SelectedMaterialNumber =
                            selectedMaterial.PartNumber;

                        vm.SelectedMaterialDescription =
                            selectedMaterial.Description;
                    }
                }


                // Nunca regresar la contraseña a la vista.
                vm.Password = null;


                return View(vm);
            }


            // ==========================================
            // CREAR RELACIÓN BOM
            // ==========================================

            var item = new ProductBom
            {
                ProductId = vm.ProductId,
                MaterialId = vm.MaterialId,
                Quantity = vm.Quantity!.Value
            };


            _context.ProductBoms.Add(item);


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "El material fue agregado al BOM correctamente.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    productId = vm.ProductId
                });
        }

        #endregion


        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(
    DeleteBomItemViewModel vm)
        {
            // ==========================================
            // BUSCAR COMPONENTE
            // ==========================================

            var item = await _context.ProductBoms
                .FirstOrDefaultAsync(x =>
                    x.Id == vm.Id);


            if (item == null)
            {
                return NotFound();
            }


            var productId =
                item.ProductId;


            // ==========================================
            // VALIDAR CONTRASEÑA
            // ==========================================

            if (string.IsNullOrWhiteSpace(
                vm.Password))
            {
                TempData["DeleteBomError"] =
                    "Ingrese su contraseña para autorizar la eliminación.";

                TempData["DeleteBomId"] =
                    item.Id;

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        productId
                    });
            }


            var validPassword =
                await ValidateCurrentUserPasswordAsync(
                    vm.Password);


            if (!validPassword)
            {
                TempData["DeleteBomError"] =
                    "La contraseña ingresada no es correcta.";

                TempData["DeleteBomId"] =
                    item.Id;

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        productId
                    });
            }


            // ==========================================
            // ELIMINAR
            // ==========================================

            _context.ProductBoms.Remove(
                item);


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "El componente fue eliminado del BOM correctamente.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    productId
                });
        }

        #endregion


        #region Edit

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.ProductBoms
                .AsNoTracking()
                .Include(x => x.Material)
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


            if (item == null)
            {
                return NotFound();
            }


            var vm = new EditBomItemViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                MaterialId = item.MaterialId,

                MaterialNumber =
                    item.Material.PartNumber,

                MaterialDescription =
                    item.Material.Description,

                Quantity =
                    item.Quantity
            };


            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit (EditBomItemViewModel vm)
        {
            // ==========================================
            // BUSCAR RELACIÓN REAL
            // ==========================================

            var item = await _context.ProductBoms
                .Include(x => x.Material)
                .FirstOrDefaultAsync(x =>
                    x.Id == vm.Id);


            if (item == null)
            {
                return NotFound();
            }


            /*
             * La información estructural nunca se toma
             * como verdadera desde el navegador.
             */

            vm.ProductId =
                item.ProductId;

            vm.MaterialId =
                item.MaterialId;

            vm.MaterialNumber =
                item.Material.PartNumber;

            vm.MaterialDescription =
                item.Material.Description;


            // ==========================================
            // VALIDAR CANTIDAD
            // ==========================================

            if (vm.Quantity <= 0)
            {
                ModelState.AddModelError(
                    nameof(vm.Quantity),
                    "Ingrese una cantidad mayor que cero.");
            }


            // ==========================================
            // VALIDAR CONTRASEÑA
            // ==========================================

            if (string.IsNullOrWhiteSpace(
                vm.Password))
            {
                ModelState.AddModelError(
                    nameof(vm.Password),
                    "Ingrese su contraseña para autorizar la modificación del BOM.");
            }
            else
            {
                var validPassword =
                    await ValidateCurrentUserPasswordAsync(
                        vm.Password);


                if (!validPassword)
                {
                    ModelState.AddModelError(
                        nameof(vm.Password),
                        "La contraseña ingresada no es correcta.");
                }
            }


            // ==========================================
            // VALIDACIÓN
            // ==========================================

            if (!ModelState.IsValid)
            {
                /*
                 * Nunca devolver contraseña al navegador.
                 */

                vm.Password = null;

                return View(vm);
            }


            // ==========================================
            // ACTUALIZAR ÚNICAMENTE CANTIDAD
            // ==========================================

            item.Quantity =
                vm.Quantity;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "La cantidad del componente fue actualizada correctamente.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    productId = item.ProductId
                });
        }

        #endregion


        #region Search Materials

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchMaterials(
            string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(
                    new List<object>());
            }


            var materials = await _context.Materials
                .Where(x =>
                    x.PartNumber.Contains(term) ||
                    x.Description.Contains(term))
                .OrderBy(x => x.PartNumber)
                .Take(20)
                .Select(x => new
                {
                    id = x.Id,
                    partNumber = x.PartNumber,
                    description = x.Description
                })
                .ToListAsync();


            return Json(materials);
        }

        #endregion


        #region Bulk Import

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkImport(
            int productId)
        {
            // ==========================================
            // PRODUCTO
            // ==========================================

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == productId);


            if (product == null)
            {
                return NotFound();
            }


            // ==========================================
            // DETERMINAR SI REQUIERE CONTRASEÑA
            // ==========================================

            var requiresPassword =
                await ProductHasBomAsync(
                    productId);


            // ==========================================
            // VIEWMODEL
            // ==========================================

            var vm = new BulkBomImportViewModel
            {
                ProductId =
                    product.Id,

                ProductNumber =
                    product.ProductNumber,

                RequiresPassword =
                    requiresPassword
            };


            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkImport(
            BulkBomImportViewModel vm)
        {
            // ==========================================
            // PRODUCTO
            // ==========================================

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == vm.ProductId);


            if (product == null)
            {
                return NotFound();
            }


            // ==========================================
            // RESTAURAR INFORMACIÓN DEL PRODUCTO
            // ==========================================

            vm.ProductNumber =
                product.ProductNumber;


            // ==========================================
            // SEGURIDAD DEL BOM
            // ==========================================

            var requiresPassword =
                await ProductHasBomAsync(
                    vm.ProductId);


            /*
             * Nunca confiamos en RequiresPassword
             * recibido desde el navegador.
             *
             * El servidor determina nuevamente
             * si el producto ya tiene BOM.
             */

            vm.RequiresPassword =
                requiresPassword;


            if (requiresPassword)
            {
                // ======================================
                // CONTRASEÑA VACÍA
                // ======================================

                if (string.IsNullOrWhiteSpace(
                    vm.Password))
                {
                    ModelState.AddModelError(
                        nameof(vm.Password),
                        "Ingrese su contraseña para autorizar la modificación del BOM.");


                    vm.Password =
                        null;


                    return View(vm);
                }


                // ======================================
                // VALIDAR CONTRASEÑA
                // ======================================

                var validPassword =
                    await ValidateCurrentUserPasswordAsync(
                        vm.Password);


                if (!validPassword)
                {
                    ModelState.AddModelError(
                        nameof(vm.Password),
                        "La contraseña ingresada no es correcta.");


                    vm.Password =
                        null;


                    return View(vm);
                }
            }


            /*
             * La contraseña ya cumplió su propósito.
             * No debe conservarse en el ViewModel.
             */

            vm.Password =
                null;


            // ==========================================
            // ARCHIVO
            // ==========================================

            if (vm.File != null &&
                vm.File.Length > 0)
            {
                const long maxFileSize =
                    5 * 1024 * 1024;


                // ======================================
                // VALIDAR TAMAÑO
                // ======================================

                if (vm.File.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        nameof(vm.File),
                        "El archivo no puede exceder 5 MB.");


                    return View(vm);
                }


                // ======================================
                // VALIDAR EXTENSIÓN
                // ======================================

                var extension =
                    Path.GetExtension(
                        vm.File.FileName)
                    .ToLowerInvariant();


                var allowedExtensions =
                    new[]
                    {
                ".txt",
                ".csv"
                    };


                if (!allowedExtensions.Contains(
                    extension))
                {
                    ModelState.AddModelError(
                        nameof(vm.File),
                        "El archivo debe estar en formato TXT o CSV.");


                    return View(vm);
                }


                // ======================================
                // LEER ARCHIVO
                // ======================================

                using var reader =
                    new StreamReader(
                        vm.File.OpenReadStream());


                vm.BomText =
                    await reader.ReadToEndAsync();
            }


            // ==========================================
            // VALIDAR ORIGEN DE DATOS
            // ==========================================

            if (string.IsNullOrWhiteSpace(
                vm.BomText))
            {
                ModelState.AddModelError(
                    "",
                    "Pegue una lista de materiales o seleccione un archivo para importar.");


                return View(vm);
            }


            // ==========================================
            // NORMALIZAR LÍNEAS
            // ==========================================

            var lines = vm.BomText
                .Split(
                    new[]
                    {
                "\r\n",
                "\n",
                "\r"
                    },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .ToArray();


            vm.TotalLines =
                lines.Length;


            // ==========================================
            // REINICIAR RESULTADOS
            // ==========================================

            vm.AddedCount =
                0;

            vm.UpdatedCount =
                0;

            vm.DuplicateCount =
                0;

            vm.InvalidCount =
                0;

            vm.NotFound.Clear();


            // ==========================================
            // PROCESAR LÍNEAS
            // ==========================================

            foreach (var line in lines)
            {
                // ======================================
                // PARSEAR LÍNEA
                // ======================================

                var parsed =
                    TryParseBomLine(
                        line,
                        out var partNumber,
                        out var quantity);


                // ======================================
                // LÍNEA INVÁLIDA
                // ======================================

                if (!parsed)
                {
                    vm.InvalidCount++;

                    continue;
                }


                // ======================================
                // BUSCAR MATERIAL
                // ======================================

                var material = await _context.Materials
                    .FirstOrDefaultAsync(x =>
                        x.PartNumber == partNumber);


                // ======================================
                // MATERIAL NO ENCONTRADO
                // ======================================

                if (material == null)
                {
                    if (!vm.NotFound.Contains(
                        partNumber,
                        StringComparer.OrdinalIgnoreCase))
                    {
                        vm.NotFound.Add(
                            partNumber);
                    }


                    continue;
                }


                // ======================================
                // BUSCAR COMPONENTE EXISTENTE
                // ======================================

                var existing =
                    await _context.ProductBoms
                        .FirstOrDefaultAsync(x =>
                            x.ProductId == vm.ProductId &&
                            x.MaterialId == material.Id);


                // ======================================
                // MATERIAL YA PERTENECE AL BOM
                // ======================================

                if (existing != null)
                {
                    // ==================================
                    // ACTUALIZAR EXISTENTE
                    // ==================================

                    if (vm.UpdateExisting)
                    {
                        existing.Quantity =
                            quantity;


                        vm.UpdatedCount++;
                    }
                    else
                    {
                        // ==============================
                        // OMITIR DUPLICADO
                        // ==============================

                        vm.DuplicateCount++;
                    }


                    continue;
                }


                // ======================================
                // AGREGAR NUEVO COMPONENTE
                // ======================================

                _context.ProductBoms.Add(
                    new ProductBom
                    {
                        ProductId =
                            vm.ProductId,

                        MaterialId =
                            material.Id,

                        Quantity =
                            quantity
                    });


                vm.AddedCount++;
            }


            // ==========================================
            // GUARDAR CAMBIOS
            // ==========================================

            await _context.SaveChangesAsync();


            return View(vm);
        }


        // =================================================
        // PARSER BOM
        // =================================================

        private static bool TryParseBomLine(
            string line,
            out string partNumber,
            out decimal quantity)
        {
            partNumber =
                string.Empty;

            quantity =
                0;


            // ==========================================
            // LÍNEA VACÍA
            // ==========================================

            if (string.IsNullOrWhiteSpace(
                line))
            {
                return false;
            }


            string[] parts;


            // ==========================================
            // COMA
            // ==========================================

            if (line.Contains(','))
            {
                parts = line.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries);
            }


            // ==========================================
            // PUNTO Y COMA
            // ==========================================

            else if (line.Contains(';'))
            {
                parts = line.Split(
                    ';',
                    StringSplitOptions.RemoveEmptyEntries);
            }


            // ==========================================
            // ESPACIOS / TABULACIÓN
            // ==========================================

            else
            {
                parts = line.Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries);
            }


            // ==========================================
            // COLUMNAS
            // ==========================================

            if (parts.Length < 2)
            {
                return false;
            }


            // ==========================================
            // NÚMERO DE PARTE
            // ==========================================

            partNumber =
                parts[0].Trim();


            if (string.IsNullOrWhiteSpace(
                partNumber))
            {
                return false;
            }


            // ==========================================
            // CANTIDAD
            // ==========================================

            if (!decimal.TryParse(
                parts[1].Trim(),
                out quantity))
            {
                return false;
            }


            if (quantity <= 0)
            {
                return false;
            }


            return true;
        }

        #endregion


        #region Download Not Found

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DownloadNotFound(
            List<string> notFound)
        {
            if (notFound == null ||
                !notFound.Any())
            {
                return RedirectToAction(
                    nameof(Index));
            }


            var content =
                string.Join(
                    Environment.NewLine,
                    notFound);


            return File(
                System.Text.Encoding.UTF8.GetBytes(
                    content),
                "text/plain",
                "MaterialsNotFound.txt");
        }

        #endregion


        #region Explosion

        [Authorize]
        public async Task<IActionResult> Explosion(
            int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == productId);


            if (product == null)
            {
                return NotFound();
            }


            var vm =
                new BomExplosionViewModel
                {
                    ProductId = product.Id,
                    ProductNumber = product.ProductNumber,
                    ProductDescription = product.ProductDescription,
                    BuildQuantity = 1
                };


            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Explosion(
            BomExplosionViewModel vm)
        {
            var product = await _context.Products
                .Include(x => x.BomItems)
                .ThenInclude(x => x.Material)
                .FirstOrDefaultAsync(x =>
                    x.Id == vm.ProductId);


            if (product == null)
            {
                return NotFound();
            }


            vm.ProductNumber =
                product.ProductNumber;

            vm.ProductDescription =
                product.ProductDescription;


            vm.Materials = product.BomItems
                .OrderBy(x =>
                    x.Material.PartNumber)
                .Select(x =>
                {
                    var required =
                        x.Quantity *
                        vm.BuildQuantity;


                    var available =
                        x.Material.Quantity;


                    return new BomExplosionItemViewModel
                    {
                        PartNumber =
                            x.Material.PartNumber,

                        Description =
                            x.Material.Description,

                        BomQuantity =
                            x.Quantity,

                        RequiredQuantity =
                            required,

                        AvailableQuantity =
                            available,

                        ShortageQuantity =
                            Math.Max(
                                0,
                                required - available)
                    };
                })
                .ToList();


            return View(vm);
        }

        #endregion

        #region Security Helpers

        private async Task<bool> ProductHasBomAsync(
            int productId)
        {
            return await _context.ProductBoms
                .AnyAsync(x =>
                    x.ProductId == productId);
        }


        private async Task<bool> ValidateCurrentUserPasswordAsync(
            string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }


            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return false;
            }


            return await _userManager.CheckPasswordAsync(
                user,
                password);
        }

        #endregion
    }
}