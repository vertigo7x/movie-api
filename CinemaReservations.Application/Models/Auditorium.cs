using System.Collections.Generic;

namespace CinemaReservations.Application.Models
{
    public class Auditorium
    {
        public int Id { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
    }
}