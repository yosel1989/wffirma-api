using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Domain.Common.Errors;
using COFIDE.WFF.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf;

public class DocumentoFirmadoValidator(
        IPdfContentHashService pdfContentHashService,
        ILogger<DocumentoFirmadoValidator> logger
    ) : IDocumentoFirmadoValidator
{
    private readonly IPdfContentHashService _pdfContentHashService = pdfContentHashService;
    private readonly ILogger<DocumentoFirmadoValidator> _logger = logger;

    public async Task<bool> TieneFirmaAsync(Stream documentoFirmado)
    {
        byte[] pdfBytes;
        if (documentoFirmado.CanSeek)
            documentoFirmado.Position = 0;

        using (var ms = new MemoryStream())
        {
            await documentoFirmado.CopyToAsync(ms);
            pdfBytes = ms.ToArray();
        }

        using var stream = new MemoryStream(pdfBytes);
        var document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);
        var firmas = ObtenerFirmas(document);

        if (firmas.Count == 0)
            return false;
        else
            return true;
    }

    public async Task<ResultadoValidacionFirma> Externo_ValidarDocumentoFirmadoAsync(
        Stream documentoOriginal,
        Stream documentoFirmado,
        string hashOriginal,
        string usuarioEsperado,
        string? ultimoDniFirmanteValidoBaseDatos,
        bool tieneDocumentoFirmado)
    {
        #region DOCUMENTO ORIGINAL

        byte[] pdfBytesOriginal;
        if (documentoOriginal.CanSeek)
            documentoOriginal.Position = 0;

        using (var ms = new MemoryStream())
        {
            await documentoOriginal.CopyToAsync(ms);
            pdfBytesOriginal = ms.ToArray();
        }

        using var streamOriginal = new MemoryStream(pdfBytesOriginal);
        var docOriginal = PdfReader.Open(streamOriginal, PdfDocumentOpenMode.ReadOnly);

        #endregion

        #region DOCUMENTO FIRMADO

        byte[] pdfBytesFirmado;
        if (documentoFirmado.CanSeek)
            documentoFirmado.Position = 0;

        using (var ms = new MemoryStream())
        {
            await documentoFirmado.CopyToAsync(ms);
            pdfBytesFirmado = ms.ToArray();
        }

        using var stream = new MemoryStream(pdfBytesFirmado);
        var docFirmado = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);
        var hashFirmado = _pdfContentHashService.ObtenerHashContenido(pdfBytesFirmado);

        #endregion


        if (hashOriginal != hashFirmado)
            return ResultadoValidacionFirma.Fail("El documento ha sido manipulado,no corresponde al original o falta la nueva firma del aprobador");

        var firmasDocumentoOriginal = ObtenerFirmas(docOriginal);
        var firmasDocumentoFirmado = ObtenerFirmas(docFirmado);
        if (firmasDocumentoFirmado.Count == 0)
            return ResultadoValidacionFirma.Fail("El PDF no tiene firma digital");


        // Validar la cantidad y dnis del documento original
        string resultadoValidarDocumentosFirmados = ValidarDocumentosFirmados(usuarioEsperado, tieneDocumentoFirmado, ultimoDniFirmanteValidoBaseDatos,
            firmasDocumentoOriginal, pdfBytesOriginal, firmasDocumentoFirmado, pdfBytesFirmado);
        if (!string.IsNullOrEmpty(resultadoValidarDocumentosFirmados))
            return ResultadoValidacionFirma.Fail(resultadoValidarDocumentosFirmados);

        // validamos el ultimo firmantes
        string resultadoValidarUltimaFirma = ValidarUltimaFirma(usuarioEsperado, firmasDocumentoFirmado);
        if (!string.IsNullOrEmpty(resultadoValidarUltimaFirma))
            return ResultadoValidacionFirma.Fail(resultadoValidarUltimaFirma);

        return ResultadoValidacionFirma.Ok();
    }


    public async Task<string> Interno_ValidarDocumentoFirmadoAsync(
      Stream documentoOriginal,
      Stream documentoFirmado)
    {
        #region DOCUMENTO ORIGINAL

        byte[] pdfBytesOriginal;
        if (documentoOriginal.CanSeek)
            documentoOriginal.Position = 0;

        using (var ms = new MemoryStream())
        {
            await documentoOriginal.CopyToAsync(ms);
            pdfBytesOriginal = ms.ToArray();
        }

        using var streamOriginal = new MemoryStream(pdfBytesOriginal);
        var docOriginal = PdfReader.Open(streamOriginal, PdfDocumentOpenMode.ReadOnly);

        #endregion

        #region DOCUMENTO FIRMADO

        byte[] pdfBytesFirmado;
        if (documentoFirmado.CanSeek)
            documentoFirmado.Position = 0;

        using (var ms = new MemoryStream())
        {
            await documentoFirmado.CopyToAsync(ms);
            pdfBytesFirmado = ms.ToArray();
        }

        using var stream = new MemoryStream(pdfBytesFirmado);
        var docFirmado = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);

        #endregion
        _logger.LogInformation("Paso Interno_ValidarDocumentoFirmadoAsync");
        var firmasDocumentoOriginal = ObtenerFirmas(docOriginal);
        var firmasDocumentoFirmado = ObtenerFirmas(docFirmado);

        var listaDniDocumentoOriginal = ObtenerListadoDniPorDocumento(firmasDocumentoOriginal, pdfBytesOriginal);
        var listaDniDocumentoFirmado = ObtenerListadoDniPorDocumento(firmasDocumentoFirmado, pdfBytesFirmado);

        var cantidadFirmasOriginal = listaDniDocumentoOriginal.Count;
        var cantidadFirmasSubido = listaDniDocumentoFirmado.Count;

        if (cantidadFirmasSubido == cantidadFirmasOriginal)
        {
            return "No es posible firmar esta versión del documento porque ha sido actualizada por otro firmante. Vuelva a firmar en unos minutos.";
        }

        return "";

    }

    private string ValidarDocumentosFirmados(string dniFirmanteActual, bool tieneDocumentoFirmado, string? ultimoDniFirmanteValidoBaseDatos, List<PdfDictionary> firmaDocumentoOriginal,
       byte[] documentoOriginal, List<PdfDictionary> firmaDocumentoFirmado, byte[] documentoFirmado)
    {
        var listaDniDocumentoOriginal = ObtenerListadoDniPorDocumento(firmaDocumentoOriginal, documentoOriginal);
        var listaDniDocumentoFirmado = ObtenerListadoDniPorDocumento(firmaDocumentoFirmado, documentoFirmado);

        var cantidadFirmasOriginal = listaDniDocumentoOriginal.Count;
        var cantidadFirmasSubido = listaDniDocumentoFirmado.Count;

        // es primer firmante?

        if (!tieneDocumentoFirmado)
        {
            // Si listaDniDocumentoOriginal tiene 0 firmas, es un documento nuevo, aca se omite esta validación
            return ValidarPrimerFirmante(
                dniFirmanteActual,
                listaDniDocumentoOriginal,
                listaDniDocumentoFirmado);
        }
        else
        {
            if (cantidadFirmasSubido == cantidadFirmasOriginal)
            {
                return "El documento debe contener la firma del usuario actual.";
            }

            return ValidarPenultimaFirma(ultimoDniFirmanteValidoBaseDatos, firmaDocumentoFirmado, firmaDocumentoOriginal);
        }
    }

    private static string ValidarPrimerFirmante(
    string dniFirmanteActual,
    List<string> listaOriginal,
    List<string> listaFirmado)
    {
        if (listaOriginal.Count == 0)
        {
            return ValidarDocumentoNuevo(
                dniFirmanteActual,
                listaFirmado);
        }

        return ValidarCantidadFirmas(
            listaOriginal.Count,
            listaFirmado.Count);
    }

    private static string ValidarDocumentoNuevo(
    string dniFirmanteActual,
    List<string> listaFirmado)
    {
        var firmantes = listaFirmado.ToHashSet();

        if (firmantes.Count > 1 &&
            !firmantes.SetEquals([dniFirmanteActual]))
        {
            return "El documento debe contener únicamente la firma del usuario actual.";
        }

        return string.Empty;
    }

    private static string ValidarCantidadFirmas(
    int firmasOriginal,
    int firmasFirmado)
    {
        if (firmasFirmado == firmasOriginal)
        {
            return "El documento debe contener la firma del usuario actual.";
        }

        if (firmasFirmado > firmasOriginal + 1)
        {
            return "El documento presenta una inconsistencia en el flujo de firmas. No es posible continuar con la firma.";
        }

        return string.Empty;
    }

    private List<string> ObtenerListadoDniPorDocumento(List<PdfDictionary> firmas, byte[] pdfBytes)
    {
        var dnisFirmasPdf = new List<string>();

        foreach (var firma in firmas)
        {
            var pkcs7 = ObtenerPkcs7(firma);
            var byteRange = ObtenerByteRange(firma);

            if (pkcs7 == null || byteRange == null)
                return dnisFirmasPdf;

            if (pkcs7 == null || byteRange == null)
                throw new ValidacionAplicacion.ApplicationException(new ApplicationError("", "El documento contiene una firma digital inválida."));

            if (!ByteRangeValido(pdfBytes, byteRange))
                throw new ValidacionAplicacion.ApplicationException(new ApplicationError("", "El documento contiene una firma digital inválida o dañada."));

            var contenidoFirmado = ObtenerContenidoFirmado(pdfBytes, byteRange);

            if (!ValidarPkcs7(pkcs7, contenidoFirmado))
                return dnisFirmasPdf;

            var cert = ObtenerCertificado(pkcs7);

            if (cert == null)
                return dnisFirmasPdf;

            var dni = ExtraerDni(cert);

            if (!string.IsNullOrWhiteSpace(dni))
                dnisFirmasPdf.Add(dni);
        }

        return dnisFirmasPdf;
    }

    private static bool ByteRangeValido(
    byte[] pdfBytes,
    PdfArray byteRange)
    {
        int offset1 = GetInt(byteRange.Elements[0]);
        int length1 = GetInt(byteRange.Elements[1]);

        int offset2 = GetInt(byteRange.Elements[2]);
        int length2 = GetInt(byteRange.Elements[3]);

        return
            offset1 >= 0 &&
            length1 >= 0 &&
            offset2 >= 0 &&
            length2 >= 0 &&
            offset1 + length1 <= pdfBytes.Length &&
            offset2 + length2 <= pdfBytes.Length;
    }

    private static string ValidarUltimaFirma(string dniFirmanteActual, List<PdfDictionary> firmaDocumentoFirmado)
    {
        var ultimaFirmaDocFirmado = ObtenerUltimaFirma(firmaDocumentoFirmado);
        if (ultimaFirmaDocFirmado == null)
            return "No se pudo determinar la última firma";

        var errorFirma = ValidarFirmaIndividual(ultimaFirmaDocFirmado, dniFirmanteActual);

        if (errorFirma != null)
            return errorFirma;

        return string.Empty;
    }

    private static string ValidarPenultimaFirma(string? dniFirmanteUltimo, List<PdfDictionary> firmaDocumentoFirmado, List<PdfDictionary> firmaDocumentoOriginal)
    {
        var penultimaFirmaDocFirmado = ObtenerPenultimaFirma(firmaDocumentoFirmado);
        var ultimaFirmaDocOriginal = ObtenerUltimaFirma(firmaDocumentoOriginal);

        if (penultimaFirmaDocFirmado == null && ultimaFirmaDocOriginal == null)
            return "No se pudo determinar la última firma";

        var dniPenultimoFirmado = ExtraerDNIDesdePdfDictionary(penultimaFirmaDocFirmado!);
        var dniUltimoOriginal = ExtraerDNIDesdePdfDictionary(ultimaFirmaDocOriginal!);

        if (penultimaFirmaDocFirmado == null || ultimaFirmaDocOriginal == null || dniFirmanteUltimo == null)
        {
            return "No se pudo determinar el flujo de firmas del documento.";
        }

        if (dniPenultimoFirmado == dniUltimoOriginal && dniUltimoOriginal == dniFirmanteUltimo)
        {
            return string.Empty;
        }

        return "El documento presenta una inconsistencia en el flujo de firmas. " +
               "No es posible continuar con la firma.";
    }

    private static string? ExtraerDNIDesdePdfDictionary(PdfDictionary firma)
    {
        var pkcs7 = ObtenerPkcs7(firma);
        var byteRange = ObtenerByteRange(firma);

        if (pkcs7 == null || byteRange == null)
            return "No se pudo extraer la firma";

        var cert = ObtenerCertificado(pkcs7);

        if (cert == null)
            return "No se pudo obtener certificado";

        var dni = ExtraerDni(cert);

        return dni;
    }

    private static PdfDictionary? ObtenerUltimaFirma(List<PdfDictionary> firmas)
    {
        return firmas
            .Select(f => new
            {
                Firma = f,
                ByteRange = ObtenerByteRange(f)
            })
            .Where(x => x.ByteRange != null)
            .OrderByDescending(x =>
                GetInt(x.ByteRange!.Elements[2]) +
                GetInt(x.ByteRange!.Elements[3]))
            .Select(x => x.Firma)
            .FirstOrDefault();
    }

    private static PdfDictionary? ObtenerPenultimaFirma(List<PdfDictionary> firmas)
    {
        return firmas
            .Select(f => new
            {
                Firma = f,
                ByteRange = ObtenerByteRange(f)
            })
            .Where(x => x.ByteRange != null)
            .OrderByDescending(x =>
                GetInt(x.ByteRange!.Elements[2]) +
                GetInt(x.ByteRange!.Elements[3]))
            .Skip(1)
            .Select(x => x.Firma)
            .FirstOrDefault();
    }

    private static string? ValidarFirmaIndividual(PdfDictionary firma, string usuarioEsperado)
    {
        var pkcs7 = ObtenerPkcs7(firma);
        var byteRange = ObtenerByteRange(firma);

        if (pkcs7 == null || byteRange == null)
            return "No se pudo extraer la firma";

        var cert = ObtenerCertificado(pkcs7);

        if (cert == null)
            return "No se pudo obtener certificado";

        var bcCert = new X509CertificateParser().ReadCertificate(cert.GetEncoded());
        var dni = ExtraerDni(cert);

        if (dni != usuarioEsperado)
            return "El certificado no corresponde al usuario";

        var now = DateTime.UtcNow;

        if (now < bcCert.NotBefore.ToUniversalTime())
            return "Certificado aún no válido";

        if (now > bcCert.NotAfter.ToUniversalTime())
            return "Certificado expirado";

        return null;
    }


    #region Métodos

    private static string ExtraerDni(X509Certificate cert)
    {
        var subject = cert.SubjectDN;

        var oids = subject.GetOidList();
        var values = subject.GetValueList();

        for (int i = 0; i < oids.Count; i++)
        {
            var oid = (DerObjectIdentifier)oids[i];
            var value = values[i].ToString();

            // SERIALNUMBER
            if (oid.Id == "2.5.4.5")
            {
                var match = Regex.Match(value, @"\b\d{8}\b");
                if (match.Success)
                    return match.Value;
            }

            // fallback si viene en CN
            if (oid.Id == X509Name.CN.Id)
            {
                var match = Regex.Match(value, @"\b\d{8}\b");
                if (match.Success)
                    return match.Value;
            }
        }

        return string.Empty;
    }

    private static X509Certificate? ObtenerCertificado(byte[] pkcs7Bytes)
    {
        var cms = new CmsSignedData(pkcs7Bytes);

        var certStore = cms.GetCertificates();
        var signers = cms.GetSignerInfos().GetSigners();

        foreach (SignerInformation signer in signers)
        {
            var certCollection = certStore.EnumerateMatches(signer.SignerID);

            foreach (X509Certificate cert in certCollection)
                return cert;
        }

        return null;
    }

    private bool ValidarPkcs7(byte[] pkcs7Bytes, byte[] contenidoFirmado)
    {
        try
        {
            var cms = new CmsSignedData(
                new CmsProcessableByteArray(contenidoFirmado),
                pkcs7Bytes);

            var signers = cms.GetSignerInfos().GetSigners();
            var certStore = cms.GetCertificates();

            foreach (SignerInformation signer in signers)
            {
                var certCollection = certStore.EnumerateMatches(signer.SignerID);

                foreach (X509Certificate cert in certCollection)
                {
                    if (signer.Verify(cert))
                        return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ValidarPkcs7.");
            return false;
        }
    }

    private static byte[]? ObtenerPkcs7(PdfDictionary firma)
    {
        if (!firma.Elements.ContainsKey("/Contents"))
            return null;

        PdfItem item = firma.Elements["/Contents"];

        if (item is PdfReference r)
        {
            if (r.Value is null)
                return null;

            item = r.Value;
        }

        string? raw = item.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (!raw.StartsWith("<") || !raw.EndsWith(">"))
            return null;

        var hex = raw[1..^1]; // solo quitar envoltura

        hex = Regex.Replace(hex, @"\s+", "");

        if (hex.Length % 2 != 0)
            return null;

        if (!Regex.IsMatch(hex, @"\A[0-9A-Fa-f]+\z"))
            return null;

        var bytes = Convert.FromHexString(hex);

        // quitar solo padding nulo final
        bytes = TrimCmsExact(bytes);

        return bytes;
    }

    private static byte[] TrimCmsExact(byte[] bytes)
    {
        if (bytes.Length < 4 || bytes[0] != 0x30)
            return bytes;

        if (bytes[1] == 0x82)
        {
            int len = (bytes[2] << 8) | bytes[3];
            int total = len + 4;

            if (bytes.Length >= total)
                return bytes.Take(total).ToArray();
        }

        return bytes;
    }


    private static PdfArray? ObtenerByteRange(PdfDictionary firma)
    {
        return firma.Elements["/ByteRange"] as PdfArray;
    }

    private static byte[] ObtenerContenidoFirmado(byte[] pdfBytes, PdfArray byteRange)
    {
        int offset1 = GetInt(byteRange.Elements[0]);
        int length1 = GetInt(byteRange.Elements[1]);

        int offset2 = GetInt(byteRange.Elements[2]);
        int length2 = GetInt(byteRange.Elements[3]);

        if (offset1 < 0 ||
            length1 < 0 ||
            offset2 < 0 ||
            length2 < 0)
        {
            throw new InvalidOperationException(
                $"ByteRange inválido: [{offset1}, {length1}, {offset2}, {length2}]");
        }

        if (offset1 + length1 > pdfBytes.Length ||
            offset2 + length2 > pdfBytes.Length)
        {
            throw new InvalidOperationException(
                $"ByteRange fuera de rango. PDF={pdfBytes.Length}, ByteRange=[{offset1}, {length1}, {offset2}, {length2}]");
        }

        var result = new byte[length1 + length2];

        Buffer.BlockCopy(pdfBytes, offset1, result, 0, length1);
        Buffer.BlockCopy(pdfBytes, offset2, result, length1, length2);

        return result;
    }

    private static int GetInt(PdfItem item)
    {
        if (item is PdfReference reference)
            item = reference.Value;

        if (item is PdfInteger pi)
            return pi.Value;

        if (item is PdfReal pr)
            return (int)pr.Value;

        throw new Exception($"No es número válido: {item.GetType()}");
    }

    private static List<PdfDictionary> ObtenerFirmas(PdfDocument document)
    {
        var firmas = new List<PdfDictionary>();

        var catalog = document.Internals.Catalog;

        if (!catalog.Elements.ContainsKey("/AcroForm"))
            return firmas;

        var acroForm = catalog.Elements["/AcroForm"] as PdfDictionary;

        if (acroForm == null || !acroForm.Elements.ContainsKey("/Fields"))
            return firmas;

        var fields = acroForm.Elements["/Fields"] as PdfArray;

        if (fields == null)
            return firmas;

        foreach (var field in fields.Elements)
        {
            var reference = field as PdfReference;
            var dict = reference?.Value as PdfDictionary;

            if (dict == null)
                continue;

            var fieldType = dict.Elements["/FT"];

            if (fieldType != null && fieldType.ToString() == "/Sig")
            {
                var vRef = dict.Elements["/V"] as PdfReference;
                var firmaDict = vRef?.Value as PdfDictionary;

                if (firmaDict != null)
                    firmas.Add(firmaDict);
            }
        }

        return firmas;
    }

    #endregion

    public async Task<ResultadoValidacionFirma> ValidarFirmasAnterioresAsync(
            Stream documentoAntesDeFirmarConFirma,
            string hashDocumentoAntesDeFirmar,
            IList<string> ListaDniFirmados)
    {
        var pdfBytes = await LeerPdfAsync(documentoAntesDeFirmarConFirma);

        using var stream = new MemoryStream(pdfBytes);

        var document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);        
        var firmas = ObtenerFirmas(document);
        var dnisEncontradosEnElDocumento = new List<string>();

        if (firmas == null || firmas.Count == 0)
            return ResultadoValidacionFirma.Fail("El PDF no tiene firma digital");

        foreach (var firma in firmas)
        {
            var resultado = ValidarFirma(firma, pdfBytes);

            if (!resultado.EsValido)
                return ResultadoValidacionFirma.Fail(resultado.Mensaje!);

            if (!string.IsNullOrWhiteSpace(resultado.Dni))
                dnisEncontradosEnElDocumento.Add(resultado.Dni);
        }

        var dnisDeFirmantes = ListaDniFirmados.ToHashSet();
        var dnisPdf = dnisEncontradosEnElDocumento.ToHashSet();

        var faltantes = dnisDeFirmantes.Except(dnisPdf).ToList();
        var extras = dnisPdf.Except(dnisDeFirmantes).ToList();

        if (faltantes.Count > 0 || extras.Count > 0)
        {
            return ResultadoValidacionFirma.Fail(
                "El documento presenta una inconsistencia en el flujo de firmas. " +
                "No es posible continuar con la firma.");
        }

        return ResultadoValidacionFirma.Ok();
    }

    private static async Task<byte[]> LeerPdfAsync(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    private ResultadoFirma ValidarFirma(PdfDictionary firma, byte[] pdfBytes)
    {
        var pkcs7 = ObtenerPkcs7(firma);
        var byteRange = ObtenerByteRange(firma);

        if (pkcs7 == null || byteRange == null)
            return ResultadoFirma.Fail("El documento contiene una firma digital inválida.");

        if (!ByteRangeValido(pdfBytes, byteRange))
            return ResultadoFirma.Fail("El documento contiene una firma digital inválida o dañada.");

        var contenidoFirmado = ObtenerContenidoFirmado(pdfBytes, byteRange);

        if (!ValidarPkcs7(pkcs7, contenidoFirmado))
            return ResultadoFirma.Fail("Firma inválida");

        var cert = ObtenerCertificado(pkcs7);

        if (cert == null)
            return ResultadoFirma.Fail("No se pudo obtener certificado");

        return ResultadoFirma.Ok(ExtraerDni(cert));
    }

    private sealed class ResultadoFirma
    {
        public bool EsValido { get; }
        public string? Mensaje { get; }
        public string? Dni { get; }

        private ResultadoFirma(bool esValido, string? mensaje, string? dni)
        {
            EsValido = esValido;
            Mensaje = mensaje;
            Dni = dni;
        }

        public static ResultadoFirma Ok(string? dni) =>
            new(true, null, dni);

        public static ResultadoFirma Fail(string mensaje) =>
            new(false, mensaje, null);
    }
}
