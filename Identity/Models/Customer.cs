using System;
using System.Collections.Generic;

namespace Identity.Models
{
    public partial class Customer
    {
        public int? CustomerId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? CountryOfResidence { get; set; }

        public string? Gender { get; set; }

        public double? Age { get; set; }
    }
}