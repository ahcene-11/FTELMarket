using FTELMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace FTELMarket.Data;

public class FtelMarketDbContext : DbContext
{
    public FtelMarketDbContext(DbContextOptions<FtelMarketDbContext> options)
        : base(options)
    {
    }

    //declaration des tables de la bdd

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FtelMarketDbContext).Assembly);
    }
}