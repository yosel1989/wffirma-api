using COFIDE.WFF.Domain.Common.Entities;
namespace COFIDE.WFF.Domain.Entities;
public class FormatoNotificacion : AuditableEntity<long>
{
    public string? NombreFormato      { get; }
    public string? CorreoRemitente    { get; }
    public string? NombreRemitente    { get; }
    public string? CorreoDestinatario { get; }
    public string? CopiaDestinatario  { get; }
    public string? AsuntoCorreo       { get; }
    public string? CuerpoMensaje      { get; }

    protected FormatoNotificacion() { }
}
