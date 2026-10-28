using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.EliminarAccesoEspecial
{
    public sealed class EliminarAccesoEspecialCommandHandler(
        IMapper mapper,
        IAccesoEspecialRepository repository
    ) : BaseCommandHandler<EliminarAccesoEspecialCommand, bool>(mapper)
    {
        private readonly IAccesoEspecialRepository _repository = repository;

        public override async Task<bool> Handle(EliminarAccesoEspecialCommand cmd, CancellationToken cancellationToken)
        {
            var accesoEspecial = await _repository.GetByIdAsync(cmd.Id, tracking: true);

            if (accesoEspecial is null)
                return false;

            _repository.Delete(accesoEspecial);
            return true;
        }
    }
}