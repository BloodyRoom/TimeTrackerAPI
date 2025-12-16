using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public interface IEntity<T>
{
    T Id { get; set; }
    bool IsDeleted { get; set; }
    DateTime Created { get; set; }
    DateTime Updated { get; set; }
}

public abstract class BaseEntity<T> : IEntity<T>
{
    [Key]
    public T Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime Created { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    public DateTime Updated { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
}