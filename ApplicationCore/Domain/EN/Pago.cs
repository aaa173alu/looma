using System;

namespace ApplicationCore.Domain.EN;

public class Pago
{
    public long Id { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public long? PedidoId { get; set; }
}
