using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models;

namespace TodoApi.Services;

public class TokenService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly SigningCredentials _signingCredentials;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
    }

    public string CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: _signingCredentials);

        return TokenHandler.WriteToken(token);
    }
}
