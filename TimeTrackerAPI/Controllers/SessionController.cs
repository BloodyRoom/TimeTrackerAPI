using Core.Helpers;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TimeTrackerAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SessionController(
    TrackerDBContext _db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        int userId = User.GetUserId();

        var active = await _db.TimeSessions
            .FirstOrDefaultAsync(s => s.UserId == userId &&
                (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));

        return Ok(active);
    }


    [HttpPost("Start")]
    public async Task<IActionResult> Start()
    {
        int userId = User.GetUserId();

        var user = await _db.Users.FindAsync(userId);
        var active = await _db.TimeSessions
            .FirstOrDefaultAsync(s => s.UserId == userId &&
                (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));

        if (active != null)
            return BadRequest("Session already running");

        var session = new TimeSessionEntity
        {
            User = user ?? new UserEntity(),
            StartTime = DateTime.UtcNow,
            LastResumeTime = DateTime.UtcNow,
            Status = SessionStatus.Active,
            DurationSeconds = 0
        };

        _db.TimeSessions.Add(session);
        await _db.SaveChangesAsync();

        return Ok(session);
    }

    [HttpPost("Pause")]
    public async Task<IActionResult> Pause()
    {
        int userId = User.GetUserId();

        var session = await _db.TimeSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SessionStatus.Active);

        if (session == null)
            return BadRequest("No active session");

        var now = DateTime.UtcNow;

        session.DurationSeconds += (int)(now - session.LastResumeTime!.Value).TotalSeconds;

        session.LastResumeTime = null;
        session.Status = SessionStatus.Paused;

        await _db.SaveChangesAsync();

        return Ok(session);
    }

    [HttpPost("Resume")]
    public async Task<IActionResult> Resume()
    {
        int userId = User.GetUserId();

        var session = await _db.TimeSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SessionStatus.Paused);

        if (session == null)
            return BadRequest("No paused session");

        session.LastResumeTime = DateTime.UtcNow;
        session.Status = SessionStatus.Active;

        await _db.SaveChangesAsync();

        return Ok(session);
    }

    [HttpPost("Stop")]
    public async Task<IActionResult> Stop()
    {
        int userId = User.GetUserId();

        var session = await _db.TimeSessions
            .FirstOrDefaultAsync(s => s.UserId == userId &&
                (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));

        if (session == null)
            return BadRequest("No running session");

        var now = DateTime.UtcNow;

        if (session.Status == SessionStatus.Active)
        {
            session.DurationSeconds += (int)(now - session.LastResumeTime!.Value).TotalSeconds;
        }

        session.EndTime = now;
        session.LastResumeTime = null;
        session.Status = SessionStatus.Stopped;

        await _db.SaveChangesAsync();

        return Ok(session);
    }

}
