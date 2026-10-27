using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Factories;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Services
{
    public class AccesoEspecialRegistrarService() : IAccesoEspecialRegistrarService
    {
        public AccesoEspecial AccesoEspecialRegistrar(
            AccesoEspecialRequestDto request,
            CancellationToken cancellationToken)
        {

            #region 01. Traer la información de Código
            var fechaActual = HoraSistema.Ahora;
            var anio = fechaActual.Year;
            #endregion

            var input = MapearInputAccesoEspecial(request);
            var solicitudBase = AccesoEspecialFactory.AccesoEspecialRegistrar(input);
            return solicitudBase;
        }


        private static AccesoEspecialRegistrarInput MapearInputAccesoEspecial(AccesoEspecialRequestDto request)
        {
            return new AccesoEspecialRegistrarInput
            {
                IdUsuario = request.IdUsuario,
                IdGerencia = request.IdGerencia,
                IdDepartamento = request.IdDepartamento,
            };
        }
    }
}
