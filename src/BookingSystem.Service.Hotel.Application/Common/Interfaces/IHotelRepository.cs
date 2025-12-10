using BookingSystem.Service.Hotel.Domain.Entities;

namespace BookingSystem.Service.Hotel.Application.Common.Interfaces
{
    public interface IHotelRepository
    {
        Task<Domain.Entities.Hotel?> GetByIdAsync(int hotelId);
        Task<List<Domain.Entities.Hotel>> GetAllAsync();
        Task<Domain.Entities.Hotel> AddAsync(Domain.Entities.Hotel hotel);
        Task UpdateAsync(Domain.Entities.Hotel hotel);
        Task DeleteAsync(int id);
        Task<(List<Domain.Entities.Hotel> Hotels,int TotalCount)> SearchAsync(string? city, string? country, Domain.Enums.StarRating? starRating, Domain.Enums.HotelStatus? hotelStatus, int pageNumber = 1, int pageSize = 10);
    }
}
