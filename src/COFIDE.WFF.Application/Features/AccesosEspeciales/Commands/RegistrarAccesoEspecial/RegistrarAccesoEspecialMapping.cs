using AutoMapper;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial
{
    public class RegistrarAccesoEspecialMapping : Profile
    {
        public RegistrarAccesoEspecialMapping()
        {
            CreateMap<RegistrarAccesoEspecialCommand, AccesoEspecialRegistrarInput>();

            CreateMap<AccesoEspecialRegistrarInput, AccesoEspecial>();
        }
    }
}
