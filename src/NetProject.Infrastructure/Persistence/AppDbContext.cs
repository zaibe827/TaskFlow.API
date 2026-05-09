using Microsoft.EntityFrameworkCore;
using NetProject.Application.Abstractions.Persistence;
using NetProject.Domain.Entities;

namespace NetProject.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Email).HasMaxLength(256);
            b.Property(x => x.PasswordHash).HasMaxLength(256);
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.Property(x => x.TokenHash).HasMaxLength(128);
            b.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoItem>(b =>
        {
            b.Property(x => x.Title).HasMaxLength(256);
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

