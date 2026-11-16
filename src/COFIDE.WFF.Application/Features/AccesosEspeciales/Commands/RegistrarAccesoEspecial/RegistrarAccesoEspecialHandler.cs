using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Services;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial
{
    public sealed class RegistrarAccesoEspecialHandler(
        ICurrentUserService currentUser,
        IMapper mapper,
        IAccesoEspecialRegistrarService usecase,
        IAccesoEspecialRepository repository,
        IUsuarioRepository usuarioRepository
    ) : BaseCommandHandler<RegistrarAccesoEspecialCommand, long>(mapper)
    {
        private readonly ICurrentUserService _currentUser = currentUser;
        private readonly IAccesoEspecialRegistrarService _usecase = usecase;
        private readonly IAccesoEspecialRepository _repository = repository;
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

        public override async Task<long> Handle(RegistrarAccesoEspecialCommand cmd, CancellationToken cancellationToken)
        {
            if (_currentUser.UserIdLocal is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            if (_currentUser.UserCargoId is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            var rolUsuario = _currentUser.CodigoRolUsuario;
            if (RolUsuarioCodigo.Desde(rolUsuario) != RolUsuario.Administrador)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.Forbidden);

            var usuario = await _usuarioRepository.GetByIdAsync(cmd.IdUsuario);
            if (usuario is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UsuarioNotFound);

            if (usuario.IdArea == cmd.IdArea)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.AccesoEspecialUserAlreadyInArea);

            var request = _mapper.Map<AccesoEspecialRequestDto>(cmd);
            var accesoEspecial = _usecase.AccesoEspecialRegistrar(request, cancellationToken);

            await _repository.AddAsync(accesoEspecial);
            return accesoEspecial.Id;
        }
    }
}
