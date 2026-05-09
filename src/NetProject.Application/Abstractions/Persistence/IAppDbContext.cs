using Microsoft.EntityFrameworkCore;
using NetProject.Domain.Entities;

namespace NetProject.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

