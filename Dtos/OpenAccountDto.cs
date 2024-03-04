using System;
using System.ComponentModel.DataAnnotations;
using ChatBot.Database.Models;
using FluentValidation;

namespace ChatBot.Dtos
{
	public class OpenAccountDto
	{
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string OtherName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string BVNNumber { get; set; }

        [Required]
        public string NIN { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public AccountType AccountType { get; set; }
    }

    public class OpenAccountDtoValidator : AbstractValidator<OpenAccountDto>
    {
        public OpenAccountDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name is required");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid Email format");
            RuleFor(x => x.PhoneNumber).MinimumLength(11).MaximumLength(11).WithMessage("Invalid Phone Number");
            RuleFor(x => x.BVNNumber).MinimumLength(11).MaximumLength(11).WithMessage("Invalid BVN");
            RuleFor(x => x.NIN).MinimumLength(11).MaximumLength(11).WithMessage("Invalid NIN");
            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of Birth is required")
                .LessThan(DateTime.Today).WithMessage("Date of Birth cannot be in the future");
            RuleFor(x => x.AccountType).IsInEnum().WithMessage("Invalid Account Type");
        }
    }

}