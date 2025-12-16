using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebMVC.Controllers
{
    public class PedidoController : Controller
    {
        private readonly PedidoCEN _pedidoCEN;
        private readonly ProductoCEN _productoCEN;
        private readonly ValoracionCEN _valoracionCEN;

        public PedidoController(PedidoCEN pedidoCEN, ProductoCEN productoCEN, ValoracionCEN valoracionCEN)
        {
            _pedidoCEN = pedidoCEN;
            _productoCEN = productoCEN;
            _valoracionCEN = valoracionCEN;
        }

        private bool EstaLogueado() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));

        private long ObtenerUsuarioId()
        {
            var userId = HttpContext.Session.GetString("UsuarioId");
            return string.IsNullOrEmpty(userId) ? 0 : long.Parse(userId);
        }

        [HttpGet]
        public IActionResult MisPedidos()
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarioId = ObtenerUsuarioId();
            var pedidos = _pedidoCEN.ObtenerPorUsuario(usuarioId);

            var vm = new MisPedidosViewModel
            {
                Pendientes = new List<PedidoCardViewModel>(),
                Entregados = new List<PedidoCardViewModel>()
            };

            foreach (var pedido in pedidos)
            {
                // Ocultar pedidos no pagados (estado previo a pago)
                if (pedido.Estado == EstadoPedido.realizado || pedido.Estado == EstadoPedido.carrito)
                    continue;

                var card = MapearPedido(pedido);
                if (card.EsEntregado)
                    vm.Entregados.Add(card);
                else
                    vm.Pendientes.Add(card);
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Seguimiento(long id)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            var pedido = _pedidoCEN.ReadOID(id);
            if (pedido == null || pedido.UsuarioId != usuarioId)
            {
                return NotFound();
            }

            var vm = MapearPedido(pedido);
            return View(vm);
        }

        [HttpPost]
        public IActionResult Valorar([FromBody] ValorarRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            if (request.Valor < 1 || request.Valor > 5)
            {
                return Json(new { success = false, message = "La valoración debe estar entre 1 y 5" });
            }

            try
            {
                var usuarioId = ObtenerUsuarioId();

                // Aseguramos que el producto pertenece a un pedido entregado del usuario
                var pedidos = _pedidoCEN.ObtenerPorUsuario(usuarioId);
                var tienePedidoEntregado = pedidos.Any(p => EsEntregado(p.Estado) && p.Items.Any(i => i.ProductoId == request.ProductoId));
                if (!tienePedidoEntregado)
                {
                    return Json(new { success = false, message = "Solo puedes valorar productos de pedidos entregados" });
                }

                _valoracionCEN.Crear(usuarioId, request.ProductoId, request.Valor, request.Comentario);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private PedidoCardViewModel MapearPedido(Pedido pedido)
        {
            var productoCache = new Dictionary<long, (string Nombre, decimal Precio)>();

            var items = new List<PedidoLineaViewModel>();
            foreach (var item in pedido.Items)
            {
                if (!productoCache.TryGetValue(item.ProductoId, out var prodInfo))
                {
                    var prod = _productoCEN.ReadOID(item.ProductoId);
                    prodInfo = (prod?.Nombre ?? "Producto", prod?.Precio ?? 0m);
                    productoCache[item.ProductoId] = prodInfo;
                }

                items.Add(new PedidoLineaViewModel
                {
                    ProductoId = item.ProductoId,
                    NombreProducto = prodInfo.Nombre,
                    Cantidad = item.Cantidad,
                    Talla = item.Talla ?? "-",
                    PrecioUnidad = prodInfo.Precio,
                    Subtotal = prodInfo.Precio * item.Cantidad,
                    PuedeValorar = EsEntregado(pedido.Estado)
                });
            }

            return new PedidoCardViewModel
            {
                Id = pedido.Id,
                Fecha = pedido.Fecha,
                Total = pedido.Total,
                Direccion = pedido.DireccionEnvio,
                Estado = pedido.Estado,
                Items = items,
                Progreso = CalcularProgreso(pedido.Estado),
                EstadoLabel = ObtenerEstadoLabel(pedido.Estado),
                EsEntregado = EsEntregado(pedido.Estado)
            };
        }

        private static bool EsEntregado(EstadoPedido estado) => estado == EstadoPedido.recibido;

        private static int CalcularProgreso(EstadoPedido estado) => estado switch
        {
            EstadoPedido.carrito => 5,
            EstadoPedido.realizado => 20,
            EstadoPedido.comprado => 45, // pagado / en almacén
            EstadoPedido.validado => 75,  // enviado
            EstadoPedido.recibido => 100, // entregado
            _ => 50
        };

        private static string ObtenerEstadoLabel(EstadoPedido estado) => estado switch
        {
            EstadoPedido.carrito => "En carrito",
            EstadoPedido.realizado => "Pendiente de pago",
            EstadoPedido.comprado => "En almacén",
            EstadoPedido.validado => "Enviado",
            EstadoPedido.recibido => "Entregado",
            EstadoPedido.rechazado => "Rechazado",
            _ => estado.ToString()
        };
    }

    public class ValorarRequest
    {
        public long ProductoId { get; set; }
        public int Valor { get; set; }
        public string? Comentario { get; set; }
    }

    public class MisPedidosViewModel
    {
        public List<PedidoCardViewModel> Pendientes { get; set; } = new();
        public List<PedidoCardViewModel> Entregados { get; set; } = new();
    }

    public class PedidoCardViewModel
    {
        public long Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public EstadoPedido Estado { get; set; }
        public string EstadoLabel { get; set; } = string.Empty;
        public int Progreso { get; set; }
        public bool EsEntregado { get; set; }
        public List<PedidoLineaViewModel> Items { get; set; } = new();
    }

    public class PedidoLineaViewModel
    {
        public long ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Talla { get; set; } = string.Empty;
        public decimal PrecioUnidad { get; set; }
        public decimal Subtotal { get; set; }
        public bool PuedeValorar { get; set; }
    }
}
