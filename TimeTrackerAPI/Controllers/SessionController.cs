using Core.Helpers;
using Core.Models.TimeStatistic;
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

    [HttpGet("Statistic/Day")]
    public async Task<IActionResult> Day()
    {
        int userId = User.GetUserId();

        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(1);

        return Ok(await GetStatistic(userId, from, to, false));
    }

    [HttpGet("Statistic/Week")]
    public async Task<IActionResult> Week()
    {
        int userId = User.GetUserId();

        var to = DateTime.UtcNow.Date.AddDays(1);
        var from = to.AddDays(-7);

        return Ok(await GetStatistic(userId, from, to, false));
    }

    [HttpGet("Statistic/Month")]
    public async Task<IActionResult> Month()
    {
        int userId = User.GetUserId();
        var now = DateTime.UtcNow;

        var from = new DateTime(
            now.Year,
            now.Month,
            1,
            0, 0, 0,
            DateTimeKind.Utc);

        var to = from.AddMonths(1);

        return Ok(await GetStatistic(userId, from, to, false));
    }

    [HttpGet("Statistic/Year")]
    public async Task<IActionResult> Year()
    {
        int userId = User.GetUserId();
        var now = DateTime.UtcNow;

        var from = new DateTime(
            now.Year,
            1,
            1,
            0, 0, 0,
            DateTimeKind.Utc);

        var to = from.AddYears(1);

        return Ok(await GetStatistic(userId, from, to, true));
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


    private async Task<TimeStatisticResponse> GetStatistic(
        int userId,
        DateTime from,
        DateTime to,
        bool groupByMonth)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var sessions = await _db.TimeSessions
            .Where(s =>
                s.UserId == userId &&
                s.Status == SessionStatus.Stopped &&
                s.EndTime >= from &&
                s.EndTime < to)
            .ToListAsync();

        var items = groupByMonth
            ? sessions
                .GroupBy(s => new DateTime(
                    s.EndTime!.Value.Year,
                    s.EndTime.Value.Month,
                    1,
                    0, 0, 0,
                    DateTimeKind.Utc))
                .Select(g => new TimeStatisticItem
                {
                    Date = g.Key,
                    TotalSeconds = g.Sum(x => x.DurationSeconds)
                })
                .OrderBy(x => x.Date)
                .ToList()
            : sessions
                .GroupBy(s => s.EndTime!.Value.Date)
                .Select(g => new TimeStatisticItem
                {
                    Date = DateTime.SpecifyKind(g.Key, DateTimeKind.Utc),
                    TotalSeconds = g.Sum(x => x.DurationSeconds)
                })
                .OrderBy(x => x.Date)
                .ToList();

        return new TimeStatisticResponse    
        {
            TotalSeconds = sessions.Sum(s => s.DurationSeconds),
            SessionsCount = sessions.Count,
            Items = items
        };
    }
}
