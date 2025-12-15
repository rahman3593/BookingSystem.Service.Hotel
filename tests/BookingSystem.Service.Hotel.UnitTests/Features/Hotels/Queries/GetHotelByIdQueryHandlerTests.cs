using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelById;
using BookingSystem.Service.Hotel.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Queries
{
    public class GetHotelByIdQueryHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelReposRepositoryMock;
        private readonly IMapper _mapperMock;
        private readonly GetHotelByIdQueryHandler _handler;

        public GetHotelByIdQueryHandlerTests()
        {
            _hotelReposRepositoryMock = new Mock<IHotelRepository>();
            _mapperMock = AutoMapperHelper.CreateMapper();
            _handler = new GetHotelByIdQueryHandler(_hotelReposRepositoryMock.Object, _mapperMock);
        }



        [Fact]
        public async Task Handle_ExistingHotel_ShouldReturnHotelDto()
        {
            //Arrange
            var query = new GetHotelByIdQuery() { HotelId = 1 };
            Domain.Entities.Hotel hotel = new Domain.Entities.Hotel("Test Hotel", "Test Description", Domain.Enums.StarRating.FiveStar, "Test City", "Test Country");
            hotel.UpdateAddress("123 Test St", "Test City", "Test State", "Test Country", "12345");
            hotel.UpdateContactInfo("test@hotel.com", "+1234567890", "http://testhotel.com");
            _hotelReposRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(hotel);
            //Act
            var hotelDto = await _handler.Handle(query, CancellationToken.None);
            //Assert
            _hotelReposRepositoryMock.Verify(repo => repo.GetByIdAsync(It.Is<int>(id => id == query.HotelId)), Times.Once);
            hotelDto.Should().NotBeNull();
            hotelDto.Name.Should().Be("Test Hotel");
            hotelDto.Description.Should().Be("Test Description");
            hotelDto.Street.Should().Be("123 Test St");
            hotelDto.City.Should().Be("Test City");
            hotelDto.State.Should().Be("Test State");
            hotelDto.Country.Should().Be("Test Country");
            hotelDto.ZipCode.Should().Be("12345");
            hotelDto.Email.Should().Be("test@hotel.com");
            hotelDto.Phone.Should().Be("+1234567890");
            hotelDto.Website.Should().Be("http://testhotel.com");
            hotelDto.StarRating.Should().Be(Domain.Enums.StarRating.FiveStar);
            hotelDto.Status.Should().Be(Domain.Enums.HotelStatus.Active.ToString());
        }
        [Fact]
        public async Task Handle_NonExistentHotel_ShouldThrowHotelNotFoundException()
        {
            //Arrange
            var query = new GetHotelByIdQuery() { HotelId = 999 };
            _hotelReposRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Domain.Entities.Hotel?)null);

            //Act & Assert
            await Assert.ThrowsAsync<BookingSystem.Service.Hotel.Domain.Exceptions.HotelNotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }
    }
}
