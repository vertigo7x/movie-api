using Microsoft.VisualStudio.TestTools.UnitTesting;
using CinemaReservations.Application.Commands.Showtime;
using System;
using System.Collections.Generic;
using System.Text;
using ApiApplication.Database.Repositories.Abstractions;
using AutoMapper;
using CinemaReservations.Application.Commands.ReserveSeats;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Configuration;
using CinemaReservations.Application.Profiles;
using CinemaReservations.Services.Cache.Interfaces;
using CinemaReservations.Services.ExternalCinema.Interfaces;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;
using ProtoDefinitions;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using FluentAssertions;
using System.Text.Json;
using StackExchange.Redis;
using CinemaReservations.Application.Exceptions;

namespace CinemaReservations.Application.Commands.Showtime.Tests
{
    [TestClass()]
    public class CreateShowtimeCommandHandlerTests
    {
        private readonly Mock<IShowtimesRepository> _showtimesRepositoryMock;
        private readonly Mock<IAuditoriumsRepository> _auditoriumsRepositoryMock;
        private readonly Mock<IExternalCinemaGrpcService> _externalCinemaGrpcServiceMock;
        private readonly Mock<IRedisService> _redisServiceMock;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<CreateShowtimeCommandHandler>> _loggerMock;

        public CreateShowtimeCommandHandlerTests()
        {
            _showtimesRepositoryMock = new Mock<IShowtimesRepository>();
            _auditoriumsRepositoryMock = new Mock<IAuditoriumsRepository>();
            _externalCinemaGrpcServiceMock = new Mock<IExternalCinemaGrpcService>();
            _redisServiceMock = new Mock<IRedisService>();
            //Arrange
            _loggerMock = new Mock<ILogger<CreateShowtimeCommandHandler>>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ShowtimeProfile());
                cfg.AddProfile(new MovieProfile());
                cfg.AddProfile(new AuditoriumProfile());
                cfg.AddProfile(new ShowProfile());
            });
            IMapper mapper = new Mapper(mapperConfig);
            _mapper = mapper;
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handler = new CreateShowtimeCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _externalCinemaGrpcServiceMock.Object,
                _redisServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            var request = new CreateShowtimeCommand
            {
                MovieId = "movieId",
                AuditoriumId = 1,
                SessionDate = DateTime.Now,
            };

            var expectedMovie = new Movie { Id = 1, ImdbId = "movieId" };
            var expectedAuditoriumEntity = new AuditoriumEntity { Id = 1, Rows = 1, SeatsPerRow = 1 };
            var expectedAuditorium = _mapper.Map<AuditoriumEntity, Auditorium>(expectedAuditoriumEntity);
            var expectedShowtimeEntity = new ShowtimeEntity 
            { 
                Id = 1, 
                Movie = new MovieEntity() { Id = 1, ImdbId = "movieId"}, 
                AuditoriumId = 1,
                SessionDate = request.SessionDate
            };

            _externalCinemaGrpcServiceMock.Setup(service => service.GetMovie(request.MovieId))
                .ReturnsAsync(new showResponse()); 

            _redisServiceMock.Setup(redis => redis.GetMovie(request.MovieId))
                .ReturnsAsync(JsonSerializer.Serialize(expectedMovie));

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(expectedAuditoriumEntity);

            _showtimesRepositoryMock.Setup(repo => repo.CreateShowtime(It.IsAny<ShowtimeEntity>(), CancellationToken.None))
                .ReturnsAsync(expectedShowtimeEntity);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(1);
            response.Movie.Should().BeEquivalentTo(expectedMovie);
            response.SessionDate.Should().Be(request.SessionDate);
            response.Auditorium.Should().BeEquivalentTo(expectedAuditorium);

            _showtimesRepositoryMock.Verify(x => x.CreateShowtime(It.IsAny<ShowtimeEntity>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_MovieNotInRedis_ReturnsResponse()
        {
            // Arrange
            var handler = new CreateShowtimeCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _externalCinemaGrpcServiceMock.Object,
                _redisServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            var request = new CreateShowtimeCommand
            {
                MovieId = "movieId",
                AuditoriumId = 1,
                SessionDate = DateTime.Now,
            };

            var expectedMovie = new Movie { Id = 1, ImdbId = "movieId" };
            var expectedAuditoriumEntity = new AuditoriumEntity { Id = 1, Rows = 1, SeatsPerRow = 1 };
            var expectedAuditorium = _mapper.Map<AuditoriumEntity, Auditorium>(expectedAuditoriumEntity);
            var expectedShowtimeEntity = new ShowtimeEntity
            {
                Id = 1,
                Movie = new MovieEntity() { Id = 1, ImdbId = "movieId" },
                AuditoriumId = 1,
                SessionDate = request.SessionDate
            };

            _externalCinemaGrpcServiceMock.Setup(service => service.GetMovie(request.MovieId))
                .ReturnsAsync(new showResponse
                {
                    Crew = "Test",
                    Id = "movieId",
                    Title = "Test",
                    Year = "2020",
                    Rank = "1"
                });

            _redisServiceMock.Setup(redis => redis.GetMovie(request.MovieId))
                .ReturnsAsync("");

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(expectedAuditoriumEntity);

            _showtimesRepositoryMock.Setup(repo => repo.CreateShowtime(It.IsAny<ShowtimeEntity>(), CancellationToken.None))
                .ReturnsAsync(expectedShowtimeEntity);

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(1);
            response.Movie.Should().BeEquivalentTo(expectedMovie);
            response.SessionDate.Should().Be(request.SessionDate);
            response.Auditorium.Should().BeEquivalentTo(expectedAuditorium);

            _redisServiceMock.Verify(x => x.SetMovie(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _externalCinemaGrpcServiceMock.Verify(x => x.GetMovie(It.IsAny<string>()), Times.Once);
            _showtimesRepositoryMock.Verify(x => x.CreateShowtime(It.IsAny<ShowtimeEntity>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_MovieNotExist_ThrowsException()
        {
            // Arrange
            var handler = new CreateShowtimeCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _externalCinemaGrpcServiceMock.Object,
                _redisServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            var request = new CreateShowtimeCommand
            {
                MovieId = "movieId",
                AuditoriumId = 1,
                SessionDate = DateTime.Now,
            };

            showResponse? nullShowResponse = null;
            _externalCinemaGrpcServiceMock.Setup(service => service.GetMovie(request.MovieId))
                .ReturnsAsync(nullShowResponse);

            _redisServiceMock.Setup(redis => redis.GetMovie(request.MovieId))
                .ReturnsAsync("");

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<MovieNotExistException>().WithMessage($"Movie not found in External Service");
            _externalCinemaGrpcServiceMock.Verify(x => x.GetMovie(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AuditoriumNotExist_ThrowsException()
        {
            // Arrange
            var handler = new CreateShowtimeCommandHandler(
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _externalCinemaGrpcServiceMock.Object,
                _redisServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            var request = new CreateShowtimeCommand
            {
                MovieId = "movieId",
                AuditoriumId = 1,
                SessionDate = DateTime.Now,
            };

            var expectedMovie = new Movie { Id = 1, ImdbId = "movieId" };
            AuditoriumEntity? expectedAuditoriumEntity = null;

            _externalCinemaGrpcServiceMock.Setup(service => service.GetMovie(request.MovieId))
                .ReturnsAsync(new showResponse());

            _redisServiceMock.Setup(redis => redis.GetMovie(request.MovieId))
                .ReturnsAsync(JsonSerializer.Serialize(expectedMovie));

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(request.AuditoriumId, CancellationToken.None))
                .ReturnsAsync(expectedAuditoriumEntity);


            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AuditoriumNotExistException>().WithMessage($"Auditorium with id {request.AuditoriumId} does not exist");
            _auditoriumsRepositoryMock.Verify(x => x.GetAsync(It.IsAny<int>(), CancellationToken.None), Times.Once);

        }
    }
}