using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;

namespace COFIDE.WFF.Worker.Services;

public class WorkerCurrentUserService : ICurrentUserService
{
    private const string WorkerName = "WORKER";
    private const string WorkerIp = "127.0.0.1";
    private const string WorkerDni = "00000000";
    private const string WorkerEmail = "worker@local";

    public bool Autenticado => true;
    public string? CodigoUsuario => WorkerName;
    public string? IpAddress => WorkerIp;
    public long? UserIdLocal => 0;
    public string? UserDni => WorkerDni;
    public long? UserAreaId => 0;
    public string? UserAreaNombre => WorkerName;
    public long? UserAreaPadreId => 0;
    public string? UserAreaDescripcionCorta => WorkerName;
    public long? UserCargoId => 0;
    public string? UserGerenciaNombre => WorkerName;
    public string? UserCodigoRolUsuario => WorkerName;
    public string? UserCorreo => WorkerEmail;
    public string? NombreUsuario => WorkerName;
    public string? ApellidoPaterno => "SYSTEM";
    public string? ApellidoMaterno => "JOB";
    public string? CodigoRolUsuario => WorkerName;
    public string? NombreRolUsuario => WorkerName;
    public RolUsuario? RolUsuario => null;
}