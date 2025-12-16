using System.Collections.Generic;

namespace ApplicationCore.Domain.EN;

public class Producto
{
    public virtual long Id { get; set; }
    public virtual string Nombre { get; set; } = string.Empty;
    public virtual string? Descripcion { get; set; }
    public virtual decimal Precio { get; set; }
    public virtual IList<string> TallasDisponibles { get; set; } = new List<string>();
    public virtual IList<string>? Fotos { get; set; }
    public virtual int Stock { get; set; }
    public virtual bool Destacado { get; set; }
    public virtual string? Color { get; set; }
}
