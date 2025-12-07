using BookingSystem.Service.Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Service.Hotel.Persistence.Contexts
{
    public class HotelDbContext:DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options): base(options)
        {

        }

        public DbSet<Domain.Entities.Hotel> Hotels { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
        }
    }
}
