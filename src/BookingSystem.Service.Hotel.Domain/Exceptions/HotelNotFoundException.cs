using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Service.Hotel.Domain.Exceptions
{
    public class HotelNotFoundException : DomainException
    {
        public HotelNotFoundException(int hotelId)
            : base($"Hotel with ID {hotelId} was not found.")
        {
        }
        public HotelNotFoundException(string message) : base(message)
        {
        }
    }
}
