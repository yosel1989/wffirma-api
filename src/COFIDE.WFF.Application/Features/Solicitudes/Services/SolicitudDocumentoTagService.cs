using AutoMapper.Internal;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using System.Text.Json;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudDocumentoTagService(
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository,
        IParametroRepository parametroRepository
    ) : ISolicitudDocumentoTagService
{
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;
    private readonly IParametroRepository _parametroRepository = parametroRepository;

    public async Task CrearDocumentoTagDesdeSolicitud(Solicitud solicitud, string? documentosTag, CancellationToken cancellationToken)
    {
        var documentos = solicitud.Documentos;

        if (documentos.Count == 0)
            return;

        var tags = ObtenerTagsConfiguracion(documentosTag);
        int contador = 0;
        foreach (var item in tags)
        {
            contador += item.Coordenadas.Count;
        }
        var tamanioFirma = await _parametroRepository.ObtenerTamanioFirmaAsync(cancellationToken);

        #region 01. Generacion de Correlativos
        var cantidadNuevosDocumentosTag = contador;
        var rangoDocumentoTag = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdDocumentoTagAsync(cantidadNuevosDocumentosTag, cancellationToken);
        var indexDocumentoTag = 0;
        Func<long> generarIdDocumentoTag = () => rangoDocumentoTag[indexDocumentoTag++];
        #endregion

        foreach (var documento in documentos)
        {
            var tagsDocumento = tags
               .Where(t =>
                   ObtenerNombreSinExtension(t.NombreDocumento).Equals(
                       ObtenerNombreSinExtension(documento.NombreOriginal),
                       StringComparison.OrdinalIgnoreCase))
               .ToList();

            if (tagsDocumento.Count == 0)
                continue;

            foreach (var tag in tagsDocumento)
            {
                foreach (var coord in tag.Coordenadas)
                {
                    documento.AgregarDocumentoTag(
                    generarIdDocumentoTag(),
                    coord.NumeroPagina,
                    coord.X,
                    coord.Y,
                    tamanioFirma.FirmaAnchoFirmante,
                    tamanioFirma.FirmaAltoFirmante);
                }
            }
        }
    }

    private static List<DocumentoTagRequestDto> PrepararTags(
    bool inCodigo,
    string? documentosTag)
    {
        var tags = ObtenerTagsConfiguracion(documentosTag);

        if (inCodigo)
            return tags;

        return tags
            .Select(tag => tag with
            {
                NombreDocumento = RemoverSufijoCodigo(tag.NombreDocumento)
            })
            .ToList();
    }

    private async Task<Func<long>> CrearGeneradorDocumentoTagAsync(
    IEnumerable<DocumentoTagRequestDto> tags,
    CancellationToken cancellationToken)
    {
        var cantidad = tags
            .SelectMany(x => x.Coordenadas)
            .Count(x => x.Id == 0);

        var rango = await _generadorCorrelativoIdRepository
            .ObtenerBloqueSiguienteIdDocumentoTagAsync(
                cantidad,
                cancellationToken);

        var index = 0;

        return () => rango[index++];
    }

    public async Task AntualizarDocumentoTagDesdeSolicitud(Solicitud solicitud, string? documentosTag, CancellationToken cancellationToken)
    {

        if (solicitud.Documentos.Count == 0)
            return;

        var tags = PrepararTags(solicitud.InCodigo, documentosTag);

        var tamanioFirma = await _parametroRepository.ObtenerTamanioFirmaAsync(cancellationToken);

        #region 01. Generacion de Correlativos
        var generarIdDocumentoTag = await CrearGeneradorDocumentoTagAsync(tags, cancellationToken);
        #endregion


        foreach (var documento in solicitud.Documentos)
        {
            ActualizarTagsDocumento(
              documento,
              solicitud.InCodigo,
              tags,
              tamanioFirma,
              generarIdDocumentoTag);
        }
    }


    #region Métodos privados

    private void ActualizarTagsDocumento(
    SolicitudDocumento documento,
    bool inCodigo,
    IEnumerable<DocumentoTagRequestDto> tags,
    TamanioFirmaDto tamanioFirma,
    Func<long> generarIdDocumentoTag)
    {
        var nombreDocumento = ObtenerNombreDocumento(documento, inCodigo);

        var tagsRequest = ObtenerTagsDocumento(
            tags,
            nombreDocumento);

        var tagsActuales = documento.DocumentoTags
            .Where(x => x.Estado)
            .ToList();

        // AGREGAR | ACTUALIZAR
        foreach (var tagRequest in tagsRequest)
        {
            foreach (var coord in tagRequest.Coordenadas)
            {
                if (coord.Id > 0)
                {
                    var tagActual = tagsActuales.FirstOrDefault(x => x.Id == coord.Id);

                    if (tagActual is null)
                        continue;

                    tagActual.Actualizar(
                    coord.NumeroPagina,
                    coord.X,
                    coord.Y);
                }
                else
                {
                    documento.AgregarDocumentoTag(
                       generarIdDocumentoTag(),
                       coord.NumeroPagina,
                       coord.X,
                       coord.Y,
                       tamanioFirma.FirmaAnchoFirmante,
                       tamanioFirma.FirmaAltoFirmante);
                }
            }
        }

        InactivarTagsEliminados(tagsRequest, tagsActuales);
    }

    private static string? ObtenerNombreDocumento(
    SolicitudDocumento documento,
    bool inCodigo)
    {
        if (documento.DocumentoConTag())
            return documento.NombreConTag!;

        return inCodigo
            ? documento.NombreCodigo
            : documento.NombreOriginal;
    }

    private static List<DocumentoTagRequestDto> ObtenerTagsDocumento(
    IEnumerable<DocumentoTagRequestDto> tags,
    string? nombreDocumento)
    {
        return tags
            .Where(t =>
                ObtenerNombreSinExtension(t.NombreDocumento)
                    .Equals(
                        ObtenerNombreSinExtension(nombreDocumento!),
                        StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static string RemoverSufijoCodigo(string nombreArchivo)
    {
        const string sufijo = "-COD";

        string nombre = Path.GetFileNameWithoutExtension(nombreArchivo);
        string extension = Path.GetExtension(nombreArchivo);

        if (nombre.EndsWith(sufijo, StringComparison.OrdinalIgnoreCase))
        {
            nombre = nombre[..^sufijo.Length];
        }

        return nombre + extension;
    }

    private static void InactivarTagsEliminados(
    IEnumerable<DocumentoTagRequestDto> tagsRequest,
    IEnumerable<DocumentoTag> tagsActuales)
    {
        var idsRequest = tagsRequest
            .SelectMany(x => x.Coordenadas)
            .Where(x => x.Id > 0)
            .Select(x => x.Id)
            .ToHashSet();

        foreach (var tagActual in tagsActuales)
        {
            if (!idsRequest.Contains(tagActual.Id))
            {
                tagActual.Inactivar();
            }
        }
    }

    private static string ObtenerNombreSinExtension(string nombre)
    {
        return Path.GetFileNameWithoutExtension(nombre);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static List<DocumentoTagRequestDto> ObtenerTagsConfiguracion(string? documentosTag)
    {
        if (string.IsNullOrWhiteSpace(documentosTag))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<DocumentoTagRequestDto>>(
                documentosTag,
                JsonOptions)
            ?? throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.ConfiguracionInvalid);
        }
        catch (JsonException)
        {
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ConfiguracionInvalid);
        }
    }
    #endregion
}
