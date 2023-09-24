using CinemaReservations.Application.Models;
using System;

namespace CinemaReservations.Application.Commands.ReserveSeats
{
    public class ReserveSeatsResponse : ApplicationResponse
    {
        public Guid Id { get; set; }
        public int ShowtimeId { get; set; }
        public int AuditoriumId { get; set; }
        public Movie Movie { get; set; }
        public Ticket Ticket { get; set; }
    }
}
