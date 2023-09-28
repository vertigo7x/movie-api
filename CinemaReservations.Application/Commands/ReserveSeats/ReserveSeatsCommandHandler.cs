using ApiApplication.Database.Repositories.Abstractions;
using AutoMapper;
using CinemaReservations.Application.Exceptions;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CinemaReservations.Application.Commands.ReserveSeats
{
    public class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatsCommand, ReserveSeatsResponse>
    {
        private readonly static int EXPIRATION_TIME = 10;

        private readonly IShowtimesRepository _showtimesRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly int _expirationTime;
        private readonly IMapper _mapper;
        private readonly ILogger<ReserveSeatsCommandHandler> _logger;

        public ReserveSeatsCommandHandler(
            IShowtimesRepository showtimesRepository,
            IAuditoriumsRepository auditoriumsRepository,
            ITicketsRepository ticketsRepository,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<ReserveSeatsCommandHandler> logger)
        {
            _showtimesRepository = showtimesRepository;
            _auditoriumsRepository = auditoriumsRepository;
            _ticketsRepository = ticketsRepository;
            _expirationTime = configuration.GetValue<int?>("ReservationTTLInMinutes") != null ? configuration.GetValue<int>("ReservationTTLInMinutes") : EXPIRATION_TIME;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReserveSeatsResponse> Handle(ReserveSeatsCommand request, CancellationToken cancellationToken)
        {
            // Check if auditorium exists
            var existAuditorium = await _auditoriumsRepository.GetAsync(request.AuditoriumId, cancellationToken);
            if (existAuditorium == null)
            {
                _logger.LogError($"Auditorium with id {request.AuditoriumId} does not exist");
                throw new AuditoriumNotExistException($"Auditorium with id {request.AuditoriumId} does not exist");
            }
            // Check if seats are in auditorium bounds
            if (CheckIfSeatsAreInBounds(request.Seats, existAuditorium))
            {
                _logger.LogError($"Seats are not in auditorium bounds");
                throw new SeatsOutOfBoundsException($"Seats are not in auditorium bounds");
            }
            // Check if seats are consecutive
            var seats = request.Seats.Select(x => x.SeatNumber).ToArray<short>();
            if (!CheckIfSeatsAreConsecutive(seats))
            {
                _logger.LogError($"Seats are not consecutive");
                throw new SeatsNotConsecutiveException($"Seats are not consecutive");
            }
            // Check if showtime exists
            var existShowtime = await _showtimesRepository.GetWithMoviesAndTicketsByIdAsync(request.ShowtimeId, cancellationToken);
            if (existShowtime == null)
            {
                _logger.LogError($"Showtime with id {request.ShowtimeId} does not exist");
                throw new ShowtimeNotExistException($"Showtime with id {request.ShowtimeId} does not exist");
            }
            // Search for seats in tickets
            var alreadyReservedTicket = CheckForAlreadyReservedSeats(existShowtime.Tickets, request.Seats);
            if (alreadyReservedTicket.Any())
            {
                _logger.LogError($"Seats are already reserved: {alreadyReservedTicket}");
                throw new SeatsAlreadyReservedException($"Seats are already reserved: {alreadyReservedTicket}");
            }
            // Reserve seats
            var ticketToCreate = CreateReservedTicket(existShowtime, existAuditorium, request.Seats);
            var createdTicket = await _ticketsRepository.CreateAsync(ticketToCreate, CancellationToken.None);

            var movie = _mapper.Map<MovieEntity, Movie>(existShowtime.Movie);
            var ticket = _mapper.Map<TicketEntity, Ticket>(createdTicket);

            return new ReserveSeatsResponse
            {
                Id = createdTicket.Id,
                ShowtimeId = existShowtime.Id,
                AuditoriumId = existAuditorium.Id,
                Movie = movie,
                Ticket = ticket,
            };


        }

        private TicketEntity CreateReservedTicket(ShowtimeEntity existShowtime, AuditoriumEntity existAuditorium, List<Seat> seats)
        {
            var ticketToCreate = new TicketEntity
            {
                Id = Guid.NewGuid(),
                ShowtimeId = existShowtime.Id,
                Paid = false,
            };
            var auditoriumSeats = new List<SeatEntity>();
            foreach (var seat in seats)
            {
                auditoriumSeats.Add(new SeatEntity
                {
                    AuditoriumId = existAuditorium.Id,
                    TicketId = ticketToCreate.Id,
                    Row = seat.Row,
                    SeatNumber = seat.SeatNumber,
                });
            }
            ticketToCreate.Seats = auditoriumSeats;
            return ticketToCreate;
        }

        private List<TicketEntity> CheckForAlreadyReservedSeats(ICollection<TicketEntity> tickets, List<Seat> seats)
        {
            var alreadyReservedTicket = new List<TicketEntity>();

            // Filter tickets that are not expired
            var validTickets = tickets
                .Where(ticket => !ticket.Paid && (DateTime.Now - ticket.CreatedTime).TotalMinutes < _expirationTime)
                .ToList();

            foreach (var seat in seats)
            {
                // Check if the seat is reserved in any of the valid tickets
                if (validTickets.Any(ticket => ticket.Seats.Any(x => x.Row == seat.Row && x.SeatNumber == seat.SeatNumber)))
                {
                    alreadyReservedTicket.AddRange(validTickets);
                }
            }

            return alreadyReservedTicket.Distinct().ToList();
        }

        private bool CheckIfSeatsAreInBounds(List<Seat> seats, AuditoriumEntity existAuditorium)
        {
            foreach (var seat in seats)
            {
                if (seat.Row > existAuditorium.Rows || seat.SeatNumber > existAuditorium.SeatsPerRow)
                {
                    return true;
                }
            }
            return false;
        }


        private static bool CheckIfSeatsAreConsecutive(short[] numbers)
        {
            Array.Sort(numbers);
            for (int i = 1; i < numbers.Length; i++)
            {
                if (numbers[i] != numbers[i - 1] + 1)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
