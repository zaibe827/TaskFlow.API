using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetProject.Application;
using NetProject.Application.Auth;
using NetProject.Application.Auth.Dtos;
using NetProject.Application.Common.Errors;
using NetProject.Application.Todos;
using NetProject.Application.Todos.Dtos;
using NetProject.Infrastructure;
using NetProject.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT auth middleware (API layer concerns).
var jwtSection = builder.Configuration.GetSection(AuthOptions.SectionName);
var signingKey = jwtSection.GetValue<string>(nameof(AuthOptions.SigningKey)) ?? "";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection[nameof(AuthOptions.Issuer)],
            ValidateAudience = true,
            ValidAudience = jwtSection[nameof(AuthOptions.Audience)],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(new
        {
            message = "Validation failed.",
            errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
        });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5173", "http://localhost:5174"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Very small minimal API endpoints to keep the initial API shippable;
// controllers can be added later without changing app setup.
app.MapPost("/api/auth/register", async ([FromServices] IAuthService auth, [FromBody] RegisterRequest req, CancellationToken ct) =>
{
    try { return Results.Ok(await auth.RegisterAsync(req, ct)); }
    catch (AppException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapPost("/api/auth/login", async ([FromServices] IAuthService auth, [FromBody] LoginRequest req, CancellationToken ct) =>
{
    try { return Results.Ok(await auth.LoginAsync(req, ct)); }
    catch (AppException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapPost("/api/auth/refresh", async ([FromServices] IAuthService auth, [FromBody] RefreshRequest req, CancellationToken ct) =>
{
    try { return Results.Ok(await auth.RefreshAsync(req, ct)); }
    catch (AppException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapGet("/api/todos", async (HttpContext http, [FromServices] ITodoService todos, CancellationToken ct) =>
{
    var userId = http.User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
    if (userId is null) return Results.Unauthorized();
    return Results.Ok(await todos.GetMyTodosAsync(Guid.Parse(userId), ct));
}).RequireAuthorization();

app.MapPost("/api/todos", async (HttpContext http, [FromServices] ITodoService todos, [FromBody] CreateTodoRequest req, CancellationToken ct) =>
{
    var userId = http.User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
    if (userId is null) return Results.Unauthorized();
    return Results.Ok(await todos.CreateAsync(Guid.Parse(userId), req, ct));
}).RequireAuthorization();

app.MapPut("/api/todos/{id:guid}", async (HttpContext http, [FromServices] ITodoService todos, Guid id, [FromBody] UpdateTodoRequest req, CancellationToken ct) =>
{
    var userId = http.User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
    if (userId is null) return Results.Unauthorized();
    var updated = await todos.UpdateAsync(Guid.Parse(userId), id, req, ct);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
}).RequireAuthorization();

app.MapDelete("/api/todos/{id:guid}", async (HttpContext http, [FromServices] ITodoService todos, Guid id, CancellationToken ct) =>
{
    var userId = http.User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
    if (userId is null) return Results.Unauthorized();
    var ok = await todos.DeleteAsync(Guid.Parse(userId), id, ct);
    return ok ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();

// Auto-migrate in Development for convenience.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
