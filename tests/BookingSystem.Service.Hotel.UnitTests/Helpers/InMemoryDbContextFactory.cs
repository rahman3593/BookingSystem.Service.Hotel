using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Service.Hotel.UnitTests.Helpers
{
    public static class InMemoryDbContextFactory
    {
        public static HotelDbContext Create()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new HotelDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
