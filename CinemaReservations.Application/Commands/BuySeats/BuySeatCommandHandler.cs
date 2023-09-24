using ApiApplication.Database.Repositories.Abstractions;
using AutoMapper;
using CinemaReservations.Application.Exceptions;
using CinemaReservations.Application.Models;
using CinemaReservations.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CinemaReservations.Application.Commands.BuySeats
{
    public class BuySeatCommandHandler : IRequestHandler<BuySeatCommand, BuySeatResponse>
    {
        private readonly static int EXPIRATION_TIME = 10;

        private readonly ITicketsRepository _ticketsRepository;
        private readonly int _expirationTime;
        private readonly IMapper _mapper;
        private readonly ILogger<BuySeatCommandHandler> _logger;

        public BuySeatCommandHandler(
            ITicketsRepository ticketsRepository,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<BuySeatCommandHandler> logger)
        {
            _ticketsRepository = ticketsRepository;
            _expirationTime = configuration.GetValue<int?>("ReservationTTLInMinutes") != null ? configuration.GetValue<int>("ReservationTTLInMinutes") : EXPIRATION_TIME;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BuySeatResponse> Handle(BuySeatCommand request, CancellationToken cancellationToken)
        {
            // Check if ticket exists
            var existingTicket = await _ticketsRepository.GetAsync(request.TicketId, cancellationToken);
            if (existingTicket == null)
            {
                _logger.LogError($"Ticket with id {request.TicketId} does not exist");
                throw new TicketNotExistException($"Ticket with id {request.TicketId} does not exist");
            }
            // Check if ticket is already bought
            if (existingTicket.Paid)
            {
                _logger.LogError($"Ticket with id {request.TicketId} is already bought");
                throw new TicketAlreadyBoughtException($"Ticket with id {request.TicketId} is already bought");
            }
            // Check if ticket is expired
            var creationDate = existingTicket.CreatedTime;
            var now = DateTime.Now;
            var diff = now - creationDate;
            if (diff.TotalMinutes > _expirationTime)
            {
                _logger.LogError($"Ticket with id {request.TicketId} is expired");
                throw new TicketExpiredException($"Ticket with id {request.TicketId} is expired");
            }
            // Pay for ticket
            var updatedTicket = await _ticketsRepository.ConfirmPaymentAsync(existingTicket, cancellationToken);
            var response = new BuySeatResponse
            {
                AuditoriumId = updatedTicket.Showtime.AuditoriumId,
                ShowtimeId = updatedTicket.ShowtimeId,
                Movie = _mapper.Map<MovieEntity, Movie>(updatedTicket.Showtime.Movie),
                Ticket = _mapper.Map<TicketEntity, Ticket>(updatedTicket)
            };
            return response;
        }
    }
}
