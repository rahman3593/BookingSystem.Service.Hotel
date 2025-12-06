using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelById
{
    public class GetHotelByIdQuery:IRequest<DTOs.HotelDto>
    {
        public int HotelId { get; set; }
    }
}
