namespace NetProject.Application.Todos.Dtos;

public sealed record TodoDto(Guid Id, string Title, bool IsDone);
public sealed record CreateTodoRequest(string Title);
public sealed record UpdateTodoRequest(string Title, bool IsDone);

