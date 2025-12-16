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

        //[Fact]
        //public async Task Validate_AllFieldsValid_ShouldPass()
        //{
        //    //Arrange
        //    var command = CreateValidCommand();
        //    //Act
        //    var result = await _validator.ValidateAsync(command);
        //    //Assert
        //    result.Errors.Should().BeEmpty();
        //    result.IsValid.Should().BeTrue();
        //}

        [Fact]
        public async Task Validate_HotelIdLessThanOrEqualToZero_ShouldHaveValidationError()
        {
            //Arrange
            var command = CreateValidCommand();
            command.HotelId = 0;
            //Act
            var result = await _validator.ValidateAsync(command);
            //Assert
            result.Errors.Should().NotBeEmpty();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(command.HotelId) && x.ErrorMessage == "Invalid hotel ID");
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
