using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatBot.Database.Models
{
	public class Customer: BaseEntity
    {
        [Required]
        [StringLength(30, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [StringLength(30, ErrorMessage = "Other Name cannot exceed 50 characters")]
        public string OtherName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Phone number must be 11 digits")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must be numeric")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "BVN must be exactly 11 digits")]
        [RegularExpression(@"^\d+$", ErrorMessage = "BVN must be numeric")]
        public string BVNNumber { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "NIN must be exactly 11 digits")]
        [RegularExpression(@"^\d+$", ErrorMessage = "NIN must be numeric")]
        public string NIN { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public ICollection<Account> Accounts { get; set; }
    }
}

