using System.Runtime.Serialization;
namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.ConfiguracionSuc.Contracts;

[DataContract(
    Namespace = "http://schemas.datacontract.org/2004/07/Dominio.SUC.Entities")]
public sealed class CadenaConexionDto
{
    [DataMember]
    public int intIdCadena { get; set; }

    [DataMember]
    public int intIdSistema { get; set; }

    [DataMember]
    public string? strCodigo { get; set; }

    [DataMember]
    public string? chrFlagMotor { get; set; }

    [DataMember]
    public string? strValor { get; set; }

    [DataMember]
    public string? strDescripcion { get; set; }

    [DataMember]
    public char? chrFlagEstado { get; set; }

    [DataMember]
    public string? strSistemaDesc { get; set; }

    [DataMember]
    public string? strFlagMotorDesc { get; set; }

    [DataMember]
    public string? strEstadoDesc { get; set; }

    [DataMember]
    public string? strUsuarioCreacion { get; set; }

    [DataMember]
    public DateTime dtFechaCreacion { get; set; }

    [DataMember]
    public string? strUsuarioModificacion { get; set; }

    [DataMember]
    public DateTime dtFechaModificacion { get; set; }
}