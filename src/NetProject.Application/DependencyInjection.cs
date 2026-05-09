using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetProject.Application.Auth;
using NetProject.Application.Todos;

namespace NetProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Avoid AutoMapper.Extensions.Microsoft.DependencyInjection (archived). Register IMapper manually.
        services.AddSingleton<IMapper>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var cfg = new MapperConfigurationExpression();
            cfg.AddMaps(typeof(AssemblyMarker).Assembly);

            var config = new MapperConfiguration(cfg, loggerFactory);
            return config.CreateMapper();
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITodoService, TodoService>();

        return services;
    }
}

