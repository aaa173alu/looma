namespace ApplicationCore.Domain.EN;

public class Ayuda
{
    public long Id { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    public string? Respuesta { get; set; }
    public long UsuarioId { get; set; }
}
