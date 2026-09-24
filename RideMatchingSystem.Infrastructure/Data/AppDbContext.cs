using Microsoft.EntityFrameworkCore;
using RideMatchingSystem.Domain.Entities;

namespace RideMatchingSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<RideRequest> RideRequests => Set<RideRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>().HasKey(d => d.Id);
        modelBuilder.Entity<RideRequest>().HasKey(r => r.Id);
    }
}
