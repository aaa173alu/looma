using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using System.Text.Json;
using System.Globalization;

namespace WebMVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CarritoCEN _carritoCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly ProductoCEN _productoCEN;
        private readonly FinalizarCompraCP _finalizarCompraCP;
        private readonly TarjetaCEN _tarjetaCEN;
        private readonly PedidoCEN _pedidoCEN;

        public CheckoutController(
            CarritoCEN carritoCEN,
            UsuarioCEN usuarioCEN,
            ProductoCEN productoCEN,
            FinalizarCompraCP finalizarCompraCP,
            TarjetaCEN tarjetaCEN,
            PedidoCEN pedidoCEN)
        {
            _carritoCEN = carritoCEN;
            _usuarioCEN = usuarioCEN;
            _productoCEN = productoCEN;
            _finalizarCompraCP = finalizarCompraCP;
            _tarjetaCEN = tarjetaCEN;
            _pedidoCEN = pedidoCEN;
        }

        private bool EstaLogueado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        private long ObtenerUsuarioId()
        {
            var userId = HttpContext.Session.GetString("UsuarioId");
            return string.IsNullOrEmpty(userId) ? 0 : long.Parse(userId);
        }

        // GET: Checkout (Paso 1 - Datos de envío)
        public IActionResult Index()
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarioId = ObtenerUsuarioId();
            var usuario = _usuarioCEN.ReadOID(usuarioId);
            var (itemsConDetalles, total, esDirecto) = ObtenerItemsParaCheckout(usuarioId);
            if (itemsConDetalles.Count == 0)
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("Index", "Carrito");
            }

            ViewBag.Usuario = usuario;
            ViewBag.NombreEnvio = usuario.NombreEnvio ?? usuario.Nombre;
            ViewBag.DireccionEnvio = usuario.DireccionEnvio ?? usuario.Direccion;
            ViewBag.TelefonoEnvio = usuario.TelefonoEnvio ?? usuario.Telefono;
            ViewBag.EmailEnvio = usuario.Email;
            ViewBag.ItemsConDetalles = itemsConDetalles;
            ViewBag.Total = total;
            ViewBag.EsDirecto = esDirecto;

            return View();
        }

        // POST: Checkout/DatosEnvio
        [HttpPost]
        public IActionResult DatosEnvio([FromBody] DatosEnvioRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            // Guardar datos de envío en sesión
            HttpContext.Session.SetString("Checkout_TipoEnvio", request.TipoEnvio ?? "");
            HttpContext.Session.SetString("Checkout_Nombre", request.Nombre ?? "");
            HttpContext.Session.SetString("Checkout_Direccion", request.Direccion ?? "");
            HttpContext.Session.SetString("Checkout_Telefono", request.Telefono ?? "");
            HttpContext.Session.SetString("Checkout_Email", request.Email ?? "");

            return Json(new { success = true });
        }

        // GET: Checkout/Pago (Paso 2 - Pago)
        public IActionResult Pago()
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            // Verificar que se hayan completado los datos de envío
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Checkout_Direccion")))
            {
                return RedirectToAction("Index");
            }

            var usuarioId = ObtenerUsuarioId();
            var (itemsConDetalles, total, _) = ObtenerItemsParaCheckout(usuarioId);
            if (itemsConDetalles.Count == 0)
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("Index", "Carrito");
            }

            ViewBag.Total = total;
            ViewBag.Nombre = HttpContext.Session.GetString("Checkout_Nombre");
            ViewBag.Direccion = HttpContext.Session.GetString("Checkout_Direccion");
            ViewBag.TarjetaPredeterminada = _tarjetaCEN.ObtenerPredeterminada(usuarioId);

            return View();
        }

        // POST: Checkout/ProcesarPago
        [HttpPost]
        public IActionResult ProcesarPago([FromBody] PagoRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            try
            {
                var usuarioId = ObtenerUsuarioId();
                var (itemsConDetalles, total, esDirecto) = ObtenerItemsParaCheckout(usuarioId);
                if (itemsConDetalles.Count == 0)
                {
                    return Json(new { success = false, message = "Tu carrito está vacío" });
                }

                // Validar tarjeta (simulación)
                string numeroParaValidar;
                if (request.TipoPago == "guardada")
                {
                    var tarjeta = _tarjetaCEN.ObtenerPredeterminada(usuarioId);
                    if (tarjeta == null)
                        return Json(new { success = false, message = "No tienes tarjeta guardada. Usa 'otra tarjeta'." });
                    numeroParaValidar = "4532148803436467"; // usar número de prueba
                }
                else
                {
                    numeroParaValidar = request.NumeroTarjeta;
                }

                var esValida = ValidarTarjetaSimulada(numeroParaValidar);

                if (!esValida)
                {
                    return Json(new { success = false, message = "Tarjeta rechazada. Usa 4532 1488 0343 6467 para pruebas." });
                }

                // Obtener datos de envío
                var direccion = HttpContext.Session.GetString("Checkout_Direccion") ?? "Dirección no especificada";
                var nombre = HttpContext.Session.GetString("Checkout_Nombre") ?? "";

                // Preparar items
                var items = itemsConDetalles.Select(x => new ItemPedido
                {
                    ProductoId = x.Item.ProductoId,
                    Cantidad = x.Item.Cantidad,
                    Talla = x.Item.Talla
                }).ToList();

                // Finalizar compra usando el CP
                var pedido = _finalizarCompraCP.Execute(usuarioId, direccion, items);
                var pedidoId = pedido.Id;

                // Marcar como pagado
                _pedidoCEN.CambiarEstado(pedidoId, EstadoPedido.comprado);

                // Limpiar sesión de checkout
                HttpContext.Session.Remove("Checkout_TipoEnvio");
                HttpContext.Session.Remove("Checkout_Nombre");
                HttpContext.Session.Remove("Checkout_Direccion");
                HttpContext.Session.Remove("Checkout_Telefono");
                HttpContext.Session.Remove("Checkout_Email");
                HttpContext.Session.Remove("CarritoId");
                HttpContext.Session.Remove("Checkout_DirectItems");

                // Reset TempData to avoid serializing stale values (e.g., decimals)
                TempData.Clear();
                TempData["Checkout_Nombre"] = nombre;
                TempData["Checkout_Direccion"] = direccion;
                TempData["Checkout_Total"] = total.ToString("0.00", CultureInfo.InvariantCulture);

                return Json(new { 
                    success = true, 
                    pedidoId = pedidoId,
                    message = "¡Compra realizada con éxito!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Validación de tarjeta simulada
        private bool ValidarTarjetaSimulada(string numeroTarjeta)
        {
            // Eliminar espacios
            numeroTarjeta = numeroTarjeta.Replace(" ", "");

            // Tarjetas de prueba que siempre funcionan
            var tarjetasValidas = new[] 
            { 
                "4532148803436467",  // Visa
                "5425233430109903",  // Mastercard
                "378282246310005"    // Amex (también válida para prueba)
            };

            return tarjetasValidas.Contains(numeroTarjeta);
        }

        // GET: Checkout/Confirmacion
        public IActionResult Confirmacion(long pedidoId)
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.PedidoId = pedidoId;
            ViewBag.Nombre = TempData["Checkout_Nombre"];
            ViewBag.Direccion = TempData["Checkout_Direccion"];
            ViewBag.Total = TempData["Checkout_Total"];
            return View();
        }

        // POST: Checkout/CompraDirecta
        [HttpPost]
        public IActionResult CompraDirecta([FromBody] CompraDirectaRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            // Guardar la selección directa en sesión (no toca el carrito)
            var items = new List<SessionItem>
            {
                new SessionItem
                {
                    ProductoId = request.ProductoId,
                    Cantidad = request.Cantidad,
                    Talla = request.Talla
                }
            };

            HttpContext.Session.SetString("Checkout_DirectItems", JsonSerializer.Serialize(items));
            return Json(new { success = true });
        }

        private (List<(ItemPedido Item, Producto Producto)> Items, decimal Total, bool EsDirecto) ObtenerItemsParaCheckout(long usuarioId)
        {
            // Si hay compra directa en sesión, usarla
            var directJson = HttpContext.Session.GetString("Checkout_DirectItems");
            if (!string.IsNullOrEmpty(directJson))
            {
                var sessionItems = JsonSerializer.Deserialize<List<SessionItem>>(directJson) ?? new();
                var itemsConDetalles = new List<(ItemPedido, Producto)>();
                decimal total = 0;
                foreach (var si in sessionItems)
                {
                    var producto = _productoCEN.ReadOID(si.ProductoId);
                    if (producto == null) continue;
                    var item = new ItemPedido
                    {
                        ProductoId = si.ProductoId,
                        Cantidad = si.Cantidad,
                        Talla = si.Talla
                    };
                    itemsConDetalles.Add((item, producto));
                    total += producto.Precio * si.Cantidad;
                }
                return (itemsConDetalles, total, true);
            }

            // Si no, usar carrito completo
            var carrito = _carritoCEN.ObtenerPorUsuario(usuarioId);
            if (carrito == null || carrito.Items.Count == 0)
            {
                return (new List<(ItemPedido, Producto)>(), 0, false);
            }

            var itemsConDetallesCarrito = new List<(ItemPedido, Producto)>();
            foreach (var item in carrito.Items)
            {
                var producto = _productoCEN.ReadOID(item.ProductoId);
                if (producto != null)
                {
                    itemsConDetallesCarrito.Add((item, producto));
                }
            }
            var totalCarrito = _carritoCEN.CalcularTotal(carrito.Id);
            return (itemsConDetallesCarrito, totalCarrito, false);
        }
    }

    public class PagoRequest
    {
        public string TipoPago { get; set; } = "";
        public string NumeroTarjeta { get; set; } = "";
        public string FechaExpiracion { get; set; } = "";
        public string CVV { get; set; } = "";
        public string TitularTarjeta { get; set; } = "";
    }

    public class DatosEnvioRequest
    {
        public string? TipoEnvio { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }

    public class CompraDirectaRequest
    {
        public long ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string? Talla { get; set; }
    }

    public class SessionItem
    {
        public long ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string? Talla { get; set; }
    }
}
