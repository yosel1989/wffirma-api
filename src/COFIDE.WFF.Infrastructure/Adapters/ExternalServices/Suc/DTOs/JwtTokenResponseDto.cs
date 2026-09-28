namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;
public class JwtTokenResponseDto
{
    public required string Token { get; init; }
    public DateTime? ExpiraEn { get; init; }
    public required string Tipo { get; init; }
}
