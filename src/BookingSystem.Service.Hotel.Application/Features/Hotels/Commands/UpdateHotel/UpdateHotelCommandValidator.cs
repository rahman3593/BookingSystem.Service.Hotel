using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.UpdateHotel
{
    public class UpdateHotelCommandValidator : AbstractValidator<UpdateHotelCommand>
    {
        public UpdateHotelCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid hotel ID.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Hotel name is required.")
                .MaximumLength(200).WithMessage("Hotel name must not exceed 200 characters.");
            RuleFor(x => x.Description).MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 1000 characters.");
            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City name cannot exceed 100 characters");
            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters");
            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Invalid email format");
            RuleFor(x => x.Phone)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Phone must not exceed 20 characters");
            RuleFor(x => x.Website)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Website))
                .WithMessage("Website must not exceeed 200 characters");
            RuleFor(x => x.Street)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Street))
                .WithMessage("State must not exceed 100 characters");
            RuleFor(x => x.ZipCode)
                .MaximumLength(20)
                .When(x => !string.IsNullOrEmpty(x.ZipCode))
                .WithMessage("Zipcode must not exceed 20 characters");
        }
    }
}
