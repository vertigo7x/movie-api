using System;
using System.Collections.Generic;

namespace CinemaReservations.Application.Models
{
    public class Ticket
    {
        public Ticket()
        {
            CreatedTime = DateTime.Now;
            Paid = false;
        }

        public Guid Id { get; set; }
        public int ShowtimeId { get; set; }
        public ICollection<Seat> Seats { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool Paid { get; set; }
    }
}