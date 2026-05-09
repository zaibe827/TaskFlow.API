using NetProject.Domain.Common;

namespace NetProject.Domain.Entities;

public sealed class TodoItem : EntityBase
{
    public required string Title { get; set; }
    public bool IsDone { get; set; }

    public Guid UserId { get; init; }
    public User? User { get; init; }
}

