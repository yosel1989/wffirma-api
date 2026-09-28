namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Correo.DTOs;

public sealed class EnviarCorreoRequestDto
{
    public string NombreUsuario { get; set; } = default!;
    public string Sistema { get; set; } = default!;
    public string Fecha { get; set; } = default!;
    public string Hora { get; set; } = default!;
    public string IpEquipo { get; set; } = default!;
    public string Asunto { get; set; } = default!;
    public string Cuerpo { get; set; } = default!;
    public string Remitente { get; set; } = default!;
    public string Para { get; set; } = default!;
    public string Resultado { get; set; } = default!;
}
