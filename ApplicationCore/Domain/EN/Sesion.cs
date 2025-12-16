using System;

namespace ApplicationCore.Domain.EN;

public class Sesion
{
    public long Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool Activa { get; set; }
    public long UsuarioId { get; set; }
}
