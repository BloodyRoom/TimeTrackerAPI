using AutoMapper;
using Core.Interfaces;
using Core.Models.User;
using Domain;
using Domain.Entities;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TimeTrackerAPI.Controllers
{
    /// <summary>
    /// Авторизація та управління акаунтом користувача
    /// </summary>
    /// <remarks>
    /// Включає:
    /// - реєстрацію користувача
    /// - логін по email/password
    /// - логін через Google
    /// - logout (revocation refresh token)
    /// </remarks>
    [ApiController]
    [Tags("Auth")]
    [Route("api/[controller]")]
    public class AccountController(
        TrackerDBContext _db,
        IJwtService _jwt,
        IMapper mapper) : ControllerBase
    {
        [HttpPost("Register")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (_db.Users.Any(u => u.Email == request.Email))
                return BadRequest("User already exists");

            string hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = mapper.Map<UserEntity>(request);
            user.PasswordHash = hash;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Login")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            var tokens = await _jwt.IssueTokens(user);
            return Ok(tokens);
        }

        [HttpPost("Google")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);
            }
            catch
            {
                return BadRequest("Invalid Google credential");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                user = new UserEntity
                {
                    Name = payload.Name,
                    Email = payload.Email,
                    Provider = "google",
                    ProviderId = payload.Subject
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            var tokens = await _jwt.IssueTokens(user);
            return Ok(tokens);
        }


        [HttpPost("Logout")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

            if (token == null)
                return Ok();

            token.IsRevoked = true;
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
