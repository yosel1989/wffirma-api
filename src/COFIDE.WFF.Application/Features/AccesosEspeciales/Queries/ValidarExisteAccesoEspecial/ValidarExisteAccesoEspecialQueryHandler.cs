using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
using MediatR;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ValidarExisteAccesoEspecial
{
    public sealed class ValidarExisteAccesoEspecialQueryHandler(
        ICurrentUserService currentUser,
        IAccesoEspecialQueryRepository queryRepository
    ) : IRequestHandler<ValidarExisteAccesoEspecialQuery, bool>
    {
        private readonly ICurrentUserService _currentUser = currentUser;
        private readonly IAccesoEspecialQueryRepository _queryRepository = queryRepository;
        public async Task<bool> Handle(ValidarExisteAccesoEspecialQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserIdLocal is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            if (_currentUser.UserCargoId is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            bool exists = await _queryRepository.ExistAsync(
                request.IdUsuario,
                request.IdArea,
                cancellationToken);

            return exists;
        }
    }
}
