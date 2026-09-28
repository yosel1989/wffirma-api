using AutoMapper;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitud;

public class SubTipoSolicitudMappingProfile : Profile
{
    public SubTipoSolicitudMappingProfile()
    {
        CreateMap<SubTipoSolicitud, SubTipoSolicitudDto>();
    }
}
