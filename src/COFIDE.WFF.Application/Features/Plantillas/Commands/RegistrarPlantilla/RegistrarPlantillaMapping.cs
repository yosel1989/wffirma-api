using AutoMapper;
using COFIDE.WFF.Application.Features.Plantillas.Models;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
namespace COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;

public class RegistrarPlantillaMapping : Profile
{
    public RegistrarPlantillaMapping()
    {
        CreateMap<RegistrarPlantillaCommand, CrearPlantillaInput>()
            .ForMember(dest => dest.NombrePlantilla, opt => opt.MapFrom(src => src.NombrePlantilla))
            .ForMember(dest => dest.Visibilidad, opt => opt.MapFrom(src => src.Visibilidad ?? false))
            .ForMember(dest => dest.Etapas, opt => opt.MapFrom(src => src.Etapas));

        CreateMap<PlantillaEtapaRequest, EtapaInput>()
            .ForMember(dest => dest.OrdenEtapa, opt => opt.MapFrom(src => src.OrdenEtapa));

        CreateMap<FirmanteFlujoRequest, FirmanteInput>();
    }
}
