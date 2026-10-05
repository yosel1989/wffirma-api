using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class DocumentoTag : AuditableEntity<long>
{
    public long IdSolicitudDocumento { get; private set; }
    public long? IdDetalleFirma { get; private set; }
    public int NumeroPagina { get; private set; }
    public decimal UbicacionX { get; private set; }
    public decimal UbicacionY { get; private set; }
    public decimal AnchoFirma { get; private set; }
    public decimal AltoFirma { get; private set; }
    public bool EsNuevo { get; private set; } = false;
    public SolicitudDocumento SolicitudDocumento { get; private set; } = null!;
    private DocumentoTag() { }

    private DocumentoTag(
        long idSolicitudDocumento,
        long? idDetalleFirma,
        long idDocumentTag,
        int numeroPagina,
        decimal ubicacionX,
        decimal ubicacionY,
        decimal anchoFirma,
        decimal altoFirma)
    {
        Id = idDocumentTag;
        IdDetalleFirma = idDetalleFirma;
        IdSolicitudDocumento = idSolicitudDocumento;
        NumeroPagina = numeroPagina;
        UbicacionX = ubicacionX;
        UbicacionY = ubicacionY;
        AnchoFirma = anchoFirma;
        AltoFirma = altoFirma;
        Estado = true;
    }


    public static DocumentoTag Crear(
        long idSolicitudDocumento,
        long idDocumentTag,
        int numeroPagina,
        decimal ubicacionX,
        decimal ubicacionY,
        decimal anchoFirma,
        decimal altoFirma)
    {
        if (idDocumentTag <= 0)
            throw new DomainException(SolicitudErrors.DocumentoTagInvalid);

        if (idSolicitudDocumento <= 0)
            throw new DomainException(SolicitudErrors.SolicitudDocumentoInvalid);

        var documentoTag = new DocumentoTag(
            idSolicitudDocumento,
            null,
            idDocumentTag,
            numeroPagina,
            ubicacionX,
            ubicacionY,
            anchoFirma,
            altoFirma
        );

        documentoTag.MarcarComoNuevo();

        return documentoTag;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }

    public void SetearIdDetalleFirma(long idDetalleFirma)
    {
        IdDetalleFirma = idDetalleFirma;
    }

    public void Actualizar(int numeroPagina, decimal ubicacionX, decimal ubicacionY)
    {
        NumeroPagina = numeroPagina;
        UbicacionX = ubicacionX;
        UbicacionY = ubicacionY;
    }

    public void Inactivar()
    {
        Estado = false;
    }
}
