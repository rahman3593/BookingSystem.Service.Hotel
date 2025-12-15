using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.UpdateHotel;
using BookingSystem.Service.Hotel.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Commands
{
    public class UpdateHotelCommandHandlerTests
    {
        private readonly Mock<IHotelRepository> _mockHotelRepository;
        private readonly UpdateHotelCommandHandler _handler;

        public UpdateHotelCommandHandlerTests()
        {
            _mockHotelRepository = new Mock<IHotelRepository>();
            _handler = new UpdateHotelCommandHandler(_mockHotelRepository.Object);
        }

        private UpdateHotelCommand CreateValidCommand()
        {
            return new UpdateHotelCommand
            {
                Id = 1,
                Name = "Updated Hotel Name",
                Description = "Updated Description",
                StarRating = StarRating.FourStar,
                Status = HotelStatus.Active,
                Street = "123 Updated St",
                City = "Paris",
                State = "IL",
                Country = "USA",
                ZipCode = "60007",
                Email = "test@hotel.com",
                Phone = "123-456-7890",
                Website = "www.updatedhotel.com"
            };
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldUpdateHotel()
        {
            //Arrange
            var command = CreateValidCommand();
            Domain.Entities.Hotel hotel = new Domain.Entities.Hotel(command.Name, command.Description, command.StarRating, command.City, command.Country);
            _mockHotelRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(hotel);
            _mockHotelRepository.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Hotel>())).Returns(Task.CompletedTask);

            //Act
            await _handler.Handle(command, CancellationToken.None);
            //Assert
            _mockHotelRepository.Verify(x => x.GetByIdAsync(command.Id), Times.Once);
            _mockHotelRepository.Verify(x => x.UpdateAsync(It.Is<Domain.Entities.Hotel>(h =>
                h.Name == command.Name &&
                h.Description == command.Description &&
                h.StarRating == command.StarRating &&
                h.Status == command.Status &&
                h.Street == command.Street &&
                h.City == command.City &&
                h.State == command.State &&
                h.Country == command.Country &&
                h.ZipCode == command.ZipCode &&
                h.Email == command.Email &&
                h.Phone == command.Phone &&
                h.Website == command.Website
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldUpdateAllProperties()
        {
            var command = CreateValidCommand();
            Domain.Entities.Hotel hotel = new Domain.Entities.Hotel(command.Name, command.Description, command.StarRating, command.City, command.Country);
            Domain.Entities.Hotel? updatedHotel = null;
            _mockHotelRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(hotel);
            _mockHotelRepository.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Hotel>()))
                .Callback<Domain.Entities.Hotel>(h => updatedHotel = h)
                .Returns(Task.CompletedTask);

            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            updatedHotel.Should().NotBeNull();
            updatedHotel!.Name.Should().Be(command.Name);
            updatedHotel.Description.Should().Be(command.Description);
            updatedHotel.StarRating.Should().Be(command.StarRating);
            updatedHotel.Status.Should().Be(command.Status);
            updatedHotel.Street.Should().Be(command.Street);
            updatedHotel.City.Should().Be(command.City);
            updatedHotel.State.Should().Be(command.State);
            updatedHotel.Country.Should().Be(command.Country);
            updatedHotel.ZipCode.Should().Be(command.ZipCode);
            updatedHotel.Email.Should().Be(command.Email);
            updatedHotel.Phone.Should().Be(command.Phone);
            updatedHotel.Website.Should().Be(command.Website);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCallRepositoryMethodsInOrder()
        {
            var command = CreateValidCommand();
            Domain.Entities.Hotel hotel = new Domain.Entities.Hotel(command.Name, command.Description, command.StarRating, command.City, command.Country);
            _mockHotelRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(hotel);
            _mockHotelRepository.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Hotel>())).Returns(Task.CompletedTask);

            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            var inOrder = new Moq.MockSequence();
            _mockHotelRepository.InSequence(inOrder).Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(hotel);
            _mockHotelRepository.InSequence(inOrder).Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Hotel>())).Returns(Task.CompletedTask);


            _mockHotelRepository.Verify(x => x.GetByIdAsync(command.Id), Times.Once);
            _mockHotelRepository.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Hotel>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentHotel_ShouldThrowNotFoundException()
        {
            //Arrange
            var command = CreateValidCommand();
            _mockHotelRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Domain.Entities.Hotel?)null);

            //Act & Assert
            await Assert.ThrowsAsync<BookingSystem.Service.Hotel.Domain.Exceptions.HotelNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
