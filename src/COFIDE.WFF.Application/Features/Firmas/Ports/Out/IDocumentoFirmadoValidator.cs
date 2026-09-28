using COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Firmas.Ports.Out;

public interface IDocumentoFirmadoValidator
{
    Task<bool> TieneFirmaAsync(Stream documentoFirmado);
    Task<ResultadoValidacionFirma> Externo_ValidarDocumentoFirmadoAsync(
         Stream documentoOriginal,
         Stream documentoFirmado,
         string hashOriginal,
         string usuarioEsperado,
         string? ultimoDniFirmanteValidoBaseDatos,
         bool tieneDocumentoFirmado);

    Task<ResultadoValidacionFirma> ValidarFirmasAnterioresAsync(
            Stream documentoAntesDeFirmarConFirma,
            string hashDocumentoAntesDeFirmar,
            IList<string> ListaDniFirmados);

    Task<string> Interno_ValidarDocumentoFirmadoAsync(
      Stream documentoOriginal,
      Stream documentoFirmado);
}
