using AutoMapper;
using NetProject.Application.Todos.Dtos;
using NetProject.Domain.Entities;

namespace NetProject.Application.Mapping;

public sealed class TodoMappingProfile : Profile
{
    public TodoMappingProfile()
    {
        CreateMap<TodoItem, TodoDto>();
    }
}

