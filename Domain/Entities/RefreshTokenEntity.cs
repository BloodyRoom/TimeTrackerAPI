using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

public class RefreshTokenEntity
{
    public int Id { get; set; }

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    public int UserId { get; set; }
    public UserEntity User { get; set; } = new UserEntity();
}
