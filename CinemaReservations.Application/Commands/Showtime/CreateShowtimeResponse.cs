using CinemaReservations.Application.Models;
using System;

namespace CinemaReservations.Application.Commands.Showtime
{
    public class CreateShowtimeResponse : ApplicationResponse
    {
        public int Id { get; set; }
        public Movie Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public Auditorium Auditorium { get; set; }
    }
}
