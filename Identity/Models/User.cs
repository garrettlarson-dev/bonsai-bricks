using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions; // Import for better regex handling

namespace Identity.Models
{
    public class User
    {
        [Required]
        public string Name { get; set; } // Consider using separate FirstName and LastName if needed

        [Required]
        [EmailAddress(ErrorMessage = "E-mail is not valid")] // Use built-in EmailAddress attribute
        public string Email { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be 8-64 characters")] // Enforce password length
        public string Password { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]+(?: [a-zA-Z]+)*$", ErrorMessage = "First name must contain only letters and spaces")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]+(?: [a-zA-Z]+)*$", ErrorMessage = "Last name must contain only letters and spaces")]
        public string LastName { get; set; }

        [Required]
        public DateOnly BirthDate { get; set; } // No regex needed for DateOnly

        [Required]
        [RegularExpression(@"^[A-Z][a-zA-Z ]+$", ErrorMessage = "Country name must start with a capital letter and contain letters and spaces")]
        public string CountryOfResidence { get; set; }
        // Consider using an enum for common countries

        [Required]
        [RegularExpression(@"^(Male|Female|Other)$", ErrorMessage = "Gender must be male, female, or other")]
        public string Gender { get; set; }

        [Required]
        [Range(13, 120, ErrorMessage = "Age must be between 13 and 120")] // Set reasonable age range
        public double Age { get; set; }
    }
}
