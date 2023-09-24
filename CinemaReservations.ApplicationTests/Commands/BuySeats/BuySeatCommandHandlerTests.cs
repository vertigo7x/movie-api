using ApiApplication.Database.Repositories.Abstractions;
using AutoMapper;
using CinemaReservations.Application.Commands.BuySeats;
using CinemaReservations.Application.Exceptions;
using CinemaReservations.Application.Profiles;
using CinemaReservations.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CinemaReservations.ApplicationTests.Commands.BuySeats
{
    public class BuySeatCommandHandlerTests
    {
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<BuySeatCommandHandler>> _loggerMock;

        public BuySeatCommandHandlerTests()
        {
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            //Arrange
            var inMemorySettings = new Dictionary<string, string> {
                {"ReservationTTLInMinutes", "10"},
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _loggerMock = new Mock<ILogger<BuySeatCommandHandler>>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MovieProfile());
                cfg.AddProfile(new ShowtimeProfile());
                cfg.AddProfile(new TicketProfile());
                cfg.AddProfile(new SeatProfile());
            });
            IMapper mapper = new Mapper(mapperConfig);
            _mapper = mapper;
        }

        [Fact]
        public async Task Handle_ValidTicket_ReturnsResponse()
        {
            // Arrange
            var handler = new BuySeatCommandHandler(
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new BuySeatCommand
            {
                TicketId = Guid.NewGuid()
            };

            var existingTicket = new TicketEntity
            {
                CreatedTime = DateTime.Now,
                Id = request.TicketId,
                Paid = false,
                ShowtimeId = 1,
                Showtime = new ShowtimeEntity
                {
                    AuditoriumId = 1,
                    Id = 1,
                    Movie = new MovieEntity
                    {
                        Id = 1,
                        Title = "Test",
                        ImdbId = "Test",
                        Stars = "Test",
                        ReleaseDate = DateTime.UtcNow
                    },
                },
                Seats = new List<SeatEntity>()
               {
                   new SeatEntity
                   {
                       Row = 1,
                       SeatNumber = 1,
                       TicketId = request.TicketId,
                       AuditoriumId = 1
                   }
               }
            };

            _ticketsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);
            _ticketsRepositoryMock.Setup(repo => repo.ConfirmPaymentAsync(It.IsAny<TicketEntity>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Ticket.Id.Should().Be(existingTicket.Id);
        }

        [Fact]
        public async Task Handle_ExpiredTicket_ThrowsException()
        {
            // Arrange
            var handler = new BuySeatCommandHandler(
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new BuySeatCommand
            {
                TicketId = Guid.NewGuid()
            };

            var existingTicket = new TicketEntity
            {
                CreatedTime = DateTime.UtcNow,
                Id = request.TicketId,
                Paid = false,
                ShowtimeId = 1,
                Showtime = new ShowtimeEntity
                {
                    AuditoriumId = 1,
                    Id = 1,
                    Movie = new MovieEntity
                    {
                        Id = 1,
                        Title = "Test",
                        ImdbId = "Test",
                        Stars = "Test",
                        ReleaseDate = DateTime.UtcNow
                    },
                },
                Seats = new List<SeatEntity>()
                   {
                       new SeatEntity
                       {
                           Row = 1,
                           SeatNumber = 1,
                           TicketId = request.TicketId,
                           AuditoriumId = 1
                       }
                   }
            };

            _ticketsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);
            _ticketsRepositoryMock.Setup(repo => repo.ConfirmPaymentAsync(It.IsAny<TicketEntity>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TicketExpiredException>().WithMessage($"Ticket with id {request.TicketId} is expired");
        }

        [Fact]
        public async Task Handle_NotExistTicket_ThrowsException()
        {
            // Arrange
            var handler = new BuySeatCommandHandler(
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new BuySeatCommand
            {
                TicketId = Guid.NewGuid()
            };

            TicketEntity existingTicket = null;

            _ticketsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);
            _ticketsRepositoryMock.Setup(repo => repo.ConfirmPaymentAsync(It.IsAny<TicketEntity>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TicketNotExistException>().WithMessage($"Ticket with id {request.TicketId} does not exist");
        }

        [Fact]
        public async Task Handle_PayedTicket_ThrowsException()
        {
            // Arrange
            var handler = new BuySeatCommandHandler(
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new BuySeatCommand
            {
                TicketId = Guid.NewGuid()
            };

            var existingTicket = new TicketEntity
            {
                CreatedTime = DateTime.UtcNow,
                Id = request.TicketId,
                Paid = true,
                ShowtimeId = 1,
                Showtime = new ShowtimeEntity
                {
                    AuditoriumId = 1,
                    Id = 1,
                    Movie = new MovieEntity
                    {
                        Id = 1,
                        Title = "Test",
                        ImdbId = "Test",
                        Stars = "Test",
                        ReleaseDate = DateTime.UtcNow
                    },
                },
                Seats = new List<SeatEntity>()
                   {
                       new SeatEntity
                       {
                           Row = 1,
                           SeatNumber = 1,
                           TicketId = request.TicketId,
                           AuditoriumId = 1
                       }
                   }
            };

            _ticketsRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);
            _ticketsRepositoryMock.Setup(repo => repo.ConfirmPaymentAsync(It.IsAny<TicketEntity>(), CancellationToken.None))
                .ReturnsAsync(existingTicket);

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TicketAlreadyBoughtException>().WithMessage($"Ticket with id {request.TicketId} is already bought");
        }
    }
}
