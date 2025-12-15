using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.UpdateHotel;
using BookingSystem.Service.Hotel.Domain.Enums;
using FluentAssertions;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Commands
{
    public class UpdateHotelCommandValidatorTests
    {

        private readonly UpdateHotelCommandValidator _validator;

        public UpdateHotelCommandValidatorTests()
        {
            _validator = new UpdateHotelCommandValidator();
        }

        private UpdateHotelCommand CreateValidCommand()
        {
            return new UpdateHotelCommand
            {
                Id = 1,
                Name = "Test Hotel",
                Description = "Test Description",
                City = "Paris",
                Country = "France",
                Street = "123 Main St",
                State = "Ile-de-France",
                ZipCode = "75001",
                Email = "test@hotel.com",
                Phone = "+33123456789",
                Website = "https://test.com",
                StarRating = StarRating.FourStar,
                Status = HotelStatus.Active
            };
        }

        [Fact]
        public async Task Validate_AllFieldsValid_ShouldPass()
        {
            //Arrange
            var updateHotelCommand = CreateValidCommand();

            //Act
            var result = await _validator.ValidateAsync(updateHotelCommand);

            //Assert
            result.Errors.Should().BeEmpty();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_OnlyRequiredFields_ShouldPass()
        {
            //Arrange
            var updateHotelCommand = CreateValidCommand();
            updateHotelCommand.Description = string.Empty;
            updateHotelCommand.Street = string.Empty;
            updateHotelCommand.State = string.Empty;
            updateHotelCommand.ZipCode = string.Empty;
            updateHotelCommand.Email = string.Empty;
            updateHotelCommand.Phone = string.Empty;
            updateHotelCommand.Website = string.Empty;

            //Act
            var result = await _validator.ValidateAsync(updateHotelCommand);

            //Assert
            result.Errors.Should().BeEmpty();
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_IdNotGreaterThanZero_ShouldFail(int invalidID)
        {
            //Arrange
            var updateHotelCommand = CreateValidCommand();
            updateHotelCommand.Id = invalidID;
            //Act
            var result = await _validator.ValidateAsync(updateHotelCommand);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(r => r.PropertyName == "Id" && r.ErrorMessage == "Invalid hotel ID.");
        }

        [Theory]
        [InlineData("Name", "Hotel name is required.")]
        [InlineData("City", "City is required")]
        [InlineData("Country", "Country is required")]
        public async Task Validate_RequiredFieldEmpty_ShouldFail(string fieldName, string expectedError)
        {
            //Arrange
            var command = CreateValidCommand();
            typeof(UpdateHotelCommand).GetProperty(fieldName)?.SetValue(command, string.Empty);
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(r => r.PropertyName == fieldName && r.ErrorMessage == expectedError);
        }

        [Theory]
        [InlineData("Name", 200, "Hotel name must not exceed 200 characters.")]
        [InlineData("Description", 1000, "Description must not exceed 1000 characters.")]
        [InlineData("City", 100, "City name cannot exceed 100 characters")]
        [InlineData("Country", 100, "Country name cannot exceed 100 characters")]
        [InlineData("Phone", 20, "Phone must not exceed 20 characters")]
        [InlineData("Website", 200, "Website must not exceeed 200 characters")]
        [InlineData("Street", 100, "Street must not exceed 100 characters")]
        [InlineData("State", 100, "State must not exceed 100 characters")]
        [InlineData("ZipCode", 20, "Zipcode must not exceed 20 characters")]

        public async Task Validate_MaxLength100Exceeded_ShouldFail(string fieldName, int maxLength, string expectedError)
        {
            //Arrange
            var command = CreateValidCommand();
            typeof(UpdateHotelCommand).GetProperty(fieldName)?.SetValue(command, new string('A', maxLength + 1));
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(r => r.PropertyName == fieldName && r.ErrorMessage == expectedError);

        }

        [Fact]
        public async Task Validate_InvalidEmailFormat_ShouldFail()
        {
            //Arrange
            var command = CreateValidCommand();
            command.Email = "test";
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(r => r.PropertyName == "Email" && r.ErrorMessage == "Invalid email format");
        }

        [Fact]
        public async Task Validate_EmptyEmail_ShouldPass()
        {
            //Arrange
            var command = CreateValidCommand();
            command.Email = string.Empty;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().BeEmpty();
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("StarRating", 99, "Star rating must be between 1 and 5")]
        [InlineData("Status", 99, "Status must be Active, Inactive, or UnderMaintenance")]
        public async Task Validate_InvalidEnumValue_ShouldFail(string fieldName, int enumValue, string expectedError)
        {
            //Arrange
            var command = CreateValidCommand();
            typeof(UpdateHotelCommand).GetProperty(fieldName)?.SetValue(command, enumValue);
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(r => r.PropertyName == fieldName && r.ErrorMessage == expectedError);
        }
    }
}
