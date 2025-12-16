using System;

namespace ApplicationCore.Domain.EN;

public class Tarjeta
{
    public virtual long Id { get; set; }
    public virtual Usuario Usuario { get; set; }
    public virtual string Marca { get; set; } = string.Empty; // Ej: VISA, MASTERCARD
    public virtual string NumeroEnmascarado { get; set; } = string.Empty; // Solo almacenar enmascarado
    public virtual string Ultimos4 { get; set; } = string.Empty;
    public virtual int MesExp { get; set; }
    public virtual int AnioExp { get; set; }
    public virtual string NombreTitular { get; set; } = string.Empty;
    public virtual bool EsPredeterminada { get; set; }
    public virtual DateTime FechaAlta { get; set; } = DateTime.UtcNow;
}
