using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookingSystem.Service.Hotel.Application.Common.Mappings;

namespace BookingSystem.Service.Hotel.UnitTests.Helpers
{
    public static class AutoMapperHelper
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
             {
                 cfg.AddProfile<MappingProfile>();
             });
            return config.CreateMapper();
        }
    }
}
