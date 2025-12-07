using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.DTOs;
using BookingSystem.Service.Hotel.Domain.Exceptions;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelById
{
    public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, DTOs.HotelDto>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public GetHotelByIdQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }
        public async Task<HotelDto> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
        {
            var hotel= await _hotelRepository.GetByIdAsync(request.HotelId);

            if (hotel == null)
            {
                throw new HotelNotFoundException(request.HotelId);
            }

            return _mapper.Map<HotelDto>(hotel);
            
        }
    }
}
