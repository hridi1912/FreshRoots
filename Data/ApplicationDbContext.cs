using FreshRoots.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FreshRoots.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        public DbSet<Farmer> Farmers { get; set; }
        public DbSet<Buyer> Buyers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Farmer PK → force column to be FarmerId (not FarmerID)
            builder.Entity<Farmer>()
                .HasKey(f => f.FarmerId);

            builder.Entity<Farmer>()
                .Property(f => f.FarmerId)
                .HasColumnName("FarmerId");

            // Farmer ↔ User (1:1)
            builder.Entity<Farmer>()
                .HasOne(f => f.User)
                .WithOne()
                .HasForeignKey<Farmer>(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Buyer PK → force column to be BuyerId (not BuyerID)
            builder.Entity<Buyer>()
                .HasKey(b => b.BuyerId);

            builder.Entity<Buyer>()
                .Property(b => b.BuyerId)
                .HasColumnName("BuyerId");

            // Buyer ↔ User (1:1)
            builder.Entity<Buyer>()
                .HasOne(b => b.User)
                .WithOne()
                .HasForeignKey<Buyer>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product ↔ Farmer (N:1)
            builder.Entity<Product>()
                .HasOne(p => p.Farmer)
                .WithMany(f => f.Products)
                .HasForeignKey(p => p.FarmerId)
                .HasPrincipalKey(f => f.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store FarmerProfile inside Products as an owned type
            builder.Entity<Product>().OwnsOne(p => p.FarmerProfile, fp =>
            {
                fp.Property(x => x.FarmName).HasMaxLength(120);
                fp.Property(x => x.Certification).HasMaxLength(120);
            });

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
}
