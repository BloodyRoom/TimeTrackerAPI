using AutoMapper;
using Core.Interfaces;
using Core.Models.User;
using Domain;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace TimeTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(
        TrackerDBContext _db, 
        IJwtService _jwt,
        IMapper mapper) : ControllerBase
    {

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
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
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            await _db.SaveChangesAsync();

            var tokens = await _jwt.IssueTokens(user);
            return Ok(tokens);
        }

        [HttpPost("Google")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);

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
        public async Task<IActionResult> Logout(LogoutRequest request)
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
