using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using DeckBuilder.Api.Configurations;
using DeckBuilder.Api.ViewModels;
using Microsoft.IdentityModel.Tokens;

namespace DeckBuilder.Api.Services;

public static class TokenService
{
    public static string GenerateAuthToken(AuthTokenInput input, JwtConfiguration jwtConfiguration)
    {
        var secretKeyBytes = Convert.FromBase64String(jwtConfiguration.Secret);
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new []
            {
                new Claim("email", input.Email),
                new Claim("name", input.Name),
            }),
            Expires = now.AddMinutes(jwtConfiguration.ExpiresInMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKeyBytes),
                SecurityAlgorithms.HmacSha256Signature),
        };
        var tokenData = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(tokenData);
    }

    public static string GenerateRefreshToken(int tokenSize = 64)
    {
        var randomNumber = new byte[tokenSize];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        
        return Convert.ToBase64String(randomNumber);
    }
}