using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.DomainServices;
using MediatR;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

public sealed class SeguimientoSolicitudesQueryHandler(
    ICurrentUserService currentUser,
        ISolicitudSeguimientoRepository solicitudQueryRepository
    ) : IRequestHandler<SeguimientoSolicitudesQuery, SeguimientoSolicitudesResponseDto>
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudSeguimientoRepository _solicitudQueryRepository = solicitudQueryRepository;

    public async Task<SeguimientoSolicitudesResponseDto> Handle(SeguimientoSolicitudesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);
        if (_currentUser.UserCargoId is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal;
        var areaUsuario = _currentUser.UserAreaId;
        var rolUsuario = _currentUser.CodigoRolUsuario;
        var idDepartamento = _currentUser.UserAreaId;

        var paginationParams = new PagedQueryParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm
        };

        var pagedResult = await _solicitudQueryRepository.ObtenerPaginadoSeguimiento(
            paginationParams,
            idUsuario,
            areaUsuario,
            rolUsuario,
            idDepartamento,
            request.Fechainicio,
            request.Fechafin,
            request.Gerencia,
            request.Departamento,
            request.Solicitante,
            request.Tiposolicitud,
            request.Subtiposolicitud,
            request.TipoOperacion,
            request.Asunto,
            request.Referencia,
            request.Estado,
            request.Firmante,
            request.Codigosolicitud,
            request.Encurso,
            request.Aprobada,
            request.Rechazada,
            request.IncluirResumen,
            cancellationToken);

        var resumen = pagedResult.ExtraData!;
        var solicitudes = pagedResult.Items.ToList();

        foreach (var solicitud in solicitudes)
        {
            AsignarEstadoFirmantes(solicitud);
            solicitud.Firmantes = SeguimientoSolicitudesMapping.AgruparFirmantes(solicitud.Firmantes);
        }

        return new SeguimientoSolicitudesResponseDto
        {
            Total = resumen.Total,
            EnCurso = resumen.EnCurso,
            Aprobado = resumen.Aprobado,
            Rechazado = resumen.Rechazado,
            ListadoSolicitudes = solicitudes,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
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
