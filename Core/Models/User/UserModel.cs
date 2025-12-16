namespace Core.Models.User;

public class UserModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // local | google | github
    public string? ProviderId { get; set; }
}
