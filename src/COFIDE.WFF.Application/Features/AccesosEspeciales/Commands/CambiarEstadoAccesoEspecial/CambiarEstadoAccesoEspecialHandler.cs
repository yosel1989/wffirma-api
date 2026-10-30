using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.CambiarEstadoAccesoEspecial
{
    public sealed class CambiarEstadoAccesoEspecialHandler(
        IMapper mapper,
        IAccesoEspecialRepository repository
    ) : BaseCommandHandler<CambiarEstadoAccesoEspecialCommand, bool>(mapper)
    {
        private readonly IAccesoEspecialRepository _repository = repository;

        public override async Task<bool> Handle(CambiarEstadoAccesoEspecialCommand cmd, CancellationToken cancellationToken)
        {
            var accesoEspecial = await _repository.GetByIdAnyStatusAsync(cmd.Id, tracking: true);

            if (accesoEspecial is null)
                return false;

            if (cmd.Estado) {
                accesoEspecial.Activar();
            }
            else
            {
                accesoEspecial.Desactivar();
            }

            await _repository.UpdateAsync(accesoEspecial);
            return true;
        }
    }
}
