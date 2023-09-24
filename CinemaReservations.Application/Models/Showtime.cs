using System;
using System.Collections.Generic;

namespace CinemaReservations.Application.Models
{
    internal class Showtime
    {
        public int Id { get; set; }
        public Movie Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public Auditorium Auditorium { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }
}
