namespace COFIDE.WFF.Application.Common.Ports.Out;

/// <summary>
/// Compensación de archivos escritos durante una transacción.
/// Si la transacción se revierte, los archivos nuevos se eliminan y los sobrescritos se restauran.
/// </summary>
public interface IRegistroArchivosTransaccion
{
    /// <summary>La transacción se confirmó: se descartan los respaldos.</summary>
    void ConfirmarArchivos();

    /// <summary>La transacción se revirtió: se deshacen los cambios en el almacenamiento.</summary>
    Task DescartarArchivosAsync(CancellationToken cancellationToken);
}
