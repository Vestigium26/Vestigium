using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vestigium.Data;
using Vestigium.Models;
using Vestigium.Models.ViewModels;

namespace Vestigium.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.BomItems)
                .ThenInclude(b => b.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            // Normalizar información
            var productNumber =
                product.ProductNumber?
                    .Trim()
                    .ToUpperInvariant();

            var productDescription =
                product.ProductDescription?
                    .Trim();


            // Validar número de producto
            if (string.IsNullOrWhiteSpace(productNumber))
            {
                ModelState.AddModelError(
                    nameof(product.ProductNumber),
                    "El número de producto es obligatorio.");
            }
            else
            {
                // Validar duplicado
                var exists =
                    await _context.Products
                        .AnyAsync(x =>
                            x.ProductNumber == productNumber);

                if (exists)
                {
                    ModelState.AddModelError(
                        nameof(product.ProductNumber),
                        "Ya existe un producto registrado con este número.");
                }
            }


            // Validar descripción
            if (string.IsNullOrWhiteSpace(productDescription))
            {
                ModelState.AddModelError(
                    nameof(product.ProductDescription),
                    "La descripción del producto es obligatoria.");
            }


            if (!ModelState.IsValid)
            {
                return View(product);
            }


            // En este punto ya sabemos que no son null
            product.ProductNumber =
                productNumber!;

            product.ProductDescription =
                productDescription!;


            _context.Products.Add(product);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"Producto {product.ProductNumber} creado correctamente.";


            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }


            // Normalizar información
            var productNumber =
                product.ProductNumber?
                    .Trim()
                    .ToUpperInvariant();

            var productDescription =
                product.ProductDescription?
                    .Trim();


            // Validar número de producto
            if (string.IsNullOrWhiteSpace(productNumber))
            {
                ModelState.AddModelError(
                    nameof(product.ProductNumber),
                    "El número de producto es obligatorio.");
            }
            else
            {
                // Buscar otro producto con el mismo número.
                // Se excluye el producto que estamos editando.
                var exists =
                    await _context.Products
                        .AnyAsync(x =>
                            x.ProductNumber == productNumber &&
                            x.Id != product.Id);

                if (exists)
                {
                    ModelState.AddModelError(
                        nameof(product.ProductNumber),
                        "Ya existe otro producto registrado con este número.");
                }
            }


            // Validar descripción
            if (string.IsNullOrWhiteSpace(productDescription))
            {
                ModelState.AddModelError(
                    nameof(product.ProductDescription),
                    "La descripción del producto es obligatoria.");
            }


            // Regresar la vista si existe algún error
            if (!ModelState.IsValid)
            {
                return View(product);
            }


            // Recuperar el producto original desde la base de datos
            var existingProduct =
                await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == product.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }


            // Actualizar únicamente propiedades permitidas
            existingProduct.ProductNumber =
                productNumber!;

            existingProduct.ProductDescription =
                productDescription!;

            existingProduct.IsActive =
                product.IsActive;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"Producto {existingProduct.ProductNumber} actualizado correctamente.";


            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(x => x.BomItems)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return NotFound();

            var vm = new ProductDeleteVM
            {
                Id = product.Id,
                ProductNumber = product.ProductNumber,
                ProductDescription = product.ProductDescription,
                BomCount = product.BomItems.Count
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(ProductDeleteVM model)
        {
            // ======================================
            // OBTENER PRODUCTO
            // ======================================

            var product = await _context.Products
                .Include(x => x.BomItems)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (product == null)
            {
                return NotFound();
            }


            // ======================================
            // RECONSTRUIR INFORMACION DEL MODELO
            // ======================================

            model.ProductNumber =
                product.ProductNumber;

            model.ProductDescription =
                product.ProductDescription;

            model.BomCount =
                product.BomItems.Count;


            // ======================================
            // SEGURIDAD PARA PRODUCTOS CON BOM
            // ======================================

            if (product.BomItems.Any())
            {
                var user =
                    await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized();
                }


                // Contraseña obligatoria solamente
                // cuando el producto tiene BOM
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(
                        nameof(model.Password),
                        "Debe ingresar su contraseña para eliminar un producto con BOM.");

                    return View(model);
                }


                var validPassword =
                    await _userManager.CheckPasswordAsync(
                        user,
                        model.Password);

                if (!validPassword)
                {
                    ModelState.AddModelError(
                        nameof(model.Password),
                        "La contraseña es incorrecta.");

                    return View(model);
                }
            }


            // ======================================
            // ELIMINAR RELACIONES BOM
            // ======================================

            if (product.BomItems.Any())
            {
                _context.ProductBoms.RemoveRange(
                    product.BomItems
                );
            }


            // ======================================
            // ELIMINAR PRODUCTO
            // ======================================

            _context.Products.Remove(
                product
            );


            await _context.SaveChangesAsync();


            // ======================================
            // MENSAJE
            // ======================================

            TempData["Success"] =
                $"Producto {product.ProductNumber} eliminado correctamente.";


            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}