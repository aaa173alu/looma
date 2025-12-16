namespace ApplicationCore.Domain.EN;

public class Valoracion
{
    public virtual long Id { get; set; }
    public virtual int Valor { get; set; }
    public virtual string? Comentario { get; set; }
    public virtual long UsuarioId { get; set; }
    public virtual long ProductoId { get; set; }
}
