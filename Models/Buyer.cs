using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshRoots.Models
{
    public class Buyer
    {
        [Key]
        public int BuyerID { get; set; }

        [Required]
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = null!;

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
