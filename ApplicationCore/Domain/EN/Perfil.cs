namespace ApplicationCore.Domain.EN;

public class Perfil
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Contrasenya { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public long UsuarioId { get; set; }
}
