using Microsoft.EntityFrameworkCore;
using MyFirstApi.Models;

namespace MyFirstApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPhone> CustomerPhones => Set<CustomerPhone>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Keep monetary values accurate in SQL Server
        modelBuilder.Entity<Car>()
            .Property(car => car.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(order => order.SalePrice)
            .HasPrecision(18, 2);
    }
}