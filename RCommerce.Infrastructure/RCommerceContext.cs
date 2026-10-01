using RCommerce.Core;
using Microsoft.EntityFrameworkCore;

namespace RCommerce.Infrastructure;

public class RCommerceContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderProduct> OrderProducts { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    public RCommerceContext(DbContextOptions<RCommerceContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            string connectionString;
            
            if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgresql://"))
            {
                connectionString = "Data Source=rcommerce.db";
            }
            else
            {
                connectionString = databaseUrl ?? "Data Source=rcommerce.db";
            }
            
            optionsBuilder.UseSqlite(connectionString);
            optionsBuilder.LogTo(Console.WriteLine);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Product>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.HasKey(e => new { e.ProductsId, e.OrdersId });
            entity.HasIndex(e => e.OrdersId, "IX_OrderProducts_OrdersId");
            entity.HasIndex(e => e.ProductsId, "IX_OrderProducts_ProductsId");
            entity.HasOne(d => d.Order).WithMany(p => p.OrderProducts).HasForeignKey(d => d.OrdersId);
            entity.HasOne(d => d.Product).WithMany(p => p.OrderProducts).HasForeignKey(d => d.ProductsId);
        });

        modelBuilder.Entity<Category>()
            .ToTable("Categories");

        modelBuilder.Entity<OrderProduct>()
            .ToTable("OrderProducts")
            .HasKey(x => new { x.OrdersId, x.ProductsId });

        // --- seed data that can be deleted ---
        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "Electronics" },
            new Category { Id = 2, CategoryName = "Clothing" },
            new Category { Id = 3, CategoryName = "Books" }
        );

        // Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Laptop",       Price = 999.99m, IsAvailable = true, CategoryId = 1, ImagePath = "https://github.com/therealisc/AssetManagementSystem/blob/master/laptop01.jpg?raw=true" },
            new Product { Id = 2, ProductName = "Headphones",   Price = 49.99m, IsAvailable = true, CategoryId = 1, ImagePath = "https://github.com/therealisc/AssetManagementSystem/blob/master/headphones01.jpg?raw=true" },
            new Product { Id = 3, ProductName = "T-Shirt",      Price = 19.99m, IsAvailable = true, CategoryId = 2, ImagePath = "https://github.com/therealisc/AssetManagementSystem/blob/master/shirt01.jpg?raw=true" },
            new Product { Id = 4, ProductName = "Trousers",     Price = 59.99m, IsAvailable = true, CategoryId = 2, ImagePath = "https://github.com/therealisc/AssetManagementSystem/blob/master/trousers01.jpg?raw=true" },
            new Product { Id = 5, ProductName = "C# in Depth",  Price = 34.99m, IsAvailable = true, CategoryId = 3, ImagePath = "https://github.com/therealisc/AssetManagementSystem/blob/master/book01.jpg?raw=true" },
            new Product { Id = 6, ProductName = "Icon",         Price = 89.99m, IsAvailable = true, CategoryId = 3, ImagePath = "https://github.com/therealisc/AssetManagementSystem/blob/master/icon01.jpg?raw=true" }
        );
    }
}
