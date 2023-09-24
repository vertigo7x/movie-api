using CinemaReservations.Application.Models;

namespace CinemaReservations.Application.Commands.BuySeats
{
    public class BuySeatResponse : ApplicationResponse
    {
        public int Id { get; set; }
        public int ShowtimeId { get; set; }
        public int AuditoriumId { get; set; }
        public Movie Movie { get; set; }
        public Ticket Ticket { get; set; }
    }
}