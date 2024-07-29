namespace DeckBuilder.Api.Configurations;

public class MongoConfiguration(IConfiguration configuration)
{
    public string ConnectionUri => configuration.GetValue<string>("MongoDb:ConnectionUri") ?? string.Empty;
    public string DatabaseName => configuration.GetValue<string>("MongoDb:DatabaseName") ?? string.Empty;
}