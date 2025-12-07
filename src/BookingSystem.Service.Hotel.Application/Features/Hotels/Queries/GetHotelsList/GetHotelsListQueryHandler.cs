using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.DTOs;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelsList
{
    public class GetHotelsListQueryHandler : IRequestHandler<GetHotelsListQuery, List<DTOs.HotelDto>>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public GetHotelsListQueryHandler(IHotelRepository hotelRepository,IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }
        public async Task<List<HotelDto>> Handle(GetHotelsListQuery request, CancellationToken cancellationToken)
        {
            var hotels = await _hotelRepository.GetAllAsync();

            return _mapper.Map<List<HotelDto>>(hotels);
        }
    }
}
