using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Service.Hotel.Domain.Entities
{
    public class RoomType : BaseEntity
    {
        // Foreign key to the Hotel entity
        public int HotelId { get; private set; }

        //Basic Properties
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int MaxOccupancy { get; private set; }
        public decimal BasePrice { get; private set; }
        public decimal Size { get; private set; } // in square meters

        //Room Features
        public string BedType { get; private set; }
        public string ViewType { get; private set; }
        public bool HasBalcony { get; private set; }
        public bool HasKitchen { get; private set; }
        public bool IsSmokingAllowed { get; private set; }

        private RoomType() { }

        public RoomType(
        int hotelId,
        string name,
        string description,
        int maxOccupancy,
        decimal basePrice,
        decimal size,
        string bedType,
        string viewType,
        bool hasBalcony = false,
        bool hasKitchen = false,
        bool isSmokingAllowed = false)
        {
            if (hotelId <= 0)
                throw new ArgumentException("Hotel ID is required", nameof(hotelId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Room type name is required", nameof(name));

            if (maxOccupancy <= 0)
                throw new ArgumentException("Max occupancy must be greater than zero", nameof(maxOccupancy));

            if (basePrice < 0)
                throw new ArgumentException("Base price cannot be negative", nameof(basePrice));

            if (size < 0)
                throw new ArgumentException("Size cannot be negative", nameof(size));

            HotelId = hotelId;
            Name = name;
            Description = description ?? string.Empty;
            MaxOccupancy = maxOccupancy;
            BasePrice = basePrice;
            Size = size;
            BedType = bedType ?? string.Empty;
            ViewType = viewType ?? string.Empty;
            HasBalcony = hasBalcony;
            HasKitchen = hasKitchen;
            IsSmokingAllowed = isSmokingAllowed;
        }

        // Business Methods
        public void UpdateDetails(
            string name,
            string description,
            int maxOccupancy,
            decimal basePrice)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Room type name is required", nameof(name));

            if (maxOccupancy <= 0)
                throw new ArgumentException("Max occupancy must be greater than zero", nameof(maxOccupancy));

            if (basePrice < 0)
                throw new ArgumentException("Base price cannot be negative", nameof(basePrice));

            Name = name;
            Description = description;
            MaxOccupancy = maxOccupancy;
            BasePrice = basePrice;
            UpdateTimestamp();
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative", nameof(newPrice));

            BasePrice = newPrice;
            UpdateTimestamp();
        }

        public void UpdateFeatures(
            string bedType,
            string viewType,
            bool hasBalcony,
            bool hasKitchen,
            bool isSmokingAllowed)
        {
            BedType = bedType ?? string.Empty;
            ViewType = viewType ?? string.Empty;
            HasBalcony = hasBalcony;
            HasKitchen = hasKitchen;
            IsSmokingAllowed = isSmokingAllowed;
            UpdateTimestamp();
        }
    }
}
