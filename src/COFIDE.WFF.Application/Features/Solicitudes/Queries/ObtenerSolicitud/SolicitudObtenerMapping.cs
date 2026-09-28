using AutoMapper;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;

public class SolicitudObtenerMapping : Profile
{
    public SolicitudObtenerMapping()
    {
        CreateMap<Solicitud, SolicitudDto>()
            .ForMember(d => d.IdSolicitud, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.IdTipoSolicitud, opt => opt.MapFrom(s => s.IdTipoSolicitud))
            .ForMember(d => d.IdSubTipoSolicitud, opt => opt.MapFrom(s => s.IdSubTipoSolicitud))
            .ForMember(d => d.IdtipoOperacion, opt => opt.MapFrom(s => s.IdTipoOperacion));

        CreateMap<SolicitudDocumento, ArchivoDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Nombre, opt => opt.MapFrom(s => s.NombreOriginal));

        CreateMap<SolicitudSustento, ArchivoDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Nombre, opt => opt.MapFrom(s => s.NombreSustento));

        CreateMap<SolicitudSustento, SustentoDto>();
    }
}
