using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Domain.Entities;
using BookingSystem.Service.Hotel.Domain.Exceptions;
using BookingSystem.Service.Hotel.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Service.Hotel.Persistence.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly HotelDbContext _context;

        public HotelRepository(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Hotel> AddAsync(Domain.Entities.Hotel hotel)
        {
            await _context.AddAsync(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }

        public async Task DeleteAsync(int hotelId)
        {
            var hotel = await GetByIdAsync(hotelId) ?? throw new HotelNotFoundException(hotelId);
            hotel.MarkAsDeleted();
            await _context.SaveChangesAsync();
        }

        public async Task<List<Domain.Entities.Hotel>> GetAllAsync()
        {
            return await _context.Hotels.ToListAsync();
        }

        public async Task<Domain.Entities.Hotel?> GetByIdAsync(int hotelId)
        {
            return await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId);
        }

        public async Task UpdateAsync(Domain.Entities.Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
        }
    }
}
