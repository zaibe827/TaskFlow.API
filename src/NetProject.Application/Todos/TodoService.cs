using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using NetProject.Application.Abstractions.Caching;
using NetProject.Application.Abstractions.Persistence;
using NetProject.Application.Todos.Dtos;
using NetProject.Domain.Entities;

namespace NetProject.Application.Todos;

public sealed class TodoService(IAppDbContext db, ICacheService cache, IMapper mapper) : ITodoService
{
    private static string CacheKey(Guid userId) => $"todos:{userId}";

    public async Task<IReadOnlyList<TodoDto>> GetMyTodosAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetAsync<List<TodoDto>>(CacheKey(userId), cancellationToken);
        if (cached is not null) return cached;

        var todos = await db.TodoItems
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Id)
            .ProjectTo<TodoDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        await cache.SetAsync(CacheKey(userId), todos, TimeSpan.FromSeconds(30), cancellationToken);
        return todos;
    }

    public async Task<TodoDto> CreateAsync(Guid userId, CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TodoItem
        {
            UserId = userId,
            Title = request.Title.Trim(),
            IsDone = false,
        };

        db.TodoItems.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKey(userId), cancellationToken);

        return mapper.Map<TodoDto>(entity);
    }

    public async Task<TodoDto?> UpdateAsync(Guid userId, Guid todoId, UpdateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.TodoItems.SingleOrDefaultAsync(
            x => x.Id == todoId && x.UserId == userId,
            cancellationToken);

        if (entity is null) return null;

        entity.Title = request.Title.Trim();
        entity.IsDone = request.IsDone;
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKey(userId), cancellationToken);

        return mapper.Map<TodoDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default)
    {
        var entity = await db.TodoItems.SingleOrDefaultAsync(
            x => x.Id == todoId && x.UserId == userId,
            cancellationToken);

        if (entity is null) return false;

        db.TodoItems.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKey(userId), cancellationToken);
        return true;
    }
}

