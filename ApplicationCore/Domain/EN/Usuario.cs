namespace ApplicationCore.Domain.EN;

public class Usuario
{
    public virtual long Id { get; set; }
    public virtual string Nombre { get; set; } = string.Empty;
    public virtual string Email { get; set; } = string.Empty;
    public virtual string Contrasenya { get; set; } = string.Empty;
    public virtual string? Telefono { get; set; }
    public virtual string? Direccion { get; set; }
    public virtual string? Rol { get; set; }
    // Datos de envío predeterminados
    public virtual string? NombreEnvio { get; set; }
    public virtual string? DireccionEnvio { get; set; }
    public virtual string? CiudadEnvio { get; set; }
    public virtual string? CPEnvio { get; set; }
    public virtual string? TelefonoEnvio { get; set; }
}
