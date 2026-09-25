using FlowAccount_Workshop.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowAccount_Workshop.Data;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasIndex(product => product.Sku)
            .IsUnique();
    }
}
