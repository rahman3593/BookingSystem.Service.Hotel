using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.CreateHotel;
using BookingSystem.Service.Hotel.Domain.Enums;
using BookingSystem.Service.Hotel.UnitTests.Helpers;
using Moq;
using FluentAssertions;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Commands
{
    public class CreateHotelCommandHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly CreateHotelCommandHandler _handler;

        public CreateHotelCommandHandlerTests()
        {
            _hotelRepositoryMock = new Mock<IHotelRepository>();
            _handler = new CreateHotelCommandHandler(_hotelRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldReturnHotelId()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Grand Hotel",
                Description = "Luxury hotel in city center",
                Street = "123 Main Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "info@grandhotel.com",
                Phone = "+33123456789",
                Website = "https://grandhotel.com",
                StarRating = StarRating.FiveStar
            };
            var mockHotel = new Domain.Entities.Hotel(command.Name, command.Description, command.StarRating, command.City, command.Country);
            EntityHelper.SetId(mockHotel, 42);

            _hotelRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Hotel>()))
                                .ReturnsAsync(mockHotel);

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert
            _hotelRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Hotel>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCallRepositoryWithCorrectData()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Grand Hotel",
                Description = "Luxury hotel in city center",
                Street = "123 Main Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "info@grandhotel.com",
                Phone = "+33123456789",
                Website = "https://grandhotel.com",
                StarRating = StarRating.FiveStar
            };

            Domain.Entities.Hotel? capturedHotel = null;

            var mockHotel = new Domain.Entities.Hotel(command.Name, command.Description, command.StarRating, command.City, command.Country);
            EntityHelper.SetId(mockHotel, 1);

            _hotelRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Hotel>()))
                                .Callback<Domain.Entities.Hotel>(h => capturedHotel = h)
                                .ReturnsAsync(mockHotel);
            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert
            capturedHotel.Should().NotBeNull();
            capturedHotel.Should().NotBeNull();
            capturedHotel!.Name.Should().Be(command.Name);
            capturedHotel.Description.Should().Be(command.Description);
            capturedHotel.Street.Should().Be(command.Street);
            capturedHotel.City.Should().Be(command.City);
            capturedHotel.State.Should().Be(command.State);
            capturedHotel.Country.Should().Be(command.Country);
            capturedHotel.ZipCode.Should().Be(command.ZipCode);
            capturedHotel.Email.Should().Be(command.Email);
            capturedHotel.Phone.Should().Be(command.Phone);
            capturedHotel.Website.Should().Be(command.Website);
            capturedHotel.StarRating.Should().Be(command.StarRating);
            capturedHotel.Status.Should().Be(HotelStatus.Active);

        }
    }
}
