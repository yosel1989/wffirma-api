using AutoMapper;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacion;

public class TipoOperacionMappingProfile : Profile
{
    public TipoOperacionMappingProfile()
    {
        CreateMap<TipoOperacion, TipoOperacionDto>();
    }
}
