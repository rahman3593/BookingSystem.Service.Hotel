using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace BookingSystem.Service.Hotel.Application.Features.RoomTypes.Commands
{
    public class CreateRoomTypeCommandValidator : AbstractValidator<CreateRoomTypeCommand>
    {
        public CreateRoomTypeCommandValidator()
        {
            // HotelId must be greater than 0; values <= 0 are invalid
            RuleFor(x => x.HotelId).GreaterThan(0).WithMessage("Invalid hotel ID");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Room type name is required")
                .MaximumLength(100).WithMessage("Room type name must not exceed 100 characters");
            RuleFor(x => x.Description).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must not exceed 1000 characters");
            RuleFor(x => x.MaxOccupancy).GreaterThan(0).WithMessage("Max occupancy must be greater than 0");
            RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative");
            RuleFor(x => x.Size).GreaterThan(0).WithMessage("Room size must be greater than 0");
            RuleFor(x => x.BedType).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.BedType)).WithMessage("Bed type must not exceed 50 characters");
            RuleFor(x => x.ViewType).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.ViewType)).WithMessage("View type must not exceed 50 characters");
        }
    }
}
