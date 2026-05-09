using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetProject.Application.Abstractions.Caching;
using NetProject.Application.Abstractions.Persistence;
using NetProject.Application.Abstractions.Security;
using NetProject.Application.Abstractions.Time;
using NetProject.Application.Auth;
using NetProject.Infrastructure.Caching;
using NetProject.Infrastructure.Persistence;
using NetProject.Infrastructure.Security;
using NetProject.Infrastructure.Time;

namespace NetProject.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        return services;
    }
}

