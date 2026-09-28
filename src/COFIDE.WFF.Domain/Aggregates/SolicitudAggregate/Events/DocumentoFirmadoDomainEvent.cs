using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Events;

namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
public sealed record DocumentoFirmadoDomainEvent(
     long IdDetalleFirma, long IdSolicitud, long IdSolicitudDocumento, bool firmado, string CodigoUsuario, string Mensaje
) : DomainEvent, IPostCommitDomainEvent;