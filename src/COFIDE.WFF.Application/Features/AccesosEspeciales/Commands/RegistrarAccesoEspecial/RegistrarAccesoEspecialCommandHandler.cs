using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Services;
namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial
{
    public sealed class RegistrarAccesoEspecialCommandHandler(
        IMapper mapper,
        IAccesoEspecialRegistrarService usecase,
        IAccesoEspecialRepository repository
    ) : BaseCommandHandler<RegistrarAccesoEspecialCommand, long>(mapper)
    {
        private readonly IAccesoEspecialRegistrarService _usecase = usecase;
        private readonly IAccesoEspecialRepository _repository = repository;

        public override async Task<long> Handle(RegistrarAccesoEspecialCommand cmd, CancellationToken cancellationToken)
        {
            var request = _mapper.Map<AccesoEspecialRequestDto>(cmd);
            var accesoEspecial = _usecase.AccesoEspecialRegistrar(request, cancellationToken);

            await _repository.AddAsync(accesoEspecial);
            return accesoEspecial.Id;
        }
    }
}
