using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Exceptions;
using voDomain = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class SolicitudDocumento : AuditableEntity<long>
{
    public long IdSolicitud { get; private set; }
    public string NombreOriginal { get; private set; } = null!;
    public string ExtensionOriginal { get; private set; } = null!;
    public string RutaOriginal { get; private set; } = null!;
    public string? HashOriginal { get; private set; } = null!;

    public string? NombreCodigo { get; private set; }
    public string? ExtensionCodigo { get; private set; }
    public string? RutaCodigo { get; private set; }
    public string? HashCodigo { get; private set; }

    public string? NombreConTag { get; private set; }
    public string? ExtensionConTag { get; private set; }
    public string? RutaConTag { get; private set; }
    public string? HashConTag { get; private set; }

    public long? PesoDocumento { get; private set; }
    public bool EsNuevo { get; private set; } = false;


    private readonly List<DetalleFirma> _detalleFirma = [];
    private readonly List<DocumentoTag> _documentoTags = [];
    public IReadOnlyCollection<DetalleFirma> DetallesFirma => _detalleFirma;
    public IReadOnlyCollection<DocumentoTag> DocumentoTags => _documentoTags;

    public Solicitud Solicitud { get; private set; } = null!;

    private SolicitudDocumento() { } // EF

    internal static SolicitudDocumento Crear(
        long idSolicitud,
        long idSolicitudDocumento,
        string nombreOriginal,
        string extensionOriginal,
        string rutaOriginal,
        string hashOriginal,
        string? nombreCodigo,
        string? extensionCodigo,
        string? rutaCodigo,
        string? hashCodigo,
        string? nombreConTag,
        string? extensionConTag,
        string? rutaConTag,
        string? hashConTag,
        long? pesoDocumento)
    {
        var ext = extensionOriginal.Trim().ToLower();

        var documento = new SolicitudDocumento
        {
            Id = idSolicitudDocumento,
            IdSolicitud = idSolicitud,
            NombreOriginal = nombreOriginal,
            ExtensionOriginal = ext,
            RutaOriginal = rutaOriginal,
            HashOriginal = hashOriginal,
            NombreCodigo = nombreCodigo,
            ExtensionCodigo = extensionCodigo,
            RutaCodigo = rutaCodigo,
            HashCodigo = hashCodigo,
            NombreConTag = nombreConTag,
            ExtensionConTag = extensionConTag,
            RutaConTag = rutaConTag,
            HashConTag = hashConTag,
            PesoDocumento = pesoDocumento,
            Estado = true
        };

        documento.MarcarComoNuevo();

        return documento;
    }

    public void Inactivar()
    {
        Estado = false;
    }

    public void AgregarDetalleFirma(DetalleFirma detalleFirma)
    {
        _detalleFirma.Add(detalleFirma);
    }

    public void AgregarDocumentoTag(
         long idDocumentoTag,
         int numeroPagina,
         decimal ubicacionX,
         decimal ubicacionY,
         decimal anchoFirma,
         decimal altoFirma)
    {

        var tag = DocumentoTag.Crear(
            this.Id,
            idDocumentoTag,
            numeroPagina,
            ubicacionX,
            ubicacionY,
            anchoFirma,
            altoFirma);

        _documentoTags.Add(tag);
    }


    public void DesactivarDetalleFirmaPorFirmante(long idFirmanteDocumento)
    {
        if (_detalleFirma.Count == 0)
            return;

        foreach (var detalle in _detalleFirma.Where(x => x.Estado && x.IdFirmanteDocumento == idFirmanteDocumento))
        {
            detalle.Inactivar();
        }
    }

    public void RechazarFirmas()
    {
        if (_detalleFirma.Count == 0)
            return;

        foreach (var detalle in _detalleFirma.Where(x => x.Estado))
        {
            detalle.Inactivar();
        }
    }

    public void ReactivarFirmas()
    {
        if (_detalleFirma.Count == 0)
            return;

        foreach (var detalle in _detalleFirma.Where(x => !x.Estado))
        {
            detalle.ActivarDetalleFirma();
        }
    }

    public void AsignarDocumentoCodigo(
        string nombreCodigo,
        string extensionCodigo,
        string rutaCodigo,
        string hashCodigo)
    {
        NombreCodigo = nombreCodigo;
        ExtensionCodigo = extensionCodigo;
        RutaCodigo = rutaCodigo;
        HashCodigo = hashCodigo;
    }

    public void AsignarDocumentoConTag(
       string nombreConTag,
       string extensionConTag,
       string rutaConTag,
       string hashConTag)
    {
        NombreConTag = nombreConTag;
        ExtensionConTag = extensionConTag;
        RutaConTag = rutaConTag;
        HashConTag = hashConTag;
    }

    public void RemoverDocumentoCodigo()
    {
        NombreCodigo = null;
        ExtensionCodigo = null;
        RutaCodigo = null;
        HashCodigo = null;
    }

    #region Métodos de Firma

    public IReadOnlyCollection<string> ObtenerRutasArchivosFirmados()
    {
        return _detalleFirma
            .Where(x =>
                        !string.IsNullOrWhiteSpace(x.RutaDocumentoFirmado) &&
                        !string.IsNullOrWhiteSpace(x.NombreDocumentoFirmado) &&
                        !string.IsNullOrWhiteSpace(x.ExtensionDocumentoFirmado))
            .Select(x => $"{x.RutaDocumentoFirmado}/{x.NombreDocumentoFirmado}.{x.ExtensionDocumentoFirmado}")
            .ToList()
            .AsReadOnly();
    }

    public IList<string> ObtenerDnisFirmados()
    {
        return _detalleFirma
            .Where(x =>
                        !string.IsNullOrWhiteSpace(x.RutaDocumentoFirmado) &&
                        !string.IsNullOrWhiteSpace(x.NombreDocumentoFirmado) &&
                        !string.IsNullOrWhiteSpace(x.ExtensionDocumentoFirmado))
            .Select(x => x.UsuarioFirmante.NumeroDocumento)
            .ToList();
    }

    public string? ObtenerUltimoDniFirmado()
    {
        return _detalleFirma
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.RutaDocumentoFirmado) &&
                !string.IsNullOrWhiteSpace(x.NombreDocumentoFirmado) &&
                !string.IsNullOrWhiteSpace(x.ExtensionDocumentoFirmado))
            .OrderByDescending(x => x.FechaHoraFirma)
            .Select(x => x.UsuarioFirmante.NumeroDocumento)
            .FirstOrDefault();
    }

    private (string Nombre, string Extension, string? Hash, string? Ruta) ObtenerInfoDocumentoBase(bool inCodigo)
    {
        if (!string.IsNullOrWhiteSpace(HashConTag))
        {
            return (
                NombreConTag!,
                ExtensionConTag!,
                HashConTag,
                RutaConTag
            );
        }

        if (inCodigo)
        {
            return (
                NombreCodigo!,
                ExtensionCodigo!,
                HashCodigo,
                RutaCodigo
            );
        }

        return (
            NombreOriginal,
            ExtensionOriginal,
            HashOriginal,
            RutaOriginal
        );
    }

    private (string Nombre, string Extension, string? Hash, string? Ruta) ObtenerInfoDocumentoBaseParaTag(bool inCodigo)
    {
        if (inCodigo)
        {
            return (
                NombreCodigo!,
                ExtensionCodigo!,
                HashCodigo,
                RutaCodigo
            );
        }

        return (
            NombreOriginal,
            ExtensionOriginal,
            HashOriginal,
            RutaOriginal
        );
    }

    private string GenerarNombreFirmado(string nombre, string extension)
    {
        if (nombre.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            nombre = nombre[..^extension.Length];

        var countFirmas = ContarFirmas();

        if (countFirmas == 0)
            return $"{nombre} [F]";

        return $"{nombre}[F]";
    }

    public int ContarFirmas()
    {
        return _detalleFirma.Count(x => !string.IsNullOrWhiteSpace(x.RutaDocumentoFirmado) &&
                         !string.IsNullOrWhiteSpace(x.NombreDocumentoFirmado) &&
                         !string.IsNullOrWhiteSpace(x.ExtensionDocumentoFirmado));
    }

    public DocumentoVigente ObtenerDatosDocumentoParaTag(bool inCodigo)
    {
        var baseDoc = ObtenerInfoDocumentoBaseParaTag(inCodigo);

        return new DocumentoVigente(
          baseDoc.Nombre,
          "",
          baseDoc.Extension,
          baseDoc.Hash,
          baseDoc.Ruta!
      );
    }

    public DocumentoVigente ObtenerDocumentoVigenteParaFirma(bool inCodigo)
    {
        var ultimoFirmado = _detalleFirma
            .Where(x =>
                x.Estado &&
                x.EstadoDocumento == voDomain.EstadoDocumento.Firmado &&
                !string.IsNullOrWhiteSpace(x.HashDocumentoFirmado))
            .OrderBy(x => x.FechaHoraFirma)
            .LastOrDefault();

        var baseDoc = ObtenerInfoDocumentoBase(inCodigo);

        var nombreFinal = ultimoFirmado?.NombreDocumentoFirmado ?? baseDoc.Nombre;
        var extensionFinal = ultimoFirmado?.ExtensionDocumentoFirmado ?? baseDoc.Extension;
        var hashFinal = ultimoFirmado?.HashDocumentoFirmado ?? baseDoc.Hash;
        var rutaFinal = ultimoFirmado?.RutaDocumentoFirmado ?? baseDoc.Ruta;

        if (string.IsNullOrWhiteSpace(hashFinal))
            throw new DomainException(SolicitudErrors.DocumentoSinHash);

        return new DocumentoVigente(
            nombreFinal,
            GenerarNombreFirmado(nombreFinal, extensionFinal),
            extensionFinal,
            hashFinal,
            rutaFinal!
        );
    }
    public long ObtenerIdDetalleFirmaVigenteParaFirma()
    {
        var ultimoFirmado = _detalleFirma
            .Where(x =>
                x.Estado &&
                x.EstadoDocumento == voDomain.EstadoDocumento.Firmado &&
                !string.IsNullOrWhiteSpace(x.HashDocumentoFirmado))
            .OrderBy(x => x.Id)
            .LastOrDefault();

        if (ultimoFirmado == null)
        {
            return _detalleFirma
                .Where(x => x.Estado)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefault();
        }

        return _detalleFirma
            .Where(x => x.Estado && x.Id > ultimoFirmado.Id)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstOrDefault();
    }
    #endregion

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }

    public bool DocumentoConTag()
    {
        return _documentoTags.Any(x =>
            x.Estado);
    }

    public bool TieneRutaTag()
    {
        var nombre = NombreConTag;
        var ruta = RutaConTag;

        if (!string.IsNullOrWhiteSpace(nombre) && !string.IsNullOrWhiteSpace(ruta))
            return true;

        return false;
    }

    public void LimpiarDocumentoConTag()
    {
        NombreConTag = null;
        RutaConTag = null;
        ExtensionConTag = null;
        HashConTag = null;
    }

    public (decimal X, decimal Y) ObtenerPosicionFirmaVisador(
    decimal posicionX,
    decimal posicionY,
    decimal anchoFirma,
    decimal altoFirma,
    bool esHorizontal = false,
    decimal? anchoPagina = null,
    decimal? altoPagina = null,
    decimal margenVertical = 1m)
    {
        var visadoresFirmados = _detalleFirma.Count(x =>
            x.Estado &&
            x.SolicitudFirmanteFlujo.EsVisador &&
            x.EstadoDocumento ==
                voDomain.EstadoDocumento.Firmado);

        /*
         Solo para documentos horizontales
         */
        if (esHorizontal && anchoPagina.HasValue && altoPagina.HasValue)
        {
            const int cantidadPosicionesReferencia = 10;
            const decimal margenHorizontal = 10m;
            const decimal margenInferior = 10m;

            var posicionInicialX =
                posicionX + margenHorizontal;

            var espacioDisponible =
                anchoPagina.Value -
                posicionInicialX -
                anchoFirma -
                margenHorizontal;

            var desplazamientoHorizontal =
                espacioDisponible /
                (cantidadPosicionesReferencia - 1);

            var posicionHorizontalX =
                Math.Round(
                    posicionInicialX +
                    (visadoresFirmados * desplazamientoHorizontal),
                    0,
                    MidpointRounding.AwayFromZero);

            var posicionHorizontalY =
                altoPagina.Value -
                altoFirma -
                margenInferior;

            return (
                posicionHorizontalX,
                posicionHorizontalY
            );
        }

        /*
          Documento vertical
         */
        if (_detalleFirma.Count == 0)
            return (posicionX, posicionY);

        if (visadoresFirmados == 0)
        {
            altoFirma = 0;
            margenVertical = 0;
        }

        var desplazamiento =
            visadoresFirmados *
            (altoFirma + margenVertical);

        var nuevaY =
            posicionY -
            desplazamiento;

        return (
            posicionX,
            nuevaY
        );
    }
}