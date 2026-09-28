using COFIDE.WFF.Application.Common.Abstractions;
namespace COFIDE.WFF.Application.Common.Commands;
/// <summary>
/// Comando base genérico que incluye validación opcional.
/// </summary>
public abstract record BaseCommand<TResponse> : ICommand<TResponse>
{
    // Se puedes colocar aquí propiedades comunes como TenantId, RequestId, etc. si se usa multi-tenant.
}

/// <summary>
/// Comando base para operaciones de escritura sin retorno (void),
/// compatible con el pipeline de CQRS y MediatR.
/// </summary>
public abstract record BaseCommand : ICommand
{
    // Este comando no necesita retorno, útil para operaciones como eliminar, activar, etc.
}