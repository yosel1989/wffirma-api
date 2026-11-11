using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Models.Pdf;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Domain.DomainServices;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.GenerarPdfSolicitud;

/// <summary>
/// Genera el formato PDF de una solicitud con los mismos datos (y las mismas reglas de acceso)
/// que la consulta del detalle de la solicitud.
/// </summary>
public sealed class GenerarPdfSolicitudQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudQueryRepository solicitudQueryRepository,
        IMaestroRepository maestroRepository,
        ISolicitudPdfGenerator pdfGenerator,
        IAccesoEspecialRepository accesoEspecialRepository
    ) : BaseQueryHandler<GenerarPdfSolicitudQuery, GenerarPdfSolicitudDto?>(mapper)
{
    private const string EstadoTuTurno = "Tu Turno";
    private const string EstadoPendiente = "Pendiente";

    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly IMaestroRepository _maestroRepository = maestroRepository;
    private readonly ISolicitudPdfGenerator _pdfGenerator = pdfGenerator;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public override async Task<GenerarPdfSolicitudDto?> Handle(GenerarPdfSolicitudQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal.Value;

        var idsAreasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);
        var datos = await _solicitudQueryRepository.ObtenerParaAutorizacionAsync(
            request.IdSolicitud, cancellationToken);

        ReglaConsultaSolicitud.Validar(
            datos, idUsuario, _currentUser.UserAreaId,
            _currentUser.CodigoRolUsuario, idsAreasAccesoEspecial);

        var solicitud = await _solicitudQueryRepository.ObtenerSolicitudPorId(
            request.IdSolicitud, idUsuario, esReuso: false, cancellationToken);

        if (solicitud is null)
            return null;

        var modelo = new SolicitudPdf(
            CodigoSolicitud: solicitud.CodigoSolicitud ?? string.Empty,
            EsUrgente: solicitud.EsUrgente,
            TipoSolicitud: await NombreTipoSolicitudAsync(solicitud.IdTipoSolicitud, cancellationToken),
            SubTipoSolicitud: await NombreSubTipoSolicitudAsync(solicitud.IdTipoSolicitud, solicitud.IdSubTipoSolicitud, cancellationToken),
            TipoOperacion: await NombreTipoOperacionAsync(solicitud.IdtipoOperacion, cancellationToken),
            Asunto: solicitud.Asunto ?? string.Empty,
            Mensaje: solicitud.Mensaje,
            FechaLimite: solicitud.FechaLimite,
            Firmantes: Firmantes(solicitud),
            ArchivosFirma: solicitud.Documentos
                .Select(d => new ArchivoPdf(NombreArchivo(d.Nombre, d.Extension), d.Peso))
                .ToList(),
            ArchivosSustento: solicitud.Sustentos
                .Select(s => new ArchivoPdf(NombreArchivo(s.Nombre, s.Extension), s.Peso))
                .ToList(),
            Etapas: Etapas(solicitud),
            CodigosReferencia: solicitud.CodigosReferencia
                .Select(c => c.Nombre)
                .OfType<string>()
                .ToList(),
            Destinatarios: solicitud.SolicitudCorreo
                .Select(c => c.Nombre)
                .OfType<string>()
                .ToList(),
            FechaGeneracion: HoraSistema.Ahora);

        return new GenerarPdfSolicitudDto
        {
            Archivo = _pdfGenerator.Generar(modelo),
            NombreArchivo = $"Solicitud_{modelo.CodigoSolicitud}.pdf"
        };
    }

    private static List<FirmantePdf> Firmantes(SolicitudDto solicitud) =>
        (solicitud.EstadoFlujo?.etapas ?? new List<PlantillaEtapav2Dto>())
            .OrderBy(e => e.OrdenEtapa)
            .SelectMany(e => e.Firmantes.Select(f => new FirmantePdf(
                e.OrdenEtapa,
                string.IsNullOrWhiteSpace(f.CargoUsuario) ? e.ValorTipoAprobador : f.CargoUsuario!,
                NombreCompleto(f))))
            .ToList();

    private static List<EtapaPdf> Etapas(SolicitudDto solicitud) =>
        (solicitud.EstadoFlujo?.etapas ?? new List<PlantillaEtapav2Dto>())
            .OrderBy(e => e.OrdenEtapa)
            .Select(e => new EtapaPdf(
                e.OrdenEtapa,
                e.ValorTipoEtapa,
                e.ValorTipoAprobador,
                e.Firmantes.Select(f => new AprobadorPdf(NombreCompleto(f), EstadoAprobador(f.Estado), f.Estado)).ToList()))
            .ToList();

    /// <summary>
    /// El detalle de la solicitud devuelve en Estado la fecha de aprobación, "Tu Turno" o "Pendiente".
    /// </summary>
    private static EstadoAprobadorPdf EstadoAprobador(string? estado) => estado switch
    {
        EstadoTuTurno => EstadoAprobadorPdf.TuTurno,
        EstadoPendiente or null or "" => EstadoAprobadorPdf.Pendiente,
        _ => EstadoAprobadorPdf.Aprobado
    };

    private static string NombreCompleto(FirmanteFlujov2Dto f)
    {
        var nombre = string.Join(" ", new[] { f.Nombres, f.ApellidoPaterno, f.ApellidoMaterno }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
        return string.IsNullOrWhiteSpace(nombre) ? f.NombreUsuarioAsignado ?? string.Empty : nombre;
    }

    private static string NombreArchivo(string? nombre, string? extension) =>
        string.IsNullOrWhiteSpace(extension) ? nombre ?? string.Empty : $"{nombre}.{extension.TrimStart('.')}";

    private async Task<string> NombreTipoSolicitudAsync(long id, CancellationToken cancellationToken) =>
        (await _maestroRepository.GetTipoSolicitudByIdSinEstadoAsync(id, cancellationToken))?.NombreTipoSolicitud ?? string.Empty;

    private async Task<string> NombreSubTipoSolicitudAsync(long idTipo, long id, CancellationToken cancellationToken) =>
        (await _maestroRepository.GetSubTipoSolicitudCombo(null, [idTipo], cancellationToken))
            .FirstOrDefault(s => s.Id == id)?.Nombre ?? string.Empty;

    private async Task<string> NombreTipoOperacionAsync(long id, CancellationToken cancellationToken) =>
        (await _maestroRepository.GetTipoOperacionCombo(null, cancellationToken))
            .FirstOrDefault(t => t.Id == id)?.Nombre ?? string.Empty;
}