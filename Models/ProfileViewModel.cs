using System.ComponentModel.DataAnnotations;

namespace FreshRoots.Models
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }     // read-only in UI; stored in AspNetUsers

        [Display(Name = "Address")]
        public string? Address { get; set; }   // optional

        [Url(ErrorMessage = "Please enter a valid URL.")]
        [Display(Name = "Profile Picture (URL)")]
        public string? ProfilePicture { get; set; }   // optional

        [Display(Name = "Pickup Location (Farmer only)")]
        public string? PickupLocation { get; set; }   // optional
    }
}
