namespace COFIDE.WFF.Application.Features.Firmas.DTOs;
public class ArchivoProtegidoDto
{
    public byte[] archivoProtegido { get; set; } = default!;
    public string hash { get; set; } = default!;
}
