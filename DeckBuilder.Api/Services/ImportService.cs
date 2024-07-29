using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Models;
using MongoDB.Driver;

namespace DeckBuilder.Api.Services;

public class ImportService(IMongoDatabase database)
{
    private readonly IMongoCollection<Card> _cardsCollection = database.GetCollection<Card>(Collections.Cards);
    private readonly IMongoCollection<Game> _gamesCollection = database.GetCollection<Game>(Collections.Games);
    
    public async Task ImportCardsFromAssets(CancellationToken cancellationToken)
    {
        var gameFolders = Directory.GetDirectories("./Assets/new");

        await Parallel.ForEachAsync(gameFolders, cancellationToken, async (directory, directoryCancellationToken) =>
            {
                var cardFiles = Directory.GetFiles(directory).Where(file => !file.Contains("game.json"));
                await Parallel.ForEachAsync(cardFiles, directoryCancellationToken, async (file, cardCancellationToken) =>
                {
                    var cards = await FileService.ReadFileAsJsonAsync<IEnumerable<Card>>(file, cancellationToken);
                          
                    var insertManyOptions = new InsertManyOptions { IsOrdered = false };
                    await _cardsCollection.InsertManyAsync(cards, insertManyOptions, cardCancellationToken);
                }); 

                var game = await FileService
                    .ReadFileAsJsonAsync<Game>($"{directory}/game.json", directoryCancellationToken);

                if (game is not null)
                    await _gamesCollection.InsertOneAsync(game, null, directoryCancellationToken);
            }
        );
    }
    
    public async Task DeleteImportedData(CancellationToken cancellationToken)
    {
        await _cardsCollection.DeleteManyAsync(ExpressionFilterDefinition<Card>.Empty, cancellationToken);
        await _gamesCollection.DeleteManyAsync(ExpressionFilterDefinition<Game>.Empty, cancellationToken);
    }
}