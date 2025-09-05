using FreshRoots.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace FreshRoots.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        // Later you can add your app tables here when you move from mock data to EF:
        // public DbSet<Product> Products { get; set; }
    }
}