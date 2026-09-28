using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Factories;

public static class SolicitudFactory
{
    public static Solicitud CrearSolicitudBase(
        CrearSolicitudInput input,
        long idSolicitud,       
        long idUSuario,
        long idArea,
        long idAreaPadre)
    {
        #region 01. Traer la información de Código
        var codigoDocumento = CodigoSolicitud.Crear(input.Anio, input.CodigoArea, input.CodigoTipoSolicitud, input.CorrelativoCodigoSolicitud);
        #endregion

        #region 02. Grabamos la Solicitud

        var solicitud = Solicitud.CrearBorrador(
              idSolicitud,
              codigoDocumento.Value,
              input.IdTipoSolicitud,
              input.IdSubTipoSolicitud,
              idUSuario,
              input.IdTipoOperacion,
              idUSuario,
              idArea,
              idAreaPadre,
              input.Asunto,
              input.Mensaje,
              input.EsUrgente,
              input.InCodigo,
              input.FechaLimite,
              0
        );
        #endregion

        return solicitud;
    }


    public static Solicitud CrearDesdeRequest(
        CrearSolicitudInput input,
        Solicitud solicitud,
        Func<long> generarIdCodigoReferencia,
        Func<long> generarIdCorreo,
        Func<long> generarIdDocumento,
        Func<long> generarIdSustento
        )
    {
        AgregarCodigosReferencia(input, solicitud, generarIdCodigoReferencia);
        AgregarCorreos(input, solicitud, generarIdCorreo);
        AgregarDocumentos(input, solicitud, generarIdDocumento);
        AgregarSustentos(input, solicitud, generarIdSustento);
        solicitud.CalcularPesoTotalDocumentos();
        return solicitud;
    }


    private static void AgregarCodigosReferencia(CrearSolicitudInput request, Solicitud solicitud, Func<long> generarIdCodigoReferencia)
    {
        if (request.CodigosReferencia?.Count <= 0)
            return;

        foreach (var codigo in request.CodigosReferencia!.Distinct())
        {
            var codigoVo = CodigoReferencia.Crear(codigo);

            solicitud.AgregarCodReferencia(
                generarIdCodigoReferencia(),
                codigoVo);
        }
    }

    private static void AgregarCorreos(CrearSolicitudInput request, Solicitud solicitud, Func<long> generarIdCorreo)
    {
        if (request.Correos?.Count <= 0)
            return;

        foreach (var correo in request.Correos!.Distinct())
        {
            solicitud.AgregarCorreo(
                generarIdCorreo(),
                correo);
        }
    }

    private static void AgregarDocumentos(CrearSolicitudInput request, Solicitud solicitud, Func<long> generarIdDocumento)
    {
        if (request.Documentos?.Count <= 0)
            return;

        foreach (var documento in request.Documentos!)
        {
            solicitud.AgregarDocumento(
                generarIdDocumento(),
                documento.NombreOriginal,
                documento.ExtensionOriginal,
                documento.RutaOriginal,
                documento.HashOriginal,
                documento.NombreCodigo,
                documento.ExtensionCodigo,
                documento.RutaCodigo,
                documento.HashCodigo,
                documento.NombreConTag,
                documento.ExtensionConTag,
                documento.RutaConTag,
                documento.HashConTag,
                documento.PesoDocumento);
        }
    }

    private static void AgregarSustentos(CrearSolicitudInput request, Solicitud solicitud, Func<long> generarIdSustento)
    {
        if (request.Sustentos?.Count <= 0)
            return;

        foreach (var sustento in request.Sustentos!)
        {
            solicitud.AgregarSustento(
                generarIdSustento(),
                sustento.NombreSustento,
                sustento.ExtensionSustento,
                sustento.RutaSustento,
                sustento.PesoSustento);
        }
    }
}
