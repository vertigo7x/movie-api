using System;

namespace CinemaReservations.Application.Models
{
    public class Movie
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string Stars { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}