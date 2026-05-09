using NetProject.Application.Todos.Dtos;

namespace NetProject.Application.Todos;

public interface ITodoService
{
    Task<IReadOnlyList<TodoDto>> GetMyTodosAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TodoDto> CreateAsync(Guid userId, CreateTodoRequest request, CancellationToken cancellationToken = default);
    Task<TodoDto?> UpdateAsync(Guid userId, Guid todoId, UpdateTodoRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default);
}

