using Microsoft.EntityFrameworkCore;
using CryptoTracker.Models;

namespace CryptoTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<PortfolioItem> PortfolioItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<PortfolioItem>()
            .Property(p => p.Amount).HasColumnType("decimal(18,8)");
        modelBuilder.Entity<PortfolioItem>()
            .Property(p => p.BuyPrice).HasColumnType("decimal(18,2)");

        // Cascade delete, when we delete user, we also wanna delete his portfolio
        modelBuilder.Entity<User>()
            .HasMany(u => u.Portfolio)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
