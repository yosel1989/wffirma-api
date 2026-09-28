using AutoMapper;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarEnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarEnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Models;
namespace COFIDE.WFF.Application.Features.Solicitudes.Mappings;

public class SolicitudMapping : Profile
{
    public static CrearSolicitudInput ConstruirSolicitudInput(
         SolicitudRequestDto request,
         IReadOnlyCollection<DocumentoProcesadoInput> documentos,
         IReadOnlyCollection<SustentoProcesadoInput> sustentos)
    {
        return new CrearSolicitudInput
        {
            IdTipoSolicitud = request.IdTipoSolicitud,
            IdSubTipoSolicitud = request.IdSubTipoSolicitud,
            IdTipoOperacion = request.IdtipoOperacion,

            Asunto = request.Asunto?.Trim(),
            Mensaje = request.Mensaje?.Trim(),

            EsUrgente = request.EsUrgente,
            InCodigo = request.InCodigo,

            FechaLimite = request.FechaLimite,
            PesoTotalDocu = request.PesoTotalDocu,

            CodigosReferencia = request.CodigosReferencia?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [],

            Correos = request.Correos?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [],

            Documentos = documentos.ToList(),
            Sustentos = sustentos.ToList()
        };
    }

    public static SolicitudEditModel MapearEditarSolicitudModel(
      SolicitudRequestDto request,
      IReadOnlyCollection<DocumentoProcesadoInput> documentos,
      IReadOnlyCollection<SustentoProcesadoInput> sustentos)
    {
        return new SolicitudEditModel
        {
            IdTipoSolicitud = request.IdTipoSolicitud,
            IdSubTipoSolicitud = request.IdSubTipoSolicitud,

            Asunto = request.Asunto,
            Mensaje = request.Mensaje,
            EsUrgente = request.EsUrgente,
            InCodigo = request.InCodigo,
            FechaLimite = request.FechaLimite,

            DocumentoIdsConservar = request.IdsDocumentosConservar?.ToList() ?? [],
            SustentoIdsConservar = request.IdsSustentosConservar?.ToList() ?? [],

            NuevosDocumentosRequest = documentos.ToList(),
            NuevosSustentosRequest = sustentos.ToList(),
            CodigosReferencia = request.CodigosReferencia?.ToList() ?? [],
            Correos = request.Correos?.ToList() ?? [],
        };
    }

    public SolicitudMapping()
    {
        CreateMap<RegistrarSolicitudCommand, SolicitudRequestDto>();
        CreateMap<EnviarSolicitudCommand, SolicitudRequestDto>();
        CreateMap<ReusarSolicitudCommand, SolicitudRequestDto>();
        CreateMap<ReusarEnviarSolicitudCommand, SolicitudRequestDto>();
        CreateMap<EditarSolicitudCommand, SolicitudRequestDto>();
        CreateMap<EditarEnviarSolicitudCommand, SolicitudRequestDto>();
    }

}


