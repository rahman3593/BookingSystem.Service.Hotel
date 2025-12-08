using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.DeleteHotel
{
    public class DeleteHotelCommandHandler : IRequestHandler<DeleteHotelCommand, Unit>
    {
        private readonly IHotelRepository _hotelRepository;

        public DeleteHotelCommandHandler(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository;
        }
        public async Task<Unit> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
        {
            await _hotelRepository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}
