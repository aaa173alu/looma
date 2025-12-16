namespace ApplicationCore.Domain.EN;

public class ItemPedido
{
    public virtual long Id { get; set; }
    public virtual long ProductoId { get; set; }
    public virtual int Cantidad { get; set; }
    public virtual string? Talla { get; set; }
}
