using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.GetHotelsList;
using BookingSystem.Service.Hotel.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Queries
{
    public class GetHotelsListQueryHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly IMapper _mapperMock;
        private readonly GetHotelsListQueryHandler _handler;
        public GetHotelsListQueryHandlerTests()
        {
            _hotelRepositoryMock = new Mock<IHotelRepository>();
            _mapperMock = AutoMapperHelper.CreateMapper();
            _handler = new GetHotelsListQueryHandler(_hotelRepositoryMock.Object, _mapperMock);
        }

        [Fact]
        public async Task Handle_WithHotels_ShouldReturnListOfHotelDtos()
        {
            //Arrange
            var query = new GetHotelsListQuery();
            var hotels = new List<Domain.Entities.Hotel>
            {
                new("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA"),
                new("Hotel B", "Description B", Domain.Enums.StarRating.FourStar, "CityB", "CountryB"),
                new("Hotel C", "Description C", Domain.Enums.StarRating.ThreeStar, "CityC", "CountryC")
            };
            _hotelRepositoryMock.Setup(x=>x.GetAllAsync())
                .ReturnsAsync(hotels);
            var expectedHotelDtos = _mapperMock.Map<List<BookingSystem.Service.Hotel.Application.DTOs.HotelDto>>(hotels);
            //Act
            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert
            _hotelRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedHotelDtos);
        }

        [Fact]
        public async Task Handle_NoHotels_ShouldReturnEmptyList()
        {
            //Arrange
            var query = new GetHotelsListQuery();
            var hotels = new List<Domain.Entities.Hotel>();
            _hotelRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(hotels);
            //Act
            var result = await _handler.Handle(query, CancellationToken.None);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
