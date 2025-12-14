using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Queries.SearchHotels;
using BookingSystem.Service.Hotel.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Queries
{
    public class SearchHotelsQueryHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly IMapper _mapperMock;
        private readonly SearchHotelsQueryHandler _searchHotelsQueryHandler;
        public SearchHotelsQueryHandlerTests()
        {
            _hotelRepositoryMock = new Mock<IHotelRepository>();
            _mapperMock = AutoMapperHelper.CreateMapper();
            _searchHotelsQueryHandler = new SearchHotelsQueryHandler(_hotelRepositoryMock.Object, _mapperMock);
        }

        [Fact]
        public async Task Handle_WithoutFilters_ShouldReturnAllHotels()
        {
            //Arrange
            var query = new SearchHotelsQuery
            {
                PageNumber = 1,
                PageSize = 10
            };
            var hotels = new List<Domain.Entities.Hotel>
            {
                new("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA"),
                new("Hotel B", "Description B", Domain.Enums.StarRating.FourStar, "CityB", "CountryB"),
                new("Hotel C", "Description C", Domain.Enums.StarRating.ThreeStar, "CityC", "CountryC")
            };
            EntityHelper.SetId(hotels[0], 1);
            EntityHelper.SetId(hotels[1], 2);
            EntityHelper.SetId(hotels[2], 3);

            _hotelRepositoryMock.Setup(repo => repo.SearchAsync(null, null, null, null, query.PageNumber, query.PageSize))
                .ReturnsAsync((hotels, hotels.Count));
            //Act
            var result = await _searchHotelsQueryHandler.Handle(query, CancellationToken.None);
            //Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(3);
            result.TotalPages.Should().Be(1);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithCityFilter_ShouldReturnFilteredHotels()
        {
            //Arrange
            var query = new SearchHotelsQuery
            {
                City = "CityA",
                PageNumber = 1,
                PageSize = 10
            };
            var hotels = new List<Domain.Entities.Hotel>
            {
                new("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA"),
                new("Hotel D", "Description D", Domain.Enums.StarRating.FourStar, "CityA", "CountryD")
            };
            EntityHelper.SetId(hotels[0], 1);
            EntityHelper.SetId(hotels[1], 4);
            _hotelRepositoryMock.Setup(repo => repo.SearchAsync("CityA", null, null, null, query.PageNumber, query.PageSize))
                .ReturnsAsync((hotels, hotels.Count));
            //Act
            var result = await _searchHotelsQueryHandler.Handle(query, CancellationToken.None);
            //Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data[0].Name.Should().Be("Hotel A");
            result.Data[1].Name.Should().Be("Hotel D");
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(2);
            result.TotalPages.Should().Be(1);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithPagination_ShouldReturnCorrectPage()
        {
            //Arrange
            var query = new SearchHotelsQuery
            {
                PageNumber = 2,
                PageSize = 5
            };
            var hotels = new List<Domain.Entities.Hotel>
            {
                new("Hotel 6", "Description 6", Domain.Enums.StarRating.ThreeStar, "CityF", "CountryF"),
                new("Hotel 7", "Description 7", Domain.Enums.StarRating.FourStar, "CityG", "CountryG"),
                new("Hotel 8", "Description 8", Domain.Enums.StarRating.FiveStar, "CityH", "CountryH"),
            };
            EntityHelper.SetId(hotels[0], 6);
            EntityHelper.SetId(hotels[1], 7);
            EntityHelper.SetId(hotels[2], 8);
            _hotelRepositoryMock.Setup(repo => repo.SearchAsync(null, null, null, null, query.PageNumber, query.PageSize))
                .ReturnsAsync((hotels, 15));
            //Act
            var result = await _searchHotelsQueryHandler.Handle(query, CancellationToken.None);
            //Assert
            result.Should().NotBeNull();
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalRecords.Should().Be(15);
            result.TotalPages.Should().Be(3);
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithMultipleFilters_ShouldPassAllFilters()
        {
            //Arrange
            var query = new SearchHotelsQuery
            {
                City = "CityA",
                Country = "CountryA",
                MinStarRating = Domain.Enums.StarRating.FourStar,
                Status = Domain.Enums.HotelStatus.Active,
                PageNumber = 1,
                PageSize = 10
            };
            var hotels = new List<Domain.Entities.Hotel>
            {
                new("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA"),
            };
            EntityHelper.SetId(hotels[0], 1);
            _hotelRepositoryMock.Setup(repo => repo.SearchAsync("CityA", "CountryA", Domain.Enums.StarRating.FourStar, Domain.Enums.HotelStatus.Active, query.PageNumber, query.PageSize))
                .ReturnsAsync((hotels, 1));
            //Act
            var result = await _searchHotelsQueryHandler.Handle(query, CancellationToken.None);
            //Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data[0].Name.Should().Be("Hotel A");
            result.Data[0].Country.Should().Be("CountryA");
            result.Data[0].City.Should().Be("CityA");
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(1);
            result.TotalPages.Should().Be(1);
        }
        [Fact]
        public async Task Handle_NoResults_ShouldReturnEmptyPagedResponse()
        {
            //Arrange
            var query = new SearchHotelsQuery
            {
                City = "NonExistentCity",
                PageNumber = 1,
                PageSize = 10
            };
            _hotelRepositoryMock.Setup(repo => repo.SearchAsync("NonExistentCity", null, null, null, query.PageNumber, query.PageSize))
                .ReturnsAsync((new List<Domain.Entities.Hotel>(), 0));
            //Act
            var result = await _searchHotelsQueryHandler.Handle(query, CancellationToken.None);
            //Assert
            result.Data.Should().BeEmpty();
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldMapHotelEntitiesToDtos()
        {
            //Arrange
            var query = new SearchHotelsQuery
            {
                PageNumber = 1,
                PageSize = 10
            };
            var hotel = new Domain.Entities.Hotel("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA");
            hotel.UpdateAddress("123 Main St", "CityA", "StateA", "CountryA","12345");
            hotel.UpdateContactInfo("test@hote.com", "+1234567890","http://test.com");
            EntityHelper.SetId(hotel, 1);
            _hotelRepositoryMock.Setup(r=>r.SearchAsync(null, null, null, null, query.PageNumber, query.PageSize))
                .ReturnsAsync((new List<Domain.Entities.Hotel> { hotel }, 1));
            //Act
            var result = await _searchHotelsQueryHandler.Handle(query, CancellationToken.None);
            //Assert
            result.Data.Should().HaveCount(1);
            var hotelDto = result.Data[0];
            hotelDto.Id.Should().Be(1);
            hotelDto.Name.Should().Be("Hotel A");
            hotelDto.Description.Should().Be("Description A");
            hotelDto.City.Should().Be("CityA");
            hotelDto.Country.Should().Be("CountryA");
            hotelDto.StarRating.Should().Be(Domain.Enums.StarRating.FiveStar);
            hotelDto.Email.Should().Be("test@hote.com");
        }
    }
}
