namespace DeckBuilder.Api.Configurations;

public class JwtConfiguration(IConfiguration configuration)
{
    public string Secret => configuration.GetValue<string>("Jwt:Secret") ?? string.Empty;
    public int ExpiresInMinutes => configuration.GetValue<int>("Jwt:ExpiresInMinutes");
    public string Issuer => configuration.GetValue<string>("Jwt:Issuer") ?? string.Empty;
    public string Audience => configuration.GetValue<string>("Jwt:Audience") ?? string.Empty;
}
