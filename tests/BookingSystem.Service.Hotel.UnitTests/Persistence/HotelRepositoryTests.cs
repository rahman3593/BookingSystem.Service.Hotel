using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Domain.Enums;
using BookingSystem.Service.Hotel.Persistence.Repositories;
using BookingSystem.Service.Hotel.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Service.Hotel.UnitTests.Persistence
{
    public class HotelRepositoryTests
    {
        [Fact]
        public async Task AddAsync_ShouldAddHotelToDatabase()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel = new Domain.Entities.Hotel("Test Hotel", "A test hotel", Domain.Enums.StarRating.FourStar, "Test City", "Test Country");
            //Act
            var result = await repository.AddAsync(hotel);
            //Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Test Hotel");

            var hotelInDb = await context.Hotels.FindAsync(result.Id);
            hotelInDb.Should().NotBeNull();
            hotelInDb!.Name.Should().Be("Test Hotel");
        }

        [Fact]
        public async Task GetByIdAsync_ExistingHotel_ShouldReturnHotel()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel = new Domain.Entities.Hotel("Test Hotel", "A test hotel", Domain.Enums.StarRating.FourStar, "Test City", "Test Country");
            var addedHotel = await repository.AddAsync(hotel);

            //Act
            var result = await repository.GetByIdAsync(addedHotel.Id);
            //Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(addedHotel.Id);
            result.Name.Should().Be("Test Hotel");
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentHotel_ShouldReturnNull()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            //Act
            var result = await repository.GetByIdAsync(999);
            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllHotels()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel1 = new Domain.Entities.Hotel("Hotel One", "First hotel", Domain.Enums.StarRating.ThreeStar, "CityA", "CountryA");
            var hotel2 = new Domain.Entities.Hotel("Hotel Two", "Second hotel", Domain.Enums.StarRating.FiveStar, "CityB", "CountryB");
            await repository.AddAsync(hotel1);
            await repository.AddAsync(hotel2);
            //Act
            var result = await repository.GetAllAsync();
            //Assert
            result.Should().HaveCount(2);
            result.Select(h => h.Name).Should().Contain(new[] { "Hotel One", "Hotel Two" });
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateHotelInDatabase()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel = new Domain.Entities.Hotel("Old Name", "Old Description", Domain.Enums.StarRating.TwoStar, "Old City", "Old Country");
            var addedHotel = await repository.AddAsync(hotel);
            addedHotel.UpdateDetails("New Name", "New Description", Domain.Enums.StarRating.FourStar);
            //Act
            await repository.UpdateAsync(addedHotel);
            //Assert
            var updatedHotel = await repository.GetByIdAsync(addedHotel.Id);
            updatedHotel.Should().NotBeNull();
            updatedHotel!.Name.Should().Be("New Name");
            updatedHotel.Description.Should().Be("New Description");
            updatedHotel.StarRating.Should().Be(Domain.Enums.StarRating.FourStar);
        }

        [Fact]
        public async Task DeleteAsync_ShouldMarkHotelAsDeleted()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel = new Domain.Entities.Hotel("To Be Deleted", "This hotel will be deleted", Domain.Enums.StarRating.ThreeStar, "CityX", "CountryY");
            var addedHotel = await repository.AddAsync(hotel);
            //Act
            await repository.DeleteAsync(addedHotel.Id);
            //Assert
            var deletedHotel = await context.Hotels.IgnoreQueryFilters().FirstOrDefaultAsync(h => h.Id == addedHotel.Id);
            deletedHotel.Should().NotBeNull();
            deletedHotel!.IsDeleted.Should().BeTrue();

            var shouldBeNull = await repository.GetByIdAsync(addedHotel.Id);
            shouldBeNull.Should().BeNull();

        }
        [Fact]
        public async Task SearchAsync_WithFilters_ShouldReturnFilteredHotels()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel1 = new Domain.Entities.Hotel("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA");
            var hotel2 = new Domain.Entities.Hotel("Hotel B", "Description B", Domain.Enums.StarRating.FourStar, "CityB", "CountryB");
            var hotel3 = new Domain.Entities.Hotel("Hotel C", "Description C", Domain.Enums.StarRating.ThreeStar, "CityA", "CountryA");
            await repository.AddAsync(hotel1);
            await repository.AddAsync(hotel2);
            await repository.AddAsync(hotel3);
            //Act
            var (hotels, totalCount) = await repository.SearchAsync("CityA", null, null, null, 1, 10);
            //Assert
            totalCount.Should().Be(2);
            hotels.Should().HaveCount(2);
            hotels.Select(x => x.Name).Should().Contain(new[] { "Hotel A", "Hotel C" });
        }

        [Fact]
        public async Task SearchAsync_CaseInsensitive_ShouldReturnMatchingHotels()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel = new Domain.Entities.Hotel("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA");
            await repository.AddAsync(hotel);
            //Act
            var (hotels, totalCount) = await repository.SearchAsync("citya", null, null, null, 1, 10);
            //Assert
            hotels.Should().HaveCount(1);
            totalCount.Should().Be(1);
        }

        [Fact]
        public async Task SearchAsync_WithMinStarRating_ShouldReturnHotelsWithEqualOrHigherRating()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel1 = new Domain.Entities.Hotel("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA");
            var hotel2 = new Domain.Entities.Hotel("Hotel B", "Description B", Domain.Enums.StarRating.FourStar, "CityB", "CountryB");
            await repository.AddAsync(hotel1);
            await repository.AddAsync(hotel2);
            //Act
            var (hotels, totalCount) = await repository.SearchAsync(null, null, Domain.Enums.StarRating.FourStar, null, 1, 10);
            //Assert
            totalCount.Should().Be(2);
            hotels.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchAsync_WithHotelStatus_ShouldReturnHotelsWithSpecifiedStatus()
        {
            //Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);
            var hotel1 = new Domain.Entities.Hotel("Hotel A", "Description A", Domain.Enums.StarRating.FiveStar, "CityA", "CountryA");
            var hotel2 = new Domain.Entities.Hotel("Hotel B", "Description B", Domain.Enums.StarRating.FourStar, "CityB", "CountryB");
            await repository.AddAsync(hotel1);
            await repository.AddAsync(hotel2);
            await repository.DeleteAsync(hotel2.Id);
            //Act
            var (hotels, totalCount) = await repository.SearchAsync(null, null, null, Domain.Enums.HotelStatus.Active, 1, 10);
            //Assert
            totalCount.Should().Be(1);
            hotels.Should().HaveCount(1);
            hotels.First().Name.Should().Be("Hotel A");
        }

        [Fact]
        public async Task SearchAsync_Pagination_ShouldReturnCorrectPage()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);

            // Add 10 hotels
            for (int i = 1; i <= 10; i++)
            {
                await repository.AddAsync(new Domain.Entities.Hotel($"Hotel {i}", "Desc", StarRating.FourStar, "City", "Country"));
            }

            // Act
            var (hotels, totalCount) = await repository.SearchAsync(null, null, null, null, pageNumber: 2, pageSize: 3);

            // Assert
            hotels.Should().HaveCount(3);
            totalCount.Should().Be(10);
            hotels[0].Name.Should().Be("Hotel 4");
        }

        [Fact]
        public async Task SearchAsync_WithMultipleFilters_ShouldReturnMatchingHotels()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);

            await repository.AddAsync(new Domain.Entities.Hotel("Luxury Paris Hotel", "Desc", StarRating.FiveStar, "Paris", "France"));
            await repository.AddAsync(new Domain.Entities.Hotel("Budget Paris Hotel", "Desc", StarRating.TwoStar, "Paris", "France"));
            await repository.AddAsync(new Domain.Entities.Hotel("Luxury London Hotel", "Desc", StarRating.FiveStar, "London", "UK"));

            // Act
            var (hotels, totalCount) = await repository.SearchAsync(
                 "Paris",
                 "France",
                StarRating.FourStar, null
            );

            // Assert
            hotels.Should().HaveCount(1);
            totalCount.Should().Be(1);
            hotels[0].Name.Should().Be("Luxury Paris Hotel");
        }

        [Fact]
        public async Task SearchAsync_NoMatches_ShouldReturnEmptyList()
        {
            // Arrange
            using var context = InMemoryDbContextFactory.Create();
            var repository = new HotelRepository(context);

            await repository.AddAsync(new Domain.Entities.Hotel("Hotel", "Desc", StarRating.FiveStar, "Paris", "France"));

            // Act
            var (hotels, totalCount) = await repository.SearchAsync("NonExistentCity", null, null, null);

            // Assert
            hotels.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }
}