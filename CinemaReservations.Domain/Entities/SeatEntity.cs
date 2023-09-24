using System;

namespace CinemaReservations.Domain.Entities
{
    public class SeatEntity
    {
        public short Row { get; set; }
        public short SeatNumber { get; set; }
        public int AuditoriumId { get; set; }
        public Guid TicketId { get; set; }
        public TicketEntity Ticket { get; set; }
        public AuditoriumEntity Auditorium { get; set; }
    }
}
