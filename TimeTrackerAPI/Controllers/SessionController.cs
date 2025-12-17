using Core.Helpers;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TimeTrackerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SessionController(
    TrackerDBContext _db) : ControllerBase
{
    [HttpPost("Start")]
    public async Task<IActionResult> Start()
    {
        int userId = User.GetUserId();

        var active = await _db.TimeSessions
            .FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));

        if (active != null)
            return BadRequest("Session already running");

        var session = new TimeSessionEntity
        {
            UserId = userId,
            StartTime = DateTime.UtcNow,
            LastResumeTime = DateTime.UtcNow,
            Status = SessionStatus.Active,
            DurationSeconds = 0
        };

        _db.TimeSessions.Add(session);
        await _db.SaveChangesAsync();

        return Ok(session);
    }
}
