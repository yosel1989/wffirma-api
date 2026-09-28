using AutoMapper;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitud;

public class TipoSolicitudMappingProfile : Profile
{
    public TipoSolicitudMappingProfile()
    {
        CreateMap<TipoSolicitud, TipoSolicitudDto>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
          .ForMember(dest => dest.NombreTipoSolicitud, opt => opt.MapFrom(src => src.NombreTipoSolicitud));
    }
}
