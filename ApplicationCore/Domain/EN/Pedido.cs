using ApplicationCore.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN;

public class Pedido
{
    public virtual long Id { get; set; }
    public virtual DateTime Fecha { get; set; }
    public virtual decimal Total { get; set; }
    public virtual string DireccionEnvio { get; set; } = string.Empty;
    public virtual EstadoPedido Estado { get; set; }
    public virtual long UsuarioId { get; set; }
    public virtual IList<ItemPedido> Items { get; set; } = new List<ItemPedido>();
}
