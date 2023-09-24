namespace CinemaReservations.Application.Exceptions
{
    public class SeatsAlreadyReservedException : CinemaReservationExceptionBase
    {
        public SeatsAlreadyReservedException(string message) : base(message)
        {
        }
    }
}
