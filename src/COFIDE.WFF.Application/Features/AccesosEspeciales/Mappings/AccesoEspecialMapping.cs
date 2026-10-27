using AutoMapper;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Inputs;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Mappings
{
    public class AccesoEspecialMapping : Profile
    {
        public static AccesoEspecialRegistrarInput ConstruirAccesoEspecialInput(
             AccesoEspecialRequestDto request)
        {
            return new AccesoEspecialRegistrarInput
            {
                IdUsuario = request.IdUsuario,
                IdGerencia = request.IdGerencia,
                IdDepartamento = request.IdDepartamento
            };
        }

        public AccesoEspecialMapping()
        {
            CreateMap<RegistrarAccesoEspecialCommand, AccesoEspecialRequestDto>();
        }

    }
}
