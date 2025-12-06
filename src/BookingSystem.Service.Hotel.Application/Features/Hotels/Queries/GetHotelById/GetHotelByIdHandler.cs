using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.DTOs;
using BookingSystem.Service.Hotel.Domain.Exceptions;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelById
{
    public class GetHotelByIdHandler : IRequestHandler<GetHotelByIdQuery, DTOs.HotelDto>
    {
        private readonly IHotelRepository _hotelRepository;

        public GetHotelByIdHandler(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository;
        }
        public async Task<HotelDto> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
        {
            var hotel= await _hotelRepository.GetByIdAsync(request.HotelId);

            if (hotel == null)
            {
                throw new HotelNotFoundException(request.HotelId);
            }

            var hotelDto = new HotelDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                StarRating = hotel.StarRating,
                Status = hotel.Status.ToString(),
                Street = hotel.Street,
                City = hotel.City,
                State = hotel.State,
                Country = hotel.Country,
                ZipCode = hotel.ZipCode,
                Email = hotel.Email,
                Phone = hotel.Phone,
                Website = hotel.Website,
                CreatedAt = hotel.CreatedAt,
                UpdatedAt = hotel.UpdatedAt
            };

            return hotelDto;
        }
    }
}
