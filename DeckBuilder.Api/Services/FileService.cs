using DeckBuilder.Api.Exceptions;
using Newtonsoft.Json;

namespace DeckBuilder.Api.Services;

public static class FileService
{
    public static async Task<T?> ReadFileAsJsonAsync<T>(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            throw KnownException.ImportFileNotFound(filePath);

        using var reader = new StreamReader(filePath);
        var content = await reader.ReadToEndAsync(cancellationToken);

        return JsonConvert.DeserializeObject<T>(content);
    }
}