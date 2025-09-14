using FreshRoots.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FreshRoots.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Store FarmerProfile inside Products as an owned type
            builder.Entity<Product>().OwnsOne(p => p.FarmerProfile, fp =>
            {
                fp.Property(x => x.FarmName).HasMaxLength(120);
                fp.Property(x => x.Certification).HasMaxLength(120);
            });
            builder.Entity<Product>()
          .Property(p => p.Price)
          .HasPrecision(18, 2);

            // Order -> Buyer (Cascade delete allowed)
            builder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Order (Cascade delete allowed)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Product (Restrict delete, no cascade)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem -> Farmer (Restrict delete, no cascade)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Farmer)
                .WithMany()
                .HasForeignKey(oi => oi.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
               .HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(p => p.FarmerId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
              .HasOne(p => p.Farmer)
              .WithMany()
              .HasForeignKey(p => p.FarmerId)
              .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
