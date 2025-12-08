using BookingSystem.Service.Hotel.Application.DTOs;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.CreateHotel;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelById;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelsList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Service.Hotel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HotelsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<HotelDto>>> GetAllHotels()
        {
            var query = new GetHotelsListQuery();
            var hotels = await _mediator.Send(query);
            return Ok(hotels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HotelDto>> GetHotelById(int id)
        {
            var query = new GetHotelByIdQuery { HotelId = id };
            var hotel = await _mediator.Send(query);
            if(hotel == null)
            {
                return NotFound($"Hotel with ID {id} not found");
            }
            return Ok(hotel);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
        {
            var hotelId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, hotelId);
        }

    }
}
