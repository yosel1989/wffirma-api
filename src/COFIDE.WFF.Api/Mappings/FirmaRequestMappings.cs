

using COFIDE.WFF.Api.Request.Firmas;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;

namespace COFIDE.WFF.Api.Mappings
{
    public static class FirmaRequestMappings
    {
        public static FirmaExternaCommand ToCommand(this FirmaExternaRequest r, long idSolicitud) =>
            new(idSolicitud, r.IdSolicitudDocumento, r.Documento.ToArchivoEntrada());
    }

}
