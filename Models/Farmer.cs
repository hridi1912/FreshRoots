using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshRoots.Models
{
    public class Farmer
    {
        [Key]
        public int FarmerId { get; set; }

        [Required]
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = null!;

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? PickupLocation { get; set; }
        public string? ProfilePicture { get; set; }

        public ICollection<Product> Products { get; set; }


        public ApplicationUser User { get; set; } = null!;
    }
}
