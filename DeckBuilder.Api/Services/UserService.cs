using DeckBuilder.Api.Configurations;
using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
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
            Decks = input.Decks!
        };
        
        await _collection.InsertOneAsync(user, null, cancellationToken);

        return user.Id;
    }

    public async ValueTask<string> LoginWithCredentails(CredentialsInput input, CancellationToken cancellationToken)
    {
        var existingUser = await _collection
            .AsQueryable()
            .FirstOrDefaultAsync(u => Equals(u.Email, input.Email), cancellationToken);

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
        return await _collection.AsQueryable().ToListAsync(cancellationToken);
    }
}