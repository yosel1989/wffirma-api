using System.ServiceModel;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.ConfiguracionSuc.Contracts;

[ServiceContract(
    Namespace = "http://cofide.com.pe/ConfiguracionSUC/2016/02")]
public interface IConfiguracionSucService
{
    [OperationContract(
        Action =
        "http://cofide.com.pe/ConfiguracionSUC/2016/02/ConfiguracionSUC/obtenerCadenaConexion",
        ReplyAction =
        "http://cofide.com.pe/ConfiguracionSUC/2016/02/ConfiguracionSUC/obtenerCadenaConexionResponse")]
    Task<CadenaConexionDto> obtenerCadenaConexionAsync(
        string strCodigoSistema,
        string strCodigoCadena);
}