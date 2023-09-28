using ApiApplication.Database.Repositories.Abstractions;
using AutoMapper;
using CinemaReservations.Application.Exceptions;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;
using CinemaReservations.Services.Cache.Interfaces;
using CinemaReservations.Services.ExternalCinema.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using ProtoDefinitions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CinemaReservations.Application.Commands.Showtime
{
    public class CreateShowtimeCommandHandler : IRequestHandler<CreateShowtimeCommand, CreateShowtimeResponse>
    {

        private readonly IShowtimesRepository _showtimesRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;
        private readonly IExternalCinemaGrpcService _externalCinemaGrpcService;
        private readonly IRedisService _redisService;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateShowtimeCommandHandler> _logger;

        public CreateShowtimeCommandHandler(
            IShowtimesRepository showtimesRepository,
            IAuditoriumsRepository auditoriumsRepository,
            IExternalCinemaGrpcService externalCinemaGrpcService,
            IRedisService redisService,
            IMapper mapper,
            ILogger<CreateShowtimeCommandHandler> logger)
        {
            _showtimesRepository = showtimesRepository;
            _auditoriumsRepository = auditoriumsRepository;
            _externalCinemaGrpcService = externalCinemaGrpcService;
            _redisService = redisService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CreateShowtimeResponse> Handle(CreateShowtimeCommand request, CancellationToken cancellationToken)
        {
            Movie movie;
            movie = await GetMovieFromCache(request.MovieId);
            if (movie == null)
            {
                _logger.LogInformation($"Movie not found in Cache, Storing in cache");
                movie = await GetMovieFromExternalService(request.MovieId);
                if (movie == null)
                {
                    _logger.LogError($"Movie not found in External Service");
                    throw new MovieNotExistException($"Movie not found in External Service");
                } else
                {
                    await StoreMovieInCache(request.MovieId, movie);
                }
            }
            var auditorium = await GetAuditoriumAsync(request.AuditoriumId);
            if (auditorium == null)
            {
                _logger.LogError($"Auditorium with id {request.AuditoriumId} does not exist");
                throw new AuditoriumNotExistException($"Auditorium with id {request.AuditoriumId} does not exist");
            }
            var showtime = new Models.Showtime
            {
                Movie = movie,
                SessionDate = request.SessionDate,
                Auditorium = auditorium,
            };
            var showtimeEntity = _mapper.Map<Models.Showtime, ShowtimeEntity>(showtime);
            _logger.LogInformation($"Creating showtime");
            var createdShowtimeEntity = await _showtimesRepository.CreateShowtime(showtimeEntity, CancellationToken.None);
            var createdShowtime = _mapper.Map<ShowtimeEntity, Models.Showtime>(createdShowtimeEntity);
            return new CreateShowtimeResponse
            {
                Id = createdShowtime.Id,
                Movie = createdShowtime.Movie,
                SessionDate = createdShowtime.SessionDate,
                Auditorium = auditorium,
            };
        }

        private async Task StoreMovieInCache(string id, Movie movie)
        {
            await _redisService.SetMovie(id, JsonSerializer.Serialize(movie));
        }

        private async Task<Movie> GetMovieFromCache(string id)
        {
            var existingMovie = await _redisService.GetMovie(id);
            if (string.IsNullOrEmpty(existingMovie))
            {
                return null;
            }
            return JsonSerializer.Deserialize<Movie>(existingMovie);
        }

        private async Task<Movie> GetMovieFromExternalService(string id)
        {
            var show = await _externalCinemaGrpcService.GetMovie(id);
            if (show == null)
            {
                return null;
            }
            var movie = _mapper.Map<showResponse, Movie>(show);
            return movie;
        }

        private async Task<Auditorium> GetAuditoriumAsync(int auditoriumId)
        {
            var existingAuditorium = await _auditoriumsRepository.GetAsync(auditoriumId, CancellationToken.None);
            if (existingAuditorium != null)
            {
                return _mapper.Map<AuditoriumEntity, Auditorium>(existingAuditorium);
            }
            return null;
            
        }
    }
}
