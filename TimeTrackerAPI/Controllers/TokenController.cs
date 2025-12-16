using AutoMapper;
using Core.Interfaces;
using Core.Models.User;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TimeTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(
        TrackerDBContext _db,
        IJwtService _jwt,
        IMapper mapper) : ControllerBase
    {
        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var stored = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (stored == null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            stored.IsRevoked = true;

            var newRefresh = _jwt.CreateRefreshToken();
            var newAccess = _jwt.CreateAccessToken(mapper.Map<UserModel>(stored.User));

            var newEntity = new RefreshTokenEntity
            {
                Token = newRefresh,
                User = stored.User,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _db.RefreshTokens.Add(newEntity);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccess,
                refreshToken = newRefresh
            });
        }

    }
}
