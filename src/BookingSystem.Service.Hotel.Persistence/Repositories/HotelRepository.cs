using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Domain.Entities;
using BookingSystem.Service.Hotel.Domain.Enums;
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

        public async Task<(List<Domain.Entities.Hotel> Hotels, int TotalCount)> SearchAsync(string? city, string? country, StarRating? starRating, HotelStatus? hotelStatus, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Hotels
                    .Where(h =>
                        (string.IsNullOrEmpty(city) || h.City.ToLower().Contains(city.ToLower())) &&
                        (string.IsNullOrEmpty(country) || h.Country.ToLower().Contains(country.ToLower())) &&
                        (!starRating.HasValue || h.StarRating >= starRating.Value) &&
                        (!hotelStatus.HasValue || h.Status == hotelStatus.Value)
                    );
            var totalCount = await query.CountAsync();
            var hotels = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (hotels, totalCount);
        }

        public async Task UpdateAsync(Domain.Entities.Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
        }
    }
}
