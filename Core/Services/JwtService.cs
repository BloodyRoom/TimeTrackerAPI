using AutoMapper;
using Core.Interfaces;
using Core.Models.User;
using Domain;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core.Services;

public class JwtService(
    TrackerDBContext _db, 
    IConfiguration configuration, 
    IMapper _mapper ) : IJwtService
{
    private readonly string _key = configuration["Jwt:Key"] ?? "";

    public string CreateAccessToken(UserModel user)
    {
        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("name", user.Name),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }



    public async Task<object> IssueTokens(UserEntity user)
    {
        var accessToken = CreateAccessToken(_mapper.Map<UserModel>(user));

        var refresh = new RefreshTokenEntity
        {
            Token = CreateRefreshToken(),
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            IsRevoked = false
        };

        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync();

        return new
        {
            accessToken,
            refreshToken = refresh.Token
        };
    }

}
