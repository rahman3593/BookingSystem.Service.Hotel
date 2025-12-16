using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.RoomTypes.Commands
{
    public class CreateRoomTypeCommand : IRequest<int>
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxOccupancy { get; set; }
        public decimal BasePrice { get; set; }
        public decimal Size { get; set; }
        public string BedType { get; set; } = string.Empty;
        public string ViewType { get; set; } = string.Empty;
        public bool HasBalcony { get; set; }
        public bool HasKitchen { get; set; }
        public bool IsSmokingAllowed { get; set; }

    }
}
