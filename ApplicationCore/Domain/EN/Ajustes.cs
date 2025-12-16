namespace ApplicationCore.Domain.EN;

public class Ajustes
{
    public long Id { get; set; }
    public string Tema { get; set; } = string.Empty;
    public bool Notificaciones { get; set; }
    public long UsuarioId { get; set; }
}
