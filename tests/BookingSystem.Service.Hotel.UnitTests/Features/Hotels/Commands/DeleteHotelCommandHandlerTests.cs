using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Application.Common.Interfaces;
using BookingSystem.Service.Hotel.Application.Features.Hotels.Commands.DeleteHotel;
using FluentAssertions;
using Moq;

namespace BookingSystem.Service.Hotel.UnitTests.Features.Hotels.Commands
{
    public class DeleteHotelCommandHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly DeleteHotelCommandHandler _handler;

        public DeleteHotelCommandHandlerTests()
        {
            _hotelRepositoryMock = new Mock<IHotelRepository>();
            _handler = new DeleteHotelCommandHandler(_hotelRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldDeleteHotel()
        {
            //Arrange
            var command = new DeleteHotelCommand() { Id = 1 };
            _hotelRepositoryMock.Setup(x=>x.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            //Act
            var result = _handler.Handle(command,CancellationToken.None);
            //Assert
            result.Should().NotBeNull();
            _hotelRepositoryMock.Verify(x => x.DeleteAsync(command.Id), Times.Once);
        }
    }
}
