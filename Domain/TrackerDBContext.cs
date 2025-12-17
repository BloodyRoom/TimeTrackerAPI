using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;

public class TrackerDBContext : DbContext
{
    public TrackerDBContext(DbContextOptions<TrackerDBContext> dbContextOptions)
        : base(dbContextOptions) { }

    public DbSet<UserEntity> Users { get; set; } 
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; } 
    public DbSet<TimeSessionEntity> TimeSessions { get; set; }
}
