using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;
using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Domain.DomainServices;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSeguimientoExcel;

public sealed class DescargarReporteSeguimientoExcelQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudSeguimientoRepository solicitudQueryRepository,
        ISeguimientoSolicitudExcelReportService excelReportService
    ) : BaseQueryHandler<DescargarReporteSeguimientoExcelQuery, ReporteSeguimientoExcelDto>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudSeguimientoRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly ISeguimientoSolicitudExcelReportService _excelReportService = excelReportService;

    public override async Task<ReporteSeguimientoExcelDto> Handle(DescargarReporteSeguimientoExcelQuery qry, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        if (_currentUser.UserCargoId is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal;
        var areaUsuario = _currentUser.UserAreaId;
        var rolUsuario = _currentUser.CodigoRolUsuario;
        var idDepartamento = _currentUser.UserAreaId;

        var request = new ReporteSeguimientoExcelRequest
        {
            CodigoUsuario = idUsuario,
            AreaUsuario = areaUsuario,
            RoLUsuario = rolUsuario,
            DepartamentoUsuario = idDepartamento,
            Fechainicio = qry.fechainicio,
            Fechafin = qry.fechafin,
            Gerencia = qry.gerencia,
            Departamento = qry.departamento,
            Solicitante = qry.solicitante,
            Tiposolicitud = qry.tiposolicitud,
            Subtiposolicitud = qry.subtiposolicitud,
            TipoOperacion = qry.tipoOperacion,
            Asunto = qry.asunto,
            Referencia = qry.referencia,
            Estado = qry.estado,
            Firmante = qry.firmante,
            Codigosolicitud = qry.codigosolicitud,
            Encurso = qry.encurso,
            Aprobada = qry.aprobada,
            Rechazada = qry.rechazada,
        };

        var solicitudes = _solicitudQueryRepository.ObtenerReporteSeguimiento(
            request,
            cancellationToken: cancellationToken)
            .ToList();
        ;

        foreach (var solicitud in solicitudes)
        {
            AsignarEstadoFirmantes(solicitud);
            solicitud.Firmantes = SeguimientoSolicitudesMapping.AgruparFirmantes(solicitud.Firmantes);
        }

        var contenidoExcel = _excelReportService.GenerarReporteSeguimiento(solicitudes);

        return new ReporteSeguimientoExcelDto
        {
            Contenido = contenidoExcel,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            NombreArchivo = $"Reporte_Seguimiento_Solicitudes_{HoraSistema.Ahora:yyyyMMdd_HHmmss}.xlsx"
        };


    }

    private static void AsignarEstadoFirmantes(SeguimientoSolicitudesDto solicitud)
    {
        if (solicitud.Firmantes is null)
            return;

        foreach (var firmante in solicitud.Firmantes)
        {
            firmante.Estado = ReglaEstadoFirmante.Determinar(
                    solicitud.IdEstado,
                    solicitud.CicloActual,
                    firmante.UltimoCiclo,
                    firmante.UltimoAprobado,
                    firmante.RevAprobado,
                    firmante.FechaRevision)
                .ToString()
                .ToUpperInvariant();
        }
    }
}
