using System;

namespace ApplicationCore.Domain.EN;

public class Favoritos
{
    public virtual long Id { get; set; }
    public virtual DateTime FechaCreacion { get; set; }
    public virtual long UsuarioId { get; set; }
    public virtual long ProductoId { get; set; }
}
