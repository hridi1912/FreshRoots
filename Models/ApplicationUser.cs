using Microsoft.AspNetCore.Identity;

namespace FreshRoots.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public string UserType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}