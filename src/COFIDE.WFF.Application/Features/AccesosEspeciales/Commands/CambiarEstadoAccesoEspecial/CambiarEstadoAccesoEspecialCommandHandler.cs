using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.CambiarEstadoAccesoEspecial
{
    public sealed class CambiarEstadoAccesoEspecialCommandHandler(
        ICurrentUserService currentUser,
        IMapper mapper,
        IAccesoEspecialRepository repository
    ) : BaseCommandHandler<CambiarEstadoAccesoEspecialCommand, bool>(mapper)
    {
        private readonly ICurrentUserService _currentUser = currentUser;
        private readonly IAccesoEspecialRepository _repository = repository;

        public override async Task<bool> Handle(CambiarEstadoAccesoEspecialCommand cmd, CancellationToken cancellationToken)
        {
            if (_currentUser.UserIdLocal is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            if (_currentUser.UserCargoId is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            var rolUsuario = _currentUser.CodigoRolUsuario;
            if (RolUsuarioCodigo.Desde(rolUsuario) != RolUsuario.Administrador)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.Forbidden);

            var accesoEspecial = await _repository.GetByIdAnyStatusAsync(cmd.Id, tracking: true);

            if (accesoEspecial is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.AccesoEspecialNotExists);

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
