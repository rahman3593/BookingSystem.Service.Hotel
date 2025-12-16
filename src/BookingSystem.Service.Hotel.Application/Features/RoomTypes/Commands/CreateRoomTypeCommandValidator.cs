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
        }
    }
}
