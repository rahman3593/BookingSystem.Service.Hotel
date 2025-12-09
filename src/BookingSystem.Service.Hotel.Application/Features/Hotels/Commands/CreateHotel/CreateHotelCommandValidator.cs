using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.CreateHotel
{
    public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
    {
        public CreateHotelCommandValidator()
        {

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Hotel name is required.")
                .MaximumLength(200).WithMessage("Hotel name must not exceed 200 characters.");

            RuleFor(x => x.Description).MaximumLength(1000)
                .When(x=>!string.IsNullOrEmpty(x.Description))
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
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

            RuleFor(x => x.Website)
                .MaximumLength(200).WithMessage("Website URL cannot exceed 200 characters");

            RuleFor(x => x.StarRating)
                .IsInEnum().WithMessage("Star rating must be between 1 and 5");
        }
    }
}
