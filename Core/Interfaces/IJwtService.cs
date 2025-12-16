using Core.Models.User;
using Domain.Entities;

namespace Core.Interfaces;

public interface IJwtService
{
    string CreateAccessToken(UserModel user);
    string CreateRefreshToken();
    Task<object> IssueTokens(UserEntity user);
}