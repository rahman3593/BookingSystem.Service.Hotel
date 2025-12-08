using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Domain.Entities;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.CreateHotel
{
    public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
    {
        private readonly IHotelRepository _hotelRepository;
        public CreateHotelCommandHandler(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository;        }
        public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
        {
            var hotel = new Domain.Entities.Hotel(name: request.Name,
                                                  description: request.Description,
                                                  starRating: request.StarRating,
                                                  city: request.City,
                                                  country: request.Country
                                                );

            hotel.UpdateAddress(street: request.Street,
                                city: request.City,
                                state: request.State,
                                country: request.Country,
                                zipCode: request.ZipCode);



            hotel.UpdateContactInfo(email: request.Email,
                                phone: request.Phone,
                                website: request.Website);


            var createdHotel = await _hotelRepository.AddAsync(hotel);
            return createdHotel.Id;
        }
    }
}
