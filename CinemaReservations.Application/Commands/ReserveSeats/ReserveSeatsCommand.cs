using CinemaReservations.Application.Models;
using MediatR;
using System.Collections.Generic;

namespace CinemaReservations.Application.Commands.ReserveSeats
{
    public class ReserveSeatsCommand : IRequest<ReserveSeatsResponse>
    {
        public int ShowtimeId { get; set; }
        public int AuditoriumId { get; set; }
        public List<Seat> Seats { get; set; }
    }
}
