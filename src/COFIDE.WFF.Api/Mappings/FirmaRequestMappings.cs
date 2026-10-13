

using COFIDE.WFF.Api.Request.Firmas;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaWebHook;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;
using COFIDE.WFF.Application.Features.Firmas.Commands.ValidarEstadoDetalleFirma;

namespace COFIDE.WFF.Api.Mappings
{
    public static class FirmaRequestMappings
    {
        public static FirmaExternaCommand ToCommand(this FirmaExternaRequest r, long idSolicitud) =>
            new(idSolicitud, r.IdSolicitudDocumento, r.Documento.ToArchivoEntrada());

        public static FirmaInternaPdfCommand ToCommand(this FirmarDocumentoRequest r, long idSolicitud) =>
            new(idSolicitud, r.IdSolicitudDocumento);

        public static FirmaInternaZipCommand ToCommand(this FirmarDocumentosZipRequest r, long idSolicitud) =>
            new(idSolicitud, r.Documentos);

        public static FirmaInternaWebHookCommand ToCommand(this FirmaInternaResultadoRequest r) =>
            new(r.Code, r.DocumentId, r.Date, r.Signer, r.DniSigner);

        public static ValidarEstadoDetalleFirmaCommand ToCommand(this ValidarDetallesFirmaRequest r, long idSolicitud) =>
            new(idSolicitud, r.Detalles);

        public static RestablecerDocumentoFirmadoCommand ToCommand(this RestablecerFirmasRequest r) =>
            new(r.IdDetalleFirma);
    }

}
