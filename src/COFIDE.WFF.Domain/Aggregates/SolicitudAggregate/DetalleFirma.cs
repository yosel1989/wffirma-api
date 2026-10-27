using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.ValueObjects;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;

using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class DetalleFirma : AuditableEntity<long>
{
    public long IdSolicitudFirmanteFlujo { get; private set; }
    public long IdSolicitudDocumento { get; private set; }
    public string? NombreDocumentoFirmado { get; private set; }
    public string? ExtensionDocumentoFirmado { get; private set; }
    public string? RutaDocumentoFirmado { get; private set; }
    public string? HashDocumentoFirmado { get; private set; }
    public long? IdFirmanteDocumento { get; private set; }
    public DateTime? FechaHoraFirma { get; private set; }
    public ValueObject.InternoExterno? InternoExterno { get; private set; }
    public ValueObject.EstadoDocumento EstadoDocumento { get; private set; } = ValueObject.EstadoDocumento.Pendiente;
    public DateTime? FechaInicioProceso { get; private set; }
    public DateTime? FechaFinEsperado { get; private set; }
    public string? CodigoFirma { get; private set; }
    public string? CodigoReferencial { get; private set; }

    public SolicitudDocumento SolicitudDocumento { get; private set; } = null!;
    public SolicitudFirmanteFlujo SolicitudFirmanteFlujo { get; private set; } = default!;
    public Usuario UsuarioFirmante { get; private set; } = null!;

    public bool EsNuevo { get; private set; } = false;
    private DetalleFirma() { }

    private DetalleFirma(
        long idDetalleFirma,
        long idSolicitudFirmanteFlujo,
        long idSolicitudDocumento,
        string? nombreDocumentoFirmado,
        string? extensionDocumentoFirmado,
        string? rutaDocumentoFirmado,
        string? hashDocumentoFirmado,
        long? idFirmanteDocumento,
        string? codigoReferencial)
    {
        Id = idDetalleFirma;
        IdSolicitudFirmanteFlujo = idSolicitudFirmanteFlujo;
        IdSolicitudDocumento = idSolicitudDocumento;
        NombreDocumentoFirmado = nombreDocumentoFirmado;
        ExtensionDocumentoFirmado = extensionDocumentoFirmado;
        RutaDocumentoFirmado = rutaDocumentoFirmado;
        HashDocumentoFirmado = hashDocumentoFirmado;
        IdFirmanteDocumento = idFirmanteDocumento;
        CodigoReferencial = codigoReferencial;
        Estado = true;
    }

    public static DetalleFirma Crear(
        long idDetalleFirma,
        long idSolicitudFirmanteFlujo,
        long idSolicitudDocumento,
        string? nombreDocumentoFirmado,
        string? extensionDocumentoFirmado,
        string? rutaDocumentoFirmado,
        string? hashDocumentoFirmado,
        long? idFirmanteDocumento)
    {
        if (idSolicitudFirmanteFlujo <= 0)
            throw new ArgumentException("El IdSolicitudFirmanteFlujo debe ser mayor a 0.", nameof(idSolicitudFirmanteFlujo));

        if (idSolicitudDocumento <= 0)
            throw new ArgumentException("El IdSolicitudDocumento debe ser mayor a 0.", nameof(idSolicitudDocumento));

        string codigoReferencial = Guid.NewGuid().ToString();

        var detalleFirma = new DetalleFirma(
            idDetalleFirma,
            idSolicitudFirmanteFlujo,
            idSolicitudDocumento,
            nombreDocumentoFirmado,
            extensionDocumentoFirmado,
            rutaDocumentoFirmado,
            hashDocumentoFirmado,
            idFirmanteDocumento,
            codigoReferencial
        );

        detalleFirma.MarcarComoNuevo();

        return detalleFirma;
    }

    public static DetalleFirma CopiarActivo(long idDetalleFirma, DetalleFirma origen)
    {
        var copia = Crear(
            idDetalleFirma,
            origen.IdSolicitudFirmanteFlujo,
            origen.IdSolicitudDocumento,
            origen.NombreDocumentoFirmado,
            origen.ExtensionDocumentoFirmado,
            origen.RutaDocumentoFirmado,
            origen.HashDocumentoFirmado,
            origen.IdFirmanteDocumento);

        copia.FechaHoraFirma = origen.FechaHoraFirma;
        copia.InternoExterno = origen.InternoExterno;
        copia.EstadoDocumento = origen.EstadoDocumento;
        copia.FechaInicioProceso = origen.FechaInicioProceso;
        copia.FechaFinEsperado = origen.FechaFinEsperado;
        copia.CodigoFirma = origen.CodigoFirma;
        return copia;
    }

    public void Firmar(
        string nombreDocumentoFirmado,
        string extensionDocumentoFirmado,
        string rutaDocumentoFirmado,
        string hashDocumentoFirmado,
        bool EsInterno
        )
    {
        ValidacionDocumentoFirmado();

        NombreDocumentoFirmado = nombreDocumentoFirmado;
        ExtensionDocumentoFirmado = extensionDocumentoFirmado;
        RutaDocumentoFirmado = rutaDocumentoFirmado;
        HashDocumentoFirmado = hashDocumentoFirmado;
        FechaHoraFirma = HoraSistema.Ahora;
        InternoExterno = (EsInterno ? ValueObject.InternoExterno.Interno : ValueObject.InternoExterno.Externo);
        EstadoDocumento = ValueObject.EstadoDocumento.Firmado;
    }

    public void ValidacionDocumentoFirmado()
    {
        if (EstadoDocumento == ValueObject.EstadoDocumento.Firmado)
            throw new DomainException(SolicitudErrors.DocumentoFirmado);
    }

    public bool TieneDocumentoFirmado()
    {
        if (EstadoDocumento == ValueObject.EstadoDocumento.Firmado)
            return true;

        return false;
    }

    public void Inactivar()
    {
        Estado = false;
    }

    public void FechaInicioFechaFin(long tiempoFirma)
    {
        FechaInicioProceso = HoraSistema.Ahora;
        FechaFinEsperado = FechaInicioProceso.Value.AddSeconds(tiempoFirma);
    }

    public void ActivarDetalleFirma()
    {
        Estado = true;
    }

    public void Bloquear()
    {
        EstadoDocumento = ValueObject.EstadoDocumento.Bloqueado;
    }

    public void ActivarDocumento()
    {
        EstadoDocumento = ValueObject.EstadoDocumento.Pendiente;
    }

    public void SetearCodigoFirma(string codigoFirma)
    {
        CodigoFirma = codigoFirma;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }

    public void DocumentoFirmadoEvent(bool firmado, string mensaje, string codigoUsuario)
    {
        AddDomainEvent(
            new DocumentoFirmadoDomainEvent(
                Id,
                SolicitudDocumento.IdSolicitud,
                IdSolicitudDocumento,
                firmado,
                codigoUsuario,
                mensaje)
            );
    }

    public string ObtenerRutasArchivosFirmados()
    {
        return $"{RutaDocumentoFirmado}/{NombreDocumentoFirmado}.{ExtensionDocumentoFirmado}";
    }

    public void RestablecerFirma()
    {
        CodigoFirma = string.Empty;
        RutaDocumentoFirmado = string.Empty;
        NombreDocumentoFirmado = string.Empty;
        ExtensionDocumentoFirmado = string.Empty;
        HashDocumentoFirmado = string.Empty;
        FechaInicioProceso = null;
        FechaFinEsperado = null;
        EstadoDocumento = ValueObject.EstadoDocumento.Pendiente;
        InternoExterno = null;
        FechaHoraFirma = null;
    }
}