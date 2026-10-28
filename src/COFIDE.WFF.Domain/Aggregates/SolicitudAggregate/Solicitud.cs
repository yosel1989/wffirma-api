using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Models;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.ValueObjects;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class Solicitud : AuditableEntity<long>
{
    #region Propiedades
    public string CodigoSolicitud { get; private set; } = default!;
    public long IdTipoSolicitud { get; private set; }
    public long IdSubTipoSolicitud { get; private set; }
    public long IdEstadoSolicitud { get; private set; }
    public ValueObject.EstadoSolicitud EstadoSolicitud => ValueObject.EstadoSolicitud.FromCodigo(IdEstadoSolicitud);
    public long IdUsuarioSolicitante { get; private set; }
    public DateTime FechaSolicitud { get; private set; }
    public long IdTipoOperacion { get; private set; }
    public long IdUsuarioAccion { get; private set; }
    public long IdAreaSolicitud { get; private set; }
    public long IdAreaPadreSolicitud { get; private set; }
    public DateTime FechaAccion { get; private set; }
    public string? Asunto { get; private set; }
    public string? Mensaje { get; private set; }
    public bool EsUrgente { get; private set; }
    public bool InCodigo { get; private set; }
    public Vo.FechaLimite? FechaLimite { get; private set; }
    public string? MotivoRechazo { get; private set; }
    public decimal? PesoTotalDocu { get; private set; }
    #endregion

    #region Relaciones
    public TipoSolicitud TipoSolicitud { get; private set; } = null!;
    public SubTipoSolicitud SubTipoSolicitud { get; private set; } = null!;
    public TipoOperacion TipoOperacion { get; private set; } = null!;
    public Usuario UsuarioSolicitante { get; private set; } = null!;
    public SolicitudPlantilla SolicitudPlantilla { get; private set; } = default!;

    private readonly List<SolicitudDocumento> _documentos = [];
    private readonly List<SolicitudSustento> _sustentos = [];
    private readonly List<SolicitudCodReferencia> _codigosReferencia = [];
    private readonly List<SolicitudCorreo> _correos = [];
    private readonly List<SolicitudComentario> _comentarios = [];
    private readonly List<SolicitudEvento> _eventos = [];

    public IReadOnlyCollection<SolicitudDocumento> Documentos => _documentos;
    public IReadOnlyCollection<SolicitudSustento> Sustentos => _sustentos;
    public IReadOnlyCollection<SolicitudCodReferencia> CodigosReferencia => _codigosReferencia;
    public IReadOnlyCollection<SolicitudCorreo> Correos => _correos;
    public IReadOnlyCollection<SolicitudComentario> Comentarios => _comentarios;
    public IReadOnlyCollection<SolicitudEvento> Eventos => _eventos;
    #endregion

    private Solicitud() { }

    #region Helpers
    private void CambiarEstado(ValueObject.EstadoSolicitud estado)
    {
        IdEstadoSolicitud = estado.Codigo;
    }
    #endregion

    #region Creación del borrador
    public static Solicitud CrearBorrador(
        long id,
        string codigoSolicitud,
        long idTipoSolicitud,
        long idSubTipoSolicitud,
        long idUsuarioSolicitante,
        long idTipoOperacion,
        long idUsuarioAccion,
        long idAreaSolicitud,
        long idAreaPadreSolicitud,
        string? asunto,
        string? mensaje,
        bool esUrgente,
        bool inCodigo,
        DateTime? fechaLimite,
        decimal? pesoTotalDocu)
    {
        var solicitud = new Solicitud
        {
            Id = id,
            CodigoSolicitud = codigoSolicitud,
            IdTipoSolicitud = idTipoSolicitud,
            IdSubTipoSolicitud = idSubTipoSolicitud,
            IdUsuarioSolicitante = idUsuarioSolicitante,
            FechaSolicitud = HoraSistema.Ahora,
            IdTipoOperacion = idTipoOperacion,
            IdUsuarioAccion = idUsuarioAccion,
            IdAreaSolicitud = idAreaSolicitud,
            IdAreaPadreSolicitud = idAreaPadreSolicitud,
            FechaAccion = HoraSistema.Ahora,
            Asunto = asunto,
            Mensaje = mensaje,
            EsUrgente = esUrgente,
            InCodigo = inCodigo,
            PesoTotalDocu = pesoTotalDocu,
            Estado = true
        };

        solicitud.CambiarEstado(ValueObject.EstadoSolicitud.Borrador);
        solicitud.DefinirUrgencia(esUrgente, fechaLimite);

        return solicitud;
    }

    public void NotificarCorreoBorrador(string? correoSolicitante)
    {
        AddDomainEvent(new SolicitudCreadaBorradorDomainEvent(Id, correoSolicitante));
    }
    #endregion

    #region Correlativos

    public int ContarNuevosCodigosReferencia(List<string>? codigosReferencia)
    {
        var nuevos = Normalizar(codigosReferencia);

        var actuales = _codigosReferencia
            .Where(x => x.Estado)
            .Select(x => x.CodigoReferencia.Valor)
            .ToHashSet();

        return nuevos.Count(x => !actuales.Contains(x));
    }

    public int ContarNuevosCorreos(List<string>? correos)
    {
        var nuevos = Normalizar(correos);

        var actuales = _correos
            .Where(x => x.Estado)
            .Select(x => x.Correo.Value)
            .ToHashSet();

        return nuevos.Count(x => !actuales.Contains(x));
    }

    #endregion

    #region Editar Borrador
    public void EditarSolicitudBorrador(
        SolicitudEditModel solicitudRequest,
        Func<long> generarIdDocumento,
        Func<long> generarIdSustento,
        Func<long> generarIdCodigoReferencia,
        Func<long> generarIdCorreo,
        long usuarioActualId)
    {

        if (EstadoSolicitud != ValueObject.EstadoSolicitud.Borrador && EstadoSolicitud != ValueObject.EstadoSolicitud.EnCurso)
            throw new DomainException(SolicitudErrors.EditarSoloBorrador);

        IdTipoSolicitud = solicitudRequest.IdTipoSolicitud;
        IdSubTipoSolicitud = solicitudRequest.IdSubTipoSolicitud;
        Asunto = solicitudRequest.Asunto;
        Mensaje = solicitudRequest.Mensaje;
        EsUrgente = solicitudRequest.EsUrgente;
        InCodigo = solicitudRequest.InCodigo;
        DefinirUrgencia(solicitudRequest.EsUrgente, solicitudRequest.FechaLimite);

        IdUsuarioAccion = usuarioActualId;
        FechaAccion = HoraSistema.Ahora;

        var idsDocumentosConservar = solicitudRequest.DocumentoIdsConservar.ToHashSet();
        var idsSustentosConservar = solicitudRequest.SustentoIdsConservar.ToHashSet();

        foreach (var documento in _documentos.Where(x => x.Estado && !idsDocumentosConservar.Contains(x.Id)))
        {
            documento.Inactivar();
        }

        foreach (var nuevoDocumento in solicitudRequest.NuevosDocumentosRequest)
        {
            AgregarDocumento(generarIdDocumento(), nuevoDocumento.NombreOriginal, nuevoDocumento.ExtensionOriginal, nuevoDocumento.RutaOriginal, nuevoDocumento.HashOriginal,
                nuevoDocumento.NombreCodigo, nuevoDocumento.ExtensionCodigo, nuevoDocumento.RutaCodigo, nuevoDocumento.HashCodigo,
                nuevoDocumento.NombreConTag, nuevoDocumento.ExtensionConTag, nuevoDocumento.RutaConTag, nuevoDocumento.HashConTag,
                nuevoDocumento.PesoDocumento);
        }

        foreach (var sustento in _sustentos.Where(x => x.Estado && !idsSustentosConservar.Contains(x.Id)))
        {
            sustento.Inactivar();
        }

        foreach (var nuevoSustento in solicitudRequest.NuevosSustentosRequest)
        {
            AgregarSustento(generarIdSustento(), nuevoSustento.NombreSustento, nuevoSustento.ExtensionSustento, nuevoSustento.RutaSustento, nuevoSustento.PesoSustento);
        }

        SincronizarCodigosReferencia(solicitudRequest.CodigosReferencia, generarIdCodigoReferencia);
        SincronizarCorreos(solicitudRequest.Correos, generarIdCorreo);

        CalcularPesoTotalDocumentos();

    }

    #endregion

    #region Editar Borrador y Enviar (Finalizar)

    public void EditarSolicitudBorradorYEnviar(
       SolicitudEditModel solicitudRequest,
       Func<long> generarIdDocumento,
       Func<long> generarIdSustento,
       Func<long> generarIdCodigoReferencia,
       Func<long> generarIdCorreo,
       long usuarioActualId,
       string? correoSolicitante)
    {

        if (EstadoSolicitud != ValueObject.EstadoSolicitud.Borrador
           && EstadoSolicitud != ValueObject.EstadoSolicitud.Rechazado
           && EstadoSolicitud != ValueObject.EstadoSolicitud.EnCurso)
            throw new DomainException(SolicitudErrors.FinalizarSoloBorrador);

        IdTipoSolicitud = solicitudRequest.IdTipoSolicitud;
        IdSubTipoSolicitud = solicitudRequest.IdSubTipoSolicitud;
        Asunto = solicitudRequest.Asunto;
        Mensaje = solicitudRequest.Mensaje;
        EsUrgente = solicitudRequest.EsUrgente;
        InCodigo = solicitudRequest.InCodigo;

        if (EstadoSolicitud != ValueObject.EstadoSolicitud.Rechazado)
        {
            foreach (var doc in _documentos)
            {
                doc.ReactivarFirmas();
            }
        }

        DefinirUrgencia(solicitudRequest.EsUrgente, solicitudRequest.FechaLimite);
        IdUsuarioAccion = usuarioActualId;
        FechaAccion = HoraSistema.Ahora;


        var idsDocumentosConservar = solicitudRequest.DocumentoIdsConservar.ToHashSet();
        var idsSustentosConservar = solicitudRequest.SustentoIdsConservar.ToHashSet();

        foreach (var documento in _documentos.Where(x => x.Estado && !idsDocumentosConservar.Contains(x.Id)))
        {
            documento.Inactivar();
        }

        foreach (var nuevoDocumento in solicitudRequest.NuevosDocumentosRequest)
        {
            AgregarDocumento(generarIdDocumento(), nuevoDocumento.NombreOriginal, nuevoDocumento.ExtensionOriginal, nuevoDocumento.RutaOriginal, nuevoDocumento.HashOriginal,
                   nuevoDocumento.NombreCodigo, nuevoDocumento.ExtensionCodigo, nuevoDocumento.RutaCodigo, nuevoDocumento.HashCodigo,
                   nuevoDocumento.NombreConTag, nuevoDocumento.ExtensionConTag, nuevoDocumento.RutaConTag, nuevoDocumento.HashConTag,
                   nuevoDocumento.PesoDocumento);
        }

        foreach (var sustento in _sustentos.Where(x => x.Estado && !idsSustentosConservar.Contains(x.Id)))
        {
            sustento.Inactivar();
        }

        foreach (var nuevoSustento in solicitudRequest.NuevosSustentosRequest)
        {
            AgregarSustento(generarIdSustento(), nuevoSustento.NombreSustento, nuevoSustento.ExtensionSustento, nuevoSustento.RutaSustento, nuevoSustento.PesoSustento);
        }

        SincronizarCodigosReferencia(solicitudRequest.CodigosReferencia, generarIdCodigoReferencia);
        SincronizarCorreos(solicitudRequest.Correos, generarIdCorreo);

        CalcularPesoTotalDocumentos();


        if (EstadoSolicitud != ValueObject.EstadoSolicitud.EnCurso)
        {
            AddDomainEvent(new SolicitudCreadaDomainEvent(this.Id, correoSolicitante));
            AddDomainEvent(new SolicitudPorAprobarFirmantesDomainEvent(this.Id));
        }

        CambiarEstado(ValueObject.EstadoSolicitud.EnCurso);
    }

    #endregion


    #region Finalizar
    public void Finalizar(
        string asunto,
        string? mensaje,
        bool esUrgente,
        bool inCodigo,
        DateTime? fechaLimite,
        decimal? pesoTotalDocu,
        string? correoSolicitante)
    {
        if (EstadoSolicitud != ValueObject.EstadoSolicitud.Borrador)
            throw new DomainException(SolicitudErrors.FinalizarSoloBorrador);

        Asunto = asunto;
        Mensaje = mensaje;
        InCodigo = inCodigo;
        DefinirUrgencia(esUrgente, fechaLimite);
        PesoTotalDocu = pesoTotalDocu;

        CambiarEstado(ValueObject.EstadoSolicitud.EnCurso);
        FechaAccion = HoraSistema.Ahora;

        AddDomainEvent(new SolicitudCreadaDomainEvent(this.Id, correoSolicitante));
        AddDomainEvent(new SolicitudPorAprobarFirmantesDomainEvent(this.Id));
    }


    public void MarcarEditarBorradorYEnviar()
    {
        AddDomainEvent(new SolicitudCreadaBorradorEnviadaEvent(
            this.Id,
            ValueObject.EstadoSolicitud.Borrador.Codigo,
            this.IdUsuarioAccion));
    }
    #endregion

    #region Entidades Hijas
    public void AgregarDocumento(
        long idSolicitudDocumento,
        string nombreOriginal,
        string extensionOriginal,
        string rutaOriginal,
        string hashOriginal,
        string? nombreCodigo,
        string? extensionCodigo,
        string? rutaCodigo,
        string? hashCodigo,
        string? nombreConTag,
        string? extensionConTag,
        string? rutaConTag,
        string? hashConTag,
        long? pesoDocumento)
    {
        if (_documentos.Count >= 30)
            throw new DomainException(SolicitudErrors.CantidadLimiteDocumentos.Format(_documentos.Count));

        var documento = SolicitudDocumento.Crear(
            this.Id,
            idSolicitudDocumento,
            nombreOriginal,
            extensionOriginal,
            rutaOriginal,
            hashOriginal,
            nombreCodigo,
            extensionCodigo,
            rutaCodigo,
            hashCodigo,
            nombreConTag,
            extensionConTag,
            rutaConTag,
            hashConTag,
            pesoDocumento);

        _documentos.Add(documento);
    }

    public void AgregarSustento(
        long idSolicitudSustento,
        string nombreSustento,
        string extensionSustento,
        string rutaSustento,
        long? pesoSustento)
    {
        if (_sustentos.Count >= 100)
            throw new DomainException(SolicitudErrors.CantidadLimiteSustentos.Format(_sustentos.Count));

        var solicitudSustento = SolicitudSustento.Crear(
            this.Id,
            idSolicitudSustento,
            nombreSustento,
            extensionSustento,
            rutaSustento,
            pesoSustento);

        _sustentos.Add(solicitudSustento);
    }

    public SolicitudSustento CrearNuevoSustento(
        long idSolicitudSustento,
        string nombreSustento,
        string extensionSustento,
        string rutaSustento,
        long? pesoSustento)
    {
        if (_sustentos.Count >= 100)
            throw new DomainException(SolicitudErrors.CantidadLimiteSustentos.Format(_sustentos.Count));

        var sustento = SolicitudSustento.Crear(
            this.Id,
            idSolicitudSustento,
            nombreSustento,
            extensionSustento,
            rutaSustento,
            pesoSustento);

        _sustentos.Add(sustento);
        return sustento;
    }

    public void AgregarCodReferencia(long idSolicitudCodReferencia, string codigoReferencia)
    {
        var solicitudReferencia = SolicitudCodReferencia.Crear(this.Id, idSolicitudCodReferencia, codigoReferencia);
        _codigosReferencia.Add(solicitudReferencia);
    }

    public void AgregarCodReferencia(long idSolicitudCodReferencia, CodigoReferencia codigoReferencia)
    {
        var existe = _codigosReferencia
            .Any(x => x.CodigoReferencia.Valor == codigoReferencia.Valor && x.Estado);

        if (existe)
            return;

        var item = SolicitudCodReferencia.Crear(
            Id,
            idSolicitudCodReferencia,
            codigoReferencia);

        _codigosReferencia.Add(item);
    }

    public void AgregarCorreo(long idSolicitudCorreo, string correo)
    {
        var solicitudCorreo = SolicitudCorreo.Crear(this.Id, idSolicitudCorreo, correo);
        _correos.Add(solicitudCorreo);
    }

    public void AgregarComentario(long idSolicitudComentario, long idUsuario, string comentario)
    {
        var solicitudComentario = SolicitudComentario.Crear(this.Id, idSolicitudComentario, idUsuario, comentario);
        _comentarios.Add(solicitudComentario);
    }
    #endregion

    public void AsignarPlantilla(SolicitudPlantilla solicitudPlantilla)
    {
        SolicitudPlantilla = solicitudPlantilla;
    }

    #region Aprobacion

    public void AprobarFirmante(long usuarioAccionId, string nombreAprobador, long idHistorialRevision, int ciclo)
    {
        ValidarUsuarioFirmoDocumento(usuarioAccionId);

        var firmante = SolicitudPlantilla.ObtenerFirmanteActivo(usuarioAccionId);

        var etapaActualAntesDeAprobar = ObtenerEtapaActual();

        if (firmante.OrdenEtapa != etapaActualAntesDeAprobar)
            throw new DomainException(SolicitudErrors.NoPuedeAprobarEstaEtapaSolicitud);

        firmante.Aprobar();

        firmante.AgregarHistorialRevision(idHistorialRevision, firmante.RevAprobado, firmante.FechaRevision, ciclo);

        if (TodoFlujoFirmaCompleto())
        {
            AprobarSolicitud(usuarioAccionId, nombreAprobador);
            return;
        }

        var etapaActualDespuesDeAprobar = ObtenerEtapaActual();

        if (etapaActualAntesDeAprobar != etapaActualDespuesDeAprobar)
        {
            AddDomainEvent(
                new SolicitudPorAprobarFirmantesDomainEvent(this.Id)
            );
        }
    }

    private long? ObtenerEtapaActual()
    {
        var pendientes = SolicitudPlantilla.FirmantesFlujo
            .Where(x => x.Estado && !x.RevAprobado)
            .ToList();

        if (pendientes.Count == 0)
            return null;

        return pendientes.Min(x => x.OrdenEtapa);
    }

    private bool TodoFlujoFirmaCompleto()
    {
        return SolicitudPlantilla.FlujoEstaCompleto();
    }

    public void ValidarUsuarioFirmoDocumento(long usuarioAccionId)
    {
        if (_documentos.Count > 0)
        {
            var documentosNoFirmados = _documentos
                .Where(d => d.DetallesFirma.Any(df =>
                    df.Estado &&
                    df.IdFirmanteDocumento == usuarioAccionId &&
                    df.EstadoDocumento != ValueObject.EstadoDocumento.Firmado))
                .ToList();

            if (documentosNoFirmados.Count > 0)
                throw new DomainException(SolicitudErrors.AprobDocumentosNoFirmados);
        }
    }

    public void AprobarSolicitud(long usuarioAccionId, string nombreAprobador)
    {
        if (EstadoSolicitud != ValueObject.EstadoSolicitud.EnCurso)
            throw new DomainException(SolicitudErrors.AprobSoloAprobEnCurso);

        var documentosActivos = _documentos
            .Where(x => x.Estado)
            .ToList();

        var existenDocumentosNoFirmados = documentosActivos.Any(d =>
         d.DetallesFirma.Any(df =>
             df.Estado &&
             df.EstadoDocumento != ValueObject.EstadoDocumento.Firmado));

        if (existenDocumentosNoFirmados)
            throw new DomainException(SolicitudErrors.AprobDocumentosNoFirmados);

        if (EsUrgente)
        {
            if (FechaLimite == null)
                throw new DomainException(SolicitudErrors.AprobFechaLimiteUrgente);

        }

        CambiarEstado(ValueObject.EstadoSolicitud.Aprobado);

        FechaAccion = HoraSistema.Ahora;
        IdUsuarioAccion = usuarioAccionId;

        AddDomainEvent(new SolicitudAprobadaDomainEvent(this.Id, nombreAprobador));
    }
    #endregion

    #region Rechazo
    public void RechazarSolicitud(long usuarioAccionId, string nombreAprobador,
        string motivoRechazo, long? idPlantillaEtapa, string nombreArea, long idHistorialRevision, int ciclo)
    {
        if (EstadoSolicitud != ValueObject.EstadoSolicitud.EnCurso)
            throw new DomainException(SolicitudErrors.RechazarSoloEnCurso);

        if (string.IsNullOrWhiteSpace(motivoRechazo))
            throw new DomainException(SolicitudErrors.MotivoRechazoRequired);

        if (motivoRechazo.Length > 350)
            throw new DomainException(SolicitudErrors.MotivoRechazoLength.Format(350));

        var estadoAnterior = IdEstadoSolicitud;

        MotivoRechazo = motivoRechazo;
        CambiarEstado(ValueObject.EstadoSolicitud.Rechazado);
        FechaAccion = HoraSistema.Ahora;
        IdUsuarioAccion = usuarioAccionId;

        foreach (var doc in _documentos)
        {
            doc.RechazarFirmas();
        }
        var firmante = SolicitudPlantilla.ObtenerFirmanteActivo(usuarioAccionId);

        SolicitudPlantilla.ResetearFirmantes();
        firmante.AgregarHistorialRevision(idHistorialRevision, false, FechaAccion, ciclo);


        AddDomainEvent(new SolicitudEstadoCambiadoDomainEvent(this.Id, estadoAnterior, IdUsuarioAccion, "Por rechazo de la solicitud"));
        AddDomainEvent(new SolicitudEventoRegistradoDomainEvent(this.Id, ValueObjects.TipoEvento.AprovalRejected.Codigo, usuarioAccionId, null, idPlantillaEtapa, nombreArea, null));
        AddDomainEvent(new SolicitudRechazadaDomainEvent(this.Id, nombreAprobador));
    }
    #endregion

    #region Anular
    public void Anular(long usuarioAccionId)
    {
        if (EstadoSolicitud == ValueObject.EstadoSolicitud.Anulado)
            throw new DomainException(SolicitudErrors.SolicitudYaAnulada);

        CambiarEstado(ValueObject.EstadoSolicitud.Anulado);
        FechaAccion = HoraSistema.Ahora;
        IdUsuarioAccion = usuarioAccionId;

        foreach (var documento in _documentos.Where(x => x.Estado))
        {
            documento.Inactivar();

            foreach (var dtl in documento.DetallesFirma.Where(x => x.Estado))
            {
                dtl.Inactivar();
            }

            foreach (var dt in documento.DocumentoTags.Where(x => x.Estado))
            {
                dt.Inactivar();
            }
        }

        foreach (var sustento in _sustentos.Where(x => x.Estado))
        {
            sustento.Inactivar();
        }

        foreach (var referencia in _codigosReferencia.Where(x => x.Estado))
        {
            referencia.Inactivar();
        }

        foreach (var correo in _correos.Where(x => x.Estado))
        {
            correo.Inactivar();
        }

        if (SolicitudPlantilla is not null && SolicitudPlantilla.Estado)
        {
            SolicitudPlantilla.Inactivar();
        }

        AddDomainEvent(new SolicitudEventoRegistradoDomainEvent(this.Id, ValueObjects.TipoEvento.RequestCancelled.Codigo, usuarioAccionId, null, null, null, null));
    }
    #endregion

    private void DefinirUrgencia(bool esUrgente, DateTime? fechaLimite)
    {
        if (esUrgente)
        {
            if (fechaLimite == null)
                throw new DomainException(SolicitudErrors.AprobFechaLimiteUrgente);

            EsUrgente = true;
            FechaLimite = Vo.FechaLimite.Crear(fechaLimite.Value);
        }
        else
        {
            EsUrgente = false;
            FechaLimite = null;
        }
    }

    public void CalcularPesoTotalDocumentos()
    {
        long? PesoTotal = 0;

        foreach (var item in _documentos.Where(x => x.Estado))
        {
            PesoTotal += item.PesoDocumento;
        }

        PesoTotalDocu = PesoTotal;
    }

    public void RegistrarEventos(IEnumerable<SolicitudEventoRegistradoDomainEvent> eventos)
    {
        AddDomainEvent(new SolicitudEventosBatchDomainEvent(eventos.ToList()));
    }

    public bool EsTurnoAprobador(long usuarioAccionId)
    {
        var firmante = SolicitudPlantilla.ObtenerFirmanteActivo(usuarioAccionId);
        var etapaActual = ObtenerEtapaActual();

        return (firmante.OrdenEtapa != etapaActual);
    }

    public bool DebeRedirigir(long usuarioAccionId)
    {
        var etapaActual = ObtenerEtapaActual();

        var estaEnEtapaActual = SolicitudPlantilla.FirmantesFlujo
            .Any(f =>
                f.IdUsuarioAsignado == usuarioAccionId &&
                !f.RevAprobado &&
                f.OrdenEtapa == etapaActual);

        return !estaEnEtapaActual;
    }

    /// <summary>
    /// No se pueden agregar ni mover etapas a una posición igual o anterior a la última etapa bloqueada
    /// (una etapa se bloquea cuando ya tiene aprobaciones o firmas).
    /// </summary>
    public void ValidarReordenamientoEtapas(IEnumerable<long> ordenesNuevos)
    {
        var ultimaEtapaBloqueada = ObtenerEstadoEtapas()
            .Where(e => e.Bloqueado)
            .Select(e => e.OrdenEtapa)
            .DefaultIfEmpty(0)
            .Max();

        if (ordenesNuevos.Any(orden => orden <= ultimaEtapaBloqueada))
            throw new DomainException(SolicitudErrors.CrearEtapaPrevia);
    }

    public IReadOnlyList<ValueObject.EstadoEtapa> ObtenerEstadoEtapas()
    {
        return
            [
               .. _documentos
                .Where(d => d.Estado)
                .SelectMany(d => d.DetallesFirma)
                .GroupBy(df => df.SolicitudFirmanteFlujo.OrdenEtapa)
                .Select(g => new ValueObject.EstadoEtapa(
                    OrdenEtapa: g.Key ?? 0,
                    Bloqueado: g.Any(df =>
                        df.SolicitudFirmanteFlujo.RevAprobado ||
                        df.EstadoDocumento == ValueObject.EstadoDocumento.Firmado ||
                        df.EstadoDocumento == ValueObject.EstadoDocumento.Bloqueado
                    )))
                .OrderBy(x => x.OrdenEtapa)
           ];
    }

    public void EsPropietarioOAdministradorDeLaSolicitud(Usuario usuario)
    {
        if (usuario.EsSolicitante() && IdUsuarioSolicitante != usuario.Id)
            throw new DomainException(SolicitudErrors.PermisoEditarSolicitud);
    }

    public void SincronizarCodigosReferencia(
       List<string>? codigosRequest,
       Func<long> nuevosIds)
    {
        var nuevosCodigos = (codigosRequest ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToHashSet();

        var actuales = _codigosReferencia
            .Where(x => x.Estado)
            .ToList();

        var actualesSet = actuales
            .Select(x => x.CodigoReferencia.Valor)
            .ToHashSet();

        foreach (var item in actuales
                .Where(x => !nuevosCodigos.Contains(x.CodigoReferencia.Valor)))
        {
            item.Inactivar();
        }

        foreach (var codigo in nuevosCodigos
                .Where(x => !actualesSet.Contains(x)))
        {
            _codigosReferencia.Add(
                SolicitudCodReferencia.Crear(
                    Id,
                    nuevosIds(),
                    codigo));
        }
    }

    public void SincronizarCorreos(
     List<string>? correosRequest,
     Func<long> nuevosIds)
    {
        var nuevosCorreos = (correosRequest ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToHashSet();

        var actuales = _correos
            .Where(x => x.Estado)
            .ToList();

        var actualesSet = actuales
            .Select(x => x.Correo.Value.ToUpperInvariant())
            .ToHashSet();

        foreach (var item in actuales
                .Where(x => !nuevosCorreos.Contains(x.Correo.Value.ToUpperInvariant())))
        {
            item.Inactivar();
        }

        foreach (var correo in nuevosCorreos
                .Where(x => !actualesSet.Contains(x)))
        {
            _correos.Add(
                SolicitudCorreo.Crear(
                    Id,
                    nuevosIds(),
                    correo));
        }
    }

    public void ValidarPermisoSolicitud(long usuarioId, long idArea, bool revisar, Usuario usuario)
    {
        if (revisar)
        {
            ValidarPermisoRevision(usuarioId);
            return;
        }

        if (usuario.EsSolicitante() && usuarioId != IdUsuarioSolicitante)
            throw new DomainException(SolicitudErrors.PermisSolicitud);

        if (usuario.EsCoordinador() && IdAreaSolicitud != idArea)
            throw new DomainException(SolicitudErrors.PermisoCoordinadorSolicitud);
    }

    private void ValidarPermisoRevision(long usuarioId)
    {
        if (EstadoSolicitud != ValueObject.EstadoSolicitud.EnCurso)
            throw new DomainException(SolicitudErrors.PermisAprobarRechazarSolicitud);

        var firmantes = SolicitudPlantilla.BuscarFirmantes(usuarioId);

        if (firmantes == null)
            throw new DomainException(SolicitudErrors.NoPuedeAprobarSolicitud);

        var firmante = SolicitudPlantilla.ObtenerFirmanteActivo(usuarioId);

        if (firmante.OrdenEtapa != ObtenerEtapaActual())
            throw new DomainException(SolicitudErrors.NoPuedeAprobarEstaEtapaSolicitud);
    }

    #region Restauración de flujo

    public IReadOnlyCollection<string> RestaurarFlujo(
        long idPuntoRestauracion,
        long idUsuarioAccion,
        Func<long> generarId)
    {
        var punto = ResolverPunto(idPuntoRestauracion);

        var archivos = EstadoSolicitud == ValueObject.EstadoSolicitud.Rechazado
            ? RestaurarRechazada(punto, generarId)
            : RestaurarEnCursoOAprobada(punto, generarId);

        PasarAEnCurso(idUsuarioAccion);
        AddDomainEvent(new SolicitudPorAprobarFirmantesDomainEvent(Id));
        return archivos;
    }

    private SolicitudFirmanteFlujo ResolverPunto(long idPuntoRestauracion)
    {
        if (EstadoSolicitud != ValueObject.EstadoSolicitud.EnCurso
            && EstadoSolicitud != ValueObject.EstadoSolicitud.Aprobado
            && EstadoSolicitud != ValueObject.EstadoSolicitud.Rechazado)
            throw new DomainException(SolicitudErrors.RestaurarEstadoNoPermitido);

        if (SolicitudPlantilla is null)
            throw new DomainException(SolicitudErrors.FlujoRequired);

        var punto = SolicitudPlantilla.FirmantesFlujo
            .FirstOrDefault(f => f.Estado && f.Id == idPuntoRestauracion)
            ?? throw new DomainException(SolicitudErrors.PuntoRestauracionNoEncontrado);

        if (!punto.ObtenerFechaRevisionEfectiva().HasValue)
            throw new DomainException(SolicitudErrors.PuntoRestauracionSinFecha);

        return punto;
    }

    private void PasarAEnCurso(long idUsuarioAccion)
    {
        if (EstadoSolicitud == ValueObject.EstadoSolicitud.EnCurso)
            return;

        var estadoAnterior = IdEstadoSolicitud;
        MotivoRechazo = null;
        CambiarEstado(ValueObject.EstadoSolicitud.EnCurso);
        FechaAccion = HoraSistema.Ahora;
        IdUsuarioAccion = idUsuarioAccion;
        AddDomainEvent(new SolicitudEstadoCambiadoDomainEvent(
            Id,
            estadoAnterior,
            idUsuarioAccion,
            "Por restauración del flujo"));
    }

    private List<string> RestaurarEnCursoOAprobada(SolicitudFirmanteFlujo punto, Func<long> generarId)
    {
        var archivos = new List<string>();

        foreach (var firmante in OrdenarPorFechaDesc(FirmantesActivos().Where(f => ParticipaEnRestauracion(f, punto))))
        {
            if (FirmoYAprobo(firmante))
                archivos.AddRange(RenovarDetalles(firmante, generarId));

            ReiniciarFirmante(firmante, punto);
        }

        return archivos;
    }

    private List<string> RestaurarRechazada(SolicitudFirmanteFlujo punto, Func<long> generarId)
    {
        foreach (var firmante in OrdenarPorFechaDesc(FirmantesActivos()))
        {
            var ultimo = firmante.ObtenerUltimoHistorialRevision();
            if (!DebeRestaurarse(firmante, ultimo, punto))
                continue;

            if (ultimo?.RevAprobado == true)
            {
                RestaurarDetallesAprobados(firmante, generarId);
                firmante.RestaurarAprobacion(ultimo.FechaRevision);
            }
            else
            {
                CrearDetallesPendientes(firmante, generarId);
                firmante.ResetearPorRechazo();
            }

            firmante.InactivarHistorialRevisionPosterior(punto.ObtenerFechaRevisionEfectiva()!.Value);
        }

        return [];
    }

    private static bool DebeRestaurarse(
        SolicitudFirmanteFlujo firmante,
        HistorialRevision? ultimo,
        SolicitudFirmanteFlujo punto)
    {
        if (firmante.EsMismoOPosteriorA(punto))
            return true;

        return ultimo?.RevAprobado == true
            && ultimo.FechaRevision <= punto.ObtenerFechaRevisionEfectiva();
    }

    private static bool ParticipaEnRestauracion(SolicitudFirmanteFlujo firmante, SolicitudFirmanteFlujo punto)
    {
        if (!firmante.EsMismoOPosteriorA(punto))
            return false;

        return firmante.Id == punto.Id
            || firmante.RevAprobado
            || firmante.HistRevision.Any(h => h.Estado && h.FechaRevision.HasValue);
    }

    private static void ReiniciarFirmante(SolicitudFirmanteFlujo firmante, SolicitudFirmanteFlujo punto)
    {
        firmante.ResetearPorRechazo();
        firmante.InactivarHistorialRevisionPosterior(punto.ObtenerFechaRevisionEfectiva()!.Value);
    }

    private static List<SolicitudFirmanteFlujo> OrdenarPorFechaDesc(IEnumerable<SolicitudFirmanteFlujo> firmantes)
        => firmantes
            .OrderByDescending(f => f.ObtenerFechaRevisionEfectiva() ?? DateTime.MinValue)
            .ThenByDescending(f => f.OrdenEtapa ?? long.MinValue)
            .ToList();

    private IEnumerable<SolicitudFirmanteFlujo> FirmantesActivos()
        => SolicitudPlantilla.FirmantesFlujo.Where(f => f.Estado);

    private bool FirmoYAprobo(SolicitudFirmanteFlujo firmante)
        => firmante.RevAprobado && DocumentosActivos().Any(documento =>
            documento.DetallesFirma.Any(detalle =>
                detalle.Estado &&
                detalle.IdSolicitudFirmanteFlujo == firmante.Id &&
                detalle.TieneDocumentoFirmado()));

    private List<string> RenovarDetalles(SolicitudFirmanteFlujo firmante, Func<long> generarId)
    {
        var archivos = new List<string>();

        foreach (var documento in DocumentosActivos())
        {
            foreach (var detalle in documento.DetallesFirma
                .Where(d => d.Estado && d.IdSolicitudFirmanteFlujo == firmante.Id)
                .ToList())
            {
                if (TieneArchivoFirmado(detalle))
                    archivos.Add(detalle.ObtenerRutasArchivosFirmados());

                detalle.Inactivar();
            }

            AsegurarDetallePendiente(documento, firmante, generarId);
        }

        return archivos;
    }

    private void RestaurarDetallesAprobados(SolicitudFirmanteFlujo firmante, Func<long> generarId)
    {
        foreach (var documento in DocumentosActivos())
        {
            if (TieneDetalleActivo(documento, firmante))
                continue;

            var firmado = documento.DetallesFirma
                .Where(d =>
                    !d.Estado &&
                    d.IdSolicitudFirmanteFlujo == firmante.Id &&
                    d.TieneDocumentoFirmado())
                .OrderByDescending(d => d.FechaHoraFirma)
                .FirstOrDefault();

            if (firmado is null)
            {
                AsegurarDetallePendiente(documento, firmante, generarId);
                continue;
            }

            documento.AgregarDetalleFirma(DetalleFirma.CopiarActivo(generarId(), firmado));
        }
    }

    private void CrearDetallesPendientes(SolicitudFirmanteFlujo firmante, Func<long> generarId)
    {
        foreach (var documento in DocumentosActivos())
            AsegurarDetallePendiente(documento, firmante, generarId);
    }

    private static void AsegurarDetallePendiente(
        SolicitudDocumento documento,
        SolicitudFirmanteFlujo firmante,
        Func<long> generarId)
    {
        if (TieneDetalleActivo(documento, firmante))
            return;

        documento.AgregarDetalleFirma(DetalleFirma.Crear(
            generarId(),
            firmante.Id,
            documento.Id,
            null,
            null,
            null,
            null,
            firmante.IdUsuarioAsignado));
    }

    private IEnumerable<SolicitudDocumento> DocumentosActivos()
        => _documentos.Where(d => d.Estado);

    private static bool TieneDetalleActivo(SolicitudDocumento documento, SolicitudFirmanteFlujo firmante)
        => documento.DetallesFirma.Any(d => d.Estado && d.IdSolicitudFirmanteFlujo == firmante.Id);

    private static bool TieneArchivoFirmado(DetalleFirma detalle)
        => detalle.TieneDocumentoFirmado()
            && !string.IsNullOrWhiteSpace(detalle.RutaDocumentoFirmado)
            && !string.IsNullOrWhiteSpace(detalle.NombreDocumentoFirmado)
            && !string.IsNullOrWhiteSpace(detalle.ExtensionDocumentoFirmado);

    #endregion

    #region Detalles de firma
    /// <summary>
    /// Crea los detalles de firma que faltan: uno por cada firmante activo del flujo y cada documento activo.
    /// No duplica los que ya existen. (Antes esta regla estaba en SolicitudPlantillaService.)
    /// </summary>
    public void GenerarDetallesFirmaPendientes(Func<long> generarIdDetalleFirma)
    {
        foreach (var firmante in FirmantesActivos())
        {
            foreach (var documento in DocumentosActivos())
                AsegurarDetallePendiente(documento, firmante, generarIdDetalleFirma);
        }
    }
    #endregion
}