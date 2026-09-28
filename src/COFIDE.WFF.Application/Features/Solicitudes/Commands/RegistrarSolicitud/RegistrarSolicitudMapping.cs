using AutoMapper;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;

public class RegistrarSolicitudMapping : Profile
{
    public RegistrarSolicitudMapping()
    {
        CreateMap<RegistrarSolicitudCommand, CrearSolicitudInput>();

        CreateMap<CrearSolicitudInput, Solicitud>();
    }
}
