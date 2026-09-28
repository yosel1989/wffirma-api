using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.ValueObjects;

public sealed class TipoEvento : ValueObject
{
    public static readonly TipoEvento AprovalRejected = new(1, "aproval_rejected");
    public static readonly TipoEvento AprovalDelegated = new(2, "aproval_delegated");
    public static readonly TipoEvento FlowApproverAdded = new(3, "flow_aprover_added");
    public static readonly TipoEvento FlowApproverRemoved = new(4, "flow_aprover_removed");
    public static readonly TipoEvento FlowStageAdded = new(5, "flow_stage_added");
    public static readonly TipoEvento FlowStageRemoved = new(6, "flow_stage_removed");
    public static readonly TipoEvento DocumentAdded = new(7, "document_added");
    public static readonly TipoEvento RequestCancelled = new(8, "request_cancelled");
    public static readonly TipoEvento FlowReverted = new(9, "flow_reverted");

    private static readonly Dictionary<long, TipoEvento> _estados =
    new()
        {
            {1, AprovalRejected},
            {2, AprovalDelegated},
            {3, FlowApproverAdded},
            {4, FlowApproverRemoved},
            {5, FlowStageAdded},
            {6, FlowStageRemoved},
            {7, DocumentAdded},
            {8, RequestCancelled},
            {9, FlowReverted}
        };

    public long Codigo { get; }
    public string Nombre { get; }

    private TipoEvento(long codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static TipoEvento FromCodigo(long codigo)
    {
        if (!_estados.TryGetValue(codigo, out var estado))
            throw new DomainException(new Common.Errors.DomainError("", $"Tipo de Evento inválido: {codigo}"));

        return estado;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
    }

    public override string ToString() => Nombre;
}