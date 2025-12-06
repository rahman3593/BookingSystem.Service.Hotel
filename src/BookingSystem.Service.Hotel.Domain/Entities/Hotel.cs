using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Domain.Enums;

namespace BookingSystem.Service.Hotel.Domain.Entities
{
    public class Hotel : BaseEntity
    {
        // Basic Properties
        public string Name { get; private set; }
        public string Description { get; private set; }
        public StarRating StarRating { get; private set; }
        public HotelStatus Status { get; private set; }

        // Address Properties (simplified - no value object)
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string ZipCode { get; private set; }

        // Contact Properties (simplified - no value object)
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Website { get; private set; }

        
        private Hotel() { }

        public Hotel(string name,string description, StarRating starRating, string city, string country)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Hotel name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required", nameof(country));

            Name = name;
            Description = description?? string.Empty;
            StarRating = starRating;
            this.City = city;
            this.Country = country;
            Status = HotelStatus.Active; 

            Street = string.Empty;
            State = string.Empty;
            ZipCode = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Website = string.Empty;

        }


        public void UpdateDetails(string name, string description, StarRating starRating)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Hotel name is required", nameof(name));

            Name = name;
            Description = description;
            StarRating = starRating;
            UpdateTimestamp();
        }

        public void UpdateAddress(string street, string city, string state, string country, string zipCode)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required", nameof(country));

            Street = street ?? string.Empty;
            City = city;
            State = state ?? string.Empty;
            Country = country;
            ZipCode = zipCode ?? string.Empty;
            UpdateTimestamp();
        }

        public void UpdateContactInfo(string email, string phone, string website)
        {
            Email = email ?? string.Empty;
            Phone = phone ?? string.Empty;
            Website = website ?? string.Empty;
            UpdateTimestamp();
        }

        public void ChangeStatus(HotelStatus status)
        {
            Status = status;
            UpdateTimestamp();
        }

    }
}
