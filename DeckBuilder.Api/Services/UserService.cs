using DeckBuilder.Api.Configurations;
using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Helpers;
using DeckBuilder.Api.Models;
using DeckBuilder.Api.ViewModels;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DeckBuilder.Api.Services;

public class UserService(IMongoDatabase database, JwtConfiguration jwtConfiguration)
{
    private readonly IMongoCollection<User> _collection = database.GetCollection<User>(Collections.Users);
    
    private string EncryptPasswordForUser(
        string userPassword,
        byte[] saltForPassword,
        int saltSize = 16,
        int hashSize = 20)
    {
        var hashForPassword = CryptoService.GenerateHash(userPassword, saltForPassword, hashSize);

        var hashBytes = new byte[saltSize + hashSize];
        Array.Copy(saltForPassword, 0, hashBytes, 0, saltSize);
        Array.Copy(hashForPassword, 0, hashBytes, saltSize, hashSize);
        
        return Convert.ToBase64String(hashBytes);
    }
    
    public async ValueTask<string> CreateUser(CreateUserInput input, CancellationToken cancellationToken)
    {
        var existingUser = await _collection.AsQueryable()
            .FirstOrDefaultAsync(u => Equals(u.Email, input.Email), cancellationToken);

        if (existingUser is not null)
            throw KnownException.UserAlreadyExists(input.Email);

        var salt = CryptoService.GenerateSalt();
        
        var user = new User
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            Email = input.Email,
            Password = EncryptPasswordForUser(input.Password, salt),
            Salt = Convert.ToBase64String(salt),
            CreatedAt = DateTime.Now,
            Decks = input.Decks
        };
        
        await _collection.InsertOneAsync(user, null, cancellationToken);
        CacheManager.SetItem(CacheKeys.GetUserByEmail(user.Email), user);
        
        return user.Id;
    }

    private async ValueTask<User> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.GetUserByEmail(email);
        var cachedUser = CacheManager.GetItem<User?>(cacheKey);

        if (cachedUser is not null)
            return cachedUser;
        
        var existingUser = await _collection
            .AsQueryable()
            .FirstOrDefaultAsync(u => Equals(u.Email, email), cancellationToken);
    
        CacheManager.SetItem(cacheKey, existingUser);
        return existingUser;
    }
    
    private async ValueTask<User> GetUserById(string userId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.GetUserById(userId);
        var cachedUser = CacheManager.GetItem<User?>(cacheKey);

        if (cachedUser is not null)
            return cachedUser;
        
        var existingUser = await _collection
            .AsQueryable()
            .FirstOrDefaultAsync(u => Equals(u.Id, userId), cancellationToken);
    
        CacheManager.SetItem(cacheKey, existingUser);
        return existingUser;
    }

    public async ValueTask<string> LoginWithCredentials(CredentialsInput input, CancellationToken cancellationToken)
    {
        var existingUser = await GetUserByEmail(input.Email, cancellationToken);
        if (existingUser is null)
            throw KnownException.InvalidCredentials();

        var encryptedPassword =
            EncryptPasswordForUser(input.Password, Convert.FromBase64String(existingUser.Salt));
        
        if (string.CompareOrdinal(encryptedPassword, existingUser.Password) != 0)
            throw KnownException.InvalidCredentials();

        var authTokenInput = new AuthTokenInput(existingUser.Email, existingUser.FullName);
        
        return TokenService.GenerateAuthToken(authTokenInput, jwtConfiguration);
    }
    
    public async ValueTask<IEnumerable<User>> SearchUsers(CancellationToken cancellationToken)
    {
        var listOutput = CacheManager.GetItem<IEnumerable<User>>(CacheKeys.UsersList);
        if (listOutput is not null)
            return listOutput;
        
        var result = await _collection.AsQueryable().ToListAsync(cancellationToken);
        CacheManager.SetItem(CacheKeys.UsersList, result);

        return result;
    }

    public async ValueTask<bool> AddDeckToUser(string userId, string deckId, CancellationToken cancellationToken)
    {
        var filter = Builders<User>.Filter.Eq(user => user.Id, userId);
        var update = Builders<User>.Update.Push(user => user.Decks, deckId);
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.MatchedCount > 0;
    }
}