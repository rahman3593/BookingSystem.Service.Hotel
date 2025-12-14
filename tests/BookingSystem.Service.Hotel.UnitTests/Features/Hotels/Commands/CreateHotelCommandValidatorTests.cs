using BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.CreateHotel;
using BookingSystem.Service.Hotel.Domain.Enums;
using FluentAssertions;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Commands
{
    public class CreateHotelCommandValidatorTests
    {
        private readonly CreateHotelCommandValidator _validator;

        public CreateHotelCommandValidatorTests()
        {
            _validator = new CreateHotelCommandValidator();
        }

        [Fact]
        public async Task validate_ValidCommand_shouldNotHaveValidationErrors()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task validate_EmptyName_ShouldHaveValidationError()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Name" && e.ErrorMessage == "Hotel name is required.");
        }

        [Fact]
        public async Task validate_NameTooLong_ShouldHaveValidationError()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = new string('A', 201),
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Name" && e.ErrorMessage == "Hotel name must not exceed 200 characters.");
        }

        [Fact]
        public async Task validate_DescriptionTooLong_ShouldHaveValidationError()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = new string('A', 1001),
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Description" && e.ErrorMessage == "Description must not exceed 1000 characters.");
        }

        [Fact]
        public async Task validate_EmptyCity_ShouldHaveValidationError()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = new string('A', 201),
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "City" && e.ErrorMessage == "City is required");
        }

        [Fact]
        public async Task validate_CityTooLong_shouldNotHaveValidationErrors()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = new string('A',101),
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "City" && e.ErrorMessage == "City name cannot exceed 100 characters");
        }

        [Fact]
        public async Task validate_CountryTooLong_shouldNotHaveValidationErrors()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = new string('A', 101),
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Country" && e.ErrorMessage == "Country name cannot exceed 100 characters");
        }


        [Fact]
        public async Task validate_InvalidEmail_ShouldHaveValidationError()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "invalid-email-format",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format");
        }

        [Fact]
        public async Task validate_PhoneTooLong_shouldNotHaveValidationErrors()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = new string('A', 21),
                Website = "https://testhotel.com",
                StarRating = StarRating.FourStar
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone number cannot exceed 20 characters");
        }


        [Fact]
        public async Task validate_WebsiteTooLong_shouldNotHaveValidationErrors()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://" + new string('a', 195) + ".com",
                StarRating = StarRating.FourStar
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Website" && e.ErrorMessage == "Website URL cannot exceed 200 characters");
        }

        [Fact]
        public async Task validate_InvalidStarRating_ShouldHaveValidationError()
        {
            //Arrange
            var command = new CreateHotelCommand
            {
                Name = "Test Hotel",
                Description = "A test hotel description",
                Street = "123 Test Street",
                City = "Paris",
                State = "Ile-de-France",
                Country = "France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://testhotel.com",
                StarRating = (StarRating)99
            };
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "StarRating" && e.ErrorMessage == "Star rating must be between 1 and 5");
        }
    }
}
