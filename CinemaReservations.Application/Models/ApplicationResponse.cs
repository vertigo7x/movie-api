using System;

namespace CinemaReservations.Application.Models
{
    public class ApplicationResponse
    {
        public DateTime EventTime { get; set; } = new DateTime().ToUniversalTime();
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
