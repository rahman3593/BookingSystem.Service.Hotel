using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Service.Hotel.Domain.Exceptions
{
    public class RoomTypeNotFoundException : DomainException
    {
        public RoomTypeNotFoundException(int roomTypeId)
            : base($"Room type with ID {roomTypeId} was not found.")
        {
        }
        public RoomTypeNotFoundException(string message) : base(message)
        {
        }
    }
}
