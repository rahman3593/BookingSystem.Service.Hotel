using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.DTOs;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelsList
{
    public class GetHotelsListQuery: IRequest<List<HotelDto>>
    {
    }
}
