using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Models;
using BookingSystem.Service.Hotel.Application.DTOs;
using BookingSystem.Service.Hotel.Domain.Enums;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.SearchHotels
{
    public class SearchHotelsQuery : IRequest<PagedResponse<HotelDto>>
    {
        public string? City { get; set; }
        public string? Country { get; set; }
        public StarRating? MinStarRating { get; set; }
        public HotelStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
