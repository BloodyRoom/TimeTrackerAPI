namespace Domain.Entities;

public class UserEntity : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Provider { get; set; } = string.Empty; // local | google | github
    public string? ProviderId { get; set; }
}
