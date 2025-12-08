using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.DeleteHotel
{
    public class DeleteHotelCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }
}
