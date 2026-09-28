namespace COFIDE.WFF.Infrastructure.Adapters.Services.Pdf;

public static class StreamHelper
{
    public static void Copiar(Stream origen, Stream destino, long cantidadBytes)
    {
        var buffer = new byte[8192];
        long restante = cantidadBytes;

        while (restante > 0)
        {
            int leidos = origen.Read(buffer, 0, (int)Math.Min(buffer.Length, restante));

            if (leidos <= 0)
                throw new EndOfStreamException("No se pudo leer el stream completo");

            destino.Write(buffer, 0, leidos);
            restante -= leidos;
        }
    }
}
