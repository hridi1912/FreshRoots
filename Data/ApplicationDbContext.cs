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

            builder.Entity<Farmer>()
                .HasOne(f => f.User)
                .WithOne()
                .HasForeignKey<Farmer>(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Buyer>()
                .HasOne(b => b.User)
                .WithOne()
                .HasForeignKey<Buyer>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
