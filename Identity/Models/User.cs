using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class User
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        
        public int CustomerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateOnly BirthDate { get; set; }

        public string CountryOfResidence { get; set; }

        public string Gender { get; set; }

        public double Age { get; set; }
    }
}
