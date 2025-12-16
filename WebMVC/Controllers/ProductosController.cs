using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace WebMVC.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ProductoCEN _productoCEN;
        private readonly CategoriaCEN _categoriaCEN;
        private readonly IWebHostEnvironment _env;

        public ProductosController(ProductoCEN productoCEN, CategoriaCEN categoriaCEN, IWebHostEnvironment env)
        {
            _productoCEN = productoCEN;
            _categoriaCEN = categoriaCEN;
            _env = env;
        }

        // Verificar si el usuario es Admin
        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            return string.Equals(rol, "admin", StringComparison.OrdinalIgnoreCase);
        }

        // GET: Productos (con filtros HQL) - SOLO ADMIN
        public IActionResult Index(string color, decimal? precioMax, bool? destacado)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Tienda");
            }

            IList<Producto> productos = _productoCEN.ReadFilter(
                color: color,
                precioMax: precioMax,
                destacado: destacado
            );
            
            ViewBag.ColorFilter = color;
            ViewBag.PrecioMaxFilter = precioMax;
            ViewBag.DestacadoFilter = destacado;
            
            return View(productos);
        }

        // GET: Productos/Details/5
        public IActionResult Details(long? id)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Tienda");
            if (id == null) return NotFound();
            Producto producto = _productoCEN.ReadOID(id.Value);
            if (producto == null) return NotFound();
            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Tienda");
            ViewBag.Categorias = new SelectList(_categoriaCEN.ReadAll(), "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Nombre,Descripcion,Precio,Stock,Destacado,Color")] Producto producto, IFormFile? fotoArchivo)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Tienda");
            try
            {
                if (ModelState.IsValid)
                {
                    var fotosList = GuardarFoto(fotoArchivo);
                    var created = _productoCEN.Crear(producto.Nombre, producto.Precio, producto.Stock, producto.Destacado, fotosList);
                    
                    // Actualizar descripción y color después de crear
                    if (!string.IsNullOrEmpty(producto.Descripcion) || !string.IsNullOrEmpty(producto.Color))
                    {
                        _productoCEN.Modify(created.Id, producto.Nombre, producto.Precio, producto.Stock, producto.Destacado, producto.Descripcion ?? "", fotos: fotosList);
                        if (!string.IsNullOrEmpty(producto.Color))
                            _productoCEN.ActualizarColor(created.Id, producto.Color);
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear el producto: {ex.Message}");
            }
            
            ViewBag.Categorias = new SelectList(_categoriaCEN.ReadAll(), "Id", "Nombre");
            return View(producto);
        }

        // GET: Productos/Edit/5
        public IActionResult Edit(long? id)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Tienda");
            if (id == null) return NotFound();
            Producto producto = _productoCEN.ReadOID(id.Value);
            if (producto == null) return NotFound();
            ViewBag.Categorias = new SelectList(_categoriaCEN.ReadAll(), "Id", "Nombre");
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, [Bind("Id,Nombre,Descripcion,Precio,Stock,Destacado,Color")] Producto producto, IFormFile? fotoArchivo)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Tienda");
            if (id != producto.Id) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var existente = _productoCEN.ReadOID(id);
                    var fotosList = GuardarFoto(fotoArchivo) ?? existente?.Fotos?.ToList();

                    _productoCEN.Modify(id, producto.Nombre, producto.Precio, producto.Stock, producto.Destacado, producto.Descripcion ?? "", fotos: fotosList);
                    
                    // Actualizar color si fue proporcionado
                    if (!string.IsNullOrEmpty(producto.Color))
                    {
                        _productoCEN.ActualizarColor(id, producto.Color);
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al editar el producto: {ex.Message}");
            }
            
            ViewBag.Categorias = new SelectList(_categoriaCEN.ReadAll(), "Id", "Nombre");
            return View(producto);
        }

        // GET: Productos/Delete/5
        public IActionResult Delete(long? id)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Tienda");
            if (id == null) return NotFound();
            Producto producto = _productoCEN.ReadOID(id.Value);
            if (producto == null) return NotFound();
            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Tienda");
            try
            {
                _productoCEN.Destroy(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo eliminar el producto: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private IList<string>? GuardarFoto(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0) return null;

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!extensionesPermitidas.Contains(extension))
                throw new Exception("Formato de imagen no permitido. Usa JPG, JPEG, PNG o WEBP.");

            // Guardamos en la webroot del sitio que se está ejecutando (para evitar rutas incorrectas si se lanza desde otra carpeta)
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "productos");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"prod_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            // Guardamos en la base la ruta web relativa
            var rutaWeb = $"/images/productos/{fileName}";
            return new List<string> { rutaWeb };
        }
    }
}
