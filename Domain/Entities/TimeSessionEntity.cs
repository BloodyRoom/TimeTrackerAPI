using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Entities;

public enum SessionStatus
{
    Active,
    Paused,
    Stopped
}


public class TimeSessionEntity : BaseEntity<int>
{
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public int DurationSeconds { get; set; }

    public DateTime? LastResumeTime { get; set; }

    public SessionStatus Status { get; set; }
}
