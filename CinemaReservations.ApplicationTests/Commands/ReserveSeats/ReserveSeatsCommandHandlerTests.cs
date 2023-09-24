using System;
using System.Collections.Generic;
using ApiApplication.Database.Repositories.Abstractions;
using AutoMapper;
using CinemaReservations.Application.Commands.BuySeats;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Configuration;
using CinemaReservations.Application.Profiles;
using Xunit;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using CinemaReservations.Application.Exceptions;

namespace CinemaReservations.Application.Commands.ReserveSeats.Tests
{
    public class ReserveSeatsCommandHandlerTests
    {

        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<IShowtimesRepository> _showtimesRepositoryMock;
        private readonly Mock<IAuditoriumsRepository> _auditoriumsRepositoryMock;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<ReserveSeatsCommandHandler>> _loggerMock;

        public ReserveSeatsCommandHandlerTests()
        {
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _showtimesRepositoryMock = new Mock<IShowtimesRepository>();
            _auditoriumsRepositoryMock = new Mock<IAuditoriumsRepository>();
            //Arrange
            var inMemorySettings = new Dictionary<string, string> {
                {"ReservationTTLInMinutes", "10"},
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _loggerMock = new Mock<ILogger<ReserveSeatsCommandHandler>>();
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


        [Fact()]
        public async Task Handle_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handler = new ReserveSeatsCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new ReserveSeatsCommand
            {
                AuditoriumId = 1,
                ShowtimeId = 2,
                Seats = new List<Seat>
                {
                    new Seat { Row = 1, SeatNumber = 1 },
                    new Seat { Row = 1, SeatNumber = 2 },
                }
            };

            var existingAuditorium = new AuditoriumEntity { Id = 1, Rows = 5, SeatsPerRow = 10 };
            var existingMovie = new MovieEntity { Id = 1, Title = "Test", ImdbId = "Test" };
            var existingShowtime = new ShowtimeEntity { 
                Id = 2, 
                AuditoriumId = 1,
                Movie = existingMovie,
                Tickets = new List<TicketEntity>() 
            };
            var createdTicket = new TicketEntity { Id = Guid.NewGuid() };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(existingAuditorium);

            _showtimesRepositoryMock.Setup(repo => repo.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None))
                .ReturnsAsync(existingShowtime);

            _ticketsRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<TicketEntity>(), CancellationToken.None))
                .ReturnsAsync(createdTicket);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(request.AuditoriumId, CancellationToken.None), Times.Once());
            _showtimesRepositoryMock.Verify(x => x.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None), Times.Once());
            _ticketsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TicketEntity>(), CancellationToken.None), Times.Once());
            
            response.Should().NotBeNull();
            response.Movie.Should().NotBeNull();
            response.Movie.Should().NotBeNull();
            response.Movie.Should().NotBeNull();
            response.Ticket.Should().NotBeNull();
            response.Ticket.Should().NotBeNull();
            response.Ticket.Should().NotBeNull();
            response.Ticket.Id.Should().Be(createdTicket.Id);
        }

        [Fact()]
        public async Task Handle_AuditoriumNotExist_ThrowsException()
        {
            // Arrange
            var handler = new ReserveSeatsCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new ReserveSeatsCommand
            {
                AuditoriumId = 999,
                ShowtimeId = 2,
                Seats = new List<Seat>
                {
                    new Seat { Row = 1, SeatNumber = 1 },
                    new Seat { Row = 1, SeatNumber = 2 },
                }
            };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ThrowsAsync(new AuditoriumNotExistException($"Auditorium with id {request.AuditoriumId} does not exist"));

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AuditoriumNotExistException>().WithMessage($"Auditorium with id {request.AuditoriumId} does not exist");
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(request.AuditoriumId, CancellationToken.None), Times.Once());
        }

        [Fact()]
        public async Task Handle_SeatsOutOfBounds_ThrowsException()
        {
            // Arrange
            var handler = new ReserveSeatsCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new ReserveSeatsCommand
            {
                AuditoriumId = 1,
                ShowtimeId = 2,
                Seats = new List<Seat>
                {
                    new Seat { Row = 6, SeatNumber = 11 },
                    new Seat { Row = 6, SeatNumber = 12 },
                }
            };

            var existingAuditorium = new AuditoriumEntity { Id = 1, Rows = 5, SeatsPerRow = 10 };
            var existingMovie = new MovieEntity { Id = 1, Title = "Test", ImdbId = "Test" };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(existingAuditorium);

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<SeatsOutOfBoundsException>().WithMessage($"Seats are not in auditorium bounds");
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(request.AuditoriumId, CancellationToken.None), Times.Once());
        }

        [Fact()]
        public async Task Handle_SeatsNotConsecutive_ThrowsException()
        {
            // Arrange
            var handler = new ReserveSeatsCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new ReserveSeatsCommand
            {
                AuditoriumId = 1,
                ShowtimeId = 2,
                Seats = new List<Seat>
                {
                    new Seat { Row = 1, SeatNumber = 5 },
                    new Seat { Row = 1, SeatNumber = 9 },
                }
            };

            var existingAuditorium = new AuditoriumEntity { Id = 1, Rows = 5, SeatsPerRow = 10 };
            var existingMovie = new MovieEntity { Id = 1, Title = "Test", ImdbId = "Test" };
            var existingShowtime = new ShowtimeEntity
            {
                Id = 2,
                AuditoriumId = 1,
                Movie = existingMovie,
                Tickets = new List<TicketEntity>()
            };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(existingAuditorium);
            _showtimesRepositoryMock.Setup(repo => repo.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None))
               .ReturnsAsync(existingShowtime);

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<SeatsNotConsecutiveException>().WithMessage($"Seats are not consecutive");
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(request.AuditoriumId, CancellationToken.None), Times.Once());
        }

        [Fact()]
        public async Task Handle_ShowtimeNotEXists_ThrowsException()
        {
            // Arrange
            var handler = new ReserveSeatsCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new ReserveSeatsCommand
            {
                AuditoriumId = 1,
                ShowtimeId = 999,
                Seats = new List<Seat>
                {
                    new Seat { Row = 1, SeatNumber = 1 },
                    new Seat { Row = 1, SeatNumber = 2 },
                }
            };

            var existingAuditorium = new AuditoriumEntity { Id = 1, Rows = 5, SeatsPerRow = 10 };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(existingAuditorium);
            _showtimesRepositoryMock.Setup(repo => repo.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None))
               .ThrowsAsync(new ShowtimeNotExistException($"Showtime with id {request.ShowtimeId} does not exist"));

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ShowtimeNotExistException>().WithMessage($"Showtime with id {request.ShowtimeId} does not exist");
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(request.AuditoriumId, CancellationToken.None), Times.Once());
        }

        [Fact()]
        public async Task Handle_SeatsAlreadyReserved_ValidReservation_ThrowsException()
        {
            // Arrange
            var handler = new ReserveSeatsCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new ReserveSeatsCommand
            {
                AuditoriumId = 1,
                ShowtimeId = 2,
                Seats = new List<Seat>
                {
                    new Seat { Row = 1, SeatNumber = 1 },
                    new Seat { Row = 1, SeatNumber = 2 },
                }
            };

            var existingAuditorium = new AuditoriumEntity { Id = 1, Rows = 5, SeatsPerRow = 10 };
            var existingMovie = new MovieEntity { Id = 1, Title = "Test", ImdbId = "Test" };
            var existingShowtime = new ShowtimeEntity
            {
                Id = 2,
                AuditoriumId = 1,
                Movie = existingMovie,
                Tickets = new List<TicketEntity>()
                {
                    new TicketEntity
                    {
                        Id = Guid.Empty,
                        ShowtimeId = 2,
                        CreatedTime = DateTime.Now,
                        Seats = new List<SeatEntity>()
                        {
                            new SeatEntity
                            {
                                Row = 1,
                                SeatNumber = 1,
                                TicketId = Guid.Empty,
                                AuditoriumId = 1
                            },
                            new SeatEntity
                            {
                                Row = 1,
                                SeatNumber = 2,
                                TicketId = Guid.Empty,
                                AuditoriumId = 1
                            }
                        }
                    }   
                }
            };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(existingAuditorium);
            _showtimesRepositoryMock.Setup(repo => repo.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None))
               .ReturnsAsync(existingShowtime);

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<SeatsAlreadyReservedException>().WithMessage($"Seats are already reserved: {existingShowtime.Tickets}");
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(request.AuditoriumId, CancellationToken.None), Times.Once());
            _showtimesRepositoryMock.Verify(x => x.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None), Times.Once());
        }

        [Fact()]
        public async Task Handle_OverwriteReservation_ReturnResult()
        {
            // Arrange
            var handler = new ReserveSeatsCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _ticketsRepositoryMock.Object,
                _configuration,
                _mapper,
                _loggerMock.Object);

            var request = new ReserveSeatsCommand
            {
                AuditoriumId = 1,
                ShowtimeId = 2,
                Seats = new List<Seat>
                {
                    new Seat { Row = 1, SeatNumber = 1 },
                    new Seat { Row = 1, SeatNumber = 2 },
                }
            };

            var existingAuditorium = new AuditoriumEntity { Id = 1, Rows = 5, SeatsPerRow = 10 };
            var existingMovie = new MovieEntity { Id = 1, Title = "Test", ImdbId = "Test" };
            var existingShowtime = new ShowtimeEntity
            {
                Id = 2,
                AuditoriumId = 1,
                Movie = existingMovie,
                Tickets = new List<TicketEntity>()
                {
                    new TicketEntity
                    {
                        Id = Guid.Empty,
                        ShowtimeId = 2,
                        CreatedTime = DateTime.Now.AddMinutes(-10),
                        Seats = new List<SeatEntity>()
                        {
                            new SeatEntity
                            {
                                Row = 1,
                                SeatNumber = 1,
                                TicketId = Guid.Empty,
                                AuditoriumId = 1
                            },
                            new SeatEntity
                            {
                                Row = 1,
                                SeatNumber = 2,
                                TicketId = Guid.Empty,
                                AuditoriumId = 1
                            }
                        }
                    }
                }
            };
            var createdTicket = new TicketEntity { Id = Guid.NewGuid() };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(existingAuditorium);

            _showtimesRepositoryMock.Setup(repo => repo.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None))
                .ReturnsAsync(existingShowtime);

            _ticketsRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<TicketEntity>(), CancellationToken.None))
                .ReturnsAsync(createdTicket);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(request.AuditoriumId, CancellationToken.None), Times.Once());
            _showtimesRepositoryMock.Verify(x => x.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, CancellationToken.None), Times.Once());
            _ticketsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TicketEntity>(), CancellationToken.None), Times.Once());

            response.Should().NotBeNull();
            response.Movie.Should().NotBeNull();
            response.Movie.Should().NotBeNull();
            response.Movie.Should().NotBeNull();
            response.Ticket.Should().NotBeNull();
            response.Ticket.Should().NotBeNull();
            response.Ticket.Should().NotBeNull();
            response.Ticket.Id.Should().Be(createdTicket.Id);
        }

    }
}