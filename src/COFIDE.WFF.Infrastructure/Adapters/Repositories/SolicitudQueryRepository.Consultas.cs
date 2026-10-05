using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using VO = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

/// <summary>
/// Lectura: datos para notificaciones, códigos de referencia existentes y documentos firmados para descarga.
/// </summary>
public partial class SolicitudQueryRepository
{
    public async Task<FormatoNotificacionSolicitud?> ObtenerParaNotificaciones(long id, CancellationToken cancellationToken)
    {
        var existe = await _context.SolicitudDocumentos
             .CountAsync(
                 d => d.IdSolicitud == id && d.Estado,
                 cancellationToken) > 0;

        var solicitudes = await _context.Solicitudes
             .AsNoTracking()
             .Include(s => s.Documentos.Where(sd => sd.Estado))
             .Include(s => s.SolicitudPlantilla)
             .Where(x => x.Id == id)
             .Select(x => new FormatoNotificacionSolicitud(
                 x.TipoSolicitud.NombreTipoSolicitud,
                 x.CodigoSolicitud,
                 x.UsuarioSolicitante.Email,
                 x.UsuarioSolicitante.Nombres + " " + x.UsuarioSolicitante.ApellidoPaterno + " " + x.UsuarioSolicitante.ApellidoMaterno ?? "",
                 x.SubTipoSolicitud.NombreSubTipoSolicitud,
                 x.TipoOperacion.NombreTipoOperacion,
                 x.Asunto!,
                 x.FechaCreacion.ToString("dd/MM/yyyy"),
                 x.EsUrgente,
                 x.FechaLimite!.Valor,
                 x.MotivoRechazo,
                 x.SolicitudPlantilla.Id,
                 existe
             ))
             .FirstOrDefaultAsync(cancellationToken);

        return solicitudes;
    }

    public async Task<IReadOnlyCollection<CodigoReferenciaExistente>> ObtenerCodigosExistentesAsync(
     long? idSolicitudExcluir,
     IReadOnlyCollection<string> codigos,
     CancellationToken cancellationToken)
    {
        var query = _context.Set<SolicitudCodReferencia>()
        .Where(x => x.Estado && codigos.Contains(x.CodigoReferencia.Valor));

        if (idSolicitudExcluir.HasValue)
        {
            query = query.Where(x => x.IdSolicitud != idSolicitudExcluir.Value);
        }

        return await query
            .Select(x => new CodigoReferenciaExistente(
                x.CodigoReferencia.Valor,
                x.Solicitud.CodigoSolicitud
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ArchivoDescargaDto>> ObtenerDocumentosFirmadosPorSolicitudAsync(
           long idSolicitud,
           CancellationToken cancellationToken,
           bool tracking = false)
    {
        var query = _context.DetalleFirmas
            .Where(df =>
                df.SolicitudDocumento.IdSolicitud == idSolicitud &&
                df.FechaHoraFirma != null &&
                df.NombreDocumentoFirmado != null &&
                df.RutaDocumentoFirmado != null);

        if (!tracking)
            query = query.AsNoTracking();

        var resultado = await query
            .GroupBy(df => df.IdSolicitudDocumento)
            .Select(g => g
                .OrderByDescending(x => x.FechaHoraFirma)
                .Select(x => new ArchivoDescargaDto(
                    x.NombreDocumentoFirmado! + "." + x.ExtensionDocumentoFirmado,
                    x.RutaDocumentoFirmado! + "\\" +
                    x.NombreDocumentoFirmado + "." +
                    x.ExtensionDocumentoFirmado))
                .First())
            .ToListAsync(cancellationToken);

        return resultado;
    }
}
