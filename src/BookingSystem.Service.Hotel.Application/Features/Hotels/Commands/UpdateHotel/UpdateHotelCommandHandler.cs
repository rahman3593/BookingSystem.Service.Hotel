using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Domain.Exceptions;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.UpdateHotel
{
    public class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand, Unit>
    {
        private readonly IHotelRepository _hotelRepository;

        public UpdateHotelCommandHandler(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository;
        }
        public async Task<Unit> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
        {
            var hotel = await _hotelRepository.GetByIdAsync(request.Id);
            if (hotel == null)
            {
                throw new HotelNotFoundException(request.Id);
            }

            hotel.UpdateDetails(name:request.Name,description:request.Description,starRating:request.StarRating);
            hotel.UpdateAddress(street: request.Street, city: request.City, state: request.State, country: request.Country, zipCode: request.ZipCode);
            hotel.UpdateContactInfo(email: request.Email, phone: request.Phone, website: request.Website);
            hotel.ChangeStatus(request.Status);
            hotel.UpdateTimestamp();
            await _hotelRepository.UpdateAsync(hotel);
            return Unit.Value;
        }
    }
}
