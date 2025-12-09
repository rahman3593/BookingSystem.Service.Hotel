using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.DTOs;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.SearchHotels
{
    public class SearchHotelsQueryHandler : IRequestHandler<SearchHotelsQuery, List<HotelDto>>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;
        public SearchHotelsQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }

        public async Task<List<HotelDto>> Handle(SearchHotelsQuery request, CancellationToken cancellationToken)
        {
            var hotels = await _hotelRepository.SearchAsync(request.City, request.Country, request.MinStarRating, request.Status);
            var hoteDtos = _mapper.Map<List<HotelDto>>(hotels);
            return hoteDtos;
        }
    }
}
