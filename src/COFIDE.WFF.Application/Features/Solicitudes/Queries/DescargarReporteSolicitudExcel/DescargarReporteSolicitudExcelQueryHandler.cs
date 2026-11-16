using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudExcel;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;

public sealed class DescargarReporteSolicitudExcelQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudQueryRepository solicitudQueryRepository,
        ISolicitudExcelReportService excelReportService,
        IAccesoEspecialRepository accesoEspecialRepository
    ) : BaseQueryHandler<DescargarReporteSolicitudExcelQuery, ReporteSolicitudExcelDto>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly ISolicitudExcelReportService _excelReportService = excelReportService;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public override async Task<ReporteSolicitudExcelDto> Handle(DescargarReporteSolicitudExcelQuery qry, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        if (_currentUser.UserCargoId is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal;
        var areaUsuario = _currentUser.UserAreaId;
        var rolUsuario = _currentUser.CodigoRolUsuario;
        var idDepartamento = _currentUser.UserAreaId;
        var idsAreasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario.Value);

        var request = new ReporteSolicitudExcelRequest
        {
            Skip = 0,
            Take = 999999,
            CodigoUsuario = idUsuario,
            AreaUsuario = areaUsuario,
            RoLUsuario = rolUsuario,
            DepartamentoUsuario = idDepartamento,
            IdsAreasAccesoEspecial = idsAreasAccesoEspecial,
            Fechainicio = qry.fechainicio,
            Fechafin = qry.fechafin,
            Gerencia = qry.gerencia,
            Departamento = qry.departamento,
            Solicitante = qry.solicitante,
            Tiposolicitud = qry.tiposolicitud,
            Subtiposolicitud = qry.subtiposolicitud,
            TipoOperacion = qry.tipoOperacion,
            AprobadorActual = qry.aprobadorActual,
            Estado = qry.estado,
            Codigosolicitud = qry.codigosolicitud,
            Asunto = qry.asunto,
            Referencia = qry.referencia,
            Urgente = qry.urgente,
            Usuario = qry.usuario,
            Usuariodepartamento = qry.usuariodepartamento,
        };

        var solicitudes = await _solicitudQueryRepository.ObtenerListadoSolicitud(
            request,
            cancellationToken: cancellationToken);

        var contenidoExcel = _excelReportService.GenerarReporteSolicitudes(solicitudes);

        return new ReporteSolicitudExcelDto
        {
            Contenido = contenidoExcel,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            NombreArchivo = $"Reporte_Solicitudes_Cantidad_{solicitudes.Count}_{HoraSistema.Ahora:yyyyMMdd_HHmmss}.xlsx"
        };
    }
}
