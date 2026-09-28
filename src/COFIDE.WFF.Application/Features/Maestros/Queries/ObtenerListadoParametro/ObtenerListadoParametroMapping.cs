using AutoMapper;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerListadoParametro;

public class ObtenerListadoParametroMapping : Profile
{
    public ObtenerListadoParametroMapping()
    {
        CreateMap<ParametroDetalle, ListadoParametroDto>()
            .ForMember(d => d.CodigoParametro,
                opt => opt.MapFrom(s => s.Cabecera.CodigoParametro))
            .ForMember(d => d.IdParametroDetalle,
                opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.CodigoDetalle,
                opt => opt.MapFrom(s => s.CodigoDetalle))
            .ForMember(d => d.ValorParametro,
                opt => opt.MapFrom(s => s.ValorParametro));
    }
}