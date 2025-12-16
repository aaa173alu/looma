using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN;

public class Carrito
{
    public virtual long Id { get; set; }
    public virtual decimal Total { get; set; }
    public virtual DateTime FechaCreacion { get; set; }
    public virtual long UsuarioId { get; set; }
    public virtual IList<ItemPedido> Items { get; set; } = new List<ItemPedido>();
}
