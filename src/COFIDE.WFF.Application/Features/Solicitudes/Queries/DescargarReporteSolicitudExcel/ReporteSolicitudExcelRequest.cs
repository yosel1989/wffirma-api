namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;

public class ReporteSolicitudExcelRequest
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public long? CodigoUsuario { get; set; }
    public long? AreaUsuario { get; set; }
    public string? RoLUsuario { get; set; }
    public long? DepartamentoUsuario { get; set; }
    public List<long>? IdsAreasAccesoEspecial { get; set; }
    public string? Fechainicio { get; set; }
    public string? Fechafin { get; set; }
    public List<long>? Gerencia { get; set; }
    public List<long>? Departamento { get; set; }
    public List<long>? Solicitante { get; set; }
    public List<long>? Tiposolicitud { get; set; }
    public List<long>? Subtiposolicitud { get; set; }
    public List<long>? TipoOperacion { get; set; }
    public List<long>? AprobadorActual { get; set; }
    public List<long>? Estado { get; set; }
    public string? Codigosolicitud { get; set; }
    public string? Asunto { get; set; }
    public string? Referencia { get; set; }
    public bool? Urgente { get; set; }
    public bool? Usuario { get; set; }
    public bool? Usuariodepartamento { get; set; }
}
