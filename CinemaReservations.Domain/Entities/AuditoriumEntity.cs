using System.Collections.Generic;

namespace CinemaReservations.Domain.Entities
{
    public class AuditoriumEntity
    {
        public int Id { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public ICollection<ShowtimeEntity> Showtimes { get; set; }


    }
}
