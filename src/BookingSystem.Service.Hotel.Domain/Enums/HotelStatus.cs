using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BookingSystem.Service.Hotel.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HotelStatus
    {
        Active = 1,
        Inactive = 2,
        UnderMaintenance = 3,
    }
}
