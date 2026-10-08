using COFIDE.WFF.Api.Request.Revisiones;
using COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;

namespace COFIDE.WFF.Api.Mappings
{
    /// <summary>Request (contrato HTTP) → Command (contrato de Application) para Revisiones.</summary>
    public static class RevisionRequestMappings
    {
        public static EditarFlujoCommand ToCommand(this ActualizarFlujoRequest r, long idSolicitud, long idPlantilla) =>
            new(idSolicitud, idPlantilla, r.Etapas);
    }
}
