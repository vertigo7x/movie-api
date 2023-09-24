using MediatR;
using System;

namespace CinemaReservations.Application.Commands.Showtime
{
    public class CreateShowtimeCommand : IRequest<CreateShowtimeResponse>
    {
        public string MovieId { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
