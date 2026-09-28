namespace COFIDE.WFF.Domain.Entities
{
    public class FirmaWebhook
    {
        public string ID_FIRMA { get; set; } = string.Empty;
        public int STATUS { get; set; }
        public string? CL_PAYLOAD { get; set; }
        public string? VC_USUARIO_CREACION { get; set; }
        public DateTime? TS_FECHA_CREACION { get; set; }
        public string? VC_TERMINAL_CREACION { get; set; }
        public string? URL_TERMINAL_CREACION { get; set; }
        public string? VC_USUARIO_MODIFICACION { get; set; }
        public DateTime? TS_FECHA_MODIFICACION { get; set; }
        public string? VC_TERMINAL_MODIFICACION { get; set; }
        public string? VC_SISTEMA_ORIGEN { get; set; }
        public string? VC_NOMBRE_TABLA { get; set; }
        public long? INT_COD_TABLA { get; set; }
    }
}
