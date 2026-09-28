namespace COFIDE.WFF.Api.Configuration;

public class SerilogOptions
{
    public string MinimumLevel { get; set; } = "Information";
    public string OutputTemplate { get; set; } = string.Empty;

    public string InfoLogPath { get; set; } = string.Empty;
    public string ErrorLogPath { get; set; } = string.Empty;

    public int RetainDaysInfo { get; set; }
    public int RetainDaysError { get; set; }

    public bool EnableConsole { get; set; }
}
