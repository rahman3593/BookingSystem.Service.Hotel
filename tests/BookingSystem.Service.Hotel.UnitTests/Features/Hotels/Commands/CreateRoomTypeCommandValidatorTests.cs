using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Features.RoomTypes.Commands;
using FluentAssertions;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Commands
{

    public class CreateRoomTypeCommandValidatorTests
    {
        private readonly CreateRoomTypeCommandValidator _validator;
        public CreateRoomTypeCommandValidatorTests()
        {
            _validator = new CreateRoomTypeCommandValidator();
        }

        [Fact]
        public async Task Validate_AllFieldsValid_ShouldPass()
        {
            //Arrange
            var command = CreateValidCommand();
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().BeEmpty();
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_HotelIdLessThanOrEqualToZero_ShouldHaveValidationError(int hotelId)
        {
            //Arrange
            var command = CreateValidCommand();
            command.HotelId = hotelId;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(command.HotelId) && x.ErrorMessage == "Invalid hotel ID");
        }

        [Fact]
        public async Task Validate_EmptyOrWhitespaceName_ShouldFailValidation()
        {
            //Arrange
            var command = CreateValidCommand();
            command.Name = string.Empty;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(command.Name) && x.ErrorMessage == "Room type name is required");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_NonPositiveMaxOccupancy_ShouldFailValidation(int maxOccupancy)
        {
            //Arrange
            var command = CreateValidCommand();
            command.MaxOccupancy = maxOccupancy;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(command.MaxOccupancy) && x.ErrorMessage == "Max occupancy must be greater than 0");
        }

        [Fact]
        public async Task Validate_BasePriceLessThanZero_ShouldFailValidation()
        {
            //Arrange
            var command = CreateValidCommand();
            command.BasePrice = -10.0m;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(command.BasePrice) && x.ErrorMessage == "Base price cannot be negative");
        }

        [Fact]
        public async Task Validate_Size_WhenNegative_ReturnsValidationError()
        {
            //Arrange
            var command = CreateValidCommand();
            command.Size = -5.0m;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(command.Size) && x.ErrorMessage == "Room size must be greater than 0");
        }

        [Fact]
        public async Task Validation_Passes_When_ViewType_And_BedType_AreEmpty()
        {
            //Arrange
            var command = CreateValidCommand();
            command.Description = string.Empty;
            command.ViewType = string.Empty;
            command.BedType = string.Empty;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().BeEmpty();
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("Name", 100, "Room type name must not exceed 100 characters")]
        [InlineData("Description", 1000, "Description must not exceed 1000 characters")]
        [InlineData("BedType", 50, "Bed type must not exceed 50 characters")]
        [InlineData("ViewType", 50, "View type must not exceed 50 characters")]
        public async Task Validate_For_maxLenght(string field, int maxLength, string errorMessage)
        {
            //Arrange
            var command = CreateValidCommand();
            typeof(CreateRoomTypeCommand).GetProperty(field)?.SetValue(command, new string('A', maxLength+1));
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(x => x.PropertyName == field && x.ErrorMessage == errorMessage);
        }

        private CreateRoomTypeCommand CreateValidCommand()
        {
            return new CreateRoomTypeCommand
            {
                HotelId = 1,
                Name = "Deluxe Suite",
                Description = "A luxurious suite with ocean view.",
                MaxOccupancy = 4,
                BasePrice = 299.99m,
                Size = 45.5m,
                BedType = "King",
                ViewType = "Ocean",
                HasBalcony = true,
                HasKitchen = false,
                IsSmokingAllowed = false
            };
        }
    }


}
