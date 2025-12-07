using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Persistence.Contexts;
using BookingSystem.Service.Hotel.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Service.Hotel.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<HotelDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("HotelDatabase"),  b => b.MigrationsAssembly(typeof(HotelDbContext).Assembly.FullName));
            });

            services.AddScoped<IHotelRepository, HotelRepository>();
            return services;
        }
    }
}
