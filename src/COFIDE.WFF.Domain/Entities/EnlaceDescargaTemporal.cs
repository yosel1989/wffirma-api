using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Domain.Entities;

public class EnlaceDescargaTemporal : AuditableEntity<long>
{
    public string Token { get; private set; } = default!;
    public long IdSolicitud { get; private set; }
    public DateTime FechaCreada { get; private set; }
    public DateTime FechaExpiracion { get; private set; }
    public int DescargasRealizadas { get; private set; }
    public int? DescargasMaximas { get; private set; }

    protected EnlaceDescargaTemporal() { }

    public EnlaceDescargaTemporal(
        string token,
        long idSolicitud,
        DateTime fechaExpiracion,
        int? descargasMaximas = null)
    {
        Token = token;
        IdSolicitud = idSolicitud;
        FechaCreada = HoraSistema.Ahora;
        FechaExpiracion = fechaExpiracion;
        DescargasMaximas = descargasMaximas;
        Estado = true;
    }

    public static EnlaceDescargaTemporal Crear(
     string token,
     long idSolicitud,
     DateTime fechaExpiracion)
    {      
        return new EnlaceDescargaTemporal(
            token,
            idSolicitud,
            fechaExpiracion,
            null);
    }

    public void ValidarDisponible()
    {
        if (!Estado)
            throw new DomainException(new Common.Errors.DomainError("", "El enlace está inactivo."));

        if (FechaExpiracion < HoraSistema.Ahora)
            throw new DomainException(new Common.Errors.DomainError("", "El enlace ha expirado."));

        if (DescargasMaximas.HasValue &&
            DescargasRealizadas >= DescargasMaximas.Value)
            throw new DomainException(new Common.Errors.DomainError("", "Se alcanzó el límite de descargas."));
    }

    public void RegistrarDescarga()
    {
        DescargasRealizadas++;  
    }
}
